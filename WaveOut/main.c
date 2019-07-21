#include "main.h"
#include <stdio.h>
#include <math.h>
#include <mmsystem.h>

#pragma comment (lib, "winmm.lib")

/******************************************************************************/
#define BUFFER_COUNT        32
#define CHANNEL_COUNT       16
#define SAMPLER_COUNT       128

#define CHORUS_PHASES       3
#define DELAY_TAPS          1048576

static const double PI        = 3.14159265;
static const double INV_FACT2 = 0.50000000;
static const double INV_FACT3 = 0.16666667;
static const double INV_FACT4 = 0.04166667;
static const double INV_FACT5 = 0.00833333;
static const double INV_FACT6 = 0.00138889;
static const double INV_FACT7 = 0.00019841;
static const double INV_FACT8 = 0.00002480;
static const double INV_FACT9 = 0.00000276;

/******************************************************************************/
bool            gDoStop = false;
bool            gDoMute = false;
bool            gIsStopped = true;
bool            gIsMuted = true;

HWAVEOUT        ghWaveOut = NULL;
WAVEFORMATEX    gWaveFmt = { 0 };
WAVEHDR         gWaveHdr[BUFFER_COUNT] = { NULL };

LPBYTE          gpDlsBuffer = NULL;
SInt32          gSampleRate = 44100;
double          gDeltaTime = 2.26757e-05;

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
CHANNEL** createChannels(UInt32 count);
SAMPLER** createSamplers(UInt32 count);

/******************************************************************************/
inline void channel(CHANNEL *ch, double *waveL, double *waveR);
inline void sampler(CHANNEL **chs, SAMPLER *smpl);
inline void delay(CHANNEL *ch, DELAY *delay, double *waveL, double *waveR);
inline void chorus(CHANNEL *ch, DELAY *delay, CHORUS *chorus, double *waveL, double *waveR);
inline void filter(FILTER *param, double input);

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
            sampler(gppFileOutChValues, gppFileOutSamplers[s]);
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
                        sampler(gppWaveOutChValues, gppWaveOutSamplers[s]);
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

CHANNEL** createChannels(UInt32 count) {
    CHANNEL **channel = (CHANNEL**)malloc(sizeof(CHANNEL*) * count);
    for (UInt32 i = 0; i < count; ++i) {
        channel[i] = (CHANNEL*)malloc(sizeof(CHANNEL));
        memset(channel[i], 0, sizeof(CHANNEL));
        channel[i]->param = (CHANNEL_PARAM*)malloc(sizeof(CHANNEL_PARAM));
        memset(channel[i]->param, 0, sizeof(CHANNEL_PARAM));
    }

    // Filter
    for (UInt32 i = 0; i < count; ++i) {
        memset(&channel[i]->eq, 0, sizeof(FILTER));
    }

    // Delay
    for (UInt32 i = 0; i < count; ++i) {
        memset(&channel[i]->delay, 0, sizeof(DELAY));

        DELAY *delay = &channel[i]->delay;
        delay->readIndex = 0;
        delay->writeIndex = 0;

        delay->pTapL = (double*)malloc(sizeof(double) * DELAY_TAPS);
        delay->pTapR = (double*)malloc(sizeof(double) * DELAY_TAPS);
        memset(delay->pTapL, 0, sizeof(double) * DELAY_TAPS);
        memset(delay->pTapR, 0, sizeof(double) * DELAY_TAPS);
    }

    // Chorus
    for (UInt32 i = 0; i < count; ++i) {
        memset(&channel[i]->chorus, 0, sizeof(CHORUS));

        CHORUS *chorus = &channel[i]->chorus;
        chorus->lfoK = 6.283 * gDeltaTime;
        chorus->pPanL = (double*)malloc(sizeof(double) * CHORUS_PHASES);
        chorus->pPanR = (double*)malloc(sizeof(double) * CHORUS_PHASES);
        chorus->pLfoRe = (double*)malloc(sizeof(double) * CHORUS_PHASES);
        chorus->pLfoIm = (double*)malloc(sizeof(double) * CHORUS_PHASES);

        for (SInt32 p = 0; p < CHORUS_PHASES; ++p) {
            chorus->pPanL[p] = cos(3.1416 * p / CHORUS_PHASES);
            chorus->pPanR[p] = sin(3.1416 * p / CHORUS_PHASES);
            chorus->pLfoRe[p] = cos(6.283 * p / CHORUS_PHASES);
            chorus->pLfoIm[p] = sin(6.283 * p / CHORUS_PHASES);
        }
    }

    return channel;
}

SAMPLER** createSamplers(UInt32 count) {
    SAMPLER** samplers = (SAMPLER**)malloc(sizeof(SAMPLER*) * count);
    for (UInt32 i = 0; i < count; ++i) {
        samplers[i] = (SAMPLER*)malloc(sizeof(SAMPLER));
        memset(samplers[i], 0, sizeof(SAMPLER));
    }

    return samplers;
}

/******************************************************************************/
inline void channel(CHANNEL *ch, double *waveL, double *waveR) {
    //
    filter(&ch->eq, ch->amp * ch->wave);
    ch->wave = ch->eq.a2;

    //
    ch->waveL = ch->wave * ch->panLeft;
    ch->waveR = ch->wave * ch->panRight;

    //
    delay(ch, &ch->delay, &ch->waveL, &ch->waveR);
    chorus(ch, &ch->delay, &ch->chorus, &ch->waveL, &ch->waveR);

    //
    ch->panLeft  += 250 * (ch->param->panLeft   - ch->panLeft)  * gDeltaTime;
    ch->panRight += 250 * (ch->param->panRight  - ch->panRight) * gDeltaTime;
    ch->amp      += 250 * (ch->param->amp       - ch->amp)      * gDeltaTime;
    ch->eq.cut   += 250 * (ch->param->cutoff    - ch->eq.cut)   * gDeltaTime;
    ch->eq.res   += 250 * (ch->param->resonance - ch->eq.res)   * gDeltaTime;

    //
    *waveL += ch->waveL;
    *waveR += ch->waveR;
    ch->wave = 0.0;
}

inline void sampler(CHANNEL **chs, SAMPLER *smpl) {
    if (NULL == chs || NULL == smpl || !smpl->isActive) {
        return;
    }

    CHANNEL *chValue = chs[smpl->channelNo];
    if (NULL == chValue) {
        return;
    }
    CHANNEL_PARAM *chParam = chValue->param;
    if (NULL == chParam) {
        return;
    }

    if (smpl->onKey) {
        if (smpl->time < smpl->envAmp.hold) {
            smpl->amp += (1.0 - smpl->amp) * smpl->envAmp.deltaA;
        } else {
            smpl->amp += (smpl->envAmp.levelS - smpl->amp) * smpl->envAmp.deltaD;
        }

        if (smpl->time < smpl->envEq.hold) {
            smpl->eq.cut += (smpl->envEq.levelD - smpl->eq.cut) * smpl->envEq.deltaA;
        } else {
            smpl->eq.cut += (smpl->envEq.levelS - smpl->eq.cut) * smpl->envEq.deltaD;
        }
    } else {
        if (chParam->holdDelta < 1.0) {
            smpl->amp -= smpl->amp * chParam->holdDelta;
        } else {
            smpl->amp -= smpl->amp * smpl->envAmp.deltaR;
        }

        smpl->eq.cut += (smpl->envEq.levelR - smpl->eq.cut) * smpl->envEq.deltaR;

        if (smpl->amp < 0.001) {
            smpl->isActive = false;
        }
    }

    //
    SInt16 *pcm = (SInt16*)(gpDlsBuffer + smpl->pcmAddr);
    SInt32 cur = (SInt32)smpl->index;
    SInt32 pre = cur - 1;
    double dt = smpl->index - cur;
    if (pre < 0) {
        pre = 0;
    }
    if (smpl->pcmLength <= cur) {
        cur = 0;
        pre = 0;
        smpl->index = 0.0;
        if (!smpl->loop.enable) {
            smpl->isActive = false;
        }
    }

    //
    filter(&smpl->eq, (pcm[cur] * dt + pcm[pre] * (1.0 - dt)) * smpl->gain * smpl->velocity * smpl->amp);
    chValue->wave += smpl->eq.a2;

    //
    smpl->index += smpl->delta * chParam->pitch;
    smpl->time += gDeltaTime;

    //
    if ((smpl->loop.start + smpl->loop.length) < smpl->index) {
        smpl->index -= smpl->loop.length;
        if (!smpl->loop.enable) {
            smpl->isActive = false;
        }
    }
}

inline void delay(CHANNEL *ch, DELAY *delay, double *waveL, double *waveR) {
    ++delay->writeIndex;
    if (DELAY_TAPS <= delay->writeIndex) {
        delay->writeIndex = 0;
    }

    delay->readIndex = delay->writeIndex - (SInt32)(ch->param->delayTime * gSampleRate);
    if (delay->readIndex < 0) {
        delay->readIndex += DELAY_TAPS;
    }

    double delayL = ch->param->delayDepth * delay->pTapL[delay->readIndex];
    double delayR = ch->param->delayDepth * delay->pTapR[delay->readIndex];

    *waveL += (0.7 * delayL + 0.3 * delayR);
    *waveR += (0.7 * delayR + 0.3 * delayL);

    delay->pTapL[delay->writeIndex] = *waveL;
    delay->pTapR[delay->writeIndex] = *waveR;
}

inline void chorus(CHANNEL *ch, DELAY *delay, CHORUS *chorus, double *waveL, double *waveR) {
    double chorusL = 0.0;
    double chorusR = 0.0;
    double index;
    double dt;
    SInt32 indexCur;
    SInt32 indexPre;

    for (register ph = 0; ph < CHORUS_PHASES; ++ph) {
        index = delay->writeIndex - (0.5 - 0.45 * chorus->pLfoRe[ph]) * gSampleRate * 0.05;
        indexCur = (SInt32)index;
        indexPre = indexCur - 1;
        dt = index - indexCur;

        if (indexCur < 0) {
            indexCur += DELAY_TAPS;
        }
        if (DELAY_TAPS <= indexCur) {
            indexCur -= DELAY_TAPS;
        }

        if (indexPre < 0) {
            indexPre += DELAY_TAPS;
        }
        if (DELAY_TAPS <= indexPre) {
            indexPre -= DELAY_TAPS;
        }

        chorusL += (delay->pTapL[indexCur] * dt + delay->pTapL[indexPre] * (1.0 - dt)) * chorus->pPanL[ph];
        chorusR += (delay->pTapR[indexCur] * dt + delay->pTapR[indexPre] * (1.0 - dt)) * chorus->pPanR[ph];

        chorus->pLfoRe[ph] -= chorus->lfoK * ch->param->chorusRate * chorus->pLfoIm[ph];
        chorus->pLfoIm[ph] += chorus->lfoK * ch->param->chorusRate * chorus->pLfoRe[ph];
    }

    *waveL += chorusL * ch->param->chorusDepth / CHORUS_PHASES;
    *waveR += chorusR * ch->param->chorusDepth / CHORUS_PHASES;
}

inline void filter(FILTER *param, double input) {
    double w = param->cut * PI * 0.975;
    double w2 = w * w;
    double c = INV_FACT8;
    double s = INV_FACT9;
    c *= w2;
    s *= w2;
    c -= INV_FACT6;
    s -= INV_FACT7;
    c *= w2;
    s *= w2;
    c += INV_FACT4;
    s += INV_FACT5;
    c *= w2;
    s *= w2;
    c -= INV_FACT2;
    s -= INV_FACT3;
    c *= w2;
    s *= w2;
    c += 1.0;
    s += 1.0;
    s *= w;

    double a = s / (param->res * 4.0 + 1.0);
    double m = 1.0 / (a + 1.0);
    double ka0 = -2.0 * c  * m;
    double ka1 = (1.0 - a) * m;
    double kb0 = (1.0 - c) * m;
    double kb1 = kb0 * 0.5;

    double output =
        kb1 * input
        + kb0 * param->b0
        + kb1 * param->b1
        - ka0 * param->a0
        - ka1 * param->a1
    ;
    param->b1 = param->b0;
    param->b0 = input;
    param->a1 = param->a0;
    param->a0 = output;

    input = output;
    output =
        kb1 * input
        + kb0 * param->b2
        + kb1 * param->b3
        - ka0 * param->a2
        - ka1 * param->a3
    ;
    param->b3 = param->b2;
    param->b2 = input;
    param->a3 = param->a2;
    param->a2 = output;
}
