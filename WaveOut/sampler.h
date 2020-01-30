#pragma once
#include "type.h"

/******************************************************************************/
enum E_KEY_STATE {
    E_KEY_STATE_WAIT,
    E_KEY_STATE_PURGE,
    E_KEY_STATE_RELEASE,
    E_KEY_STATE_HOLD,
    E_KEY_STATE_PRESS
};

/******************************************************************************/
#pragma pack(8)
typedef struct FILTER {
    double cut;
    double res;
    double a00;
    double b00;
    double a01;
    double b01;
    double a10;
    double b10;
    double a11;
    double b11;
} FILTER;
#pragma

#pragma pack(8)
typedef struct ENVELOPE {
    double deltaA;
    double deltaD;
    double deltaR;
    double levelS;
    double hold;
} ENVELOPE;
#pragma

#pragma pack(4)
typedef struct WAVE_LOOP {
    UInt32 begin;
    UInt32 length;
    bool enable;
    byte type;
    byte reserved1;
    byte reserved2;
} WAVE_LOOP;
#pragma

#pragma pack(8)
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
    double chorusSend;
    double chorusRate;
} CHANNEL_PARAM;
#pragma

#pragma pack(8)
typedef struct CHANNEL {
    CHANNEL_PARAM *pParam;
    double *pWave;
    double *pDelTapL;
    double *pDelTapR;
    double choLfo[3];
    double choPanL[3];
    double choPanR[3];
    UInt32 buffLen;
    UInt32 sampleRate;
    SInt32 writeIndex;
    SInt32 readIndex;
    double amp;
    double panL;
    double panR;
    double deltaTime;
    FILTER eq;
} CHANNEL;
#pragma

#pragma pack(4)
typedef struct SAMPLER {
    UInt16 channelNo;
    byte   noteNo;
    byte   state;
    UInt32 buffOfs;

    double gain;
    double delta;
    double index;
    double time;
    double amp;
    double velocity;

    WAVE_LOOP loop;
    ENVELOPE envAmp;
    ENVELOPE envEq;
    FILTER eq;
} SAMPLER;
#pragma

/******************************************************************************/
extern CHANNEL** createChannels(UInt32 count, UInt32 sampleRate, UInt32 buffLen);
extern SAMPLER** createSamplers(UInt32 count);

/******************************************************************************/
extern inline void channel(CHANNEL *pCh, SInt16 *waveBuff);
extern inline void sampler(CHANNEL **ppCh, SAMPLER *pSmpl, byte *pWaveBuffer);

/******************************************************************************/
inline void delay(CHANNEL *pCh, double *waveL, double *waveR);
inline void chorus(CHANNEL *pCh, double *waveL, double *waveR);
inline void filter(FILTER *pFilter, double input);
