#include "main.h"
#include "sampler.h"
#include <stdio.h>
#include <math.h>
#include <mmsystem.h>

#pragma comment (lib, "winmm.lib")

/******************************************************************************/
DWORD            gThreadId;
CRITICAL_SECTION csBufferInfo;

volatile Bool   gDoStop = true;
volatile Bool   gIsStopped = true;
volatile int    gWriteCount = 0;
volatile int    gWriteIndex = -1;
volatile int    gReadIndex = -1;
int             gActiveCount = 0;

HWAVEOUT        ghWaveOut = NULL;
WAVEFORMATEX    gWaveFmt = { 0 };
WAVEHDR         **gppWaveHdr = NULL;

LPBYTE          gpWaveTable = NULL;
SYSTEM_VALUE    gSysValue = { 0 };
CHANNEL_VALUE   **gppChValues = NULL;
CHANNEL         **gppChParams = NULL;
SAMPLER         **gppSamplers = NULL;

/******************************************************************************/
void CALLBACK waveOutProc(HWAVEOUT hwo, uint uMsg);
DWORD writeWaveOutBuffer(LPVOID *param);

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
    //
    gDoStop = true;
    while (!gIsStopped) {
        Sleep(100);
    }
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
    //
    gDoStop = false;
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
    waveout_Dispose();
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

Bool waveout_Open() {
    if (NULL != ghWaveOut) {
        waveout_Close();
    }
    //
    gWaveFmt.wFormatTag = 32 == gSysValue.bits ? 3 : 1;
    gWaveFmt.nChannels = 2;
    gWaveFmt.wBitsPerSample = (WORD)gSysValue.bits;
    gWaveFmt.nSamplesPerSec = (DWORD)gSysValue.sampleRate;
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
        return false;
    }
    //
    gppWaveHdr = (PWAVEHDR*)malloc(sizeof(PWAVEHDR) * gSysValue.bufferCount);
    for (int n = 0; n < gSysValue.bufferCount; ++n) {
        gppWaveHdr[n] = (PWAVEHDR)malloc(sizeof(WAVEHDR));
        gppWaveHdr[n]->dwBufferLength = gSysValue.bufferLength * gWaveFmt.nBlockAlign;
        gppWaveHdr[n]->dwFlags = WHDR_BEGINLOOP | WHDR_ENDLOOP;
        gppWaveHdr[n]->dwLoops = 0;
        gppWaveHdr[n]->dwUser = 0;
        gppWaveHdr[n]->lpData = (LPSTR)malloc(gSysValue.bufferLength * gWaveFmt.nBlockAlign);
        memset(gppWaveHdr[n]->lpData, 0, gSysValue.bufferLength * gWaveFmt.nBlockAlign);
        waveOutPrepareHeader(ghWaveOut, gppWaveHdr[n], sizeof(WAVEHDR));
        waveOutWrite(ghWaveOut, gppWaveHdr[n], sizeof(WAVEHDR));
    }
    //
    gDoStop = false;
    InitializeCriticalSection((LPCRITICAL_SECTION)&csBufferInfo);
    CreateThread(NULL, 0, writeWaveOutBuffer, NULL, 0, &gThreadId);
    return true;
}

void waveout_Close() {
    if (NULL == ghWaveOut) {
        return;
    }
    //
    gDoStop = true;
    while (!gIsStopped) {
        Sleep(100);
    }
    //
    for (int n = 0; n < gSysValue.bufferCount; ++n) {
        waveOutUnprepareHeader(ghWaveOut, gppWaveHdr[n], sizeof(WAVEHDR));
    }
    waveOutReset(ghWaveOut);
    waveOutClose(ghWaveOut);
    ghWaveOut = NULL;
}

void waveout_Dispose() {
    if (NULL != ghWaveOut) {
        waveout_Close();
    }
    disposeChannels(gppChValues);
    disposeSamplers(gppSamplers, gSysValue.samplerCount);
}

/******************************************************************************/
void CALLBACK waveOutProc(HWAVEOUT hwo, uint uMsg) {
    switch (uMsg) {
    case MM_WOM_OPEN:
        break;
    case MM_WOM_CLOSE:
        gDoStop = true;
        while (!gIsStopped) {
            Sleep(100);
        }
        gDoStop = false;
        for (int b = 0; b < gSysValue.bufferCount; ++b) {
            free(gppWaveHdr[b]->lpData);
            gppWaveHdr[b]->lpData = NULL;
        }
        break;
    case MM_WOM_DONE:
        //
        if (gDoStop) {
            gIsStopped = true;
            break;
        }
        gIsStopped = false;
        //
        EnterCriticalSection((LPCRITICAL_SECTION)&csBufferInfo);
        if (gWriteCount < 1) {
            waveOutWrite(ghWaveOut, gppWaveHdr[gReadIndex], sizeof(WAVEHDR));
            return;
        }
        gReadIndex = (gReadIndex + 1) % gSysValue.bufferCount;
        waveOutWrite(ghWaveOut, gppWaveHdr[gReadIndex], sizeof(WAVEHDR));
        gWriteCount--;
        LeaveCriticalSection((LPCRITICAL_SECTION)&csBufferInfo);
        break;
    default:
        break;
    }
}

DWORD writeWaveOutBuffer(LPVOID *param) {
    while (true) {
        if (NULL == gppWaveHdr[0]->lpData) {
            continue;
        }
        EnterCriticalSection((LPCRITICAL_SECTION)&csBufferInfo);
        if (gSysValue.bufferCount <= gWriteCount ||
            (gWriteIndex + 1) % gSysValue.bufferCount == gReadIndex) {
            LeaveCriticalSection((LPCRITICAL_SECTION)&csBufferInfo);
            continue;
        }
        gWriteIndex = (gWriteIndex + 1) % gSysValue.bufferCount;
        //
        LPBYTE outBuff = gppWaveHdr[gWriteIndex]->lpData;
        memset(outBuff, 0, gppWaveHdr[gWriteIndex]->dwBufferLength);
        //
        gActiveCount = 0;
        for (int s = 0; s < gSysValue.samplerCount; ++s) {
            if (E_KEY_STATE_STANDBY == gppSamplers[s]->state) {
                continue;
            }
            sampler(gppChValues, gppSamplers[s], gpWaveTable);
            gActiveCount++;
        }
        //
        switch (gSysValue.bits) {
        case 16:
            for (int c = 0; c < gSysValue.channelCount; ++c) {
                if (E_CH_STATE_STANDBY == gppChValues[c]->state) {
                    continue;
                }
                channel16(gppChValues[c], (short*)outBuff);
            }
            break;
        case 24:
            for (int c = 0; c < gSysValue.channelCount; ++c) {
                if (E_CH_STATE_STANDBY == gppChValues[c]->state) {
                    continue;
                }
                channel24(gppChValues[c], (int24*)outBuff);
            }
            break;
        case 32:
            for (int c = 0; c < gSysValue.channelCount; ++c) {
                if (E_CH_STATE_STANDBY == gppChValues[c]->state) {
                    continue;
                }
                channel32(gppChValues[c], (float*)outBuff);
            }
            break;
        }
        //
        gWriteCount++;
        LeaveCriticalSection((LPCRITICAL_SECTION)&csBufferInfo);
    }
    return 0;
}
