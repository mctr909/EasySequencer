#pragma once
#include <windows.h>
#include <mmsystem.h>

#include "struct.h"

#pragma comment (lib, "winmm.lib")

/******************************************************************************/
#define BUFFER_COUNT        8
#define CHANNEL_COUNT       16
#define SAMPLER_COUNT       128

/******************************************************************************/
__declspec(dllexport) BOOL WINAPI WaveOutOpen(UInt32 sampleRate, UInt32 bufferLength);
__declspec(dllexport) VOID WINAPI WaveOutClose();
__declspec(dllexport) CHANNEL** WINAPI GetChannelPtr();
__declspec(dllexport) SAMPLER** WINAPI GetSamplerPtr();
__declspec(dllexport) LPBYTE WINAPI LoadDLS(LPWSTR filePath, UInt32 *size, UInt32 sampleRate);

/******************************************************************************/
void CALLBACK WaveOutProc(HWAVEOUT hwo, UInt32 uMsg);
