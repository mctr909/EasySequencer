#pragma once
#include <windows.h>
#include "sampler.h"

/******************************************************************************/
#pragma pack(4)
typedef struct {
    UInt32 riff;
    UInt32 fileSize;
    UInt32 dataId;
} RIFF;
#pragma

#pragma pack(4)
typedef struct {
    UInt32 chunkId;
    UInt32 chunkSize;
    UInt16 formatId;
    UInt16 channels;
    UInt32 sampleRate;
    UInt32 bytePerSec;
    UInt16 blockAlign;
    UInt16 bitPerSample;
    UInt32 dataId;
    UInt32 dataSize;
} FMT_;
#pragma

/******************************************************************************/
__declspec(dllexport) BOOL WINAPI WaveOutOpen(UInt32 sampleRate, UInt32 waveBufferLength);
__declspec(dllexport) VOID WINAPI WaveOutClose();
__declspec(dllexport) VOID WINAPI FileOutOpen(LPWSTR filePath, UInt32 bufferLength);
__declspec(dllexport) VOID WINAPI FileOutClose();
__declspec(dllexport) VOID WINAPI FileOut();
__declspec(dllexport) CHANNEL_PARAM** WINAPI GetWaveOutChannelPtr(UInt32 sampleRate);
__declspec(dllexport) CHANNEL_PARAM** WINAPI GetFileOutChannelPtr(UInt32 sampleRate);
__declspec(dllexport) SAMPLER** WINAPI GetWaveOutSamplerPtr(UInt32 samplers);
__declspec(dllexport) SAMPLER** WINAPI GetFileOutSamplerPtr(UInt32 samplers);
__declspec(dllexport) LPBYTE WINAPI LoadDLS(LPWSTR filePath, UInt32 *size);

/******************************************************************************/
void CALLBACK waveOutProc(HWAVEOUT hwo, UInt32 uMsg);
