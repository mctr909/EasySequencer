#pragma once
#include "type.h"
#include "filter.h"
#include "note.h"

/******************************************************************************/
struct SYSTEM_VALUE;
struct CHANNEL_PARAM;
struct CHANNEL_VALUE;
struct ENVELOPE;
struct WAVE_INFO;
struct SAMPLER;

/******************************************************************************/
#pragma pack(push, 8)
typedef struct SYSTEM_VALUE {
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
typedef struct CHANNEL_VALUE {
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
    double* pDelTapL;
    double* pDelTapR;
    double* pWave;
    CHANNEL_PARAM* pParam;
    SYSTEM_VALUE* pSystemValue;
    FILTER filter;
} CHANNEL_VALUE;
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

#pragma pack(push, 4)
typedef struct SAMPLER {
    int unisonNum;
    double delta;
    double index;
    double time;
    double egAmp;
    ENVELOPE* pEnvAmp;
    WAVE_INFO* pWaveInfo;
    void* pNote;
} SAMPLER;
#pragma pack(pop)

#ifdef __cplusplus
extern "C" {
#endif
    __declspec(dllexport) SAMPLER** createSamplers(int count);
    __declspec(dllexport) void disposeSamplers(SAMPLER** ppSmpl, int count);
    __declspec(dllexport) inline Bool sampler(CHANNEL_VALUE **ppCh, SAMPLER *pSmpl, byte *pWaveBuffer);
#ifdef __cplusplus
}
#endif
