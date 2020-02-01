#include <math.h>
#include <stdlib.h>
#include <string.h>
#include "channel.h"
#include "filter.h"

/******************************************************************************/
#define CHORUS_PHASES          3
#define DELAY_TAPS             1048576
#define VALUE_TRANSITION_SPEED 250

#define PI        3.14159265
#define PI2       6.28318531
#define INV_SQRT3 0.577350269

/******************************************************************************/
inline void effect(CHANNEL *pCh, double *waveL, double *waveR);

/******************************************************************************/
CHANNEL** createChannels(UInt32 count, UInt32 sampleRate, UInt32 buffLen) {
    CHANNEL **channel = (CHANNEL**)malloc(sizeof(CHANNEL*) * count);
    for (UInt32 i = 0; i < count; ++i) {
        CHANNEL *pCh = (CHANNEL*)malloc(sizeof(CHANNEL));
        memset(pCh, 0, sizeof(CHANNEL));
        channel[i] = pCh;

        pCh->buffLen = buffLen;
        pCh->pWave = (double*)malloc(sizeof(double) * buffLen);
        memset(pCh->pWave, 0, sizeof(double) * buffLen);

        // CHANNEL_PARAM
        pCh->pParam = (CHANNEL_PARAM*)malloc(sizeof(CHANNEL_PARAM));
        memset(pCh->pParam, 0, sizeof(CHANNEL_PARAM));

        pCh->sampleRate = sampleRate;
        pCh->deltaTime = 1.0 / sampleRate;

        // Delay
        pCh->writeIndex = 0;
        pCh->pDelTapL = (double*)malloc(sizeof(double) * DELAY_TAPS);
        pCh->pDelTapR = (double*)malloc(sizeof(double) * DELAY_TAPS);
        memset(pCh->pDelTapL, 0, sizeof(double) * DELAY_TAPS);
        memset(pCh->pDelTapR, 0, sizeof(double) * DELAY_TAPS);

        // Chorus
        pCh->choLfo[0] = 1.0;
        pCh->choLfo[1] = -0.5;
        pCh->choLfo[2] = -0.5;
        for (SInt32 p = 0; p < CHORUS_PHASES; ++p) {
            pCh->choPanL[p] = cos(0.5 * PI * p / CHORUS_PHASES);
            pCh->choPanR[p] = sin(0.5 * PI * p / CHORUS_PHASES);
        }

        // Filter
        memset(&pCh->filter, 0, sizeof(FILTER));
    }

    return channel;
}

inline void channel(CHANNEL *pCh, float *outBuff) {
    double *inputBuff = pCh->pWave;
    double *inputBuffTerm = inputBuff + pCh->buffLen;
    for (; inputBuff < inputBuffTerm; inputBuff++, outBuff += 2) {
        // filter
        filter(&pCh->filter, *inputBuff * pCh->amp);
        // pan
        double tempL = pCh->filter.a10 * pCh->panL;
        double tempR = pCh->filter.a10 * pCh->panR;
        // effect
        effect(pCh, &tempL, &tempR);
        // output
        tempL += *(outBuff + 0);
        tempR += *(outBuff + 1);
        if (1.0 < tempL) tempL = 1.0;
        if (tempL < -1.0) tempL = -1.0;
        if (1.0 < tempR) tempR = 1.0;
        if (tempR < -1.0) tempR = -1.0;
        *(outBuff + 0) = (float)tempL;
        *(outBuff + 1) = (float)tempR;

        // next step
        double transitionDelta = pCh->deltaTime * VALUE_TRANSITION_SPEED;
        pCh->amp        += (pCh->pParam->amp       - pCh->amp)        * transitionDelta;
        pCh->panL       += (pCh->pParam->panLeft   - pCh->panL)       * transitionDelta;
        pCh->panR       += (pCh->pParam->panRight  - pCh->panR)       * transitionDelta;
        pCh->filter.cut += (pCh->pParam->cutoff    - pCh->filter.cut) * transitionDelta;
        pCh->filter.res += (pCh->pParam->resonance - pCh->filter.res) * transitionDelta;
        *inputBuff = 0.0;
    }
}

/******************************************************************************/
inline void effect(CHANNEL *pCh, double *waveL, double *waveR) {
    CHANNEL_PARAM *pParam = pCh->pParam;
    double *pTapL = pCh->pDelTapL;
    double *pTapR = pCh->pDelTapR;
    pCh->writeIndex++;
    if (DELAY_TAPS <= pCh->writeIndex) {
        pCh->writeIndex = 0;
    }
    SInt32 readIndex = pCh->writeIndex - (SInt32)(pParam->delayTime * pCh->sampleRate);
    if (readIndex < 0) {
        readIndex += DELAY_TAPS;
    }
    /*** output delay ***/
    double delayL = pParam->delaySend * pTapL[readIndex];
    double delayR = pParam->delaySend * pTapR[readIndex];
    *waveL += (delayL * (1.0 - pParam->delayCross) + delayR * pParam->delayCross);
    *waveR += (delayR * (1.0 - pParam->delayCross) + delayL * pParam->delayCross);
    pTapL[pCh->writeIndex] = *waveL;
    pTapR[pCh->writeIndex] = *waveR;
    /*** mix chorus ***/
    double chorusL = 0.0;
    double chorusR = 0.0;
    for (SInt32 ph = 0; ph < CHORUS_PHASES; ++ph) {
        double pos = pCh->writeIndex - (0.505 + 0.495 * pCh->choLfo[ph]) * pCh->sampleRate * pParam->chorusDepth;
        SInt32 cur = (SInt32)pos;
        SInt32 pre = cur - 1;
        double dt = pos - cur;
        if (cur < 0) {
            cur += DELAY_TAPS;
        }
        if (DELAY_TAPS <= cur) {
            cur -= DELAY_TAPS;
        }
        if (pre < 0) {
            pre += DELAY_TAPS;
        }
        if (DELAY_TAPS <= pre) {
            pre -= DELAY_TAPS;
        }
        chorusL += (pTapL[pre] * (1.0 - dt) + pTapL[cur] * dt) * pCh->choPanL[ph];
        chorusR += (pTapR[pre] * (1.0 - dt) + pTapR[cur] * dt) * pCh->choPanR[ph];
    }
    /*** update lfo ***/
    double lfoDelta = PI2 * INV_SQRT3 * pParam->chorusRate * pCh->deltaTime;
    pCh->choLfo[0] += (pCh->choLfo[1] - pCh->choLfo[2]) * lfoDelta;
    pCh->choLfo[1] += (pCh->choLfo[2] - pCh->choLfo[0]) * lfoDelta;
    pCh->choLfo[2] += (pCh->choLfo[0] - pCh->choLfo[1]) * lfoDelta;
    /*** output chorus ***/
    *waveL += chorusL * pParam->chorusSend / CHORUS_PHASES;
    *waveR += chorusR * pParam->chorusSend / CHORUS_PHASES;
}
