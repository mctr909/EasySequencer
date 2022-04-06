using System;
using System.IO;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using EasySequencer;
using Instruments;

namespace Player {
    unsafe public class Sender {
        #region WaveOut.dll
        [DllImport("WaveOut.dll")]
        private static extern CHANNEL_PARAM** message_getChannelParamPtr();
        [DllImport("WaveOut.dll")]
        private static extern void message_send(byte* pMsg);

        [DllImport("WaveOut.dll")]
        private static extern void waveout_test(IntPtr filePath);

        [DllImport("WaveOut.dll")]
        private static extern IntPtr waveout_getFileOutProgressPtr();
        [DllImport("WaveOut.dll")]
        private static extern void waveout_fileOut(
            IntPtr filePath,
            uint sampleRate,
            uint bitRate,
            IntPtr pEvents,
            uint eventSize,
            uint baseTick
        );

        [DllImport("WaveOut.dll")]
        private static extern INST_LIST* waveout_systemValues(
            IntPtr filePath,
            int sampleRate,
            int bits,
            int bufferLength,
            int bufferCount
        );
        [DllImport("WaveOut.dll")]
        private static extern IntPtr waveout_getActiveSamplersPtr();
        [DllImport("WaveOut.dll")]
        private static extern void waveout_open();
        [DllImport("WaveOut.dll")]
        private static extern void waveout_close();
        #endregion

        public static readonly int SampleRate = 48000;
        public static readonly double DeltaTime = 1.0 / SampleRate;
        public static readonly double AttackSpeed = 12.0;
        public static readonly double DecaySpeed = 12.0;
        public static readonly double ReleaseSpeed = 12.0;

        public static int CHANNEL_COUNT = 16;
        public static int SAMPLER_COUNT = 64;
        public static bool IsFileOutput { get; private set; }

        public static int ActiveCount {
            get { return Marshal.PtrToStructure<int>(mpActiveCountPtr); }
        }

        private static IntPtr mpActiveCountPtr = waveout_getActiveSamplersPtr();

        private INST_LIST* mpInstList;
        private CHANNEL_PARAM** mppChParam;

        public int InstCount {
            get { return mpInstList->count; }
        }
        public INST_INFO Instruments(int num) {
            return *mpInstList->ppData[num];
        }
        public CHANNEL_PARAM Channel(int num) {
            return *mppChParam[num];
        }
        public void MuteChannel(int num, bool mute) {
            mppChParam[num]->Enable = !mute;
        }

        public Sender(string dlsPath) {
            //var sf2 = new SF2.SF2(dlsPath, mpWaveTable, fileSize);
            //sf2.GetInstList(mpInstList);
            mpInstList = waveout_systemValues(Marshal.StringToHGlobalAuto(dlsPath), SampleRate, 32, 256, 32);
            mppChParam = message_getChannelParamPtr();
            waveout_open();
        }

        public void Send(Event msg) {
            fixed (byte* ptr = &msg.Data[0]) {
                message_send(ptr);
            }
        }

        public void FileOut(string filePath, SMF smf) {
            IsFileOutput = true;
            var prog = waveout_getFileOutProgressPtr();
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
                    waveout_fileOut(Marshal.StringToHGlobalAuto(filePath),
                        44100, 16, (IntPtr)evPtr, (uint)evArr.Length, 960);
                }
                IsFileOutput = false;
            });
        }
    }
}
