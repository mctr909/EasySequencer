#pragma once
#include "type.h"

/******************************************************************************/
class InstList;
typedef struct INST_SAMPLER INST_SAMPLER;
typedef struct EFFECT EFFECT;

/******************************************************************************/
#pragma pack(push, 8)
typedef struct SYSTEM_VALUE {
    InstList *cInstList;
    INST_SAMPLER **ppSampler;
    EFFECT **ppEffect;
    WAVDAT *pWaveTable;
    int bufferLength;
    int bufferCount;
    int bits;
    int sampleRate;
    double deltaTime;
} SYSTEM_VALUE;
#pragma pack(pop)

Bool sampler(SYSTEM_VALUE* pSystemValue, INST_SAMPLER* pSmpl);
