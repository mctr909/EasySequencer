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

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct INST_ID {
        public byte isDrum;
        public byte bankMSB;
        public byte bankLSB;
        public byte progNum;
    };

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    unsafe public struct INST_LIST {
        public int count;
        public INST_INFO** ppData;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    unsafe public struct INST_INFO {
        public INST_ID id;
        uint layerIndex;
        uint layerCount;
        uint artIndex;
        public fixed char name[32];
        public fixed char category[32];
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
