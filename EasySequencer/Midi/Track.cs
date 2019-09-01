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
                Events.Add(new Event(time, No, index, Message.Load(ms, ref currentStatus)));
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

        private static void WriteMessage(MemoryStream ms, Message msg) {
            switch (msg.Type) {
                // 2バイトメッセージ
                case E_EVENT_TYPE.NOTE_ON:
                case E_EVENT_TYPE.NOTE_OFF:
                case E_EVENT_TYPE.POLY_KEY:
                case E_EVENT_TYPE.CTRL_CHG:
                case E_EVENT_TYPE.PITCH:
                    ms.WriteByte(msg.Status);
                    ms.WriteByte(msg.Data[0]);
                    ms.WriteByte(msg.Data[1]);
                    return;
                // 1バイトメッセージ
                case E_EVENT_TYPE.PROG_CHG:
                case E_EVENT_TYPE.CH_PRESS:
                    ms.WriteByte(msg.Status);
                    ms.WriteByte(msg.Data[0]);
                    return;
                // システムエクスクルーシブ
                case E_EVENT_TYPE.SYS_EX:
                    ms.WriteByte(msg.Status);
                    Util.WriteDelta(ms, (uint)(msg.Data.Length - 1));
                    ms.Write(msg.Data, 1, msg.Data.Length - 1);
                    return;
                // メタデータ
                case E_EVENT_TYPE.META:
                    ms.WriteByte(msg.Status);
                    msg.Meta.Write(ms);
                    return;
                default:
                    return;
            }
        }
    }
}
