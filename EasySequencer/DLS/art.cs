using System;
using System.Collections.Generic;

namespace DLS {
    unsafe public class ART {
        public HashSet<CONNECTION> List = null;

        public ART(byte* ptr) {
            CK_ART1* pInfo = (CK_ART1*)ptr;
            ptr += sizeof(CK_ART1);

            List = new HashSet<CONNECTION>();

            for (uint i = 0; i < pInfo->count; ++i) {
                List.Add(*(CONNECTION*)ptr);
                ptr += sizeof(CONNECTION);
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
                return (conn.scale == int.MinValue) ? 1.0 : Math.Pow(2.0, conn.scale / (1200 * 65536.0));

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

    unsafe public class LART : Chunk {
        public ART art = null;

        public LART(byte* ptr, uint size) {
            Load(ptr, size);
        }

        protected override void LoadChunk(CHUNK_TYPE type, byte* ptr, uint size) {
            switch (type) {
            case CHUNK_TYPE.ART1:
            case CHUNK_TYPE.ART2:
                art = new ART(ptr);
                break;
            default:
                throw new Exception("[LART]Unknown ChunkType");
            }
        }
    }
}
