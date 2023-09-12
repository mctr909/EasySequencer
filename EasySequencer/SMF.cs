using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SMF {
    public enum E_STATUS : byte {
        NOTE_OFF  = 0x80,
        NOTE_ON   = 0x90,
        KEY_PRESS = 0xA0,
        CONTROL   = 0xB0,
        PROGRAM   = 0xC0,
        CH_PRESS  = 0xD0,
        PITCH     = 0xE0,

        SYSEX_BEGIN = 0xF0,
        TIME_CODE   = 0xF1,
        SONG_POS    = 0xF2,
        SONG_NUM    = 0xF3,
        TUNE_REQ    = 0xF6,
        SYSEX_END   = 0xF7,
        CLOCK       = 0xF8,
        START       = 0xFA,
        CONTINUE    = 0xFB,
        STOP        = 0xFC,
        SENS        = 0xFE,
        META        = 0xFF
    }

    public enum E_CONTROL : byte {
        BANK_MSB = 0,
        BANK_LSB = 32,
        MODULATION = 1,
        PORTAMENTO_TIME = 5,
        VOLUME = 7,
        PAN = 10,
        EXPRESSION = 11,
        DAMPER = 64,
        PORTAMENTO = 65,

        RESONANCE = 71,
        RELEASE = 72,
        ATTACK = 73,
        CUTOFF = 74,

        VIB_RATE = 76,
        VIB_DEPTH = 77,
        VIB_DELAY = 78,

        REVERB = 91,
        CHORUS = 93,
        DELAY = 94,
        EFFECT_MSB = 12,
        EFFECT_LSB = 44,

        DATA_MSB = 6,
        DATA_LSB = 38,
        RPN_LSB = 100,
        RPN_MSB = 101,

        ALL_SOUND_OFF = 120,
        ALL_RESET = 121,
        ALL_NOTE_OFF = 123,

        DRUM = 254
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
            var dTime = a.Tick - b.Tick;
            if (0 == dTime) {
                var dTrack = a.Track - b.Track;
                if (0 == dTrack) {
                    var dCh = a.Channel - b.Channel;
                    if (0 == dCh) {
                        var aType = (int)a.Type;
                        if (aType < 0xA0) {
                            // 0x280 - 0x290
                            aType |= 0x0200;
                        } else if (aType < 0xF0) {
                            // 0x1A0 - 0x1E0
                            aType |= 0x0100;
                        } else {
                            // 0x0F0 - 0x0FF
                        }
                        var bType = (int)b.Type;
                        if (bType < 0xA0) {
                            // 0x280 - 0x290
                            bType |= 0x0200;
                        } else if (aType < 0xF0) {
                            // 0x1A0 - 0x1E0
                            bType |= 0x0100;
                        } else {
                            // 0x0F0 - 0x0FF
                        }
                        return aType - bType;
                    } else {
                        return dCh;
                    }
                } else {
                    return dTrack;
                }
            } else {
                return dTime;
            }
        });

        public int Tick;
        public int Track;

        public byte[] Data { get; private set; }

        public byte Status { get { return Data[0]; } }

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

        public Meta Meta {
            get {
                if (Type == E_STATUS.META) {
                    return new Meta(Data);
                } else {
                    return null;
                }
            }
        }

        public Event(int tick, int track, int ch, E_STATUS type, params int[] value) {
            Tick = tick;
            Track = track;
            switch (value.Length) {
            case 1:
                Data = new byte[] {
                    (byte)((byte)type | ch),
                    (byte)value[0]
                };
                break;
            case 2:
                Data = new byte[] {
                    (byte)((byte)type | ch),
                    (byte)value[0],
                    (byte)value[1]
                };
                break;
            default:
                Data = new byte[] {
                    (byte)((byte)type | ch)
                };
                break;
            }
        }

        public Event(int tick, int track, int ch, E_CONTROL type, params int[] value) {
            Tick = tick;
            Track = track;
            switch (value.Length) {
            case 1:
                Data = new byte[] {
                    (byte)((byte)E_STATUS.CONTROL | ch),
                    (byte)type,
                    (byte)value[0]
                };
                break;
            default:
                Data = new byte[] {
                    (byte)((byte)E_STATUS.CONTROL | ch),
                    (byte)type,
                    0
                };
                break;
            }
        }

        public Event(int tick, int track, Meta meta) {
            Tick = tick;
            Track = track;
            Data = meta.Data;
        }

        public Event(int ch, E_STATUS type, params int[] value) {
            Tick = 0;
            Track = 0;
            switch (value.Length) {
            case 1:
                Data = new byte[] {
                    (byte)((byte)type | ch),
                    (byte)value[0]
                };
                break;
            case 2:
                Data = new byte[] {
                    (byte)((byte)type | ch),
                    (byte)value[0],
                    (byte)value[1]
                };
                break;
            default:
                Data = new byte[] {
                    (byte)((byte)type | ch)
                };
                break;
            }
        }

        public Event(int ch, E_CONTROL type, params int[] value) {
            Tick = 0;
            Track = 0;
            switch (value.Length) {
            case 1:
                Data = new byte[] {
                    (byte)((byte)E_STATUS.CONTROL | ch),
                    (byte)type,
                    (byte)value[0]
                };
                break;
            default:
                Data = new byte[] {
                    (byte)((byte)E_STATUS.CONTROL | ch),
                    (byte)type,
                    0
                };
                break;
            }
        }

        public Event(Meta meta) {
            Tick = 0;
            Track = 0;
            Data = meta.Data;
        }

        public Event(MemoryStream ms, int tick, int track, ref byte currentStatus) {
            Tick = tick;
            Track = track;

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
                Data = new byte[] {
                    status,
                    (byte)ms.ReadByte(),
                    (byte)ms.ReadByte()
                };
                break;
            case E_STATUS.PROGRAM:
            case E_STATUS.CH_PRESS:
                Data = new byte[] {
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
                Data = list.ToArray();
                break;
            case E_STATUS.META:
                var metaType = (byte)ms.ReadByte();
                var dataLen = Utils.ReadDelta(ms);
                var delta = Utils.GetDeltaBytes(dataLen);
                Data = new byte[2 + delta.Length + dataLen];
                Data[0] = status;
                Data[1] = metaType;
                Array.Copy(delta, 0, Data, 2, delta.Length);
                ms.Read(Data, 2 + delta.Length, dataLen);
                break;
            default:
                Data = null;
                break;
            }

            if (type == E_STATUS.NOTE_ON && Data[2] == 0) {
                Data[0] = (byte)((byte)E_STATUS.NOTE_OFF | (Data[0] & 0x0F));
            }
        }

        public void WriteMessage(MemoryStream ms) {
            ms.Write(Data, 0, Data.Length);
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
        public byte[] Data { get; private set; }

        public E_META Type { get { return (E_META)Data[1]; } }

        public string String {
            get {
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
                    int dataLen;
                    var begin = Utils.GetDelta(Data, out dataLen, 2) + 3;
                    return Encoding.Default.GetString(Data, begin, dataLen);
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
                    Data = new byte[data.Length + delta.Length + 2];
                    Data[0] = 0xFF;
                    Data[1] = (byte)type;
                    Array.Copy(delta, 0, Data, 2, delta.Length);
                    Array.Copy(data, 0, Data, 2 + delta.Length, data.Length);
                    break;
                }
            }
        }

        public uint UInt {
            get {
                switch (Type) {
                case E_META.SEQ_NUM:
                case E_META.CH_PREF:
                case E_META.PORT:
                    return Data[3];
                case E_META.TEMPO:
                    return Utils.GetUINT24(Data, 3);
                case E_META.MEASURE:
                    return Utils.GetUINT32(Data, 3);
                case E_META.KEY:
                    return Utils.GetUINT16(Data, 3);
                default:
                    return 0xFFFFFFFF;
                }
            }
            set {
                switch (Type) {
                case E_META.SEQ_NUM:
                case E_META.CH_PREF:
                case E_META.PORT:
                    Data[3] = (byte)value;
                    break;
                case E_META.TEMPO:
                    Utils.SetUINT24(Data, value, 3);
                    break;
                case E_META.MEASURE:
                    Utils.SetUINT32(Data, value, 3);
                    break;
                case E_META.KEY:
                    Utils.SetUINT16(Data, value, 3);
                    break;
                }
            }
        }

        public Meta(byte[] data) {
            Data = data;
        }

        public Meta(E_META type) {
            switch (type) {
            case E_META.TEXT:
            case E_META.COPYWRITAE:
            case E_META.TRACK_NAME:
            case E_META.INST_NAME:
            case E_META.LYRICS:
            case E_META.MARKER:
            case E_META.QUEUE:
            case E_META.PROG_NAME:
            case E_META.DEVICE_NAME:
                Data = new byte[] { 0xFF, (byte)type, 0 };
                break;
            case E_META.SEQ_NUM:
            case E_META.CH_PREF:
            case E_META.PORT:
                Data = new byte[] { 0xFF, (byte)type, 1, 0 };
                break;
            case E_META.TEMPO:
                Data = new byte[] { 0xFF, (byte)type, 3, 0x07, 0xA1, 0x20 };
                break;
            case E_META.MEASURE:
                Data = new byte[] { 0xFF, (byte)type, 4, 4, 2, 24, 8 };
                break;
            case E_META.KEY:
                Data = new byte[] { 0xFF, (byte)type, 2, 0, 0 };
                break;
            }
        }
    }

    internal class Utils {
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

    public class File {
        public enum E_FORMAT : ushort {
            FORMAT0 = 0x0000,
            FORMAT1 = 0x0001,
            FORMAT2 = 0x0002,
            INVALID = 0xFFFF
        }

        private struct Header {
            public E_FORMAT Format;
            public int Tracks;
            public int Ticks;

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
        
        public class Track {
            public bool Enable = true;
            public byte Port = 0;
            public string Name = "";
            public List<Event> Events = new List<Event>();
        }

        private Header mHead;
        
        public List<Track> Tracks = new List<Track>();

        public Event[] EventList {
            get {
                var list = new List<Event>();
                foreach (var tr in Tracks) {
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
                return list[list.Length - 1].Tick;
            }
        }

        public File(E_FORMAT format = E_FORMAT.FORMAT1, int ticks = 960) {
            mHead = new Header(format, 0, ticks);
        }

        public File(string filePath) {
            var fs = new FileStream(filePath, FileMode.Open);
            var br = new BinaryReader(fs);

            mHead = new Header(br);

            for (var i = 0; i < mHead.Tracks; ++i) {
                var tr = new Track();
                Tracks.Add(tr);
                tr.Events = LoadTrack(br, i, mHead.Ticks);
            }

            mHead.Ticks = 960;
            mHead.Format = E_FORMAT.FORMAT1;

            br.Close();
        }

        public void Write(string path) {
            var str = new FileStream(path, FileMode.Create);
            mHead.Write(str);
            foreach (var tr in Tracks) {
                WriteTrack(str, tr.Events);
            }
            str.Close();
            str.Dispose();
        }

        public void AddEvent(Event ev) {
            var trackNum = ev.Track;
            if (Tracks.Count <= trackNum) {
                Tracks.Add(new Track());
            }
            if (E_STATUS.META == ev.Type && E_META.PORT == ev.Meta.Type) {
                switch (ev.Meta.Type) {
                case E_META.PORT:
                    Tracks[trackNum].Port = (byte)ev.Meta.UInt;
                    break;
                case E_META.TRACK_NAME:
                    Tracks[trackNum].Name = ev.Meta.String;
                    break;
                }
            }
            if (ev.Type == E_STATUS.NOTE_OFF || ev.Type == E_STATUS.NOTE_ON) {
            } else {
                Tracks[trackNum].Events.Add(ev);
            }
            Tracks[trackNum].Events.Sort(Event.Compare);
        }

        private List<Event> LoadTrack(BinaryReader br, int trackNum, int baseTick) {
            Utils.ReadUINT32(br);
            int size = (int)Utils.ReadUINT32(br);

            var ms = new MemoryStream(br.ReadBytes(size), false);
            int time = 0;
            byte currentStatus = 0;
            var events = new List<Event>();
            while (ms.Position < ms.Length) {
                var delta = Utils.ReadDelta(ms) * 960 / baseTick;
                time += delta;
                var ev = new Event(ms, time, trackNum, ref currentStatus);
                events.Add(ev);
                if (E_STATUS.META == ev.Type) {
                    switch(ev.Meta.Type) {
                    case E_META.PORT:
                        Tracks[trackNum].Port = (byte)ev.Meta.UInt;
                        break;
                    case E_META.TRACK_NAME:
                        Tracks[trackNum].Name = ev.Meta.String;
                        break;
                    }
                }
            }
            return events;
        }

        private void WriteTrack(Stream str, List<Event> eventList) {
            var temp = new MemoryStream();
            Utils.WriteUINT32(temp, 0x4D54726B);
            Utils.WriteUINT32(temp, 0);
            int currentTime = 0;
            foreach (var ev in eventList) {
                Utils.WriteDelta(temp, ev.Tick - currentTime);
                ev.WriteMessage(temp);
                currentTime = ev.Tick;
            }
            temp.Seek(4, SeekOrigin.Begin);
            Utils.WriteUINT32(temp, (uint)(temp.Length - 8));
            temp.WriteTo(str);
        }
    }
}
