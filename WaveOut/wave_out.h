#pragma once
#include <windows.h>

/******************************************************************************/
BOOL waveout_open(
    int sampleRate,
    int bits,
    int channelCount,
    int bufferLength,
    int bufferCount,
    void (*fpWriteBufferProc)(LPBYTE)
);

BOOL waveout_close();
