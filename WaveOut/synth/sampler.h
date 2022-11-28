#pragma once
#include "../type.h"

/******************************************************************************/
class InstList;
class Channel;
typedef struct INST_SAMPLER INST_SAMPLER;
typedef struct EFFECT EFFECT;
typedef struct CHANNEL_PARAM CHANNEL_PARAM;

/******************************************************************************/
#pragma pack(push, 8)
typedef struct SYSTEM_VALUE {
    InstList *cInstList;
    INST_SAMPLER **ppSampler;
    Channel **ppChannels;
    CHANNEL_PARAM **ppChannelParam;
    WAVDAT *pWaveTable;
    int bufferLength;
    int bufferCount;
    int sampleRate;
    double deltaTime;
    double* pBufferL;
    double* pBufferR;
} SYSTEM_VALUE;
#pragma pack(pop)

extern inline Bool sampler(SYSTEM_VALUE* pSystemValue, INST_SAMPLER* pSmpl);
