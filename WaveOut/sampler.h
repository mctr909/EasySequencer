#pragma once
#include "type.h"
#include "filter.h"

/******************************************************************************/
enum E_KEY_STATE {
    E_KEY_STATE_STANDBY,
    E_KEY_STATE_PURGE,
    E_KEY_STATE_RELEASE,
    E_KEY_STATE_HOLD,
    E_KEY_STATE_PRESS
};

/******************************************************************************/
#pragma pack(push, 4)
typedef struct {
    uint bufferLength;
    uint sampleRate;
    double deltaTime;
} SYSTEM_VALUE;
#pragma pack(pop)

#pragma pack(push, 8)
typedef struct CHANNEL_PARAM {
    double amp;
    double pitch;
    double holdDelta;
    double panLeft;
    double panRight;
    double cutoff;
    double resonance;
    double delaySend;
    double delayTime;
    double delayCross;
    double chorusSend;
    double chorusRate;
    double chorusDepth;
} CHANNEL_PARAM;
#pragma pack(pop)

#pragma pack(push, 4)
typedef struct CHANNEL {
    CHANNEL_PARAM *pParam;
    SYSTEM_VALUE *pSystemValue;
    int writeIndex;
    double *pWave;
    double *pDelTapL;
    double *pDelTapR;
    double choLfo[3];
    double choPanL[3];
    double choPanR[3];
    double amp;
    double panL;
    double panR;
    FILTER filter;
} CHANNEL;
#pragma pack(pop)

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
    uint waveOfs;
    uint loopBegin;
    uint loopLength;
    bool loopEnable;
    byte unityNote;
    ushort reserved;
    double gain;
    double delta;
} WAVE_INFO;
#pragma pack(pop)

#pragma pack(push, 4)
typedef struct SAMPLER {
    ushort    channelNum;
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
extern SAMPLER** createSamplers(uint count);
extern CHANNEL** createChannels(uint count, SYSTEM_VALUE *pSys);
extern inline void sampler(CHANNEL **ppCh, SAMPLER *pSmpl, byte *pWaveBuffer);
extern inline void channel(CHANNEL *pCh, float *waveBuff);
