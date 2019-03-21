using System;
using System.Runtime.InteropServices;

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
                    if (LIST_TYPE.INFO == mp_list->type) {
                        var listPtr = ptr + sizeof(CK_LIST);
                        var listPos = sizeof(CK_LIST);
                        while (listPos < mp_chunk->size) {
                            var infoType = Marshal.PtrToStringAnsi((IntPtr)listPtr, 4);
                            listPtr += 4;
                            var infoSize = *(int*)listPtr;
                            listPtr += 4;
                            var text = Marshal.PtrToStringAnsi((IntPtr)listPtr);
                            listPtr += infoSize;
                            LoadInfo(infoType, text);
                            listPos += infoSize + 8;
                        }
                    }
                    else {
                        LoadList(mp_list->type, ptr + sizeof(CK_LIST), mp_chunk->size);
                    }
                }
                else {
                    LoadChunk(mp_chunk->type, ptr, mp_chunk->size);
                }
                ptr += mp_chunk->size;
            }
        }

        protected virtual void LoadInfo(string type, string text) { }
        protected virtual void LoadChunk(CHUNK_TYPE type, byte* ptr, uint size) { }
        protected virtual void LoadList(LIST_TYPE type, byte* ptr, uint size) { }

        public virtual void Dispose() { }
    }
}
