#pragma once
#include "filter.h"

/******************************************************************************/
typedef struct SYSTEM_VALUE SYSTEM_VALUE;

/******************************************************************************/
#pragma pack(push, 8)
typedef struct EFFECT_PARAM {
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
} EFFECT_PARAM;
#pragma pack(pop)

#pragma pack(push, 4)
typedef struct EFFECT {
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
    double* pOutput;
    EFFECT_PARAM* pParam;
    SYSTEM_VALUE* pSystemValue;
    FILTER filter;
} EFFECT;
#pragma pack(pop)

/******************************************************************************/
#ifdef __cplusplus
extern "C" {
#endif
    void effect_create(SYSTEM_VALUE* pSystemValue);
    void effect_dispose(SYSTEM_VALUE* pSystemValue);
    void effect(EFFECT* pEffect, double *pInput, double* pOutputL, double* pOutputR);
#ifdef __cplusplus
}
#endif
