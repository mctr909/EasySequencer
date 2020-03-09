using System;
using System.IO;

namespace MIDI {
    public class Meta {
        private const string ENC = "shift-jis";
        private byte[] mData;

        public E_META_TYPE Type { get; private set; }

        public E_KEY Key {
            get {
                if (E_META_TYPE.KEY == Type) {
                    return (E_KEY)((mData[0] << 8) | mData[1]);
                } else {
                    return E_KEY.INVALID;
                }
            }
        }

        public double Tempo {
            get {
                if (E_META_TYPE.TEMPO == Type) {
                    return 60000000.0 / ((mData[0] << 16) | (mData[1] << 8) | mData[2]);
                } else {
                    return 0.0;
                }
            }
        }

        public int MeasureNumer {
            get {
                if (E_META_TYPE.MEASURE == Type) {
                    return mData[0];
                } else {
                    return 0;
                }
            }
        }

        public int MeasureDenomi {
            get {
                if (E_META_TYPE.MEASURE == Type) {
                    return (int)System.Math.Pow(2.0, mData[1]);
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
                    return System.Text.Encoding.GetEncoding(ENC).GetString(mData, 0, mData.Length);
                default:
                    return null;
                }
            }
        }

        public void Write(MemoryStream ms) {
            ms.WriteByte((byte)Type);
            Util.WriteDelta(ms, (uint)mData.Length);
            ms.Write(mData, 0, mData.Length);
        }

        public Meta(byte[] data) {
            Type = (E_META_TYPE)data[1];
            mData = new byte[data.Length - 6];
            Array.Copy(data, 6, mData, 0, mData.Length);
        }

        public Meta(E_META_TYPE type, params byte[] data) {
            Type = type;
            mData = data;
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
                mData = System.Text.Encoding.GetEncoding(ENC).GetBytes(text);
                break;
            default:
                Type = E_META_TYPE.INVALID;
                mData = null;
                break;
            }
        }
    }
}
