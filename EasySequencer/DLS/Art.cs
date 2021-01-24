using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace DLS {
    public class LART : RiffChunk {
        public ART Art = null;

        public LART(IntPtr ptr, uint size) : base(ptr, size) { }

        protected override void LoadChunk(IntPtr ptr, string type, uint size) {
            switch (type) {
            case "art1":
            case "art2":
                Art = new ART(ptr);
                break;
            default:
                throw new Exception("[LART]Unknown ChunkType");
            }
        }
    }

    public class ART {
        public List<CONNECTION> List { get; private set; } = new List<CONNECTION>();

        public ART(IntPtr ptr) {
            ptr += 4;
            var connCount = Marshal.PtrToStructure<uint>(ptr);
            ptr += 4;
            for (uint i = 0; i < connCount; ++i) {
                List.Add(Marshal.PtrToStructure<CONNECTION>(ptr));
                ptr += Marshal.SizeOf<CONNECTION>();
            }
        }

        public static double GetValue(CONNECTION conn) {
            switch (conn.destination) {
            case DST_TYPE.ATTENUATION:
            case DST_TYPE.FILTER_Q:
                return Math.Pow(10.0, conn.scale / (200 * 65536.0));

            case DST_TYPE.PAN:
                return (conn.scale / 655360.0) - 0.5;

            case DST_TYPE.LFO_START_DELAY:
            case DST_TYPE.VIB_START_DELAY:
            case DST_TYPE.EG1_ATTACK_TIME:
            case DST_TYPE.EG1_DECAY_TIME:
            case DST_TYPE.EG1_RELEASE_TIME:
            case DST_TYPE.EG1_DELAY_TIME:
            case DST_TYPE.EG1_HOLD_TIME:
            case DST_TYPE.EG1_SHUTDOWN_TIME:
            case DST_TYPE.EG2_ATTACK_TIME:
            case DST_TYPE.EG2_DECAY_TIME:
            case DST_TYPE.EG2_RELEASE_TIME:
            case DST_TYPE.EG2_DELAY_TIME:
            case DST_TYPE.EG2_HOLD_TIME:
                return Math.Pow(2.0, conn.scale / (1200 * 65536.0));

            case DST_TYPE.EG1_SUSTAIN_LEVEL:
            case DST_TYPE.EG2_SUSTAIN_LEVEL:
                return conn.scale / 655360.0;

            case DST_TYPE.PITCH:
            case DST_TYPE.LFO_FREQUENCY:
            case DST_TYPE.VIB_FREQUENCY:
            case DST_TYPE.FILTER_CUTOFF:
                return Math.Pow(2.0, (conn.scale / 65536.0 - 6900) / 1200.0) * 440;

            default:
                return 0.0;
            }
        }
    }
}
