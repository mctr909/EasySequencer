using System;
using System.IO;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using EasySequencer;
using SMF;

namespace SynthDll {
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct INST_INFO {
        public byte is_drum;
        public byte bank_msb;
        public byte bank_lsb;
        public byte prog_num;
        uint layer_index;
        uint layer_count;
        uint art_index;
        IntPtr p_name;
        IntPtr p_category;

        public string Name {
            get { return Marshal.PtrToStringAnsi(p_name); }
        }
        public string Category {
            get { return Marshal.PtrToStringAnsi(p_category); }
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

        public double rms_l;
        public double rms_r;
        public double peak_l;
        public double peak_r;

        public IntPtr p_keyboard;
        IntPtr p_name;

        public string Name { get { return Marshal.PtrToStringAnsi(p_name); } }
    }

    public enum E_KEY_STATE : byte {
        FREE,
        PRESS,
        HOLD
    };

    public static class Synth {
        const int MSG_BUFF_LEN = 1024;
        public const int TRACK_COUNT = 256;
        public const int SAMPLER_COUNT = 64;

        #region [synth.dll]
        [DllImport("synth.dll")]
        static extern IntPtr synth_setup(
            IntPtr filePath,
            int sampleRate,
            int bufferLength,
            int bufferCount
        );
        [DllImport("synth.dll")]
        static extern void synth_close();
        [DllImport("synth.dll")]
        static extern void fileout(
            IntPtr waveTablePath,
            IntPtr savePath,
            uint sampleRate,
            uint baseTick,
            uint eventSize,
            IntPtr pEvents,
            IntPtr pProgress
        );
        [DllImport("synth.dll")]
        static extern void send_message(byte port, IntPtr pMsg);
        #endregion

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        struct SYSTEM_VALUE {
            public int inst_count;
            public IntPtr p_inst_list;
            public IntPtr p_channel_params;
            public IntPtr p_active_counter;
        };
        static SYSTEM_VALUE mSysValue;
        static IntPtr[] mpInstList;
        static IntPtr[] mpChParam;
        static IntPtr mpMsgBuff = Marshal.AllocHGlobal(MSG_BUFF_LEN);

        public static int ActiveCount { get { return Marshal.PtrToStructure<int>(mSysValue.p_active_counter); } }
        public static int InstCount { get { return mSysValue.inst_count; } }

        public static INST_INFO Instruments(int num) {
            return Marshal.PtrToStructure<INST_INFO>(mpInstList[num]);
        }
        public static CHANNEL_PARAM GetChannel(int num) {
            return Marshal.PtrToStructure<CHANNEL_PARAM>(mpChParam[num]);
        }
        public static void MuteChannel(int num, bool mute) {
            Send((byte)(num / 16), new Event(num % 16, E_CONTROL.ALL_NOTE_OFF, mute ? 127 : 0));
        }
        public static void RythmChannel(byte port, int chNum, bool isDrum) {
            Send(port, new Event(chNum, E_CONTROL.DRUM, isDrum ? 127 : 0));
        }

        public static bool Setup(string waveTablePath, int sampleRate) {
            var ptrSysVal = synth_setup(Marshal.StringToHGlobalAuto(waveTablePath), sampleRate, 256, 32);
            if (IntPtr.Zero == ptrSysVal) {
                synth_close();
                return false;
            }
            Marshal.Copy(new byte[MSG_BUFF_LEN], 0, mpMsgBuff, MSG_BUFF_LEN);
            mSysValue = Marshal.PtrToStructure<SYSTEM_VALUE>(ptrSysVal);
            mpInstList = new IntPtr[InstCount];
            Marshal.Copy(mSysValue.p_inst_list, mpInstList, 0, InstCount);
            mpChParam = new IntPtr[TRACK_COUNT];
            Marshal.Copy(mSysValue.p_channel_params, mpChParam, 0, TRACK_COUNT);
            return true;
        }

        public static void FileOut(string wavetablePath, string filePath, SMF.File smf) {
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

            int prog = 0;
            var ptrProg = Marshal.AllocHGlobal(sizeof(int));
            Marshal.StructureToPtr(prog, ptrProg, true);
            var fm = new StatusWindow((int)ms.Length, ptrProg);
            fm.Show();

            Task.Factory.StartNew(() => {
                var ptrEvents = Marshal.AllocHGlobal(evArr.Length);
                Marshal.Copy(evArr, 0, ptrEvents, evArr.Length);
                fileout(
                    Marshal.StringToHGlobalAuto(wavetablePath),
                    Marshal.StringToHGlobalAuto(filePath),
                    48000, 960, (uint)evArr.Length, ptrEvents, ptrProg
                );
                Marshal.FreeHGlobal(ptrEvents);
            });

            Marshal.FreeHGlobal(ptrProg);
        }

        public static void Send(byte port, Event msg) {
            Marshal.Copy(msg.Data, 0, mpMsgBuff, msg.Data.Length);
            send_message(port, mpMsgBuff);
        }
    }
}
