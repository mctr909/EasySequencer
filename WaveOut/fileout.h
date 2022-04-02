#pragma once
#include "instruments.h"
#include <windows.h>

/******************************************************************************/
#ifdef __cplusplus
extern "C" {
#endif
__declspec(dllexport) int* WINAPI waveout_getFileOutProgressPtr();
__declspec(dllexport) void WINAPI waveout_fileOut(
    LPWSTR filePath,
    LPBYTE pWaveTable,
    INST_LIST *pList,
    uint sampleRate,
    uint bitRate,
    LPBYTE ppEvents,
    uint eventSize,
    uint baseTick
);
#ifdef __cplusplus
}
#endif
