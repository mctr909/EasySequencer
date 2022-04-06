#include "sampler.h"
#include "effect.h"
#include "channel_const.h"

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
void effect_create(SYSTEM_VALUE* pSystemValue) {
    effect_dispose(pSystemValue);
    pSystemValue->ppEffect = (EFFECT**)malloc(sizeof(EFFECT*) * CHANNEL_COUNT);
    for (int c = 0; c < CHANNEL_COUNT; c++) {
        pSystemValue->ppEffect[c] = (EFFECT*)malloc(sizeof(EFFECT));
        memset(pSystemValue->ppEffect[c], 0, sizeof(EFFECT));

        auto pEffect = pSystemValue->ppEffect[c];
        pEffect->pSystemValue = pSystemValue;

        // allocate output buffer
        pEffect->pOutput = (double*)malloc(sizeof(double) * pEffect->pSystemValue->bufferLength);
        memset(pEffect->pOutput, 0, sizeof(double) * pEffect->pSystemValue->bufferLength);

        // allocate effect params
        pEffect->pParam = (EFFECT_PARAM*)malloc(sizeof(EFFECT_PARAM));
        memset(pEffect->pParam, 0, sizeof(EFFECT_PARAM));

        // allocate delay taps
        pEffect->writeIndex = 0;
        pEffect->pDelTapL = (double*)malloc(sizeof(double) * DELAY_TAPS);
        pEffect->pDelTapR = (double*)malloc(sizeof(double) * DELAY_TAPS);
        memset(pEffect->pDelTapL, 0, sizeof(double) * DELAY_TAPS);
        memset(pEffect->pDelTapR, 0, sizeof(double) * DELAY_TAPS);

        // initialize chorus
        pEffect->choLfoU = 0.505 + 0.495;
        pEffect->choLfoV = 0.505 + 0.495 * -0.5;
        pEffect->choLfoW = 0.505 + 0.495 * -0.5;
        pEffect->choPanUL = cos(0.5 * PI * 0 / 3.0);
        pEffect->choPanUR = sin(0.5 * PI * 0 / 3.0);
        pEffect->choPanVL = cos(0.5 * PI * 1 / 3.0);
        pEffect->choPanVR = sin(0.5 * PI * 1 / 3.0);
        pEffect->choPanWL = cos(0.5 * PI * 2 / 3.0);
        pEffect->choPanWR = sin(0.5 * PI * 2 / 3.0);

        // initialize filter
        memset(&pEffect->filter, 0, sizeof(FILTER));
    }
}

void effect_dispose(SYSTEM_VALUE* pSystemValue) {
    auto ppEffect = pSystemValue->ppEffect;
    if (NULL == ppEffect) {
        return;
    }
    for (int c = 0; c < CHANNEL_COUNT; c++) {
        free(ppEffect[c]->pOutput);
        free(ppEffect[c]->pParam);
        free(ppEffect[c]->pDelTapL);
        free(ppEffect[c]->pDelTapR);
        free(ppEffect[c]);
    }
    free(pSystemValue->ppEffect);
    pSystemValue->ppEffect = NULL;
}

void effect(EFFECT* pEffect, double *pInput, double* pOutputL, double* pOutputR) {
    auto pParam = pEffect->pParam;

    /*** filter ***/
    filter_lpf(&pEffect->filter, *pInput * pEffect->amp);

    /*** pan ***/
    *pOutputL = pEffect->filter.a10 * pEffect->panL;
    *pOutputR = pEffect->filter.a10 * pEffect->panR;

    auto pDelayTapL = pEffect->pDelTapL;
    auto pDelayTapR = pEffect->pDelTapR;
    pEffect->writeIndex++;
    if (DELAY_TAPS <= pEffect->writeIndex) {
        pEffect->writeIndex = 0;
    }

    /*** output delay ***/
    {
        int delayIndex = pEffect->writeIndex - (int)(pParam->delayTime * pEffect->pSystemValue->sampleRate);
        if (delayIndex < 0) {
            delayIndex += DELAY_TAPS;
        }
        double delayL = pParam->delaySend * pDelayTapL[delayIndex];
        double delayR = pParam->delaySend * pDelayTapR[delayIndex];
        *pOutputL += (delayL * (1.0 - pParam->delayCross) + delayR * pParam->delayCross);
        *pOutputR += (delayR * (1.0 - pParam->delayCross) + delayL * pParam->delayCross);
        pDelayTapL[pEffect->writeIndex] = *pOutputL;
        pDelayTapR[pEffect->writeIndex] = *pOutputR;
    }

    /*** output chorus ***/
    {
        double depth = pEffect->pSystemValue->sampleRate * pParam->chorusDepth;
        double posU = pEffect->writeIndex - pEffect->choLfoU * depth;
        double posV = pEffect->writeIndex - pEffect->choLfoV * depth;
        double posW = pEffect->writeIndex - pEffect->choLfoW * depth;
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
            chorusL += pEffect->choPanUL * (pDelayTapL[DELAY_TAPS - 1] * (1.0 - dtU) + pDelayTapL[idxU] * dtU);
            chorusR += pEffect->choPanUR * (pDelayTapR[DELAY_TAPS - 1] * (1.0 - dtU) + pDelayTapR[idxU] * dtU);
        } else {
            chorusL += pEffect->choPanUL * (pDelayTapL[idxU - 1] * (1.0 - dtU) + pDelayTapL[idxU] * dtU);
            chorusR += pEffect->choPanUR * (pDelayTapR[idxU - 1] * (1.0 - dtU) + pDelayTapR[idxU] * dtU);
        }
        if (idxV == 0) {
            chorusL += pEffect->choPanVL * (pDelayTapL[DELAY_TAPS - 1] * (1.0 - dtV) + pDelayTapL[idxV] * dtV);
            chorusR += pEffect->choPanVR * (pDelayTapR[DELAY_TAPS - 1] * (1.0 - dtV) + pDelayTapR[idxV] * dtV);
        } else {
            chorusL += pEffect->choPanVL * (pDelayTapL[idxV - 1] * (1.0 - dtV) + pDelayTapL[idxV] * dtV);
            chorusR += pEffect->choPanVR * (pDelayTapR[idxV - 1] * (1.0 - dtV) + pDelayTapR[idxV] * dtV);
        }
        if (idxW == 0) {
            chorusL += pEffect->choPanWL * (pDelayTapL[DELAY_TAPS - 1] * (1.0 - dtW) + pDelayTapL[idxW] * dtW);
            chorusR += pEffect->choPanWR * (pDelayTapR[DELAY_TAPS - 1] * (1.0 - dtW) + pDelayTapR[idxW] * dtW);
        } else {
            chorusL += pEffect->choPanWL * (pDelayTapL[idxW - 1] * (1.0 - dtW) + pDelayTapL[idxW] * dtW);
            chorusR += pEffect->choPanWR * (pDelayTapR[idxW - 1] * (1.0 - dtW) + pDelayTapR[idxW] * dtW);
        }
        *pOutputL += chorusL * pParam->chorusSend / 3.0;
        *pOutputR += chorusR * pParam->chorusSend / 3.0;
    }

    /*** update lfo ***/
    double lfoDelta = PI2 * INV_SQRT3 * pParam->chorusRate * pEffect->pSystemValue->deltaTime;
    pEffect->choLfoU += (pEffect->choLfoV - pEffect->choLfoW) * lfoDelta;
    pEffect->choLfoV += (pEffect->choLfoW - pEffect->choLfoU) * lfoDelta;
    pEffect->choLfoW += (pEffect->choLfoU - pEffect->choLfoV) * lfoDelta;

    /*** next step ***/
    double transitionDelta = pEffect->pSystemValue->deltaTime * VALUE_TRANSITION_SPEED;
    pEffect->amp += (pParam->amp - pEffect->amp) * transitionDelta;
    pEffect->panL += (pParam->panLeft - pEffect->panL) * transitionDelta;
    pEffect->panR += (pParam->panRight - pEffect->panR) * transitionDelta;
    pEffect->filter.cut += (pParam->cutoff - pEffect->filter.cut) * transitionDelta;
    pEffect->filter.res += (pParam->resonance - pEffect->filter.res) * transitionDelta;
}
