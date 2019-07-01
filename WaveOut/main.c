#include "main.h"
#include "sampler.h"
#include <stdio.h>

/******************************************************************************/
bool            gIsStop = true;
bool            gIsMute = true;
bool            gIssueMute = false;

HWAVEOUT        ghWaveOut = NULL;
WAVEFORMATEX    gWaveFmt = { 0 };
WAVEHDR         gWaveHdr[BUFFER_COUNT] = { NULL };

LPBYTE          gpDlsBuffer = NULL;

SInt32          gWaveBufferLength = 0;
SInt32          gFileBufferLength = 0;

CHANNEL         **gppWaveOutChannels = NULL;
CHANNEL         **gppFileOutChannels = NULL;
SAMPLER         **gppWaveOutSamplers = NULL;
SAMPLER         **gppFileOutSamplers = NULL;

float           *gpFileOutBuffer = NULL;
FILE            *gfpFileOut = NULL;
RIFF            gRiff;
FMT_            gFmt;

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
        (DWORD_PTR)WaveOutProc,
        (DWORD_PTR)gWaveHdr,
        CALLBACK_FUNCTION
    )) {
        return false;
    }

    //
    gIsStop = false;

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
    gIsStop = true;

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
            sampler(gppFileOutChannels, gppFileOutSamplers[s]);
        }

        waveL = 0.0;
        waveR = 0.0;
        for (c = 0; c < CHANNEL_COUNT; ++c) {
            channel(gppFileOutChannels[c], &waveL, &waveR);
        }

        *pWave = (float)waveL; ++pWave;
        *pWave = (float)waveR; ++pWave;
    }

    fwrite(gpFileOutBuffer, sizeof(float) * 2 * gFileBufferLength, 1, gfpFileOut);
    gFmt.dataSize += sizeof(float) * 2 * gFileBufferLength;
}

CHANNEL** WINAPI GetWaveOutChannelPtr() {
    if (NULL == gppWaveOutChannels) {
        gppWaveOutChannels = createChannels(CHANNEL_COUNT);
    }
    return gppWaveOutChannels;
}

CHANNEL** WINAPI GetFileOutChannelPtr() {
    if (NULL == gppFileOutChannels) {
        gppFileOutChannels = createChannels(CHANNEL_COUNT);
    }
    return gppFileOutChannels;
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
    gIssueMute = true;
    while (!gIsMute) {
        Sleep(100);
    }

    gpDlsBuffer = loadDLS(filePath, size, sampleRate);

    //
    gIssueMute = false;

    return gpDlsBuffer;
}

/******************************************************************************/
void CALLBACK WaveOutProc(HWAVEOUT hwo, UInt32 uMsg) {
    static SInt32 b;
    static double waveL;
    static double waveR;

    register SInt16* pWave;
    register t, s, c;

    switch (uMsg) {
    case MM_WOM_OPEN:
        break;
    case MM_WOM_CLOSE:
        gIssueMute = true;
        while (!gIsMute) {
            Sleep(100);
        }
        gIssueMute = false;
        for (b = 0; b < BUFFER_COUNT; ++b) {
            free(gWaveHdr[b].lpData);
            gWaveHdr[b].lpData = NULL;
        }
        break;
    case MM_WOM_DONE:
        //
        if (gIsStop) {
            break;
        }

        //
        if (gIssueMute || NULL == gppWaveOutChannels || NULL == gppWaveOutSamplers || NULL == gpDlsBuffer) {
            for (b = 0; b < BUFFER_COUNT; ++b) {
                if (0 == (gWaveHdr[b].dwFlags & WHDR_INQUEUE)) {
                    memset(gWaveHdr[b].lpData, 0, gWaveFmt.nBlockAlign * gWaveBufferLength);
                    waveOutWrite(ghWaveOut, &gWaveHdr[b], sizeof(WAVEHDR));
                }
            }
            gIsMute = true;
            break;
        }

        gIsMute = false;

        //
        for (b = 0; b < BUFFER_COUNT; ++b) {
            if (0 == (gWaveHdr[b].dwFlags & WHDR_INQUEUE)) {
                pWave = (SInt16*)gWaveHdr[b].lpData;
                for (t = 0; t < gWaveBufferLength; ++t) {
                    for (s = 0; s < SAMPLER_COUNT; ++s) {
                        sampler(gppWaveOutChannels, gppWaveOutSamplers[s]);
                    }

                    waveL = 0.0;
                    waveR = 0.0;
                    for (c = 0; c < CHANNEL_COUNT; ++c) {
                        channel(gppWaveOutChannels[c], &waveL, &waveR);
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