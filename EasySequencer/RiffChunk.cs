using System;
using System.IO;
using System.Runtime.InteropServices;

public class RiffChunk {
    protected RiffChunk() { }

    protected RiffChunk(IntPtr ptr, uint size) {
        loop(ptr, size);
    }

    protected IntPtr Load(string path, long offset = 0, long size = 0) {
        var fs = new FileStream(path, FileMode.Open);
        fs.Seek(offset, SeekOrigin.Begin);

        if (0 == size) {
            size = fs.Length;
        }

        var pFile = Marshal.AllocHGlobal((int)size);
        if (IntPtr.Zero == pFile) {
            fs.Close();
            fs.Dispose();
            return pFile;
        }

        var readPos = 0;
        var readBuff = new byte[4096];
        while (fs.Position < fs.Length) {
            var readLen = fs.Read(readBuff, 0, readBuff.Length);
            Marshal.Copy(readBuff, 0, pFile + readPos, readLen);
            readPos += readLen;
        }
        fs.Close();
        fs.Dispose();

        var riffId = Marshal.PtrToStringAnsi(pFile, 4);
        var riffSize = Marshal.PtrToStructure<uint>(pFile + 4);
        var fileType = Marshal.PtrToStringAnsi(pFile + 8, 4);
        if ("RIFF" == riffId && CheckFileType(fileType, riffSize)) {
            loop(pFile + 12, riffSize - 4);
        }

        return pFile;
    }

    protected virtual bool CheckFileType(string type, uint size) { return false; }
    protected virtual void LoadChunk(IntPtr ptr, string type, uint size) { }
    protected virtual void LoadList(IntPtr ptr, string type, uint size) { }
    protected virtual void LoadInfo(IntPtr ptr, string type, uint size) { }

    private void loop(IntPtr ptr, uint size) {
        uint pos = 0;
        while (pos < size) {
            var chunkId = Marshal.PtrToStringAnsi(ptr, 4);
            var chunkSize = Marshal.PtrToStructure<uint>(ptr + 4);
            ptr += 8;
            pos += 8;
            if (0 == chunkSize) {
                break;
            }

            if ("LIST" == chunkId) {
                var listType = Marshal.PtrToStringAnsi(ptr, 4);
                if ("INFO" == listType) {
                    infoLoop(ptr + 4, chunkSize - 4);
                } else {
                    LoadList(ptr + 4, listType, chunkSize - 4);
                }
            } else {
                LoadChunk(ptr, chunkId, chunkSize);
            }

            ptr += (int)chunkSize;
            pos += chunkSize;
        }
    }

    private void infoLoop(IntPtr ptr, uint size) {
        uint pos = 0;
        while (pos < size) {
            var infoType = Marshal.PtrToStringAnsi(ptr, 4);
            var infoSize = Marshal.PtrToStructure<uint>(ptr + 4);
            ptr += 8;
            pos += 8;
            if (0 == infoSize) {
                break;
            }

            LoadInfo(ptr, infoType, infoSize);

            infoSize += (uint)(infoSize % 2 == 0 ? 0 : 1);
            ptr += (int)infoSize;
            pos += infoSize;
        }
    }
}
