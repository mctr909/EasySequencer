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
        private static extern void WriteWaveOutBuffer();
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
        private Task mTask;

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

            WaveOutOpen((uint)Const.SampleRate, 512);

            mTask = new Task(mainLoop, TaskCreationOptions.PreferFairness);
            mTask.Start();
        }

        public void Send(Event msg) {
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
                Channel[msg.Channel].ProgramChange(msg.ProgNo);
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
                    if (E_EVENT_TYPE.META == ev.Type) {
                        if (E_META_TYPE.TEMPO == ev.Meta.Type) {
                            bpm = ev.Meta.Tempo;
                        }
                    }
                    switch (ev.Type) {
                    case E_EVENT_TYPE.NOTE_OFF:
                        noteOff(ppFileOutSampler, mFileOutChannel[ev.Channel], ev.NoteNo, E_KEY_STATE.RELEASE);
                        break;
                    case E_EVENT_TYPE.NOTE_ON:
                        noteOn(ppFileOutSampler, mFileOutChannel[ev.Channel], ev.NoteNo, ev.Velocity);
                        break;
                    case E_EVENT_TYPE.CTRL_CHG:
                        mFileOutChannel[ev.Channel].CtrlChange(ev.CtrlType, ev.CtrlValue);
                        break;
                    case E_EVENT_TYPE.PROG_CHG:
                        mFileOutChannel[ev.Channel].ProgramChange(ev.ProgNo);
                        break;
                    case E_EVENT_TYPE.PITCH:
                        mFileOutChannel[ev.Channel].PitchBend(ev.Pitch);
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

                pSmpl->envEq.deltaA = 12000 * Const.DeltaTime;
                pSmpl->envEq.deltaD = 12000 * Const.DeltaTime;
                pSmpl->envEq.deltaR = 12000 * Const.DeltaTime;
                pSmpl->envEq.levelS = 1.0;
                pSmpl->envEq.hold = 0.0;

                pSmpl->eq.a0 = 0.0;
                pSmpl->eq.b0 = 0.0;
                pSmpl->eq.a1 = 0.0;
                pSmpl->eq.b1 = 0.0;
                pSmpl->eq.a2 = 0.0;
                pSmpl->eq.b2 = 0.0;
                pSmpl->eq.a3 = 0.0;
                pSmpl->eq.b3 = 0.0;
                pSmpl->eq.a4 = 0.0;
                pSmpl->eq.b4 = 0.0;
                pSmpl->eq.a5 = 0.0;
                pSmpl->eq.b5 = 0.0;
                pSmpl->eq.cutoff = 1.0;
                pSmpl->eq.resonance = 0.0;

                pSmpl->keyState = E_KEY_STATE.PRESS;

                return;
            }
        }

        private void loadDls(string dlsPath) {
            uint dlsSize = 0;
            var dlsPtr = LoadDLS(Marshal.StringToHGlobalAuto(dlsPath), out dlsSize);
            //var dls = new DLS.DLS(dlsPtr, dlsSize);
            //mInstList = dls.GetInstList();
            var sf2 = new SF2.SF2(dlsPath, dlsPtr, dlsSize);
            mInstList = sf2.GetInstList();
        }

        private void mainLoop() {
            while (true) {
                WriteWaveOutBuffer();
            }
        }
    }
}
