#pragma once
#include <windows.h>
#include "type.h"

/******************************************************************************/
typedef struct INST_LIST INST_LIST;
typedef struct CHANNEL_PARAM CHANNEL_PARAM;

/******************************************************************************/
#ifdef __cplusplus
extern "C" {
#endif
    __declspec(dllexport) int* WINAPI waveout_getActiveSamplersPtr();
    __declspec(dllexport) CHANNEL_PARAM** WINAPI waveout_getChannelParamPtr();
    __declspec(dllexport) E_LOAD_STATUS WINAPI waveout_open(
        LPWSTR filePath,
        byte *pInstList,
        int sampleRate,
        int bits,
        int bufferLength,
        int bufferCount
    );
    __declspec(dllexport) void WINAPI waveout_close();
#ifdef __cplusplus
}
#endif
