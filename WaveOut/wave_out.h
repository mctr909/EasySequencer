#pragma once
#include <windows.h>

/******************************************************************************/
#ifdef __cplusplus
extern "C" {
#endif
    __declspec(dllexport) BOOL waveout_open(
        int sampleRate,
        int bits,
        int channelCount,
        int bufferLength,
        int bufferCount,
        void (*fpWriteBufferProc)(LPBYTE)
    );
    __declspec(dllexport) BOOL waveout_close();
#ifdef __cplusplus
}
#endif
