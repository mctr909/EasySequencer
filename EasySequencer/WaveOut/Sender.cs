using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using MIDI;
using DLS;

namespace WaveOut {
    unsafe public class Sender {
        [DllImport("WaveOut.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr LoadDLS(IntPtr filePath, out uint size);

        [DllImport("WaveOut.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool WaveOutOpen(uint sampleRate, uint bufferLength);

        [DllImport("WaveOut.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern void WaveOutClose();

        [DllImport("WaveOut.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern void FileOutOpen(IntPtr filePath, uint bufferLength);

        [DllImport("WaveOut.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern void FileOutClose();

        [DllImport("WaveOut.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern void FileOut();

        [DllImport("WaveOut.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern CHANNEL_PARAM** GetWaveOutChannelPtr(uint sampleRate);

        [DllImport("WaveOut.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern CHANNEL_PARAM** GetFileOutChannelPtr(uint sampleRate);

        [DllImport("WaveOut.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern SAMPLER** GetWaveOutSamplerPtr(uint samplers);

        [DllImport("WaveOut.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern SAMPLER** GetFileOutSamplerPtr(uint samplers);

        public const int CHANNEL_COUNT = 16;
        public const int SAMPLER_COUNT = 128;

        private Channel[] mFileOutChannel;
        private Dictionary<INST_ID, INST_INFO> mInstList;

        public Channel[] Channel { get; private set; }
        public SAMPLER** ppWaveOutSampler { get; private set; }
        public SAMPLER** ppFileOutSampler { get; private set; }
        public static bool IsFileOutput { get; private set; }
        public int OutputTime;

        public Sender(string dlsPath) {
            loadDls(dlsPath);

            var ppChannel = GetWaveOutChannelPtr((uint)Const.SampleRate);
            ppWaveOutSampler = GetWaveOutSamplerPtr(SAMPLER_COUNT);
            Channel = new Channel[CHANNEL_COUNT];
            for (int i = 0; i < CHANNEL_COUNT; ++i) {
                Channel[i] = new Channel(mInstList, ppWaveOutSampler, ppChannel[i], i);
            }

            var ppFileOutChannel = GetFileOutChannelPtr((uint)Const.SampleRate);
            ppFileOutSampler = GetFileOutSamplerPtr(SAMPLER_COUNT);
            mFileOutChannel = new Channel[CHANNEL_COUNT];
            for (int i = 0; i < CHANNEL_COUNT; ++i) {
                mFileOutChannel[i] = new Channel(mInstList, ppFileOutSampler, ppFileOutChannel[i], i);
            }

            WaveOutOpen((uint)Const.SampleRate, 1024);
        }

        public void Send(Message msg) {
            switch (msg.Type) {
            case E_EVENT_TYPE.NOTE_OFF:
                noteOff(ppWaveOutSampler, Channel[msg.Channel], msg.NoteNo, E_KEY_STATE.RELEASE);
                break;

            case E_EVENT_TYPE.NOTE_ON:
                noteOn(ppWaveOutSampler, Channel[msg.Channel], msg.NoteNo, msg.Velocity);
                break;

            case E_EVENT_TYPE.CTRL_CHG:
                Channel[msg.Channel].CtrlChange(msg.CtrlType, msg.CtrlValue);
                break;

            case E_EVENT_TYPE.PROG_CHG:
                Channel[msg.Channel].ProgramChange(msg.ProgramNo);
                break;

            case E_EVENT_TYPE.PITCH:
                Channel[msg.Channel].PitchBend(msg.Pitch);
                break;

            default:
                break;
            }
        }

        public void FileOut(string filePath, Event[] events, int ticks) {
            Task.Factory.StartNew(() => {
                double delta_sec = Const.DeltaTime * 256;
                double curTime = 0.0;
                double bpm = 120.0;

                IsFileOutput = true;
                FileOutOpen(Marshal.StringToHGlobalAuto(filePath), 256);

                OutputTime = 0;

                foreach (var ev in events) {
                    var eventTime = (double)ev.Time / ticks;
                    while (curTime < eventTime) {
                        FileOut();
                        curTime += bpm * delta_sec / 60.0;
                        OutputTime = (int)curTime;
                    }

                    var msg = ev.Message;
                    var type = msg.Type;
                    if (E_EVENT_TYPE.META == type) {
                        if (E_META_TYPE.TEMPO == msg.Meta.Type) {
                            bpm = msg.Meta.Tempo;
                        }
                    }

                    switch (msg.Type) {
                    case E_EVENT_TYPE.NOTE_OFF:
                        noteOff(ppFileOutSampler, mFileOutChannel[msg.Channel], msg.NoteNo, E_KEY_STATE.RELEASE);
                        break;

                    case E_EVENT_TYPE.NOTE_ON:
                        noteOn(ppFileOutSampler, mFileOutChannel[msg.Channel], msg.NoteNo, msg.Velocity);
                        break;

                    case E_EVENT_TYPE.CTRL_CHG:
                        mFileOutChannel[msg.Channel].CtrlChange(msg.CtrlType, msg.CtrlValue);
                        break;

                    case E_EVENT_TYPE.PROG_CHG:
                        mFileOutChannel[msg.Channel].ProgramChange(msg.ProgramNo);
                        break;

                    case E_EVENT_TYPE.PITCH:
                        mFileOutChannel[msg.Channel].PitchBend(msg.Pitch);
                        break;

                    default:
                        break;
                    }
                }

                FileOutClose();
                IsFileOutput = false;
            });
        }

        private void noteOff(SAMPLER** ppSmpl, Channel ch, byte noteNo, E_KEY_STATE keyState) {
            for (var i = 0; i < SAMPLER_COUNT; ++i) {
                var pSmpl = ppSmpl[i];
                if (pSmpl->channelNo == ch.No && pSmpl->noteNo == noteNo) {
                    if (E_KEY_STATE.PURGE == keyState) {
                        pSmpl->keyState = E_KEY_STATE.PURGE;
                    } else {
                        if (!ch.Enable || ch.Hld < 64) {
                            pSmpl->keyState = E_KEY_STATE.RELEASE;
                        } else {
                            pSmpl->keyState = E_KEY_STATE.HOLD;
                        }
                    }
                }
            }
        }

        private void noteOn(SAMPLER** ppSmpl, Channel ch, byte noteNo, byte velocity) {
            if (0 == velocity) {
                noteOff(ppSmpl, ch, noteNo, E_KEY_STATE.RELEASE);
                return;
            } else {
                noteOff(ppSmpl, ch, noteNo, E_KEY_STATE.PURGE);
            }

            var wave = ch.WaveInfo[noteNo];
            if (uint.MaxValue == wave.buffOfs) {
                return;
            }

            for (var i = 0; i < SAMPLER_COUNT; ++i) {
                var pSmpl = ppSmpl[i];
                if (E_KEY_STATE.WAIT != pSmpl->keyState) {
                    continue;
                }

                pSmpl->channelNo = ch.No;
                pSmpl->noteNo = noteNo;
                pSmpl->buffOfs = wave.buffOfs;

                pSmpl->gain = wave.gain;

                var diffNote = noteNo - wave.unityNote;
                if (diffNote < 0) {
                    pSmpl->delta = wave.delta / Const.SemiTone[-diffNote];
                } else {
                    pSmpl->delta = wave.delta * Const.SemiTone[diffNote];
                }

                pSmpl->index = 0.0;
                pSmpl->time = 0.0;

                pSmpl->velocity = velocity / 127.0;
                pSmpl->amp = 0.0;

                pSmpl->loop = wave.loop;

                pSmpl->envAmp = wave.envAmp;

                pSmpl->envEq.levelA = 1.0;
                pSmpl->envEq.levelD = 1.0;
                pSmpl->envEq.levelS = 1.0;
                pSmpl->envEq.levelR = 1.0;
                pSmpl->envEq.deltaA = 1000 * Const.DeltaTime;
                pSmpl->envEq.deltaD = 1000 * Const.DeltaTime;
                pSmpl->envEq.deltaR = 1000 * Const.DeltaTime;
                pSmpl->envEq.hold = 0.0;

                pSmpl->eq.a0 = 0.0;
                pSmpl->eq.b0 = 0.0;
                pSmpl->eq.a1 = 0.0;
                pSmpl->eq.b1 = 0.0;
                pSmpl->eq.a2 = 0.0;
                pSmpl->eq.b2 = 0.0;
                pSmpl->eq.a3 = 0.0;
                pSmpl->eq.b3 = 0.0;
                pSmpl->eq.cutoff = 1.0;
                pSmpl->eq.resonance = 0.0;

                pSmpl->keyState = E_KEY_STATE.PRESS;

                return;
            }
        }

        private void loadDls(string dlsPath) {
            uint dlsSize = 0;
            var dlsPtr = LoadDLS(Marshal.StringToHGlobalAuto(dlsPath), out dlsSize);
            var dls = new File(dlsPtr, dlsSize);

            mInstList = new Dictionary<INST_ID, INST_INFO>();

            foreach (var inst in dls.instruments.List) {
                var envAmp = new ENVELOPE();
                if (null != inst.articulations) {
                    envAmp.levelA = 0.0;
                    envAmp.levelD = 1.0;
                    envAmp.levelS = 1.0;
                    envAmp.levelR = 0.0;
                    envAmp.deltaA = 1.0;
                    envAmp.deltaD = 1.0;
                    envAmp.deltaR = 16.0;
                    envAmp.hold = 0.0;
                    var holdTime = 0.0;

                    foreach (var conn in inst.articulations.art.List) {
                        if (SRC_TYPE.NONE != conn.source) {
                            continue;
                        }

                        switch (conn.destination) {
                            case DST_TYPE.EG1_ATTACK_TIME:
                                envAmp.deltaA = 64 * Const.DeltaTime / ART.GetValue(conn);
                                holdTime += ART.GetValue(conn);
                                break;
                            case DST_TYPE.EG1_DECAY_TIME:
                                envAmp.deltaD = 24 * Const.DeltaTime / ART.GetValue(conn);
                                break;
                            case DST_TYPE.EG1_RELEASE_TIME:
                                envAmp.deltaR = 24 * Const.DeltaTime / ART.GetValue(conn);
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

                    if (1.0 < envAmp.deltaA) {
                        envAmp.deltaA = 1.0;
                    }
                    if (1.0 < envAmp.deltaD) {
                        envAmp.deltaD = 1.0;
                    }
                    if (1.0 < envAmp.deltaR) {
                        envAmp.deltaR = 1.0;
                    }
                }

                var waveInfo = new WAVE_INFO[128];
                for (var noteNo = 0; noteNo < waveInfo.Length; ++noteNo) {
                    RGN_ region = null;
                    foreach (var rgn in inst.regions.List) {
                        if (rgn.pHeader->key.low <= noteNo && noteNo <= rgn.pHeader->key.high) {
                            region = rgn;
                            break;
                        }
                    }

                    if (null == region) {
                        waveInfo[noteNo].buffOfs = uint.MaxValue;
                        continue;
                    }

                    if (null != region.articulations) {
                        envAmp.levelA = 0.0;
                        envAmp.levelD = 1.0;
                        envAmp.levelS = 1.0;
                        envAmp.levelR = 0.0;
                        envAmp.deltaA = 1.0;
                        envAmp.deltaD = 1.0;
                        envAmp.deltaR = 16.0;
                        envAmp.hold = 0.0;
                        var holdTime = 0.0;

                        foreach (var conn in region.articulations.art.List) {
                            if (SRC_TYPE.NONE != conn.source)
                                continue;
                            switch (conn.destination) {
                                case DST_TYPE.EG1_ATTACK_TIME:
                                    envAmp.deltaA = 64 * Const.DeltaTime / ART.GetValue(conn);
                                    holdTime += ART.GetValue(conn);
                                    break;
                                case DST_TYPE.EG1_DECAY_TIME:
                                    envAmp.deltaD = 24 * Const.DeltaTime / ART.GetValue(conn);
                                    break;
                                case DST_TYPE.EG1_RELEASE_TIME:
                                    envAmp.deltaR = 24 * Const.DeltaTime / ART.GetValue(conn);
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

                        if (1.0 < envAmp.deltaA) {
                            envAmp.deltaA = 1.0;
                        }
                        if (1.0 < envAmp.deltaD) {
                            envAmp.deltaD = 1.0;
                        }
                        if (1.0 < envAmp.deltaR) {
                            envAmp.deltaR = 1.0;
                        }
                    }

                    waveInfo[noteNo].envAmp = envAmp;
                    var wave = dls.wavePool.List[(int)region.pWaveLink->tableIndex];
                    waveInfo[noteNo].buffOfs = wave.buffAddr - (uint)dlsPtr.ToInt64();
                    waveInfo[noteNo].samples = wave.buffSize / wave.pFormat->blockAlign;

                    if (null == region.pSampler) {
                        waveInfo[noteNo].unityNote = (byte)wave.pSampler->unityNote;
                        waveInfo[noteNo].delta
                            = Math.Pow(2.0, wave.pSampler->fineTune / 1200.0)
                            * wave.pFormat->sampleRate
                        ;
                        waveInfo[noteNo].gain = wave.pSampler->Gain / 32768.0;
                        if (0 < wave.pSampler->loopCount) {
                            waveInfo[noteNo].loop.begin = wave.pLoops[0].start;
                            waveInfo[noteNo].loop.length = wave.pLoops[0].length;
                            waveInfo[noteNo].loop.enable = true;
                        } else {
                            waveInfo[noteNo].loop.begin = 0;
                            waveInfo[noteNo].loop.length = waveInfo[noteNo].samples;
                            waveInfo[noteNo].loop.enable = false;
                        }
                    } else {
                        waveInfo[noteNo].unityNote = (byte)region.pSampler->unityNote;
                        waveInfo[noteNo].delta
                            = Math.Pow(2.0, region.pSampler->fineTune / 1200.0)
                            * wave.pFormat->sampleRate
                        ;
                        waveInfo[noteNo].gain = region.pSampler->Gain / 32768.0;
                        if (0 < region.pSampler->loopCount) {
                            waveInfo[noteNo].loop.begin = region.pLoops[0].start;
                            waveInfo[noteNo].loop.length = region.pLoops[0].length;
                            waveInfo[noteNo].loop.enable = true;
                        } else if (0 < wave.pSampler->loopCount) {
                            waveInfo[noteNo].loop.begin = wave.pLoops[0].start;
                            waveInfo[noteNo].loop.length = wave.pLoops[0].length;
                            waveInfo[noteNo].loop.enable = true;
                        } else {
                            waveInfo[noteNo].loop.begin = 0;
                            waveInfo[noteNo].loop.length = waveInfo[noteNo].samples;
                            waveInfo[noteNo].loop.enable = false;
                            waveInfo[noteNo].envAmp.deltaR = Const.DeltaTime * waveInfo[noteNo].delta / waveInfo[noteNo].samples;
                        }
                    }
                }

                var id = new INST_ID();
                id.isDrum = inst.pHeader->locale.bankFlags;
                id.programNo = inst.pHeader->locale.programNo;
                id.bankMSB = inst.pHeader->locale.bankMSB;
                id.bankLSB = inst.pHeader->locale.bankLSB;

                var instInfo = new INST_INFO();
                instInfo.catgory = inst.category;
                instInfo.name = inst.name;
                instInfo.waves = waveInfo;
                mInstList.Add(id, instInfo);
            }
        }
    }
}
