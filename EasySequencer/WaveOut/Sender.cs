using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using MIDI;

namespace WaveOut {
    unsafe public class Sender {
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
        public const int SAMPLER_COUNT = 256;

        private Instruments mInstruments = null;
        private Channel[] mFileOutChannel;

        public Channel[] Channel { get; private set; }
        public SAMPLER** ppWaveOutSampler { get; private set; }
        public SAMPLER** ppFileOutSampler { get; private set; }
        public static bool IsFileOutput { get; private set; }
        public int OutputTime;

        public Sender(string dlsPath) {
            mInstruments = new Instruments(dlsPath);

            var ppChannel = GetWaveOutChannelPtr((uint)Const.SampleRate);
            ppWaveOutSampler = GetWaveOutSamplerPtr(SAMPLER_COUNT);
            Channel = new Channel[CHANNEL_COUNT];
            for (int i = 0; i < CHANNEL_COUNT; ++i) {
                Channel[i] = new Channel(mInstruments, ppWaveOutSampler, ppChannel[i], i);
            }

            var ppFileOutChannel = GetFileOutChannelPtr((uint)Const.SampleRate);
            ppFileOutSampler = GetFileOutSamplerPtr(SAMPLER_COUNT);
            mFileOutChannel = new Channel[CHANNEL_COUNT];
            for (int i = 0; i < CHANNEL_COUNT; ++i) {
                mFileOutChannel[i] = new Channel(mInstruments, ppFileOutSampler, ppFileOutChannel[i], i);
            }

            WaveOutOpen((uint)Const.SampleRate, 256);
        }

        public void Send(Message msg) {
            switch (msg.Type) {
            case EVENT_TYPE.NOTE_OFF:
                noteOff(ppWaveOutSampler, Channel[msg.Channel], msg.V1, E_KEY_STATE.RELEASE);
                break;

            case EVENT_TYPE.NOTE_ON:
                noteOn(ppWaveOutSampler, Channel[msg.Channel], msg.V1, msg.V2);
                break;

            case EVENT_TYPE.CTRL_CHG:
                Channel[msg.Channel].CtrlChange((CTRL_TYPE)msg.V1, msg.V2);
                break;

            case EVENT_TYPE.PRGM_CHG:
                Channel[msg.Channel].ProgramChange(msg.V1);
                break;

            case EVENT_TYPE.PITCH:
                Channel[msg.Channel].PitchBend(msg.V1, msg.V2);
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
                    if (EVENT_TYPE.META == type) {
                        if (META_TYPE.TEMPO == msg.Meta.Type) {
                            bpm = msg.Meta.Tempo;
                        }
                    }

                    switch (msg.Type) {
                    case EVENT_TYPE.NOTE_OFF:
                        noteOff(ppFileOutSampler, mFileOutChannel[msg.Channel], msg.V1, E_KEY_STATE.RELEASE);
                        break;

                    case EVENT_TYPE.NOTE_ON:
                        noteOn(ppFileOutSampler, mFileOutChannel[msg.Channel], msg.V1, msg.V2);
                        break;

                    case EVENT_TYPE.CTRL_CHG:
                        mFileOutChannel[msg.Channel].CtrlChange((CTRL_TYPE)msg.V1, msg.V2);
                        break;

                    case EVENT_TYPE.PRGM_CHG:
                        mFileOutChannel[msg.Channel].ProgramChange(msg.V1);
                        break;

                    case EVENT_TYPE.PITCH:
                        mFileOutChannel[msg.Channel].PitchBend(msg.V1, msg.V2);
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
            if (uint.MaxValue == wave.pcmAddr) {
                return;
            }

            for (var i = 0; i < SAMPLER_COUNT; ++i) {
                var pSmpl = ppSmpl[i];
                if (E_KEY_STATE.WAIT != pSmpl->keyState) {
                    continue;
                }

                pSmpl->channelNo = ch.No;
                pSmpl->noteNo = noteNo;

                pSmpl->pcmAddr = wave.pcmAddr;
                pSmpl->pcmLength = wave.pcmLength;

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
    }
}