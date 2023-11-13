#include "../inst_list.h"

#include <stdlib.h>

#include "channel.h"
#include "sampler.h"

/******************************************************************************/
Sampler::Sampler(Synth* p_synth) {
    mp_synth = p_synth;
}

Sampler::~Sampler() {
}

void
Sampler::note_on(Channel* p_channel, INST_LAYER* p_layer, INST_REGION* p_region, byte note_num, byte velocity) {
    m_state = Sampler::E_STATE::RESERVED;
    mp_channel = p_channel;
    m_channel_num = p_channel->m_num;
    m_note_num = note_num;

    auto p_inst_list = mp_synth->mp_inst_list;
    auto p_inst_info = p_channel->mp_inst;

    auto p_wave_info = p_inst_list->mppWaveList[p_region->waveIndex];
    m_wave_info.enable_loop = 1 == p_wave_info->loopEnable;
    m_wave_info.loop_length = p_wave_info->loopLength;
    m_wave_info.loop_end = (long)p_wave_info->loopBegin + p_wave_info->loopLength;
    m_wave_info.index = 0.0;
    m_wave_info.p_data = mp_synth->mp_wave_table + p_wave_info->offset;

    m_gain = velocity / 127.0 / 32768.0;
    m_tune = 1.0;
    m_hold = mp_synth->m_delta_time;
    m_current_pan_re = p_channel->m_pan_re;
    m_current_pan_im = p_channel->m_pan_im;

    INST_ART *p_art = nullptr;
    if (INVALID_INDEX != p_inst_info->artIndex) {
        p_art = p_inst_list->mppArtList[p_inst_info->artIndex];
    }
    if (INVALID_INDEX != p_layer->artIndex) {
        p_art = p_inst_list->mppArtList[p_layer->artIndex];
    }
    if (INVALID_INDEX != p_region->artIndex) {
        p_art = p_inst_list->mppArtList[p_region->artIndex];
    }

    m_eg_amp.value = 0.0;
    m_eg_amp.enable_decay = false;
    m_eg_pitch.enable_decay = false;
    m_eg_lpf.enable_decay = false;

    if (nullptr == p_art) {
        m_eg_amp.attack = 0.5;
        m_eg_amp.decay = 0.5;
        m_eg_amp.release = 0.5;
        m_eg_amp.sustain = 1.0;

        m_eg_pitch.value = 1.0;
        m_eg_pitch.attack = 0.5;
        m_eg_pitch.decay = 0.5;
        m_eg_pitch.release = 0.5;
        m_eg_pitch.top = 1.0;
        m_eg_pitch.fall = 1.0;

        m_eg_lpf.value = 1.0;
        m_eg_lpf.attack = 0.5;
        m_eg_lpf.decay = 0.5;
        m_eg_lpf.release = 0.5;
        m_eg_lpf.sustain = 1.0;
        m_eg_lpf.top = 1.0;
        m_eg_lpf.fall = 1.0;
        m_eg_lpf.resonance = 0.0;
    } else {
        m_tune *= p_art->pitch;
        m_gain *= p_art->gain;

        m_eg_amp.attack = p_art->eg_amp.attack * mp_synth->m_delta_time;
        m_eg_amp.decay = p_art->eg_amp.decay * mp_synth->m_delta_time;
        m_eg_amp.release = p_art->eg_amp.release * mp_synth->m_delta_time;
        m_eg_amp.sustain = p_art->eg_amp.sustain;

        m_eg_pitch.value = p_art->eg_pitch.rise;
        m_eg_pitch.attack = p_art->eg_pitch.attack * mp_synth->m_delta_time;
        m_eg_pitch.decay = p_art->eg_pitch.decay * mp_synth->m_delta_time;
        m_eg_pitch.release = p_art->eg_pitch.release * mp_synth->m_delta_time;
        m_eg_pitch.top = p_art->eg_pitch.top;
        m_eg_pitch.fall = p_art->eg_pitch.fall;
        
        m_eg_lpf.value = p_art->eg_cutoff.rise;
        m_eg_lpf.attack = p_art->eg_cutoff.attack * mp_synth->m_delta_time;
        m_eg_lpf.decay = p_art->eg_cutoff.decay * mp_synth->m_delta_time;
        m_eg_lpf.release = p_art->eg_cutoff.release * mp_synth->m_delta_time;
        m_eg_lpf.sustain = p_art->eg_cutoff.sustain;
        m_eg_lpf.top = p_art->eg_cutoff.top;
        m_eg_lpf.fall = p_art->eg_cutoff.fall;
        m_eg_lpf.resonance = p_art->eg_cutoff.resonance;
    }

    auto diff_note = 0;
    if (INVALID_INDEX == p_region->wsmpIndex) {
        diff_note = note_num - p_wave_info->unityNote;
        m_tune *= p_wave_info->pitch;
        m_gain *= p_wave_info->gain;
    } else {
        auto p_wsmp = p_inst_list->mppWaveList[p_region->wsmpIndex];
        diff_note = note_num - p_wsmp->unityNote;
        m_tune *= p_wsmp->pitch;
        m_gain *= p_wsmp->gain;
    }

    if (diff_note < 0) {
        m_tune *= 1.0 / SEMITONE[-diff_note];
    } else {
        m_tune *= SEMITONE[diff_note];
    }
    m_tune *= p_wave_info->sampleRate;
    m_tune = m_tune / mp_synth->m_sample_rate / OVER_SAMPLING;
    m_state = Sampler::E_STATE::PRESS;
}

void
Sampler::step() {
    auto p_output_l = mp_channel->mp_buffer_l;
    auto p_output_r = mp_channel->mp_buffer_r;
    for (int32 i = 0; i < mp_synth->m_buffer_length; i++) {
        /*** generate envelope ***/
        switch (m_state) {
        case E_STATE::PRESS:
            if (m_eg_amp.enable_decay) {
                m_eg_amp.value += (m_eg_amp.sustain - m_eg_amp.value) * m_eg_amp.decay;
            }
            else {
                m_eg_amp.value += (1.0 - m_eg_amp.value) * m_eg_amp.attack;
                m_eg_amp.enable_decay = m_eg_amp.value >= 0.99;
            }
            if (m_eg_pitch.enable_decay) {
                m_eg_pitch.value += (1.0 - m_eg_pitch.value) * m_eg_pitch.decay;
            }
            else {
                m_eg_pitch.value += (m_eg_pitch.top - m_eg_pitch.value) * m_eg_pitch.attack;
                m_eg_pitch.enable_decay = m_eg_pitch.value <= 1.01 || m_eg_pitch.value >= 0.99;
            }
            if (m_eg_lpf.enable_decay) {
                m_eg_lpf.value += (m_eg_lpf.sustain - m_eg_lpf.value) * m_eg_lpf.decay;
            }
            else {
                m_eg_lpf.value += (m_eg_lpf.top - m_eg_lpf.value) * m_eg_lpf.attack;
                m_eg_lpf.enable_decay = m_eg_lpf.value <= 1.01 || m_eg_lpf.value >= 0.99;
            }
            break;
        case E_STATE::RELEASE:
            m_eg_amp.value -= m_eg_amp.value * m_eg_amp.release;
            m_eg_pitch.value += (m_eg_pitch.fall - m_eg_pitch.value) * m_eg_pitch.release;
            m_eg_lpf.value += (m_eg_lpf.fall - m_eg_lpf.value) * m_eg_lpf.release;
            break;
        case E_STATE::HOLD:
            m_eg_amp.value -= m_eg_amp.value * m_hold;
            break;
        case E_STATE::PURGE:
            m_eg_amp.value -= m_eg_amp.value * PURGE_SPEED;
            break;
        }
        /*** check amp ***/
        if (m_eg_amp.enable_decay && m_eg_amp.value < FREE_THRESHOLD) {
            m_state = E_STATE::FREE;
            return;
        }
        /*** generate wave ***/
        auto wave_l = 0.0;
        auto wave_r = 0.0;
        auto gain = m_gain * m_eg_amp.value / OVER_SAMPLING;
        {
            auto smoothed_wave = 0.0;
            auto delta = m_tune * m_eg_pitch.value * mp_channel->m_pitch;
            for (int32 o = 0; o < OVER_SAMPLING; o++) {
                auto ii = (int32)m_wave_info.index;
                auto ei = m_wave_info.index - ii;
                smoothed_wave += m_wave_info.p_data[ii] * (1.0 - ei) + m_wave_info.p_data[ii + 1] * ei;
                m_wave_info.index += delta;
                if (m_wave_info.loop_end < m_wave_info.index) {
                    if (m_wave_info.enable_loop) {
                        m_wave_info.index -= m_wave_info.loop_length;
                    }
                    else {
                        m_state = E_STATE::FREE;
                        return;
                    }
                }
            }
            auto pan_l = m_current_pan_re * 0.707 - m_current_pan_im * 0.707;
            auto pan_r = m_current_pan_re * 0.707 + m_current_pan_im * 0.707;
            auto wave = smoothed_wave * gain;
            wave_l += wave * pan_l;
            wave_r += wave * pan_r;
        }
        /*** LPF ***/
        filter_lpf24(&m_filter, m_eg_lpf.value, m_eg_lpf.resonance, wave_l, wave_r);
        p_output_l[i] += m_filter.l[5];
        p_output_r[i] += m_filter.r[5];

        m_current_pan_re += (mp_channel->m_pan_re - m_current_pan_re) * 0.02;
        m_current_pan_im += (mp_channel->m_pan_im - m_current_pan_im) * 0.02;
    }
}
