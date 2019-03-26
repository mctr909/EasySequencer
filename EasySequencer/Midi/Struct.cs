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
        public double bi;
        public double a0;
        public double b0;
        public double a1;
        public double b1;
        public double a2;
        public double b2;
        public double a3;
        public double b3;
        public double a4;
        public double b4;
        public double a5;
        public double b5;
        public double a6;
        public double b6;
        public double a7;
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

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    unsafe public struct CHANNEL {
        public double wave;
        public double waveL;
        public double waveR;

        public double pitch;
        public double holdDelta;

        public double panLeft;
        public double panRight;

        public double tarCutoff;
        public double tarResonance;

        public double tarAmp;
        public double curAmp;

        public FILTER eq;
        public DELAY delay;
        public CHORUS chorus;
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

        public double tarAmp;
        public double curAmp;

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
