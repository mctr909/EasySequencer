using System;
using System.IO;
using System.Collections.Generic;

namespace MIDI {
    public class SMF {
        private struct Header {
            public readonly FORMAT Format;
            public ushort Tracks;
            public readonly ushort Ticks;

            public Header(FORMAT format, ushort tracks, ushort ticks) {
                Format = format;
                Tracks = tracks;
                Ticks = ticks;
            }

            public Header(BinaryReader br) {
                ReadUI32(br);
                ReadUI32(br);
                Format = (FORMAT)ReadUI16(br);
                Tracks = ReadUI16(br);
                Ticks = ReadUI16(br);

                if (!Enum.IsDefined(typeof(FORMAT), Format)) {
                    Format = FORMAT.INVALID;
                }
            }

            public void Write(MemoryStream ms) {
                WriteUI32(ms, 0x4D546864);
                WriteUI32(ms, 6);
                WriteUI16(ms, (ushort)Format);
                WriteUI16(ms, Tracks);
                WriteUI16(ms, Ticks);
            }
        }

        public class Track {
            public readonly ushort No;
            public HashSet<Event> Events { get; private set; }

            public Track(ushort no) {
                No = no;
                Events = new HashSet<Event>();
            }

            public Track(BinaryReader br, ushort no) {
                No = no;
                Events = new HashSet<Event>();

                ReadUI32(br);
                int size = (int)ReadUI32(br);

                var ms = new MemoryStream(br.ReadBytes(size), false);
                uint time = 0;
                ushort index = 0;
                int currentStatus = 0;
                while (ms.Position < ms.Length) {
                    var delta = ReadDelta(ms);
                    time += delta;
                    if (0 == delta) {
                        ++index;
                    } else {
                        index = 0;
                    }
                    Events.Add(new Event(time, No, index, ReadMessage(ms, ref currentStatus)));
                }
            }

            public void Write(MemoryStream ms) {
                var temp = new MemoryStream();
                WriteUI32(temp, 0);
                WriteUI32(temp, 0);

                uint currentTime = 0;
                foreach (var ev in Events) {
                    WriteDelta(temp, ev.Time - currentTime);
                    WriteMessage(temp, ev.Message);
                    currentTime = ev.Time;
                }

                temp.Seek(0, SeekOrigin.Begin);
                WriteUI32(temp, 0x4D54726B);
                WriteUI32(temp, (uint)(temp.Length - 8));

                temp.WriteTo(ms);
            }

            private static Message ReadMessage(MemoryStream ms, ref int currentStatus) {
                EVENT_TYPE type;
                byte ch;

                var status = ms.ReadByte();

                if (status < 0x80) {
                    // ランニングステータス
                    ms.Seek(-1, SeekOrigin.Current);
                    status = currentStatus;
                } else {
                    // ステータスの更新
                    currentStatus = status;
                }

                if (status < 0xF0) {
                    // チャンネルメッセージ
                    type = (EVENT_TYPE)(status & 0xF0);
                    ch = (byte)(status & 0x0F);
                } else {
                    // システムメッセージ
                    type = (EVENT_TYPE)status;
                    ch = 0xF0;
                }

                switch (type) {
                    // 2バイトメッセージ
                    case EVENT_TYPE.NOTE_ON:
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
                    case EVENT_TYPE.SYS_EX:
                        return new Message(EVENT_TYPE.SYS_EX, ReadBytes(ms));
                    // メタデータ
                    case EVENT_TYPE.META:
                        return new Message((META_TYPE)ms.ReadByte(), ReadBytes(ms));
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
        }

        private string mPath;
        private Header mHead;
        private Dictionary<int, Track> mTracks;

        public int Ticks { get { return mHead.Ticks; } }

        public Event[] EventList {
            get {
                var hash = new HashSet<Event>();
                foreach (var tr in mTracks) {
                    foreach (var ev in tr.Value.Events) {
                        hash.Add(ev);
                    }
                }

                var evList = new Event[hash.Count];
                hash.CopyTo(evList);
                Array.Sort(evList, Event.Compare);

                return evList;
            }
        }

        public SMF(FORMAT format = FORMAT.FORMAT1, ushort ticks = 960) {
            mHead = new Header(format, 0, ticks);
            mTracks = new Dictionary<int, Track>();
        }

        public SMF(string filePath) {
            var fs = new FileStream(filePath, FileMode.Open);
            var br = new BinaryReader(fs);

            mPath = filePath;
            mHead = new Header(br);
            mTracks = new Dictionary<int, Track>();

            for (ushort i = 0; i < mHead.Tracks; ++i) {
                mTracks.Add(i, new Track(br, i));
            }

            br.Close();
        }

        private static ushort ReadUI16(BinaryReader br) {
            return (ushort)((br.ReadByte() << 8) | br.ReadByte());
        }

        private static uint ReadUI32(BinaryReader br) {
            return (uint)((br.ReadByte() << 24) | (br.ReadByte() << 16) | (br.ReadByte() << 8) | br.ReadByte());
        }

        private static uint ReadDelta(MemoryStream ms) {
            var temp = (uint)ms.ReadByte();
            var retVal = temp & 0x7F;

            while (0x7F < temp) {
                temp = (uint)ms.ReadByte();
                retVal <<= 7;
                retVal |= temp & 0x7F;
            }

            return retVal;
        }

        private static byte[] ReadBytes(MemoryStream ms) {
            var arr = new byte[ReadDelta(ms)];
            ms.Read(arr, 0, arr.Length);
            return arr;
        }

        private static void WriteUI16(MemoryStream ms, ushort value) {
            ms.WriteByte((byte)(value >> 8));
            ms.WriteByte((byte)(value & 0xFF));
        }

        private static void WriteUI32(MemoryStream ms, uint value) {
            ms.WriteByte((byte)((value >> 24) & 0xFF));
            ms.WriteByte((byte)((value >> 16) & 0xFF));
            ms.WriteByte((byte)((value >> 8) & 0xFF));
            ms.WriteByte((byte)(value & 0xFF));
        }

        private static void WriteDelta(MemoryStream ms, uint value) {
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
    }
}
