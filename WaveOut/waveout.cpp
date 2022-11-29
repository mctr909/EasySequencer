#include "waveout.h"
#include "message_reciever.h"
#include "inst/inst_list.h"
#include "synth/channel.h"
#include "synth/channel_const.h"
#include "synth/channel_params.h"
#include "synth/sampler.h"
#include "type.h"

#include <math.h>
#include <stdio.h>
#include <mmsystem.h>

#pragma comment (lib, "winmm.lib")

/******************************************************************************/
DWORD            gThreadId;
CRITICAL_SECTION gcsBufferLock;

BOOL   gDoStop = TRUE;
BOOL   gWaveOutStopped = TRUE;
BOOL   gThreadStopped = TRUE;

int32  gWriteCount = 0;
int32  gWriteIndex = 0;
int32  gReadIndex = 0;
int32  gBufferCount = 0;
int32  gBufferLength = 0;

HWAVEOUT        ghWaveOut = NULL;
WAVEFORMATEX    gWaveFmt = { 0 };
WAVEHDR         **gppWaveHdr = NULL;

void (*gfpWriteBuffer)(LPSTR) = NULL;

/******************************************************************************/
int32         gActiveCount = 0;
SYSTEM_VALUE  gSysValue = { 0 };

/******************************************************************************/
void write_buffer(LPSTR pData);

BOOL waveOutOpen(
    int32 sampleRate,
    int32 bufferLength,
    int32 bufferCount,
    void(*fpWriteBufferProc)(LPSTR)
);
BOOL waveOutClose();
void CALLBACK waveOutProc(HWAVEOUT hwo, UINT uMsg, DWORD_PTR dwInstance, DWORD dwParam1, DWORD dwParam);
DWORD writeBufferTask(LPVOID *param);

/******************************************************************************/
int* WINAPI waveout_getActiveSamplersPtr() {
    return &gActiveCount;
}

CHANNEL_PARAM** WINAPI waveout_getChannelParamPtr() {
    return gSysValue.ppChannel_params;
}

byte *WINAPI waveout_open(
    LPWSTR filePath,
    int32 sampleRate,
    int32 bufferLength,
    int32 bufferCount
) {
    waveout_close();
    //
    if (NULL != gSysValue.cInst_list) {
        delete gSysValue.cInst_list;
    }
    //
    auto cInst = new InstList();
    auto loadStatus = cInst->Load(filePath);
    if (E_LOAD_STATUS::SUCCESS != loadStatus) {
        delete cInst;
    }
    auto captionErr = L"ウェーブテーブル読み込み失敗";
    switch (loadStatus) {
    case E_LOAD_STATUS::WAVE_TABLE_OPEN_FAILED:
        MessageBoxW(NULL, L"ファイルが開けませんでした。", captionErr, 0);
        return NULL;
    case E_LOAD_STATUS::WAVE_TABLE_ALLOCATE_FAILED:
        MessageBoxW(NULL, L"メモリの確保ができませんでした。", captionErr, 0);
        return NULL;
    case E_LOAD_STATUS::WAVE_TABLE_UNKNOWN_FILE:
        MessageBoxW(NULL, L"対応していない形式です。", captionErr, 0);
        return NULL;
    default:
        break;
    }
    //
    gSysValue.cInst_list = cInst;
    gSysValue.ppSampler = gSysValue.cInst_list->GetSamplerPtr();
    gSysValue.pWave_table = (WAVDAT*)gSysValue.cInst_list->GetWaveTablePtr();
    gSysValue.buffer_length = bufferLength;
    gSysValue.buffer_count = bufferCount;
    gSysValue.sample_rate = sampleRate;
    gSysValue.delta_time = 1.0 / sampleRate;
    gSysValue.pBuffer_l = (double*)calloc(bufferLength, sizeof(double));
    gSysValue.pBuffer_r = (double*)calloc(bufferLength, sizeof(double));
    //
    message_createChannels(&gSysValue);
    //
    waveOutOpen(gSysValue.sample_rate, gSysValue.buffer_length, gSysValue.buffer_count, write_buffer);
    //
    return (byte*)gSysValue.cInst_list->GetInstList();
}

void WINAPI waveout_close() {
    waveOutClose();
    if (NULL != gSysValue.cInst_list) {
        delete gSysValue.cInst_list;
        gSysValue.cInst_list = NULL;
    }
    if (NULL != gSysValue.pBuffer_l) {
        free(gSysValue.pBuffer_l);
        gSysValue.pBuffer_l = NULL;
    }
    if (NULL != gSysValue.pBuffer_r) {
        free(gSysValue.pBuffer_r);
        gSysValue.pBuffer_r = NULL;
    }
    message_disposeChannels(&gSysValue);
}

/******************************************************************************/
void write_buffer(LPSTR pData) {
    /* sampler loop */
    int32 activeCount = 0;
    for (int32 i = 0; i < SAMPLER_COUNT; i++) {
        auto pSmpl = gSysValue.ppSampler[i];
        if (pSmpl->state < E_SAMPLER_STATE::PURGE) {
            continue;
        }
        if (sampler(&gSysValue, pSmpl)) {
            activeCount++;
        }
    }
    gActiveCount = activeCount;
    /* channel loop */
    for (int32 i = 0; i < CHANNEL_COUNT; i++) {
        auto pCh = gSysValue.ppChannels[i];
        pCh->step(gSysValue.pBuffer_l, gSysValue.pBuffer_r);
    }
    /* write buffer */
    auto pOutput = (short*)pData;
    for (int32 i = 0, j = 0; i < gSysValue.buffer_length; i++, j += 2) {
        auto pL = &gSysValue.pBuffer_l[i];
        auto pR = &gSysValue.pBuffer_r[i];
        if (*pL < -1.0) {
            *pL = -1.0;
        }
        if (1.0 < *pL) {
            *pL = 1.0;
        }
        if (*pR < -1.0) {
            *pR = -1.0;
        }
        if (1.0 < *pR) {
            *pR = 1.0;
        }
        pOutput[j] = (short)(*pL * 32767);
        pOutput[j + 1] = (short)(*pR * 32767);
        *pL = 0.0;
        *pR = 0.0;
    }
}

BOOL waveOutOpen(
    int32 sampleRate,
    int32 bufferLength,
    int32 bufferCount,
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
    gWaveFmt.wFormatTag = 1;
    gWaveFmt.nChannels = 2;
    gWaveFmt.wBitsPerSample = (WORD)16;
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
    gppWaveHdr = (PWAVEHDR*)calloc(gBufferCount, sizeof(PWAVEHDR));
    for (int32 n = 0; n < gBufferCount; ++n) {
        gppWaveHdr[n] = (PWAVEHDR)calloc(1, sizeof(WAVEHDR));
        gppWaveHdr[n]->dwBufferLength = (DWORD)bufferLength * gWaveFmt.nBlockAlign;
        gppWaveHdr[n]->dwFlags = WHDR_BEGINLOOP | WHDR_ENDLOOP;
        gppWaveHdr[n]->dwLoops = 0;
        gppWaveHdr[n]->dwUser = 0;
        gppWaveHdr[n]->lpData = (LPSTR)calloc(bufferLength, gWaveFmt.nBlockAlign);
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
    for (int32 n = 0; n < gBufferCount; ++n) {
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
        for (int32 b = 0; b < gBufferCount; ++b) {
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
