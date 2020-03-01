#pragma once
#include "type.h"
#include "filter.h"
#include "channel.h"

/******************************************************************************/
enum E_KEY_STATE {
    E_KEY_STATE_STANDBY,
    E_KEY_STATE_PURGE,
    E_KEY_STATE_RELEASE,
    E_KEY_STATE_HOLD,
    E_KEY_STATE_PRESS
};

/******************************************************************************/
#pragma pack(push, 8)
typedef struct ENVELOPE {
    double attack;
    double decay;
    double release;
    double rise;
    double top;
    double sustain;
    double fall;
    double hold;
} ENVELOPE;
#pragma pack(pop)

#pragma pack(push, 4)
typedef struct WAVE_LOOP {
    UInt32 begin;
    UInt32 length;
    bool enable;
    byte type;
    byte reserved1;
    byte reserved2;
} WAVE_LOOP;
#pragma pack(pop)

#pragma pack(push, 4)
typedef struct SAMPLER {
    UInt16 channelNo;
    byte   noteNo;
    byte   state;
    UInt32 waveOfs;

    double velocity;
    double gain;
    double delta;
    double index;
    double time;
    double egAmp;
    double egPitch;

    WAVE_LOOP loop;
    ENVELOPE envAmp;
    ENVELOPE envPitch;
    ENVELOPE envEq;
    FILTER filter;
} SAMPLER;
#pragma pack(pop)

/******************************************************************************/
extern SAMPLER** createSamplers(UInt32 count);

extern inline void sampler(CHANNEL **ppCh, SAMPLER *pSmpl, byte *pWaveBuffer);
