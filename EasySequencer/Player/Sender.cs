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
        public bool IsDrum;
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

    unsafe public class Sender {
        #region WaveOut.dll
        [DllImport("WaveOut.dll")]
        private static extern CHANNEL_PARAM** waveout_getChannelParamPtr();
        [DllImport("WaveOut.dll")]
        private static extern void message_send(byte* pMsg);

        [DllImport("WaveOut.dll")]
        private static extern IntPtr waveout_getActiveSamplersPtr();
        [DllImport("WaveOut.dll")]
        private static extern int waveout_open(
            IntPtr filePath,
            out INST_LIST* pInstList,
            int sampleRate,
            int bits,
            int bufferLength,
            int bufferCount
        );
        [DllImport("WaveOut.dll")]
        private static extern void waveout_close();

        [DllImport("WaveOut.dll")]
        private static extern IntPtr fileout_getProgressPtr();
        [DllImport("WaveOut.dll")]
        private static extern void fileout_save(
            IntPtr waveTablePath,
            IntPtr savePath,
            uint sampleRate,
            uint bitRate,
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

        private static IntPtr mpActiveCountPtr = waveout_getActiveSamplersPtr();

        private INST_LIST* mpInstList = null;
        private CHANNEL_PARAM** mppChParam;
        private string mWaveTablePath;

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
            mppChParam[num]->IsDrum = isDrum;
        }
        public bool IsDrumChannel(int num) {
            return mppChParam[num]->IsDrum;
        }

        public Sender(string waveTablePath) {
            mWaveTablePath = waveTablePath;
            waveout_open(Marshal.StringToHGlobalAuto(mWaveTablePath), out mpInstList, SampleRate, 32, SampleRate / 150, 32);
            mppChParam = waveout_getChannelParamPtr();
        }

        public void Send(Event msg) {
            fixed (byte* ptr = &msg.Data[0]) {
                message_send(ptr);
            }
        }

        public void FileOut(string filePath, SMF smf) {
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

            var prog = fileout_getProgressPtr();
            *(int*)prog = 0;
            var fm = new StatusWindow((int)ms.Length, prog);
            fm.Show();
            Task.Factory.StartNew(() => {
                fixed (byte* evPtr = &evArr[0]) {
                    fileout_save(
                        Marshal.StringToHGlobalAuto(mWaveTablePath),
                        Marshal.StringToHGlobalAuto(filePath),
                        48000, 16, (IntPtr)evPtr, (uint)evArr.Length, 960);
                }
                IsFileOutput = false;
            });
        }
    }
}
