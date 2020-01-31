#include "main.h"
#include "sampler.h"
#include "channel.h"
#include <stdio.h>
#include <math.h>
#include <mmsystem.h>

#pragma comment (lib, "winmm.lib")

/******************************************************************************/
#define BUFFER_COUNT        128
#define CHANNEL_COUNT       16

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
bool            gDoStop = false;
bool            gIsStopped = true;

HWAVEOUT        ghWaveOut = NULL;
WAVEFORMATEX    gWaveFmt = { 0 };
WAVEHDR         gWaveHdr[BUFFER_COUNT] = { NULL };

LPBYTE          gpDlsBuffer = NULL;

SInt32          gWaveOutSamplers = 256;
SInt32          gFileOutSamplers = 256;
SInt32          gWaveBufferLength = 0;
SInt32          gFileBufferLength = 0;

SInt32          gWriteWaveBufferCount = 0;
SInt32          gWriteWaveBufferIndex = -1;
SInt32          gReadWaveBufferIndex = -1;

CHANNEL         **gppWaveOutChValues = NULL;
CHANNEL_PARAM   **gppWaveOutChParams = NULL;
CHANNEL         **gppFileOutChValues = NULL;
CHANNEL_PARAM   **gppFileOutChParams = NULL;
SAMPLER         **gppWaveOutSamplers = NULL;
SAMPLER         **gppFileOutSamplers = NULL;

SInt16          *gpFileOutBuffer = NULL;
FILE            *gfpFileOut = NULL;
RIFF            gRiff;
FMT_            gFmt;

/******************************************************************************/
void CALLBACK waveOutProc(HWAVEOUT hwo, UInt32 uMsg);

/******************************************************************************/
BOOL WINAPI WaveOutOpen(UInt32 sampleRate, UInt32 waveBufferLength) {
    if (NULL != ghWaveOut) {
        WaveOutClose();
    }

    //
    gWaveBufferLength = waveBufferLength;

    //
    gWaveFmt.wFormatTag = WAVE_FORMAT_PCM;
    gWaveFmt.nChannels = 2;
    gWaveFmt.wBitsPerSample = 16;
    gWaveFmt.nSamplesPerSec = sampleRate;
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

    gDoStop = false;

    //
    for (UInt32 n = 0; n < BUFFER_COUNT; ++n) {
        gWaveHdr[n].dwBufferLength = gWaveBufferLength * gWaveFmt.nBlockAlign;
        gWaveHdr[n].dwFlags = WHDR_BEGINLOOP | WHDR_ENDLOOP;
        gWaveHdr[n].dwLoops = 0;
        gWaveHdr[n].dwUser = 0;
        if (NULL == gWaveHdr[n].lpData) {
            gWaveHdr[n].lpData = (LPSTR)malloc(gWaveBufferLength * gWaveFmt.nBlockAlign);
            if (NULL != gWaveHdr[n].lpData) {
                memset(gWaveHdr[n].lpData, 0, gWaveBufferLength * gWaveFmt.nBlockAlign);
                waveOutPrepareHeader(ghWaveOut, &gWaveHdr[n], sizeof(WAVEHDR));
                waveOutWrite(ghWaveOut, &gWaveHdr[n], sizeof(WAVEHDR));
            }
        }
    }

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

VOID WINAPI WriteWaveOutBuffer() {
    if (BUFFER_COUNT <= gWriteWaveBufferCount || (gWriteWaveBufferIndex + 1) % BUFFER_COUNT == gReadWaveBufferIndex) {
        return;
    }
    gWriteWaveBufferCount++;
    gWriteWaveBufferIndex = (gWriteWaveBufferIndex + 1) % BUFFER_COUNT;
    //
    SInt16* outBuff = (SInt16*)gWaveHdr[gWriteWaveBufferIndex].lpData;
    memset(outBuff, 0, sizeof(SInt16)*gWaveBufferLength * 2);
    for (SInt32 s = 0; s < gWaveOutSamplers; ++s) {
        sampler(gppWaveOutChValues, gppWaveOutSamplers[s], gpDlsBuffer);
    }
    for (SInt32 c = 0; c < CHANNEL_COUNT; ++c) {
        channel(gppWaveOutChValues[c], outBuff);
    }
}

VOID WINAPI FileOutOpen(LPWSTR filePath, UInt32 bufferLength) {
    if (NULL != gpFileOutBuffer) {
        free(gpFileOutBuffer);
    }

    gFileBufferLength = bufferLength;
    gpFileOutBuffer = (SInt16*)malloc(sizeof(SInt16) * 2 * bufferLength);

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
    gFmt.formatId     = 1;
    gFmt.channels     = 2;
    gFmt.sampleRate   = gppFileOutChValues[0]->sampleRate;
    gFmt.bitPerSample = 16;
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
    memset(gpFileOutBuffer, 0, sizeof(SInt16) * 2 * gFileBufferLength);
    for (SInt32 s = 0; s < gFileOutSamplers; ++s) {
        sampler(gppFileOutChValues, gppFileOutSamplers[s], gpDlsBuffer);
    }
    for (SInt32 c = 0; c < CHANNEL_COUNT; ++c) {
        channel(gppFileOutChValues[c], gpFileOutBuffer);
    }
    fwrite(gpFileOutBuffer, sizeof(SInt16) * 2 * gFileBufferLength, 1, gfpFileOut);
    gFmt.dataSize += sizeof(SInt16) * 2 * gFileBufferLength;
}

CHANNEL_PARAM** WINAPI GetWaveOutChannelPtr(UInt32 sampleRate) {
    if (NULL == gppWaveOutChValues) {
        gWaveBufferLength = 256;
        gppWaveOutChValues = (CHANNEL**)malloc(sizeof(CHANNEL*) * CHANNEL_COUNT);
        gppWaveOutChParams = (CHANNEL_PARAM**)malloc(sizeof(CHANNEL_PARAM*) * CHANNEL_COUNT);
        for (int i = 0; i < CHANNEL_COUNT; ++i) {
            gppWaveOutChValues[i] = createChannel(sampleRate, gWaveBufferLength);
            gppWaveOutChParams[i] = gppWaveOutChValues[i]->pParam;
        }
    }
    return gppWaveOutChParams;
}

CHANNEL_PARAM** WINAPI GetFileOutChannelPtr(UInt32 sampleRate) {
    if (NULL == gppFileOutChValues) {
        gFileBufferLength = 256;
        gppFileOutChValues = (CHANNEL**)malloc(sizeof(CHANNEL*) * CHANNEL_COUNT);
        gppFileOutChParams = (CHANNEL_PARAM**)malloc(sizeof(CHANNEL_PARAM*) * CHANNEL_COUNT);
        for (int i = 0; i < CHANNEL_COUNT; ++i) {
            gppFileOutChValues[i] = createChannel(sampleRate, gFileBufferLength);
            gppFileOutChParams[i] = gppFileOutChValues[i]->pParam;
        }
    }
    return gppFileOutChParams;
}

SAMPLER** WINAPI GetWaveOutSamplerPtr(UInt32 samplers) {
    if (NULL == gppWaveOutSamplers) {
        gWaveOutSamplers = samplers;
        gppWaveOutSamplers = (SAMPLER**)malloc(sizeof(SAMPLER*) * samplers);
        for (UInt32 i = 0; i < samplers; i++) {
            gppWaveOutSamplers[i] = createSampler();
        }
    }
    return gppWaveOutSamplers;
}

SAMPLER** WINAPI GetFileOutSamplerPtr(UInt32 samplers) {
    if (NULL == gppFileOutSamplers) {
        gFileOutSamplers = samplers;
        gppFileOutSamplers = (SAMPLER**)malloc(sizeof(SAMPLER*) * samplers);
        for (UInt32 i = 0; i < samplers; i++) {
            gppFileOutSamplers[i] = createSampler();
        }
    }
    return gppFileOutSamplers;
}

LPBYTE WINAPI LoadDLS(LPWSTR filePath, UInt32 *size) {
    //
    gDoStop = true;
    while (!gIsStopped) {
        Sleep(100);
    }

    if (NULL == size) {
        return NULL;
    }

    //
    if (NULL != gpDlsBuffer) {
        free(gpDlsBuffer);
        gpDlsBuffer = NULL;
    }

    //
    FILE *fpDLS = NULL;
    _wfopen_s(&fpDLS, filePath, TEXT("rb"));
    if (NULL != fpDLS) {
        //
        fseek(fpDLS, 4, SEEK_SET);
        fread_s(size, sizeof(*size), sizeof(*size), 1, fpDLS);
        *size -= 8;

        //
        gpDlsBuffer = (LPBYTE)malloc(*size);
        if (NULL != gpDlsBuffer) {
            fseek(fpDLS, 12, SEEK_SET);
            fread_s(gpDlsBuffer, *size, *size, 1, fpDLS);
        }

        //
        fclose(fpDLS);
    }

    //
    gDoStop = false;

    return gpDlsBuffer;
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
        if (gWriteWaveBufferCount < 1) {
            waveOutWrite(ghWaveOut, &gWaveHdr[gReadWaveBufferIndex], sizeof(WAVEHDR));
            return;
        }
        gReadWaveBufferIndex = (gReadWaveBufferIndex + 1) % BUFFER_COUNT;
        waveOutWrite(ghWaveOut, &gWaveHdr[gReadWaveBufferIndex], sizeof(WAVEHDR));
        gWriteWaveBufferCount--;
        break;
    default:
        break;
    }
}
