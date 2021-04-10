#pragma once
#include "../WaveOut/sampler.h"
#include "channel.h"
#include <windows.h>

/******************************************************************************/
#ifdef __cplusplus
extern "C" {
#endif
    __declspec(dllexport) CHANNEL** WINAPI midi_GetChannelParamPtr();
    __declspec(dllexport) int* midi_GetWavFileOutProgressPtr();
    __declspec(dllexport) void WINAPI midi_CreateChannels(INST_LIST *list, SAMPLER **ppSmpl, Note **ppNote, CHANNEL_PARAM **ppCh, uint samplerCount);
    __declspec(dllexport) void WINAPI midi_Send(LPBYTE msg);
    __declspec(dllexport) void WINAPI midi_WavFileOut(
        LPWSTR filePath,
        LPBYTE pWaveTable,
        INST_LIST *list,
        uint sampleRate,
        uint bitRate,
        LPBYTE ppEvents,
        uint eventSize,
        uint baseTick
    );
#ifdef __cplusplus
}
#endif
