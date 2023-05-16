#include "channel.h"
#include "synth.h"
#include "../inst/inst_list.h"

#include "sampler.h"

/******************************************************************************/
#define PURGE_THRESHOLD 0.0005
#define OVER_SAMPLING   8

/******************************************************************************/
Sampler::Sampler(Synth* p_synth) {
    mp_synth = p_synth;
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
    m_loop_enable = 1 == p_wave_info->loopEnable;
    m_loop_length = p_wave_info->loopLength;
    m_loop_end = (long)p_wave_info->loopBegin + p_wave_info->loopLength;
    mp_wave_data = mp_synth->mp_wave_table + p_wave_info->offset;

    m_index = 0.0;
    m_time = 0.0;
    m_gain = velocity / 127.0 / 32768.0;
    m_pitch = 1.0;
    m_hold = mp_synth->m_delta_time;

    INST_ART *p_art = nullptr;
    if (UINT_MAX != p_inst_info->artIndex) {
        p_art = p_inst_list->mppArtList[p_inst_info->artIndex];
    }
    if (UINT_MAX != p_layer->artIndex) {
        p_art = p_inst_list->mppArtList[p_layer->artIndex];
    }
    if (UINT_MAX != p_region->artIndex) {
        p_art = p_inst_list->mppArtList[p_region->artIndex];
    }

    if (nullptr != p_art) {
        //pan += p_art->pan;
        //p_art->transpose;
        m_pitch *= p_art->pitch;
        m_gain *= p_art->gain;
        m_amp = 0.0;

        m_eg_amp.attack = p_art->eg_amp.attack * mp_synth->m_delta_time;
        m_eg_amp.decay = p_art->eg_amp.decay * mp_synth->m_delta_time;
        m_eg_amp.release = p_art->eg_amp.release * mp_synth->m_delta_time;
        m_eg_amp.hold = p_art->eg_amp.hold;
        m_eg_amp.sustain = p_art->eg_amp.sustain;

        m_epitch = p_art->eg_pitch.rise;
        m_eg_pitch.attack = p_art->eg_pitch.attack * mp_synth->m_delta_time;
        m_eg_pitch.decay = p_art->eg_pitch.decay * mp_synth->m_delta_time;
        m_eg_pitch.release = p_art->eg_pitch.release * mp_synth->m_delta_time;
        m_eg_pitch.hold = p_art->eg_pitch.hold;
        m_eg_pitch.top = p_art->eg_pitch.top;
        m_eg_pitch.fall = p_art->eg_pitch.fall;

        m_cutoff = p_art->eg_cutoff.rise;
        m_eg_cutoff.attack = p_art->eg_cutoff.attack * mp_synth->m_delta_time;
        m_eg_cutoff.decay = p_art->eg_cutoff.decay * mp_synth->m_delta_time;
        m_eg_cutoff.release = p_art->eg_cutoff.release * mp_synth->m_delta_time;
        m_eg_cutoff.hold = p_art->eg_cutoff.hold;
        m_eg_cutoff.sustain = p_art->eg_cutoff.sustain;
        m_eg_cutoff.top = p_art->eg_cutoff.top;
        m_eg_cutoff.fall = p_art->eg_cutoff.fall;
        m_eg_cutoff.resonance = p_art->eg_cutoff.resonance;
    }

    auto diff_note = 0;
    if (UINT_MAX == p_region->wsmpIndex) {
        diff_note = note_num - p_wave_info->unityNote;
        m_pitch *= p_wave_info->pitch;
        m_gain *= p_wave_info->gain;
    } else {
        auto p_wsmp = p_inst_list->mppWaveList[p_region->wsmpIndex];
        diff_note = note_num - p_wsmp->unityNote;
        m_pitch *= p_wsmp->pitch;
        m_gain *= p_wsmp->gain;
    }

    if (diff_note < 0) {
        m_pitch *= 1.0 / SEMITONE[-diff_note];
    } else {
        m_pitch *= SEMITONE[diff_note];
    }
    m_pitch *= p_wave_info->sampleRate;
    m_pitch = m_pitch / mp_synth->m_sample_rate / OVER_SAMPLING;
    m_state = Sampler::E_STATE::PRESS;
}

void
Sampler::step() {
    auto p_output_l = mp_channel->mp_input_l;
    auto p_output_r = mp_channel->mp_input_r;
    for (int32 i = 0; i < mp_synth->m_buffer_length; i++) {
        /*** generate wave ***/
        double smoothed_wave = 0.0;
        auto delta = m_pitch * m_epitch * mp_channel->m_pitch;
        for (int32 o = 0; o < OVER_SAMPLING; o++) {
            auto ii = (int32)m_index;
            auto dt = m_index - ii;
            smoothed_wave += mp_wave_data[ii - 1] * (1.0 - dt) + mp_wave_data[ii] * dt;
            m_index += delta;
            if (m_loop_end < m_index) {
                if (m_loop_enable) {
                    m_index -= m_loop_length;
                } else {
                    m_state = E_STATE::FREE;
                    return;
                }
            }
        }
        /*** output ***/
        auto wave = smoothed_wave * m_gain * m_amp / OVER_SAMPLING;
        p_output_l[i] += wave;
        p_output_r[i] += wave;
        /*** generate envelope ***/
        gen_envelope();
        m_time += mp_synth->m_delta_time;
        /*** purge threshold ***/
        if (E_STATE::PRESS != m_state && m_amp < PURGE_THRESHOLD) {
            m_state = E_STATE::FREE;
            return;
        }
    }
}
