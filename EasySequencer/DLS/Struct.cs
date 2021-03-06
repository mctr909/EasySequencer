﻿using System.Runtime.InteropServices;

namespace DLS {
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct MIDI_LOCALE {
        public byte bankLSB;
        public byte bankMSB;
        private byte reserve1;
        public byte bankFlags;
        public byte programNo;
        private byte reserve2;
        private byte reserve3;
        private byte reserve4;
    };

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct RANGE {
        public ushort low;
        public ushort high;
    };

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct CONNECTION {
        public SRC_TYPE source;
        public SRC_TYPE control;
        public DST_TYPE destination;
        public TRN_TYPE transform;
        public int scale;
    };

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct WAVE_LOOP {
        public uint size;
        public uint type;
        public uint start;
        public uint length;
    };

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct CK_VERS {
        public uint msb;
        public uint lsb;
    };

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct CK_COLH {
        public uint instruments;
    };

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct CK_INSH {
        public uint regions;
        public MIDI_LOCALE locale;
    };

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public struct CK_RGNH {
        public RANGE key;
        public RANGE velocity;
        public ushort options;
        public ushort keyGroup;
        //public ushort layer;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct CK_WLNK {
        public ushort options;
        public ushort phaseGroup;
        public uint channel;
        public uint tableIndex;
    };

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct CK_WSMP {
        public uint size;
        public ushort unityNote;
        public short fineTune;
        public int gainInt;
        public uint options;
        public uint loopCount;

        public double Gain {
            get { return System.Math.Pow(10.0, gainInt / (200 * 65536.0)); }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct CK_PTBL {
        public uint size;
        public uint count;
    };

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct CK_FMT {
        public ushort tag;
        public ushort channels;
        public uint sampleRate;
        public uint bytesPerSec;
        public ushort blockAlign;
        public ushort bits;
    };
}
