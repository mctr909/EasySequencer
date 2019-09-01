using System.IO;

namespace MIDI {
    public struct Message {
        public byte Status { get; private set; }
        public byte[] Data { get; private set; }

        public E_EVENT_TYPE Type { get { return (E_EVENT_TYPE)((Status < 0xF0) ? (Status & 0xF0) : Status); } }

        public E_CTRL_TYPE CtrlType { get { return (E_EVENT_TYPE.CTRL_CHG == Type) ? (E_CTRL_TYPE)Data[0] : E_CTRL_TYPE.INVALID; } }

        public int Channel { get { return (Status < 0xF0) ? (Status & 0x0F) : 0; } }

        public byte NoteNo { get { return (E_EVENT_TYPE.NOTE_OFF == Type || E_EVENT_TYPE.NOTE_ON == Type) ? Data[0] : (byte)0; } }

        public byte Velocity { get { return (E_EVENT_TYPE.NOTE_OFF == Type || E_EVENT_TYPE.NOTE_ON == Type) ? Data[1] : (byte)0; } }

        public byte CtrlValue { get { return (E_EVENT_TYPE.CTRL_CHG == Type) ? Data[1] : (byte)0; } }

        public byte ProgramNo { get { return (E_EVENT_TYPE.PROG_CHG == Type) ? Data[0] : (byte)0; } }

        public short Pitch { get { return (short)((E_EVENT_TYPE.PITCH == Type) ? (((Data[1] << 7) | Data[0]) - 8192) : 0); } }

        public Meta Meta { get { return new Meta(Data); } }

        public Message(byte status, params byte[] data) {
            Status = status;
            Data = new byte[data.Length];
            data.CopyTo(Data, 0);
        }

        public Message(E_EVENT_TYPE type, byte channel, params byte[] data) {
            Status = (byte)((byte)type | channel);
            Data = new byte[data.Length];
            data.CopyTo(Data, 0);
        }

        public Message(E_CTRL_TYPE type, byte channel, params byte[] data) {
            Status = (byte)((byte)E_EVENT_TYPE.CTRL_CHG | channel);
            Data = new byte[data.Length + 1];
            Data[0] = (byte)type;
            data.CopyTo(Data, 1);
        }

        public Message(E_META_TYPE type, params byte[] data) {
            Status = (byte)E_EVENT_TYPE.META;
            Data = new byte[data.Length + 1];
            Data[0] = (byte)type;
            data.CopyTo(Data, 1);
        }

        public static Message Load(MemoryStream ms, ref int currentStatus) {
            var status = ms.ReadByte();

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
                ch = 0xF0;
            }

            switch (type) {
            case E_EVENT_TYPE.NOTE_ON:
            case E_EVENT_TYPE.NOTE_OFF:
            case E_EVENT_TYPE.POLY_KEY:
            case E_EVENT_TYPE.CTRL_CHG:
            case E_EVENT_TYPE.PITCH:
                return new Message((byte)((byte)type | ch), (byte)ms.ReadByte(), (byte)ms.ReadByte());
            case E_EVENT_TYPE.PROG_CHG:
            case E_EVENT_TYPE.CH_PRESS:
                return new Message((byte)((byte)type | ch), (byte)ms.ReadByte());
            case E_EVENT_TYPE.SYS_EX:
                return new Message((byte)type, Util.ReadBytes(ms));
            case E_EVENT_TYPE.META:
                return new Message((E_META_TYPE)ms.ReadByte(), Util.ReadBytes(ms));
            default:
                return new Message();
            }
        }

        public static Message NoteOn(byte channel, byte noteNo, byte velocity) {
            return new Message((byte)((byte)E_EVENT_TYPE.NOTE_ON | channel), noteNo, velocity);
        }

        public static Message NoteOff(byte channel, byte noteNo, byte velocity = 0) {
            return new Message((byte)((byte)E_EVENT_TYPE.NOTE_OFF | channel), noteNo, velocity);
        }
    }
}
