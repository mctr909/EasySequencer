#pragma once
#include "sampler.h"
#include <windows.h>

/******************************************************************************/
#ifdef __cplusplus
extern "C" {
#endif
    __declspec(dllexport) int* WINAPI waveout_GetActiveSamplersPtr();
    __declspec(dllexport) CHANNEL** WINAPI waveout_GetChannelPtr();
    __declspec(dllexport) NOTE** WINAPI waveout_GetNotePtr();
    __declspec(dllexport) SAMPLER** WINAPI waveout_GetSamplerPtr();
    __declspec(dllexport) LPBYTE WINAPI waveout_LoadWaveTable(LPWSTR filePath, uint *size);
    __declspec(dllexport) void WINAPI waveout_SystemValues(
        int sampleRate,
        int bits,
        int bufferLength,
        int bufferCount,
        int channelCount,
        int samplerCount
    );
    __declspec(dllexport) void WINAPI waveout_Open();
    __declspec(dllexport) void WINAPI waveout_Close();
#ifdef __cplusplus
}
#endif
