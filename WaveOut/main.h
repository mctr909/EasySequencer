#pragma once
#include <windows.h>
#include "sampler.h"

/******************************************************************************/
__declspec(dllexport) LPBYTE WINAPI LoadFile(LPWSTR filePath, uint *size);
__declspec(dllexport) VOID WINAPI SystemValues(uint sampleRate, uint waveBufferLength);
__declspec(dllexport) int* WINAPI GetActiveCountPtr();
__declspec(dllexport) BOOL WINAPI WaveOutOpen();
__declspec(dllexport) VOID WINAPI WaveOutClose();
__declspec(dllexport) VOID WINAPI FileOutOpen(LPWSTR filePath);
__declspec(dllexport) VOID WINAPI FileOutClose();
__declspec(dllexport) VOID WINAPI FileOut();
__declspec(dllexport) CHANNEL_PARAM** WINAPI GetWaveOutChannelPtr();
__declspec(dllexport) CHANNEL_PARAM** WINAPI GetFileOutChannelPtr();
__declspec(dllexport) SAMPLER** WINAPI GetWaveOutSamplerPtr();
__declspec(dllexport) SAMPLER** WINAPI GetFileOutSamplerPtr();
