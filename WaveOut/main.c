﻿#include "main.h"
#include "sampler.h"
#include "wave_out.h"
#include "filter.h"

#include <stdio.h>

/******************************************************************************/
int           gActiveCount = 0;
LPBYTE        gpWaveTable = NULL;
SYSTEM_VALUE  gSysValue = { 0 };
CHANNEL_VALUE **gppChValues = NULL;
CHANNEL       **gppChParams = NULL;
SAMPLER       **gppSamplers = NULL;

/******************************************************************************/
inline void setSampler();
void write16(LPBYTE pData);
void write24(LPBYTE pData);
void write32(LPBYTE pData);

/******************************************************************************/
int* waveout_GetActiveSamplersPtr() {
    return &gActiveCount;
}

CHANNEL** waveout_GetChannelPtr() {
    return gppChParams;
}

SAMPLER** waveout_GetSamplerPtr() {
    return gppSamplers;
}

LPBYTE waveout_LoadWaveTable(LPWSTR filePath, uint *size) {
    if (NULL == size) {
        return NULL;
    }
    waveout_close();
    //
    if (NULL != gpWaveTable) {
        free(gpWaveTable);
        gpWaveTable = NULL;
    }
    //
    FILE *fpDLS = NULL;
    _wfopen_s(&fpDLS, filePath, TEXT("rb"));
    if (NULL != fpDLS) {
        fseek(fpDLS, 4, SEEK_SET);
        fread_s(size, sizeof(*size), sizeof(*size), 1, fpDLS);
        *size -= 8;
        gpWaveTable = (LPBYTE)malloc(*size);
        if (NULL != gpWaveTable) {
            fseek(fpDLS, 12, SEEK_SET);
            fread_s(gpWaveTable, *size, *size, 1, fpDLS);
        }
        fclose(fpDLS);
    }
    return gpWaveTable;
}

void waveout_SystemValues(
    int sampleRate,
    int bits,
    int bufferLength,
    int bufferCount,
    int channelCount,
    int samplerCount
) {
    waveout_Close();
    //
    gSysValue.bufferLength = bufferLength;
    gSysValue.bufferCount = bufferCount;
    gSysValue.channelCount = channelCount;
    gSysValue.samplerCount = samplerCount;
    gSysValue.sampleRate = sampleRate;
    gSysValue.bits = bits;
    gSysValue.deltaTime = 1.0 / sampleRate;
    //
    gppChValues = createChannels(&gSysValue);
    free(gppChParams);
    gppChParams = (CHANNEL**)malloc(sizeof(CHANNEL*) * gSysValue.channelCount);
    for (int i = 0; i < gSysValue.channelCount; ++i) {
        gppChParams[i] = gppChValues[i]->pParam;
    }
    //
    gppSamplers = createSamplers(gSysValue.samplerCount);
}

void waveout_Open() {
    switch (gSysValue.bits) {
    case 16:
        waveout_open(gSysValue.sampleRate, 16, 2, gSysValue.bufferLength, gSysValue.bufferCount, write16);
        break;
    case 24:
        waveout_open(gSysValue.sampleRate, 24, 2, gSysValue.bufferLength, gSysValue.bufferCount, write24);
        break;
    case 32:
        waveout_open(gSysValue.sampleRate, 32, 2, gSysValue.bufferLength, gSysValue.bufferCount, write32);
        break;
    default:
        break;
    }
}

void waveout_Close() {
    waveout_close();
    disposeChannels(gppChValues);
    disposeSamplers(gppSamplers, gSysValue.samplerCount);
}

/******************************************************************************/
inline void setSampler() {
    int activeCount = 0;
    for (int sj = 0; sj < gSysValue.samplerCount; sj++) {
        if (E_KEY_STATE_STANDBY == gppSamplers[sj]->state) {
            continue;
        }
        sampler(gppChValues, gppSamplers[sj], gpWaveTable);
        activeCount++;
    }
    gActiveCount = activeCount;
}

void write16(LPBYTE pData) {
    setSampler();
    for (int i = 0; i < gSysValue.channelCount; i++) {
        CHANNEL_VALUE* pCh = gppChValues[i];
        double* inputBuff = pCh->pWave;
        double* inputBuffTerm = inputBuff + pCh->pSystemValue->bufferLength;
        short* pBuff = (short*)pData;
        for (; inputBuff < inputBuffTerm; inputBuff++, pBuff += 2) {
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
            tempL += *(pBuff + 0);
            tempR += *(pBuff + 1);
            if (32767.0 < tempL) tempL = 32767.0;
            if (tempL < -32767.0) tempL = -32767.0;
            if (32767.0 < tempR) tempR = 32767.0;
            if (tempR < -32767.0) tempR = -32767.0;
            *(pBuff + 0) = (short)tempL;
            *(pBuff + 1) = (short)tempR;
            *inputBuff = 0.0;
        }
    }
}

void write24(LPBYTE pData) {
    setSampler();
    for (int i = 0; i < gSysValue.channelCount; i++) {
        CHANNEL_VALUE* pCh = gppChValues[i];
        double* inputBuff = pCh->pWave;
        double* inputBuffTerm = inputBuff + pCh->pSystemValue->bufferLength;
        int24* pBuff = (int24*)pData;
        for (; inputBuff < inputBuffTerm; inputBuff++, pBuff += 2) {
            // filter
            filter_lpf(&pCh->filter, *inputBuff * pCh->amp);
            // pan
            double tempL = pCh->filter.a10 * pCh->panL;
            double tempR = pCh->filter.a10 * pCh->panR;
            // effect
            effect(pCh, &tempL, &tempR);
            // output
            tempL += fromInt24(pBuff + 0);
            tempR += fromInt24(pBuff + 1);
            if (1.0 < tempL) tempL = 1.0;
            if (tempL < -1.0) tempL = -1.0;
            if (1.0 < tempR) tempR = 1.0;
            if (tempR < -1.0) tempR = -1.0;
            setInt24(pBuff + 0, tempL);
            setInt24(pBuff + 1, tempR);
            *inputBuff = 0.0;
        }
    }
}

void write32(LPBYTE pData) {
    setSampler();
    for (int i = 0; i < gSysValue.channelCount; i++) {
        CHANNEL_VALUE* pCh = gppChValues[i];
        double* inputBuff = pCh->pWave;
        double* inputBuffTerm = inputBuff + pCh->pSystemValue->bufferLength;
        float* pBuff = (float*)pData;
        for (; inputBuff < inputBuffTerm; inputBuff++, pBuff += 2) {
            // filter
            filter_lpf(&pCh->filter, *inputBuff * pCh->amp);
            // pan
            double tempL = pCh->filter.a10 * pCh->panL;
            double tempR = pCh->filter.a10 * pCh->panR;
            // effect
            effect(pCh, &tempL, &tempR);
            // output
            tempL += *(pBuff + 0);
            tempR += *(pBuff + 1);
            if (1.0 < tempL) tempL = 1.0;
            if (tempL < -1.0) tempL = -1.0;
            if (1.0 < tempR) tempR = 1.0;
            if (tempR < -1.0) tempR = -1.0;
            *(pBuff + 0) = (float)tempL;
            *(pBuff + 1) = (float)tempR;
            *inputBuff = 0.0;
        }
    }
}
