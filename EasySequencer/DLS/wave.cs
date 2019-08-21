using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace DLS {
    unsafe public class WAVE : Chunk {
        public CK_FMT* pFormat = null;
        public CK_WSMP* pSampler = null;
        public WAVE_LOOP* pLoops = null;
        public uint buffAddr = 0;
        public uint buffSize = 0;

        public WAVE(byte* ptr, uint size) {
            Load(ptr, size);
        }

        public override void Dispose() {
            if (null != pLoops) {
                Marshal.FreeHGlobal((IntPtr)pLoops);
                pLoops = null;
            }
        }

        protected override void LoadChunk(CHUNK_TYPE type, byte* ptr, uint size) {
            switch (type) {
            case CHUNK_TYPE.DLID:
            case CHUNK_TYPE.GUID:
                break;
            case CHUNK_TYPE.FMT_:
                pFormat = (CK_FMT*)ptr;
                break;
            case CHUNK_TYPE.DATA:
                buffAddr = (uint)((IntPtr)ptr).ToInt64();
                buffSize = size;
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
            default:
                throw new Exception("[WAVE]Unknown ChunkType");
            }
        }

        protected override void LoadList(LIST_TYPE type, byte* ptr, uint size) {
            switch (type) {
            case LIST_TYPE.WAVE:
                break;
            case LIST_TYPE.INFO:
                break;
            default:
                throw new Exception("[WAVE]Unknown ListType");
            }
        }
    }

    unsafe public class WVPL : Chunk {
        public List<WAVE> List = new List<WAVE>();
        private uint m_listCount;

        public WVPL(byte* ptr, uint size, uint waveCount) {
            m_listCount = waveCount;
            Load(ptr, size);
        }

        public override void Dispose() {
            foreach (var wave in List) {
                if (null != wave) {
                    wave.Dispose();
                }
            }
        }

        protected override void LoadList(LIST_TYPE type, byte* ptr, uint size) {
            switch (type) {
            case LIST_TYPE.WAVE:
                if (List.Count < m_listCount) {
                    List.Add(new WAVE(ptr, size));
                }
                break;
            case LIST_TYPE.INFO:
                break;
            default:
                throw new Exception("[WVPL]Unknown ListType");
            }
        }
    }
}
