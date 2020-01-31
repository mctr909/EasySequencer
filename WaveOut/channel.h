#pragma once
#include "type.h"
#include "filter.h"

/******************************************************************************/
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
    double *pWave;
    double *pDelTapL;
    double *pDelTapR;
    double choLfo[3];
    double choPanL[3];
    double choPanR[3];
    UInt32 buffLen;
    UInt32 sampleRate;
    SInt32 writeIndex;
    double amp;
    double panL;
    double panR;
    double deltaTime;
    FILTER filter;
} CHANNEL;
#pragma pack(pop)

/******************************************************************************/
extern CHANNEL* createChannel(UInt32 sampleRate, UInt32 buffLen);
extern void releaseChannel(CHANNEL *pCh);
extern inline void channel(CHANNEL *pCh, SInt16 *waveBuff);
