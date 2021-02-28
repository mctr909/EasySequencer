using System;
using System.Runtime.InteropServices;

namespace Player {
    public enum E_NOTE_STATE : byte {
        FREE,
        RESERVED,
        PRESS,
        RELEASE,
        HOLD,
        PURGE
    };

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct INST_ID {
        public byte isDrum;
        public byte programNo;
        public byte bankMSB;
        public byte bankLSB;
    };

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct FILTER {
        private double cut;
        private double res;
        private double a00;
        private double b00;
        private double a01;
        private double b01;
        private double a10;
        private double b10;
        private double a11;
        private double b11;
    };

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct ENVELOPE {
        public double deltaA;
        public double deltaD;
        public double deltaR;
        public double levelR;
        public double levelT;
        public double levelS;
        public double levelF;
        public double hold;
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

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CHANNEL_PARAM {
        public bool Enable;
        public bool IsOsc;
        public INST_ID InstId;
        public IntPtr Name;
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

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct CHANNEL {
        private double amp;
        private double pitch;
        private double holdDelta;
        private double panLeft;
        private double panRight;
        private double cutoff;
        private double resonance;
        private double delaySend;
        private double delayTime;
        private double delayCross;
        private double chorusSend;
        private double chorusRate;
        private double chorusDepth;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct SAMPLER {
        public byte channelNum;
        public byte noteNum;
        public E_NOTE_STATE state;
        private bool unisonNum;
        private double velocity;
        private double delta;
        private double index;
        private double time;
        private double egAmp;
        private IntPtr envAmp;
        private IntPtr waveInfo;
    };

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct REGION {
        public byte keyLo;
        public byte keyHi;
        public byte velLo;
        public byte velHi;
        public WAVE_INFO waveInfo;
        public ENVELOPE env;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    unsafe public struct INST_LIST {
        public int instCount;
        public INST_REC** ppInst;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    unsafe public struct INST_REC {
        public INST_ID id;
        public int regionCount;
        public IntPtr pName;
        public IntPtr pCategory;
        public REGION **ppRegions;
    }
}
