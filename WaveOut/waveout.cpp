#include "waveout.h"
#include "message_reciever.h"
#include "inst/inst_list.h"
#include "synth/channel.h"
#include "synth/sampler.h"
#include "synth/effect.h"

#include <stdio.h>
#include <mmsystem.h>

#pragma comment (lib, "winmm.lib")

/******************************************************************************/
DWORD            gThreadId;
CRITICAL_SECTION gcsBufferLock;

volatile BOOL   gDoStop = TRUE;
volatile BOOL   gWaveOutStopped = TRUE;
volatile BOOL   gThreadStopped = TRUE;

volatile int    gWriteCount = 0;
volatile int    gWriteIndex = 0;
volatile int    gReadIndex = 0;
int             gBufferCount = 0;
int             gBufferLength = 0;

HWAVEOUT        ghWaveOut = NULL;
WAVEFORMATEX    gWaveFmt = { 0 };
WAVEHDR         **gppWaveHdr = NULL;

void (*gfpWriteBuffer)(LPSTR) = NULL;

/******************************************************************************/
int           gActiveCount = 0;
SYSTEM_VALUE  gSysValue = { 0 };

/******************************************************************************/
inline void runSampler();
void write16(LPSTR pData);
void write24(LPSTR pData);
void write32(LPSTR pData);

BOOL waveOutOpen(
    int sampleRate,
    int bits,
    int channelCount,
    int bufferLength,
    int bufferCount,
    void(*fpWriteBufferProc)(LPSTR)
);
BOOL waveOutClose();
void CALLBACK waveOutProc(HWAVEOUT hwo, UINT uMsg, DWORD_PTR dwInstance, DWORD dwParam1, DWORD dwParam);
DWORD writeBufferTask(LPVOID *param);

/******************************************************************************/
int* WINAPI waveout_getActiveSamplersPtr() {
    return &gActiveCount;
}

LPBYTE WINAPI waveout_open(
    LPWSTR filePath,
    int sampleRate,
    int bits,
    int bufferLength,
    int bufferCount
) {
    waveout_close();
    //
    if (NULL != gSysValue.cInstList) {
        delete gSysValue.cInstList;
    }
    gSysValue.cInstList = new InstList(filePath);
    gSysValue.ppSampler = gSysValue.cInstList->GetSamplerPtr();
    gSysValue.pWaveTable = (WAVDAT*)gSysValue.cInstList->GetWaveTablePtr();
    gSysValue.bufferLength = bufferLength;
    gSysValue.bufferCount = bufferCount;
    gSysValue.sampleRate = sampleRate;
    gSysValue.bits = bits;
    gSysValue.deltaTime = 1.0 / sampleRate;
    //
    effect_create(&gSysValue);
    message_createChannels(&gSysValue);
    //
    switch (gSysValue.bits) {
    case 16:
        waveOutOpen(gSysValue.sampleRate, 16, 2, gSysValue.bufferLength, gSysValue.bufferCount, write16);
        break;
    case 24:
        waveOutOpen(gSysValue.sampleRate, 24, 2, gSysValue.bufferLength, gSysValue.bufferCount, write24);
        break;
    case 32:
        waveOutOpen(gSysValue.sampleRate, 32, 2, gSysValue.bufferLength, gSysValue.bufferCount, write32);
        break;
    default:
        break;
    }
    return (LPBYTE)gSysValue.cInstList->GetInstList();
}

void WINAPI waveout_close() {
    waveOutClose();
    if (NULL != gSysValue.cInstList) {
        delete gSysValue.cInstList;
        gSysValue.cInstList = NULL;
    }
    effect_dispose(&gSysValue);
}

/******************************************************************************/
inline void runSampler() {
    int activeCount = 0;
    for (int s = 0; s < SAMPLER_COUNT; s++) {
        auto pSmpl = gSysValue.ppSampler[s];
        if (pSmpl->state < E_SAMPLER_STATE::PURGE) {
            continue;
        }
        if (sampler(&gSysValue, pSmpl)) {
            activeCount++;
        }
    }
    gActiveCount = activeCount;
}

void write16(LPSTR pData) {
    runSampler();
    for (int c = 0; c < CHANNEL_COUNT; c++) {
        auto pEffect = gSysValue.ppEffect[c];
        auto pInputBuff = pEffect->pOutput;
        auto pInputBuffTerm = pInputBuff + pEffect->pSystemValue->bufferLength;
        auto pBuff = (short*)pData;
        for (; pInputBuff < pInputBuffTerm; pInputBuff++, pBuff += 2) {
            double tempL, tempR;
            // effect
            effect(pEffect, pInputBuff, &tempL, &tempR);
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
            *pInputBuff = 0.0;
        }
    }
}

void write24(LPSTR pData) {
    runSampler();
    for (int c = 0; c < CHANNEL_COUNT; c++) {
        auto pEffect = gSysValue.ppEffect[c];
        auto pInputBuff = pEffect->pOutput;
        auto pInputBuffTerm = pInputBuff + pEffect->pSystemValue->bufferLength;
        auto pBuff = (int24*)pData;
        for (; pInputBuff < pInputBuffTerm; pInputBuff++, pBuff += 2) {
            double tempL, tempR;
            // effect
            effect(pEffect, pInputBuff, &tempL, &tempR);
            // output
            tempL += fromInt24(pBuff + 0);
            tempR += fromInt24(pBuff + 1);
            if (1.0 < tempL) tempL = 1.0;
            if (tempL < -1.0) tempL = -1.0;
            if (1.0 < tempR) tempR = 1.0;
            if (tempR < -1.0) tempR = -1.0;
            setInt24(pBuff + 0, tempL);
            setInt24(pBuff + 1, tempR);
            *pInputBuff = 0.0;
        }
    }
}

void write32(LPSTR pData) {
    runSampler();
    for (int c = 0; c < CHANNEL_COUNT; c++) {
        auto pEffect = gSysValue.ppEffect[c];
        auto pInputBuff = pEffect->pOutput;
        auto pInputBuffTerm = pInputBuff + pEffect->pSystemValue->bufferLength;
        auto pBuff = (float*)pData;
        for (; pInputBuff < pInputBuffTerm; pInputBuff++, pBuff += 2) {
            double tempL, tempR;
            // effect
            effect(pEffect, pInputBuff, &tempL, &tempR);
            // output
            tempL += *(pBuff + 0);
            tempR += *(pBuff + 1);
            if (1.0 < tempL) tempL = 1.0;
            if (tempL < -1.0) tempL = -1.0;
            if (1.0 < tempR) tempR = 1.0;
            if (tempR < -1.0) tempR = -1.0;
            *(pBuff + 0) = (float)tempL;
            *(pBuff + 1) = (float)tempR;
            *pInputBuff = 0.0;
        }
    }
}

BOOL waveOutOpen(
    int sampleRate,
    int bits,
    int channelCount,
    int bufferLength,
    int bufferCount,
    void(*fpWriteBufferProc)(LPSTR)
) {
    if (NULL != ghWaveOut) {
        if (!waveOutClose()) {
            return FALSE;
        }
    }
    if (NULL == fpWriteBufferProc) {
        return FALSE;
    }
    //
    gWriteCount = 0;
    gWriteIndex = 0;
    gReadIndex = 0;
    gBufferCount = bufferCount;
    gBufferLength = bufferLength;
    gfpWriteBuffer = fpWriteBufferProc;
    //
    gWaveFmt.wFormatTag = 32 == bits ? 3 : 1;
    gWaveFmt.nChannels = channelCount;
    gWaveFmt.wBitsPerSample = (WORD)bits;
    gWaveFmt.nSamplesPerSec = (DWORD)sampleRate;
    gWaveFmt.nBlockAlign = gWaveFmt.nChannels * gWaveFmt.wBitsPerSample / 8;
    gWaveFmt.nAvgBytesPerSec = gWaveFmt.nSamplesPerSec * gWaveFmt.nBlockAlign;
    //
    if (MMSYSERR_NOERROR != waveOutOpen(
        &ghWaveOut,
        WAVE_MAPPER,
        &gWaveFmt,
        (DWORD_PTR)waveOutProc,
        (DWORD_PTR)gppWaveHdr,
        CALLBACK_FUNCTION
    )) {
        return FALSE;
    }
    //
    gDoStop = FALSE;
    InitializeCriticalSection((LPCRITICAL_SECTION)&gcsBufferLock);
    //
    gppWaveHdr = (PWAVEHDR*)malloc(sizeof(PWAVEHDR) * gBufferCount);
    for (int n = 0; n < gBufferCount; ++n) {
        gppWaveHdr[n] = (PWAVEHDR)malloc(sizeof(WAVEHDR));
        gppWaveHdr[n]->dwBufferLength = bufferLength * gWaveFmt.nBlockAlign;
        gppWaveHdr[n]->dwFlags = WHDR_BEGINLOOP | WHDR_ENDLOOP;
        gppWaveHdr[n]->dwLoops = 0;
        gppWaveHdr[n]->dwUser = 0;
        gppWaveHdr[n]->lpData = (LPSTR)malloc(bufferLength * gWaveFmt.nBlockAlign);
        memset(gppWaveHdr[n]->lpData, 0, bufferLength * gWaveFmt.nBlockAlign);
        waveOutPrepareHeader(ghWaveOut, gppWaveHdr[n], sizeof(WAVEHDR));
        waveOutWrite(ghWaveOut, gppWaveHdr[n], sizeof(WAVEHDR));
    }
    //
    auto hThread = CreateThread(NULL, 0, (LPTHREAD_START_ROUTINE)writeBufferTask, NULL, 0, &gThreadId);
    SetThreadPriority(hThread, THREAD_PRIORITY_HIGHEST);
    return TRUE;
}

BOOL waveOutClose() {
    if (NULL == ghWaveOut) {
        return TRUE;
    }
    //
    gDoStop = TRUE;
    long elapsedTime = 0;
    while (!gWaveOutStopped || !gThreadStopped) {
        Sleep(100);
        elapsedTime++;
        if (10 < elapsedTime) {
            return FALSE;
        }
    }
    //
    for (int n = 0; n < gBufferCount; ++n) {
        waveOutUnprepareHeader(ghWaveOut, gppWaveHdr[n], sizeof(WAVEHDR));
    }
    waveOutReset(ghWaveOut);
    waveOutClose(ghWaveOut);
    return TRUE;
}

void CALLBACK waveOutProc(HWAVEOUT hwo, UINT uMsg, DWORD_PTR dwInstance, DWORD dwParam1, DWORD dwParam) {
    switch (uMsg) {
    case MM_WOM_OPEN:
        break;
    case MM_WOM_CLOSE:
        gDoStop = TRUE;
        while (!gWaveOutStopped || !gThreadStopped) {
            Sleep(100);
        }
        gDoStop = FALSE;
        for (int b = 0; b < gBufferCount; ++b) {
            free(gppWaveHdr[b]->lpData);
            gppWaveHdr[b]->lpData = NULL;
        }
        break;
    case MM_WOM_DONE:
        if (gDoStop) {
            gWaveOutStopped = TRUE;
            break;
        }
        gWaveOutStopped = FALSE;
        EnterCriticalSection((LPCRITICAL_SECTION)&gcsBufferLock);
        if (gWriteCount < gBufferCount / 4) {
            waveOutWrite(ghWaveOut, gppWaveHdr[gReadIndex], sizeof(WAVEHDR));
            LeaveCriticalSection((LPCRITICAL_SECTION)&gcsBufferLock);
            return;
        }
        {
            waveOutWrite(ghWaveOut, gppWaveHdr[gReadIndex], sizeof(WAVEHDR));
            gReadIndex = (gReadIndex + 1) % gBufferCount;
            gWriteCount--;
        }
        LeaveCriticalSection((LPCRITICAL_SECTION)&gcsBufferLock);
        break;
    default:
        break;
    }
}

DWORD writeBufferTask(LPVOID *param) {
    while (TRUE) {
        if (gDoStop) {
            gThreadStopped = TRUE;
            break;
        }
        gThreadStopped = FALSE;
        if (NULL == gppWaveHdr || NULL == gppWaveHdr[0] || NULL == gppWaveHdr[0]->lpData) {
            Sleep(100);
            continue;
        }
        EnterCriticalSection((LPCRITICAL_SECTION)&gcsBufferLock);
        if (gBufferCount <= gWriteCount + 1) {
            LeaveCriticalSection((LPCRITICAL_SECTION)&gcsBufferLock);
            continue;
        }
        {
            LPSTR pBuff = gppWaveHdr[gWriteIndex]->lpData;
            memset(pBuff, 0, gWaveFmt.nBlockAlign * gBufferLength);
            gfpWriteBuffer(pBuff);
            gWriteIndex = (gWriteIndex + 1) % gBufferCount;
            gWriteCount++;
        }
        LeaveCriticalSection((LPCRITICAL_SECTION)&gcsBufferLock);
    }
    return 0;
}
