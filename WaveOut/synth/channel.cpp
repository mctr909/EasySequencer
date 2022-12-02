#include <math.h>

#include "channel_params.h"
#include "filter.h"
#include "sampler.h"
#include "synth.h"
#include "../inst/inst_list.h"

#include "channel.h"

Channel::Channel(Synth* p_synth, int32 number) {
    mp_synth = p_synth;
    this->number = (byte)number;
    param.p_keyboard = (byte*)calloc(1, sizeof(byte) * 128);
    p_input_l = (double*)calloc(p_synth->buffer_length, sizeof(double));
    p_input_r = (double*)calloc(p_synth->buffer_length, sizeof(double));
    m_delay.index = 0;
    m_delay.tap_length = p_synth->sample_rate;
    m_delay.p_tap_l = (double*)calloc(m_delay.tap_length, sizeof(double));
    m_delay.p_tap_r = (double*)calloc(m_delay.tap_length, sizeof(double));
    m_chorus.lfo_u = 1.0;
    m_chorus.lfo_v = -0.5;
    m_chorus.lfo_w = -0.5;

    state = E_STATE::FREE;
    init_ctrl();
}

Channel::~Channel() {
    if (NULL != p_input_l) {
        free(p_input_l);
        p_input_l = NULL;
    }
    if (NULL != p_input_r) {
        free(p_input_r);
        p_input_r = NULL;
    }
    if (NULL != m_delay.p_tap_l) {
        free(m_delay.p_tap_l);
        m_delay.p_tap_l = NULL;
    }
    if (NULL != m_delay.p_tap_r) {
        free(m_delay.p_tap_r);
        m_delay.p_tap_r = NULL;
    }
    if (NULL != param.p_keyboard) {
        free(param.p_keyboard);
        param.p_keyboard = NULL;
    }
}

/******************************************************************************/
void
Channel::set_amp(byte vol, byte exp) {
    param.vol = vol;
    param.exp = exp;
    m_target_amp = vol * vol * exp * exp / 260144641.0;
}

void
Channel::set_pan(byte value) {
    param.pan = value;
    m_target_pan_re = cos((value - 64) * 1.570796 / 127.0);
    m_target_pan_im = sin((value - 64) * 1.570796 / 127.0);
}

void
Channel::set_damper(byte value) {
    if (value < 64) {
        for (int32 i = 0; i < SAMPLER_COUNT; ++i) {
            auto p_smpl = mp_synth->pp_samplers[i];
            if (Sampler::E_STATE::HOLD == p_smpl->state) {
                p_smpl->state = Sampler::E_STATE::RELEASE;
            }
        }
        for (int32 i = 0; i < 128; ++i) {
            if ((byte)E_KEY_STATE::HOLD == param.p_keyboard[i]) {
                param.p_keyboard[i] = static_cast<byte>(E_KEY_STATE::FREE);
            }
        }
    }
    param.damper = value;
}

void
Channel::set_res(byte value) {
    param.resonance = value;
}

void
Channel::set_cut(byte value) {
    param.cutoff = value;
}

void
Channel::set_rpn() {
    auto type = static_cast<E_RPN>(m_rpn_lsb | m_rpn_msb << 8);
    switch (type) {
    case E_RPN::BEND_RANGE:
        param.bend_range = m_data_msb;
        break;
    case E_RPN::VIB_DEPTH_RANGE:
        break;
    default:
        break;
    }
}

void
Channel::set_nrpn() {
}

/******************************************************************************/
void
Channel::init_ctrl() {
    set_amp(100, 100);
    set_pan(64);

    set_damper(0);

    param.rev_send = 0;

    param.cho_send = 0;
    m_chorus.send = param.cho_send / 127.0;
    m_chorus.depth = 30 * 0.001;
    m_chorus.rate = 100 * 0.006283 / 1.732 * mp_synth->delta_time;
    
    param.del_send = 0;
    m_delay.send = param.del_send / 128.0;
    m_delay.cross = 64 / 127.0;
    m_delay.time = static_cast<long>(mp_synth->sample_rate * 200 * 0.001);
    
    set_res(64);
    set_cut(64);

    param.release = 64;
    param.attack = 64;

    param.vib_rate = 64;
    param.vib_depth = 64;
    param.vib_delay = 64;

    param.bend_range = 2;
    param.pitch = 0;
    pitch = 1.0;

    m_rpn_lsb = 0xFF;
    m_rpn_msb = 0xFF;
    m_nrpn_lsb = 0xFF;
    m_nrpn_msb = 0xFF;

    param.is_drum = number == 9 ? 1 : 0;
    param.bank_msb = 0;
    param.bank_lsb = 0;
    param.prog_num = 0;
    program_change(0);

    param.enable = 1;
}

void
Channel::all_reset() {
    set_amp(param.vol, 100);
    set_pan(64);
    set_damper(0);

    param.pitch = 0;
    pitch = 1.0;

    m_rpn_lsb = 0xFF;
    m_rpn_msb = 0xFF;
    m_nrpn_lsb = 0xFF;
    m_nrpn_msb = 0xFF;
}

void
Channel::note_off(byte note_num) {
    for (int32 i = 0; i < SAMPLER_COUNT; ++i) {
        auto p_smpl = mp_synth->pp_samplers[i];
        auto ch_param = *mp_synth->pp_channel_params[p_smpl->channel_num];
        if (p_smpl->state < Sampler::E_STATE::PRESS ||
            (ch_param.is_drum && !p_smpl->loop_enable)) {
            continue;
        }
        if (p_smpl->channel_num == number && p_smpl->note_num == note_num) {
            if (param.damper < 64) {
                p_smpl->state = Sampler::E_STATE::RELEASE;
            } else {
                p_smpl->state = Sampler::E_STATE::HOLD;
            }
        }
    }
    if (param.damper < 64) {
        param.p_keyboard[note_num] = static_cast<byte>(E_KEY_STATE::FREE);
    } else {
        param.p_keyboard[note_num] = static_cast<byte>(E_KEY_STATE::HOLD);
    }
}

void
Channel::note_on(byte note_num, byte velocity) {
    if (0 == velocity) {
        note_off(note_num);
        return;
    }
    if (E_STATE::FREE == state) {
        state = E_STATE::STANDBY;
    }
    param.p_keyboard[note_num] = static_cast<byte>(E_KEY_STATE::PRESS);
    for (uint32 i = 0; i < SAMPLER_COUNT; ++i) {
        auto p_smpl = mp_synth->pp_samplers[i];
        if (p_smpl->channel_num == number && p_smpl->note_num == note_num &&
            Sampler::E_STATE::PRESS <= p_smpl->state) {
            p_smpl->state = Sampler::E_STATE::PURGE;
        }
    }
    auto p_inst_list = mp_synth->p_inst_list;
    auto pp_layer = p_inst_list->mppLayerList + p_inst->layerIndex;
    for (uint32 idxl = 0; idxl < p_inst->layerCount; idxl++) {
        auto p_layer = pp_layer[idxl];
        auto pp_region = p_inst_list->mppRegionList + p_layer->regionIndex;
        for (uint32 idxr = 0; idxr < p_layer->regionCount; idxr++) {
            auto p_region = pp_region[idxr];
            if (p_region->keyLow <= note_num && note_num <= p_region->keyHigh &&
                p_region->velocityLow <= velocity && velocity <= p_region->velocityHigh) {
                for (uint32 idxs = 0; idxs < SAMPLER_COUNT; idxs++) {
                    auto p_smpl = mp_synth->pp_samplers[idxs];
                    if (Sampler::E_STATE::FREE == p_smpl->state) {
                        p_smpl->note_on(this, p_layer, p_region, note_num, velocity);
                        break;
                    }
                }
                break;
            }
        }
    }
}

void
Channel::ctrl_change(byte type, byte value) {
    switch ((E_CTRL_TYPE)type) {
    case E_CTRL_TYPE::BANK_MSB:
        param.bank_msb = value;
        break;
    case E_CTRL_TYPE::BANK_LSB:
        param.bank_lsb = value;
        break;

    case E_CTRL_TYPE::VOLUME:
        set_amp(value, param.exp);
        break;
    case E_CTRL_TYPE::PAN:
        set_pan(value);
        break;
    case E_CTRL_TYPE::EXPRESSION:
        set_amp(param.vol, value);
        break;

    case E_CTRL_TYPE::MODULATION:
        param.mod = value;
        break;

    case E_CTRL_TYPE::HOLD:
        set_damper(value);
        break;
    case E_CTRL_TYPE::RELEACE:
        param.release = value;
        break;
    case E_CTRL_TYPE::ATTACK:
        param.attack = value;
        break;

    case E_CTRL_TYPE::RESONANCE:
        set_res(value);
        break;
    case E_CTRL_TYPE::CUTOFF:
        set_cut(value);
        break;

    case E_CTRL_TYPE::VIB_RATE:
        param.vib_rate = value;
        break;
    case E_CTRL_TYPE::VIB_DEPTH:
        param.vib_depth = value;
        break;
    case E_CTRL_TYPE::VIB_DELAY:
        param.vib_delay = value;
        break;

    case E_CTRL_TYPE::REVERB:
        param.rev_send = value;
        break;
    case E_CTRL_TYPE::CHORUS:
        param.cho_send = value;
        m_chorus.send = value / 127.0;
        break;
    case E_CTRL_TYPE::DELAY:
        param.del_send = value;
        m_delay.send = value / 128.0;
        break;

    case E_CTRL_TYPE::NRPN_LSB:
        m_nrpn_lsb = value;
        break;
    case E_CTRL_TYPE::NRPN_MSB:
        m_nrpn_msb = value;
        break;
    case E_CTRL_TYPE::RPN_LSB:
        m_rpn_lsb = value;
        break;
    case E_CTRL_TYPE::RPN_MSB:
        m_rpn_msb = value;
        break;
    case E_CTRL_TYPE::DATA_MSB:
        m_data_msb = value;
        set_rpn();
        set_nrpn();
        break;
    case E_CTRL_TYPE::DATA_LSB:
        m_data_lsb = value;
        set_rpn();
        set_nrpn();
        break;

    case E_CTRL_TYPE::ALL_RESET:
        all_reset();
        break;
    }
}

void
Channel::program_change(byte value) {
    param.prog_num = value;
    p_inst = mp_synth->p_inst_list->GetInstInfo(param.is_drum, param.bank_lsb, param.bank_msb, param.prog_num);
    param.p_name = (byte*)p_inst->pName;
}

void
Channel::pitch_bend(byte lsb, byte msb) {
    param.pitch = ((msb << 7) | lsb) - 8192;
    auto temp = param.pitch * param.bend_range;
    if (temp < 0) {
        temp = -temp;
        pitch = 1.0 / (SEMITONE[temp >> 13] * PITCH_MSB[(temp >> 7) % 64] * PITCH_LSB[temp % 128]);
    } else {
        pitch = SEMITONE[temp >> 13] * PITCH_MSB[(temp >> 7) % 64] * PITCH_LSB[temp % 128];
    }
}

void
Channel::step(double* p_output_l, double* p_output_r) {
    for (int32 i = 0; i < mp_synth->buffer_length; i++) {
        auto output_l = p_input_l[i] * m_current_pan_re - p_input_r[i] * m_current_pan_im;
        auto output_r = p_input_l[i] * m_current_pan_im + p_input_r[i] * m_current_pan_re;
        output_l *= m_current_amp;
        output_r *= m_current_amp;
        m_current_amp += (m_target_amp - m_current_amp) * 0.02;
        m_current_pan_re += (m_target_pan_re - m_current_pan_re) * 0.02;
        m_current_pan_im += (m_target_pan_im - m_current_pan_im) * 0.02;
        p_input_l[i] = 0.0;
        p_input_r[i] = 0.0;

        /* delay */
        {
            auto delay_index = m_delay.index - m_delay.time;
            if (delay_index < 0) {
                delay_index += m_delay.tap_length;
            }
            auto tap_l = m_delay.p_tap_l[delay_index];
            auto tap_r = m_delay.p_tap_r[delay_index];
            auto del_l = tap_l * (1.0 - m_delay.cross) + tap_r * m_delay.cross;
            auto del_r = tap_r * (1.0 - m_delay.cross) + tap_l * m_delay.cross;
            output_l += del_l * m_delay.send;
            output_r += del_r * m_delay.send;
            m_delay.p_tap_l[m_delay.index] = output_l;
            m_delay.p_tap_r[m_delay.index] = output_r;
            m_delay.index++;
            if (m_delay.tap_length <= m_delay.index) {
                m_delay.index -= m_delay.tap_length;
            }
        }

        /* chorus */
        {
            auto tu = m_delay.index
                - ((0.5 + 0.5 * m_chorus.lfo_u) * m_chorus.depth * 0.999 + 0.001) * mp_synth->sample_rate;
            auto tv = m_delay.index
                - ((0.5 + 0.5 * m_chorus.lfo_v) * m_chorus.depth * 0.999 + 0.001) * mp_synth->sample_rate;
            auto tw = m_delay.index
                - ((0.5 + 0.5 * m_chorus.lfo_w) * m_chorus.depth * 0.999 + 0.001) * mp_synth->sample_rate;
            if (tu < 0.0) {
                tu += m_delay.tap_length;
            }
            if (tv < 0.0) {
                tv += m_delay.tap_length;
            }
            if (tw < 0.0) {
                tw += m_delay.tap_length;
            }
            auto idx_ua = static_cast<int32>(tu);
            auto idx_va = static_cast<int32>(tv);
            auto idx_wa = static_cast<int32>(tw);
            auto idx_ub = idx_ua + 1;
            auto idx_vb = idx_va + 1;
            auto idx_wb = idx_wa + 1;
            auto du = tu - idx_ua;
            auto dv = tv - idx_va;
            auto dw = tw - idx_wa;
            if (m_delay.tap_length <= idx_ub) {
                idx_ub -= m_delay.tap_length;
            }
            if (m_delay.tap_length <= idx_vb) {
                idx_vb -= m_delay.tap_length;
            }
            if (m_delay.tap_length <= idx_wb) {
                idx_wb -= m_delay.tap_length;
            }
            auto cho_l
                = (m_delay.p_tap_l[idx_ua] * (1.0 - du) + m_delay.p_tap_l[idx_ub] * du) * 0.5
                + (m_delay.p_tap_l[idx_va] * (1.0 - dv) + m_delay.p_tap_l[idx_vb] * dv) * 0.5;
            auto cho_r
                = (m_delay.p_tap_r[idx_ua] * (1.0 - du) + m_delay.p_tap_r[idx_ub] * du) * 0.5
                + (m_delay.p_tap_r[idx_wa] * (1.0 - dw) + m_delay.p_tap_r[idx_wb] * dw) * 0.5;
            output_l += cho_l * m_chorus.send;
            output_r += cho_r * m_chorus.send;
            m_chorus.lfo_u += (m_chorus.lfo_v - m_chorus.lfo_w) * m_chorus.rate;
            m_chorus.lfo_v += (m_chorus.lfo_w - m_chorus.lfo_u) * m_chorus.rate;
            m_chorus.lfo_w += (m_chorus.lfo_u - m_chorus.lfo_v) * m_chorus.rate;
        }

        p_output_l[i] += output_l;
        p_output_r[i] += output_r;

        /* meter */
        {
            auto delta = RMS_ATTENUTE * mp_synth->delta_time;
            auto attenute = 1.0 - delta;
            auto rms_l = param.rms_l * attenute;
            auto rms_r = param.rms_r * attenute;
            param.rms_l = rms_l + output_l * output_l * delta;
            param.rms_r = rms_r + output_r * output_r * delta;
            attenute = 1.0 - PEAK_ATTENUTE * mp_synth->delta_time;
            auto peak_l = param.peak_l * attenute;
            auto peak_r = param.peak_r * attenute;
            param.peak_l = fmax(peak_l, fabs(output_l));
            param.peak_r = fmax(peak_r, fabs(output_r));
        }

        switch (state) {
        case E_STATE::STANDBY:
            if (START_AMP <= sqrt(param.rms_l) || START_AMP <= sqrt(param.rms_r)) {
                state = E_STATE::ACTIVE;
            }
            break;
        case E_STATE::ACTIVE:
            if (sqrt(param.rms_l) < STOP_AMP && sqrt(param.rms_r) < STOP_AMP) {
                state = E_STATE::FREE;
            }
            break;
        }
    }
}
