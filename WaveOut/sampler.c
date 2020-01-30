#include <math.h>
#include <stdlib.h>
#include <string.h>
#include "sampler.h"

/******************************************************************************/
#define CHORUS_PHASES          3
#define DELAY_TAPS             1048576
#define ADJUST_CUTOFF          0.98
#define PURGE_THRESHOLD        0.001
#define PURGE_SPEED            250
#define VALUE_TRANSITION_SPEED 250

#define PI        3.14159265
#define PI2       6.28318531
#define INV_SQRT3 0.577350269
#define INV_FACT2 5.00000000e-01
#define INV_FACT3 1.66666667e-01
#define INV_FACT4 4.16666667e-02
#define INV_FACT5 8.33333333e-03
#define INV_FACT6 1.38888889e-03
#define INV_FACT7 1.98412698e-04
#define INV_FACT8 2.48015873e-05
#define INV_FACT9 2.75573192e-06

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
        memset(&pCh->eq, 0, sizeof(FILTER));
    }

    return channel;
}

SAMPLER** createSamplers(UInt32 count) {
    SAMPLER** samplers = (SAMPLER**)malloc(sizeof(SAMPLER*) * count);
    for (UInt32 i = 0; i < count; ++i) {
        samplers[i] = (SAMPLER*)malloc(sizeof(SAMPLER));
        memset(samplers[i], 0, sizeof(SAMPLER));
    }

    return samplers;
}

/******************************************************************************/
inline void channel(CHANNEL *pCh, SInt16 *outBuff) {
    double *inputBuff = pCh->pWave;
    for (UInt32 i=0; i < pCh->buffLen; i++) {
        // filter
        filter(&pCh->eq, *inputBuff * pCh->amp);
        // pan
        double tempL = pCh->eq.a10 * pCh->panL;
        double tempR = pCh->eq.a10 * pCh->panR;
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
        pCh->eq.cut += (pCh->pParam->cutoff    - pCh->eq.cut) * transitionDelta;
        pCh->eq.res += (pCh->pParam->resonance - pCh->eq.res) * transitionDelta;
        *inputBuff = 0.0;
        inputBuff++;
    }
}

inline void sampler(CHANNEL **ppCh, SAMPLER *pSmpl, byte *pWaveBuffer) {
    if (NULL == ppCh || NULL == pSmpl || E_KEY_STATE_WAIT == pSmpl->state) {
        return;
    }
    CHANNEL *pChValue = ppCh[pSmpl->channelNo];
    if (NULL == pChValue) {
        return;
    }
    CHANNEL_PARAM *pChParam = pChValue->pParam;
    if (NULL == pChParam) {
        return;
    }

    double *pOutBuff = pChValue->pWave;
    SInt16 *pWave = (SInt16*)(pWaveBuffer + pSmpl->buffOfs);

    for (UInt32 idx = 0; idx < pChValue->buffLen; idx++) {
        /***********************/
        /**** generate wave ****/
        /***********************/
        SInt32 cur = (SInt32)pSmpl->index;
        SInt32 pre = cur - 1;
        double dt = pSmpl->index - cur;
        if (pre < 0) {
            pre = 0;
        }
        double wave = (pWave[pre] * (1.0 - dt) + pWave[cur] * dt) * pSmpl->gain;
        pSmpl->index += pSmpl->delta * pChParam->pitch;
        if ((pSmpl->loop.begin + pSmpl->loop.length) < pSmpl->index) {
            if (pSmpl->loop.enable) {
                pSmpl->index -= pSmpl->loop.length;
            } else {
                pSmpl->index = pSmpl->loop.begin + pSmpl->loop.length;
                pSmpl->state = E_KEY_STATE_WAIT;
                return;
            }
        }

        /***************************/
        /**** generate envelope ****/
        /***************************/
        switch (pSmpl->state) {
        case E_KEY_STATE_PURGE:
            pSmpl->amp -= pSmpl->amp * pChValue->deltaTime * PURGE_SPEED;
            break;
        case E_KEY_STATE_RELEASE:
            pSmpl->amp -= pSmpl->amp * pSmpl->envAmp.deltaR;
            break;
        case E_KEY_STATE_HOLD:
            pSmpl->amp -= pSmpl->amp * pChParam->holdDelta;
            break;
        case E_KEY_STATE_PRESS:
            if (pSmpl->time <= pSmpl->envAmp.hold) {
                pSmpl->amp += (1.0 - pSmpl->amp) * pSmpl->envAmp.deltaA;
            } else {
                pSmpl->amp += (pSmpl->envAmp.levelS - pSmpl->amp) * pSmpl->envAmp.deltaD;
            }
            break;
        }
        if (pSmpl->envAmp.hold < pSmpl->time && pSmpl->amp < PURGE_THRESHOLD) {
            pSmpl->state = E_KEY_STATE_WAIT;
            return;
        }
        //
        pOutBuff[idx] += wave * pSmpl->velocity * pSmpl->amp;
        //
        pSmpl->time += pChValue->deltaTime;
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

inline void filter(FILTER *pFilter, double input) {
    /** sin, cosの近似 **/
    double rad = pFilter->cut * PI * ADJUST_CUTOFF;
    double rad2 = rad * rad;
    double c = INV_FACT8;
    double s = INV_FACT9;
    c *= rad2;
    s *= rad2;
    c -= INV_FACT6;
    s -= INV_FACT7;
    c *= rad2;
    s *= rad2;
    c += INV_FACT4;
    s += INV_FACT5;
    c *= rad2;
    s *= rad2;
    c -= INV_FACT2;
    s -= INV_FACT3;
    c *= rad2;
    s *= rad2;
    c++;
    s++;
    s *= rad;

    /** IIRローパスフィルタ パラメータ設定 **/
    double a = s / (pFilter->res * 4.0 + 1.0);
    double m = 1.0 / (a + 1.0);
    double ka0 = -2.0 * c  * m;
    double kb0 = (1.0 - c) * m;
    double ka1 = (1.0 - a) * m;
    double kb1 = kb0 * 0.5;

    /** フィルタ1段目 **/
    double output =
        kb1 * input
        + kb0 * pFilter->b00
        + kb1 * pFilter->b01
        - ka0 * pFilter->a00
        - ka1 * pFilter->a01
    ;
    pFilter->b01 = pFilter->b00;
    pFilter->b00 = input;
    pFilter->a01 = pFilter->a00;
    pFilter->a00 = output;

    /** フィルタ2段目 **/
    input = output;
    output =
        kb1 * input
        + kb0 * pFilter->b10
        + kb1 * pFilter->b11
        - ka0 * pFilter->a10
        - ka1 * pFilter->a11
    ;
    pFilter->b11 = pFilter->b10;
    pFilter->b10 = input;
    pFilter->a11 = pFilter->a10;
    pFilter->a10 = output;
}
