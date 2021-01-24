using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace DLS {
    public class LRGN : RiffChunk {
        public HashSet<RGN_> List = new HashSet<RGN_>();

        public LRGN(IntPtr ptr, uint size) : base(ptr, size) { }

        protected override void LoadList(IntPtr ptr, string type, uint size) {
            switch (type) {
            case "rgn ":
                List.Add(new RGN_(ptr, size));
                break;
            default:
                throw new Exception("[LRGN]Unknown ListType");
            }
        }
    }

    public class RGN_ : RiffChunk {
        public CK_RGNH Header { get; private set; }
        public CK_WLNK WaveLink { get; private set; }
        public CK_WSMP Sampler { get; private set; }
        public List<WAVE_LOOP> Loops { get; private set; } = new List<WAVE_LOOP>();
        public LART Articulations = null;

        public bool HasSampler { get; private set; }
        public bool HasLoop { get; private set; }

        public RGN_(IntPtr ptr, uint size) : base(ptr, size) { }

        protected override void LoadChunk(IntPtr ptr, string type, uint size) {
            switch (type) {
            case "rgnh":
                Header = Marshal.PtrToStructure<CK_RGNH>(ptr);
                break;
            case "wlnk":
                WaveLink = Marshal.PtrToStructure<CK_WLNK>(ptr);
                break;
            case "wsmp":
                Sampler = Marshal.PtrToStructure<CK_WSMP>(ptr);
                ptr += Marshal.SizeOf<CK_WSMP>();
                for (uint i = 0; i < Sampler.loopCount; ++i) {
                    Loops.Add(Marshal.PtrToStructure<WAVE_LOOP>(ptr));
                    ptr += Marshal.SizeOf<WAVE_LOOP>();
                }
                HasSampler = true;
                HasLoop = 0 < Sampler.loopCount;
                break;
            default:
                throw new Exception("[RGN_]Unknown ChunkType");
            }
        }

        protected override void LoadList(IntPtr ptr, string type, uint size) {
            switch (type) {
            case "lart":
            case "lar2":
                Articulations = new LART(ptr, size);
                break;
            default:
                throw new Exception("[RGN_]Unknown ListType");
            }
        }
    }
}
