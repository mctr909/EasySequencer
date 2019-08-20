#pragma once
#include "type.h"

/******************************************************************************/
#pragma pack(8)
typedef struct DELAY {
    SInt32 writeIndex;
    SInt32 readIndex;
    double *pTapL;
    double *pTapR;
} DELAY;
#pragma

#pragma pack(8)
typedef struct CHORUS {
    double lfoK;
    double *pPanL;
    double *pPanR;
    double *pLfoRe;
    double *pLfoIm;
} CHORUS;
#pragma

#pragma pack(8)
typedef struct FILTER {
    double cut; //   0
    double res; //   8
    double a0;  //  16
    double b0;  //  24
    double a1;  //  32
    double b1;  //  40
    double a2;  //  48
    double b2;  //  56
    double a3;  //  64
    double b3;  //  72
} FILTER;
#pragma

#pragma pack(8)
typedef struct ENVELOPE {
    double rise;
    double top;
    double sustain;
    double fall;
    double deltaA;
    double deltaD;
    double deltaR;
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

    double delayDepth;
    double delayTime;
    double chorusDepth;
    double chorusRate;
} CHANNEL_PARAM;
#pragma

#pragma pack(4)
typedef struct CHANNEL {
    CHANNEL_PARAM *param;

    double wave;
    double waveL;
    double waveR;

    double amp;
    double panLeft;
    double panRight;

    double deltaTime;
    UInt32 sampleRate;

    FILTER eq;
    DELAY delay;
    CHORUS chorus;
} CHANNEL;
#pragma

#pragma pack(4)
typedef struct SAMPLER {
    UInt16 channelNo;
    byte   noteNo;
    byte   keyState;

    UInt32 pcmAddr;
    UInt32 pcmLength;

    double gain;
    double delta;

    double index;
    double time;

    double velocity;
    double amp;

    WAVE_LOOP loop;
    ENVELOPE envAmp;
    ENVELOPE envEq;
    FILTER eq;
} SAMPLER;
#pragma

/******************************************************************************/
enum E_KEY_STATE {
    E_KEY_STATE_WAIT,
    E_KEY_STATE_PURGE,
    E_KEY_STATE_RELEASE,
    E_KEY_STATE_HOLD,
    E_KEY_STATE_PRESS
};

/******************************************************************************/
extern CHANNEL** createChannels(UInt32 count, UInt32 sampleRate);
extern SAMPLER** createSamplers(UInt32 count);

/******************************************************************************/
extern inline void channel(CHANNEL *ch, double *waveL, double *waveR);
extern inline void sampler(CHANNEL **chs, SAMPLER *smpl, byte *pDlsBuffer);

/******************************************************************************/
inline void delay(CHANNEL *ch, DELAY *delay, double *waveL, double *waveR);
inline void chorus(CHANNEL *ch, DELAY *delay, CHORUS *chorus, double *waveL, double *waveR);
inline void filter(FILTER *param, double input);
