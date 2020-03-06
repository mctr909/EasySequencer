#pragma once
#include "sampler.h"
#include <windows.h>

/******************************************************************************/
__declspec(dllexport) int* WINAPI waveout_GetActiveSamplersPtr();
__declspec(dllexport) CHANNEL** WINAPI waveout_GetChannelPtr();
__declspec(dllexport) SAMPLER** WINAPI waveout_GetSamplerPtr();

/******************************************************************************/
__declspec(dllexport) LPBYTE WINAPI waveout_LoadWaveTable(LPWSTR filePath, uint *size);
__declspec(dllexport) VOID WINAPI waveout_SystemValues(
    int sampleRate,
    int bits,
    int bufferLength,
    int bufferCount,
    int channelCount,
    int samplerCount
);
__declspec(dllexport) BOOL WINAPI waveout_Open();
__declspec(dllexport) VOID WINAPI waveout_Close();
__declspec(dllexport) VOID WINAPI waveout_Dispose();
