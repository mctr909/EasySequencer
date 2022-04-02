#include "sampler.h"
#include "effect.h"
#include "channel.h"

/******************************************************************************/
#define PURGE_THRESHOLD 0.0005
#define PURGE_SPEED     500
#define OVER_SAMPLING   4

/******************************************************************************/
void sampler_create(SYSTEM_VALUE* pSystemValue) {
    sampler_dispose(pSystemValue);
    pSystemValue->ppSampler = (SAMPLER**)malloc(sizeof(SAMPLER*) * SAMPLER_COUNT);
    for (int s = 0; s < SAMPLER_COUNT; ++s) {
        pSystemValue->ppSampler[s] = (SAMPLER*)malloc(sizeof(SAMPLER));
        memset(pSystemValue->ppSampler[s], 0, sizeof(SAMPLER));
    }
}

void sampler_dispose(SYSTEM_VALUE* pSystemValue) {
    if (NULL == pSystemValue->ppSampler) {
        return;
    }
    for (int s = 0; s < SAMPLER_COUNT; ++s) {
        free(pSystemValue->ppSampler[s]);
    }
    free(pSystemValue->ppSampler);
    pSystemValue->ppSampler = NULL;
}

Bool sampler(SYSTEM_VALUE *pSystemValue, SAMPLER* pSmpl) {
    auto pEffectParam = pSystemValue->ppEffect[pSmpl->channelNumber]->pParam;
    auto pOutput = pSystemValue->ppEffect[pSmpl->channelNumber]->pOutput;
    auto pOutputTerm = pOutput + pSystemValue->bufferLength;
    auto pWaveInfo = pSmpl->pWaveInfo;
    auto pEnvAmp = pSmpl->pEnvAmp;
    long loopEnd = (long)pWaveInfo->loopBegin + pWaveInfo->loopLength;
    auto pWaveData = (short*)(pSystemValue->pWaveTable + pWaveInfo->waveOfs);

    for (; pOutput < pOutputTerm; pOutput++) {
        //*******************************
        // generate wave
        //*******************************
        double smoothedWave = 0.0;
        double delta = pSmpl->delta * pEffectParam->pitch / OVER_SAMPLING;
        for (int o = 0; o < OVER_SAMPLING; o++) {
            int index = (int)pSmpl->index;
            double di = pSmpl->index - index;
            smoothedWave += (pWaveData[index - 1] * (1.0 - di) + pWaveData[index] * di) * pWaveInfo->gain;
            pSmpl->index += delta;
            if (loopEnd < pSmpl->index) {
                if (pWaveInfo->loopEnable) {
                    pSmpl->index -= pWaveInfo->loopLength;
                } else {
                    pSmpl->state = E_SAMPLER_STATE::FREE;
                    return false;
                }
            }
        }

        // output
        *pOutput += smoothedWave * pSmpl->velocity * pSmpl->egAmp / OVER_SAMPLING;

        //*******************************
        // generate envelope
        //*******************************
        switch (pSmpl->state) {
        case E_SAMPLER_STATE::PRESS:
            if (pSmpl->time <= pEnvAmp->hold) {
                pSmpl->egAmp += (1.0 - pSmpl->egAmp) * pEnvAmp->attack;
            } else {
                pSmpl->egAmp += (pEnvAmp->sustain - pSmpl->egAmp) * pEnvAmp->decay;
            }
            break;
        case E_SAMPLER_STATE::RELEASE:
            pSmpl->egAmp -= pSmpl->egAmp * pEnvAmp->release;
            break;
        case E_SAMPLER_STATE::HOLD:
            pSmpl->egAmp -= pSmpl->egAmp * pEffectParam->holdDelta;
            break;
        case E_SAMPLER_STATE::PURGE:
            pSmpl->egAmp -= pSmpl->egAmp * pSystemValue->deltaTime * PURGE_SPEED;
            break;
        }
        pSmpl->time += pSystemValue->deltaTime;

        //*******************************
        // free condition
        //*******************************
        if (pEnvAmp->hold < pSmpl->time && pSmpl->egAmp < PURGE_THRESHOLD) {
            pSmpl->state = E_SAMPLER_STATE::FREE;
            return false;
        }
    }
    return true;
}
