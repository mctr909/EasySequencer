using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using MIDI;

namespace WaveOut {
    unsafe public class Sender {
        [DllImport("WaveOut.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr LoadFile(IntPtr filePath, out uint size);
        [DllImport("WaveOut.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern void SystemValues(uint sampleRate, uint bufferLength);
        [DllImport("WaveOut.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool WaveOutOpen();
        [DllImport("WaveOut.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern void WaveOutClose();
        [DllImport("WaveOut.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern void FileOutOpen(IntPtr filePath);
        [DllImport("WaveOut.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern void FileOutClose();
        [DllImport("WaveOut.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern void FileOut();
        [DllImport("WaveOut.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int* GetActiveCountPtr();
        [DllImport("WaveOut.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern CHANNEL_PARAM** GetWaveOutChannelPtr();
        [DllImport("WaveOut.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern CHANNEL_PARAM** GetFileOutChannelPtr();
        [DllImport("WaveOut.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern SAMPLER** GetWaveOutSamplerPtr();
        [DllImport("WaveOut.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern SAMPLER** GetFileOutSamplerPtr();

        public const int CHANNEL_COUNT = 16;
        public const int SAMPLER_COUNT = 64;

        public static int* ActiveCountPtr = GetActiveCountPtr();

        private Channel[] mFileOutChannel;
        private Dictionary<INST_ID, INST_INFO> mInstList;

        public Channel[] Channel { get; private set; }
        public SAMPLER** ppWaveOutSampler { get; private set; }
        public SAMPLER** ppFileOutSampler { get; private set; }
        public static bool IsFileOutput { get; private set; }
        public int OutputTime;

        public Sender(string dlsPath) {
            uint fileSize = 0;
            var dlsPtr = LoadFile(Marshal.StringToHGlobalAuto(dlsPath), out fileSize);
            var dls = new DLS.DLS(dlsPtr, fileSize);
            mInstList = dls.GetInstList();
            //var sf2 = new SF2.SF2(dlsPath, dlsPtr, fileSize);
            //mInstList = sf2.GetInstList();

            SystemValues((uint)Const.SampleRate, 512);

            var ppChannel = GetWaveOutChannelPtr();
            ppWaveOutSampler = GetWaveOutSamplerPtr();
            Channel = new Channel[CHANNEL_COUNT];
            for (int i = 0; i < CHANNEL_COUNT; ++i) {
                Channel[i] = new Channel(mInstList, ppWaveOutSampler, ppChannel[i], i);
            }

            var ppFileOutChannel = GetFileOutChannelPtr();
            ppFileOutSampler = GetFileOutSamplerPtr();
            mFileOutChannel = new Channel[CHANNEL_COUNT];
            for (int i = 0; i < CHANNEL_COUNT; ++i) {
                mFileOutChannel[i] = new Channel(mInstList, ppFileOutSampler, ppFileOutChannel[i], i);
            }

            WaveOutOpen();
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
                double delta_sec = Const.DeltaTime * 512;
                double curTime = 0.0;
                double bpm = 120.0;
                IsFileOutput = true;
                FileOutOpen(Marshal.StringToHGlobalAuto(filePath));
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
                if (pSmpl->channelNum == ch.No && pSmpl->noteNum == noteNo) {
                    if (E_KEY_STATE.PURGE == keyState) {
                        pSmpl->state = E_KEY_STATE.PURGE;
                    } else {
                        if (!ch.Enable || ch.Hld < 64) {
                            pSmpl->state = E_KEY_STATE.RELEASE;
                        } else {
                            pSmpl->state = E_KEY_STATE.HOLD;
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
            foreach (var region in ch.Regions) {
                if (noteNo < region.keyLo || region.keyHi < noteNo || velocity < region.velLo || region.velHi < velocity) {
                    continue;
                }
                double pitch;
                var diffNote = noteNo - region.waveInfo.unityNote;
                if (diffNote < 0) {
                    pitch = 1.0 / Const.SemiTone[-diffNote];
                } else {
                    pitch = Const.SemiTone[diffNote];
                }
                for (var j = 0; j < SAMPLER_COUNT; ++j) {
                    var pSmpl = ppSmpl[j];
                    if (E_KEY_STATE.WAIT != pSmpl->state) {
                        continue;
                    }
                    pSmpl->channelNum = ch.No;
                    pSmpl->noteNum = noteNo;
                    pSmpl->waveInfo = region.waveInfo;
                    pSmpl->waveInfo.delta = region.waveInfo.delta * pitch;
                    pSmpl->index = 0.0;
                    pSmpl->time = 0.0;
                    pSmpl->velocity = velocity / 127.0;
                    pSmpl->egAmp = 0.0;
                    pSmpl->envAmp = region.env;
                    pSmpl->state = E_KEY_STATE.PRESS;
                    break;
                }
                break;
            }
        }
    }
}
