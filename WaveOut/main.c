#include "main.h"
#include "sampler.h"

/******************************************************************************/
HWAVEOUT        g_hWaveOut = NULL;
WAVEFORMATEX    g_waveFmt = { 0 };
WAVEHDR         g_waveHdr[BUFFER_COUNT] = { NULL };

Int32           g_bufferLength = 4096;
LPBYTE          gp_dlsBuffer = NULL;
CHANNEL         **gpp_channels = NULL;
SAMPLER         **gpp_samplers = NULL;

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
    static Int32 b;
    static double waveL;
    static double waveR;

    register Int16* pWave;
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
                pWave = (Int16*)g_waveHdr[b].lpData;
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
                    *pWave = (Int16)(waveL * 32767); ++pWave;
                    *pWave = (Int16)(waveR * 32767); ++pWave;
                }
                waveOutWrite(g_hWaveOut, &g_waveHdr[b], sizeof(WAVEHDR));
            }
        }
        break;
    default:
        break;
    }
}