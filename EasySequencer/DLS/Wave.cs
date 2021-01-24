using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace DLS {
    public class WVPL : RiffChunk {
        public List<WAVE> List = new List<WAVE>();

        public WVPL(IntPtr ptr, uint size) : base(ptr, size) { }

        protected override void LoadList(IntPtr ptr, string type, uint size) {
            switch (type) {
            case "wave":
                List.Add(new WAVE(ptr, size));
                break;
            default:
                throw new Exception("[WVPL]Unknown ListType");
            }
        }
    }

    public class WAVE : RiffChunk {
        public CK_FMT Format { get; private set; }
        public CK_WSMP Sampler { get; private set; }
        public List<WAVE_LOOP> Loops { get; private set; } = new List<WAVE_LOOP>();
        public bool HasLoop { get; private set; }
        public uint Addr { get; private set; }
        public uint Size { get; private set; }

        public WAVE(IntPtr ptr, uint size) : base(ptr, size) { }

        protected override void LoadChunk(IntPtr ptr, string type, uint size) {
            switch (type) {
            case "DLID":
            case "GUID":
                break;
            case "fmt ":
                Format = Marshal.PtrToStructure<CK_FMT>(ptr);
                break;
            case "data":
                Addr = (uint)ptr.ToInt64();
                Size = size;
                break;
            case "wsmp":
                Sampler = Marshal.PtrToStructure<CK_WSMP>(ptr);
                ptr += Marshal.SizeOf<CK_WSMP>();
                for (uint i = 0; i < Sampler.loopCount; ++i) {
                    Loops.Add(Marshal.PtrToStructure<WAVE_LOOP>(ptr));
                    ptr += Marshal.SizeOf<WAVE_LOOP>();
                }
                HasLoop = 0 < Sampler.loopCount;
                break;
            default:
                throw new Exception("[WAVE]Unknown ChunkType");
            }
        }

        protected override void LoadList(IntPtr ptr, string type, uint size) {
            switch (type) {
            default:
                throw new Exception("[WAVE]Unknown ListType");
            }
        }
    }
}
