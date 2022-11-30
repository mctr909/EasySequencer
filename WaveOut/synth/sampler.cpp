#include "channel.h"
#include "channel_const.h"
#include "../inst/inst_list.h"
#include "../message_reciever.h"

#include "sampler.h"

/******************************************************************************/
#define PURGE_THRESHOLD 0.0005
#define PURGE_SPEED     500
#define OVER_SAMPLING   8

/******************************************************************************/
Sampler::Sampler(SYSTEM_VALUE* pSystemValue) {
    mpSystemValue = pSystemValue;
}

void
Sampler::note_on(Channel* pChannel, INST_LAYER* pLayer, INST_REGION* pRegion, byte note_num, byte velocity) {
    state = Sampler::E_STATE::RESERVED;
    mpChannel = pChannel;
    channel_num = pChannel->number;
    this->note_num = note_num;

    auto cInstList = mpSystemValue->pInst_list;
    auto pInstInfo = pChannel->mpInst;

    auto pWave = cInstList->mppWaveList[pRegion->waveIndex];
    loop_enable = 1 == pWave->loopEnable;
    loop_length = pWave->loopLength;
    loop_end = (long)pWave->loopBegin + pWave->loopLength;
    mpWaveData = mpSystemValue->pWave_table + pWave->offset;

    index = 0.0;
    time = 0.0;
    pitch = 1.0;
    gain = velocity * velocity / 16129.0 / 32768.0;
    
    if (UINT_MAX != pInstInfo->artIndex) {
        auto pArt = cInstList->mppArtList[pInstInfo->artIndex];
        pan += pArt->pan;
        //pArt->transpose;
        pitch *= pArt->pitch;
        gain *= pArt->gain;
        pEnv = &pArt->env;
        eg_amp = 0.0;
        eg_cutoff = pArt->env.cutoffRise;
        eg_pitch = pArt->env.pitchRise;
    }
    if (UINT_MAX != pLayer->artIndex) {
        auto pArt = cInstList->mppArtList[pLayer->artIndex];
        pan += pArt->pan;
        //pArt->transpose;
        pitch *= pArt->pitch;
        gain *= pArt->gain;
        pEnv = &pArt->env;
        eg_amp = 0.0;
        eg_cutoff = pArt->env.cutoffRise;
        eg_pitch = pArt->env.pitchRise;
    }
    if (UINT_MAX != pRegion->artIndex) {
        auto pArt = cInstList->mppArtList[pRegion->artIndex];
        pan += pArt->pan;
        //pArt->transpose;
        pitch *= pArt->pitch;
        gain *= pArt->gain;
        pEnv = &pArt->env;
        eg_amp = 0.0;
        eg_cutoff = pArt->env.cutoffRise;
        eg_pitch = pArt->env.pitchRise;
    }

    auto diffNote = 0;
    if (UINT_MAX == pRegion->wsmpIndex) {
        diffNote = note_num - pWave->unityNote;
        pitch *= pWave->pitch;
        gain *= pWave->gain;
    } else {
        auto pWsmp = cInstList->mppWaveList[pRegion->wsmpIndex];
        diffNote = note_num - pWsmp->unityNote;
        pitch *= pWsmp->pitch;
        gain *= pWsmp->gain;
    }

    if (diffNote < 0) {
        pitch *= 1.0 / SemiTone[-diffNote];
    } else {
        pitch *= SemiTone[diffNote];
    }
    pitch *= pWave->sampleRate;
    pitch = pitch / mpSystemValue->sample_rate / OVER_SAMPLING;
    state = Sampler::E_STATE::PRESS;
}

bool
Sampler::step() {
    auto pOutput_l = mpChannel->pInput_l;
    auto pOutput_r = mpChannel->pInput_r;

    for (int32 i = 0; i < mpSystemValue->buffer_length; i++) {
        //*******************************
        // generate wave
        //*******************************
        double smoothedWave = 0.0;
        double delta = pitch * mpChannel->pitch;
        for (int32 o = 0; o < OVER_SAMPLING; o++) {
            auto ii = (int32)index;
            auto dt = index - ii;
            smoothedWave += mpWaveData[ii - 1] * (1.0 - dt) + mpWaveData[ii] * dt;
            index += delta;
            if (loop_end < index) {
                if (loop_enable) {
                    index -= loop_length;
                } else {
                    state = E_STATE::FREE;
                    return false;
                }
            }
        }

        /*** output ***/
        auto wave = smoothedWave * gain * eg_amp / OVER_SAMPLING;
        pOutput_l[i] += wave;
        pOutput_r[i] += wave;

        //*******************************
        // generate envelope
        //*******************************
        switch (state) {
        case E_STATE::PRESS:
            if (time <= pEnv->ampH) {
                eg_amp += (1.0 - eg_amp) * mpSystemValue->delta_time * pEnv->ampA;
            } else {
                eg_amp += (pEnv->ampS - eg_amp) * mpSystemValue->delta_time * pEnv->ampD;
            }
            break;
        case E_STATE::RELEASE:
            eg_amp -= eg_amp * mpSystemValue->delta_time * pEnv->ampR;
            break;
        case E_STATE::HOLD:
            eg_amp -= eg_amp * mpSystemValue->delta_time;
            break;
        case E_STATE::PURGE:
            eg_amp -= eg_amp * mpSystemValue->delta_time * PURGE_SPEED;
            break;
        }
        time += mpSystemValue->delta_time;

        //*******************************
        // free condition
        //*******************************
        if (pEnv->ampH < time && eg_amp < PURGE_THRESHOLD) {
            state = E_STATE::FREE;
            return false;
        }
    }
    return true;
}
