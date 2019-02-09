#include "main.h"
#include "sampler.h"
#include <stdio.h>

/******************************************************************************/
HWAVEOUT        g_hWaveOut = NULL;
WAVEFORMATEX    g_waveFmt = { 0 };
WAVEHDR         g_waveHdr[BUFFER_COUNT] = { NULL };

LPBYTE          gp_dlsBuffer = NULL;

SInt32          g_bufferLength = 4096;
SInt32          g_fileOutBufferLength = 4096;

CHANNEL         **gpp_channels = NULL;
SAMPLER         **gpp_samplers = NULL;
CHANNEL         **gpp_fileOutChannels = NULL;
SAMPLER         **gpp_fileOutSamplers = NULL;

float           *gp_fileOutBuff = NULL;
FILE            *gp_file = NULL;
RIFF            g_riff;
FMT_            g_fmt;

bool            g_isStop = true;
bool            g_isMute = true;
bool            g_issueMute = false;

/******************************************************************************/
BOOL WINAPI WaveOutOpen(UInt32 sampleRate, UInt32 bufferLength) {
    if (NULL != g_hWaveOut) {
        WaveOutClose();
    }

    //
    g_bufferLength = bufferLength;

    //
    g_waveFmt.wFormatTag = WAVE_FORMAT_PCM;
    g_waveFmt.nChannels = 2;
    g_waveFmt.wBitsPerSample = 16;
    g_waveFmt.nSamplesPerSec = sampleRate;
    g_waveFmt.nBlockAlign = g_waveFmt.nChannels * g_waveFmt.wBitsPerSample / 8;
    g_waveFmt.nAvgBytesPerSec = g_waveFmt.nSamplesPerSec * g_waveFmt.nBlockAlign;

    //
    if (MMSYSERR_NOERROR != waveOutOpen(
        &g_hWaveOut,
        WAVE_MAPPER,
        &g_waveFmt,
        (DWORD_PTR)WaveOutProc,
        (DWORD_PTR)g_waveHdr,
        CALLBACK_FUNCTION
    )) {
        return false;
    }

    //
    g_isStop = false;

    //
    for (UInt32 n = 0; n < BUFFER_COUNT; ++n) {
        g_waveHdr[n].dwBufferLength = g_bufferLength * g_waveFmt.nBlockAlign;
        g_waveHdr[n].dwFlags = WHDR_BEGINLOOP | WHDR_ENDLOOP;
        g_waveHdr[n].dwLoops = 0;
        g_waveHdr[n].dwUser = 0;
        if (NULL == g_waveHdr[n].lpData) {
            g_waveHdr[n].lpData = (LPSTR)malloc(g_bufferLength * g_waveFmt.nBlockAlign);
            if (NULL != g_waveHdr[n].lpData) {
                memset(g_waveHdr[n].lpData, 0, g_bufferLength * g_waveFmt.nBlockAlign);
                waveOutPrepareHeader(g_hWaveOut, &g_waveHdr[n], sizeof(WAVEHDR));
                waveOutWrite(g_hWaveOut, &g_waveHdr[n], sizeof(WAVEHDR));
            }
        }
    }

    return true;
}

VOID WINAPI WaveOutClose() {
    if (NULL == g_hWaveOut) {
        return;
    }

    //
    g_isStop = true;

    //
    for (UInt32 n = 0; n < BUFFER_COUNT; ++n) {
        waveOutUnprepareHeader(g_hWaveOut, &g_waveHdr[n], sizeof(WAVEHDR));
    }
    waveOutReset(g_hWaveOut);
    waveOutClose(g_hWaveOut);
    g_hWaveOut = NULL;
}

CHANNEL** WINAPI GetChannelPtr() {
    if (NULL == gpp_channels) {
        gpp_channels = createChannels(CHANNEL_COUNT);
    }
    return gpp_channels;
}

SAMPLER** WINAPI GetSamplerPtr() {
    if (NULL == gpp_samplers) {
        gpp_samplers = createSamplers(SAMPLER_COUNT);
    }
    return gpp_samplers;
}

CHANNEL** WINAPI GetFileOutChannelPtr() {
    if (NULL == gpp_fileOutChannels) {
        gpp_fileOutChannels = createChannels(CHANNEL_COUNT);
    }
    return gpp_fileOutChannels;
}

SAMPLER** WINAPI GetFileOutSamplerPtr() {
    if (NULL == gpp_fileOutSamplers) {
        gpp_fileOutSamplers = createSamplers(SAMPLER_COUNT);
    }
    return gpp_fileOutSamplers;
}

VOID WINAPI FileOutOpen(LPWSTR filePath, UInt32 bufferLength) {
    if (NULL != gp_fileOutBuff) {
        free(gp_fileOutBuff);
    }

    g_fileOutBufferLength = bufferLength;
    gp_fileOutBuff = (float*)malloc(sizeof(float) * 2 * bufferLength);

    if (NULL != gp_file) {
        fclose(gp_file);
        gp_file = NULL;
    }

    _wfopen_s(&gp_file, filePath, L"wb");

    //
    g_riff.riff     = 0x46464952;
    g_riff.fileSize = 0;
    g_riff.dataId   = 0x45564157;

    //
    g_fmt.chunkId      = 0x20746D66;
    g_fmt.chunkSize    = 16;
    g_fmt.formatId     = 3;
    g_fmt.channels     = 2;
    g_fmt.sampleRate   = 44100;
    g_fmt.bitPerSample = 32;
    g_fmt.blockAlign   = g_fmt.channels * g_fmt.bitPerSample >> 3;
    g_fmt.bytePerSec   = g_fmt.sampleRate * g_fmt.blockAlign;
    g_fmt.dataId       = 0x61746164;
    g_fmt.dataSize     = 0;

    //
    fwrite(&g_riff, sizeof(g_riff), 1, gp_file);
    fwrite(&g_fmt, sizeof(g_fmt), 1, gp_file);
}

VOID WINAPI FileOut() {
    register SInt32 t, s, c;
    register float *pWave;
    static double waveL;
    static double waveR;

    pWave = gp_fileOutBuff;

    for (t = 0; t < g_fileOutBufferLength; ++t) {
        for (s = 0; s < SAMPLER_COUNT; ++s) {
            sampler(gpp_fileOutChannels, gpp_fileOutSamplers[s]);
        }

        waveL = 0.0;
        waveR = 0.0;
        for (c = 0; c < CHANNEL_COUNT; ++c) {
            channel(gpp_fileOutChannels[c], &waveL, &waveR);
        }

        *pWave = (float)waveL; ++pWave;
        *pWave = (float)waveR; ++pWave;
    }

    fwrite(gp_fileOutBuff, sizeof(float) * 2 * g_fileOutBufferLength, 1, gp_file);
    g_fmt.dataSize += sizeof(float) * 2 * g_fileOutBufferLength;
}

VOID WINAPI FileOutClose() {
    if (NULL == gp_file) {
        return;
    }

    //
    g_riff.fileSize = g_fmt.dataSize + sizeof(g_fmt) + 4;

    //
    fseek(gp_file, 0, SEEK_SET);
    fwrite(&g_riff, sizeof(g_riff), 1, gp_file);
    fwrite(&g_fmt, sizeof(g_fmt), 1, gp_file);

    //
    fclose(gp_file);
    gp_file = NULL;
}

LPBYTE WINAPI LoadDLS(LPWSTR filePath, UInt32 *size, UInt32 sampleRate) {
    //
    g_issueMute = true;
    while (!g_isMute) {
        Sleep(100);
    }

    gp_dlsBuffer = loadDLS(filePath, size, sampleRate);

    //
    g_issueMute = false;

    return gp_dlsBuffer;
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
        for (b = 0; b < BUFFER_COUNT; ++b) {
            free(g_waveHdr[b].lpData);
            g_waveHdr[b].lpData = NULL;
        }
        break;
    case MM_WOM_DONE:
        //
        if (g_isStop) {
            break;
        }

        //
        if (g_issueMute || NULL == gpp_channels || NULL == gpp_samplers || NULL == gp_dlsBuffer) {
            for (b = 0; b < BUFFER_COUNT; ++b) {
                if (0 == (g_waveHdr[b].dwFlags & WHDR_INQUEUE)) {
                    memset(g_waveHdr[b].lpData, 0, g_waveFmt.nBlockAlign * g_bufferLength);
                    waveOutWrite(g_hWaveOut, &g_waveHdr[b], sizeof(WAVEHDR));
                }
            }
            g_isMute = true;
            break;
        }

        g_isMute = false;

        //
        for (b = 0; b < BUFFER_COUNT; ++b) {
            if (0 == (g_waveHdr[b].dwFlags & WHDR_INQUEUE)) {
                pWave = (SInt16*)g_waveHdr[b].lpData;
                for (t = 0; t < g_bufferLength; ++t) {
                    for (s = 0; s < SAMPLER_COUNT; ++s) {
                        sampler(gpp_channels, gpp_samplers[s]);
                    }

                    waveL = 0.0;
                    waveR = 0.0;
                    for (c = 0; c < CHANNEL_COUNT; ++c) {
                        channel(gpp_channels[c], &waveL, &waveR);
                    }

                    if (1.0 < waveL) waveL = 1.0;
                    if (waveL < -1.0) waveL = -1.0;
                    if (1.0 < waveR) waveR = 1.0;
                    if (waveR < -1.0) waveR = -1.0;
                    *pWave = (SInt16)(waveL * 32767); ++pWave;
                    *pWave = (SInt16)(waveR * 32767); ++pWave;
                }
                waveOutWrite(g_hWaveOut, &g_waveHdr[b], sizeof(WAVEHDR));
            }
        }
        break;
    default:
        break;
    }
}