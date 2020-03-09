﻿using System;
using System.IO;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using MIDI;
using EasySequencer;

namespace Player {
    unsafe public class Sender {
        [DllImport("MidiSender.dll")]
        private static extern CHANNEL_PARAM** midi_GetChannelParamPtr();
        [DllImport("MidiSender.dll")]
        private static extern IntPtr midi_GetWavFileOutProgressPtr();
        [DllImport("MidiSender.dll")]
        private static extern void midi_CreateChannels(INST_LIST* list, SAMPLER** ppSmpl, CHANNEL** ppCh, int samplerCount);
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

        [DllImport("WaveOut.dll")]
        private static extern IntPtr waveout_GetActiveSamplersPtr();
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
        public static bool IsFileOutput {
            get { return false; }
        }
        public static int ActiveCount {
            get { return Marshal.PtrToStructure<int>(mpActiveCountPtr); }
        }

        public CHANNEL_PARAM** Channel { get; private set; }
        public INST_LIST* InstList { get; private set; }
        public SAMPLER** ppWaveOutSampler { get; private set; }

        private static IntPtr mpActiveCountPtr = waveout_GetActiveSamplersPtr();
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

            waveout_SystemValues(SampleRate, 32, 512, 16, CHANNEL_COUNT, SAMPLER_COUNT);
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

        public void FileOut(string filePath, SMF smf) {
            var prog = (int*)midi_GetWavFileOutProgressPtr();
            *prog = 0;
            var fm = new StatusWindow(smf.MaxTime, prog);
            fm.Show();
            Task.Factory.StartNew(() => {
                var ms = new MemoryStream();
                var bw = new BinaryWriter(ms);
                foreach (var ev in smf.EventList) {
                    bw.Write(ev.Time);
                    bw.Write(ev.Data);
                }
                var evArr = ms.ToArray();
                fixed (byte* evPtr = &evArr[0]) {
                    midi_WavFileOut(Marshal.StringToHGlobalAuto(filePath), mpWaveTable, InstList,
                        44100, 16, (IntPtr)evPtr, (uint)evArr.Length, (uint)smf.Ticks);
                }
            });
        }
    }
}