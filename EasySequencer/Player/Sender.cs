using System;
using System.IO;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using SMF;
using EasySequencer;

namespace Player {
    unsafe public class Sender {
        #region MidiSender.dll
        [DllImport("MidiSender.dll")]
        private static extern CHANNEL_PARAM** midi_GetChannelParamPtr();
        [DllImport("MidiSender.dll")]
        private static extern IntPtr midi_GetWavFileOutProgressPtr();
        [DllImport("MidiSender.dll")]
        private static extern void midi_CreateChannels(INST_LIST* list, IntPtr ppSmpl, NOTE** ppNote, IntPtr ppCh, int samplerCount);
        [DllImport("MidiSender.dll")]
        private static extern void midi_Send(byte *pMsg);
        [DllImport("MidiSender.dll")]
        private static extern void midi_WavFileOut(
            IntPtr filePath,
            IntPtr pWaveTable,
            INST_LIST* list,
            uint sampleRate,
            uint bitRate,
            IntPtr pEvents,
            uint eventSize,
            uint baseTick
        );
        #endregion

        #region WaveOut.dll
        [DllImport("WaveOut.dll")]
        private static extern IntPtr waveout_GetActiveSamplersPtr();
        [DllImport("WaveOut.dll")]
        private static extern IntPtr waveout_GetChannelPtr();
        [DllImport("WaveOut.dll")]
        private static extern NOTE** waveout_GetNotePtr();
        [DllImport("WaveOut.dll")]
        private static extern IntPtr waveout_GetSamplerPtr();
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
        private static extern void waveout_Open();
        [DllImport("WaveOut.dll")]
        private static extern void waveout_Close();
        [DllImport("WaveOut.dll")]
        private static extern void waveout_Dispose();
        #endregion

        public static readonly int SampleRate = 44100;
        public static readonly double DeltaTime = 1.0 / SampleRate;
        public static readonly double AttackSpeed = 54.3656;
        public static readonly double DecaySpeed = 15;
        public static readonly double ReleaseSpeed = 0.1;

        public static int CHANNEL_COUNT = 16;
        public static int SAMPLER_COUNT = 32;
        public static bool IsFileOutput { get; private set; }

        public static int ActiveCount {
            get { return Marshal.PtrToStructure<int>(mpActiveCountPtr); }
        }

        private static IntPtr mpActiveCountPtr = waveout_GetActiveSamplersPtr();

        private IntPtr mpWaveTable;
        private INST_LIST* mpInstList;
        private IntPtr mppSamplers;
        private NOTE** mppNotes;
        private IntPtr mppChannels;
        private CHANNEL_PARAM** mppChParam;

        public int InstCount {
            get { return mpInstList->instCount; }
        }
        public INST_REC Instruments(int num) {
            return *mpInstList->ppInst[num];
        }
        public NOTE Note(int num) {
            return *mppNotes[num];
        }
        public CHANNEL_PARAM Channel(int num) {
            return *mppChParam[num];
        }
        public void MuteChannel(int num, bool mute) {
            mppChParam[num]->Enable = !mute;
        }
        public void DrumChannel(int num, bool isDrum) {
            mppChParam[num]->InstId.isDrum = (byte)(isDrum ? 1 : 0);
        }
        public void OscChannel(int num, bool isOsc) {
            mppChParam[num]->IsOsc = isOsc;
        }

        public Sender(string dlsPath) {
            uint fileSize = 0;
            mpWaveTable = waveout_LoadWaveTable(Marshal.StringToHGlobalAuto(dlsPath), out fileSize);
            mpInstList = (INST_LIST*)Marshal.AllocHGlobal(Marshal.SizeOf<INST_LIST>());
            var dls = new DLS.DLS(mpWaveTable, fileSize);
            dls.GetInstList(mpInstList);
            //var sf2 = new SF2.SF2(dlsPath, mpWaveTable, fileSize);
            //sf2.GetInstList(mpInstList);
            waveout_SystemValues(SampleRate, 16, 220, 50, CHANNEL_COUNT, SAMPLER_COUNT);
            mppChannels = waveout_GetChannelPtr();
            mppSamplers = waveout_GetSamplerPtr();
            mppNotes = waveout_GetNotePtr();
            midi_CreateChannels(mpInstList, mppSamplers, mppNotes, mppChannels, SAMPLER_COUNT);
            mppChParam = midi_GetChannelParamPtr();
            waveout_Open();
        }

        public void Send(Event msg) {
            fixed (byte* ptr = &msg.Data[0]) {
                midi_Send(ptr);
            }
        }

        public void FileOut(string filePath, SMF.SMF smf) {
            IsFileOutput = true;
            var prog = midi_GetWavFileOutProgressPtr();
            *(int*)prog = 0;
            var fm = new StatusWindow(smf.MaxTime, prog);
            fm.Show();
            Task.Factory.StartNew(() => {
                var ms = new MemoryStream();
                var bw = new BinaryWriter(ms);
                foreach (var ev in smf.EventList) {
                    bw.Write(ev.Tick);
                    bw.Write(ev.Data);
                }
                var evArr = ms.ToArray();
                fixed (byte* evPtr = &evArr[0]) {
                    midi_WavFileOut(Marshal.StringToHGlobalAuto(filePath), mpWaveTable, mpInstList,
                        44100, 16, (IntPtr)evPtr, (uint)evArr.Length, 960);
                }
                IsFileOutput = false;
            });
        }
    }
}
