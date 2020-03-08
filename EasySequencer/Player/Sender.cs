using System;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using MIDI;

namespace Player {
    unsafe public class Sender {
        [DllImport("MidiSender.dll")]
        private static extern CHANNEL** wavfileout_GetChannelPtr();
        [DllImport("MidiSender.dll")]
        private static extern SAMPLER** wavfileout_GetSamplerPtr();
        [DllImport("MidiSender.dll")]
        private static extern void wavfileout_Open(IntPtr filePath, IntPtr pWaveTable, uint sampleRate, uint bitRate);
        [DllImport("MidiSender.dll")]
        private static extern void wavfileout_Close();
        [DllImport("MidiSender.dll")]
        private static extern void wavfileout_Write();
        [DllImport("MidiSender.dll")]
        private static extern CHANNEL_PARAM** midi_GetChannelParamPtr();
        [DllImport("MidiSender.dll")]
        private static extern void midi_CreateChannels(INST_LIST* list, SAMPLER** ppSmpl, CHANNEL** ppCh, int samplerCount);
        [DllImport("MidiSender.dll")]
        private static extern void midi_Send(byte *pMsg);

        [DllImport("WaveOut.dll")]
        private static extern int* waveout_GetActiveSamplersPtr();
        [DllImport("WaveOut.dll")]
        private static extern CHANNEL** waveout_GetChannelPtr();
        [DllImport("WaveOut.dll")]
        private static extern SAMPLER** waveout_GetSamplerPtr();
        [DllImport("WaveOut.dll")]
        private static extern IntPtr waveout_LoadWaveTable(IntPtr filePath, out uint size);
        [DllImport("WaveOut.dll")]
        private static extern void waveout_SystemValues(
            int sampleRate,
            int bits,
            int bufferLength,
            int bufferCount,
            int channelCount,
            int samplerCount
        );
        [DllImport("WaveOut.dll")]
        private static extern bool waveout_Open();
        [DllImport("WaveOut.dll")]
        private static extern void waveout_Close();
        [DllImport("WaveOut.dll")]
        private static extern void waveout_Dispose();

        public static readonly int SampleRate = 44100;
        public static readonly double DeltaTime = 1.0 / SampleRate;
        public static readonly double EnvelopeSpeed = 24.0;

        public static int CHANNEL_COUNT = 16;
        public static int SAMPLER_COUNT = 64;
        public static int* ActiveCountPtr = waveout_GetActiveSamplersPtr();
        public static bool IsFileOutput { get; private set; }
        public static int OutputTime;

        public CHANNEL_PARAM** Channel { get; private set; }
        public INST_LIST* InstList { get; private set; }
        public SAMPLER** ppWaveOutSampler { get; private set; }

        private IntPtr mpWaveTable;
        private CHANNEL** mppChannels;

        public Sender(string dlsPath) {
            uint fileSize = 0;
            mpWaveTable = waveout_LoadWaveTable(Marshal.StringToHGlobalAuto(dlsPath), out fileSize);

            InstList = (INST_LIST*)Marshal.AllocHGlobal(Marshal.SizeOf<INST_LIST>());
            var dls = new DLS.DLS(mpWaveTable, fileSize);
            dls.GetInstList(InstList);

            //var sf2 = new SF2.SF2(dlsPath, mpWaveTable, fileSize);
            //sf2.GetInstList(mInstList);

            //
            waveout_SystemValues(SampleRate, 32, 1024, 8, CHANNEL_COUNT, SAMPLER_COUNT);
            mppChannels = waveout_GetChannelPtr();
            ppWaveOutSampler = waveout_GetSamplerPtr();
            midi_CreateChannels(InstList, ppWaveOutSampler, mppChannels, SAMPLER_COUNT);
            Channel = midi_GetChannelParamPtr();
            waveout_Open();
        }

        public void Send(Event msg) {
            fixed (byte* ptr = &msg.Data[0]) {
                midi_Send(ptr);
            }
        }

        public void FileOut(string filePath, Event[] events, int ticks) {
            IsFileOutput = true;

            wavfileout_Open(Marshal.StringToHGlobalAuto(filePath), mpWaveTable, 44100, 16);
            var ppFileOutSampler = wavfileout_GetSamplerPtr();
            var ppFileOutChannel = wavfileout_GetChannelPtr();

            double delta_sec = Sender.DeltaTime * 512;
            double curTime = 0.0;
            double bpm = 120.0;

            Task.Factory.StartNew(() => {
                foreach (var ev in events) {
                    var eventTime = (double)ev.Time / ticks;
                    while (curTime < eventTime) {
                        wavfileout_Write();
                        curTime += bpm * delta_sec / 60.0;
                        OutputTime = (int)curTime;
                    }
                    if (E_EVENT_TYPE.META == ev.Type) {
                        if (E_META_TYPE.TEMPO == ev.Meta.Type) {
                            bpm = ev.Meta.Tempo;
                        }
                    }
                    //switch (ev.Type) {
                    //case E_EVENT_TYPE.NOTE_OFF:
                    //    fileOutChannel[ev.Channel].NoteOff(ev.NoteNo, E_KEY_STATE.RELEASE);
                    //    break;
                    //case E_EVENT_TYPE.NOTE_ON:
                    //    fileOutChannel[ev.Channel].NoteOn(ev.NoteNo, ev.Velocity);
                    //    break;
                    //case E_EVENT_TYPE.CTRL_CHG:
                    //    fileOutChannel[ev.Channel].CtrlChange(ev.CtrlType, ev.CtrlValue);
                    //    break;
                    //case E_EVENT_TYPE.PROG_CHG:
                    //    fileOutChannel[ev.Channel].ProgramChange(ev.ProgNo);
                    //    break;
                    //case E_EVENT_TYPE.PITCH:
                    //    fileOutChannel[ev.Channel].PitchBend(ev.Pitch);
                    //    break;
                    //default:
                    //    break;
                    //}
                }
                wavfileout_Close();
                IsFileOutput = false;
            });
        }
    }
}
