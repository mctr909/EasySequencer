#include <windows.h>
#include <stdio.h>
#include <math.h>
#include "sampler.h"

/******************************************************************************/
#define     CHORUS_PHASES   3
#define     DELAY_TAPS      1048576

LPBYTE      __pBuffer = NULL;
SInt32      __sampleRate = 44100;
double      __deltaTime = 2.26757e-05;

/******************************************************************************/
LPBYTE loadDLS(LPWSTR filePath, UInt32 *size, UInt32 sampleRate) {
    if (NULL == size) {
        return NULL;
    }

    if (sampleRate < 8000 && 192000 < sampleRate) {
        return NULL;
    }

    //
    __sampleRate = sampleRate;
    __deltaTime = 1.0 / sampleRate;

    //
    if (NULL != __pBuffer) {
        free(__pBuffer);
        __pBuffer = NULL;
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
        __pBuffer = (LPBYTE)malloc(*size);
        if (NULL != __pBuffer) {
            fseek(fpDLS, 12, SEEK_SET);
            fread_s(__pBuffer, *size, *size, 1, fpDLS);
        }

        //
        fclose(fpDLS);
    }

    return __pBuffer;
}

CHANNEL** createChannels(UInt32 count) {
    CHANNEL **channels = (CHANNEL**)malloc(sizeof(CHANNEL*) * count);
    for (UInt32 i = 0; i < count; ++i) {
        channels[i] = (CHANNEL*)malloc(sizeof(CHANNEL));
        memset(channels[i], 0, sizeof(CHANNEL));
    }

    //
    for (UInt32 i = 0; i < count; ++i) {
        memset(&channels[i]->delay, 0, sizeof(DELAY));

        DELAY *delay = &channels[i]->delay;
        delay->readIndex = 0;
        delay->writeIndex = 0;

        delay->pTapL = (double*)malloc(sizeof(double) * DELAY_TAPS);
        delay->pTapR = (double*)malloc(sizeof(double) * DELAY_TAPS);
        memset(delay->pTapL, 0, sizeof(double) * DELAY_TAPS);
        memset(delay->pTapR, 0, sizeof(double) * DELAY_TAPS);
    }

    //
    for (UInt32 i = 0; i < count; ++i) {
        memset(&channels[i]->chorus, 0, sizeof(CHORUS));

        CHORUS *chorus = &channels[i]->chorus;
        chorus->lfoK = 6.283 / __sampleRate;
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

    return channels;
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
inline extern void channel(CHANNEL *ch, double *waveL, double *waveR) {
    //
    filter(&ch->eq, ch->curAmp * ch->wave);
    ch->wave = ch->eq.pole03;

    //
    ch->waveL = ch->wave * ch->panLeft;
    ch->waveR = ch->wave * ch->panRight;

    //
    delay(ch, &ch->delay);
    chorus(ch, &ch->delay, &ch->chorus);

    //
    ch->curAmp += 200 * (ch->tarAmp - ch->curAmp) * __deltaTime;
    ch->eq.cutoff += 200 * (ch->tarCutoff - ch->eq.cutoff) * __deltaTime;
    ch->eq.resonance += 200 * (ch->tarResonance - ch->eq.resonance) * __deltaTime;

    //
    *waveL += ch->waveL;
    *waveR += ch->waveR;
    ch->wave = 0.0;
}

inline extern void sampler(CHANNEL **chs, SAMPLER *smpl) {
    if (NULL == chs || NULL == smpl || !smpl->isActive) {
        return;
    }

    CHANNEL *ch = chs[smpl->channelNo];
    if (NULL == ch) {
        return;
    }

    if (smpl->onKey) {
        if (smpl->time < smpl->envAmp.hold) {
            smpl->curAmp += (1.0 - smpl->curAmp) * smpl->envAmp.deltaA;
        }
        else {
            smpl->curAmp += (smpl->envAmp.levelS - smpl->curAmp) * smpl->envAmp.deltaD;
        }

        if (smpl->time < smpl->envEq.hold) {
            smpl->eq.cutoff += (smpl->envEq.levelD - smpl->eq.cutoff) * smpl->envEq.deltaA;
        }
        else {
            smpl->eq.cutoff += (smpl->envEq.levelS - smpl->eq.cutoff) * smpl->envEq.deltaD;
        }
    }
    else {
        if (ch->holdDelta < 1.0) {
            smpl->curAmp -= smpl->curAmp * ch->holdDelta;
        }
        else {
            smpl->curAmp -= smpl->curAmp * smpl->envAmp.deltaR;
        }

        smpl->eq.cutoff += (smpl->envEq.levelR - smpl->eq.cutoff) * smpl->envEq.deltaR;

        if (smpl->curAmp < 0.001) {
            smpl->isActive = false;
        }
    }

    //
    SInt16 *pcm = (SInt16*)(__pBuffer + smpl->pcmAddr);
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
    filter(&smpl->eq, (pcm[cur] * dt + pcm[pre] * (1.0 - dt)) * smpl->gain * smpl->tarAmp * smpl->curAmp);
    ch->wave += smpl->eq.pole03;

    //
    smpl->index += smpl->delta * ch->pitch;
    smpl->time += __deltaTime;

    //
    if ((smpl->loop.start + smpl->loop.length) < smpl->index) {
        smpl->index -= smpl->loop.length;
        if (!smpl->loop.enable) {
            smpl->isActive = false;
        }
    }
}

/******************************************************************************/
inline void delay(CHANNEL *ch, DELAY *delay) {
    ++delay->writeIndex;
    if (DELAY_TAPS <= delay->writeIndex) {
        delay->writeIndex = 0;
    }

    delay->readIndex = delay->writeIndex - (SInt32)(delay->rate * __sampleRate);
    if (delay->readIndex < 0) {
        delay->readIndex += DELAY_TAPS;
    }

    double delayL = delay->depth * delay->pTapL[delay->readIndex];
    double delayR = delay->depth * delay->pTapR[delay->readIndex];

    ch->waveL += (0.7 * delayL + 0.3 * delayR);
    ch->waveR += (0.7 * delayR + 0.3 * delayL);

    delay->pTapL[delay->writeIndex] = ch->waveL;
    delay->pTapR[delay->writeIndex] = ch->waveR;
}

inline void chorus(CHANNEL *ch, DELAY *delay, CHORUS *chorus) {
    double chorusL = 0.0;
    double chorusR = 0.0;
    double index;
    double dt;
    SInt32 indexCur;
    SInt32 indexPre;

    for (register ph = 0; ph < CHORUS_PHASES; ++ph) {
        index = delay->writeIndex - (0.5 - 0.45 * chorus->pLfoRe[ph]) * __sampleRate * 0.1;
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

        chorus->pLfoRe[ph] -= chorus->lfoK * chorus->rate * chorus->pLfoIm[ph];
        chorus->pLfoIm[ph] += chorus->lfoK * chorus->rate * chorus->pLfoRe[ph];
    }

    ch->waveL += chorusL * chorus->depth / CHORUS_PHASES;
    ch->waveR += chorusR * chorus->depth / CHORUS_PHASES;
}

inline void filter(FILTER *filter, double input) {
    double fi = 1.0 - filter->cutoff;
    double p = filter->cutoff + 0.8 * filter->cutoff * fi;
    double q = p + p - 1.0;

    input -= 0.9 * (1.0 + 0.5 * fi * (1.0 - fi + 5.6 * fi * fi)) * filter->resonance * filter->pole03;

    filter->pole00 = (input          + filter->pole10) * p - filter->pole00 * q;
    filter->pole01 = (filter->pole00 + filter->pole11) * p - filter->pole01 * q;
    filter->pole02 = (filter->pole01 + filter->pole12) * p - filter->pole02 * q;
    filter->pole03 = (filter->pole02 + filter->pole13) * p - filter->pole03 * q;

    filter->pole10 = input;
    filter->pole11 = filter->pole00;
    filter->pole12 = filter->pole01;
    filter->pole13 = filter->pole02;
}
