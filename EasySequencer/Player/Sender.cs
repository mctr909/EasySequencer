using System;
using System.IO;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using EasySequencer;

namespace Player {
    #region struct
    public enum E_KEY_STATE : byte {
        FREE,
        PRESS,
        HOLD
    };

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct INST_ID {
        public byte isDrum;
        public byte bankMSB;
        public byte bankLSB;
        public byte progNum;
    };

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct INST_INFO {
        public INST_ID id;
        uint layerIndex;
        uint layerCount;
        uint artIndex;
        IntPtr pName;
        IntPtr pCategory;

        public string Name {
            get { return Marshal.PtrToStringAnsi(pName); }
        }
        public string Category {
            get { return Marshal.PtrToStringAnsi(pCategory); }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    unsafe struct INST_LIST {
        public int count;
        public INST_INFO** ppData;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CHANNEL_PARAM {
        public INST_ID InstId;
        public bool Enable;
        public byte Vol;
        public byte Exp;
        public byte Pan;
        public byte Rev;
        public byte Del;
        public byte Cho;
        public byte Mod;
        public byte Hld;
        public byte Fc;
        public byte Fq;
        public byte Atk;
        public byte Rel;
        public byte VibRate;
        public byte VibDepth;
        public byte VibDelay;
        public byte BendRange;
        public int Pitch;
        public double PeakL;
        public double PeakR;
        public double RmsL;
        public double RmsR;
        IntPtr pName;
        IntPtr pKeyBoard;

        public string Name {
            get { return Marshal.PtrToStringAnsi(pName); }
        }
        public E_KEY_STATE KeyBoard(int noteNum) {
            return (E_KEY_STATE)Marshal.PtrToStructure<byte>(pKeyBoard + noteNum);
        } 
    }
    #endregion

    unsafe public class Sender : IDisposable {
        #region WaveOut.dll
        [DllImport("WaveOut.dll")]
        private static extern INST_LIST* ptr_inst_list();
        [DllImport("WaveOut.dll")]
        private static extern CHANNEL_PARAM** ptr_channel_params();
        [DllImport("WaveOut.dll")]
        private static extern IntPtr ptr_active_counter();
        [DllImport("WaveOut.dll")]
        private static extern void send_message(byte port, byte* pMsg);

        [DllImport("WaveOut.dll")]
        private static extern void waveout_open(
            IntPtr filePath,
            int sampleRate,
            int bufferLength,
            int bufferCount
        );
        [DllImport("WaveOut.dll")]
        private static extern void waveout_close();

        [DllImport("WaveOut.dll")]
        private static extern IntPtr fileout_progress_ptr();
        [DllImport("WaveOut.dll")]
        private static extern void fileout_save(
            IntPtr waveTablePath,
            IntPtr savePath,
            uint sampleRate,
            IntPtr pEvents,
            uint eventSize,
            uint baseTick
        );
        #endregion

        public static readonly int SampleRate = 44100;
        public static readonly double DeltaTime = 1.0 / SampleRate;
        public static int CHANNEL_COUNT = 16;
        public static int SAMPLER_COUNT = 128;
        public static bool IsFileOutput { get; private set; }

        public static int ActiveCount {
            get { return Marshal.PtrToStructure<int>(mpActiveCountPtr); }
        }

        private static IntPtr mpActiveCountPtr;

        private INST_LIST* mpInstList = null;
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
        public void DrumChannel(int num, bool isDrum) {
            mppChParam[num]->InstId.isDrum = (byte)(isDrum ? 1 : 0);
        }
        public bool IsDrumChannel(int num) {
            return mppChParam[num]->InstId.isDrum == 1;
        }

        public Sender() { }
        public void Dispose() {
            waveout_close();
        }

        public bool SetUp(string waveTablePath) {
            waveout_open(Marshal.StringToHGlobalAuto(waveTablePath), SampleRate, 256, 32);
            mpInstList = ptr_inst_list();
            if (null == mpInstList) {
                return false;
            }
            mppChParam = ptr_channel_params();
            mpActiveCountPtr = ptr_active_counter();
            return true;
        }

        public void FileOut(string wavetablePath, string filePath, SMF smf) {
            IsFileOutput = true;

            var ms = new MemoryStream();
            var bw = new BinaryWriter(ms);
            foreach (var ev in smf.EventList) {
                if (((int)ev.Type & 0xF0) == (int)E_STATUS.SYSEX_BEGIN) {
                    if (null == ev.Meta) {
                        continue;
                    }
                    bw.Write(ev.Tick);
                    bw.Write(ev.Data);
                }
                bw.Write(ev.Tick);
                bw.Write(ev.Data);
            }
            var evArr = ms.ToArray();

            var prog = fileout_progress_ptr();
            *(int*)prog = 0;
            var fm = new StatusWindow((int)ms.Length, prog);
            fm.Show();
            Task.Factory.StartNew(() => {
                fixed (byte* evPtr = &evArr[0]) {
                    fileout_save(
                        Marshal.StringToHGlobalAuto(wavetablePath),
                        Marshal.StringToHGlobalAuto(filePath),
                        48000, (IntPtr)evPtr, (uint)evArr.Length, 960);
                }
                IsFileOutput = false;
            });
        }

        public void Send(Event msg) {
            fixed (byte* ptr = &msg.Data[0]) {
                send_message(0, ptr);
            }
        }
    }
}
