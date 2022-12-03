using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;
using Player;

namespace SF2 {
    #region enum
    public enum E_OPER : ushort {
        OFFSET_ADDRS__START_LSB = 0,
        OFFSET_ADDRS__START_MSB = 4,
        OFFSET_ADDRS__END_LSB = 1,
        OFFSET_ADDRS__END_MSB = 12,
        OFFSET_ADDRS__LOOP_START_LSB = 2,
        OFFSET_ADDRS__LOOP_START_MSB = 45,
        OFFSET_ADDRS__LOOP_END_LSB = 3,
        OFFSET_ADDRS__LOOP_END_MSB = 50,
        //
        LFO_MOD__TO_PITCH = 5,
        LFO_MOD__TO_FILTER_FC = 10,
        LFO_MOD__TO_VOLUME = 13,
        LFO_MOD__DELAY = 21,
        LFO_MOD__FREQ = 22,
        //
        LFO_VIB__TO_PITCH = 6,
        LFO_VIB__DELAY = 23,
        LFO_VIB__FREQ = 24,
        //
        ENV_MOD__TO_PITCH = 7,
        ENV_MOD__TO_FILTER_FC = 11,
        ENV_MOD__DELAY = 25,
        ENV_MOD__ATTACK = 26,
        ENV_MOD__HOLD = 27,
        ENV_MOD__DECAY = 28,
        ENV_MOD__SUSTAIN = 29,
        ENV_MOD__RELEASE = 30,
        ENV_MOD__KEY_NUM_TO_HOLD = 31,
        ENV_MOD__KEY_NUM_TO_DECAY = 32,
        //
        ENV_VOL__DELAY = 33,
        ENV_VOL__ATTACK = 34,
        ENV_VOL__HOLD = 35,
        ENV_VOL__DECAY = 36,
        ENV_VOL__SUSTAIN = 37,
        ENV_VOL__RELEASE = 38,
        ENV_VOL__KEY_NUM_TO_HOLD = 39,
        ENV_VOL__KEY_NUM_TO_DECAY = 40,
        //
        INITIAL_FILTER__FC = 8,
        INITIAL_FILTER__Q = 9,
        //
        UNUSED1 = 14,
        CHORUS_EFFECTS_SEND = 15,
        REVERB_EFFECTS_SEND = 16,
        PAN = 17,
        UNUSED2 = 18,
        UNUSED3 = 19,
        UNUSED4 = 20,
        //
        INSTRUMENT = 41,
        RESERVED1 = 42,
        KEY_RANGE = 43,
        VEL_RANGE = 44,
        //
        KEYNUM = 46,
        VELOCITY = 47,
        INITIAL_ATTENUATION = 48,
        RESERVED2 = 49,
        //
        COARSE_TUNE = 51,
        FINETUNE = 52,
        SAMPLE_ID = 53,
        SAMPLE_MODES = 54,
        RESERVED3 = 55,
        SCALE_TUNING = 56,
        EXCLUSIVE_CLASS = 57,
        OVERRIDING_ROOTKEY = 58,
        UNUSED5 = 59,
        END_OPER = 60,
    };
    #endregion

    #region struct
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public struct PHDR {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public byte[] name;
        public ushort presetno;
        public ushort bank;
        public ushort bagIndex;
        public uint   library;
        public uint   genre;
        public uint   morph;
    };

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public struct INST {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public byte[] name;
        public ushort bagIndex;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public struct SHDR {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public byte[] name;
        public uint start;
        public uint end;
        public uint loopstart;
        public uint loopend;
        public uint sampleRate;
        public byte originalKey;
        public sbyte correction;
        public ushort sampleLink;
        public ushort type;
    };

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct BAG {
        public ushort genIndex;
        public ushort modIndex;
    };

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public struct MOD {
        public ushort srcOper;
        public E_OPER destOper;
        public short  modAmount;
        public ushort amtSrcOper;
        public ushort modTransOper;
    };

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct GEN {
        public E_OPER genOper;
        public short  genAmount;
    };

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct ENV_AMP {
        public double attack;
        public double hold;
        public double decay;
        public double sustain;
        public double release;
    };

    public struct Layer {
        public byte   keyLo;
        public byte   keyHi;
        public byte   velLo;
        public byte   velHi;
        public int    instId;
        public int    rootKey;
        public double gain;
        public double pan;
        public double coarseTune;
        public double fineTune;
        public ENV_AMP env;

        public void Init() {
            keyHi = 127;
            velHi = 127;
            instId = -1;
            rootKey = -1;
            gain = 1.0;
            env.sustain = -1.0;
            env.hold = -1.0;
        }
    }

    public struct INSTRUMENT {
        public byte   keyLo;
        public byte   keyHi;
        public byte   velLo;
        public byte   velHi;
        public int    sampleId;
        public int    rootKey;
        public bool   loopEnable;
        public uint   waveBegin;
        public uint   waveEnd;
        public double gain;
        public double pan;
        public double coarseTune;
        public double fineTune;
        public ENV_AMP env;

        public void Init() {
            keyHi = 127;
            velHi = 127;
            sampleId = -1;
            rootKey = -1;
            gain = 1.0;
            env.sustain = -1.0;
            env.hold = -1.0;
        }
    }

    public struct INST_ID {
        public byte isDrum;
        public byte bankMSB;
        public byte bankLSB;
        public byte progNum;
    };
    #endregion

    public class Const {
        public static readonly double AttackSpeed = 12.0;
        public static readonly double DecaySpeed = 12.0;
        public static readonly double ReleaseSpeed = 12.0;
    }

    public class SF2 : RiffChunk {
        private IntPtr mSf2Ptr;
        private string mPath;
        private PDTA mPdta;
        private SDTA mSdta;

        public SF2(string path) {
            mPath = path;
            Load(mPath);
            OutputPresetList();
            OutputInstList();
            OutputSampleList();
        }

        public SF2(string path, IntPtr ptr, uint size) : base(ptr, size) {
            mPath = path;
            mSf2Ptr = ptr;
        }

        protected override bool CheckFileType(string fileType, uint fileSize) {
            return "sfbk" == fileType;
        }

        protected override void LoadList(IntPtr ptr, string listType, uint listSize) {
            switch (listType) {
            case "pdta":
                mPdta = new PDTA(ptr, listSize);
                break;
            case "sdta":
                mSdta = new SDTA(ptr, listSize);
                break;
            default:
                break;
            }
        }

        private void OutputPresetList() {
            var sw = new StreamWriter(Path.GetDirectoryName(mPath) + "\\" + Path.GetFileNameWithoutExtension(mPath) + "_preset.csv");
            foreach (var preset in mPdta.PresetList) {
                sw.Write("{0},{1},\"{2}\n{3}\"",
                    preset.Key.bankMSB,
                    preset.Key.progNum,
                    1 == preset.Key.isDrum ? "Drum" : "Note",
                    preset.Value.Item1
                );
                sw.Write(",\"Key\nLow\",\"Key\nHigh\",\"Vel\nLow\",\"Vel\nHigh\"");
                sw.Write(",Gain,Pan");
                sw.Write(",RootKey,\"Tune\nHalf tone\",\"Tune\nCent\"");
                sw.Write(",A,H,D,S,R");
                sw.WriteLine(",Inst");

                foreach (var prgn in preset.Value.Item2) {
                    sw.Write(",,,{0},{1},{2},{3}",
                        prgn.keyLo, prgn.keyHi,
                        prgn.velLo, prgn.velHi
                    );
                    sw.Write(",{0},{1}",
                        prgn.gain.ToString("0.000"),
                        prgn.pan.ToString("0.000")
                    );

                    if (0 <= prgn.rootKey) {
                        sw.Write(",{0}", prgn.rootKey);
                    } else {
                        sw.Write(",--");
                    }
                    if (1.0 == prgn.coarseTune) {
                        sw.Write(",0");
                    } else {
                        sw.Write(",{0}", 12.0 / Math.Log(2.0, prgn.coarseTune));
                    }
                    if (1.0 == prgn.fineTune) {
                        sw.Write(",0");
                    } else {
                        sw.Write(",{0}", 1200.0 / Math.Log(2.0, prgn.fineTune));
                    }

                    sw.Write(",{0}", (Const.AttackSpeed * Sender.DELTA_TIME / prgn.env.attack).ToString("0.000"));
                    sw.Write(",{0}", prgn.env.hold.ToString("0.000"));
                    sw.Write(",{0}", (Const.AttackSpeed * Sender.DELTA_TIME / prgn.env.decay).ToString("0.000"));
                    sw.Write(",{0}", prgn.env.sustain.ToString("0.000"));
                    sw.Write(",{0}", (Const.AttackSpeed * Sender.DELTA_TIME / prgn.env.release).ToString("0.000"));

                    sw.Write(",{0}:{1}",
                        prgn.instId,
                        mPdta.InstList[prgn.instId].Item1
                    );

                    sw.WriteLine();
                }
            }
            sw.Close();
            sw.Dispose();
        }

        private void OutputInstList() {
            var sw = new StreamWriter(Path.GetDirectoryName(mPath)
                + "\\" + Path.GetFileNameWithoutExtension(mPath) + "_inst.csv");
            int instNo = 0;
            foreach (var inst in mPdta.InstList) {
                sw.Write("{0}:{1}", instNo, inst.Item1);
                sw.Write(",\"Key\nLow\",\"Key\nHigh\",\"Vel\nLow\",\"Vel\nHigh\"");
                sw.Write(",Gain,Pan");
                sw.Write(",RootKey");
                sw.Write(",\"Tune\nHalf tone\",\"Tune\nCent\"");
                sw.Write(",A,H,D,S,R");
                sw.WriteLine(",Sample,Offset,Length,\"Loop\nBegin\",\"Loop\nLength\"");
                instNo++;
                foreach (var irgn in inst.Item2) {
                    var smpl = mPdta.SHDR[irgn.sampleId];
                    var smplName = Encoding.ASCII.GetString(smpl.name).Replace("\0", "").TrimEnd();
                    sw.Write(",{0},{1},{2},{3}",
                        irgn.keyLo, irgn.keyHi,
                        irgn.velLo, irgn.velHi
                    );
                    sw.Write(",{0},{1}",
                        irgn.gain.ToString("0.000"),
                        irgn.pan.ToString("0.000")
                    );

                    if (0 <= irgn.rootKey) {
                        sw.Write(",{0}", irgn.rootKey);
                    } else {
                        sw.Write(",--");
                    }

                    if (1.0 == irgn.coarseTune) {
                        sw.Write(",0");
                    } else {
                        sw.Write(",{0}", 12.0 / Math.Log(2.0, irgn.coarseTune));
                    }
                    if (1.0 == irgn.fineTune) {
                        sw.Write(",0");
                    } else {
                        sw.Write(",{0}", 1200.0 / Math.Log(2.0, irgn.fineTune));
                    }

                    sw.Write(",{0}", (Const.AttackSpeed * Sender.DELTA_TIME / irgn.env.attack).ToString("0.000"));
                    sw.Write(",{0}", irgn.env.hold.ToString("0.000"));
                    sw.Write(",{0}", (Const.AttackSpeed * Sender.DELTA_TIME / irgn.env.decay).ToString("0.000"));
                    sw.Write(",{0}", irgn.env.sustain.ToString("0.000"));
                    sw.Write(",{0}", (Const.AttackSpeed * Sender.DELTA_TIME / irgn.env.release).ToString("0.000"));

                    var waveBegin = smpl.start + irgn.waveBegin;
                    var waveEnd = smpl.end + irgn.waveEnd;
                    var waveLen = waveEnd - waveBegin + 1;
                    if (irgn.loopEnable) {
                        var loopBegin = smpl.loopstart - waveBegin;
                        var loopLen = smpl.loopend - smpl.loopstart + 1;
                        sw.WriteLine(",{0}:{1},0x{2},{3},{4},{5}",
                            irgn.sampleId,
                            smplName,
                            (waveBegin * 2).ToString("X8"),
                            waveEnd - waveBegin + 1,
                            loopBegin,
                            loopLen
                        );
                    } else {
                        sw.WriteLine(",{0}:{1},0x{2},{3},-,-",
                            irgn.sampleId,
                            smplName,
                            (waveBegin * 2).ToString("X8"),
                            waveLen
                        );
                    }
                }
            }
            sw.Close();
            sw.Dispose();
        }

        private void OutputSampleList() {
            var sw = new StreamWriter(Path.GetDirectoryName(mPath)
                + "\\" + Path.GetFileNameWithoutExtension(mPath) + "_sample.csv");
            int no = 0;
            sw.WriteLine("Name,UnityKey,Tune,SampleRate,Type,Addr,Length");
            foreach (var sample in mPdta.SHDR) {
                sw.Write("{0}:{1}", no, Encoding.ASCII.GetString(sample.name).Replace("\0", "").TrimEnd());
                sw.Write(",{0}", sample.originalKey);
                sw.Write(",{0}", sample.correction);
                sw.Write(",{0}", sample.sampleRate);
                sw.Write(",{0}", sample.type);
                sw.Write(",0x{0}", (sample.start * 2).ToString("X8"));
                sw.Write(",{0}", sample.end - sample.start + 1);
                sw.WriteLine();
                no++;
            }
            sw.Close();
            sw.Dispose();
        }
    }

    public class PDTA : RiffChunk {
        public Dictionary<INST_ID, Tuple<string, Layer[]>> PresetList
            = new Dictionary<INST_ID, Tuple<string, Layer[]>>();

        public List<Tuple<string, INSTRUMENT[]>> InstList
            = new List<Tuple<string, INSTRUMENT[]>>();

        public List<SHDR> SHDR = new List<SHDR>();

        private struct Preset {
            public INST_ID Id;
            public string Name;
            public Layer[] Layer;
        }

        private List<PHDR> mPHDR = new List<PHDR>();
        private List<BAG> mPBAG = new List<BAG>();
        private List<MOD> mPMOD = new List<MOD>();
        private List<GEN> mPGEN = new List<GEN>();
        private List<INST> mINST = new List<INST>();
        private List<BAG> mIBAG = new List<BAG>();
        private List<MOD> mIMOD = new List<MOD>();
        private List<GEN> mIGEN = new List<GEN>();

        public PDTA(IntPtr ptr, uint size) : base(ptr, size) {
            SetPresetList();
            SetInstList();
        }

        private void SetPresetList() {
            var presetList = new List<Preset>();
            for (int i = 0; i < mPHDR.Count; i++) {
                var preset = mPHDR[i];
                int bagCount;
                if (i < mPHDR.Count - 1) {
                    bagCount = mPHDR[i + 1].bagIndex - preset.bagIndex;
                } else {
                    bagCount = mPBAG.Count - preset.bagIndex;
                }

                var global = new Layer();
                global.Init();
                var list = new List<Layer>();
                for (int ib = 0, bagIdx = preset.bagIndex; ib < bagCount; ib++, bagIdx++) {
                    var bag = mPBAG[bagIdx];
                    int genCount;
                    if (bagIdx < mPBAG.Count - 1) {
                        genCount = mPBAG[bagIdx + 1].genIndex - bag.genIndex;
                    } else {
                        genCount = mPGEN.Count - bag.genIndex;
                    }
                    var v = GetPresetGen(global, bag.genIndex, genCount);
                    if (v.instId < 0) {
                        global = v;
                    } else {
                        list.Add(v);
                    }
                }

                var name = Encoding.ASCII.GetString(preset.name);
                if (0 <= name.IndexOf("\0")) {
                    name = name.Substring(0, name.IndexOf("\0"));
                }
                var pre = new Preset();
                pre.Id.isDrum = (byte)(0 < (preset.bank & 0x80) ? 1 : 0);
                pre.Id.bankMSB = (byte)(preset.bank & 0x7F);
                pre.Id.progNum = (byte)preset.presetno;
                pre.Name = name.Replace("\0", "").TrimEnd();
                pre.Layer = list.ToArray();
                presetList.Add(pre);
            }

            presetList.Sort(Compare);
            foreach (var preset in presetList) {
                if (!PresetList.ContainsKey(preset.Id)) {
                    PresetList.Add(preset.Id, new Tuple<string, Layer[]>(preset.Name, preset.Layer));
                }
            }
            presetList.Clear();

            mPHDR.Clear();
            mPBAG.Clear();
            mPGEN.Clear();
            mPMOD.Clear();
        }

        private void SetInstList() {
            for (int i = 0; i < mINST.Count; i++) {
                var inst = mINST[i];
                int bagCount;
                if (i < mINST.Count - 1) {
                    bagCount = mINST[i + 1].bagIndex - inst.bagIndex;
                } else {
                    bagCount = mIBAG.Count - inst.bagIndex;
                }

                var global = new INSTRUMENT();
                global.Init();
                var list = new List<INSTRUMENT>();
                for (int ib = 0, bagIdx = inst.bagIndex; ib < bagCount; ib++, bagIdx++) {
                    var bag = mIBAG[bagIdx];
                    int genCount;
                    if (bagIdx < mIBAG.Count - 1) {
                        genCount = mIBAG[bagIdx + 1].genIndex - bag.genIndex;
                    } else {
                        genCount = mIGEN.Count - bag.genIndex;
                    }
                    var v = GetInstGen(global, bag.genIndex, genCount);
                    if (v.sampleId < 0) {
                        global = v;
                    } else {
                        list.Add(v);
                    }
                }

                var name = Encoding.ASCII.GetString(inst.name);
                if (0 <= name.IndexOf("\0")) {
                    name = name.Substring(0, name.IndexOf("\0"));
                }
                InstList.Add(new Tuple<string, INSTRUMENT[]>(name.TrimEnd(), list.ToArray()));
            }
            mINST.Clear();
            mIBAG.Clear();
            mIGEN.Clear();
            mIMOD.Clear();
        }

        private Layer GetPresetGen(Layer global, int begin, int count) {
            var v = new Layer();
            v.Init();

            for (int i = 0, genIdx = begin; i < count; i++, genIdx++) {
                var g = mPGEN[genIdx];

                switch (g.genOper) {
                case E_OPER.KEY_RANGE:
                    v.keyLo = (byte)(g.genAmount & 0x7F);
                    v.keyHi = (byte)((g.genAmount >> 8) & 0x7F);
                    break;
                case E_OPER.VEL_RANGE:
                    v.velLo = (byte)(g.genAmount & 0x7F);
                    v.velHi = (byte)((g.genAmount >> 8) & 0x7F);
                    break;

                case E_OPER.INITIAL_ATTENUATION:
                    v.gain = Math.Pow(10.0, -g.genAmount / 200.0);
                    break;
                case E_OPER.PAN:
                    v.pan = g.genAmount / 500.0;
                    break;
                case E_OPER.INSTRUMENT:
                    v.instId = g.genAmount;
                    break;
                case E_OPER.COARSE_TUNE:
                    v.coarseTune = Math.Pow(2.0, g.genAmount / 120.0);
                    break;
                case E_OPER.FINETUNE:
                    v.fineTune = Math.Pow(2.0, g.genAmount / 1200.0);
                    break;
                case E_OPER.OVERRIDING_ROOTKEY:
                    v.rootKey = g.genAmount;
                    break;

                case E_OPER.ENV_VOL__ATTACK:
                    v.env.attack = Const.AttackSpeed * Sender.DELTA_TIME
                        / Math.Pow(2.0, g.genAmount / 1200.0);
                    break;
                case E_OPER.ENV_VOL__HOLD:
                    v.env.hold = Math.Pow(2.0, g.genAmount / 1200.0);
                    break;
                case E_OPER.ENV_VOL__DECAY:
                    v.env.decay = Const.AttackSpeed * Sender.DELTA_TIME
                        / Math.Pow(2.0, g.genAmount / 1200.0);
                    break;
                case E_OPER.ENV_VOL__SUSTAIN:
                    v.env.sustain = Math.Pow(10.0, -(ushort)g.genAmount / 200.0);
                    break;
                case E_OPER.ENV_VOL__RELEASE:
                    v.env.release = Const.AttackSpeed * Sender.DELTA_TIME
                        / Math.Pow(2.0, g.genAmount / 1200.0);
                    break;

                default:
                    break;
                }
            }

            /**** set global value ****/
            if (0 <= v.instId) {
                if (v.rootKey < 0) {
                    v.rootKey = global.rootKey;
                }
                if (v.coarseTune == 0.0) {
                    v.coarseTune = global.coarseTune;
                }
                if (v.fineTune == 0.0) {
                    v.fineTune = global.fineTune;
                }
                if (v.env.attack <= 0.0) {
                    v.env.attack = global.env.attack;
                }
                if (v.env.decay <= 0.0) {
                    v.env.decay = global.env.decay;
                }
                if (v.env.release <= 0.0) {
                    v.env.release = global.env.release;
                }
                if (v.env.hold < 0.0) {
                    v.env.hold = global.env.hold;
                }
                if (v.env.sustain < 0.0) {
                    v.env.sustain = global.env.sustain;
                }
            }

            /**** set default value ****/
            {
                if (v.coarseTune == 0.0) {
                    v.coarseTune = 1.0;
                }
                if (v.fineTune == 0.0) {
                    v.fineTune = 1.0;
                }
                if (v.env.attack <= 0.0) {
                    v.env.attack = 1000 * Const.AttackSpeed * Sender.DELTA_TIME;
                }
                if (v.env.decay <= 0.0) {
                    v.env.decay = 1000 * Const.AttackSpeed * Sender.DELTA_TIME;
                }
                if (v.env.release <= 0.0) {
                    v.env.release = 1000 * Const.AttackSpeed * Sender.DELTA_TIME;
                }
                if (v.env.hold < 0.0) {
                    v.env.hold = 0.0;
                }
                if (v.env.sustain < 0.0) {
                    v.env.sustain = 1.0;
                }
                v.env.hold += Const.AttackSpeed * Sender.DELTA_TIME / v.env.attack;
            }

            return v;
        }

        private INSTRUMENT GetInstGen(INSTRUMENT global, int begin, int count) {
            var v = new INSTRUMENT();
            v.Init();

            for (int i = 0, genIdx = begin; i < count; i++, genIdx++) {
                var g = mIGEN[genIdx];

                switch (g.genOper) {
                case E_OPER.KEY_RANGE:
                    v.keyLo = (byte)(g.genAmount & 0x7F);
                    v.keyHi = (byte)((g.genAmount >> 8) & 0x7F);
                    break;
                case E_OPER.VEL_RANGE:
                    v.velLo = (byte)(g.genAmount & 0x7F);
                    v.velHi = (byte)((g.genAmount >> 8) & 0x7F);
                    break;

                case E_OPER.INITIAL_ATTENUATION:
                    v.gain = Math.Pow(10.0, -g.genAmount / 200.0);
                    break;
                case E_OPER.PAN:
                    v.pan = g.genAmount / 500.0;
                    break;
                case E_OPER.COARSE_TUNE:
                    v.coarseTune = Math.Pow(2.0, g.genAmount / 120.0);
                    break;
                case E_OPER.FINETUNE:
                    v.fineTune = Math.Pow(2.0, g.genAmount / 1200.0);
                    break;
                case E_OPER.OVERRIDING_ROOTKEY:
                    v.rootKey = g.genAmount;
                    break;

                case E_OPER.SAMPLE_MODES:
                    v.loopEnable = 0 < (g.genAmount & 1);
                    break;
                case E_OPER.SAMPLE_ID:
                    v.sampleId = g.genAmount;
                    break;

                case E_OPER.OFFSET_ADDRS__START_LSB:
                    v.waveBegin |= (ushort)g.genAmount;
                    break;
                case E_OPER.OFFSET_ADDRS__START_MSB:
                    v.waveBegin |= (uint)((ushort)g.genAmount << 16);
                    break;
                case E_OPER.OFFSET_ADDRS__END_LSB:
                    v.waveEnd |= (ushort)g.genAmount;
                    break;
                case E_OPER.OFFSET_ADDRS__END_MSB:
                    v.waveEnd |= (uint)((ushort)g.genAmount << 16);
                    break;

                case E_OPER.ENV_VOL__ATTACK:
                    v.env.attack = Const.AttackSpeed * Sender.DELTA_TIME
                        / Math.Pow(2.0, g.genAmount / 1200.0);
                    break;
                case E_OPER.ENV_VOL__HOLD:
                    v.env.hold = Math.Pow(2.0, g.genAmount / 1200.0);
                    break;
                case E_OPER.ENV_VOL__DECAY:
                    v.env.decay = Const.AttackSpeed * Sender.DELTA_TIME
                        / Math.Pow(2.0, g.genAmount / 1200.0);
                    break;
                case E_OPER.ENV_VOL__SUSTAIN:
                    v.env.sustain = Math.Pow(10.0, -(ushort)g.genAmount / 200.0);
                    break;
                case E_OPER.ENV_VOL__RELEASE:
                    v.env.release = Const.AttackSpeed * Sender.DELTA_TIME
                        / Math.Pow(2.0, g.genAmount / 1200.0);
                    break;
                default:
                    break;
                }
            }

            /**** set global value ****/
            if (0 <= v.sampleId) {
                if (v.rootKey < 0) {
                    v.rootKey = global.rootKey;
                }
                if (v.coarseTune == 0.0) {
                    v.coarseTune = global.coarseTune;
                }
                if (v.fineTune == 0.0) {
                    v.fineTune = global.fineTune;
                }
                if (v.env.attack <= 0.0) {
                    v.env.attack = global.env.attack;
                }
                if (v.env.decay <= 0.0) {
                    v.env.decay = global.env.decay;
                }
                if (v.env.release <= 0.0) {
                    v.env.release = global.env.release;
                }
                if (v.env.hold < 0.0) {
                    v.env.hold = global.env.hold;
                }
                if (v.env.sustain < 0.0) {
                    v.env.sustain = global.env.sustain;
                }
            }

            /**** set default value ****/
            {
                if (v.coarseTune == 0.0) {
                    v.coarseTune = 1.0;
                }
                if (v.fineTune == 0.0) {
                    v.fineTune = 1.0;
                }
                if (v.env.attack <= 0.0) {
                    v.env.attack = 1000 * Const.AttackSpeed * Sender.DELTA_TIME;
                }
                if (v.env.decay <= 0.0) {
                    v.env.decay = 1000 * Const.AttackSpeed * Sender.DELTA_TIME;
                }
                if (v.env.release <= 0.0) {
                    v.env.release = 1000 * Const.AttackSpeed * Sender.DELTA_TIME;
                }
                if (v.env.hold < 0.0) {
                    v.env.hold = 0.0;
                }
                if (v.env.sustain < 0.0) {
                    v.env.sustain = 1.0;
                }
                v.env.hold += Const.AttackSpeed * Sender.DELTA_TIME / v.env.attack;
            }

            return v;
        }

        private static readonly Comparison<Preset> Compare = new Comparison<Preset>((a, b) => {
            var av = (long)a.Id.isDrum << 24;
            av |= (long)a.Id.progNum << 16;
            av |= (long)a.Id.bankMSB << 8;
            av |= a.Id.bankLSB;
            var bv = (long)b.Id.isDrum << 24;
            bv |= (long)b.Id.progNum << 16;
            bv |= (long)b.Id.bankMSB << 8;
            bv |= b.Id.bankLSB;
            var dComp = av - bv;
            return 0 == dComp ? 0 : (0 < dComp ? 1 : -1);
        });

        protected override void LoadChunk(IntPtr ptr, string chunkType, uint chunkSize) {
            switch (chunkType) {
            case "phdr":
                for (int pos = 0; pos < chunkSize; pos += Marshal.SizeOf<PHDR>()) {
                    mPHDR.Add(Marshal.PtrToStructure<PHDR>(ptr + pos));
                }
                break;
            case "pbag":
                for (int pos = 0; pos < chunkSize; pos += Marshal.SizeOf<BAG>()) {
                    mPBAG.Add(Marshal.PtrToStructure<BAG>(ptr + pos));
                }
                break;
            case "pmod":
                for (int pos = 0; pos < chunkSize; pos += Marshal.SizeOf<MOD>()) {
                    mPMOD.Add(Marshal.PtrToStructure<MOD>(ptr + pos));
                }
                break;
            case "pgen":
                for (int pos = 0; pos < chunkSize; pos += Marshal.SizeOf<GEN>()) {
                    mPGEN.Add(Marshal.PtrToStructure<GEN>(ptr + pos));
                }
                break;
            case "inst":
                for (int pos = 0; pos < chunkSize; pos += Marshal.SizeOf<INST>()) {
                    mINST.Add(Marshal.PtrToStructure<INST>(ptr + pos));
                }
                break;
            case "ibag":
                for (int pos = 0; pos < chunkSize; pos += Marshal.SizeOf<BAG>()) {
                    mIBAG.Add(Marshal.PtrToStructure<BAG>(ptr + pos));
                }
                break;
            case "imod":
                for (int pos = 0; pos < chunkSize; pos += Marshal.SizeOf<MOD>()) {
                    mIMOD.Add(Marshal.PtrToStructure<MOD>(ptr + pos));
                }
                break;
            case "igen":
                for (int pos = 0; pos < chunkSize; pos += Marshal.SizeOf<GEN>()) {
                    mIGEN.Add(Marshal.PtrToStructure<GEN>(ptr + pos));
                }
                break;
            case "shdr":
                for (int pos = 0; pos < chunkSize; pos += Marshal.SizeOf<SHDR>()) {
                    var shdr = Marshal.PtrToStructure<SHDR>(ptr + pos);
                    var name = Encoding.ASCII.GetString(shdr.name);
                    if (0 <= name.IndexOf("\0")) {
                        name = name.Substring(0, name.IndexOf("\0"));
                    }
                    shdr.name = Encoding.ASCII.GetBytes(name);
                    SHDR.Add(shdr);
                }
                break;
            default:
                break;
            }
        }
    }

    public class SDTA : RiffChunk {
        public IntPtr pData { get; private set; }

        public SDTA(IntPtr ptr, uint size) : base(ptr, size) { }

        protected override void LoadChunk(IntPtr ptr, string chunkType, uint chunkSize) {
            switch (chunkType) {
            case "smpl":
                pData = ptr;
                break;
            default:
                break;
            }
        }
    }
}
