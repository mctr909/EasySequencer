namespace MIDI {
    public struct Message {
        public byte[] Data { get; private set; }

        public EVENT_TYPE Type {
            get { return (EVENT_TYPE)((0xF0 <= Data[0]) ? Data[0] : (Data[0] & 0xF0)); }
        }

        public int Channel { get { return Data[0] & 0x0F; } }

        public byte Status { get { return Data[0]; } }

        public byte V1 { get { return Data[1]; } }

        public byte V2 { get { return Data[2]; } }

        public Meta Meta { get { return new Meta(Data); } }

        public Message(params byte[] data) {
            Data = new byte[data.Length];
            data.CopyTo(Data, 0);
        }

        public Message(EVENT_TYPE type, params byte[] data) {
            Data = new byte[data.Length + 1];
            Data[0] = (byte)type;
            data.CopyTo(Data, 1);
        }

        public Message(EVENT_TYPE type, byte channel, params byte[] data) {
            Data = new byte[data.Length + 1];
            Data[0] = (byte)((int)type | channel);
            data.CopyTo(Data, 1);
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
            data.CopyTo(Data, 2);
        }
    }
}
