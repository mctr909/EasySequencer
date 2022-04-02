#pragma once
#include "type.h"

/******************************************************************************/
#define SAMPLER_COUNT 64

/******************************************************************************/
enum struct E_SAMPLER_STATE : ushort {
    FREE,
    RESERVED,
    PRESS,
    RELEASE,
    HOLD,
    PURGE
};

/******************************************************************************/
typedef struct ENV_AMP ENV_AMP;
typedef struct ENV_FILTER ENV_FILTER;
typedef struct ENV_PITCH ENV_PITCH;
typedef struct WAVE_INFO WAVE_INFO;
typedef struct INST_LIST INST_LIST;
typedef struct EFFECT EFFECT;

/******************************************************************************/
#pragma pack(push, 4)
typedef struct SAMPLER {
    byte channelNumber;
    byte noteNumber;
    E_SAMPLER_STATE state;
    double velocity;
    double delta;
    double index;
    double time;
    double egAmp;
    double egFilter;
    double egPitch;
    ENV_AMP *pEnvAmp;
    ENV_FILTER *pEnvFilter;
    ENV_PITCH *pEnvPitch;
    WAVE_INFO *pWaveInfo;
} SAMPLER;
#pragma pack(pop)

#pragma pack(push, 8)
typedef struct SYSTEM_VALUE {
    INST_LIST *pInstList;
    SAMPLER **ppSampler;
    EFFECT **ppEffect;
    byte *pWaveTable;
    int bufferLength;
    int bufferCount;
    int bits;
    int sampleRate;
    double deltaTime;
} SYSTEM_VALUE;
#pragma pack(pop)

#ifdef __cplusplus
extern "C" {
#endif
    void sampler_create(SYSTEM_VALUE* pSystemValue);
    void sampler_dispose(SYSTEM_VALUE* pSystemValue);
    Bool sampler(SYSTEM_VALUE *pSystemValue, SAMPLER* pSmpl);
#ifdef __cplusplus
}
#endif
