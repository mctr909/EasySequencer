#pragma once
#include "type.h"
#include <windows.h>

/******************************************************************************/
typedef struct INST_LIST INST_LIST;
typedef struct CHANNEL_PARAM CHANNEL_PARAM;

/******************************************************************************/
#ifdef __cplusplus
extern "C" {
#endif
    __declspec(dllexport) void WINAPI waveout_open(
        LPWSTR filePath,
        int32 sampleRate,
        int32 bufferLength,
        int32 bufferCount
    );
    __declspec(dllexport) void WINAPI waveout_close();
#ifdef __cplusplus
}
#endif
