namespace MIDI {
    public enum EVENT_TYPE : byte {
        INVALID  = 0x00,
        NOTE_OFF = 0x80,
        NOTE_ON  = 0x90,
        POLY_KEY = 0xA0,
        CTRL_CHG = 0xB0,
        PRGM_CHG = 0xC0,
        CH_PRESS = 0xD0,
        PITCH    = 0xE0,
        SYS_EX   = 0xF0,
        META     = 0xFF
    }

    public enum CTRL_TYPE : byte {
        BANK_MSB = 0,
        BANK_LSB = 32,

        VOLUME     = 7,
        PAN        = 10,
        EXPRESSION = 11,

        MODULATION      = 1,
        PORTAMENTO      = 65,
        PORTAMENTO_TIME = 5,

        HOLD = 64,

        RESONANCE = 71,
        CUTOFF    = 74,

        RELEACE = 72,
        ATTACK  = 73,

        VIB_RATE  = 76,
        VIB_DEPTH = 77,
        VIB_DELAY = 78,

        REVERB = 91,
        CHORUS = 93,
        DELAY  = 94,

        NRPN_LSB = 98,
        NRPN_MSB = 99,
        RPN_LSB  = 100,
        RPN_MSB  = 101,
        DATA     = 6,

        ALL_RESET = 121
    }

    public enum META_TYPE : byte {
        SEQ_NO    = 0x00,
        TEXT      = 0x01,
        COMPOSER  = 0x02,
        SEQ_NAME  = 0x03,
        INST_NAME = 0x04,
        LYRIC     = 0x05,
        MARKER    = 0x06,
        QUEUE     = 0x07,
        PRG_NAME  = 0x08,
        DEVICE    = 0x09,
        //
        CH_PREFIX = 0x20,
        PORT      = 0x21,
        TRACK_END = 0x2F,
        //
        TEMPO     = 0x51,
        SMPTE     = 0x54,
        MEASURE   = 0x58,
        KEY       = 0x59,
        //
        META      = 0x7F,
        //
        INVALID   = 0xFF
    }

    public enum KEY : ushort {
        //
        G_MAJOR  = 0x0100,
        E_MINOR  = 0x0101,
        D_MAJOR  = 0x0200,
        B_MINOR  = 0x0201,
        A_MAJOR  = 0x0300,
        Fs_MINOR = 0x0301,
        E_MAJOR  = 0x0400,
        Cs_MINOR = 0x0401,
        B_MAJOR  = 0x0500,
        Gs_MINOR = 0x0501,
        Fs_MAJOR = 0x0600,
        Ds_MINOR = 0x0601,
        Cs_MAJOR = 0x0700,
        As_MINOR = 0x0701,
        //
        C_MAJOR = 0x0000,
        A_MINOR = 0x0001,
        //
        F_MAJOR  = 0xFF00,
        D_MINOR  = 0xFF01,
        Bb_MAJOR = 0xFE00,
        G_MINOR  = 0xFE01,
        Eb_MAJOR = 0xFD00,
        C_MINOR  = 0xFD01,
        Ab_MAJOR = 0xFC00,
        F_MINOR  = 0xFC01,
        Db_MAJOR = 0xFB00,
        Bb_MINOR = 0xFB01,
        Gb_MAJOR = 0xFA00,
        Eb_MINOR = 0xFA01,
        Cb_MAJOR = 0xF900,
        Ab_MINOR = 0xF901
    }

    public class Meta {
        public readonly byte[] Data;

        public META_TYPE Type {
            get { return (META_TYPE)Data[1]; }
        }

        public int Length {
            get { return Data.Length - 2; }
        }

        public double BPM {
            get { return 60000000.0 / ((Data[2] << 16) | (Data[3] << 8) | Data[4]); }
        }

        public KEY Key {
            get { return (KEY)((Data[2] << 8) | Data[3]); }
        }

        public string Text {
            get { return System.Text.Encoding.GetEncoding("shift-jis").GetString(Data, 2, Data.Length - 2); }
        }

        public Meta(params byte[] data) {
            Data = data;
        }

        public Meta(META_TYPE type, params byte[] data) {
            Data = new byte[data.Length + 2];
            Data[0] = (byte)EVENT_TYPE.META;
            Data[1] = (byte)type;
            for (var i = 0; i < data.Length; ++i) {
                Data[i + 2] = data[i];
            }
        }

        public new string ToString() {
            switch (Type) {
            case META_TYPE.SEQ_NO:
                return string.Format("[{0}]\t{1}", Type, (Data[2] << 8) | Data[3]);

            case META_TYPE.TEXT:
            case META_TYPE.COMPOSER:
            case META_TYPE.SEQ_NAME:
            case META_TYPE.INST_NAME:
            case META_TYPE.LYRIC:
            case META_TYPE.MARKER:
            case META_TYPE.PRG_NAME:
                return string.Format("[{0}]\t\"{1}\"", Type, Text);

            case META_TYPE.CH_PREFIX:
            case META_TYPE.PORT:
                return string.Format("[{0}]\t{1}", Type, Data[2]);

            case META_TYPE.TEMPO:
                return string.Format("[{0}]\t{1}", Type, BPM.ToString("0.00"));

            case META_TYPE.MEASURE:
                return string.Format("[{0}]\t{1}/{2} ({3}, {4})", Type, Data[2], (int)System.Math.Pow(2.0, Data[3]), Data[4], Data[5]);

            case META_TYPE.KEY:
                return string.Format("[{0}]\t{1}", Type, Key);

            case META_TYPE.META:
                return string.Format("[{0}]\t{1}", Type, System.BitConverter.ToString(Data, 2));

            default:
                return string.Format("[{0}]", Type);
            }
        }
    }

    public struct Message {
        public readonly byte[] Data;

        public EVENT_TYPE Type {
            get { return (EVENT_TYPE)((0xF0 <= Data[0]) ? Data[0] : (Data[0] & 0xF0)); }
        }

        public int Channel {
            get { return (Data[0] & 0x0F); }
        }

        public byte Status { get { return Data[0]; } }

        public byte V1 { get { return Data[1]; } }

        public byte V2 { get { return Data[2]; } }

        public Meta Meta {
            get { return new Meta(Data); }
        }

        public Message(params byte[] data) {
            Data = new byte[data.Length];
            for (var i = 0; i < data.Length; ++i) {
                Data[i] = data[i];
            }
        }

        public Message(EVENT_TYPE type, byte channel, params byte[] data) {
            Data = new byte[data.Length + 1];
            Data[0] = (byte)((int)type | channel);
            for (var i = 0; i < data.Length; ++i) {
                Data[i + 1] = data[i];
            }
        }

        public Message(CTRL_TYPE type, byte channel, byte value = 0) {
            Data = new byte[3];
            Data[0] = (byte)((int)EVENT_TYPE.CTRL_CHG | channel);
            Data[1] = (byte)type;
            Data[2] = value;
        }

        public Message(META_TYPE type, byte[] data) {
            Data = new byte[data.Length + 2];
            Data[0] = (byte)EVENT_TYPE.META;
            Data[1] = (byte)type;
            for (var i = 0; i < data.Length; ++i) {
                Data[i + 2] = data[i];
            }
        }
    }

    public struct Time {
        public uint Value { get; private set; }
        public uint Index { get; private set; }

        public Time(uint value, uint index) {
            Value = value;
            Index = index;
        }

        public void Step(uint delta) {
            Value += delta;
            Index = (delta == 0 ? (Index + 1) : 0);
        }
    }

    public struct Event {
        public readonly uint Time;
        public readonly uint Track;
        public readonly uint Index;
        public readonly Message Message;

        public Event(Time time, uint trackNo, Message message) {
            Time = time.Value;
            Track = trackNo;
            Index = time.Index;
            Message = message;
        }
    }
}
