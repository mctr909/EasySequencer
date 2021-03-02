#include <stdlib.h>
#include "sampler.h"

/******************************************************************************/
#define PURGE_THRESHOLD 0.0005
#define PURGE_SPEED     500
#define OVER_SAMPLING   4

/******************************************************************************/
NOTE** createNotes(uint count) {
    NOTE** notes = (NOTE**)malloc(sizeof(NOTE*) * count);
    for (uint i = 0; i < count; ++i) {
        notes[i] = (NOTE*)malloc(sizeof(NOTE));
        memset(notes[i], 0, sizeof(NOTE));
    }
    return notes;
}

SAMPLER** createSamplers(uint count) {
    SAMPLER** samplers = (SAMPLER**)malloc(sizeof(SAMPLER*) * count);
    for (uint i = 0; i < count; ++i) {
        samplers[i] = (SAMPLER*)malloc(sizeof(SAMPLER));
        memset(samplers[i], 0, sizeof(SAMPLER));
    }
    return samplers;
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

/******************************************************************************/
inline Bool sampler(CHANNEL_VALUE** ppCh, SAMPLER* pSmpl, byte* pWaveBuffer) {
    NOTE* pNote = pSmpl->pNote;
    CHANNEL_VALUE* pChValue = ppCh[pNote->channelNum];
    SYSTEM_VALUE* pSystemValue = pChValue->pSystemValue;
    CHANNEL* pChParam = pChValue->pParam;
    WAVE_INFO* pWaveInfo = pSmpl->pWaveInfo;
    ENVELOPE* pEnvAmp = pSmpl->pEnvAmp;

    long loopEnd = (long)pWaveInfo->loopBegin + pWaveInfo->loopLength;
    short* pWave = (short*)(pWaveBuffer + pWaveInfo->waveOfs);
    double* pOutput = pChValue->pWave;
    double* pOutputTerm = pOutput + pSystemValue->bufferLength;

    for (; pOutput < pOutputTerm; pOutput++) {
        //*******************************
        // generate wave
        //*******************************
        double smoothedWave = 0.0;
        double delta = pSmpl->delta * pChParam->pitch / OVER_SAMPLING;
        for (int o = 0; o < OVER_SAMPLING; o++) {
            int idx = (int)pSmpl->index;
            double dt = pSmpl->index - idx;
            smoothedWave += (pWave[idx - 1] * (1.0 - dt) + pWave[idx] * dt) * pWaveInfo->gain;
            //
            pSmpl->index += delta;
            if (loopEnd < pSmpl->index) {
                if (pWaveInfo->loopEnable) {
                    pSmpl->index -= pWaveInfo->loopLength;
                } else {
                    pSmpl->index = loopEnd;
                    return true;
                }
            }
        }
        // output
        *pOutput += smoothedWave * pNote->velocity * pSmpl->egAmp / OVER_SAMPLING;
        //*******************************
        // generate envelope
        //*******************************
        switch (pNote->state) {
        case E_NOTE_STATE_PURGE:
            pSmpl->egAmp -= pSmpl->egAmp * pSystemValue->deltaTime * PURGE_SPEED;
            break;
        case E_NOTE_STATE_RELEASE:
            pSmpl->egAmp -= pSmpl->egAmp * pEnvAmp->release;
            break;
        case E_NOTE_STATE_HOLD:
            pSmpl->egAmp -= pSmpl->egAmp * pChParam->holdDelta;
            break;
        case E_NOTE_STATE_PRESS:
            if (pSmpl->time <= pEnvAmp->hold) {
                pSmpl->egAmp += (1.0 - pSmpl->egAmp) * pEnvAmp->attack;
            } else {
                pSmpl->egAmp += (pEnvAmp->sustain - pSmpl->egAmp) * pEnvAmp->decay;
            }
            break;
        }
        pSmpl->time += pSystemValue->deltaTime;
        //*******************************
        // free condition
        //*******************************
        if (pEnvAmp->hold < pSmpl->time && pSmpl->egAmp < PURGE_THRESHOLD) {
            return true;
        }
    }
    return false;
}
