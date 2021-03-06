﻿using System;
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

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct NOTE {
        public byte channelNum;
        public byte num;
        public E_NOTE_STATE state;
        private byte reserved;
        public double velocity;
        private IntPtr pChannel;
        private IntPtr ppSamplers;
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
