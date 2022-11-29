#include "sampler.h"
#include "channel.h"
#include "../inst/inst_list.h"

/******************************************************************************/
#define PURGE_THRESHOLD 0.0005
#define PURGE_SPEED     500
#define OVER_SAMPLING   8

/******************************************************************************/
inline Bool sampler(SYSTEM_VALUE* pSystemValue, INST_SAMPLER* pSmpl) {
    auto pCh = pSystemValue->ppChannels[pSmpl->channelNum];
    auto pOutput_l = pCh->pInput_l;
    auto pOutput_r = pCh->pInput_r;
    auto pWaveInfo = pSmpl->pWave;
    auto pEnv = pSmpl->pEnv;
    auto pWaveData = pSystemValue->pWaveTable + pWaveInfo->offset;
    long loopEnd = (long)pWaveInfo->loopBegin + pWaveInfo->loopLength;
    auto pitch = pSmpl->pitch / pSystemValue->sampleRate / OVER_SAMPLING;

    for (int i = 0; i < pSystemValue->bufferLength; i++) {
        //*******************************
        // generate wave
        //*******************************
        double smoothedWave = 0.0;
        double delta = pitch * pCh->pitch;
        for (int o = 0; o < OVER_SAMPLING; o++) {
            int index = (int)pSmpl->index;
            double di = pSmpl->index - index;
            smoothedWave += pWaveData[index - 1] * (1.0 - di) + pWaveData[index] * di;
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

        /*** output ***/
        auto wave = smoothedWave * pSmpl->gain * pSmpl->egAmp / OVER_SAMPLING;
        pOutput_l[i] += wave;
        pOutput_r[i] += wave;

        //*******************************
        // generate envelope
        //*******************************
        switch (pSmpl->state) {
        case E_SAMPLER_STATE::PRESS:
            if (pSmpl->time <= pEnv->ampH) {
                pSmpl->egAmp += (1.0 - pSmpl->egAmp) * pSystemValue->deltaTime * pEnv->ampA;
            } else {
                pSmpl->egAmp += (pEnv->ampS - pSmpl->egAmp) * pSystemValue->deltaTime * pEnv->ampD;
            }
            break;
        case E_SAMPLER_STATE::RELEASE:
            pSmpl->egAmp -= pSmpl->egAmp * pSystemValue->deltaTime * pEnv->ampR;
            break;
        case E_SAMPLER_STATE::HOLD:
            pSmpl->egAmp -= pSmpl->egAmp * pSystemValue->deltaTime;
            break;
        case E_SAMPLER_STATE::PURGE:
            pSmpl->egAmp -= pSmpl->egAmp * pSystemValue->deltaTime * PURGE_SPEED;
            break;
        }
        pSmpl->time += pSystemValue->deltaTime;

        //*******************************
        // free condition
        //*******************************
        if (pEnv->ampH < pSmpl->time && pSmpl->egAmp < PURGE_THRESHOLD) {
            pSmpl->state = E_SAMPLER_STATE::FREE;
            return false;
        }
    }
    return true;
}
