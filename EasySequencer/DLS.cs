using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Player;

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

        unsafe public void GetInstList(INST_LIST *list) {
            list->instCount = 0;
            list->ppInst = (INST_REC**)Marshal.AllocHGlobal(sizeof(INST_REC*) * Instruments.List.Count);
            foreach (var inst in Instruments.List) {
                list->ppInst[list->instCount] = (INST_REC*)Marshal.AllocHGlobal(Marshal.SizeOf<INST_REC>());
                var pInst = list->ppInst[list->instCount];
                list->instCount++;
                //
                pInst->id.isDrum = (byte)(inst.Header.locale.bankFlags == 0x80 ? 1 : 0);
                pInst->id.programNo = inst.Header.locale.programNo;
                pInst->id.bankMSB = inst.Header.locale.bankMSB;
                pInst->id.bankLSB = inst.Header.locale.bankLSB;
                pInst->regionCount = inst.Regions.List.Count;
                if (string.IsNullOrWhiteSpace(inst.Name)) {
                    pInst->pName = Marshal.StringToHGlobalAuto(string.Format(
                        "MSB:{0} LSB:{1} PROG:{2}",
                        pInst->id.bankMSB.ToString("000"),
                        pInst->id.bankLSB.ToString("000"),
                        pInst->id.programNo.ToString("000")
                    ));
                } else {
                    pInst->pName = Marshal.StringToHGlobalAuto(inst.Name);
                }
                if (string.IsNullOrWhiteSpace(inst.Category)) {
                    if (0 < pInst->id.isDrum) {
                        pInst->pCategory = Marshal.StringToHGlobalAuto("Drum set");
                    } else {
                        pInst->pCategory = Marshal.StringToHGlobalAuto("");
                    }
                } else {
                    pInst->pCategory = Marshal.StringToHGlobalAuto(inst.Category);
                }
                pInst->ppRegions = (REGION**)Marshal.AllocHGlobal(sizeof(REGION*) * inst.Regions.List.Count);

                #region instEnv
                var instEnv = new ENVELOPE();
                instEnv.deltaA = 1000.0 * Sender.DeltaTime * Sender.AttackSpeed;  // 1msec
                instEnv.deltaD = 1000.0 * Sender.DeltaTime * Sender.DecaySpeed;   // 1msec
                instEnv.deltaR = 1000.0 * Sender.DeltaTime * Sender.ReleaseSpeed; // 1msec
                instEnv.levelS = 1.0;
                instEnv.hold = 0.0;
                if (null != inst.Articulations) {
                    foreach (var conn in inst.Articulations.Art.List) {
                        if (SRC_TYPE.NONE != conn.source) {
                            continue;
                        }
                        switch (conn.destination) {
                        case DST_TYPE.EG1_ATTACK_TIME:
                            instEnv.deltaA = Sender.AttackSpeed * Sender.DeltaTime / ART.GetValue(conn);
                            instEnv.hold += ART.GetValue(conn);
                            break;
                        case DST_TYPE.EG1_HOLD_TIME:
                            instEnv.hold += ART.GetValue(conn);
                            break;
                        case DST_TYPE.EG1_DECAY_TIME:
                            instEnv.deltaD = Sender.DecaySpeed * Sender.DeltaTime / ART.GetValue(conn);
                            break;
                        case DST_TYPE.EG1_RELEASE_TIME:
                            instEnv.deltaR = Sender.ReleaseSpeed * Sender.DeltaTime / ART.GetValue(conn);
                            break;
                        case DST_TYPE.EG1_SUSTAIN_LEVEL:
                            instEnv.levelS = (0.0 == ART.GetValue(conn)) ? 1.0 : (ART.GetValue(conn) * 0.01);
                            break;
                        }
                    }
                }
                if (instEnv.hold < Sender.DeltaTime) {
                    instEnv.hold = Sender.DeltaTime;
                }
                #endregion

                var ppRegions = pInst->ppRegions;
                var rgnIdx = 0;
                foreach (var rgn in inst.Regions.List) {
                    ppRegions[rgnIdx] = (REGION*)Marshal.AllocHGlobal(Marshal.SizeOf<REGION>());
                    var pRegion = ppRegions[rgnIdx];
                    rgnIdx++;

                    if (null == rgn.Articulations) {
                        pRegion->env = instEnv;
                    } else {
                        #region regionEnv
                        var regionEnv = new ENVELOPE();
                        regionEnv.deltaA = 1000.0 * Sender.DeltaTime * Sender.AttackSpeed;  // 1msec
                        regionEnv.deltaD = 1000.0 * Sender.DeltaTime * Sender.DecaySpeed;   // 1msec
                        regionEnv.deltaR = 1000.0 * Sender.DeltaTime * Sender.ReleaseSpeed; // 1msec
                        regionEnv.levelS = 1.0;
                        regionEnv.hold = 0.0;
                        foreach (var conn in rgn.Articulations.Art.List) {
                            if (SRC_TYPE.NONE != conn.source) {
                                continue;
                            }
                            switch (conn.destination) {
                            case DST_TYPE.EG1_ATTACK_TIME:
                                regionEnv.deltaA = Sender.AttackSpeed * Sender.DeltaTime / ART.GetValue(conn);
                                regionEnv.hold += ART.GetValue(conn);
                                break;
                            case DST_TYPE.EG1_HOLD_TIME:
                                regionEnv.hold += ART.GetValue(conn);
                                break;
                            case DST_TYPE.EG1_DECAY_TIME:
                                regionEnv.deltaD = Sender.DecaySpeed * Sender.DeltaTime / ART.GetValue(conn);
                                break;
                            case DST_TYPE.EG1_SUSTAIN_LEVEL:
                                regionEnv.levelS = (0.0 == ART.GetValue(conn)) ? 1.0 : (ART.GetValue(conn) * 0.01);
                                break;
                            case DST_TYPE.EG1_RELEASE_TIME:
                                regionEnv.deltaR = Sender.ReleaseSpeed * Sender.DeltaTime / ART.GetValue(conn);
                                break;
                            }
                        }
                        if (regionEnv.hold < Sender.DeltaTime) {
                            regionEnv.hold = Sender.DeltaTime;
                        }
                        #endregion
                        pRegion->env = regionEnv;
                    }

                    var wave = WavePool.List[(int)rgn.WaveLink.tableIndex];
                    var samples = wave.Size / wave.Format.blockAlign;
                    pRegion->waveInfo.waveOfs = wave.Addr - (uint)mDlsPtr.ToInt64();
                    if (rgn.HasSampler) {
                        pRegion->waveInfo.gain = rgn.Sampler.Gain / 32768.0;
                        pRegion->waveInfo.unityNote = (byte)rgn.Sampler.unityNote;
                        pRegion->waveInfo.delta
                            = Math.Pow(2.0, rgn.Sampler.fineTune / 1200.0)
                            * wave.Format.sampleRate / Sender.SampleRate;
                        ;
                        if (rgn.HasLoop) {
                            pRegion->waveInfo.loopBegin = rgn.Loops[0].start;
                            pRegion->waveInfo.loopLength = rgn.Loops[0].length;
                            pRegion->waveInfo.loopEnable = true;
                        } else if (wave.HasLoop) {
                            pRegion->waveInfo.loopBegin = wave.Loops[0].start;
                            pRegion->waveInfo.loopLength = wave.Loops[0].length;
                            pRegion->waveInfo.loopEnable = true;
                        } else {
                            pRegion->waveInfo.loopBegin = 0;
                            pRegion->waveInfo.loopLength = samples;
                            pRegion->waveInfo.loopEnable = false;
                            pRegion->env.deltaR = Sender.DeltaTime * pRegion->waveInfo.delta / samples;
                        }
                    } else {
                        pRegion->waveInfo.gain = wave.Sampler.Gain / 32768.0;
                        pRegion->waveInfo.unityNote = (byte)wave.Sampler.unityNote;
                        pRegion->waveInfo.delta
                            = Math.Pow(2.0, wave.Sampler.fineTune / 1200.0)
                            * wave.Format.sampleRate / Sender.SampleRate;
                        ;
                        if (wave.HasLoop) {
                            pRegion->waveInfo.loopBegin = wave.Loops[0].start;
                            pRegion->waveInfo.loopLength = wave.Loops[0].length;
                            pRegion->waveInfo.loopEnable = true;
                        } else {
                            pRegion->waveInfo.loopBegin = 0;
                            pRegion->waveInfo.loopLength = samples;
                            pRegion->waveInfo.loopEnable = false;
                        }
                    }
                    pRegion->keyLo = (byte)rgn.Header.key.low;
                    pRegion->keyHi = (byte)rgn.Header.key.high;
                    pRegion->velLo = (byte)rgn.Header.velocity.low;
                    pRegion->velHi = (byte)rgn.Header.velocity.high;
                }
            }
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
        public List<WAVE_LOOP> Loops { get; private set; } = new List<WAVE_LOOP>();
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
            case "wsmp":
                Sampler = Marshal.PtrToStructure<CK_WSMP>(ptr);
                ptr += Marshal.SizeOf<CK_WSMP>();
                for (uint i = 0; i < Sampler.loopCount; ++i) {
                    Loops.Add(Marshal.PtrToStructure<WAVE_LOOP>(ptr));
                    ptr += Marshal.SizeOf<WAVE_LOOP>();
                }
                HasSampler = true;
                HasLoop = 0 < Sampler.loopCount;
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
        public List<CONNECTION> List { get; private set; } = new List<CONNECTION>();

        public ART(IntPtr ptr) {
            ptr += 4;
            var connCount = Marshal.PtrToStructure<uint>(ptr);
            ptr += 4;
            for (uint i = 0; i < connCount; ++i) {
                List.Add(Marshal.PtrToStructure<CONNECTION>(ptr));
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
        public List<WAVE_LOOP> Loops { get; private set; } = new List<WAVE_LOOP>();
        public bool HasLoop { get; private set; }
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
            case "wsmp":
                Sampler = Marshal.PtrToStructure<CK_WSMP>(ptr);
                ptr += Marshal.SizeOf<CK_WSMP>();
                for (uint i = 0; i < Sampler.loopCount; ++i) {
                    Loops.Add(Marshal.PtrToStructure<WAVE_LOOP>(ptr));
                    ptr += Marshal.SizeOf<WAVE_LOOP>();
                }
                HasLoop = 0 < Sampler.loopCount;
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
