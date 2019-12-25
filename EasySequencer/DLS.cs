using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using WaveOut;

namespace DLS {
    #region enum
    public enum SRC_TYPE : ushort {
        // MODULATOR SOURCES
        NONE = 0x0000,
        LFO = 0x0001,
        KEY_ON_VELOCITY = 0x0002,
        KEY_NUMBER = 0x0003,
        EG1 = 0x0004,
        EG2 = 0x0005,
        PITCH_WHEEL = 0x0006,
        POLY_PRESSURE = 0x0007,
        CHANNEL_PRESSURE = 0x0008,
        VIBRATO = 0x0009,

        // MIDI CONTROLLER SOURCES
        CC1 = 0x0081,
        CC7 = 0x0087,
        CC10 = 0x008A,
        CC11 = 0x008B,
        CC91 = 0x00DB,
        CC93 = 0x00DD,

        // REGISTERED PARAMETER NUMBERS
        RPN0 = 0x0100,
        RPN1 = 0x0101,
        RPN2 = 0x0102
    };

    public enum DST_TYPE : ushort {
        // GENERIC DESTINATIONS
        NONE = 0x0000,
        ATTENUATION = 0x0001,
        RESERVED = 0x0002,
        PITCH = 0x0003,
        PAN = 0x0004,
        KEY_NUMBER = 0x0005,

        // CHANNEL OUTPUT DESTINATIONS
        LEFT = 0x0010,
        RIGHT = 0x0011,
        CENTER = 0x0012,
        LFET_CHANNEL = 0x0013,
        LEFT_REAR = 0x0014,
        RIGHT_REAR = 0x0015,
        CHORUS = 0x0080,
        REVERB = 0x0081,

        // MODULATOR LFO DESTINATIONS
        LFO_FREQUENCY = 0x0104,
        LFO_START_DELAY = 0x0105,

        // VIBRATO LFO DESTINATIONS
        VIB_FREQUENCY = 0x0114,
        VIB_START_DELAY = 0x0115,

        // EG1 DESTINATIONS
        EG1_ATTACK_TIME = 0x0206,
        EG1_DECAY_TIME = 0x0207,
        EG1_RESERVED = 0x0208,
        EG1_RELEASE_TIME = 0x0209,
        EG1_SUSTAIN_LEVEL = 0x020A,
        EG1_DELAY_TIME = 0x020B,
        EG1_HOLD_TIME = 0x020C,
        EG1_SHUTDOWN_TIME = 0x020D,

        // EG2 DESTINATIONS
        EG2_ATTACK_TIME = 0x030A,
        EG2_DECAY_TIME = 0x030B,
        EG2_RESERVED = 0x030C,
        EG2_RELEASE_TIME = 0x030D,
        EG2_SUSTAIN_LEVEL = 0x030E,
        EG2_DELAY_TIME = 0x030F,
        EG2_HOLD_TIME = 0x0310,

        // FILTER DESTINATIONS
        FILTER_CUTOFF = 0x0500,
        FILTER_Q = 0x0501
    };

    public enum TRN_TYPE : ushort {
        NONE = 0x0000,
        CONCAVE = 0x0001,
        CONVEX = 0x0002,
        SWITCH = 0x0003
    };
    #endregion

    #region struct
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
    #endregion

    public class DLS : RiffChunk {
        private IntPtr mDlsPtr;

        public LINS Instruments = null;
        public WVPL WavePool = null;

        public CK_COLH Colh { get; private set; }
        public CK_PTBL Ptbl { get; private set; }
        public CK_VERS Version { get; private set; }

        public DLS(IntPtr dlsPtr, uint dlsSize) : base(dlsPtr, dlsSize) {
            mDlsPtr = dlsPtr;
        }

        public Dictionary<INST_ID, INST_INFO> GetInstList() {
            var instList = new Dictionary<INST_ID, INST_INFO>();
            foreach (var inst in Instruments.List) {
                var envAmp = new ENVELOPE();
                if (null != inst.Articulations) {
                    envAmp.deltaA = 1000.0 * Const.EnvelopeSpeed * Const.DeltaTime; // 1msec
                    envAmp.deltaD = 1000.0 * Const.EnvelopeSpeed * Const.DeltaTime; // 1msec
                    envAmp.deltaR = 1000.0 * Const.EnvelopeSpeed * Const.DeltaTime; // 1msec
                    envAmp.levelS = 1.0;
                    envAmp.hold = 0.0;
                    var holdTime = 0.0;
                    foreach (var conn in inst.Articulations.Art.List) {
                        if (SRC_TYPE.NONE != conn.source) {
                            continue;
                        }
                        switch (conn.destination) {
                        case DST_TYPE.EG1_ATTACK_TIME:
                            envAmp.deltaA = Const.EnvelopeSpeed * Const.DeltaTime / ART.GetValue(conn);
                            holdTime += ART.GetValue(conn);
                            break;
                        case DST_TYPE.EG1_DECAY_TIME:
                            envAmp.deltaD = Const.EnvelopeSpeed * Const.DeltaTime / ART.GetValue(conn);
                            break;
                        case DST_TYPE.EG1_RELEASE_TIME:
                            envAmp.deltaR = Const.EnvelopeSpeed * Const.DeltaTime / ART.GetValue(conn);
                            break;
                        case DST_TYPE.EG1_SUSTAIN_LEVEL:
                            envAmp.levelS = (0.0 == ART.GetValue(conn)) ? 1.0 : (ART.GetValue(conn) * 0.01);
                            break;
                        case DST_TYPE.EG1_HOLD_TIME:
                            holdTime += ART.GetValue(conn);
                            break;
                        }
                    }
                    envAmp.hold += holdTime;
                    if (envAmp.hold < Const.DeltaTime) {
                        envAmp.hold = Const.DeltaTime;
                    }
                }

                var waves = new WAVE_INFO[128, 128];
                for (var velo = 0; velo < 128; velo++) {
                    for (var noteNo = 0; noteNo < 128; ++noteNo) {
                        var wave = new WAVE_INFO();
                        wave.buffOfs = uint.MaxValue;
                        RGN_ region = null;
                        foreach (var rgn in inst.Regions.List) {
                            if (rgn.Header.key.low <= noteNo && noteNo <= rgn.Header.key.high && 
                                rgn.Header.velocity.low <= velo && velo <= rgn.Header.velocity.high) {
                                region = rgn;
                                break;
                            }
                        }
                        if (null == region) {
                            continue;
                        }
                        if (null != region.Articulations) {
                            envAmp.deltaA = 1000.0 * Const.EnvelopeSpeed * Const.DeltaTime; // 1msec
                            envAmp.deltaD = 1000.0 * Const.EnvelopeSpeed * Const.DeltaTime; // 1msec
                            envAmp.deltaR = 1000.0 * Const.EnvelopeSpeed * Const.DeltaTime; // 1msec
                            envAmp.levelS = 1.0;
                            envAmp.hold = 0.0;
                            var holdTime = 0.0;
                            foreach (var conn in region.Articulations.Art.List) {
                                if (SRC_TYPE.NONE != conn.source)
                                    continue;
                                switch (conn.destination) {
                                case DST_TYPE.EG1_ATTACK_TIME:
                                    envAmp.deltaA = Const.EnvelopeSpeed * Const.DeltaTime / ART.GetValue(conn);
                                    holdTime += ART.GetValue(conn);
                                    break;
                                case DST_TYPE.EG1_DECAY_TIME:
                                    envAmp.deltaD = Const.EnvelopeSpeed * Const.DeltaTime / ART.GetValue(conn);
                                    break;
                                case DST_TYPE.EG1_RELEASE_TIME:
                                    envAmp.deltaR = Const.EnvelopeSpeed * Const.DeltaTime / ART.GetValue(conn);
                                    break;
                                case DST_TYPE.EG1_SUSTAIN_LEVEL:
                                    envAmp.levelS = (0.0 == ART.GetValue(conn)) ? 1.0 : (ART.GetValue(conn) * 0.01);
                                    break;
                                case DST_TYPE.EG1_HOLD_TIME:
                                    holdTime += ART.GetValue(conn);
                                    break;
                                }
                            }
                            envAmp.hold += holdTime;
                            if (envAmp.hold < Const.DeltaTime) {
                                envAmp.hold = Const.DeltaTime;
                            }
                        }
                        wave.envAmp = envAmp;
                        var wv = WavePool.List[(int)region.WaveLink.tableIndex];
                        wave.buffOfs = wv.Addr - (uint)mDlsPtr.ToInt64();
                        wave.samples = wv.Size / wv.Format.blockAlign;
                        if (region.HasSampler) {
                            wave.unityNote = (byte)region.Sampler.unityNote;
                            wave.delta
                                = Math.Pow(2.0, region.Sampler.fineTune / 1200.0)
                                * wv.Format.sampleRate / Const.SampleRate;
                            ;
                            wave.gain = region.Sampler.Gain / 32768.0;
                            if (0 < region.Sampler.loopCount) {
                                wave.loop.begin = region.Loops[0].start;
                                wave.loop.length = region.Loops[0].length;
                                wave.loop.enable = true;
                            } else if (0 < wv.Sampler.loopCount) {
                                wave.loop.begin = wv.Loops[0].start;
                                wave.loop.length = wv.Loops[0].length;
                                wave.loop.enable = true;
                            } else {
                                wave.loop.begin = 0;
                                wave.loop.length = wave.samples;
                                wave.loop.enable = false;
                                wave.envAmp.deltaR = Const.DeltaTime * wave.delta / wave.samples;
                            }
                        } else {
                            wave.unityNote = (byte)wv.Sampler.unityNote;
                            wave.delta
                                = Math.Pow(2.0, wv.Sampler.fineTune / 1200.0)
                                * wv.Format.sampleRate / Const.SampleRate;
                            ;
                            wave.gain = wv.Sampler.Gain / 32768.0;
                            if (0 < wv.Sampler.loopCount) {
                                wave.loop.begin = wv.Loops[0].start;
                                wave.loop.length = wv.Loops[0].length;
                                wave.loop.enable = true;
                            } else {
                                wave.loop.begin = 0;
                                wave.loop.length = wave.samples;
                                wave.loop.enable = false;
                            }
                        }
                        var diffNote = noteNo - wave.unityNote;
                        if (diffNote < 0) {
                            wave.delta /= Const.SemiTone[-diffNote];
                        } else {
                            wave.delta *= Const.SemiTone[diffNote];
                        }
                        waves[velo, noteNo] = wave;
                    }
                }
                var id = new INST_ID();
                id.isDrum = (byte)(inst.Header.locale.bankFlags == 0x80 ? 1 : 0);
                id.programNo = inst.Header.locale.programNo;
                id.bankMSB = inst.Header.locale.bankMSB;
                id.bankLSB = inst.Header.locale.bankLSB;
                var instInfo = new INST_INFO();
                instInfo.catgory = inst.Category;
                instInfo.name = inst.Name;
                instInfo.waves = waves;
                instList.Add(id, instInfo);
            }
            return instList;
        }

        protected override void LoadChunk(IntPtr ptr, string type, uint size) {
            switch (type) {
            case "colh":
                Colh = Marshal.PtrToStructure<CK_COLH>(ptr);
                break;
            case "vers":
                Version = Marshal.PtrToStructure<CK_VERS>(ptr);
                break;
            case "msyn":
                break;
            case "ptbl":
                Ptbl = Marshal.PtrToStructure<CK_PTBL>(ptr);
                break;
            case "DLID":
                break;
            default:
                throw new Exception("[File]Unknown ChunkType");
            }
        }

        protected override void LoadList(IntPtr ptr, string type, uint size) {
            switch (type) {
            case "lins":
                Instruments = new LINS(ptr, size);
                break;
            case "wvpl":
                WavePool = new WVPL(ptr, size);
                break;
            default:
                throw new Exception("[File]Unknown ListType");
            }
        }
    }

    public class LINS : RiffChunk {
        public HashSet<INS_> List = new HashSet<INS_>();

        public LINS(IntPtr ptr, uint size) : base(ptr, size) { }

        protected override void LoadList(IntPtr ptr, string type, uint size) {
            switch (type) {
            case "ins ":
                List.Add(new INS_(ptr, size));
                break;
            default:
                throw new Exception("[LINS]Unknown ListType");
            }
        }
    }

    public class INS_ : RiffChunk {
        public CK_INSH Header;
        public LRGN Regions = null;
        public LART Articulations = null;
        public string Name { get; private set; } = "";
        public string Category { get; private set; } = "";

        public INS_(IntPtr ptr, uint size) : base(ptr, size) { }

        protected override void LoadInfo(IntPtr ptr, string type, uint size) {
            switch (type) {
            case "INAM":
                Name = Marshal.PtrToStringAnsi(ptr).Trim();
                break;
            case "IKEY":
                Category = Marshal.PtrToStringAnsi(ptr).Trim();
                break;
            }
        }

        protected override void LoadChunk(IntPtr ptr, string type, uint size) {
            switch (type) {
            case "insh":
                Header = Marshal.PtrToStructure<CK_INSH>(ptr);
                break;
            default:
                throw new Exception("[INS_]Unknown ChunkType");
            }
        }

        protected override void LoadList(IntPtr ptr, string type, uint size) {
            switch (type) {
            case "lrgn":
                Regions = new LRGN(ptr, size);
                break;
            case "lart":
            case "lar2":
                Articulations = new LART(ptr, size);
                break;
            default:
                throw new Exception("[INS_]Unknown ListType");
            }
        }
    }

    public class LRGN : RiffChunk {
        public HashSet<RGN_> List = new HashSet<RGN_>();

        public LRGN(IntPtr ptr, uint size) : base(ptr, size) { }

        protected override void LoadList(IntPtr ptr, string type, uint size) {
            switch (type) {
            case "rgn ":
                List.Add(new RGN_(ptr, size));
                break;
            default:
                throw new Exception("[LRGN]Unknown ListType");
            }
        }
    }

    public class RGN_ : RiffChunk {
        public CK_RGNH Header { get; private set; }
        public CK_WLNK WaveLink { get; private set; }
        public CK_WSMP Sampler { get; private set; }
        public WAVE_LOOP[] Loops { get; private set; }
        public LART Articulations = null;

        public bool HasSampler { get; private set; }
        public bool HasLoop { get; private set; }

        public RGN_(IntPtr ptr, uint size) : base(ptr, size) { }

        protected override void LoadChunk(IntPtr ptr, string type, uint size) {
            switch (type) {
            case "rgnh":
                Header = Marshal.PtrToStructure<CK_RGNH>(ptr);
                break;
            case "wlnk":
                WaveLink = Marshal.PtrToStructure<CK_WLNK>(ptr);
                break;
            case "wsmp": {
                    Sampler = Marshal.PtrToStructure<CK_WSMP>(ptr);
                    ptr += Marshal.SizeOf<CK_WSMP>();
                    Loops = new WAVE_LOOP[Sampler.loopCount];
                    for (uint i = 0; i < Sampler.loopCount; ++i) {
                        Loops[i] = Marshal.PtrToStructure<WAVE_LOOP>(ptr);
                        ptr += Marshal.SizeOf<WAVE_LOOP>();
                    }
                    HasSampler = true;
                    HasLoop = 0 < Sampler.loopCount;
                }
                break;
            default:
                throw new Exception("[RGN_]Unknown ChunkType");
            }
        }

        protected override void LoadList(IntPtr ptr, string type, uint size) {
            switch (type) {
            case "lart":
            case "lar2":
                Articulations = new LART(ptr, size);
                break;
            default:
                throw new Exception("[RGN_]Unknown ListType");
            }
        }
    }

    public class LART : RiffChunk {
        public ART Art = null;

        public LART(IntPtr ptr, uint size) : base(ptr, size) { }

        protected override void LoadChunk(IntPtr ptr, string type, uint size) {
            switch (type) {
            case "art1":
            case "art2":
                Art = new ART(ptr);
                break;
            default:
                throw new Exception("[LART]Unknown ChunkType");
            }
        }
    }

    public class ART {
        public CONNECTION[] List { get; private set; }

        public ART(IntPtr ptr) {
            ptr += 4;
            var connCount = Marshal.PtrToStructure<uint>(ptr);
            ptr += 4;
            List = new CONNECTION[connCount];
            for (uint i = 0; i < connCount; ++i) {
                List[i] = Marshal.PtrToStructure<CONNECTION>(ptr);
                ptr += Marshal.SizeOf<CONNECTION>();
            }
        }

        public static double GetValue(CONNECTION conn) {
            switch (conn.destination) {
            case DST_TYPE.ATTENUATION:
            case DST_TYPE.FILTER_Q:
                return Math.Pow(10.0, conn.scale / (200 * 65536.0));

            case DST_TYPE.PAN:
                return (conn.scale / 655360.0) - 0.5;

            case DST_TYPE.LFO_START_DELAY:
            case DST_TYPE.VIB_START_DELAY:
            case DST_TYPE.EG1_ATTACK_TIME:
            case DST_TYPE.EG1_DECAY_TIME:
            case DST_TYPE.EG1_RELEASE_TIME:
            case DST_TYPE.EG1_DELAY_TIME:
            case DST_TYPE.EG1_HOLD_TIME:
            case DST_TYPE.EG1_SHUTDOWN_TIME:
            case DST_TYPE.EG2_ATTACK_TIME:
            case DST_TYPE.EG2_DECAY_TIME:
            case DST_TYPE.EG2_RELEASE_TIME:
            case DST_TYPE.EG2_DELAY_TIME:
            case DST_TYPE.EG2_HOLD_TIME:
                return (conn.scale == int.MinValue) ? 1.0 : Math.Pow(2.0, conn.scale / (1200 * 65536.0));

            case DST_TYPE.EG1_SUSTAIN_LEVEL:
            case DST_TYPE.EG2_SUSTAIN_LEVEL:
                return conn.scale / 655360.0;

            case DST_TYPE.PITCH:
            case DST_TYPE.LFO_FREQUENCY:
            case DST_TYPE.VIB_FREQUENCY:
            case DST_TYPE.FILTER_CUTOFF:
                return Math.Pow(2.0, (conn.scale / 65536.0 - 6900) / 1200.0) * 440;

            default:
                return 0.0;
            }
        }
    }

    public class WVPL : RiffChunk {
        public List<WAVE> List = new List<WAVE>();

        public WVPL(IntPtr ptr, uint size) : base(ptr, size) { }

        protected override void LoadList(IntPtr ptr, string type, uint size) {
            switch (type) {
            case "wave":
                List.Add(new WAVE(ptr, size));
                break;
            default:
                throw new Exception("[WVPL]Unknown ListType");
            }
        }
    }

    public class WAVE : RiffChunk {
        public CK_FMT Format { get; private set; }
        public CK_WSMP Sampler { get; private set; }
        public WAVE_LOOP[] Loops { get; private set; }
        public uint Addr { get; private set; }
        public uint Size { get; private set; }

        public WAVE(IntPtr ptr, uint size) : base(ptr, size) { }

        protected override void LoadChunk(IntPtr ptr, string type, uint size) {
            switch (type) {
            case "DLID":
            case "GUID":
                break;
            case "fmt ":
                Format = Marshal.PtrToStructure<CK_FMT>(ptr);
                break;
            case "data":
                Addr = (uint)ptr.ToInt64();
                Size = size;
                break;
            case "wsmp": {
                    Sampler = Marshal.PtrToStructure<CK_WSMP>(ptr);
                    ptr += Marshal.SizeOf<CK_WSMP>();
                    Loops = new WAVE_LOOP[Sampler.loopCount];
                    for (uint i = 0; i < Sampler.loopCount; ++i) {
                        Loops[i] = Marshal.PtrToStructure<WAVE_LOOP>(ptr);
                        ptr += Marshal.SizeOf<WAVE_LOOP>();
                    }
                }
                break;
            default:
                throw new Exception("[WAVE]Unknown ChunkType");
            }
        }

        protected override void LoadList(IntPtr ptr, string type, uint size) {
            switch (type) {
            default:
                throw new Exception("[WAVE]Unknown ListType");
            }
        }
    }
}
