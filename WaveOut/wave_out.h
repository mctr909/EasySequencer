#pragma once
#include <windows.h>

/******************************************************************************/
typedef struct INST_LIST INST_LIST;

/******************************************************************************/
#ifdef __cplusplus
extern "C" {
#endif
    __declspec(dllexport) LPBYTE WINAPI waveout_loadWaveTable(LPWSTR filePath, unsigned int *size);
    __declspec(dllexport) void WINAPI waveout_systemValues(
        INST_LIST *pList,
        int sampleRate,
        int bits,
        int bufferLength,
        int bufferCount
    );
    __declspec(dllexport) int* WINAPI waveout_getActiveSamplersPtr();
    __declspec(dllexport) void WINAPI waveout_open();
    __declspec(dllexport) void WINAPI waveout_close();
#ifdef __cplusplus
}
#endif
