using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DLS;

namespace WaveOut {
    unsafe public class Instruments {
        [DllImport("WaveOut.dll", SetLastError = true, CharSet = CharSet.Auto)]
        unsafe private static extern IntPtr LoadDLS(IntPtr filePath, out uint size);

        public Dictionary<INST_ID, WAVE_INFO[]> List;
        public Dictionary<INST_ID, string[]> Names;

        public Instruments(string dlsPath) {
            uint dlsSize = 0;
            var dlsPtr = LoadDLS(Marshal.StringToHGlobalAuto(dlsPath), out dlsSize);
            var dls = new File(dlsPtr, dlsSize);

            List = new Dictionary<INST_ID, WAVE_INFO[]>();
            Names = new Dictionary<INST_ID, string[]>();

            foreach (var inst in dls.instruments.List) {
                var envAmp = new ENVELOPE();
                if (null != inst.articulations) {
                    envAmp.levelA = 0.0;
                    envAmp.levelD = 1.0;
                    envAmp.levelS = 1.0;
                    envAmp.levelR = 0.0;
                    envAmp.deltaA = 1.0;
                    envAmp.deltaD = 1.0;
                    envAmp.deltaR = 16.0;
                    envAmp.hold = 0.0;
                    var holdTime = 0.0;

                    foreach (var conn in inst.articulations.art.List) {
                        if (SRC_TYPE.NONE != conn.source) {
                            continue;
                        }

                        switch (conn.destination) {
                        case DST_TYPE.EG1_ATTACK_TIME:
                            envAmp.deltaA = 64 * Const.DeltaTime / ART.GetValue(conn);
                            holdTime += ART.GetValue(conn);
                            break;
                        case DST_TYPE.EG1_DECAY_TIME:
                            envAmp.deltaD = 24 * Const.DeltaTime / ART.GetValue(conn);
                            break;
                        case DST_TYPE.EG1_RELEASE_TIME:
                            envAmp.deltaR = 24 * Const.DeltaTime / ART.GetValue(conn);
                            break;
                        case DST_TYPE.EG1_SUSTAIN_LEVEL:
                            envAmp.levelS = (0.0 == ART.GetValue(conn)) ? 1.0 : (ART.GetValue(conn) * 0.01);
                            break;
                        case DST_TYPE.EG1_HOLD_TIME:
                            holdTime += ART.GetValue(conn);
                            break;
                        }
                    }

                    envAmp.hold += holdTime;
                    if (envAmp.hold < Const.DeltaTime) {
                        envAmp.hold = Const.DeltaTime;
                    }

                    if (1.0 < envAmp.deltaA) {
                        envAmp.deltaA = 1.0;
                    }
                    if (1.0 < envAmp.deltaD) {
                        envAmp.deltaD = 1.0;
                    }
                    if (1.0 < envAmp.deltaR) {
                        envAmp.deltaR = 1.0;
                    }
                }

                var waveInfo = new WAVE_INFO[128];
                for (var noteNo = 0; noteNo < waveInfo.Length; ++noteNo) {
                    RGN_ region = null;
                    foreach (var rgn in inst.regions.List) {
                        if (rgn.pHeader->key.low <= noteNo && noteNo <= rgn.pHeader->key.high) {
                            region = rgn;
                            break;
                        }
                    }

                    if (null == region) {
                        waveInfo[noteNo].pcmAddr = uint.MaxValue;
                        continue;
                    }

                    if (null != region.articulations) {
                        envAmp.levelA = 0.0;
                        envAmp.levelD = 1.0;
                        envAmp.levelS = 1.0;
                        envAmp.levelR = 0.0;
                        envAmp.deltaA = 1.0;
                        envAmp.deltaD = 1.0;
                        envAmp.deltaR = 16.0;
                        envAmp.hold = 0.0;
                        var holdTime = 0.0;

                        foreach (var conn in region.articulations.art.List) {
                            if (SRC_TYPE.NONE != conn.source)
                                continue;
                            switch (conn.destination) {
                            case DST_TYPE.EG1_ATTACK_TIME:
                                envAmp.deltaA = 64 * Const.DeltaTime / ART.GetValue(conn);
                                holdTime += ART.GetValue(conn);
                                break;
                            case DST_TYPE.EG1_DECAY_TIME:
                                envAmp.deltaD = 24 * Const.DeltaTime / ART.GetValue(conn);
                                break;
                            case DST_TYPE.EG1_RELEASE_TIME:
                                envAmp.deltaR = 24 * Const.DeltaTime / ART.GetValue(conn);
                                break;
                            case DST_TYPE.EG1_SUSTAIN_LEVEL:
                                envAmp.levelS = (0.0 == ART.GetValue(conn)) ? 1.0 : (ART.GetValue(conn) * 0.01);
                                break;
                            case DST_TYPE.EG1_HOLD_TIME:
                                holdTime += ART.GetValue(conn);
                                break;
                            }
                        }

                        envAmp.hold += holdTime;
                        if (envAmp.hold < Const.DeltaTime) {
                            envAmp.hold = Const.DeltaTime;
                        }

                        if (1.0 < envAmp.deltaA) {
                            envAmp.deltaA = 1.0;
                        }
                        if (1.0 < envAmp.deltaD) {
                            envAmp.deltaD = 1.0;
                        }
                        if (1.0 < envAmp.deltaR) {
                            envAmp.deltaR = 1.0;
                        }
                    }

                    waveInfo[noteNo].envAmp = envAmp;
                    var wave = dls.wavePool.List[(int)region.pWaveLink->tableIndex];
                    waveInfo[noteNo].pcmAddr = wave.pcmAddr - (uint)dlsPtr.ToInt32();
                    waveInfo[noteNo].pcmLength = wave.dataSize / wave.pFormat->blockAlign;

                    if (null == region.pSampler) {
                        waveInfo[noteNo].unityNote = (byte)wave.pSampler->unityNote;
                        waveInfo[noteNo].delta
                            = Math.Pow(2.0, wave.pSampler->fineTune / 1200.0)
                            * wave.pFormat->sampleRate
                        ;
                        waveInfo[noteNo].gain = wave.pSampler->Gain / 32768.0;
                        if (0 < wave.pSampler->loopCount) {
                            waveInfo[noteNo].loop.begin = wave.pLoops[0].start;
                            waveInfo[noteNo].loop.length = wave.pLoops[0].length;
                            waveInfo[noteNo].loop.enable = true;
                        } else {
                            waveInfo[noteNo].loop.begin = 0;
                            waveInfo[noteNo].loop.length = waveInfo[noteNo].pcmLength;
                            waveInfo[noteNo].loop.enable = false;
                        }
                    } else {
                        waveInfo[noteNo].unityNote = (byte)region.pSampler->unityNote;
                        waveInfo[noteNo].delta
                            = Math.Pow(2.0, region.pSampler->fineTune / 1200.0)
                            * wave.pFormat->sampleRate
                        ;
                        waveInfo[noteNo].gain = region.pSampler->Gain / 32768.0;
                        if (0 < region.pSampler->loopCount) {
                            waveInfo[noteNo].loop.begin = region.pLoops[0].start;
                            waveInfo[noteNo].loop.length = region.pLoops[0].length;
                            waveInfo[noteNo].loop.enable = true;
                        } else if (0 < wave.pSampler->loopCount) {
                            waveInfo[noteNo].loop.begin = wave.pLoops[0].start;
                            waveInfo[noteNo].loop.length = wave.pLoops[0].length;
                            waveInfo[noteNo].loop.enable = true;
                        } else {
                            waveInfo[noteNo].loop.begin = 0;
                            waveInfo[noteNo].loop.length = waveInfo[noteNo].pcmLength;
                            waveInfo[noteNo].loop.enable = false;
                            waveInfo[noteNo].envAmp.deltaR = Const.DeltaTime * waveInfo[noteNo].delta / waveInfo[noteNo].pcmLength;
                        }
                    }
                }

                var id = new INST_ID();
                id.isDrum = inst.pHeader->locale.bankFlags;
                id.programNo = inst.pHeader->locale.programNo;
                id.bankMSB = inst.pHeader->locale.bankMSB;
                id.bankLSB = inst.pHeader->locale.bankLSB;

                List.Add(id, waveInfo);
                Names.Add(id, new string[] { inst.name, inst.category });
            }
        }
    }
}
