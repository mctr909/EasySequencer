#include <stdlib.h>
#include <string.h>
#include <math.h>
#include "sampler.h"

/******************************************************************************/
#define PURGE_THRESHOLD 0.0005
#define PURGE_SPEED     500

#define CHORUS_PHASES          3
#define DELAY_TAPS             1048576
#define VALUE_TRANSITION_SPEED 250

#define PI        3.14159265
#define PI2       6.28318531
#define INV_SQRT3 0.577350269

/******************************************************************************/
inline void effect(CHANNEL_VALUE *pCh, double *waveL, double *waveR);

/******************************************************************************/
SAMPLER** createSamplers(uint count) {
    SAMPLER** samplers = (SAMPLER**)malloc(sizeof(SAMPLER*) * count);
    for (uint i = 0; i < count; ++i) {
        samplers[i] = (SAMPLER*)malloc(sizeof(SAMPLER));
        memset(samplers[i], 0, sizeof(SAMPLER));
    }
    return samplers;
}

CHANNEL_VALUE** createChannels(SYSTEM_VALUE *pSys) {
    CHANNEL_VALUE **channel = (CHANNEL_VALUE**)malloc(sizeof(CHANNEL_VALUE*) * pSys->channelCount);
    for (int i = 0; i < pSys->channelCount; ++i) {
        CHANNEL_VALUE *pCh = (CHANNEL_VALUE*)malloc(sizeof(CHANNEL_VALUE));
        memset(pCh, 0, sizeof(CHANNEL_VALUE));
        channel[i] = pCh;

        pCh->pSystemValue = pSys;

        // allocate wave buffer
        pCh->pWave = (double*)malloc(sizeof(double) * pCh->pSystemValue->bufferLength);
        memset(pCh->pWave, 0, sizeof(double) * pCh->pSystemValue->bufferLength);
        // allocate channels
        pCh->pParam = (CHANNEL*)malloc(sizeof(CHANNEL));
        memset(pCh->pParam, 0, sizeof(CHANNEL));
        // allocate delay taps
        pCh->writeIndex = 0;
        pCh->pDelTapL = (double*)malloc(sizeof(double) * DELAY_TAPS);
        pCh->pDelTapR = (double*)malloc(sizeof(double) * DELAY_TAPS);
        memset(pCh->pDelTapL, 0, sizeof(double) * DELAY_TAPS);
        memset(pCh->pDelTapR, 0, sizeof(double) * DELAY_TAPS);
        // initialize chorus
        pCh->choLfo[0] = 1.0;
        pCh->choLfo[1] = -0.5;
        pCh->choLfo[2] = -0.5;
        for (int p = 0; p < CHORUS_PHASES; ++p) {
            pCh->choPanL[p] = cos(0.5 * PI * p / CHORUS_PHASES);
            pCh->choPanR[p] = sin(0.5 * PI * p / CHORUS_PHASES);
        }
        // initialize filter
        memset(&pCh->filter, 0, sizeof(FILTER));
    }

    return channel;
}

void disposeSamplers(SAMPLER** ppSmpl, uint count) {
    if (NULL == ppSmpl) {
        return;
    }
    for (uint i = 0; i < count; ++i) {
        free(ppSmpl[i]);
    }
    free(ppSmpl);
}

void disposeChannels(CHANNEL_VALUE **ppCh) {
    if (NULL == ppCh) {
        return;
    }
    uint channels = ppCh[0]->pSystemValue->channelCount;
    for (uint i = 0; i < channels; ++i) {
        free(ppCh[i]->pWave);
        free(ppCh[i]->pParam);
        free(ppCh[i]->pDelTapL);
        free(ppCh[i]->pDelTapR);
        free(ppCh[i]);
    }
    free(ppCh);
}

inline void sampler(CHANNEL_VALUE **ppCh, SAMPLER *pSmpl, byte *pWaveBuffer) {
    CHANNEL_VALUE *pChValue = ppCh[pSmpl->channelNum];
    SYSTEM_VALUE *pSystemValue = pChValue->pSystemValue;
    CHANNEL *pChParam = pChValue->pParam;
    WAVE_INFO *pWaveInfo = &pSmpl->waveInfo;
    ENVELOPE *pEnvAmp = &pSmpl->envAmp;

    short *pWave = (short*)(pWaveBuffer + pWaveInfo->waveOfs);
    double *pOutput = pChValue->pWave;
    double *pOutputTerm = pOutput + pSystemValue->bufferLength;

    pChValue->state = E_CH_STATE_ACTIVE;

    for (; pOutput < pOutputTerm; pOutput++) {
        //*******************************
        // generate wave
        //*******************************
        double sumWave = 0.0;
        for (auto o = 0; o < 16; o++) {
            int pos = (int)pSmpl->index;
            double dt = pSmpl->index - pos;
            sumWave += (pWave[pos - 1] * (1.0 - dt) + pWave[pos] * dt) * pWaveInfo->gain;
            //
            pSmpl->index += pWaveInfo->delta * pChParam->pitch * 0.0625;
            if ((pWaveInfo->loopBegin + pWaveInfo->loopLength) < pSmpl->index) {
                if (pWaveInfo->loopEnable) {
                    pSmpl->index -= pWaveInfo->loopLength;
                } else {
                    pSmpl->index = pWaveInfo->loopBegin + pWaveInfo->loopLength;
                    pSmpl->state = E_KEY_STATE_STANDBY;
                    return;
                }
            }
        }
        // output
        *pOutput += sumWave * pSmpl->velocity * pSmpl->egAmp * 0.0625;
        //*******************************
        // generate envelope
        //*******************************
        switch (pSmpl->state) {
        case E_KEY_STATE_PURGE:
            pSmpl->egAmp -= pSmpl->egAmp * pSystemValue->deltaTime * PURGE_SPEED;
            break;
        case E_KEY_STATE_RELEASE:
            pSmpl->egAmp -= pSmpl->egAmp * pEnvAmp->release;
            break;
        case E_KEY_STATE_HOLD:
            pSmpl->egAmp -= pSmpl->egAmp * pChParam->holdDelta;
            break;
        case E_KEY_STATE_PRESS:
            if (pSmpl->time <= pEnvAmp->hold) {
                pSmpl->egAmp += (1.0 - pSmpl->egAmp) * pEnvAmp->attack;
            } else {
                pSmpl->egAmp += (pEnvAmp->sustain - pSmpl->egAmp) * pEnvAmp->decay;
            }
            break;
        }
        pSmpl->time += pSystemValue->deltaTime;
        //*******************************
        // standby condition
        //*******************************
        if (pEnvAmp->hold < pSmpl->time && pSmpl->egAmp < PURGE_THRESHOLD) {
            pSmpl->state = E_KEY_STATE_STANDBY;
            return;
        }
    }
}

inline void channel32(CHANNEL_VALUE *pCh, float *outBuff) {
    double *inputBuff = pCh->pWave;
    double *inputBuffTerm = inputBuff + pCh->pSystemValue->bufferLength;
    for (; inputBuff < inputBuffTerm; inputBuff++, outBuff += 2) {
        // filter
        filter_lpf(&pCh->filter, *inputBuff * pCh->amp);
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
        *inputBuff = 0.0;
    }
}

inline void channel24(CHANNEL_VALUE *pCh, int24 *outBuff) {
    double *inputBuff = pCh->pWave;
    double *inputBuffTerm = inputBuff + pCh->pSystemValue->bufferLength;
    for (; inputBuff < inputBuffTerm; inputBuff++, outBuff += 2) {
        // filter
        filter_lpf(&pCh->filter, *inputBuff * pCh->amp);
        // pan
        double tempL = pCh->filter.a10 * pCh->panL;
        double tempR = pCh->filter.a10 * pCh->panR;
        // effect
        effect(pCh, &tempL, &tempR);
        // output
        tempL += fromInt24(outBuff + 0);
        tempR += fromInt24(outBuff + 1);
        if (1.0 < tempL) tempL = 1.0;
        if (tempL < -1.0) tempL = -1.0;
        if (1.0 < tempR) tempR = 1.0;
        if (tempR < -1.0) tempR = -1.0;
        setInt24(outBuff + 0, tempL);
        setInt24(outBuff + 1, tempR);
        *inputBuff = 0.0;
    }
}

inline void channel16(CHANNEL_VALUE *pCh, short *outBuff) {
    double *inputBuff = pCh->pWave;
    double *inputBuffTerm = inputBuff + pCh->pSystemValue->bufferLength;
    for (; inputBuff < inputBuffTerm; inputBuff++, outBuff += 2) {
        // filter
        filter_lpf(&pCh->filter, *inputBuff * pCh->amp);
        // pan
        double tempL = pCh->filter.a10 * pCh->panL;
        double tempR = pCh->filter.a10 * pCh->panR;
        // effect
        effect(pCh, &tempL, &tempR);
        // output
        tempL *= 32767.0;
        tempR *= 32767.0;
        tempL += *(outBuff + 0);
        tempR += *(outBuff + 1);
        if (32767.0 < tempL) tempL = 32767.0;
        if (tempL < -32767.0) tempL = -32767.0;
        if (32767.0 < tempR) tempR = 32767.0;
        if (tempR < -32767.0) tempR = -32767.0;
        *(outBuff + 0) = (short)tempL;
        *(outBuff + 1) = (short)tempR;
        *inputBuff = 0.0;
    }
}

/******************************************************************************/
inline void effect(CHANNEL_VALUE *pCh, double *waveL, double *waveR) {
    CHANNEL *pParam = pCh->pParam;
    double *pTapL = pCh->pDelTapL;
    double *pTapR = pCh->pDelTapR;
    pCh->writeIndex++;
    if (DELAY_TAPS <= pCh->writeIndex) {
        pCh->writeIndex = 0;
    }
    int readIndex = pCh->writeIndex - (int)(pParam->delayTime * pCh->pSystemValue->sampleRate);
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
    for (int ph = 0; ph < CHORUS_PHASES; ++ph) {
        double pos = pCh->writeIndex - (0.505 + 0.495 * pCh->choLfo[ph]) * pCh->pSystemValue->sampleRate * pParam->chorusDepth;
        int cur = (int)pos;
        int pre = cur - 1;
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
    double lfoDelta = PI2 * INV_SQRT3 * pParam->chorusRate * pCh->pSystemValue->deltaTime;
    pCh->choLfo[0] += (pCh->choLfo[1] - pCh->choLfo[2]) * lfoDelta;
    pCh->choLfo[1] += (pCh->choLfo[2] - pCh->choLfo[0]) * lfoDelta;
    pCh->choLfo[2] += (pCh->choLfo[0] - pCh->choLfo[1]) * lfoDelta;
    /*** output chorus ***/
    *waveL += chorusL * pParam->chorusSend / CHORUS_PHASES;
    *waveR += chorusR * pParam->chorusSend / CHORUS_PHASES;
    /*** next step ***/
    double transitionDelta = pCh->pSystemValue->deltaTime * VALUE_TRANSITION_SPEED;
    pCh->amp        += (pCh->pParam->amp       - pCh->amp)        * transitionDelta;
    pCh->panL       += (pCh->pParam->panLeft   - pCh->panL)       * transitionDelta;
    pCh->panR       += (pCh->pParam->panRight  - pCh->panR)       * transitionDelta;
    pCh->filter.cut += (pCh->pParam->cutoff    - pCh->filter.cut) * transitionDelta;
    pCh->filter.res += (pCh->pParam->resonance - pCh->filter.res) * transitionDelta;
    pCh->state = E_CH_STATE_STANDBY;
}
