#include <stdlib.h>
#include <string.h>
#include "sampler.h"
#include "filter.h"

/******************************************************************************/
#define PURGE_THRESHOLD 0.0005
#define PURGE_SPEED     500

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
    SYSTEM_VALUE *pSystemValue = pChValue->pSystemValue;
    CHANNEL_PARAM *pChParam = pChValue->pParam;
    WAVE_LOOP *pLoop = &pSmpl->loop;
    ENVELOPE *pEnvAmp = &pSmpl->envAmp;
    ENVELOPE *pEnvPitch = &pSmpl->envPitch;
    ENVELOPE *pEnvEq = &pSmpl->envEq;
    FILTER *pFilter = &pSmpl->filter;

    SInt16 *pWave = (SInt16*)(pWaveBuffer + pSmpl->waveOfs);
    double *pOutBuff = pChValue->pWave;
    double *pOutBuffTerm = pOutBuff + pSystemValue->bufferLength;

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
        pSmpl->index += pSmpl->delta * pSmpl->egPitch * pChParam->pitch;
        if ((pLoop->begin + pLoop->length) < pSmpl->index) {
            if (pLoop->enable) {
                pSmpl->index -= pLoop->length;
            } else {
                pSmpl->index = pLoop->begin + pLoop->length;
                pSmpl->state = E_KEY_STATE_STANDBY;
                return;
            }
        }
        /***************************/
        /**** generate envelope ****/
        /***************************/
        switch (pSmpl->state) {
        case E_KEY_STATE_PURGE:
            pSmpl->egAmp -= pSmpl->egAmp * pSystemValue->deltaTime * PURGE_SPEED;
            pSmpl->egPitch += (1.0 - pSmpl->egPitch) * pEnvPitch->deltaD;
            pFilter->cut += (pEnvEq->levelF - pFilter->cut) * pEnvEq->deltaR;
            break;
        case E_KEY_STATE_RELEASE:
            pSmpl->egAmp -= pSmpl->egAmp * pEnvAmp->deltaR;
            pSmpl->egPitch += (1.0 - pSmpl->egPitch) * pEnvPitch->deltaD;
            pFilter->cut += (pEnvEq->levelF - pFilter->cut) * pEnvEq->deltaR;
            break;
        case E_KEY_STATE_HOLD:
            pSmpl->egAmp -= pSmpl->egAmp * pChParam->holdDelta;
            pSmpl->egPitch += (1.0 - pSmpl->egPitch) * pEnvPitch->deltaD;
            pFilter->cut += (pEnvEq->levelF - pFilter->cut) * pEnvEq->deltaR;
            break;
        case E_KEY_STATE_PRESS:
            if (pSmpl->time <= pEnvAmp->hold) {
                pSmpl->egAmp += (1.0 - pSmpl->egAmp) * pEnvAmp->deltaA;
            } else {
                pSmpl->egAmp += (pEnvAmp->levelS - pSmpl->egAmp) * pEnvAmp->deltaD;
            }
            if (pSmpl->time <= pEnvPitch->hold) {
                pSmpl->egPitch += (pEnvPitch->levelT - pSmpl->egPitch) * pEnvPitch->deltaA;
            } else {
                pSmpl->egPitch += (1.0 - pSmpl->egPitch) * pEnvPitch->deltaD;
            }
            if (pSmpl->time <= pEnvEq->hold) {
                pFilter->cut += (pEnvEq->levelT - pFilter->cut) * pEnvEq->deltaA;
            } else {
                pFilter->cut += (pEnvEq->levelS - pFilter->cut) * pEnvEq->deltaD;
            }
            break;
        }
        if (pEnvAmp->hold < pSmpl->time && pSmpl->egAmp < PURGE_THRESHOLD) {
            pSmpl->state = E_KEY_STATE_STANDBY;
            return;
        }
        // output
        *pOutBuff += pFilter->a10 * pSmpl->velocity * pSmpl->egAmp;
        //
        pSmpl->time += pSystemValue->deltaTime;
    }
}
