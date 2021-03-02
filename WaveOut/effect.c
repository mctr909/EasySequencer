#include "effect.h"
#include "sampler.h"
#include <stdlib.h>
#include <string.h>
#include <math.h>

/******************************************************************************/
#define DELAY_TAPS             1048576
#define VALUE_TRANSITION_SPEED 250

#define PI        3.14159265
#define PI2       6.28318531
#define INV_SQRT3 0.577350269

/******************************************************************************/
CHANNEL_VALUE** createChannels(SYSTEM_VALUE* pSys) {
    CHANNEL_VALUE** channel = (CHANNEL_VALUE**)malloc(sizeof(CHANNEL_VALUE*) * pSys->channelCount);
    for (int i = 0; i < pSys->channelCount; ++i) {
        CHANNEL_VALUE* pCh = (CHANNEL_VALUE*)malloc(sizeof(CHANNEL_VALUE));
        memset(pCh, 0, sizeof(CHANNEL_VALUE));
        channel[i] = pCh;

        pCh->pSystemValue = pSys;

        // allocate wave buffer
        pCh->pWave = (double*)malloc(sizeof(double) * pCh->pSystemValue->bufferLength * 2);
        memset(pCh->pWave, 0, sizeof(double) * pCh->pSystemValue->bufferLength * 2);
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

void disposeChannels(CHANNEL_VALUE** ppCh) {
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

/******************************************************************************/
inline void effect(CHANNEL_VALUE* pCh, double* waveL, double* waveR) {
    CHANNEL* pParam = pCh->pParam;
    double* pTapL = pCh->pDelTapL;
    double* pTapR = pCh->pDelTapR;
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
    pCh->amp += (pCh->pParam->amp - pCh->amp) * transitionDelta;
    pCh->panL += (pCh->pParam->panLeft - pCh->panL) * transitionDelta;
    pCh->panR += (pCh->pParam->panRight - pCh->panR) * transitionDelta;
    pCh->filter.cut += (pCh->pParam->cutoff - pCh->filter.cut) * transitionDelta;
    pCh->filter.res += (pCh->pParam->resonance - pCh->filter.res) * transitionDelta;
}
