#pragma once
#include "../type.h"

/******************************************************************************/
#define SAMPLER_COUNT 128

/******************************************************************************/
typedef struct INST_ENV INST_ENV;
typedef struct INST_WAVE INST_WAVE;
typedef struct SYSTEM_VALUE SYSTEM_VALUE;

/******************************************************************************/
enum struct E_SAMPLER_STATE : byte {
    FREE,
    RESERVED,
    PURGE,
    PRESS,
    RELEASE,
    HOLD
};

/******************************************************************************/
#pragma pack(4)
struct INST_SAMPLER {
    E_SAMPLER_STATE state = E_SAMPLER_STATE::FREE;
    byte channel_num = 0;
    byte note_num = 0;
    byte reserved1 = 0;
    int16 pan = 0;
    int16 reserved2 = 0;
    double gain = 1.0;
    double index = 0.0;
    double time = 0.0;
    double pitch = 1.0;
    double eg_amp = 0.0;
    double eg_cutoff = 1.0;
    double eg_pitch = 1.0;
    INST_ENV* pEnv = 0;
    INST_WAVE* pWave = 0;
};
#pragma pack()

/******************************************************************************/
extern inline Bool sampler(SYSTEM_VALUE* pSystemValue, INST_SAMPLER* pSmpl);
