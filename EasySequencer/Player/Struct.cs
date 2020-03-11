using System;
using System.Runtime.InteropServices;

namespace Player {
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
        public E_KEY_STATE state;
        private bool isOsc;
        private double velocity;
        private double time;
        private double index;
        private double egAmp;
        private double egPitch;
        private ENVELOPE envAmp;
        private ENVELOPE envPitch;
        private ENVELOPE envCutoff;
        private FILTER filter;
        private WAVE_INFO waveInfo;
        private byte waveForm0;
        private byte waveForm1;
        private byte waveForm2;
        private byte waveForm3;
        private byte waveForm4;
        private byte waveForm5;
        private byte waveForm6;
        private byte waveForm7;
        private double gain0;
        private double gain1;
        private double gain2;
        private double gain3;
        private double gain4;
        private double gain5;
        private double gain6;
        private double gain7;
        private double pitch0;
        private double pitch1;
        private double pitch2;
        private double pitch3;
        private double pitch4;
        private double pitch5;
        private double pitch6;
        private double pitch7;
        private double param0;
        private double param1;
        private double param2;
        private double param3;
        private double param4;
        private double param5;
        private double param6;
        private double param7;
        private double value0;
        private double value1;
        private double value2;
        private double value3;
        private double value4;
        private double value5;
        private double value6;
        private double value7;
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
