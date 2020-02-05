using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace WaveOut {
    public enum E_KEY_STATE : byte {
        WAIT,
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

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct WAVE_LOOP {
        public uint begin;
        public uint length;
        public bool enable;
        private byte reserved1;
        private byte reserved2;
        private byte reserved3;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct FILTER {
        public double cutoff;
        public double resonance;
        public double a0;  //  16
        public double b0;  //  24
        public double a1;  //  32
        public double b1;  //  40
        public double a2;  //  48
        public double b2;  //  56
        public double a3;  //  64
        public double b3;  //  72
        public double a4;
        public double b4;
        public double a5;
        public double b5;
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
    unsafe public struct CHANNEL_PARAM {
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
        public ushort channelNo;
        public byte noteNo;
        public E_KEY_STATE keyState;
        public uint dataOfs;

        public double velocity;
        public double gain;
        public double delta;
        public double index;
        public double time;
        public double egAmp;
        public double egPitch;

        public WAVE_LOOP loop;
        public ENVELOPE envAmp;
        public ENVELOPE envPitch;
        public ENVELOPE envEq;
        public FILTER filter;
    };

    public struct WAVE_INFO {
        public byte presetKeyLow;
        public byte presetKeyHigh;
        public byte presetVelLow;
        public byte presetVelHigh;
        public byte instKeyLow;
        public byte instKeyHigh;
        public byte instVelLow;
        public byte instVelHigh;
        public uint dataOfs;
        public double gain;
        public double delta;
        public byte unityNote;
        public WAVE_LOOP loop;
        public ENVELOPE env;
    }

    public struct INST_INFO {
        public string name;
        public string catgory;
        public List<WAVE_INFO> waveList;
    }
}
