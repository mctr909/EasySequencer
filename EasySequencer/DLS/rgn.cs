using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace DLS {
    unsafe public class RGN_ : Chunk {
        public CK_RGNH* pHeader = null;
        public CK_WSMP* pSampler = null;
        public CK_WLNK* pWaveLink = null;
        public WaveLoop* pLoops = null;
        public LART articulations = null;

        public RGN_(byte* ptr, uint size) {
            Load(ptr, size);
        }

        public override void Dispose() {
            if (null != pLoops) {
                Marshal.FreeHGlobal((IntPtr)pLoops);
                pLoops = null;
            }

            if (null != articulations) {
                articulations.Dispose();
                articulations = null;
            }
        }

        protected override void LoadChunk(CHUNK_TYPE type, byte* ptr, uint size) {
            switch (type) {
            case CHUNK_TYPE.RGNH:
                pHeader = (CK_RGNH*)ptr;
                break;
            case CHUNK_TYPE.WSMP: {
                    if (null != pLoops) {
                        Marshal.FreeHGlobal((IntPtr)pLoops);
                        pLoops = null;
                    }

                    pSampler = (CK_WSMP*)ptr;
                    var pLoop = ptr + sizeof(CK_WSMP);
                    pLoops = (WaveLoop*)Marshal.AllocHGlobal(sizeof(WaveLoop) * (int)pSampler->loopCount);
                    for (uint i = 0; i < pSampler->loopCount; ++i) {
                        pLoops[i] = *(WaveLoop*)pLoop;
                        pLoop += sizeof(WaveLoop);
                    }
                }
                break;
            case CHUNK_TYPE.WLNK:
                pWaveLink = (CK_WLNK*)ptr;
                break;
            default:
                // "Unknown ChunkType"
                break;
            }
        }

        protected override void LoadList(LIST_TYPE type, byte* ptr, uint size) {
            switch (type) {
            case LIST_TYPE.LART:
            case LIST_TYPE.LAR2:
                articulations = new LART(ptr, size);
                break;
            default:
                // "Unknown ListType"
                break;
            }
        }
    }

    unsafe public class LRGN : Chunk {
        public List<RGN_> List = new List<RGN_>();
        private uint m_listCount;

        public LRGN(byte* ptr, uint size, uint waveCount) {
            m_listCount = waveCount;
            Load(ptr, size);
        }

        protected override void LoadList(LIST_TYPE type, byte* ptr, uint size) {
            switch (type) {
            case LIST_TYPE.RGN_:
                if (List.Count < m_listCount) {
                    List.Add(new RGN_(ptr, size));
                }
                break;
            default:
                // "Unknown ListType"
                break;
            }
        }
    }
}
