#pragma once
#include <windows.h>

/******************************************************************************/
typedef struct INST_LIST INST_LIST;

/******************************************************************************/
#ifdef __cplusplus
extern "C" {
#endif
    __declspec(dllexport) LPBYTE WINAPI waveout_LoadWaveTable(LPWSTR filePath, unsigned int *size);
    __declspec(dllexport) void WINAPI waveout_SystemValues(
        INST_LIST *pList,
        int sampleRate,
        int bits,
        int bufferLength,
        int bufferCount
    );
    __declspec(dllexport) int* WINAPI waveout_GetActiveSamplersPtr();
    __declspec(dllexport) void WINAPI waveout_Open();
    __declspec(dllexport) void WINAPI waveout_Close();
#ifdef __cplusplus
}
#endif
