#pragma once
#include "../WaveOut/sampler.h"
#include "channel.h"
#include <windows.h>

/******************************************************************************/
#ifdef __cplusplus
extern "C" {
#endif
    __declspec(dllexport) CHANNEL** WINAPI wavfileout_GetChannelPtr();
    __declspec(dllexport) SAMPLER** WINAPI wavfileout_GetSamplerPtr();
    __declspec(dllexport) void WINAPI wavfileout_Open(LPWSTR filePath, LPBYTE pWaveTable, uint sampleRate, uint bitRate);
    __declspec(dllexport) void WINAPI wavfileout_Close();
    __declspec(dllexport) void WINAPI wavfileout_Write();
    __declspec(dllexport) CHANNEL_PARAM** WINAPI midi_GetChannelParamPtr();
    __declspec(dllexport) void WINAPI midi_CreateChannels(INST_LIST *list, SAMPLER **ppSmpl, CHANNEL **ppCh, uint samplerCount);
    __declspec(dllexport) void WINAPI midi_Send(LPBYTE msg);
#ifdef __cplusplus
}
#endif
