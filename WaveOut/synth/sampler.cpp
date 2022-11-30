#include "channel.h"
#include "synth.h"
#include "../inst/inst_list.h"

#include "sampler.h"

/******************************************************************************/
#define PURGE_THRESHOLD 0.0005
#define PURGE_SPEED     500
#define OVER_SAMPLING   8

/******************************************************************************/
Sampler::Sampler(Synth* pSynth) {
    mpSynth = pSynth;
}

void
Sampler::note_on(Channel* pChannel, INST_LAYER* pLayer, INST_REGION* pRegion, byte note_num, byte velocity) {
    state = Sampler::E_STATE::RESERVED;
    mpChannel = pChannel;
    channel_num = pChannel->number;
    this->note_num = note_num;

    auto cInst_list = mpSynth->pInst_list;
    auto pInst_info = pChannel->mpInst;

    auto pWave = cInst_list->mppWaveList[pRegion->waveIndex];
    loop_enable = 1 == pWave->loopEnable;
    loop_length = pWave->loopLength;
    loop_end = (long)pWave->loopBegin + pWave->loopLength;
    mpWave_data = mpSynth->pWave_table + pWave->offset;

    index = 0.0;
    time = 0.0;
    pitch = 1.0;
    gain = velocity * velocity / 16129.0 / 32768.0;
    
    if (UINT_MAX != pInst_info->artIndex) {
        auto pArt = cInst_list->mppArtList[pInst_info->artIndex];
        pan += pArt->pan;
        //pArt->transpose;
        pitch *= pArt->pitch;
        gain *= pArt->gain;
        pEg = &pArt->env;
        eg_amp = 0.0;
        eg_pitch = pArt->env.pitchRise;
        eg_cutoff = pArt->env.cutoffRise;
    }
    if (UINT_MAX != pLayer->artIndex) {
        auto pArt = cInst_list->mppArtList[pLayer->artIndex];
        pan += pArt->pan;
        //pArt->transpose;
        pitch *= pArt->pitch;
        gain *= pArt->gain;
        pEg = &pArt->env;
        eg_amp = 0.0;
        eg_pitch = pArt->env.pitchRise;
        eg_cutoff = pArt->env.cutoffRise;
    }
    if (UINT_MAX != pRegion->artIndex) {
        auto pArt = cInst_list->mppArtList[pRegion->artIndex];
        pan += pArt->pan;
        //pArt->transpose;
        pitch *= pArt->pitch;
        gain *= pArt->gain;
        pEg = &pArt->env;
        eg_amp = 0.0;
        eg_pitch = pArt->env.pitchRise;
        eg_cutoff = pArt->env.cutoffRise;
    }

    auto diff_note = 0;
    if (UINT_MAX == pRegion->wsmpIndex) {
        diff_note = note_num - pWave->unityNote;
        pitch *= pWave->pitch;
        gain *= pWave->gain;
    } else {
        auto pWsmp = cInst_list->mppWaveList[pRegion->wsmpIndex];
        diff_note = note_num - pWsmp->unityNote;
        pitch *= pWsmp->pitch;
        gain *= pWsmp->gain;
    }

    if (diff_note < 0) {
        pitch *= 1.0 / SemiTone[-diff_note];
    } else {
        pitch *= SemiTone[diff_note];
    }
    pitch *= pWave->sampleRate;
    pitch = pitch / mpSynth->sample_rate / OVER_SAMPLING;
    state = Sampler::E_STATE::PRESS;
}

void
Sampler::step() {
    auto pOutput_l = mpChannel->pInput_l;
    auto pOutput_r = mpChannel->pInput_r;
    for (int32 i = 0; i < mpSynth->buffer_length; i++) {
        /*** generate wave ***/
        double smoothed_wave = 0.0;
        auto delta = pitch * mpChannel->pitch;
        for (int32 o = 0; o < OVER_SAMPLING; o++) {
            auto ii = (int32)index;
            auto dt = index - ii;
            smoothed_wave += mpWave_data[ii - 1] * (1.0 - dt) + mpWave_data[ii] * dt;
            index += delta;
            if (loop_end < index) {
                if (loop_enable) {
                    index -= loop_length;
                } else {
                    state = E_STATE::FREE;
                    return;
                }
            }
        }
        /*** output ***/
        auto wave = smoothed_wave * gain * eg_amp / OVER_SAMPLING;
        pOutput_l[i] += wave;
        pOutput_r[i] += wave;
        /*** generate envelope ***/
        switch (state) {
        case E_STATE::PRESS:
            if (time <= pEg->ampH) {
                eg_amp += (1.0 - eg_amp) * mpSynth->delta_time * pEg->ampA;
            } else {
                eg_amp += (pEg->ampS - eg_amp) * mpSynth->delta_time * pEg->ampD;
            }
            break;
        case E_STATE::RELEASE:
            eg_amp -= eg_amp * mpSynth->delta_time * pEg->ampR;
            break;
        case E_STATE::HOLD:
            eg_amp -= eg_amp * mpSynth->delta_time;
            break;
        case E_STATE::PURGE:
            eg_amp -= eg_amp * mpSynth->delta_time * PURGE_SPEED;
            break;
        }
        time += mpSynth->delta_time;
        /*** purge threshold ***/
        if (pEg->ampH < time && eg_amp < PURGE_THRESHOLD) {
            state = E_STATE::FREE;
            return;
        }
    }
}
