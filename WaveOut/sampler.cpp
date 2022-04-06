#include "sampler.h"
#include "effect.h"
#include "channel.h"

/******************************************************************************/
#define PURGE_THRESHOLD 0.0005
#define PURGE_SPEED     500
#define OVER_SAMPLING   4

/******************************************************************************/
Bool sampler(SYSTEM_VALUE* pSystemValue, INST_SAMPLER* pSmpl) {
    auto pEffectParam = pSystemValue->ppEffect[pSmpl->channelNum]->pParam;
    auto pOutput = pSystemValue->ppEffect[pSmpl->channelNum]->pOutput;
    auto pOutputTerm = pOutput + pSystemValue->bufferLength;
    auto pWaveInfo = pSmpl->pWave;
    auto pEnv = pSmpl->pEnv;
    auto pWaveData = (short*)(pSystemValue->pWaveTable) + pWaveInfo->offset;
    long loopEnd = (long)pWaveInfo->loopBegin + pWaveInfo->loopLength;
    auto pitch = pSmpl->pitch / pSystemValue->sampleRate / OVER_SAMPLING;

    for (; pOutput < pOutputTerm; pOutput++) {
        //*******************************
        // generate wave
        //*******************************
        double smoothedWave = 0.0;
        double delta = pitch * pEffectParam->pitch;
        for (int o = 0; o < OVER_SAMPLING; o++) {
            int index = (int)pSmpl->index;
            double di = pSmpl->index - index;
            smoothedWave += pWaveData[index - 1] * (1.0 - di) + pWaveData[index] * di;
            pSmpl->index += delta;
            if (loopEnd < pSmpl->index) {
                if (pWaveInfo->loopEnable) {
                    pSmpl->index -= pWaveInfo->loopLength;
                } else {
                    pSmpl->state = E_KEY_STATE::FREE;
                    return false;
                }
            }
        }

        /*** output ***/
        *pOutput += smoothedWave * pSmpl->gain * pSmpl->egAmp / OVER_SAMPLING;

        //*******************************
        // generate envelope
        //*******************************
        switch (pSmpl->state) {
        case E_KEY_STATE::PRESS:
            if (pSmpl->time <= pEnv->ampH) {
                pSmpl->egAmp += (1.0 - pSmpl->egAmp) * pEnv->ampA;
            } else {
                pSmpl->egAmp += (pEnv->ampS - pSmpl->egAmp) * pEnv->ampD;
            }
            break;
        case E_KEY_STATE::RELEASE:
            pSmpl->egAmp -= pSmpl->egAmp * pEnv->ampR;
            break;
        case E_KEY_STATE::HOLD:
            pSmpl->egAmp -= pSmpl->egAmp * pEffectParam->holdDelta;
            break;
        case E_KEY_STATE::PURGE:
            pSmpl->egAmp -= pSmpl->egAmp * pSystemValue->deltaTime * PURGE_SPEED;
            break;
        }
        pSmpl->time += pSystemValue->deltaTime;

        //*******************************
        // free condition
        //*******************************
        if (pEnv->ampH < pSmpl->time && pSmpl->egAmp < PURGE_THRESHOLD) {
            pSmpl->state = E_KEY_STATE::FREE;
            return false;
        }
    }
    return true;
}
