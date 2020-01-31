#include <stdlib.h>
#include <string.h>
#include "sampler.h"
#include "filter.h"

/******************************************************************************/
#define PURGE_THRESHOLD 0.001
#define PURGE_SPEED     250

/******************************************************************************/
SAMPLER** createSamplers(UInt32 count) {
    SAMPLER** samplers = (SAMPLER**)malloc(sizeof(SAMPLER*) * count);
    for (UInt32 i = 0; i < count; ++i) {
        samplers[i] = (SAMPLER*)malloc(sizeof(SAMPLER));
        memset(samplers[i], 0, sizeof(SAMPLER));
    }

    return samplers;
}

inline void sampler(CHANNEL **ppCh, SAMPLER *pSmpl, byte *pWaveBuffer) {
    CHANNEL *pChValue = ppCh[pSmpl->channelNo];
    CHANNEL_PARAM *pChParam = pChValue->pParam;
    WAVE_LOOP *pLoop = &pSmpl->loop;
    ENVELOPE *pEnvAmp = &pSmpl->envAmp;
    ENVELOPE *pEnvEq = &pSmpl->envEq;
    FILTER *pFilter = &pSmpl->filter;

    SInt16 *pWave = (SInt16*)(pWaveBuffer + pSmpl->waveOfs);
    double *pOutBuff = pChValue->pWave;
    double *pOutBuffTerm = pOutBuff + pChValue->buffLen;

    for (; pOutBuff < pOutBuffTerm; pOutBuff++) {
        /***********************/
        /**** generate wave ****/
        /***********************/
        SInt32 pos = (SInt32)pSmpl->index;
        double dt = pSmpl->index - pos;
        double wave = (pWave[pos - 1] * (1.0 - dt) + pWave[pos] * dt) * pSmpl->gain;
        //
        filter(pFilter, wave);
        //
        pSmpl->index += pSmpl->delta * pChParam->pitch;
        if ((pLoop->begin + pLoop->length) < pSmpl->index) {
            if (pLoop->enable) {
                pSmpl->index -= pLoop->length;
            } else {
                pSmpl->index = pLoop->begin + pLoop->length;
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
            pFilter->cut += (pEnvEq->levelF - pFilter->cut) * pEnvEq->deltaR;
            break;
        case E_KEY_STATE_RELEASE:
            pSmpl->amp -= pSmpl->amp * pEnvAmp->deltaR;
            pFilter->cut += (pEnvEq->levelF - pFilter->cut) * pEnvEq->deltaR;
            break;
        case E_KEY_STATE_HOLD:
            pSmpl->amp -= pSmpl->amp * pChParam->holdDelta;
            pFilter->cut += (pEnvEq->levelF - pFilter->cut) * pEnvEq->deltaR;
            break;
        case E_KEY_STATE_PRESS:
            if (pSmpl->time <= pEnvAmp->hold) {
                pSmpl->amp += (1.0 - pSmpl->amp) * pEnvAmp->deltaA;
            } else {
                pSmpl->amp += (pEnvAmp->levelS - pSmpl->amp) * pEnvAmp->deltaD;
            }
            if (pSmpl->time <= pEnvEq->hold) {
                pFilter->cut += (pEnvEq->levelT - pFilter->cut) * pEnvEq->deltaA;
            } else {
                pFilter->cut += (pEnvEq->levelS - pFilter->cut) * pEnvEq->deltaD;
            }
            break;
        }
        if (pEnvAmp->hold < pSmpl->time && pSmpl->amp < PURGE_THRESHOLD) {
            pSmpl->state = E_KEY_STATE_WAIT;
            return;
        }
        // output
        *pOutBuff += pFilter->a10 * pSmpl->velocity * pSmpl->amp;
        //
        pSmpl->time += pChValue->deltaTime;
    }
}
