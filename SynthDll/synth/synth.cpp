#include <stdio.h>

#include "channel.h"
#include "sampler.h"
#include "../inst/inst_list.h"

#include "synth.h"

/******************************************************************************/
Synth::Synth(InstList* p_inst_list, int32 sample_rate, int32 buffer_length) {
    this->sample_rate = sample_rate;
    delta_time = 1.0 / sample_rate;
    this->buffer_length = buffer_length;
    /* inst wave */
    this->p_inst_list = p_inst_list;
    p_wave_table = p_inst_list->mpWaveTable;
    /* allocate output buffer */
    mp_buffer_l = (double*)calloc(buffer_length, sizeof(double));
    mp_buffer_r = (double*)calloc(buffer_length, sizeof(double));
    /* allocate samplers */
    pp_samplers = (Sampler**)malloc(sizeof(Sampler*) * SAMPLER_COUNT);
    for (uint32 i = 0; i < SAMPLER_COUNT; i++) {
        pp_samplers[i] = new Sampler(this);
    }
    /* allocate channel params */
    pp_channel_params = (CHANNEL_PARAM**)malloc(sizeof(CHANNEL_PARAM*) * CHANNEL_COUNT);
    /* allocate channels */
    pp_channels = (Channel**)malloc(sizeof(Channel*) * CHANNEL_COUNT);
    for (int32 i = 0; i < CHANNEL_COUNT; i++) {
        pp_channels[i] = new Channel(this, i);
        pp_channel_params[i] = &pp_channels[i]->param;
    }
}

Synth::~Synth() {
    /* dispose channels */
    if (nullptr != pp_channels) {
        for (int32 i = 0; i < CHANNEL_COUNT; i++) {
            delete pp_channels[i];
        }
        free(pp_channels);
        pp_channels = nullptr;
    }
    /* dispose channel params */
    if (nullptr != pp_channel_params) {
        free(pp_channel_params);
        pp_channel_params = nullptr;
    }
    /* dispose samplers */
    if (nullptr != pp_samplers) {
        for (uint32 i = 0; i < SAMPLER_COUNT; i++) {
            delete pp_samplers[i];
        }
        free(pp_samplers);
        pp_samplers = nullptr;
    }
    /* dispose output buffer */
    if (nullptr != mp_buffer_l) {
        free(mp_buffer_l);
        mp_buffer_l = nullptr;
    }
    if (nullptr != mp_buffer_r) {
        free(mp_buffer_r);
        mp_buffer_r = nullptr;
    }
}

void
Synth::write_buffer(WAVE_DATA* p_pcm, void* p_param) {
    auto p_this = (Synth*)p_param;
    /* sampler loop */
    int32 active_count = 0;
    for (int32 i = 0; i < SAMPLER_COUNT; i++) {
        auto p_smpl = p_this->pp_samplers[i];
        if (p_smpl->state < Sampler::E_STATE::PURGE) {
            continue;
        }
        p_smpl->step();
        active_count++;
    }
    p_this->active_count = active_count;
    /* channel loop */
    for (int32 i = 0; i < CHANNEL_COUNT; i++) {
        auto p_ch = p_this->pp_channels[i];
        if (Channel::E_STATE::FREE == p_ch->state) {
            continue;
        }
        p_ch->step(p_this->mp_buffer_l, p_this->mp_buffer_r);
    }
    /* write buffer */
    for (int32 i = 0, j = 0; i < p_this->buffer_length; i++, j += 2) {
        auto p_l = p_this->mp_buffer_l + i;
        auto p_r = p_this->mp_buffer_r + i;
        if (*p_l < -1.0) {
            *p_l = -1.0;
        }
        if (1.0 < *p_l) {
            *p_l = 1.0;
        }
        if (*p_r < -1.0) {
            *p_r = -1.0;
        }
        if (1.0 < *p_r) {
            *p_r = 1.0;
        }
        p_pcm[j] = (WAVE_DATA)(*p_l * WAVE_MAX);
        p_pcm[j + 1] = (WAVE_DATA)(*p_r * WAVE_MAX);
        *p_l = 0.0;
        *p_r = 0.0;
    }
}

int32
Synth::send_message(byte port, byte* p_msg) {
    auto type = (E_EVENT_TYPE)(*p_msg & 0xF0);
    auto ch = (port << 4) | (*p_msg & 0x0F);
    switch (type) {
    case E_EVENT_TYPE::NOTE_OFF:
        pp_channels[ch]->note_off(p_msg[1]);
        return 3;
    case E_EVENT_TYPE::NOTE_ON:
        pp_channels[ch]->note_on(p_msg[1], p_msg[2]);
        return 3;
    case E_EVENT_TYPE::POLY_KEY:
        return 3;
    case E_EVENT_TYPE::CTRL_CHG:
        pp_channels[ch]->ctrl_change(p_msg[1], p_msg[2]);
        return 3;
    case E_EVENT_TYPE::PROG_CHG:
        pp_channels[ch]->program_change(p_msg[1]);
        return 2;
    case E_EVENT_TYPE::CH_PRESS:
        return 2;
    case E_EVENT_TYPE::PITCH:
        pp_channels[ch]->pitch_bend(p_msg[1], p_msg[2]);
        return 3;
    case E_EVENT_TYPE::SYS_EX:
        switch (p_msg[0]) {
        case 0xF0:
            return sys_ex(p_msg);
        case 0xFF:
            return meta_data(p_msg);
        default:
            return 0;
        }
    default:
        return 0;
    }
}

int32
Synth::sys_ex(byte* p_data) {
    for (int32 i = 0; i < 1024; i++) {
        if (0xF7 == p_data[i]) {
            if (5 <= i && 0x7E == p_data[2] && 0x7F == p_data[3] && 0x09 == p_data[4] && 0x01 == p_data[5]) {
                /* GM reset */
                for (int32 i = 0; i < CHANNEL_COUNT; i++) {
                    pp_channels[i]->init_ctrl();
                }
            }
            if (5 <= i && 0x7E == p_data[2] && 0x7F == p_data[3] && 0x09 == p_data[4] && 0x03 == p_data[5]) {
                /* GM2 reset */
                for (int32 i = 0; i < CHANNEL_COUNT; i++) {
                    pp_channels[i]->init_ctrl();
                }
            }
            if (10 <= i && 0x41 == p_data[2] && 0x42 == p_data[4] && 0x12 == p_data[5] && 0x40 == p_data[6] &&
                0x00 == p_data[7] && 0x7F == p_data[8] && 0x00 == p_data[9] && 0x41 == p_data[10]) {
                /* GS reset */
                for (int32 i = 0; i < CHANNEL_COUNT; i++) {
                    pp_channels[i]->init_ctrl();
                }
            }
            if (8 <= i && 0x43 == p_data[2] && 0x4C == p_data[4] && 0x00 == p_data[5] && 0x00 == p_data[6] &&
                0x7E == p_data[7] && 0x00 == p_data[8]) {
                /* XG reset */
                for (int32 i = 0; i < CHANNEL_COUNT; i++) {
                    pp_channels[i]->init_ctrl();
                }
            }
            return i;
        }
    }
    return 1;
}

int32
Synth::meta_data(byte* p_data) {
    auto type = (E_META_TYPE)p_data[1];
    auto size = p_data[2];

    switch (type) {
    case E_META_TYPE::TEMPO:
        bpm = 60000000.0 / ((p_data[3] << 16) | (p_data[4] << 8) | p_data[5]);
        break;
    case E_META_TYPE::KEY:
        break;
    case E_META_TYPE::MEASURE:
        break;
    default:
        break;
    }

    return size + 3;
}
