#include "main.h"
#include "sampler.h"
#include "channel.h"
#include <stdio.h>
#include <math.h>
#include <mmsystem.h>

#pragma comment (lib, "winmm.lib")

/******************************************************************************/
#define BUFFER_COUNT   32
#define SAMPLER_COUNT 128
#define CHANNEL_COUNT  16

/******************************************************************************/
#pragma pack(push, 4)
typedef struct {
    UInt32 riff;
    UInt32 fileSize;
    UInt32 dataId;
} RIFF;
#pragma pack(pop)

#pragma pack(push, 4)
typedef struct {
    UInt32 chunkId;
    UInt32 chunkSize;
    UInt16 formatId;
    UInt16 channels;
    UInt32 sampleRate;
    UInt32 bytePerSec;
    UInt16 blockAlign;
    UInt16 bitPerSample;
    UInt32 dataId;
    UInt32 dataSize;
} FMT_;
#pragma pack(pop)

/******************************************************************************/
DWORD            gThreadId;
CRITICAL_SECTION csBufferInfo;

volatile bool   gDoStop = true;
volatile bool   gIsStopped = true;
volatile SInt32 gWriteCount = 0;
volatile SInt32 gWriteIndex = -1;
volatile SInt32 gReadIndex = -1;

SYSTEM_VALUE    gSystemValue;
SInt32          gActiveCount = 0;

HWAVEOUT        ghWaveOut = NULL;
WAVEFORMATEX    gWaveFmt = { 0 };
WAVEHDR         gWaveHdr[BUFFER_COUNT] = { NULL };

LPBYTE          gpFileData = NULL;

CHANNEL         **gppWaveOutChValues = NULL;
CHANNEL_PARAM   **gppWaveOutChParams = NULL;
CHANNEL         **gppFileOutChValues = NULL;
CHANNEL_PARAM   **gppFileOutChParams = NULL;
SAMPLER         **gppWaveOutSamplers = NULL;
SAMPLER         **gppFileOutSamplers = NULL;

float           *gpFileOutBuffer = NULL;
FILE            *gfpFileOut = NULL;
RIFF            gRiff;
FMT_            gFmt;

/******************************************************************************/
void CALLBACK waveOutProc(HWAVEOUT hwo, UInt32 uMsg);
DWORD WINAPI writeWaveOutBuffer(LPVOID *param);

/******************************************************************************/
VOID WINAPI SystemValues(UInt32 sampleRate, UInt32 waveBufferLength) {
    if (NULL != ghWaveOut) {
        WaveOutClose();
    }
    gSystemValue.sampleRate = sampleRate;
    gSystemValue.bufferLength = waveBufferLength;
    gSystemValue.deltaTime = 1.0 / sampleRate;
}

BOOL WINAPI WaveOutOpen() {
    if (NULL != ghWaveOut) {
        WaveOutClose();
    }
    //
    gWaveFmt.wFormatTag = 3;
    gWaveFmt.nChannels = 2;
    gWaveFmt.wBitsPerSample = 32;
    gWaveFmt.nSamplesPerSec = gSystemValue.sampleRate;
    gWaveFmt.nBlockAlign = gWaveFmt.nChannels * gWaveFmt.wBitsPerSample / 8;
    gWaveFmt.nAvgBytesPerSec = gWaveFmt.nSamplesPerSec * gWaveFmt.nBlockAlign;
    //
    if (MMSYSERR_NOERROR != waveOutOpen(
        &ghWaveOut,
        WAVE_MAPPER,
        &gWaveFmt,
        (DWORD_PTR)waveOutProc,
        (DWORD_PTR)gWaveHdr,
        CALLBACK_FUNCTION
    )) {
        return false;
    }
    //
    for (UInt32 n = 0; n < BUFFER_COUNT; ++n) {
        gWaveHdr[n].dwBufferLength = gSystemValue.bufferLength * gWaveFmt.nBlockAlign;
        gWaveHdr[n].dwFlags = WHDR_BEGINLOOP | WHDR_ENDLOOP;
        gWaveHdr[n].dwLoops = 0;
        gWaveHdr[n].dwUser = 0;
        if (NULL == gWaveHdr[n].lpData) {
            gWaveHdr[n].lpData = (LPSTR)malloc(gSystemValue.bufferLength * gWaveFmt.nBlockAlign);
            if (NULL != gWaveHdr[n].lpData) {
                memset(gWaveHdr[n].lpData, 0, gSystemValue.bufferLength * gWaveFmt.nBlockAlign);
                waveOutPrepareHeader(ghWaveOut, &gWaveHdr[n], sizeof(WAVEHDR));
                waveOutWrite(ghWaveOut, &gWaveHdr[n], sizeof(WAVEHDR));
            }
        }
    }
    //
    gDoStop = false;
    InitializeCriticalSection((LPCRITICAL_SECTION)&csBufferInfo);
    CreateThread(NULL, 0, writeWaveOutBuffer, NULL, 0, &gThreadId);
    return true;
}

VOID WINAPI WaveOutClose() {
    if (NULL == ghWaveOut) {
        return;
    }
    //
    gDoStop = true;
    while (!gIsStopped) {
        Sleep(100);
    }
    //
    for (UInt32 n = 0; n < BUFFER_COUNT; ++n) {
        waveOutUnprepareHeader(ghWaveOut, &gWaveHdr[n], sizeof(WAVEHDR));
    }
    waveOutReset(ghWaveOut);
    waveOutClose(ghWaveOut);
    ghWaveOut = NULL;
}

VOID WINAPI FileOutOpen(LPWSTR filePath) {
    if (NULL != gpFileOutBuffer) {
        free(gpFileOutBuffer);
    }

    gpFileOutBuffer = (float*)malloc(sizeof(float) * gSystemValue.bufferLength * 2);

    if (NULL != gfpFileOut) {
        fclose(gfpFileOut);
        gfpFileOut = NULL;
    }

    _wfopen_s(&gfpFileOut, filePath, L"wb");

    //
    gRiff.riff     = 0x46464952;
    gRiff.fileSize = 0;
    gRiff.dataId   = 0x45564157;
    //
    gFmt.chunkId      = 0x20746D66;
    gFmt.chunkSize    = 16;
    gFmt.formatId     = 3;
    gFmt.channels     = 2;
    gFmt.sampleRate   = gppFileOutChValues[0]->pSystemValue->sampleRate;
    gFmt.bitPerSample = 32;
    gFmt.blockAlign   = gFmt.channels * gFmt.bitPerSample >> 3;
    gFmt.bytePerSec   = gFmt.sampleRate * gFmt.blockAlign;
    gFmt.dataId       = 0x61746164;
    gFmt.dataSize     = 0;
    //
    fwrite(&gRiff, sizeof(gRiff), 1, gfpFileOut);
    fwrite(&gFmt, sizeof(gFmt), 1, gfpFileOut);
}

VOID WINAPI FileOutClose() {
    if (NULL == gfpFileOut) {
        return;
    }
    //
    gRiff.fileSize = gFmt.dataSize + sizeof(gFmt) + 4;
    //
    fseek(gfpFileOut, 0, SEEK_SET);
    fwrite(&gRiff, sizeof(gRiff), 1, gfpFileOut);
    fwrite(&gFmt, sizeof(gFmt), 1, gfpFileOut);
    //
    fclose(gfpFileOut);
    gfpFileOut = NULL;
}

VOID WINAPI FileOut() {
    memset(gpFileOutBuffer, 0, sizeof(float) * gSystemValue.bufferLength * 2);
    #pragma loop(hint_parallel(SAMPLER_COUNT))
    for (SInt32 s = 0; s < SAMPLER_COUNT; ++s) {
        if (E_KEY_STATE_STANDBY == gppFileOutSamplers[s]->state) {
            continue;
        }
        sampler(gppFileOutChValues, gppFileOutSamplers[s], gpFileData);
    }
    #pragma loop(hint_parallel(CHANNEL_COUNT))
    for (SInt32 c = 0; c < CHANNEL_COUNT; ++c) {
        channel(gppFileOutChValues[c], gpFileOutBuffer);
    }
    fwrite(gpFileOutBuffer, sizeof(float) * gSystemValue.bufferLength * 2, 1, gfpFileOut);
    gFmt.dataSize += sizeof(float) * gSystemValue.bufferLength * 2;
}

SInt32* WINAPI GetActiveCountPtr() {
    return &gActiveCount;
}

CHANNEL_PARAM** WINAPI GetWaveOutChannelPtr() {
    if (NULL == gppWaveOutChValues) {
        gppWaveOutChValues = createChannels(CHANNEL_COUNT, &gSystemValue);
        gppWaveOutChParams = (CHANNEL_PARAM**)malloc(sizeof(CHANNEL_PARAM*) * CHANNEL_COUNT);
        for (int i = 0; i < CHANNEL_COUNT; ++i) {
            gppWaveOutChParams[i] = gppWaveOutChValues[i]->pParam;
        }
    }
    return gppWaveOutChParams;
}

CHANNEL_PARAM** WINAPI GetFileOutChannelPtr() {
    if (NULL == gppFileOutChValues) {
        gppFileOutChValues = createChannels(CHANNEL_COUNT, &gSystemValue);
        gppFileOutChParams = (CHANNEL_PARAM**)malloc(sizeof(CHANNEL_PARAM*) * CHANNEL_COUNT);
        for (int i = 0; i < CHANNEL_COUNT; ++i) {
            gppFileOutChParams[i] = gppFileOutChValues[i]->pParam;
        }
    }
    return gppFileOutChParams;
}

SAMPLER** WINAPI GetWaveOutSamplerPtr() {
    if (NULL == gppWaveOutSamplers) {
        gppWaveOutSamplers = createSamplers(SAMPLER_COUNT);
    }
    return gppWaveOutSamplers;
}

SAMPLER** WINAPI GetFileOutSamplerPtr() {
    if (NULL == gppFileOutSamplers) {
        gppFileOutSamplers = createSamplers(SAMPLER_COUNT);
    }
    return gppFileOutSamplers;
}

LPBYTE WINAPI LoadFile(LPWSTR filePath, UInt32 *size) {
    if (NULL == size) {
        return NULL;
    }
    //
    gDoStop = true;
    while (!gIsStopped) {
        Sleep(100);
    }
    //
    if (NULL != gpFileData) {
        free(gpFileData);
        gpFileData = NULL;
    }
    //
    FILE *fpDLS = NULL;
    _wfopen_s(&fpDLS, filePath, TEXT("rb"));
    if (NULL != fpDLS) {
        fseek(fpDLS, 4, SEEK_SET);
        fread_s(size, sizeof(*size), sizeof(*size), 1, fpDLS);
        *size -= 8;
        gpFileData = (LPBYTE)malloc(*size);
        if (NULL != gpFileData) {
            fseek(fpDLS, 12, SEEK_SET);
            fread_s(gpFileData, *size, *size, 1, fpDLS);
        }
        fclose(fpDLS);
    }
    //
    gDoStop = false;
    return gpFileData;
}

/******************************************************************************/
void CALLBACK waveOutProc(HWAVEOUT hwo, UInt32 uMsg) {
    switch (uMsg) {
    case MM_WOM_OPEN:
        break;
    case MM_WOM_CLOSE:
        gDoStop = true;
        while (!gIsStopped) {
            Sleep(100);
        }
        gDoStop = false;
        for (SInt32 b = 0; b < BUFFER_COUNT; ++b) {
            free(gWaveHdr[b].lpData);
            gWaveHdr[b].lpData = NULL;
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
            waveOutWrite(ghWaveOut, &gWaveHdr[gReadIndex], sizeof(WAVEHDR));
            return;
        }
        gReadIndex = (gReadIndex + 1) % BUFFER_COUNT;
        waveOutWrite(ghWaveOut, &gWaveHdr[gReadIndex], sizeof(WAVEHDR));
        gWriteCount--;
        LeaveCriticalSection((LPCRITICAL_SECTION)&csBufferInfo);
        break;
    default:
        break;
    }
}

DWORD WINAPI writeWaveOutBuffer(LPVOID *param) {
    while (true) {
        if (NULL == gWaveHdr[0].lpData) {
            continue;
        }
        EnterCriticalSection((LPCRITICAL_SECTION)&csBufferInfo);
        if (BUFFER_COUNT <= gWriteCount || (gWriteIndex + 1) % BUFFER_COUNT == gReadIndex) {
            LeaveCriticalSection((LPCRITICAL_SECTION)&csBufferInfo);
            continue;
        }
        gWriteIndex = (gWriteIndex + 1) % BUFFER_COUNT;
        gActiveCount = 0;
        //
        float* outBuff = (float*)gWaveHdr[gWriteIndex].lpData;
        memset(outBuff, 0, sizeof(float) * gSystemValue.bufferLength * 2);

        #pragma loop(hint_parallel(SAMPLER_COUNT))
        for (SInt32 s = 0; s < SAMPLER_COUNT; ++s) {
            if (E_KEY_STATE_STANDBY == gppWaveOutSamplers[s]->state) {
                continue;
            }
            sampler(gppWaveOutChValues, gppWaveOutSamplers[s], gpFileData);
            if (E_KEY_STATE_PURGE != gppWaveOutSamplers[s]->state) {
                gActiveCount++;
            }
        }
        #pragma loop(hint_parallel(CHANNEL_COUNT))
        for (SInt32 c = 0; c < CHANNEL_COUNT; ++c) {
            channel(gppWaveOutChValues[c], outBuff);
        }
        gWriteCount++;
        LeaveCriticalSection((LPCRITICAL_SECTION)&csBufferInfo);
    }
    return 0;
}
