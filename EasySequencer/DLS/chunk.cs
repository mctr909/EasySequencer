using System;

namespace DLS {
    unsafe public class Chunk : IDisposable {
        protected Chunk() { }

        protected void Load(byte* ptr, uint size) {
            var ptrTerm = ptr + size;

            while (ptr < ptrTerm) {
                var mp_chunk = (CK_CHUNK*)ptr;
                ptr += sizeof(CK_CHUNK);

                if (CHUNK_TYPE.LIST == mp_chunk->type) {
                    var mp_list = (CK_LIST*)ptr;
                    LoadList(mp_list->type, ptr + sizeof(CK_LIST), mp_chunk->size);
                }
                else {
                    LoadChunk(mp_chunk->type, ptr, mp_chunk->size);
                }

                ptr += mp_chunk->size;
            }
        }

        protected virtual void LoadChunk(CHUNK_TYPE type, byte* ptr, uint size) { }

        protected virtual void LoadList(LIST_TYPE type, byte* ptr, uint size) { }

        public virtual void Dispose() { }
    }
}
