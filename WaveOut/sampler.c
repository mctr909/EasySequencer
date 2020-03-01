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
    double *pOutput = pChValue->pWave;
    double *pOutputTerm = pOutput + pSystemValue->bufferLength;

    for (; pOutput < pOutputTerm; pOutput++) {
        /***********************/
        /**** generate wave ****/
        /***********************/
        double sumWave = 0.0;
        for (auto o = 0; o < 16; o++) {
            SInt32 pos = (SInt32)pSmpl->index;
            double dt = pSmpl->index - pos;
            sumWave += (pWave[pos - 1] * (1.0 - dt) + pWave[pos] * dt) * pSmpl->gain;
            //
            pSmpl->index += pSmpl->delta * pSmpl->egPitch * pChParam->pitch * 0.0625;
            if ((pLoop->begin + pLoop->length) < pSmpl->index) {
                if (pLoop->enable) {
                    pSmpl->index -= pLoop->length;
                } else {
                    pSmpl->index = pLoop->begin + pLoop->length;
                    pSmpl->state = E_KEY_STATE_STANDBY;
                    return;
                }
            }
        }
        // output
        filter(pFilter, sumWave * 0.0625);
        *pOutput += pFilter->a10 * pSmpl->velocity * pSmpl->egAmp;
        /***************************/
        /**** generate envelope ****/
        /***************************/
        switch (pSmpl->state) {
        case E_KEY_STATE_PURGE:
            pSmpl->egAmp -= pSmpl->egAmp * pSystemValue->deltaTime * PURGE_SPEED;
            pSmpl->egPitch += (1.0 - pSmpl->egPitch) * pEnvPitch->decay;
            pFilter->cut += (pEnvEq->fall - pFilter->cut) * pEnvEq->release;
            break;
        case E_KEY_STATE_RELEASE:
            pSmpl->egAmp -= pSmpl->egAmp * pEnvAmp->release;
            pSmpl->egPitch += (1.0 - pSmpl->egPitch) * pEnvPitch->decay;
            pFilter->cut += (pEnvEq->fall - pFilter->cut) * pEnvEq->release;
            break;
        case E_KEY_STATE_HOLD:
            pSmpl->egAmp -= pSmpl->egAmp * pChParam->holdDelta;
            pSmpl->egPitch += (1.0 - pSmpl->egPitch) * pEnvPitch->decay;
            pFilter->cut += (pEnvEq->fall - pFilter->cut) * pEnvEq->release;
            break;
        case E_KEY_STATE_PRESS:
            if (pSmpl->time <= pEnvAmp->hold) {
                pSmpl->egAmp += (1.0 - pSmpl->egAmp) * pEnvAmp->attack;
            } else {
                pSmpl->egAmp += (pEnvAmp->sustain - pSmpl->egAmp) * pEnvAmp->decay;
            }
            if (pSmpl->time <= pEnvPitch->hold) {
                pSmpl->egPitch += (pEnvPitch->top - pSmpl->egPitch) * pEnvPitch->attack;
            } else {
                pSmpl->egPitch += (1.0 - pSmpl->egPitch) * pEnvPitch->decay;
            }
            if (pSmpl->time <= pEnvEq->hold) {
                pFilter->cut += (pEnvEq->top - pFilter->cut) * pEnvEq->attack;
            } else {
                pFilter->cut += (pEnvEq->sustain - pFilter->cut) * pEnvEq->decay;
            }
            break;
        }
        // standby condition
        if (pEnvAmp->hold < pSmpl->time && pSmpl->egAmp < PURGE_THRESHOLD) {
            pSmpl->state = E_KEY_STATE_STANDBY;
            return;
        }
        //
        pSmpl->time += pSystemValue->deltaTime;
    }
}
