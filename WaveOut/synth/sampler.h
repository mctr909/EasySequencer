#pragma once
#include "../type.h"

/******************************************************************************/
#define SAMPLER_COUNT 128

/******************************************************************************/
typedef struct INST_LAYER INST_LAYER;
typedef struct INST_REGION INST_REGION;
typedef struct INST_ENV INST_ENV;
typedef struct SYSTEM_VALUE SYSTEM_VALUE;
class Channel;

/******************************************************************************/
class Sampler {
public:
    enum struct E_STATE : byte {
        FREE,
        RESERVED,
        PURGE,
        PRESS,
        RELEASE,
        HOLD
    };

private:
    SYSTEM_VALUE* mpSystemValue = 0;
    Channel* mpChannel = 0;
    WAVDAT* mpWaveData = 0;
    long loop_length = 0;
    long loop_end = 0;

    int16 pan = 0;
    double gain = 1.0;
    double index = 0.0;
    double time = 0.0;
    double pitch = 1.0;
    double eg_amp = 0.0;
    double eg_cutoff = 1.0;
    double eg_pitch = 1.0;
    INST_ENV* pEnv = 0;

public:
    E_STATE state = E_STATE::FREE;
    byte channel_num = 0;
    byte note_num = 0;
    bool loop_enable = false;

public:
    Sampler(SYSTEM_VALUE* pSystemValue);
    void note_on(Channel* pChannel, INST_LAYER* pLayer, INST_REGION *pRegion, byte note_num, byte velocity);
    bool step();
};
