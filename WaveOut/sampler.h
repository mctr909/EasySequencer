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

enum E_CH_STATE {
    E_CH_STATE_STANDBY,
    E_CH_STATE_ACTIVE
};

enum E_WAVE_FORM {
    E_WAVE_FORM_SINE,
    E_WAVE_FORM_PWM,
    E_WAVE_FORM_SAW,
    E_WAVE_FORM_TRI
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

#pragma pack(push, 8)
typedef struct WAVE_INFO {
    uint waveOfs;
    uint loopBegin;
    uint loopLength;
    Bool loopEnable;
    byte originNote;
    ushort reserved;
    double gain;
    double delta;
} WAVE_INFO;
#pragma pack(pop)

#pragma pack(push, 8)
typedef struct {
    int bufferLength;
    int bufferCount;
    int channelCount;
    int samplerCount;
    int bits;
    int sampleRate;
    double deltaTime;
} SYSTEM_VALUE;
#pragma pack(pop)

#pragma pack(push, 8)
typedef struct CHANNEL {
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
} CHANNEL;
#pragma pack(pop)

#pragma pack(push, 4)
typedef struct SAMPLER {
    byte channelNum;
    byte noteNum;
    byte state;
    byte unisonNum;
    double velocity;
    double time;
    double index;
    double egAmp;
    ENVELOPE envAmp;
    WAVE_INFO waveInfo;
} SAMPLER;
#pragma pack(pop)

#pragma pack(push, 4)
typedef struct CHANNEL_VALUE {
    byte state;
    byte reserved1;
    ushort reserved2;
    int writeIndex;
    double amp;
    double panL;
    double panR;
    double choLfoU;
    double choLfoV;
    double choLfoW;
    double choPanUL;
    double choPanUR;
    double choPanVL;
    double choPanVR;
    double choPanWL;
    double choPanWR;
    double *pDelTapL;
    double *pDelTapR;
    double *pWave;
    CHANNEL *pParam;
    SYSTEM_VALUE *pSystemValue;
    FILTER filter;
} CHANNEL_VALUE;
#pragma pack(pop)

/******************************************************************************/
#ifdef __cplusplus
extern "C" {
#endif
    __declspec(dllexport) SAMPLER** createSamplers(uint count);
    __declspec(dllexport) CHANNEL_VALUE** createChannels(SYSTEM_VALUE *pSys);
    __declspec(dllexport) void disposeSamplers(SAMPLER** ppSmpl, uint count);
    __declspec(dllexport) void disposeChannels(CHANNEL_VALUE**ppCh);
    __declspec(dllexport) inline void sampler(CHANNEL_VALUE **ppCh, SAMPLER *pSmpl, byte *pWaveBuffer);
    __declspec(dllexport) inline void effect(CHANNEL_VALUE* pCh, double* waveL, double* waveR);
#ifdef __cplusplus
}
#endif
