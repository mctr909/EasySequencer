#pragma once
#include "type.h"
#include "filter.h"

/******************************************************************************/
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
    double delayCross;
    double chorusSend;
    double chorusRate;
    double chorusDepth;
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
    FILTER filter;
} CHANNEL;
#pragma

/******************************************************************************/
extern CHANNEL** createChannels(UInt32 count, UInt32 sampleRate, UInt32 buffLen);
extern inline void channel(CHANNEL *pCh, SInt16 *waveBuff);

/******************************************************************************/
inline void delay(CHANNEL *pCh, double *waveL, double *waveR);
inline void chorus(CHANNEL *pCh, double *waveL, double *waveR);
