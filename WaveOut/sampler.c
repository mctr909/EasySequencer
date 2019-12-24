#include <math.h>
#include <stdlib.h>
#include "sampler.h"

/******************************************************************************/
#define CHORUS_PHASES          3
#define DELAY_TAPS             1048576
#define ADJUST_CUTOFF          0.98
#define PURGE_THRESHOLD        0.001
#define PURGE_SPEED            250
#define VALUE_TRANSITION_SPEED 250

static const double PI        = 3.14159265;
static const double PI2       = 6.28318531;
static const double INV_FACT2 = 5.00000000e-01;
static const double INV_FACT3 = 1.66666667e-01;
static const double INV_FACT4 = 4.16666667e-02;
static const double INV_FACT5 = 8.33333333e-03;
static const double INV_FACT6 = 1.38888889e-03;
static const double INV_FACT7 = 1.98412698e-04;
static const double INV_FACT8 = 2.48015873e-05;
static const double INV_FACT9 = 2.75573192e-06;

/******************************************************************************/
CHANNEL** createChannels(UInt32 count, UInt32 sampleRate) {
    CHANNEL **channel = (CHANNEL**)malloc(sizeof(CHANNEL*) * count);
    for (UInt32 i = 0; i < count; ++i) {
        CHANNEL *pCh = (CHANNEL*)malloc(sizeof(CHANNEL));
        memset(pCh, 0, sizeof(CHANNEL));
        channel[i] = pCh;

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
        pCh->pChoPanL  = (double*)malloc(sizeof(double) * CHORUS_PHASES);
        pCh->pChoPanR  = (double*)malloc(sizeof(double) * CHORUS_PHASES);
        pCh->pChoLfoRe = (double*)malloc(sizeof(double) * CHORUS_PHASES);
        pCh->pChoLfoIm = (double*)malloc(sizeof(double) * CHORUS_PHASES);
        for (SInt32 p = 0; p < CHORUS_PHASES; ++p) {
            pCh->pChoPanL[p] = cos(0.5 * PI * p / CHORUS_PHASES);
            pCh->pChoPanR[p] = sin(0.5 * PI * p / CHORUS_PHASES);
            pCh->pChoLfoRe[p] = cos(PI2 * p / CHORUS_PHASES);
            pCh->pChoLfoIm[p] = sin(PI2 * p / CHORUS_PHASES);
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
inline void channel(CHANNEL *pCh, double *waveL, double *waveR) {
    //
    filter(&pCh->eq, pCh->amp * pCh->wave);
    pCh->wave = pCh->eq.a2;

    //
    double tempL = pCh->wave * pCh->panL;
    double tempR = pCh->wave * pCh->panR;

    //
    delay(pCh, &tempL, &tempR);
    chorus(pCh, &tempL, &tempR);

    //
    pCh->amp    += (pCh->pParam->amp       - pCh->amp)    * pCh->deltaTime * VALUE_TRANSITION_SPEED;
    pCh->panL   += (pCh->pParam->panLeft   - pCh->panL)   * pCh->deltaTime * VALUE_TRANSITION_SPEED;
    pCh->panR   += (pCh->pParam->panRight  - pCh->panR)   * pCh->deltaTime * VALUE_TRANSITION_SPEED;
    pCh->eq.cut += (pCh->pParam->cutoff    - pCh->eq.cut) * pCh->deltaTime * VALUE_TRANSITION_SPEED;
    pCh->eq.res += (pCh->pParam->resonance - pCh->eq.res) * pCh->deltaTime * VALUE_TRANSITION_SPEED;

    //
    *waveL += tempL;
    *waveR += tempR;
    pCh->wave = 0.0;
}

inline void sampler(CHANNEL **ppCh, SAMPLER *pSmpl, byte *pDlsBuffer) {
    if (NULL == ppCh || NULL == pSmpl || E_KEY_STATE_WAIT == pSmpl->keyState) {
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

    /***********************/
    /**** generate wave ****/
    /***********************/
    double wave;
    SInt16 *pWave = (SInt16*)(pDlsBuffer + pSmpl->buffOfs);
    SInt32 cur = (SInt32)pSmpl->index;
    SInt32 pre = cur - 1;
    double dt = pSmpl->index - cur;
    if (pre < 0) {
        pre = 0;
    }
    wave = (pWave[pre] * (1.0 - dt) + pWave[cur] * dt) * pSmpl->gain;
    pSmpl->index += pSmpl->delta * pChParam->pitch;
    if ((pSmpl->loop.begin + pSmpl->loop.length) < pSmpl->index) {
        if (pSmpl->loop.enable) {
            pSmpl->index -= pSmpl->loop.length;
        } else {
            pSmpl->index = pSmpl->loop.begin + pSmpl->loop.length;
            pSmpl->keyState = E_KEY_STATE_WAIT;
        }
    }

    /***************************/
    /**** generate envelope ****/
    /***************************/
    switch (pSmpl->keyState) {
    case E_KEY_STATE_PURGE:
        pSmpl->amp -= pSmpl->amp * pChValue->deltaTime * PURGE_SPEED;
        if (pSmpl->amp < PURGE_THRESHOLD) {
            pSmpl->keyState = E_KEY_STATE_WAIT;
        }
        break;
    case E_KEY_STATE_RELEASE:
        pSmpl->amp -= pSmpl->amp * pSmpl->envAmp.deltaR;
        if (pSmpl->amp < PURGE_THRESHOLD) {
            pSmpl->keyState = E_KEY_STATE_WAIT;
        }
        break;
    case E_KEY_STATE_HOLD:
        pSmpl->amp -= pSmpl->amp * pChParam->holdDelta;
        if (pSmpl->amp < PURGE_THRESHOLD) {
            pSmpl->keyState = E_KEY_STATE_WAIT;
        }
        break;
    case E_KEY_STATE_PRESS:
        if (pSmpl->time < pSmpl->envAmp.hold) {
            pSmpl->amp += (1.0 - pSmpl->amp) * pSmpl->envAmp.deltaA;
        } else {
            pSmpl->amp += (pSmpl->envAmp.levelS - pSmpl->amp) * pSmpl->envAmp.deltaD;
        }
        break;
    }
    //
    pChValue->wave += wave * pSmpl->velocity * pSmpl->amp;
    //
    pSmpl->time += pChValue->deltaTime;
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

    double delayL = pCh->pParam->delayDepth * pCh->pDelTapL[pCh->readIndex];
    double delayR = pCh->pParam->delayDepth * pCh->pDelTapR[pCh->readIndex];

    *waveL += (0.9 * delayL + 0.1 * delayR);
    *waveR += (0.9 * delayR + 0.1 * delayL);

    pCh->pDelTapL[pCh->writeIndex] = *waveL;
    pCh->pDelTapR[pCh->writeIndex] = *waveR;
}

inline void chorus(CHANNEL *pCh, double *waveL, double *waveR) {
    double chorusL = 0.0;
    double chorusR = 0.0;
    double index;
    double dt;
    SInt32 cur;
    SInt32 pre;

    for (SInt32 ph = 0; ph < CHORUS_PHASES; ++ph) {
        index = pCh->writeIndex - (0.5 - 0.4 * pCh->pChoLfoRe[ph]) * pCh->sampleRate * 0.01;
        cur = (SInt32)index;
        pre = cur - 1;
        dt = index - cur;

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

        chorusL += (pCh->pDelTapL[cur] * dt + pCh->pDelTapL[pre] * (1.0 - dt)) * pCh->pChoPanL[ph];
        chorusR += (pCh->pDelTapR[cur] * dt + pCh->pDelTapR[pre] * (1.0 - dt)) * pCh->pChoPanR[ph];

        pCh->pChoLfoRe[ph] -= pCh->pChoLfoIm[ph] * PI2 * pCh->pParam->chorusRate * pCh->deltaTime;
        pCh->pChoLfoIm[ph] += pCh->pChoLfoRe[ph] * PI2 * pCh->pParam->chorusRate * pCh->deltaTime;
    }

    *waveL += chorusL * pCh->pParam->chorusDepth / CHORUS_PHASES;
    *waveR += chorusR * pCh->pParam->chorusDepth / CHORUS_PHASES;
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
        + kb0 * pFilter->b0
        + kb1 * pFilter->b1
        - ka0 * pFilter->a0
        - ka1 * pFilter->a1
    ;
    pFilter->b1 = pFilter->b0;
    pFilter->b0 = input;
    pFilter->a1 = pFilter->a0;
    pFilter->a0 = output;

    /** フィルタ2段目 **/
    input = output;
    output =
        kb1 * input
        + kb0 * pFilter->b2
        + kb1 * pFilter->b3
        - ka0 * pFilter->a2
        - ka1 * pFilter->a3
    ;
    pFilter->b3 = pFilter->b2;
    pFilter->b2 = input;
    pFilter->a3 = pFilter->a2;
    pFilter->a2 = output;

    /** フィルタ3段目 **/
    input = output;
    output =
        kb1 * input
        + kb0 * pFilter->b4
        + kb1 * pFilter->b5
        - ka0 * pFilter->a4
        - ka1 * pFilter->a5
        ;
    pFilter->b5 = pFilter->b4;
    pFilter->b4 = input;
    pFilter->a5 = pFilter->a4;
    pFilter->a4 = output;
}
