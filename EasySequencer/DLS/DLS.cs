using System;
using System.Runtime.InteropServices;
using Player;

namespace DLS {
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
}
