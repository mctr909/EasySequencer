#pragma once
#include "type.h"
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
typedef struct WAVE_INFO {
    UInt32 waveOfs;
    UInt32 loopBegin;
    UInt32 loopLength;
    bool   loopEnable;
    byte   unityNote;
    UInt16 reserved;
    double gain;
    double delta;
} WAVE_INFO;
#pragma pack(pop)

#pragma pack(push, 4)
typedef struct SAMPLER {
    UInt16    channelNum;
    byte      noteNum;
    byte      state;
    double    velocity;
    double    index;
    double    time;
    double    egAmp;
    ENVELOPE  envAmp;
    WAVE_INFO waveInfo;
} SAMPLER;
#pragma pack(pop)

/******************************************************************************/
extern SAMPLER** createSamplers(UInt32 count);

extern inline void sampler(CHANNEL **ppCh, SAMPLER *pSmpl, byte *pWaveBuffer);
