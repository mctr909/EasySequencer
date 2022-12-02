using System;
using System.IO;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using EasySequencer;

namespace Player {
    #region struct
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
    public struct CHANNEL_PARAM {
        public byte is_drum;
        public byte bank_msb;
        public byte bank_lsb;
        public byte prog_num;
        public byte enable;
        public byte vol;
        public byte exp;
        public byte pan;

        public byte rev_send;
        public byte del_send;
        public byte cho_send;
        public byte mod;
        public byte hold;
        public byte cutoff;
        public byte resonance;
        public byte attack;

        public byte release;
        public byte vib_rate;
        public byte vib_depth;
        public byte vib_delay;
        public byte bend_range;
        byte reserved1;
        byte reserved2;
        byte reserved3;

        public int pitch;

        public double peak_l;
        public double peak_r;
        public double rms_l;
        public double rms_r;

        public IntPtr p_keyboard;
        IntPtr p_name;

        public string Name { get { return Marshal.PtrToStringAnsi(p_name); } }
    }
    struct SYSTEM_VALUE {
        public int inst_count;
        public IntPtr p_inst_list;
        public IntPtr p_channel_params;
        public IntPtr p_active_counter;
        public IntPtr p_fileout_progress;
    };
    #endregion

    public enum E_KEY_STATE : byte {
        FREE,
        PRESS,
        HOLD
    };

    public class Sender : IDisposable {
        #region WaveOut.dll
        [DllImport("WaveOut.dll")]
        static extern IntPtr synth_setup(
            IntPtr filePath,
            int sampleRate,
            int bufferLength,
            int bufferCount
        );
        [DllImport("WaveOut.dll")]
        static extern void synth_close();
        [DllImport("WaveOut.dll")]
        static extern void fileout_save(
            IntPtr waveTablePath,
            IntPtr savePath,
            uint sampleRate,
            IntPtr pEvents,
            uint eventSize,
            uint baseTick
        );
        [DllImport("WaveOut.dll")]
        static extern void send_message(byte port, IntPtr pMsg);
        #endregion

        public static readonly int SampleRate = 44100;
        public static readonly double DeltaTime = 1.0 / SampleRate;
        public static int CHANNEL_COUNT = 16;
        public static int SAMPLER_COUNT = 128;
        public static bool IsFileOutput { get; private set; }

        private IntPtr[] mpInstList;
        private IntPtr[] mpChParam;
        private IntPtr mpActiveCountPtr;
        private IntPtr mpProgressPtr;
        private IntPtr mpMessage;

        public int ActiveCount {
            get {
                if (IntPtr.Zero == mpActiveCountPtr) {
                    return 0;
                } else {
                    return Marshal.PtrToStructure<int>(mpActiveCountPtr);
                }
            }
        }
        public int InstCount { get; private set; }
        public INST_INFO Instruments(int num) {
            return Marshal.PtrToStructure<INST_INFO>(mpInstList[num]);
        }
        public CHANNEL_PARAM Channel(int num) {
            return Marshal.PtrToStructure<CHANNEL_PARAM>(mpChParam[num]);
        }
        public void MuteChannel(int num, bool mute) {
            //mppChParam[num]->enable = !mute;
        }
        public void DrumChannel(int num, bool isDrum) {
            //mppChParam[num]->is_drum = (byte)(isDrum ? 1 : 0);
        }

        public Sender() {
            mpMessage = Marshal.AllocHGlobal(1024);
        }
        public void Dispose() {
            synth_close();
            Marshal.FreeHGlobal(mpMessage);
        }

        public bool Setup(string waveTablePath) {
            var ptrSysVal = synth_setup(Marshal.StringToHGlobalAuto(waveTablePath), SampleRate, 256, 32);
            if (IntPtr.Zero == ptrSysVal) {
                synth_close();
                return false;
            }
            var sysVal = Marshal.PtrToStructure<SYSTEM_VALUE>(ptrSysVal);
            InstCount = sysVal.inst_count;
            mpInstList = new IntPtr[InstCount];
            Marshal.Copy(sysVal.p_inst_list, mpInstList, 0, InstCount);
            mpChParam = new IntPtr[CHANNEL_COUNT];
            Marshal.Copy(sysVal.p_channel_params, mpChParam, 0, CHANNEL_COUNT);
            mpActiveCountPtr = sysVal.p_active_counter;
            mpProgressPtr = sysVal.p_fileout_progress;
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
            var fm = new StatusWindow((int)ms.Length, mpProgressPtr);
            int initProg = 0;
            Marshal.StructureToPtr(initProg, mpProgressPtr, false);
            fm.Show();

            Task.Factory.StartNew(() => {
                var ptrEvents = Marshal.AllocHGlobal(evArr.Length);
                Marshal.Copy(evArr, 0, ptrEvents, evArr.Length);
                fileout_save(
                    Marshal.StringToHGlobalAuto(wavetablePath),
                    Marshal.StringToHGlobalAuto(filePath),
                    48000, ptrEvents, (uint)evArr.Length, 960
                );
                Marshal.FreeHGlobal(ptrEvents);
                IsFileOutput = false;
            });
        }

        public void Send(Event msg) {
            Marshal.Copy(msg.Data, 0, mpMessage, msg.Data.Length);
            send_message(0, mpMessage);
        }
    }
}
