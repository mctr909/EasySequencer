﻿#include "main.h"
#include "sampler.h"
#include <stdio.h>
#include <math.h>
#include <mmsystem.h>

#pragma comment (lib, "winmm.lib")

/******************************************************************************/
#define BUFFER_COUNT        32
#define CHANNEL_COUNT       16
#define SAMPLER_COUNT       128

/******************************************************************************/
bool            gDoStop = false;
bool            gDoMute = false;
bool            gIsStopped = true;
bool            gIsMuted = true;

HWAVEOUT        ghWaveOut = NULL;
WAVEFORMATEX    gWaveFmt = { 0 };
WAVEHDR         gWaveHdr[BUFFER_COUNT] = { NULL };

LPBYTE          gpDlsBuffer = NULL;

SInt32          gWaveBufferLength = 0;
SInt32          gFileBufferLength = 0;

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

VOID WINAPI FileOutOpen(LPWSTR filePath, UInt32 bufferLength) {
    if (NULL != gpFileOutBuffer) {
        free(gpFileOutBuffer);
    }

    gFileBufferLength = bufferLength;
    gpFileOutBuffer = (float*)malloc(sizeof(float) * 2 * bufferLength);

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
    gFmt.sampleRate   = 44100;
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
    register SInt32 t, s, c;
    register float *pWave;
    static double waveL;
    static double waveR;

    pWave = gpFileOutBuffer;

    for (t = 0; t < gFileBufferLength; ++t) {
        for (s = 0; s < SAMPLER_COUNT; ++s) {
            sampler(gppFileOutChValues, gppFileOutSamplers[s], gpDlsBuffer);
        }

        waveL = 0.0;
        waveR = 0.0;
        for (c = 0; c < CHANNEL_COUNT; ++c) {
            channel(gppFileOutChValues[c], &waveL, &waveR);
        }

        *pWave = (float)waveL; ++pWave;
        *pWave = (float)waveR; ++pWave;
    }

    fwrite(gpFileOutBuffer, sizeof(float) * 2 * gFileBufferLength, 1, gfpFileOut);
    gFmt.dataSize += sizeof(float) * 2 * gFileBufferLength;
}

CHANNEL_PARAM** WINAPI GetWaveOutChannelPtr() {
    if (NULL == gppWaveOutChValues) {
        gppWaveOutChValues = createChannels(CHANNEL_COUNT);
        gppWaveOutChParams = (CHANNEL_PARAM**)malloc(sizeof(CHANNEL_PARAM*)*CHANNEL_COUNT);
        for (int i = 0; i < CHANNEL_COUNT; ++i) {
            gppWaveOutChParams[i] = gppWaveOutChValues[i]->param;
        }
    }
    return gppWaveOutChParams;
}

CHANNEL_PARAM** WINAPI GetFileOutChannelPtr() {
    if (NULL == gppFileOutChValues) {
        gppFileOutChValues = createChannels(CHANNEL_COUNT);
        gppFileOutChParams = (CHANNEL_PARAM**)malloc(sizeof(CHANNEL_PARAM*)*CHANNEL_COUNT);
        for (int i = 0; i < CHANNEL_COUNT; ++i) {
            gppFileOutChParams[i] = gppFileOutChValues[i]->param;
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

LPBYTE WINAPI LoadDLS(LPWSTR filePath, UInt32 *size, UInt32 sampleRate) {
    //
    gDoMute = true;
    while (!gIsMuted) {
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

    gSampleRate = sampleRate;
    gDeltaTime = 1.0 / sampleRate;

    //
    gDoMute = false;

    return gpDlsBuffer;
}

/******************************************************************************/
void CALLBACK waveOutProc(HWAVEOUT hwo, UInt32 uMsg) {
    static SInt32 b;
    static double waveL;
    static double waveR;

    register SInt16* pWave;
    register t, s, c;

    switch (uMsg) {
    case MM_WOM_OPEN:
        break;
    case MM_WOM_CLOSE:
        gDoStop = true;
        while (!gIsStopped) {
            Sleep(100);
        }
        gDoStop = false;
        for (b = 0; b < BUFFER_COUNT; ++b) {
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
        if (gDoMute || NULL == gppWaveOutChParams || NULL == gppWaveOutSamplers || NULL == gpDlsBuffer) {
            for (b = 0; b < BUFFER_COUNT; ++b) {
                if (0 == (gWaveHdr[b].dwFlags & WHDR_INQUEUE)) {
                    memset(gWaveHdr[b].lpData, 0, gWaveFmt.nBlockAlign * gWaveBufferLength);
                    waveOutWrite(ghWaveOut, &gWaveHdr[b], sizeof(WAVEHDR));
                }
            }
            gIsMuted = true;
            break;
        }
        gIsMuted = false;

        //
        for (b = 0; b < BUFFER_COUNT; ++b) {
            if (0 == (gWaveHdr[b].dwFlags & WHDR_INQUEUE)) {
                pWave = (SInt16*)gWaveHdr[b].lpData;
                for (t = 0; t < gWaveBufferLength; ++t) {
                    for (s = 0; s < SAMPLER_COUNT; ++s) {
                        sampler(gppWaveOutChValues, gppWaveOutSamplers[s], gpDlsBuffer);
                    }

                    waveL = 0.0;
                    waveR = 0.0;
                    for (c = 0; c < CHANNEL_COUNT; ++c) {
                        channel(gppWaveOutChValues[c], &waveL, &waveR);
                    }

                    if (1.0 < waveL) waveL = 1.0;
                    if (waveL < -1.0) waveL = -1.0;
                    if (1.0 < waveR) waveR = 1.0;
                    if (waveR < -1.0) waveR = -1.0;
                    *pWave = (SInt16)(waveL * 32767); ++pWave;
                    *pWave = (SInt16)(waveR * 32767); ++pWave;
                }
                waveOutWrite(ghWaveOut, &gWaveHdr[b], sizeof(WAVEHDR));
            }
        }
        break;
    default:
        break;
    }
}
