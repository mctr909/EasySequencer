using System;
using System.Collections.Generic;

namespace DLS {
    unsafe public class INS_ : Chunk {
        public CK_INSH* pHeader = null;
        public LRGN regions = null;
        public LART articulations = null;
        public string name = "";
        public string category = "";

        public INS_(byte* ptr, uint size) {
            Load(ptr, size);
        }

        protected override void LoadInfo(string type, string text) {
            switch(type) {
            case "INAM": name = text.Trim(); break;
            case "IKEY": category = text.Trim(); break;
            }
        }

        protected override void LoadChunk(CHUNK_TYPE type, byte* ptr, uint size) {
            switch (type) {
            case CHUNK_TYPE.INSH:
                pHeader = (CK_INSH*)ptr;
                break;
            case CHUNK_TYPE.PTBL:
                break;
            default:
                throw new Exception("[INS_]Unknown ChunkType");
            }
        }

        protected override void LoadList(LIST_TYPE type, byte* ptr, uint size) {
            switch (type) {
            case LIST_TYPE.LRGN:
                if (null != regions) {
                    regions.Dispose();
                    regions = null;
                }
                regions = new LRGN(ptr, size, pHeader->regions);
                break;
            case LIST_TYPE.LART:
            case LIST_TYPE.LAR2:
                if (null != articulations) {
                    articulations.Dispose();
                    articulations = null;
                }
                articulations = new LART(ptr, size);
                break;
            case LIST_TYPE.INS_:
                break;
            case LIST_TYPE.INFO:
                break;
            default:
                throw new Exception("[INS_]Unknown ListType");
            }
        }
    }

    unsafe public class LINS : Chunk {
        public HashSet<INS_> List = new HashSet<INS_>();
        private uint m_listCount;

        public LINS(byte* ptr, uint size, uint instruments) {
            m_listCount = instruments;
            Load(ptr, size);
        }

        protected override void LoadList(LIST_TYPE type, byte* ptr, uint size) {
            switch (type) {
            case LIST_TYPE.INS_:
                if (List.Count < m_listCount) {
                    List.Add(new INS_(ptr, size));
                }
                break;
            default:
                throw new Exception("[LINS]Unknown ListType");
            }
        }
    }
}
