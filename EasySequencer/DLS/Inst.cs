using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace DLS {
    public class LINS : RiffChunk {
        public HashSet<INS_> List = new HashSet<INS_>();

        public LINS(IntPtr ptr, uint size) : base(ptr, size) { }

        protected override void LoadList(IntPtr ptr, string type, uint size) {
            switch (type) {
            case "ins ":
                List.Add(new INS_(ptr, size));
                break;
            default:
                throw new Exception("[LINS]Unknown ListType");
            }
        }
    }

    public class INS_ : RiffChunk {
        public CK_INSH Header;
        public LRGN Regions = null;
        public LART Articulations = null;
        public string Name { get; private set; } = "";
        public string Category { get; private set; } = "";

        public INS_(IntPtr ptr, uint size) : base(ptr, size) { }

        protected override void LoadInfo(IntPtr ptr, string type, uint size) {
            switch (type) {
            case "INAM":
                Name = Marshal.PtrToStringAnsi(ptr).Trim();
                break;
            case "ICAT":
                Category = Marshal.PtrToStringAnsi(ptr).Trim();
                break;
            }
        }

        protected override void LoadChunk(IntPtr ptr, string type, uint size) {
            switch (type) {
            case "insh":
                Header = Marshal.PtrToStructure<CK_INSH>(ptr);
                break;
            default:
                throw new Exception("[INS_]Unknown ChunkType");
            }
        }

        protected override void LoadList(IntPtr ptr, string type, uint size) {
            switch (type) {
            case "lrgn":
                Regions = new LRGN(ptr, size);
                break;
            case "lart":
            case "lar2":
                Articulations = new LART(ptr, size);
                break;
            default:
                throw new Exception("[INS_]Unknown ListType");
            }
        }
    }
}
