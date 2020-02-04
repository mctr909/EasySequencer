#pragma once
#include <windows.h>
#include "sampler.h"
#include "channel.h"

/******************************************************************************/
__declspec(dllexport) VOID WINAPI SystemValues(UInt32 sampleRate, UInt32 waveBufferLength);
__declspec(dllexport) BOOL WINAPI WaveOutOpen();
__declspec(dllexport) VOID WINAPI WaveOutClose();
__declspec(dllexport) VOID WINAPI FileOutOpen(LPWSTR filePath);
__declspec(dllexport) VOID WINAPI FileOutClose();
__declspec(dllexport) VOID WINAPI FileOut();
__declspec(dllexport) SInt32* WINAPI GetActiveCountPtr();
__declspec(dllexport) CHANNEL_PARAM** WINAPI GetWaveOutChannelPtr();
__declspec(dllexport) CHANNEL_PARAM** WINAPI GetFileOutChannelPtr();
__declspec(dllexport) SAMPLER** WINAPI GetWaveOutSamplerPtr();
__declspec(dllexport) SAMPLER** WINAPI GetFileOutSamplerPtr();
__declspec(dllexport) LPBYTE WINAPI LoadFile(LPWSTR filePath, UInt32 *size);
