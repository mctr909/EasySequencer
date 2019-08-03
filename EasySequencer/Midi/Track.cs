using System.IO;
using System.Collections.Generic;

namespace MIDI {
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

            Util.ReadUI32(br);
            int size = (int)Util.ReadUI32(br);

            var ms = new MemoryStream(br.ReadBytes(size), false);
            uint time = 0;
            ushort index = 0;
            int currentStatus = 0;
            while (ms.Position < ms.Length) {
                var delta = Util.ReadDelta(ms);
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
            Util.WriteUI32(temp, 0x4D54726B);
            Util.WriteUI32(temp, 0);

            uint currentTime = 0;
            foreach (var ev in Events) {
                Util.WriteDelta(temp, ev.Time - currentTime);
                WriteMessage(temp, ev.Message);
                currentTime = ev.Time;
            }

            temp.Seek(4, SeekOrigin.Begin);
            Util.WriteUI32(temp, (uint)(temp.Length - 8));

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
                    return new Message(EVENT_TYPE.SYS_EX, Util.ReadBytes(ms));
                // メタデータ
                case EVENT_TYPE.META:
                    return new Message((META_TYPE)ms.ReadByte(), Util.ReadBytes(ms));
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
                    Util.WriteDelta(ms, (uint)(msg.Data.Length - 1));
                    ms.Write(msg.Data, 1, msg.Data.Length - 1);
                    return;
                // メタデータ
                case EVENT_TYPE.META:
                    msg.Meta.Write(ms);
                    return;
                default:
                    return;
            }
        }
    }
}
