using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;
using WaveOut;

namespace SF2 {
    #region enum
    public enum E_OPER : ushort {
        ADDRS_OFFSET_START_LSB = 0,
        ADDRS_OFFSET_END_LSB = 1,
        ADDRS_OFFSET_LOOP_START_LSB = 2,
        ADDRS_OFFSET_LOOP_END_LSB = 3,
        ADDRS_OFFSET_START_MSB = 4,
        MOD_LFO_TO_PITCH = 5,
        VIB_LFO_TO_PITCH = 6,
        MOD_ENV_TO_PITCH = 7,
        INITIAL_FILTER_FC = 8,
        INITIAL_FILTER_Q = 9,
        MOD_LFO_TO_FILTER_FC = 10,
        MOD_ENV_TO_FILTER_FC = 11,
        ADDRS_OFFSET_END_MSB = 12,
        MOD_LFO_TO_VOLUME = 13,
        UNUSED1 = 14,
        CHORUS_EFFECTS_SEND = 15,
        REVERB_EFFECTS_SEND = 16,
        PAN = 17,
        UNUSED2 = 18,
        UNUSED3 = 19,
        UNUSED4 = 20,
        DELAY_MOD_LFO = 21,
        FREQ_MOD_LFO = 22,
        DELAY_VIB_LFO = 23,
        FREQ_VIB_LFO = 24,
        DELAY_MOD_ENV = 25,
        ATTACK_MOD_ENV = 26,
        HOLD_MOD_ENV = 27,
        DECAY_MOD_ENV = 28,
        SUSTAIN_MOD_ENV = 29,
        RELEASE_MOD_ENV = 30,
        KEY_NUM_TO_MOD_ENV_HOLD = 31,
        KEY_NUM_TO_MOD_ENV_DECAY = 32,
        ENV_VOL_DELAY = 33,
        ENV_VOL_ATTACK = 34,
        ENV_VOL_HOLD = 35,
        ENV_VOL_DECAY = 36,
        ENV_VOL_SUSTAIN = 37,
        ENV_VOL_RELEASE = 38,
        KEY_NUM_TO_VOL_ENV_HOLD = 39,
        KEY_NUM_TO_VOL_ENV_DECAY = 40,
        INSTRUMENT = 41,
        RESERVED1 = 42,
        KEY_RANGE = 43,
        VEL_RANGE = 44,
        ADDRS_OFFSET_LOOP_START_MSB = 45,
        KEYNUM = 46,
        VELOCITY = 47,
        INITIAL_ATTENUATION = 48,
        RESERVED2 = 49,
        ADDRS_OFFSET_LOOP_END_MSB = 50,
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

    public struct PRESET_RANGE {
        public byte keyLow;
        public byte keyHigh;
        public byte velLow;
        public byte velHigh;
        public int instId;
        public double gain;
        public double pan;
        public void Init() {
            keyHigh = 127;
            velHigh = 127;
            instId = -1;
            gain = 1.0;
        }
    }

    public struct INST_RANGE {
        public byte keyLow;
        public byte keyHigh;
        public byte velLow;
        public byte velHigh;
        public bool loopEnable;
        public int sampleId;
        public int rootKey;
        public uint waveBegin;
        public uint waveEnd;
        public double gain;
        public double pan;
        public double coarseTune;
        public double fineTune;
        public ENVELOPE env;
        public void Init() {
            keyHigh = 127;
            velHigh = 127;
            sampleId = -1;
            rootKey = -1;
            gain = 1.0;
            env.levelS = -1.0;
            env.hold = -1.0;
        }
    }
    #endregion

    public class SF2 : RiffChunk {
        private IntPtr mSf2Ptr;
        private string mPath;
        private PDTA mPdta;
        private SDTA mSdta;

        public SF2(string path, IntPtr ptr, uint size) : base(ptr, size) {
            mPath = path;
            mSf2Ptr = ptr;

            OutputPresetList();
            OutputInstList();
            OutputSampleList();

            var loc = new INST_ID();
            var ins = GetInstList()[loc];
            OutputWave(ins, 60, 127);
        }

        public Dictionary<INST_ID, INST_INFO> GetInstList() {
            var instList = new Dictionary<INST_ID, INST_INFO>();
            foreach (var preset in mPdta.PresetList) {
                var waves = new WAVE_INFO[128, 128];
                for (int velo = 0; velo < 128; velo++) {
                    for (int noteNo = 0; noteNo < 128; noteNo++) {
                        var wave = new WAVE_INFO();
                        wave.buffOfs = uint.MaxValue;
                        var loop = new WAVE_LOOP();
                        foreach (var range in preset.Value.Item2) {
                            if (noteNo < range.keyLow || range.keyHigh < noteNo || velo < range.keyLow || range.velHigh < velo) {
                                continue;
                            }
                            foreach (var inst in mPdta.InstList[range.instId].Item2) {
                                if (noteNo < inst.keyLow || inst.keyHigh < noteNo || inst.velHigh < velo || velo < inst.keyLow || inst.velHigh < velo) {
                                    continue;
                                }
                                var smpl = mPdta.SampleList[inst.sampleId];
                                var waveBegin = smpl.start + inst.waveBegin;
                                loop.enable = inst.loopEnable;
                                if (loop.enable) {
                                    loop.begin = smpl.loopstart - waveBegin;
                                    loop.length = smpl.loopend - smpl.loopstart + 1;
                                } else {
                                    var waveEnd = smpl.end + inst.waveEnd;
                                    loop.begin = 0;
                                    loop.length = waveEnd - waveBegin + 1;
                                }
                                wave.buffOfs = (uint)(mSdta.pData.ToInt64() - mSf2Ptr.ToInt64() + waveBegin * 2);
                                wave.gain = inst.gain * range.gain / 32768.0;
                                if (0 <= inst.rootKey) {
                                    wave.unityNote = (byte)inst.rootKey;
                                } else {
                                    wave.unityNote = smpl.originalKey;
                                }
                                wave.delta = inst.fineTune * inst.coarseTune * smpl.sampleRate / Const.SampleRate;
                                var diffNote = noteNo - wave.unityNote;
                                if (diffNote < 0) {
                                    wave.delta /= Const.SemiTone[-diffNote];
                                } else {
                                    wave.delta *= Const.SemiTone[diffNote];
                                }
                                wave.envAmp = inst.env;
                                wave.loop = loop;
                            }
                        }
                        waves[velo, noteNo] = wave;
                    }
                }
                var instInfo = new INST_INFO();
                instInfo.name = preset.Value.Item1;
                instInfo.catgory = preset.Key.isDrum == 1 ? "Drum set" : "";
                instInfo.waves = waves;
                instList.Add(preset.Key, instInfo);
            }
            return instList;
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
                    preset.Key.programNo,
                    1 == preset.Key.isDrum ? "Drum" : "Note",
                    preset.Value.Item1
                );
                sw.Write(",\"Key\nLow\",\"Key\nHigh\",\"Vel\nLow\",\"Vel\nHigh\"");
                sw.WriteLine(",Gain,Pan,Inst");
                foreach (var prgn in preset.Value.Item2) {
                    sw.Write(",,,{0},{1},{2},{3}",
                        prgn.keyLow, prgn.keyHigh,
                        prgn.velLow, prgn.velHigh
                    );
                    sw.Write(",{0},{1},{2}:{3}",
                        prgn.gain.ToString("0.000"),
                        prgn.pan.ToString("0.000"),
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
                    var smpl = mPdta.SampleList[irgn.sampleId];
                    var smplName = Encoding.ASCII.GetString(smpl.name).Replace("\0", "").TrimEnd();
                    sw.Write(",{0},{1},{2},{3}",
                        irgn.keyLow, irgn.keyHigh,
                        irgn.velLow, irgn.velHigh
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

                    sw.Write(",{0}", (Const.EnvelopeSpeed * Const.DeltaTime / irgn.env.deltaA).ToString("0.000"));
                    sw.Write(",{0}", irgn.env.hold.ToString("0.000"));
                    sw.Write(",{0}", (Const.EnvelopeSpeed * Const.DeltaTime / irgn.env.deltaD).ToString("0.000"));
                    sw.Write(",{0}", irgn.env.levelS.ToString("0.000"));
                    sw.Write(",{0}", (Const.EnvelopeSpeed * Const.DeltaTime / irgn.env.deltaR).ToString("0.000"));

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
            foreach (var sample in mPdta.SampleList) {
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

        private void OutputWave(INST_INFO info, byte noteNo, byte velocity) {
            var sw = new StreamWriter(Path.GetDirectoryName(mPath)
                + "\\" + Path.GetFileNameWithoutExtension(mPath)
                + "_wave.csv");

            var wav = info.waves[velocity, noteNo];
            var ptr = mSf2Ptr + (int)wav.buffOfs;
            var idx = 0.0;
            var loopCount = 0;
            var sampleCount = 0;
            while (sampleCount < Const.SampleRate) {
                var cur = (int)idx;
                var pre = cur - 1;
                if (pre < 0) {
                    pre = 0;
                }
                var dt = idx - cur;

                var output
                    = Marshal.PtrToStructure<short>(ptr + cur * 2) * dt
                    + Marshal.PtrToStructure<short>(ptr + pre * 2) * (1.0 - dt);
                output *= wav.gain;

                sw.WriteLine(output.ToString("0.000") + "," + loopCount);
                sampleCount++;

                idx += wav.delta;
                if (wav.loop.begin + wav.loop.length <= idx) {
                    if (wav.loop.enable) {
                        idx -= wav.loop.length;
                        loopCount++;
                    } else {
                        break;
                    }
                }
            }
            sw.Close();
            sw.Dispose();
        }
    }

    public class PDTA : RiffChunk {
        public Dictionary<INST_ID, Tuple<string, PRESET_RANGE[]>> PresetList = new Dictionary<INST_ID, Tuple<string, PRESET_RANGE[]>>();
        public List<Tuple<string, INST_RANGE[]>> InstList = new List<Tuple<string, INST_RANGE[]>>();
        public List<SHDR> SampleList = new List<SHDR>();

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
            for (int i = 0; i < mPHDR.Count; i++) {
                var preset = mPHDR[i];
                int bagCount;
                if (i < mPHDR.Count - 1) {
                    bagCount = mPHDR[i + 1].bagIndex - preset.bagIndex;
                } else {
                    bagCount = mPBAG.Count - preset.bagIndex;
                }
                var list = new List<PRESET_RANGE>();
                for (int ib = 0, bagIdx = preset.bagIndex; ib < bagCount; ib++, bagIdx++) {
                    var bag = mPBAG[bagIdx];
                    var range = new PRESET_RANGE();
                    range.Init();
                    int genCount;
                    if (bagIdx < mPBAG.Count - 1) {
                        genCount = mPBAG[bagIdx + 1].genIndex - bag.genIndex;
                    } else {
                        genCount = mPGEN.Count - bag.genIndex;
                    }
                    for (int j = 0, genIdx = bag.genIndex; j < genCount; j++, genIdx++) {
                        var gen = mPGEN[genIdx];
                        switch (gen.genOper) {
                        case E_OPER.KEY_RANGE:
                            range.keyLow = (byte)(gen.genAmount & 0x7F);
                            range.keyHigh = (byte)((gen.genAmount >> 8) & 0x7F);
                            break;
                        case E_OPER.VEL_RANGE:
                            range.velLow = (byte)(gen.genAmount & 0x7F);
                            range.velHigh = (byte)((gen.genAmount >> 8) & 0x7F);
                            break;
                        case E_OPER.INITIAL_ATTENUATION:
                            range.gain = Math.Pow(10.0, -gen.genAmount / 200.0);
                            break;
                        case E_OPER.PAN:
                            range.pan = gen.genAmount / 500.0;
                            break;
                        case E_OPER.INSTRUMENT:
                            range.instId = gen.genAmount;
                            break;
                        default:
                            break;
                        }
                    }
                    if (0 <= range.instId) {
                        list.Add(range);
                    }
                }
                var id = new INST_ID();
                id.isDrum = (byte)(0 < (preset.bank & 0x80) ? 1 : 0);
                id.bankMSB = (byte)(preset.bank & 0x7F);
                id.programNo = (byte)preset.presetno;
                if (!PresetList.ContainsKey(id)) {
                    PresetList.Add(id, new Tuple<string, PRESET_RANGE[]>(
                        Encoding.ASCII.GetString(preset.name).Replace("\0", "").TrimEnd(), list.ToArray()));
                }
            }
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
                var global = new INST_RANGE();
                global.Init();
                var list = new List<INST_RANGE>();
                for (int ib = 0, bagIdx = inst.bagIndex; ib < bagCount; ib++, bagIdx++) {
                    var bag = mIBAG[bagIdx];
                    int genCount;
                    if (bagIdx < mIBAG.Count - 1) {
                        genCount = mIBAG[bagIdx + 1].genIndex - bag.genIndex;
                    } else {
                        genCount = mIGEN.Count - bag.genIndex;
                    }
                    var range = new INST_RANGE();
                    range.Init();
                    for (int j = 0, genIdx = bag.genIndex; j < genCount; j++, genIdx++) {
                        var gen = mIGEN[genIdx];
                        switch (gen.genOper) {
                        case E_OPER.KEY_RANGE:
                            range.keyLow = (byte)(gen.genAmount & 0x7F);
                            range.keyHigh = (byte)((gen.genAmount >> 8) & 0x7F);
                            break;
                        case E_OPER.VEL_RANGE:
                            range.velLow = (byte)(gen.genAmount & 0x7F);
                            range.velHigh = (byte)((gen.genAmount >> 8) & 0x7F);
                            break;
                        case E_OPER.INITIAL_ATTENUATION:
                            range.gain = Math.Pow(10.0, -gen.genAmount / 200.0);
                            break;
                        case E_OPER.PAN:
                            range.pan = gen.genAmount / 500.0;
                            break;
                        case E_OPER.COARSE_TUNE:
                            range.coarseTune = Math.Pow(2.0, gen.genAmount / 120.0);
                            break;
                        case E_OPER.FINETUNE:
                            range.fineTune = Math.Pow(2.0, gen.genAmount / 1200.0);
                            break;
                        case E_OPER.OVERRIDING_ROOTKEY:
                            range.rootKey = gen.genAmount;
                            break;
                        case E_OPER.SAMPLE_MODES:
                            range.loopEnable = 0 < (gen.genAmount & 1);
                            break;
                        case E_OPER.SAMPLE_ID:
                            range.sampleId = gen.genAmount;
                            break;
                        case E_OPER.ADDRS_OFFSET_START_LSB:
                            range.waveBegin |= (ushort)gen.genAmount;
                            break;
                        case E_OPER.ADDRS_OFFSET_START_MSB:
                            range.waveBegin |= (uint)((ushort)gen.genAmount << 16);
                            break;
                        case E_OPER.ADDRS_OFFSET_END_LSB:
                            range.waveEnd |= (ushort)gen.genAmount;
                            break;
                        case E_OPER.ADDRS_OFFSET_END_MSB:
                            range.waveEnd |= (uint)((ushort)gen.genAmount << 16);
                            break;
                        case E_OPER.ENV_VOL_ATTACK:
                            range.env.deltaA = Const.EnvelopeSpeed * Const.DeltaTime
                                / Math.Pow(2.0, gen.genAmount / 1200.0);
                            break;
                        case E_OPER.ENV_VOL_HOLD:
                            range.env.hold = Math.Pow(2.0, gen.genAmount / 1200.0);
                            break;
                        case E_OPER.ENV_VOL_DECAY:
                            range.env.deltaD = Const.EnvelopeSpeed * Const.DeltaTime
                                / Math.Pow(2.0, gen.genAmount / 1200.0);
                            break;
                        case E_OPER.ENV_VOL_SUSTAIN:
                            range.env.levelS = Math.Pow(10.0, -gen.genAmount / 200.0);
                            break;
                        case E_OPER.ENV_VOL_RELEASE:
                            range.env.deltaR = Const.EnvelopeSpeed * Const.DeltaTime
                                / Math.Pow(2.0, gen.genAmount / 1200.0);
                            break;
                        default:
                            break;
                        }
                    }
                    if (range.sampleId < 0) {
                        global = range;
                    } else {
                        // set global value
                        if (range.rootKey < 0) {
                            range.rootKey = global.rootKey;
                        }
                        if (range.coarseTune == 0.0) {
                            range.coarseTune = global.coarseTune;
                        }
                        if (range.fineTune == 0.0) {
                            range.fineTune = global.fineTune;
                        }
                        if (range.env.deltaA <= 0.0) {
                            range.env.deltaA = global.env.deltaA;
                        }
                        if (range.env.deltaD <= 0.0) {
                            range.env.deltaD = global.env.deltaD;
                        }
                        if (range.env.deltaR <= 0.0) {
                            range.env.deltaR = global.env.deltaR;
                        }
                        if (range.env.hold < 0.0) {
                            range.env.hold = global.env.hold;
                        }
                        if (range.env.levelS < 0.0) {
                            range.env.levelS = global.env.levelS;
                        }

                        // set default value
                        if (range.coarseTune == 0.0) {
                            range.coarseTune = 1.0;
                        }
                        if (range.fineTune == 0.0) {
                            range.fineTune = 1.0;
                        }
                        if (range.env.deltaA <= 0.0) {
                            range.env.deltaA = 1000 * Const.EnvelopeSpeed * Const.DeltaTime;
                        }
                        if (range.env.deltaD <= 0.0) {
                            range.env.deltaD = 1000 * Const.EnvelopeSpeed * Const.DeltaTime;
                        }
                        if (range.env.deltaR <= 0.0) {
                            range.env.deltaR = 1000 * Const.EnvelopeSpeed * Const.DeltaTime;
                        }
                        if (range.env.hold < 0.0) {
                            range.env.hold = 0.0;
                        }
                        if (range.env.levelS < 0.0) {
                            range.env.levelS = 1.0;
                        }
                        range.env.hold += Const.EnvelopeSpeed * Const.DeltaTime / range.env.deltaA;
                        //
                        list.Add(range);
                    }
                }
                InstList.Add(new Tuple<string, INST_RANGE[]>(
                    Encoding.ASCII.GetString(inst.name).Replace("\0", "").TrimEnd(), list.ToArray()));
            }
            mINST.Clear();
            mIBAG.Clear();
            mIGEN.Clear();
            mIMOD.Clear();
        }

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
                    SampleList.Add(Marshal.PtrToStructure<SHDR>(ptr + pos));
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
