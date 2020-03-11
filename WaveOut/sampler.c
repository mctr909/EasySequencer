#include <stdlib.h>
#include <string.h>
#include <math.h>
#include "sampler.h"

/******************************************************************************/
#define PURGE_THRESHOLD 0.0005
#define PURGE_SPEED     500

#define OVER_SAMPLING   8
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
        pCh->choLfoU = 0.505 + 0.495;
        pCh->choLfoV = 0.505 + 0.495 * -0.5;
        pCh->choLfoW = 0.505 + 0.495 * -0.5;
        pCh->choPanUL = cos(0.5 * PI * 0 / 3.0);
        pCh->choPanUR = sin(0.5 * PI * 0 / 3.0);
        pCh->choPanVL = cos(0.5 * PI * 1 / 3.0);
        pCh->choPanVR = sin(0.5 * PI * 1 / 3.0);
        pCh->choPanWL = cos(0.5 * PI * 2 / 3.0);
        pCh->choPanWR = sin(0.5 * PI * 2 / 3.0);
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
        for (auto o = 0; o < OVER_SAMPLING; o++) {
            int pos = (int)pSmpl->index;
            double dt = pSmpl->index - pos;
            sumWave += (pWave[pos - 1] * (1.0 - dt) + pWave[pos] * dt) * pWaveInfo->gain;
            //
            pSmpl->index += pWaveInfo->delta * pChParam->pitch / OVER_SAMPLING;
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
        *pOutput += sumWave * pSmpl->velocity * pSmpl->egAmp / OVER_SAMPLING;
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

inline void oscillator(CHANNEL_VALUE **ppCh, SAMPLER *pSmpl, byte *pWaveBuffer) {
    CHANNEL_VALUE *pChValue = ppCh[pSmpl->channelNum];
    SYSTEM_VALUE *pSystemValue = pChValue->pSystemValue;
    CHANNEL *pChParam = pChValue->pParam;
    ENVELOPE *pEnvAmp = &pSmpl->envAmp;
    ENVELOPE *pEnvPitch = &pSmpl->envPitch;
    ENVELOPE *pEnvCutoff = &pSmpl->envCutoff;
    FILTER *pFilter = &pSmpl->filter;


    double *pOutput = pChValue->pWave;
    double *pOutputTerm = pOutput + pSystemValue->bufferLength;
    
    pChValue->state = E_CH_STATE_ACTIVE;

    for (; pOutput < pOutputTerm; pOutput++) {
        //*******************************
        // generate wave
        //*******************************
        double sumWave = 0.0;
        for (int w = 0; w < 8; w++) {
            if (0.0 == pSmpl->gain[w]) {
                continue;
            }
            double gain = pSmpl->gain[w] * pSmpl->velocity * pSmpl->egAmp * 0.0625;
            double delta = pSmpl->pitch[w] * pSmpl->egPitch * pChParam->pitch * pSystemValue->deltaTime * 0.0625;
            double *value = &pSmpl->value[w];
            double *param = &pSmpl->param[w];
            switch (pSmpl->waveForm[w]) {
            case E_WAVE_FORM_SINE:
                for (int o = 0; o < 16; o++) {
                    sumWave += *value * gain;
                    *param -= *value * PI2 * delta;
                    *value += *param * PI2 * delta;
                }
                break;
            case E_WAVE_FORM_PWM:
                for (int o = 0; o < 16; o++) {
                    sumWave += (*value < *param) ? gain : -gain;
                    *value += delta;
                    if (1.0 <= *value) {
                        *value -= 1.0;
                    }
                }
                break;
            case E_WAVE_FORM_SAW:
                for (int o = 0; o < 16; o++) {
                    sumWave += gain * (*value < 0.5) ? (*value * 2.0) : (*value * 2.0 - 2.0);
                    *value += delta;
                    if (1.0 <= *value) {
                        *value -= 1.0;
                    }
                }
                break;
            case E_WAVE_FORM_TRI:
                for (int o = 0; o < 16; o++) {
                    if (*value < 0.25) {
                        sumWave += gain * *value * 4.0;
                    } else if (*value < 0.75) {
                        sumWave += gain * (2.0 - *value * 4.0);
                    } else {
                        sumWave += gain * (*value * 4.0 - 4.0);
                    }
                    *value += delta;
                    if (1.0 <= *value) {
                        *value -= 1.0;
                    }
                }
                break;
            }
        }
        // output
        filter_lpf(pFilter, sumWave);
        *pOutput += pFilter->a10;
        //*******************************
        // generate envelope
        //*******************************
        switch (pSmpl->state) {
        case E_KEY_STATE_PURGE:
            pSmpl->egAmp -= pSmpl->egAmp * pSystemValue->deltaTime * PURGE_SPEED;
            pSmpl->egPitch += (pEnvPitch->fall - pSmpl->egPitch) * pEnvPitch->release;
            pFilter->cut += (pEnvCutoff->fall - pFilter->cut) * pEnvCutoff->release;
            break;
        case E_KEY_STATE_RELEASE:
            pSmpl->egAmp -= pSmpl->egAmp * pEnvAmp->release;
            pSmpl->egPitch += (pEnvPitch->fall - pSmpl->egPitch) * pEnvPitch->release;
            pFilter->cut += (pEnvCutoff->fall - pFilter->cut) * pEnvCutoff->release;
            break;
        case E_KEY_STATE_HOLD:
            pSmpl->egAmp -= pSmpl->egAmp * pChParam->holdDelta;
            pSmpl->egPitch += (pEnvPitch->fall - pSmpl->egPitch) * pEnvPitch->release;
            pFilter->cut += (pEnvCutoff->fall - pFilter->cut) * pEnvCutoff->release;
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
            if (pSmpl->time <= pEnvCutoff->hold) {
                pFilter->cut += (pEnvCutoff->top - pFilter->cut) * pEnvCutoff->attack;
            } else {
                pFilter->cut += (pEnvCutoff->sustain - pFilter->cut) * pEnvCutoff->decay;
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
    /*** output delay ***/
    {
        int delayIndex = pCh->writeIndex - (int)(pParam->delayTime * pCh->pSystemValue->sampleRate);
        if (delayIndex < 0) {
            delayIndex += DELAY_TAPS;
        }
        double delayL = pParam->delaySend * pTapL[delayIndex];
        double delayR = pParam->delaySend * pTapR[delayIndex];
        *waveL += (delayL * (1.0 - pParam->delayCross) + delayR * pParam->delayCross);
        *waveR += (delayR * (1.0 - pParam->delayCross) + delayL * pParam->delayCross);
        pTapL[pCh->writeIndex] = *waveL;
        pTapR[pCh->writeIndex] = *waveR;
    }
    /*** output chorus ***/
    {
        double depth = pCh->pSystemValue->sampleRate * pParam->chorusDepth;
        double posU = pCh->writeIndex - pCh->choLfoU * depth;
        double posV = pCh->writeIndex - pCh->choLfoV * depth;
        double posW = pCh->writeIndex - pCh->choLfoW * depth;
        int idxU = (int)posU;
        int idxV = (int)posV;
        int idxW = (int)posW;
        double dtU = posU - idxU;
        double dtV = posV - idxV;
        double dtW = posW - idxW;
        if (idxU < 0) {
            idxU += DELAY_TAPS;
        }
        if (DELAY_TAPS <= idxU) {
            idxU -= DELAY_TAPS;
        }
        if (idxV < 0) {
            idxV += DELAY_TAPS;
        }
        if (DELAY_TAPS <= idxV) {
            idxV -= DELAY_TAPS;
        }
        if (idxW < 0) {
            idxW += DELAY_TAPS;
        }
        if (DELAY_TAPS <= idxW) {
            idxW -= DELAY_TAPS;
        }
        double chorusL = 0.0;
        double chorusR = 0.0;
        if (idxU == 0) {
            chorusL += pCh->choPanUL * (pTapL[DELAY_TAPS - 1] * (1.0 - dtU) + pTapL[idxU] * dtU);
            chorusR += pCh->choPanUR * (pTapR[DELAY_TAPS - 1] * (1.0 - dtU) + pTapR[idxU] * dtU);
        } else {
            chorusL += pCh->choPanUL * (pTapL[idxU - 1] * (1.0 - dtU) + pTapL[idxU] * dtU);
            chorusR += pCh->choPanUR * (pTapR[idxU - 1] * (1.0 - dtU) + pTapR[idxU] * dtU);
        }
        if (idxV == 0) {
            chorusL += pCh->choPanVL * (pTapL[DELAY_TAPS - 1] * (1.0 - dtV) + pTapL[idxV] * dtV);
            chorusR += pCh->choPanVR * (pTapR[DELAY_TAPS - 1] * (1.0 - dtV) + pTapR[idxV] * dtV);
        } else {
            chorusL += pCh->choPanVL * (pTapL[idxV - 1] * (1.0 - dtV) + pTapL[idxV] * dtV);
            chorusR += pCh->choPanVR * (pTapR[idxV - 1] * (1.0 - dtV) + pTapR[idxV] * dtV);
        }
        if (idxW == 0) {
            chorusL += pCh->choPanWL * (pTapL[DELAY_TAPS - 1] * (1.0 - dtW) + pTapL[idxW] * dtW);
            chorusR += pCh->choPanWR * (pTapR[DELAY_TAPS - 1] * (1.0 - dtW) + pTapR[idxW] * dtW);
        } else {
            chorusL += pCh->choPanWL * (pTapL[idxW - 1] * (1.0 - dtW) + pTapL[idxW] * dtW);
            chorusR += pCh->choPanWR * (pTapR[idxW - 1] * (1.0 - dtW) + pTapR[idxW] * dtW);
        }
        *waveL += chorusL * pParam->chorusSend / 3.0;
        *waveR += chorusR * pParam->chorusSend / 3.0;
    }
    /*** update lfo ***/
    double lfoDelta = PI2 * INV_SQRT3 * pParam->chorusRate * pCh->pSystemValue->deltaTime;
    pCh->choLfoU += (pCh->choLfoV - pCh->choLfoW) * lfoDelta;
    pCh->choLfoV += (pCh->choLfoW - pCh->choLfoU) * lfoDelta;
    pCh->choLfoW += (pCh->choLfoU - pCh->choLfoV) * lfoDelta;
    /*** next step ***/
    double transitionDelta = pCh->pSystemValue->deltaTime * VALUE_TRANSITION_SPEED;
    pCh->amp        += (pCh->pParam->amp       - pCh->amp)        * transitionDelta;
    pCh->panL       += (pCh->pParam->panLeft   - pCh->panL)       * transitionDelta;
    pCh->panR       += (pCh->pParam->panRight  - pCh->panR)       * transitionDelta;
    pCh->filter.cut += (pCh->pParam->cutoff    - pCh->filter.cut) * transitionDelta;
    pCh->filter.res += (pCh->pParam->resonance - pCh->filter.res) * transitionDelta;
    pCh->state = E_CH_STATE_STANDBY;
}
