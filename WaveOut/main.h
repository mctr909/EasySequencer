#pragma once
#include <windows.h>
#include "struct.h"

/******************************************************************************/
__declspec(dllexport) BOOL WINAPI WaveOutOpen(UInt32 sampleRate, UInt32 waveBufferLength);
__declspec(dllexport) VOID WINAPI WaveOutClose();
__declspec(dllexport) VOID WINAPI FileOutOpen(LPWSTR filePath, UInt32 bufferLength);
__declspec(dllexport) VOID WINAPI FileOutClose();
__declspec(dllexport) VOID WINAPI FileOut();
__declspec(dllexport) CHANNEL_PARAM** WINAPI GetWaveOutChannelPtr();
__declspec(dllexport) CHANNEL_PARAM** WINAPI GetFileOutChannelPtr();
__declspec(dllexport) SAMPLER** WINAPI GetWaveOutSamplerPtr();
__declspec(dllexport) SAMPLER** WINAPI GetFileOutSamplerPtr();
__declspec(dllexport) LPBYTE WINAPI LoadDLS(LPWSTR filePath, UInt32 *size, UInt32 sampleRate);
