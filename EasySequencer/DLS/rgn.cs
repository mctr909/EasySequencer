using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace DLS {
    unsafe public class RGN_ : Chunk {
        public CK_RGNH* pHeader = null;
        public CK_WSMP* pSampler = null;
        public CK_WLNK* pWaveLink = null;
        public WAVE_LOOP* pLoops = null;
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
                    pLoops = (WAVE_LOOP*)Marshal.AllocHGlobal(sizeof(WAVE_LOOP) * (int)pSampler->loopCount);
                    for (uint i = 0; i < pSampler->loopCount; ++i) {
                        pLoops[i] = *(WAVE_LOOP*)pLoop;
                        pLoop += sizeof(WAVE_LOOP);
                    }
                }
                break;
            case CHUNK_TYPE.WLNK:
                pWaveLink = (CK_WLNK*)ptr;
                break;
            default:
                throw new Exception("[RGN_]Unknown ChunkType");
            }
        }

        protected override void LoadList(LIST_TYPE type, byte* ptr, uint size) {
            switch (type) {
            case LIST_TYPE.LART:
            case LIST_TYPE.LAR2:
                articulations = new LART(ptr, size);
                break;
            case LIST_TYPE.RGN_:
                break;
            case LIST_TYPE.INFO:
                break;
            default:
                throw new Exception("[RGN_]Unknown ListType");
            }
        }
    }

    unsafe public class LRGN : Chunk {
        public HashSet<RGN_> List = new HashSet<RGN_>();
        private uint m_listCount;

        public LRGN(byte* ptr, uint size, uint regions) {
            m_listCount = regions;
            Load(ptr, size);
        }

        protected override void LoadList(LIST_TYPE type, byte* ptr, uint size) {
            switch (type) {
            case LIST_TYPE.RGN_:
                if (List.Count < m_listCount) {
                    List.Add(new RGN_(ptr, size));
                }
                break;
            case LIST_TYPE.LART:
            case LIST_TYPE.LAR2:
                break;
            case LIST_TYPE.INFO:
                break;
            default:
                throw new Exception("[LRGN]Unknown ListType");
            }
        }
    }
}
