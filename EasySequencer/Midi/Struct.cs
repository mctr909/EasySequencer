using System;
using System.Runtime.InteropServices;

namespace MIDI {
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct INST_ID {
        public byte isDrum;
        public byte programNo;
        public byte bankMSB;
        public byte bankLSB;
    };

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct WAVE_LOOP {
        public uint start;
        public uint length;
        public bool enable;
        private byte reserved1;
        private byte reserved2;
        private byte reserved3;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
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
    };

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct DELAY {
        public double depth;
        public double rate;
        private IntPtr pTapL;
        private IntPtr pTapR;
        private Int32 writeIndex;
        private Int32 readIndex;
    };

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct CHORUS {
        public double depth;
        public double rate;
        private double lfoK;
        private IntPtr pPanL;
        private IntPtr pPanR;
        private IntPtr pLfoRe;
        private IntPtr pLfoIm;
    };

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct ENVELOPE {
        public double levelA;
        public double levelD;
        public double levelS;
        public double levelR;
        public double deltaA;
        public double deltaD;
        public double deltaR;
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

        public double delayDepth;
        public double delayTime;
        public double chorusDepth;
        public double chorusRate;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    unsafe public struct SAMPLER {
        public uint channelNo;
        public ushort noteNo;
        public bool onKey;
        public bool isActive;

        public uint pcmAddr;
        public uint pcmLength;

        public double gain;
        public double delta;

        public double index;
        public double time;

        public double velocity;
        public double amp;

        public WAVE_LOOP loop;
        public ENVELOPE envAmp;
        public ENVELOPE envEq;
        public FILTER eq;
    };

    public struct CONTROL {
        public byte vol;
        public byte exp;
        public byte pan;

        public byte rev;
        public byte cho;
        public byte del;

        public byte res;
        public byte cut;
        public byte atk;
        public byte rel;

        public byte vibRate;
        public byte vibDepth;
        public byte vibDelay;

        public byte bendRange;
        public byte hold;

        public byte nrpnMSB;
        public byte nrpnLSB;
        public byte rpnMSB;
        public byte rpnLSB;
    };

    public struct WAVE_INFO {
        public uint pcmAddr;
        public uint pcmLength;
        public double gain;
        public double delta;
        public byte unityNote;
        public WAVE_LOOP loop;
        public ENVELOPE envAmp;
    }
}
