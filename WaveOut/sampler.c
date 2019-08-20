#include <math.h>
#include <stdlib.h>
#include "sampler.h"

/******************************************************************************/
#define CHORUS_PHASES          3
#define DELAY_TAPS             1048576
#define ADJUST_CUTOFF          0.98
#define PURGE_THRESHOLD        0.001
#define PURGE_SPEED            2500
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
        channel[i] = (CHANNEL*)malloc(sizeof(CHANNEL));
        memset(channel[i], 0, sizeof(CHANNEL));
        channel[i]->param = (CHANNEL_PARAM*)malloc(sizeof(CHANNEL_PARAM));
        memset(channel[i]->param, 0, sizeof(CHANNEL_PARAM));
        channel[i]->sampleRate = sampleRate;
        channel[i]->deltaTime = 1.0 / sampleRate;
    }

    // Filter
    for (UInt32 i = 0; i < count; ++i) {
        memset(&channel[i]->eq, 0, sizeof(FILTER));
    }

    // Delay
    for (UInt32 i = 0; i < count; ++i) {
        memset(&channel[i]->delay, 0, sizeof(DELAY));

        DELAY *delay = &channel[i]->delay;
        delay->readIndex = 0;
        delay->writeIndex = 0;

        delay->pTapL = (double*)malloc(sizeof(double) * DELAY_TAPS);
        delay->pTapR = (double*)malloc(sizeof(double) * DELAY_TAPS);
        memset(delay->pTapL, 0, sizeof(double) * DELAY_TAPS);
        memset(delay->pTapR, 0, sizeof(double) * DELAY_TAPS);
    }

    // Chorus
    for (UInt32 i = 0; i < count; ++i) {
        memset(&channel[i]->chorus, 0, sizeof(CHORUS));

        CHORUS *chorus = &channel[i]->chorus;
        chorus->lfoK = PI2 * channel[i]->deltaTime;
        chorus->pPanL = (double*)malloc(sizeof(double) * CHORUS_PHASES);
        chorus->pPanR = (double*)malloc(sizeof(double) * CHORUS_PHASES);
        chorus->pLfoRe = (double*)malloc(sizeof(double) * CHORUS_PHASES);
        chorus->pLfoIm = (double*)malloc(sizeof(double) * CHORUS_PHASES);

        for (SInt32 p = 0; p < CHORUS_PHASES; ++p) {
            chorus->pPanL[p] = cos(PI * p / CHORUS_PHASES);
            chorus->pPanR[p] = sin(PI * p / CHORUS_PHASES);
            chorus->pLfoRe[p] = cos(PI2 * p / CHORUS_PHASES);
            chorus->pLfoIm[p] = sin(PI2 * p / CHORUS_PHASES);
        }
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
inline void channel(CHANNEL *ch, double *waveL, double *waveR) {
    //
    filter(&ch->eq, ch->amp * ch->wave);
    ch->wave = ch->eq.a2;

    //
    ch->waveL = ch->wave * ch->panLeft;
    ch->waveR = ch->wave * ch->panRight;

    //
    delay(ch, &ch->delay, &ch->waveL, &ch->waveR);
    chorus(ch, &ch->delay, &ch->chorus, &ch->waveL, &ch->waveR);

    //
    ch->amp      += (ch->param->amp       - ch->amp)      * ch->deltaTime * VALUE_TRANSITION_SPEED;
    ch->panLeft  += (ch->param->panLeft   - ch->panLeft)  * ch->deltaTime * VALUE_TRANSITION_SPEED;
    ch->panRight += (ch->param->panRight  - ch->panRight) * ch->deltaTime * VALUE_TRANSITION_SPEED;
    ch->eq.cut   += (ch->param->cutoff    - ch->eq.cut)   * ch->deltaTime * VALUE_TRANSITION_SPEED;
    ch->eq.res   += (ch->param->resonance - ch->eq.res)   * ch->deltaTime * VALUE_TRANSITION_SPEED;

    //
    *waveL += ch->waveL;
    *waveR += ch->waveR;
    ch->wave = 0.0;
}

inline void sampler(CHANNEL **chs, SAMPLER *smpl, byte *pDlsBuffer) {
    if (NULL == chs || NULL == smpl || E_KEY_STATE_WAIT == smpl->keyState) {
        return;
    }

    CHANNEL *chValue = chs[smpl->channelNo];
    if (NULL == chValue) {
        return;
    }
    CHANNEL_PARAM *chParam = chValue->param;
    if (NULL == chParam) {
        return;
    }

    /***********************/
    /**** generate wave ****/
    /***********************/
    double wave;
    SInt16 *pcm = (SInt16*)(pDlsBuffer + smpl->pcmAddr);
    SInt32 cur = (SInt32)smpl->index;
    SInt32 pre = cur - 1;
    double dt = smpl->index - cur;
    if (pre < 0) {
        pre = 0;
    }
    wave = (pcm[pre] * (1.0 - dt) + pcm[cur] * dt) * smpl->gain;
    smpl->index += smpl->delta * chParam->pitch * chValue->deltaTime;
    if ((smpl->loop.begin + smpl->loop.length) < smpl->index) {
        if (smpl->loop.enable) {
            smpl->index -= smpl->loop.length;
        } else {
            smpl->index = smpl->loop.begin + smpl->loop.length;
            smpl->keyState = E_KEY_STATE_WAIT;
        }
    }

    /***************************/
    /**** generate envelope ****/
    /***************************/
    switch (smpl->keyState) {
    case E_KEY_STATE_PURGE:
        smpl->amp -= smpl->amp * chValue->deltaTime * PURGE_SPEED;
        if (smpl->amp < PURGE_THRESHOLD) {
            smpl->keyState = E_KEY_STATE_WAIT;
        }
        break;

    case E_KEY_STATE_RELEASE:
        smpl->eq.cut += (smpl->envEq.fall - smpl->eq.cut) * smpl->envEq.deltaR;
        smpl->amp -= smpl->amp * smpl->envAmp.deltaR;
        if (smpl->amp < PURGE_THRESHOLD) {
            smpl->keyState = E_KEY_STATE_WAIT;
        }
        break;

    case E_KEY_STATE_HOLD:
        smpl->eq.cut += (smpl->envEq.fall - smpl->eq.cut) * smpl->envEq.deltaR;
        smpl->amp -= smpl->amp * chParam->holdDelta;
        if (smpl->amp < PURGE_THRESHOLD) {
            smpl->keyState = E_KEY_STATE_WAIT;
        }
        break;

    case E_KEY_STATE_PRESS:
        if (smpl->time < smpl->envEq.hold) {
            smpl->eq.cut += (smpl->envEq.top     - smpl->eq.cut) * smpl->envEq.deltaA;
        } else {
            smpl->eq.cut += (smpl->envEq.sustain - smpl->eq.cut) * smpl->envEq.deltaD;
        }
        if (smpl->time < smpl->envAmp.hold) {
            smpl->amp += (1.0                  - smpl->amp) * smpl->envAmp.deltaA;
        } else {
            smpl->amp += (smpl->envAmp.sustain - smpl->amp) * smpl->envAmp.deltaD;
        }
        break;
    }

    //
    filter(&smpl->eq, wave * smpl->velocity * smpl->amp);
    chValue->wave += smpl->eq.a2;

    //
    smpl->time += chValue->deltaTime;
}

/******************************************************************************/
inline void delay(CHANNEL *ch, DELAY *delay, double *waveL, double *waveR) {
    ++delay->writeIndex;
    if (DELAY_TAPS <= delay->writeIndex) {
        delay->writeIndex = 0;
    }

    delay->readIndex = delay->writeIndex - (SInt32)(ch->param->delayTime * ch->sampleRate);
    if (delay->readIndex < 0) {
        delay->readIndex += DELAY_TAPS;
    }

    double delayL = ch->param->delayDepth * delay->pTapL[delay->readIndex];
    double delayR = ch->param->delayDepth * delay->pTapR[delay->readIndex];

    *waveL += (0.9 * delayL + 0.1 * delayR);
    *waveR += (0.9 * delayR + 0.1 * delayL);

    delay->pTapL[delay->writeIndex] = *waveL;
    delay->pTapR[delay->writeIndex] = *waveR;
}

inline void chorus(CHANNEL *ch, DELAY *delay, CHORUS *chorus, double *waveL, double *waveR) {
    double chorusL = 0.0;
    double chorusR = 0.0;
    double index;
    double dt;
    SInt32 indexCur;
    SInt32 indexPre;

    for (register ph = 0; ph < CHORUS_PHASES; ++ph) {
        index = delay->writeIndex - (0.5 - 0.4 * chorus->pLfoRe[ph]) * ch->sampleRate * 0.01;
        indexCur = (SInt32)index;
        indexPre = indexCur - 1;
        dt = index - indexCur;

        if (indexCur < 0) {
            indexCur += DELAY_TAPS;
        }
        if (DELAY_TAPS <= indexCur) {
            indexCur -= DELAY_TAPS;
        }

        if (indexPre < 0) {
            indexPre += DELAY_TAPS;
        }
        if (DELAY_TAPS <= indexPre) {
            indexPre -= DELAY_TAPS;
        }

        chorusL += (delay->pTapL[indexCur] * dt + delay->pTapL[indexPre] * (1.0 - dt)) * chorus->pPanL[ph];
        chorusR += (delay->pTapR[indexCur] * dt + delay->pTapR[indexPre] * (1.0 - dt)) * chorus->pPanR[ph];

        chorus->pLfoRe[ph] -= chorus->lfoK * ch->param->chorusRate * chorus->pLfoIm[ph];
        chorus->pLfoIm[ph] += chorus->lfoK * ch->param->chorusRate * chorus->pLfoRe[ph];
    }

    *waveL += chorusL * ch->param->chorusDepth / CHORUS_PHASES;
    *waveR += chorusR * ch->param->chorusDepth / CHORUS_PHASES;
}

inline void filter(FILTER *param, double input) {
    /** sin, cosの近似 **/
    double rad = param->cut * PI * ADJUST_CUTOFF;
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
    double a = s / (param->res * 4.0 + 1.0);
    double m = 1.0 / (a + 1.0);
    double ka0 = -2.0 * c  * m;
    double kb0 = (1.0 - c) * m;
    double ka1 = (1.0 - a) * m;
    double kb1 = kb0 * 0.5;

    /** フィルタ1段目 **/
    double output =
        kb1 * input
        + kb0 * param->b0
        + kb1 * param->b1
        - ka0 * param->a0
        - ka1 * param->a1
    ;
    param->b1 = param->b0;
    param->b0 = input;
    param->a1 = param->a0;
    param->a0 = output;

    /** フィルタ2段目 **/
    input = output;
    output =
        kb1 * input
        + kb0 * param->b2
        + kb1 * param->b3
        - ka0 * param->a2
        - ka1 * param->a3
    ;
    param->b3 = param->b2;
    param->b2 = input;
    param->a3 = param->a2;
    param->a2 = output;
}
