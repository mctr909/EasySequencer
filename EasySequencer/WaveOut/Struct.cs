using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace WaveOut {
    public enum E_KEY_STATE : byte {
        STANDBY,
        PURGE,
        RELEASE,
        HOLD,
        PRESS
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
        public double cut;
        public double res;
        public double a00;  //  16
        public double b00;  //  24
        public double a01;  //  32
        public double b01;  //  40
        public double a10;  //  48
        public double b10;  //  56
        public double a11;  //  64
        public double b11;  //  72
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
    unsafe public struct CHANNEL {
        public double amp;
        public double pitch;
        public double holdDelta;
        public double panLeft;
        public double panRight;
        public double cutoff;
        public double resonance;
        public double delaySend;
        public double delayTime;
        public double delayCross;
        public double chorusSend;
        public double chorusRate;
        public double chorusDepth;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    unsafe public struct SAMPLER {
        public ushort      channelNum;
        public byte        noteNum;
        public E_KEY_STATE state;
        public double      velocity;
        public double      index;
        public double      time;
        public double      egAmp;
        public ENVELOPE    envAmp;
        public WAVE_INFO   waveInfo;
    };

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    unsafe public struct REGION {
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
        public byte* pName;
        public byte* pCategory;
        public REGION **ppRegions;
    }

    public struct INST_INFO {
        public string name;
        public string catgory;
        public List<REGION> regions;
    }
}
