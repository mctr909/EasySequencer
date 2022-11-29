#pragma once
#include "type.h"
#include <windows.h>

/******************************************************************************/
#ifdef __cplusplus
extern "C" {
#endif
__declspec(dllexport) int32* WINAPI fileout_getProgressPtr();
__declspec(dllexport) void WINAPI fileout_save(
    LPWSTR waveTablePath,
    LPWSTR savePath,
    uint32 sampleRate,
    byte *pEvents,
    uint32 eventSize,
    uint32 baseTick
);
#ifdef __cplusplus
}
#endif
