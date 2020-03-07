using System;
using System.IO;

namespace MIDI {
    public struct Event {
        public uint Time { get; private set; }
        public byte[] Data { get; private set; }

        public byte Status {
            get { return Data[0]; }
        }
        public int Channel {
            get { return (Status < 0xF0) ? (Status & 0x0F) : 0; }
        }
        public E_EVENT_TYPE Type {
            get { return (E_EVENT_TYPE)((Status < 0xF0) ? (Status & 0xF0) : Status); }
        }

        public byte NoteNo {
            get { return (E_EVENT_TYPE.NOTE_OFF == Type || E_EVENT_TYPE.NOTE_ON == Type) ? Data[1] : (byte)0; }
        }
        public byte Velocity {
            get { return (E_EVENT_TYPE.NOTE_OFF == Type || E_EVENT_TYPE.NOTE_ON == Type) ? Data[2] : (byte)0; }
        }

        public E_CTRL_TYPE CtrlType {
            get { return E_EVENT_TYPE.CTRL_CHG == Type ? (E_CTRL_TYPE)Data[1] : E_CTRL_TYPE.INVALID; }
        }
        public byte CtrlValue {
            get { return E_EVENT_TYPE.CTRL_CHG == Type ? Data[2] : (byte)0; }
        }

        public byte ProgNo {
            get { return E_EVENT_TYPE.PROG_CHG == Type ? Data[1] : (byte)0; }
        }

        public short Pitch {
            get { return (short)(E_EVENT_TYPE.PITCH == Type ? (((Data[2] << 7) | Data[1]) - 8192) : 0); }
        }

        public Meta Meta {
            get { return E_EVENT_TYPE.META == Type ? new Meta(Data) : null; }
        }

        public Event(byte status, params byte[] data) {
            Time = 0;
            Data = new byte[data.Length + 1];
            Data[0] = status;
            data.CopyTo(Data, 1);
        }

        public Event(MemoryStream ms, uint time, ref byte currentStatus) {
            Time = time;
            var status = (byte)ms.ReadByte();

            if (status < 0x80) {
                ms.Seek(-1, SeekOrigin.Current);
                status = currentStatus;
            } else {
                currentStatus = status;
            }

            E_EVENT_TYPE type;
            byte ch;
            if (status < 0xF0) {
                type = (E_EVENT_TYPE)(status & 0xF0);
                ch = (byte)(status & 0x0F);
            } else {
                type = (E_EVENT_TYPE)status;
                ch = 0;
            }

            switch (type) {
            case E_EVENT_TYPE.NOTE_ON:
            case E_EVENT_TYPE.NOTE_OFF:
            case E_EVENT_TYPE.POLY_KEY:
            case E_EVENT_TYPE.CTRL_CHG:
            case E_EVENT_TYPE.PITCH:
                Data = new byte[] {
                    status,
                    (byte)ms.ReadByte(),
                    (byte)ms.ReadByte()
                };
                break;
            case E_EVENT_TYPE.PROG_CHG:
            case E_EVENT_TYPE.CH_PRESS:
                Data = new byte[] {
                    status,
                    (byte)ms.ReadByte()
                };
                break;
            case E_EVENT_TYPE.SYS_EX:
                var sysEx = Util.ReadBytes(ms);
                Data = new byte[sysEx.Length + 1];
                Data[0] = status;
                sysEx.CopyTo(Data, 1);
                break;
            case E_EVENT_TYPE.META:
                var metaType = (byte)ms.ReadByte();
                var metaData = Util.ReadBytes(ms);
                Data = new byte[metaData.Length + 2];
                Data[0] = status;
                Data[1] = metaType;
                metaData.CopyTo(Data, 2);
                break;
            default:
                Data = new byte[] { 0 };
                break;
            }
        }

        public Event(E_EVENT_TYPE type, byte channel, params byte[] data) {
            Time = 0;
            Data = new byte[data.Length + 1];
            Data[0] = (byte)((byte)type | channel);
            data.CopyTo(Data, 1);
        }

        public Event(E_CTRL_TYPE type, byte channel, params byte[] data) {
            Time = 0;
            Data = new byte[data.Length + 2];
            Data[0] = (byte)((byte)E_EVENT_TYPE.CTRL_CHG | channel);
            Data[1] = (byte)type;
            data.CopyTo(Data, 2);
        }

        public Event(E_META_TYPE type, params byte[] data) {
            Time = 0;
            Data = new byte[data.Length + 2];
            Data[0] = (byte)E_EVENT_TYPE.META;
            Data[1] = (byte)type;
            data.CopyTo(Data, 2);
        }

        public void WriteMessage(MemoryStream ms) {
            switch (Type) {
            // 2バイトメッセージ
            case E_EVENT_TYPE.NOTE_ON:
            case E_EVENT_TYPE.NOTE_OFF:
            case E_EVENT_TYPE.POLY_KEY:
            case E_EVENT_TYPE.CTRL_CHG:
            case E_EVENT_TYPE.PITCH:
                ms.WriteByte(Data[0]);
                ms.WriteByte(Data[1]);
                ms.WriteByte(Data[2]);
                return;
            // 1バイトメッセージ
            case E_EVENT_TYPE.PROG_CHG:
            case E_EVENT_TYPE.CH_PRESS:
                ms.WriteByte(Data[0]);
                ms.WriteByte(Data[1]);
                return;
            // システムエクスクルーシブ
            case E_EVENT_TYPE.SYS_EX:
                ms.WriteByte(Data[0]);
                Util.WriteDelta(ms, (uint)(Data.Length - 2));
                ms.Write(Data, 2, Data.Length - 2);
                return;
            // メタデータ
            case E_EVENT_TYPE.META:
                ms.WriteByte(Status);
                Meta.Write(ms);
                return;
            default:
                return;
            }
        }

        public static Event NoteOn(byte channel, byte noteNo, byte velocity) {
            return new Event((byte)((byte)E_EVENT_TYPE.NOTE_ON | channel), noteNo, velocity);
        }

        public static Event NoteOff(byte channel, byte noteNo, byte velocity = 0) {
            return new Event((byte)((byte)E_EVENT_TYPE.NOTE_OFF | channel), noteNo, velocity);
        }

        public static readonly Comparison<Event> Compare = new Comparison<Event>((a, b) => {
            var dTime = (long)a.Time - b.Time;
            if (0 == dTime) {
                var aV = (uint)a.Type;
                var bV = (uint)b.Type;
                if (aV < 0xA0) {
                    aV += 0x200;
                    aV |= (uint)a.Channel << 10;
                } else if (aV < 0xF0) {
                    aV += 0x100;
                    aV |= (uint)a.Channel << 10;
                }
                if (bV < 0xA0) {
                    bV += 0x200;
                    bV |= (uint)b.Channel << 10;
                } else if (bV < 0xF0) {
                    bV += 0x100;
                    bV |= (uint)b.Channel << 10;
                }
                var dComp = (long)aV - bV;
                return 0 == dComp ? 0 : (0 < dComp ? 1 : -1);
            } else {
                return 0 < dTime ? 1 : -1;
            }
        });
    }
}
