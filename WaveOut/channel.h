#pragma once
#include "type.h"
#include "filter.h"

/******************************************************************************/
#pragma pack(push, 4)
typedef struct {
    UInt32 sampleRate;
    UInt32 bufferLength;
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
    SInt32 writeIndex;
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

/******************************************************************************/
extern CHANNEL** createChannels(UInt32 count, SYSTEM_VALUE *pSys);
extern inline void channel(CHANNEL *pCh, float *waveBuff);
