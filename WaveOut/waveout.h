#pragma once
#include <windows.h>

/******************************************************************************/
typedef struct INST_LIST INST_LIST;
typedef struct CHANNEL_PARAM CHANNEL_PARAM;

/******************************************************************************/
#ifdef __cplusplus
extern "C" {
#endif
    __declspec(dllexport) int* WINAPI waveout_getActiveSamplersPtr();
    __declspec(dllexport) CHANNEL_PARAM** WINAPI waveout_getChannelParamPtr();
    __declspec(dllexport) byte* WINAPI waveout_open(
        LPWSTR filePath,
        int sampleRate,
        int bits,
        int bufferLength,
        int bufferCount
    );
    __declspec(dllexport) void WINAPI waveout_close();
#ifdef __cplusplus
}
#endif
