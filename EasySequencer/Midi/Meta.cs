using System.IO;

namespace MIDI {
    public struct Meta {
        private byte[] mData;

        public META_TYPE Type {
            get { return (META_TYPE)mData[1]; }
        }

        public double Tempo {
            get {
                if (META_TYPE.TEMPO == Type) {
                    return 60000000.0 / ((mData[2] << 16) | (mData[3] << 8) | mData[4]);
                } else {
                    return 0.0;
                }
            }
        }

        public KEY Key {
            get {
                if (META_TYPE.KEY == Type) {
                    return (KEY)((mData[2] << 8) | mData[3]);
                } else {
                    return KEY.INVALID;
                }
            }
        }

        public int MeasureNumer {
            get {
                if (META_TYPE.MEASURE == Type) {
                    return mData[2];
                } else {
                    return 0;
                }
            }
        }

        public int MeasureDenomi {
            get {
                if (META_TYPE.MEASURE == Type) {
                    return (int)System.Math.Pow(2.0, mData[3]);
                } else {
                    return 0;
                }
            }
        }

        public string Text {
            get {
                switch (Type) {
                    case META_TYPE.TEXT:
                    case META_TYPE.COMPOSER:
                    case META_TYPE.SEQ_NAME:
                    case META_TYPE.INST_NAME:
                    case META_TYPE.LYRIC:
                    case META_TYPE.MARKER:
                    case META_TYPE.PRG_NAME:
                        return System.Text.Encoding.GetEncoding("shift-jis").GetString(mData, 2, mData.Length - 2);
                    default:
                        return null;
                }
            }
        }

        public Meta(params byte[] data) {
            mData = data;
        }

        public Meta(META_TYPE type, params byte[] data) {
            mData = new byte[data.Length + 2];
            mData[0] = (byte)EVENT_TYPE.META;
            mData[1] = (byte)type;
            data.CopyTo(mData, 2);
        }

        public void Write(MemoryStream ms) {
            ms.WriteByte(mData[0]);
            ms.WriteByte(mData[1]);
            Util.WriteDelta(ms, (uint)mData.Length - 2);
            ms.Write(mData, 2, mData.Length - 2);
        }

        //public new string ToString() {
        //    switch (Type) {
        //    case META_TYPE.SEQ_NO:
        //        return string.Format("[{0}]\t{1}", Type, (Data[2] << 8) | Data[3]);
        //    case META_TYPE.CH_PREFIX:
        //    case META_TYPE.PORT:
        //        return string.Format("[{0}]\t{1}", Type, Data[2]);
        //    case META_TYPE.MEASURE:
        //        return string.Format("[{0}]\t{1}/{2} ({3}, {4})", Type, Data[2], (int)System.Math.Pow(2.0, Data[3]), Data[4], Data[5]);
        //    case META_TYPE.META:
        //        return string.Format("[{0}]\t{1}", Type, System.BitConverter.ToString(Data, 2));
        //    default:
        //        return string.Format("[{0}]", Type);
        //    }
        //}
    }
}
