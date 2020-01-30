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
        pCh->readIndex = 0;
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

inline void channel(CHANNEL *pCh, SInt16 *outBuff) {
    double *inputBuff = pCh->pWave;
    for (UInt32 i = 0; i < pCh->buffLen; i++) {
        // filter
        filter(&pCh->filter, *inputBuff * pCh->amp);
        // pan
        double tempL = pCh->filter.a10 * pCh->panL;
        double tempR = pCh->filter.a10 * pCh->panR;
        // effect
        delay(pCh, &tempL, &tempR);
        chorus(pCh, &tempL, &tempR);
        // output
        tempL = tempL * 32767 + *outBuff;
        tempR = tempR * 32767 + *(outBuff + 1);
        if (32767 < tempL) tempL = 32767;
        if (tempL < -32767) tempL = -32767;
        if (32767 < tempR) tempR = 32767;
        if (tempR < -32767) tempR = -32767;
        *outBuff = (SInt16)tempL;
        *(outBuff + 1) = (SInt16)tempR;
        outBuff += 2;
        // next step
        double transitionDelta = pCh->deltaTime * VALUE_TRANSITION_SPEED;
        pCh->amp    += (pCh->pParam->amp       - pCh->amp)    * transitionDelta;
        pCh->panL   += (pCh->pParam->panLeft   - pCh->panL)   * transitionDelta;
        pCh->panR   += (pCh->pParam->panRight  - pCh->panR)   * transitionDelta;
        pCh->filter.cut += (pCh->pParam->cutoff    - pCh->filter.cut) * transitionDelta;
        pCh->filter.res += (pCh->pParam->resonance - pCh->filter.res) * transitionDelta;
        *inputBuff = 0.0;
        inputBuff++;
    }
}

/******************************************************************************/
inline void delay(CHANNEL *pCh, double *waveL, double *waveR) {
    pCh->writeIndex++;
    if (DELAY_TAPS <= pCh->writeIndex) {
        pCh->writeIndex = 0;
    }

    pCh->readIndex = pCh->writeIndex - (SInt32)(pCh->pParam->delayTime * pCh->sampleRate);
    if (pCh->readIndex < 0) {
        pCh->readIndex += DELAY_TAPS;
    }

    double delayL = pCh->pParam->delaySend * pCh->pDelTapL[pCh->readIndex];
    double delayR = pCh->pParam->delaySend * pCh->pDelTapR[pCh->readIndex];

    *waveL += (0.9 * delayL + 0.1 * delayR);
    *waveR += (0.9 * delayR + 0.1 * delayL);

    pCh->pDelTapL[pCh->writeIndex] = *waveL;
    pCh->pDelTapR[pCh->writeIndex] = *waveR;
}

inline void chorus(CHANNEL *pCh, double *waveL, double *waveR) {
    double pos;
    double dt;
    SInt32 cur;
    SInt32 pre;
    double chorusL = 0.0;
    double chorusR = 0.0;

    for (SInt32 ph = 0; ph < CHORUS_PHASES; ++ph) {
        pos = pCh->writeIndex - (0.5 - 0.4 * pCh->choLfo[ph]) * pCh->sampleRate * 0.01;
        cur = (SInt32)pos;
        pre = cur - 1;
        dt = pos - cur;
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
        chorusL += (pCh->pDelTapL[cur] * dt + pCh->pDelTapL[pre] * (1.0 - dt)) * pCh->choPanL[ph];
        chorusR += (pCh->pDelTapR[cur] * dt + pCh->pDelTapR[pre] * (1.0 - dt)) * pCh->choPanR[ph];
    }

    double lfoDelta = PI2 * INV_SQRT3 * pCh->pParam->chorusRate * pCh->deltaTime;
    pCh->choLfo[0] += (pCh->choLfo[1] - pCh->choLfo[2]) * lfoDelta;
    pCh->choLfo[1] += (pCh->choLfo[2] - pCh->choLfo[0]) * lfoDelta;
    pCh->choLfo[2] += (pCh->choLfo[0] - pCh->choLfo[1]) * lfoDelta;

    *waveL += chorusL * pCh->pParam->chorusSend / CHORUS_PHASES;
    *waveR += chorusR * pCh->pParam->chorusSend / CHORUS_PHASES;
}
