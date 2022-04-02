using System;
using System.Runtime.InteropServices;

namespace Instruments {
    public enum E_KEY_STATE : byte {
        FREE,
        PRESS,
        HOLD
    };

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct ENV_AMP {
        public double attack;
        public double hold;
        public double decay;
        public double sustain;
        public double release;
    };

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct ENV_FILTER {
        public double attack;
        public double hold;
        public double decay;
        public double sustain;
        public double release;
        public double rise;
        public double top;
        public double fall;
    };

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct ENV_PITCH {
        public double attack;
        public double release;
        public double rise;
        public double fall;
    };

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct WAVE_INFO {
        public uint waveOfs;
        public uint loopBegin;
        public uint loopLength;
        public bool loopEnable;
        public byte unityNote;
        public ushort reserved;
        public double gain;
        public double delta;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct INST_ID {
        public byte isDrum;
        public byte programNo;
        public byte bankMSB;
        public byte bankLSB;
    };

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct REGION {
        public byte keyLo;
        public byte keyHi;
        public byte velLo;
        public byte velHi;
        public WAVE_INFO waveInfo;
        public ENV_AMP envAmp;
        public ENV_FILTER envFilter;
        public ENV_PITCH envPitch;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    unsafe public struct INST_INFO {
        public INST_ID id;
        public int regionCount;
        public IntPtr pName;
        public IntPtr pCategory;
        public REGION** ppRegions;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    unsafe public struct INST_LIST {
        public int instCount;
        public INST_INFO** ppInst;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe public struct CHANNEL_PARAM {
        public fixed int KeyBoard[128];
        public INST_ID InstId;
        public IntPtr Name;
        public bool Enable;
        public byte Vol;
        public byte Exp;
        public byte Pan;
        public byte Rev;
        public byte Del;
        public byte Cho;
        public byte Mod;
        public byte Hld;
        public byte Fc;
        public byte Fq;
        public byte Atk;
        public byte Rel;
        public byte VibRate;
        public byte VibDepth;
        public byte VibDelay;
        public byte BendRange;
        public int Pitch;
    }
}
