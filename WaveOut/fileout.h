#pragma once
#include "type.h"
#include <windows.h>

/******************************************************************************/
#ifdef __cplusplus
extern "C" {
#endif
__declspec(dllexport) int* WINAPI fileout_getProgressPtr();
__declspec(dllexport) void WINAPI fileout_save(
    LPWSTR waveTablePath,
    LPWSTR savePath,
    uint sampleRate,
    uint bitRate,
    LPBYTE ppEvents,
    uint eventSize,
    uint baseTick
);
#ifdef __cplusplus
}
#endif
