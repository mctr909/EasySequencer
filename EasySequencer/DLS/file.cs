using System;

namespace DLS {
    unsafe public class File : Chunk {
        public LINS instruments = null;
        public WVPL wavePool = null;

        private CK_COLH m_colh;
        private CK_PTBL m_ptbl;
        private CK_VERS* mp_version = null;

        public File(IntPtr dlsPtr, uint dlsSize) {
            Load((byte*)dlsPtr, dlsSize);
        }

        public override void Dispose() {
            instruments.Dispose();
            wavePool.Dispose();
        }

        protected override void LoadChunk(CHUNK_TYPE type, byte* ptr, uint size) {
            switch (type) {
            case CHUNK_TYPE.COLH:
                m_colh = *(CK_COLH*)ptr;
                break;
            case CHUNK_TYPE.VERS:
                mp_version = (CK_VERS*)ptr;
                break;
            case CHUNK_TYPE.MSYN:
                break;
            case CHUNK_TYPE.PTBL:
                m_ptbl = *(CK_PTBL*)ptr;
                break;
            case CHUNK_TYPE.DLID:
                break;
            default:
                //"Unknown ChunkType"
                break;
            }
        }

        protected override void LoadList(LIST_TYPE type, byte* ptr, uint size) {
            switch (type) {
            case LIST_TYPE.LINS:
                instruments = new LINS(ptr, size, m_colh.instruments);
                break;
            case LIST_TYPE.WVPL:
                wavePool = new WVPL(ptr, size, m_ptbl.count);
                break;
            case LIST_TYPE.INFO:
                break;
            default:
                // "Unknown ListType"
                break;
            }
        }
    }
}
