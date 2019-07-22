using System;
using System.IO;
using System.Collections.Generic;

namespace MIDI {
    public class SMF {
        public enum FORMAT : ushort {
            FORMAT0 = 0x0000,
            FORMAT1 = 0x0001,
            FORMAT2 = 0x0002,
            INVALID = 0xFFFF
        }

        #region 構造体
        private struct Head {
            public readonly FORMAT Format;
            public UInt16 Tracks;
            public readonly UInt16 Ticks;

            public Head(FORMAT format, UInt16 tracks, UInt16 ticks) {
                Format = format;
                Tracks = tracks;
                Ticks = ticks;
            }

            public Head(BinaryReader br) {
                ReadUInt32(br);
                ReadUInt32(br);
                Format = (FORMAT)ReadUInt16(br);
                Tracks = ReadUInt16(br);
                Ticks = ReadUInt16(br);

                if (!Enum.IsDefined(typeof(FORMAT), Format)) {
                    Format = FORMAT.INVALID;
                }
            }

            public void Write(MemoryStream ms) {
                WriteUInt32(ms, 0x4D546864);
                WriteUInt32(ms, 6);
                WriteUInt16(ms, (UInt16)Format);
                WriteUInt16(ms, Tracks);
                WriteUInt16(ms, Ticks);
            }
        }

        private class Track {
            public readonly UInt16 No;
            public HashSet<Event> Events;

            public Track(int no) {
                No = (UInt16)no;
                Events = new HashSet<Event>();
            }

            public Track(BinaryReader br, int no) {
                No = (UInt16)no;
                Events = new HashSet<Event>();

                ReadUInt32(br);
                int size = (int)ReadUInt32(br);

                MemoryStream stream = new MemoryStream(br.ReadBytes(size), false);
                Time time = new Time(0, 0);
                int currentStatus = 0;

                while (stream.Position < stream.Length) {
                    time.Step(ReadDelta(stream));
                    Events.Add(new Event(time, No, ReadMessage(stream, ref currentStatus)));
                }
            }

            public void Write(MemoryStream ms) {
                MemoryStream temp = new MemoryStream();
                WriteUInt32(temp, 0);
                WriteUInt32(temp, 0);

                UInt32 currentTime = 0;
                foreach (var ev in Events) {
                    WriteDelta(temp, ev.Time - currentTime);
                    WriteMessage(temp, ev.Message);
                    currentTime = ev.Time;
                }

                temp.Seek(0, SeekOrigin.Begin);
                WriteUInt32(temp, 0x4D54726B);
                WriteUInt32(temp, (UInt32)(temp.Length - 8));

                temp.WriteTo(ms);
            }
        }
        #endregion

        #region メンバ変数
        private string mPath;
        private Head mHead;
        private Dictionary<int, Track> mTracks;
        #endregion

        #region privateメソッド
        private static UInt16 ReadUInt16(BinaryReader br) {
            return (UInt16)((br.ReadByte() << 8) | br.ReadByte());
        }

        private static UInt32 ReadUInt32(BinaryReader br) {
            return (UInt32)((br.ReadByte() << 24) | (br.ReadByte() << 16) | (br.ReadByte() << 8) | br.ReadByte());
        }

        private static UInt32 ReadDelta(MemoryStream ms) {
            UInt32 temp = (UInt32)ms.ReadByte();
            UInt32 retVal = temp & 0x7F;

            while (0x7F < temp) {
                temp = (UInt32)ms.ReadByte();
                retVal <<= 7;
                retVal |= temp & 0x7F;
            }

            return retVal;
        }

        private static void WriteUInt16(MemoryStream ms, UInt16 value) {
            ms.WriteByte((byte)(value >> 8));
            ms.WriteByte((byte)(value & 0xFF));
        }

        private static void WriteUInt32(MemoryStream ms, UInt32 value) {
            ms.WriteByte((byte)((value >> 24) & 0xFF));
            ms.WriteByte((byte)((value >> 16) & 0xFF));
            ms.WriteByte((byte)((value >> 8) & 0xFF));
            ms.WriteByte((byte)(value & 0xFF));
        }

        private static void WriteDelta(MemoryStream ms, UInt32 value) {
            if (0 < (value >> 21)) {
                ms.WriteByte((byte)(0x80 | ((value >> 21) & 0x7F)));
                ms.WriteByte((byte)(0x80 | ((value >> 14) & 0x7F)));
                ms.WriteByte((byte)(0x80 | ((value >> 7) & 0x7F)));
                ms.WriteByte((byte)(value & 0x7F));
                return;
            }

            if (0 < (value >> 14)) {
                ms.WriteByte((byte)(0x80 | ((value >> 14) & 0x7F)));
                ms.WriteByte((byte)(0x80 | ((value >> 7) & 0x7F)));
                ms.WriteByte((byte)(value & 0x7F));
                return;
            }

            if (0 < (value >> 7)) {
                ms.WriteByte((byte)(0x80 | ((value >> 7) & 0x7F)));
                ms.WriteByte((byte)(value & 0x7F));
                return;
            }

            ms.WriteByte((byte)value);
            return;
        }

        private static byte[] ToDelta(UInt32 value) {
            if (0 < (value >> 21)) {
                return new byte[] {
                    (byte)(0x80 | ((value >> 21) & 0x7F)),
                    (byte)(0x80 | ((value >> 14) & 0x7F)),
                    (byte)(0x80 | ((value >> 7) & 0x7F)),
                    (byte)(value & 0x7F)
                };
            }

            if (0 < (value >> 14)) {
                return new byte[] {
                    (byte)(0x80 | ((value >> 14) & 0x7F)),
                    (byte)(0x80 | ((value >> 7) & 0x7F)),
                    (byte)(value & 0x7F)
                };
            }

            if (0 < (value >> 7)) {
                return new byte[] {
                    (byte)(0x80 | ((value >> 7) & 0x7F)),
                    (byte)(value & 0x7F)
                };
            }

            return new byte[] { (byte)value };
        }

        private static Message ReadMessage(MemoryStream ms, ref int currentStatus) {
            EVENT_TYPE type;
            byte ch;

            var inputStatus = ms.ReadByte();

            if (inputStatus < 0x80) {
                // ランニングステータス
                ms.Seek(-1, SeekOrigin.Current);
                inputStatus = currentStatus;
            }
            else {
                // ステータスの更新
                currentStatus = inputStatus;
            }

            if (inputStatus < 0xF0) {
                // チャンネルメッセージ
                type = (EVENT_TYPE)(inputStatus & 0xF0);
                ch = (byte)(inputStatus & 0x0F);
            }
            else {
                // システムメッセージ
                type = (EVENT_TYPE)inputStatus;
                ch = 0xF0;
            }

            switch (type) {
            // 2バイトメッセージ
            case EVENT_TYPE.NOTE_ON: {
                    var v1 = (byte)ms.ReadByte();
                    var v2 = (byte)ms.ReadByte();
                    if (0 == v2) {
                        return new Message(EVENT_TYPE.NOTE_OFF, ch, v1, v2);
                    }
                    else {
                        return new Message(EVENT_TYPE.NOTE_ON, ch, v1, v2);
                    }
                }

            case EVENT_TYPE.NOTE_OFF:
            case EVENT_TYPE.POLY_KEY:
            case EVENT_TYPE.CTRL_CHG:
            case EVENT_TYPE.PITCH:
                return new Message(type, ch, (byte)ms.ReadByte(), (byte)ms.ReadByte());

            // 1バイトメッセージ
            case EVENT_TYPE.PRGM_CHG:
            case EVENT_TYPE.CH_PRESS:
                return new Message(type, ch, (byte)ms.ReadByte());

            // システムエクスクルーシブ
            case EVENT_TYPE.SYS_EX: {
                    var temp = new byte[ReadDelta(ms)];
                    ms.Read(temp, 0, temp.Length);
                    return new Message(EVENT_TYPE.SYS_EX, 0, temp);
                }

            // メタデータ
            case EVENT_TYPE.META: {
                    var meta = (META_TYPE)ms.ReadByte();
                    var data = new byte[ReadDelta(ms)];
                    ms.Read(data, 0, data.Length);
                    return new Message(meta, data);
                }

            default:
                return new Message();
            }
        }

        private static void WriteMessage(MemoryStream ms, Message msg) {
            switch (msg.Type) {
            // 2バイトメッセージ
            case EVENT_TYPE.NOTE_ON:
            case EVENT_TYPE.NOTE_OFF:
            case EVENT_TYPE.POLY_KEY:
            case EVENT_TYPE.CTRL_CHG:
            case EVENT_TYPE.PITCH:
                ms.WriteByte(msg.Status);
                ms.WriteByte(msg.V1);
                ms.WriteByte(msg.V2);
                return;

            // 1バイトメッセージ
            case EVENT_TYPE.PRGM_CHG:
            case EVENT_TYPE.CH_PRESS:
                ms.WriteByte(msg.Status);
                ms.WriteByte(msg.V1);
                return;

            // システムエクスクルーシブ
            case EVENT_TYPE.SYS_EX:
                ms.WriteByte(msg.Status);
                WriteDelta(ms, (uint)(msg.Data.Length - 1));
                ms.Write(msg.Data, 1, msg.Data.Length - 1);
                return;

            // メタデータ
            case EVENT_TYPE.META:
                ms.WriteByte(msg.Status);
                ms.WriteByte(msg.V1);
                WriteDelta(ms, (uint)msg.Meta.Length);
                ms.Write(msg.Meta.Data, 0, msg.Meta.Length);
                return;

            default:
                return;
            }
        }
        #endregion

        public int Ticks {
            get {
                return mHead.Ticks;
            }
        }

        public Event[] EventList {
            get {
                HashSet<Event> hash = new HashSet<Event>();
                foreach (var tr in mTracks) {
                    foreach (var ev in tr.Value.Events) {
                        hash.Add(ev);
                    }
                }

                Event[] evList = new Event[hash.Count];
                hash.CopyTo(evList);
                Array.Sort(evList, new Comparison<Event>((a, b) => (
                    0 == ((((long)a.Time << 16) | (a.Track << 8) | a.Index) - (((long)b.Time << 16) | (b.Track << 8) | b.Index)) ? 0 :
                    0 < ((((long)a.Time << 16) | (a.Track << 8) | a.Index) - (((long)b.Time << 16) | (b.Track << 8) | b.Index)) ? 1 : -1
                )));

                return evList;
            }
        }

        public SMF(FORMAT format = FORMAT.FORMAT1, ushort ticks = 960) {
            mHead = new Head(format, 0, ticks);
            mTracks = new Dictionary<int, Track>();
        }

        public SMF(string filePath) {
            FileStream fs = new FileStream(filePath, FileMode.Open);
            BinaryReader br = new BinaryReader(fs);

            mPath = filePath;
            mHead = new Head(br);
            mTracks = new Dictionary<int, Track>();

            for (int i = 0; i < mHead.Tracks; ++i) {
                mTracks.Add(i, new Track(br, i));
            }

            br.Close();
        }
    }
}