using System;
using System.IO;

namespace MIDI {
    public struct Meta {
        private const string ENC = "shift-jis";

        public E_META_TYPE Type { get; private set; }

        public byte[] Data { get; private set; }

        public double Tempo {
            get {
                if (E_META_TYPE.TEMPO == Type) {
                    return 60000000.0 / ((Data[0] << 16) | (Data[1] << 8) | Data[2]);
                } else {
                    return 0.0;
                }
            }
        }

        public E_KEY Key {
            get {
                if (E_META_TYPE.KEY == Type) {
                    return (E_KEY)((Data[0] << 8) | Data[1]);
                } else {
                    return E_KEY.INVALID;
                }
            }
        }

        public int MeasureNumer {
            get {
                if (E_META_TYPE.MEASURE == Type) {
                    return Data[0];
                } else {
                    return 0;
                }
            }
        }

        public int MeasureDenomi {
            get {
                if (E_META_TYPE.MEASURE == Type) {
                    return (int)System.Math.Pow(2.0, Data[1]);
                } else {
                    return 0;
                }
            }
        }

        public string Text {
            get {
                switch (Type) {
                    case E_META_TYPE.TEXT:
                    case E_META_TYPE.COMPOSER:
                    case E_META_TYPE.SEQ_NAME:
                    case E_META_TYPE.INST_NAME:
                    case E_META_TYPE.LYRIC:
                    case E_META_TYPE.MARKER:
                    case E_META_TYPE.PRG_NAME:
                        return System.Text.Encoding.GetEncoding(ENC).GetString(Data, 0, Data.Length);
                    default:
                        return null;
                }
            }
        }

        public void Write(MemoryStream ms) {
            ms.WriteByte((byte)Type);
            Util.WriteDelta(ms, (uint)Data.Length);
            ms.Write(Data, 0, Data.Length);
        }

        public Meta(params byte[] data) {
            Type = (E_META_TYPE)data[0];
            Data = new byte[data.Length - 1];
            Array.Copy(data, 1, Data, 0, Data.Length);
        }

        public Meta(E_META_TYPE type, params byte[] data) {
            Type = type;
            Data = data;
        }

        public Meta(E_META_TYPE type, string text) {
            switch (type) {
                case E_META_TYPE.TEXT:
                case E_META_TYPE.COMPOSER:
                case E_META_TYPE.SEQ_NAME:
                case E_META_TYPE.INST_NAME:
                case E_META_TYPE.LYRIC:
                case E_META_TYPE.MARKER:
                case E_META_TYPE.PRG_NAME:
                    Type = type;
                    Data = System.Text.Encoding.GetEncoding(ENC).GetBytes(text);
                    break;
                default:
                    Type = E_META_TYPE.INVALID;
                    Data = null;
                    break;
            }
        }
    }
}
