#pragma once
#include "sampler.h"

#ifdef __cplusplus
extern "C" {
#endif
    __declspec(dllexport) CHANNEL_VALUE** createChannels(SYSTEM_VALUE* pSys);
    __declspec(dllexport) void disposeChannels(CHANNEL_VALUE** ppCh);
    __declspec(dllexport) inline void effect(CHANNEL_VALUE* pCh, double* waveL, double* waveR);
#ifdef __cplusplus
}
#endif
