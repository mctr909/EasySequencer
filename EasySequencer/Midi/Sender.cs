using System.Runtime.InteropServices;

namespace MIDI {
    unsafe public class Sender {
        private SAMPLER** mppSampler = null;
        private Instruments mInst = null;

        [DllImport("WaveOut.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool WaveOutOpen(uint sampleRate, uint bufferLength);

        [DllImport("WaveOut.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern void WaveOutClose();

        [DllImport("WaveOut.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern CHANNEL** GetChannelPtr();

        [DllImport("WaveOut.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern SAMPLER** GetSamplerPtr();

        private const int CHANNEL_COUNT = 16;
        private const int SAMPLER_COUNT = 128;

        public Channel[] Channel { get; private set; }

        public Sender(string dlsPath) {
            var ppChannel = GetChannelPtr();
            mppSampler = GetSamplerPtr();

            mInst = new Instruments(dlsPath, Const.SampleRate);

            Channel = new Channel[CHANNEL_COUNT];
            for (int i = 0; i < CHANNEL_COUNT; ++i) {
                Channel[i] = new Channel(mInst, ppChannel[i], i);
            }

            WaveOutOpen((uint)Const.SampleRate, 256);
        }

        public void Send(Message msg) {
            switch (msg.Type) {
            case EVENT_TYPE.NOTE_OFF:
                noteOff(Channel[msg.Channel], msg.V1);
                break;

            case EVENT_TYPE.NOTE_ON:
                noteOn(Channel[msg.Channel], msg.V1, msg.V2);
                break;

            case EVENT_TYPE.CTRL_CHG:
                Channel[msg.Channel].CtrlChange(msg.V1, msg.V2);
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

        private void noteOff(Channel ch, byte noteNo) {
            for (var i = 0; i < SAMPLER_COUNT; ++i) {
                if (mppSampler[i]->channelNo == ch.No && mppSampler[i]->noteNo == noteNo) {
                    if (!ch.Enable || ch.Hld < 64) {
                        ch.KeyBoard[noteNo] = KEY_STATUS.OFF;
                    }
                    else {
                        ch.KeyBoard[noteNo] = KEY_STATUS.HOLD;
                    }
                    mppSampler[i]->onKey = false;
                }
            }
        }

        private void noteOn(Channel ch, byte noteNo, byte velocity) {
            noteOff(ch, noteNo);

            if (0 == velocity) {
                return;
            }

            var wave = ch.WaveInfo[noteNo];
            if (uint.MaxValue == wave.pcmAddr) {
                return;
            }

            for (var i = 0; i < SAMPLER_COUNT; ++i) {
                var pSmpl = mppSampler[i];
                if (pSmpl->isActive) {
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
                }
                else {
                    pSmpl->delta = wave.delta * Const.SemiTone[diffNote];
                }

                pSmpl->index = 0.0;
                pSmpl->time = 0.0;

                pSmpl->tarAmp = velocity / 127.0;
                pSmpl->curAmp = 0.0;

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

                pSmpl->eq.cutoff = 1.0;
                pSmpl->eq.resonance = 0.0;
                pSmpl->eq.pole00 = 0.0;
                pSmpl->eq.pole01 = 0.0;
                pSmpl->eq.pole02 = 0.0;
                pSmpl->eq.pole03 = 0.0;
                pSmpl->eq.pole10 = 0.0;
                pSmpl->eq.pole11 = 0.0;
                pSmpl->eq.pole12 = 0.0;
                pSmpl->eq.pole13 = 0.0;

                pSmpl->onKey = true;
                pSmpl->isActive = true;

                ch.KeyBoard[noteNo] = KEY_STATUS.ON;

                return;
            }
        }
    }
}