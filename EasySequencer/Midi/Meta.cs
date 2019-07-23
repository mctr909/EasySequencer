namespace MIDI {
    public class Meta {
        public byte[] Data { get; private set; }

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
            data.CopyTo(Data, 2);
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
}
