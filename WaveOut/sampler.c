#include <stdlib.h>
#include <string.h>
#include "sampler.h"

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
    CHANNEL *pChValue = ppCh[pSmpl->channelNum];
    SYSTEM_VALUE *pSystemValue = pChValue->pSystemValue;
    CHANNEL_PARAM *pChParam = pChValue->pParam;
    WAVE_INFO *pWaveInfo = &pSmpl->waveInfo;
    ENVELOPE *pEnvAmp = &pSmpl->envAmp;

    SInt16 *pWave = (SInt16*)(pWaveBuffer + pWaveInfo->waveOfs);
    double *pOutput = pChValue->pWave;
    double *pOutputTerm = pOutput + pSystemValue->bufferLength;

    for (; pOutput < pOutputTerm; pOutput++) {
        //*******************************
        // generate wave
        //*******************************
        double sumWave = 0.0;
        for (auto o = 0; o < 16; o++) {
            SInt32 pos = (SInt32)pSmpl->index;
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
