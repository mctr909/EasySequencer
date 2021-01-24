using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SMF {
    public enum E_STATUS : byte {
        NOTE_OFF = 0x80,
        NOTE_ON = 0x90,
        KEY_PRESS = 0xA0,
        CONTROL = 0xB0,
        PROGRAM = 0xC0,
        CH_PRESS = 0xD0,
        PITCH = 0xE0,
        SYSEX_BEGIN = 0xF0,
        TIME_CODE = 0xF1,
        SONG_POS = 0xF2,
        SONG_NUM = 0xF3,
        RESERVED1 = 0xF4,
        RESERVED2 = 0xF5,
        TUNE_REQ = 0xF6,
        SYSEX_END = 0xF7,
        CLOCK = 0xF8,
        RESERVED3 = 0xF9,
        START = 0xFA,
        CONTINUE = 0xFB,
        STOP = 0xFC,
        RESERVED4 = 0xFD,
        SENS = 0xFE,
        META = 0xFF
    }

    public enum E_CONTROL : byte {
        BANK_MSB = 0,
        MODULATION = 1,
        DATA_MSB = 6,
        VOL = 7,
        PAN = 10,
        EXP = 11,
        FX_CTRL = 12,
        BANK_LSB = 32,
        DATA_LSB = 38,
        DAMPER = 64,
        RESONANCE = 71,
        RELEASE = 72,
        ATTACK = 73,
        CUTOFF = 74,
        VIB_RATE = 76,
        VIB_DELAY = 78,
        REVERB = 91,
        CHORUS = 93,
        DELAY = 94,
        RPN_LSB = 100,
        RPN_MSB = 101,
        ALL_RESET = 121
    }

    public enum E_META {
        SEQ_NUM = 0x00,
        TEXT = 0x01,
        COPYWRITAE = 0x02,
        TRACK_NAME = 0x03,
        INST_NAME = 0x04,
        LYRICS = 0x05,
        MARKER = 0x06,
        QUEUE = 0x07,
        PROG_NAME = 0x08,
        DEVICE_NAME = 0x09,
        CH_PREF = 0x20,
        PORT = 0x21,
        EOT = 0x2F,
        TEMPO = 0x51,
        SMPTE = 0x54,
        MEASURE = 0x58,
        KEY = 0x59,
        USER = 0x7F
    }

    public enum E_KEY {
        CF_MAJOR = 0xF900,
        GF_MAJOR = 0xFA00,
        DF_MAJOR = 0xFB00,
        AF_MAJOR = 0xFC00,
        EF_MAJOR = 0xFD00,
        BF_MAJOR = 0xFE00,
        F_MAJOR  = 0xFF00,
        C_MAJOR  = 0x0000,
        G_MAJOR  = 0x0100,
        D_MAJOR  = 0x0200,
        A_MAJOR  = 0x0300,
        E_MAJOR  = 0x0400,
        B_MAJOR  = 0x0500,
        FS_MAJOR = 0x0600,
        CS_MAJOR = 0x0700,

        AF_MINOR = 0xF901,
        EF_MINOR = 0xFA01,
        BF_MINOR = 0xFB01,
        F_MINOR  = 0xFC01,
        C_MINOR  = 0xFD01,
        G_MINOR  = 0xFE01,
        D_MINOR  = 0xFF01,
        A_MINOR  = 0x0001,
        E_MINOR  = 0x0101,
        B_MINOR  = 0x0201,
        FS_MINOR = 0x0301,
        CS_MINOR = 0x0401,
        GS_MINOR = 0x0501,
        DS_MINOR = 0x0601,
        AS_MINOR = 0x0701,

        INVALID = 0xFFFF
    }

    public struct Event {
        public static readonly Comparison<Event> Compare = new Comparison<Event>((a, b) => {
            var dTime = a.tick - b.tick;
            if (0 == dTime) {
                var aEv = (uint)a.Type;
                var bEv = (uint)b.Type;
                if (aEv < 0xA0) {
                    aEv += 0x200;
                    aEv |= (uint)a.Channel << 10;
                } else if (aEv < 0xF0) {
                    aEv += 0x100;
                    aEv |= (uint)a.Channel << 10;
                }
                if (bEv < 0xA0) {
                    bEv += 0x200;
                    bEv |= (uint)b.Channel << 10;
                } else if (bEv < 0xF0) {
                    bEv += 0x100;
                    bEv |= (uint)b.Channel << 10;
                }
                var dComp = (long)aEv - bEv;
                return 0 == dComp ? 0 : (0 < dComp ? 1 : -1);
            } else {
                return 0 < dTime ? 1 : -1;
            }
        });

        public int tick;
        public int track;
        public byte[] data;

        public byte Status { get { return data[0]; } }

        public E_STATUS Type {
            get {
                if (Status < 0xF0) {
                    return (E_STATUS)(Status & 0xF0);
                } else {
                    return (E_STATUS)Status;
                }
            }
        }

        public int Channel {
            get {
                if (0x80 <= Status && Status < 0xF0) {
                    return Status & 0xF;
                } else {
                    return -1;
                }
            }
        }

        public Meta Meta { get { return Meta.Instance(data); } }

        public Event(int tick, int track, int ch, E_STATUS type, params int[] value) {
            this.tick = tick;
            this.track = track;
            switch (value.Length) {
            case 1:
                data = new byte[] {
                    (byte)((byte)type | ch),
                    (byte)value[0]
                };
                break;
            case 2:
                data = new byte[] {
                    (byte)((byte)type | ch),
                    (byte)value[0],
                    (byte)value[1]
                };
                break;
            default:
                data = new byte[] {
                    (byte)((byte)type | ch)
                };
                break;
            }
        }

        public Event(int tick, int track, int ch, E_CONTROL type, params int[] value) {
            this.tick = tick;
            this.track = track;
            switch (value.Length) {
            case 1:
                data = new byte[] {
                    (byte)((byte)E_STATUS.CONTROL | ch),
                    (byte)type,
                    (byte)value[0]
                };
                break;
            default:
                data = new byte[] {
                    (byte)((byte)E_STATUS.CONTROL | ch),
                    (byte)type,
                    0
                };
                break;
            }
        }

        public Event(int tick, int track, Meta meta) {
            this.tick = tick;
            this.track = track;
            meta.Copy(out data);
        }

        public Event(int ch, E_STATUS type, params int[] value) {
            tick = 0;
            track = 0;
            switch (value.Length) {
            case 1:
                data = new byte[] {
                    (byte)((byte)type | ch),
                    (byte)value[0]
                };
                break;
            case 2:
                data = new byte[] {
                    (byte)((byte)type | ch),
                    (byte)value[0],
                    (byte)value[1]
                };
                break;
            default:
                data = new byte[] {
                    (byte)((byte)type | ch)
                };
                break;
            }
        }

        public Event(int ch, E_CONTROL type, params int[] value) {
            tick = 0;
            track = 0;
            switch (value.Length) {
            case 1:
                data = new byte[] {
                    (byte)((byte)E_STATUS.CONTROL | ch),
                    (byte)type,
                    (byte)value[0]
                };
                break;
            default:
                data = new byte[] {
                    (byte)((byte)E_STATUS.CONTROL | ch),
                    (byte)type,
                    0
                };
                break;
            }
        }

        public Event(Meta meta) {
            tick = 0;
            track = 0;
            meta.Copy(out data);
        }

        public Event(MemoryStream ms, int tick, int track, ref byte currentStatus) {
            this.tick = tick;
            this.track = track;

            var status = (byte)ms.ReadByte();
            if (status < 0x80) {
                ms.Seek(-1, SeekOrigin.Current);
                status = currentStatus;
            } else {
                currentStatus = status;
            }

            E_STATUS type;
            if (status < 0xF0) {
                type = (E_STATUS)(status & 0xF0);
            } else {
                type = (E_STATUS)status;
            }

            switch (type) {
            case E_STATUS.NOTE_ON:
            case E_STATUS.NOTE_OFF:
            case E_STATUS.KEY_PRESS:
            case E_STATUS.CONTROL:
            case E_STATUS.PITCH:
                data = new byte[] {
                    status,
                    (byte)ms.ReadByte(),
                    (byte)ms.ReadByte()
                };
                break;
            case E_STATUS.PROGRAM:
            case E_STATUS.CH_PRESS:
                data = new byte[] {
                    status,
                    (byte)ms.ReadByte()
                };
                break;
            case E_STATUS.SYSEX_BEGIN:
                var list = new List<byte>();
                list.Add(status);
                var temp = (byte)ms.ReadByte();
                while (temp != (byte)E_STATUS.SYSEX_END) {
                    list.Add(temp);
                    temp = (byte)ms.ReadByte();
                }
                list.Add(temp);
                data = list.ToArray();
                break;
            case E_STATUS.META:
                var metaType = (byte)ms.ReadByte();
                var dataLen = Utils.ReadDelta(ms);
                var delta = Utils.GetDeltaBytes(dataLen);
                data = new byte[2 + delta.Length + dataLen];
                data[0] = status;
                data[1] = metaType;
                Array.Copy(delta, 0, data, 2, delta.Length);
                ms.Read(data, 2 + delta.Length, dataLen);
                break;
            default:
                data = null;
                break;
            }
        }

        public void WriteMessage(MemoryStream ms) {
            ms.Write(data, 0, data.Length);
        }
    }

    public struct Mesure {
        public int numerator;
        public int denominator;

        public Mesure(uint data) {
            numerator = (int)(data >> 24) & 0xFF;
            denominator = (int)Math.Pow(2, (data >> 16) & 0xFF);
        }

        public uint ToInt() {
            var b4 = (uint)numerator << 24;
            var b3 = (uint)Math.Log(denominator, 2) << 16;
            var b2 = (uint)24 << 8;
            var b1 = (uint)8;
            return b4 | b3 | b2 | b1;
        }
    }

    public class Meta {
        byte[] data;

        public E_META Type { get { return (E_META)data[1]; } }

        public string String {
            get {
                switch(Type) {
                case E_META.TEXT:
                case E_META.COPYWRITAE:
                case E_META.TRACK_NAME:
                case E_META.INST_NAME:
                case E_META.LYRICS:
                case E_META.MARKER:
                case E_META.QUEUE:
                case E_META.PROG_NAME:
                case E_META.DEVICE_NAME:
                    int dataLen;
                    var begin = Utils.GetDelta(data, out dataLen, 2) + 2;
                    return Encoding.Default.GetString(data, begin, dataLen);
                default:
                    return null;
                }
            }
            set {
                switch (Type) {
                case E_META.TEXT:
                case E_META.COPYWRITAE:
                case E_META.TRACK_NAME:
                case E_META.INST_NAME:
                case E_META.LYRICS:
                case E_META.MARKER:
                case E_META.QUEUE:
                case E_META.PROG_NAME:
                case E_META.DEVICE_NAME:
                    var data = Encoding.Default.GetBytes(value);
                    var delta = Utils.GetDeltaBytes(data.Length);
                    var type = Type;
                    this.data = new byte[data.Length + delta.Length + 2];
                    this.data[0] = 0xFF;
                    this.data[1] = (byte)type;
                    Array.Copy(delta, 0, this.data, 2, delta.Length);
                    Array.Copy(data, 0, this.data, 2 + delta.Length, data.Length);
                    break;
                }
            }
        }

        public uint Int {
            get {
                switch (Type) {
                case E_META.SEQ_NUM:
                case E_META.CH_PREF:
                    return data[3];
                case E_META.TEMPO:
                    return Utils.GetUINT24(data, 3);
                case E_META.MEASURE:
                    return Utils.GetUINT32(data, 3);
                case E_META.KEY:
                    return Utils.GetUINT16(data, 3);
                default:
                    return 0xFFFFFFFF;
                }
            }
            set {
                switch (Type) {
                case E_META.SEQ_NUM:
                case E_META.CH_PREF:
                    data[3] = (byte)value;
                    break;
                case E_META.TEMPO:
                    Utils.SetUINT24(data, value, 3);
                    break;
                case E_META.MEASURE:
                    Utils.SetUINT32(data, value, 3);
                    break;
                case E_META.KEY:
                    Utils.SetUINT16(data, value, 3);
                    break;
                }
            }
        }

        Meta(byte[] data) {
            this.data = data;
        }

        public Meta(E_META type) {
            switch(type) {
            case E_META.TEXT:
            case E_META.COPYWRITAE:
            case E_META.TRACK_NAME:
            case E_META.INST_NAME:
            case E_META.LYRICS:
            case E_META.MARKER:
            case E_META.QUEUE:
            case E_META.PROG_NAME:
            case E_META.DEVICE_NAME:
                data = new byte[3];
                data[0] = 0xFF;
                data[1] = (byte)type;
                break;
            case E_META.SEQ_NUM:
            case E_META.CH_PREF:
                data = new byte[4];
                data[0] = 0xFF;
                data[1] = (byte)type;
                data[2] = 1;
                break;
            case E_META.TEMPO:
                data = new byte[6];
                data[0] = 0xFF;
                data[1] = (byte)type;
                data[2] = 3;
                data[3] = 0x07;
                data[4] = 0xA1;
                data[5] = 0x20;
                break;
            case E_META.MEASURE:
                data = new byte[7];
                data[0] = 0xFF;
                data[1] = (byte)type;
                data[2] = 4;
                data[3] = 4;
                data[4] = 2;
                data[5] = 24;
                data[6] = 8;
                break;
            case E_META.KEY:
                data = new byte[5];
                data[0] = 0xFF;
                data[1] = (byte)type;
                data[2] = 2;
                break;
            }
        }

        public void Copy(out byte[] data) {
            data = new byte[this.data.Length];
            Array.Copy(this.data, data, data.Length);
        }

        public static Meta Instance(byte[] data) {
            if (0xFF == data[0]) {
                return new Meta(data);
            } else {
                return null;
            }
        }
    }

    public class Track {
        public readonly int No;
        public List<Event> Events { get; private set; }

        public Track(int no) {
            No = no;
            Events = new List<Event>();
        }

        public Track(BinaryReader br, int no) {
            No = no;
            Events = new List<Event>();

            Utils.ReadUINT32(br);
            int size = (int)Utils.ReadUINT32(br);

            var ms = new MemoryStream(br.ReadBytes(size), false);
            int time = 0;
            byte currentStatus = 0;
            while (ms.Position < ms.Length) {
                var delta = Utils.ReadDelta(ms);
                time += delta;
                Events.Add(new Event(ms, time, no, ref currentStatus));
            }
        }

        public void Write(Stream str) {
            var temp = new MemoryStream();
            Utils.WriteUINT32(temp, 0x4D54726B);
            Utils.WriteUINT32(temp, 0);
            int currentTime = 0;
            foreach (var ev in Events) {
                Utils.WriteDelta(temp, ev.tick - currentTime);
                ev.WriteMessage(temp);
                currentTime = ev.tick;
            }
            temp.Seek(4, SeekOrigin.Begin);
            Utils.WriteUINT32(temp, (uint)(temp.Length - 8));
            temp.WriteTo(str);
        }
    }

    class Utils {
        public static byte[] GetDeltaBytes(int value) {
            if (0 < (value >> 21)) {
                return new byte[] {
                    (byte)(0x80 | ((value >> 21) & 0x7F)),
                    (byte)(0x80 | ((value >> 14) & 0x7F)),
                    (byte)(0x80 | ((value >> 7) & 0x7F)),
                    (byte)(value & 0x7F)
                };
            }
            if (0 < (value >> 14)) {
                return new byte[] {
                    (byte)(0x80 | ((value >> 14) & 0x7F)),
                    (byte)(0x80 | ((value >> 7) & 0x7F)),
                    (byte)(value & 0x7F)
                };
            }
            if (0 < (value >> 7)) {
                return new byte[] {
                    (byte)(0x80 | ((value >> 7) & 0x7F)),
                    (byte)(value & 0x7F)
                };
            }
            if (0 <= value) {
                return new byte[] { (byte)value };
            }
            return null;
        }

        public static int GetDelta(byte[] data, out int retVal, int pos = 0) {
            var begin = pos;
            var temp = data[pos];
            retVal = temp & 0x7F;

            while (0x7F < temp) {
                temp = data[++pos];
                retVal <<= 7;
                retVal |= temp & 0x7F;
            }

            return pos - begin;
        }

        public static uint GetUINT16(byte[] data, int pos = 0) {
            return (uint)((data[pos] << 8) | data[pos + 1]);
        }

        public static uint GetUINT24(byte[] data, int pos = 0) {
            return (uint)((data[pos] << 16) | (data[pos + 1] << 8) | data[pos + 2]);
        }

        public static uint GetUINT32(byte[] data, int pos = 0) {
            var b4 = (uint)data[pos] << 24;
            var b3 = (uint)data[pos + 1] << 16;
            var b2 = (uint)data[pos + 2] << 8;
            var b1 = (uint)data[pos + 3];
            return b4 | b3 | b2 | b1;
        }

        public static void SetUINT16(byte[] data, uint value, int pos = 0) {
            data[pos] = (byte)((value >> 8) & 0xFF);
            data[pos + 1] = (byte)(value & 0xFF);
        }

        public static void SetUINT24(byte[] data, uint value, int pos = 0) {
            data[pos] = (byte)((value >> 16) & 0xFF);
            data[pos + 1] = (byte)((value >> 8) & 0xFF);
            data[pos + 2] = (byte)(value & 0xFF);
        }

        public static void SetUINT32(byte[] data, uint value, int pos = 0) {
            data[pos] = (byte)((value >> 24) & 0xFF);
            data[pos + 1] = (byte)((value >> 16) & 0xFF);
            data[pos + 2] = (byte)((value >> 8) & 0xFF);
            data[pos + 3] = (byte)(value & 0xFF);
        }

        public static int ReadDelta(MemoryStream ms) {
            var temp = ms.ReadByte();
            var retVal = temp & 0x7F;

            while (0x7F < temp) {
                temp = ms.ReadByte();
                retVal <<= 7;
                retVal |= temp & 0x7F;
            }

            return retVal;
        }

        public static int ReadUINT16(BinaryReader br) {
            return (ushort)((br.ReadByte() << 8) | br.ReadByte());
        }

        public static uint ReadUINT32(BinaryReader br) {
            var b4 = (uint)br.ReadByte() << 24;
            var b3 = (uint)br.ReadByte() << 16;
            var b2 = (uint)br.ReadByte() << 8;
            var b1 = (uint)br.ReadByte();
            return b4 | b3 | b2 | b1;
        }

        public static void WriteDelta(Stream str, int value) {
            if (0 < (value >> 21)) {
                str.WriteByte((byte)(0x80 | ((value >> 21) & 0x7F)));
                str.WriteByte((byte)(0x80 | ((value >> 14) & 0x7F)));
                str.WriteByte((byte)(0x80 | ((value >> 7) & 0x7F)));
                str.WriteByte((byte)(value & 0x7F));
                return;
            }
            if (0 < (value >> 14)) {
                str.WriteByte((byte)(0x80 | ((value >> 14) & 0x7F)));
                str.WriteByte((byte)(0x80 | ((value >> 7) & 0x7F)));
                str.WriteByte((byte)(value & 0x7F));
                return;
            }
            if (0 < (value >> 7)) {
                str.WriteByte((byte)(0x80 | ((value >> 7) & 0x7F)));
                str.WriteByte((byte)(value & 0x7F));
                return;
            }
            if (0 <= value) {
                str.WriteByte((byte)value);
                return;
            }
            str.WriteByte(0);
            return;
        }

        public static void WriteUINT16(Stream str, int value) {
            str.WriteByte((byte)(value >> 8));
            str.WriteByte((byte)(value & 0xFF));
        }

        public static void WriteUINT32(Stream str, uint value) {
            str.WriteByte((byte)((value >> 24) & 0xFF));
            str.WriteByte((byte)((value >> 16) & 0xFF));
            str.WriteByte((byte)((value >> 8) & 0xFF));
            str.WriteByte((byte)(value & 0xFF));
        }
    }

    public class SMF {
        public enum E_FORMAT : ushort {
            FORMAT0 = 0x0000,
            FORMAT1 = 0x0001,
            FORMAT2 = 0x0002,
            INVALID = 0xFFFF
        }

        private struct Header {
            public readonly E_FORMAT Format;
            public int Tracks;
            public readonly int Ticks;

            public Header(E_FORMAT format, int tracks, int ticks) {
                Format = format;
                Tracks = tracks;
                Ticks = ticks;
            }

            public Header(BinaryReader br) {
                Utils.ReadUINT32(br);
                Utils.ReadUINT32(br);
                Format = (E_FORMAT)Utils.ReadUINT16(br);
                Tracks = Utils.ReadUINT16(br);
                Ticks = Utils.ReadUINT16(br);
                if (!Enum.IsDefined(typeof(E_FORMAT), Format)) {
                    Format = E_FORMAT.INVALID;
                }
            }

            public void Write(Stream str) {
                Utils.WriteUINT32(str, 0x4D546864);
                Utils.WriteUINT32(str, 6);
                Utils.WriteUINT16(str, (int)Format);
                Utils.WriteUINT16(str, Tracks);
                Utils.WriteUINT16(str, Ticks);
            }
        }

        private string mPath;
        private Header mHead;
        private List<Track> mTracks;

        public int Ticks { get { return mHead.Ticks; } }

        public Event[] EventList {
            get {
                var list = new List<Event>();
                foreach (var tr in mTracks) {
                    foreach (var ev in tr.Events) {
                        list.Add(ev);
                    }
                }
                list.Sort(Event.Compare);
                return list.ToArray();
            }
        }

        public int MaxTime {
            get {
                var list = EventList;
                return list[list.Length - 1].tick;
            }
        }

        public SMF(E_FORMAT format = E_FORMAT.FORMAT1, int ticks = 960) {
            mHead = new Header(format, 0, ticks);
            mTracks = new List<Track>();
        }

        public SMF(string filePath) {
            var fs = new FileStream(filePath, FileMode.Open);
            var br = new BinaryReader(fs);

            mPath = filePath;
            mHead = new Header(br);

            mTracks = new List<Track>();
            for (var i = 0; i < mHead.Tracks; ++i) {
                mTracks.Add(new Track(br, i));
            }

            br.Close();
        }

        public void Write(string path) {
            var str = new FileStream(path, FileMode.Create);
            mHead.Write(str);
            foreach (var tr in mTracks) {
                tr.Write(str);
            }
            str.Close();
            str.Dispose();
        }
    }
}
