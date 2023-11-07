#include <stdlib.h>
#include <math.h>

#include "../inst_list.h"

#include "filter.h"
#include "sampler.h"
#include "channel.h"

/******************************************************************************/
const double Channel::PITCH_MSB[64] = {
    1.00000000, 1.00090294, 1.00180670, 1.00271128, 1.00361667, 1.00452287, 1.00542990, 1.00633775,
    1.00724641, 1.00815590, 1.00906621, 1.00997733, 1.01088929, 1.01180206, 1.01271566, 1.01363008,
    1.01454533, 1.01546141, 1.01637831, 1.01729605, 1.01821461, 1.01913400, 1.02005422, 1.02097527,
    1.02189715, 1.02281986, 1.02374341, 1.02466779, 1.02559301, 1.02651906, 1.02744595, 1.02837367,
    1.02930224, 1.03023164, 1.03116188, 1.03209296, 1.03302488, 1.03395764, 1.03489125, 1.03582569,
    1.03676098, 1.03769712, 1.03863410, 1.03957193, 1.04051060, 1.04145012, 1.04239049, 1.04333171,
    1.04427378, 1.04521670, 1.04616047, 1.04710510, 1.04805057, 1.04899690, 1.04994409, 1.05089213,
    1.05184102, 1.05279077, 1.05374138, 1.05469285, 1.05564518, 1.05659837, 1.05755241, 1.05850732
};
const double Channel::PITCH_LSB[128] = {
    1.00000000, 1.00000705, 1.00001410, 1.00002115, 1.00002820, 1.00003526, 1.00004231, 1.00004936,
    1.00005641, 1.00006346, 1.00007051, 1.00007756, 1.00008462, 1.00009167, 1.00009872, 1.00010577,
    1.00011282, 1.00011988, 1.00012693, 1.00013398, 1.00014103, 1.00014808, 1.00015514, 1.00016219,
    1.00016924, 1.00017629, 1.00018334, 1.00019040, 1.00019745, 1.00020450, 1.00021155, 1.00021861,
    1.00022566, 1.00023271, 1.00023976, 1.00024682, 1.00025387, 1.00026092, 1.00026798, 1.00027503,
    1.00028208, 1.00028914, 1.00029619, 1.00030324, 1.00031029, 1.00031735, 1.00032440, 1.00033145,
    1.00033851, 1.00034556, 1.00035262, 1.00035967, 1.00036672, 1.00037378, 1.00038083, 1.00038788,
    1.00039494, 1.00040199, 1.00040904, 1.00041610, 1.00042315, 1.00043021, 1.00043726, 1.00044432,
    1.00045137, 1.00045842, 1.00046548, 1.00047253, 1.00047959, 1.00048664, 1.00049370, 1.00050075,
    1.00050781, 1.00051486, 1.00052191, 1.00052897, 1.00053602, 1.00054308, 1.00055013, 1.00055719,
    1.00056424, 1.00057130, 1.00057835, 1.00058541, 1.00059246, 1.00059952, 1.00060657, 1.00061363,
    1.00062069, 1.00062774, 1.00063480, 1.00064185, 1.00064891, 1.00065596, 1.00066302, 1.00067007,
    1.00067713, 1.00068419, 1.00069124, 1.00069830, 1.00070535, 1.00071241, 1.00071947, 1.00072652,
    1.00073358, 1.00074064, 1.00074769, 1.00075475, 1.00076180, 1.00076886, 1.00077592, 1.00078297,
    1.00079003, 1.00079709, 1.00080414, 1.00081120, 1.00081826, 1.00082531, 1.00083237, 1.00083943,
    1.00084648, 1.00085354, 1.00086060, 1.00086766, 1.00087471, 1.00088177, 1.00088883, 1.00089589
};

/******************************************************************************/
Channel::Channel(Synth* p_synth, int32 number) {
    mp_synth = p_synth;
    m_param.enable = 1;
    m_param.p_keyboard = (byte*)calloc(128, sizeof(byte));
    m_num = (byte)number;
    mp_buffer_l = (double*)calloc(p_synth->m_buffer_length, sizeof(double));
    mp_buffer_r = (double*)calloc(p_synth->m_buffer_length, sizeof(double));
    m_delay.index = 0;
    m_delay.tap_length = p_synth->m_sample_rate;
    m_delay.p_tap_l = (double*)calloc(m_delay.tap_length, sizeof(double));
    m_delay.p_tap_r = (double*)calloc(m_delay.tap_length, sizeof(double));
    m_chorus.lfo_u = 1.0;
    m_chorus.lfo_v = -0.5;
    m_chorus.lfo_w = -0.5;
    m_state = E_STATE::FREE;
    init_ctrl();
}

Channel::~Channel() {
    if (nullptr != m_param.p_keyboard) {
        free(m_param.p_keyboard);
        m_param.p_keyboard = nullptr;
    }
    if (nullptr != mp_buffer_l) {
        free(mp_buffer_l);
        mp_buffer_l = nullptr;
    }
    if (nullptr != mp_buffer_r) {
        free(mp_buffer_r);
        mp_buffer_r = nullptr;
    }
    if (nullptr != m_delay.p_tap_l) {
        free(m_delay.p_tap_l);
        m_delay.p_tap_l = nullptr;
    }
    if (nullptr != m_delay.p_tap_r) {
        free(m_delay.p_tap_r);
        m_delay.p_tap_r = nullptr;
    }
}

/******************************************************************************/
void
Channel::set_amp(byte vol, byte exp) {
    m_param.vol = vol;
    m_param.exp = exp;
    m_target_amp = vol * vol * exp * exp / 260144641.0;
}

void
Channel::set_pan(byte value) {
    m_param.pan = value;
    m_target_pan_re = cos((value - 64) * 1.570796 / 127.0);
    m_target_pan_im = sin((value - 64) * 1.570796 / 127.0);
}

void
Channel::set_damper(byte value) {
    if (value < 64) {
        for (int32 i = 0; i < SAMPLER_COUNT; ++i) {
            auto p_smpl = mp_synth->mpp_samplers[i];
            if (Sampler::E_STATE::HOLD == p_smpl->m_state) {
                p_smpl->m_state = Sampler::E_STATE::RELEASE;
            }
        }
        for (int32 i = 0; i < 128; ++i) {
            if ((byte)E_KEY_STATE::HOLD == m_param.p_keyboard[i]) {
                m_param.p_keyboard[i] = static_cast<byte>(E_KEY_STATE::FREE);
            }
        }
    }
    m_param.damper = value;
}

void
Channel::set_res(byte value) {
    m_param.resonance = value;
}

void
Channel::set_cut(byte value) {
    m_param.cutoff = value;
}

void
Channel::set_rpn() {
    auto type = static_cast<E_RPN>(m_rpn_lsb | m_rpn_msb << 8);
    switch (type) {
    case E_RPN::BEND_RANGE:
        m_param.bend_range = m_data_msb;
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

    m_param.rev_send = 0;

    m_param.cho_send = 0;
    m_chorus.send = m_param.cho_send / 127.0;
    m_chorus.depth = 30 * 0.001;
    m_chorus.rate = 100 * 0.006283 / 1.732 * mp_synth->m_delta_time;
    
    m_param.del_send = 0;
    m_delay.send = m_param.del_send / 127.0 * 0.9;
    m_delay.cross = 64 / 127.0;
    m_delay.time = static_cast<int32>(mp_synth->m_sample_rate * 200 * 0.001);
    
    set_res(64);
    set_cut(64);

    m_param.release = 64;
    m_param.attack = 64;

    m_param.vib_rate = 64;
    m_param.vib_depth = 64;
    m_param.vib_delay = 64;

    m_param.bend_range = 2;
    m_param.pitch = 0;
    m_pitch = 1.0;

    m_rpn_lsb = 0xFF;
    m_rpn_msb = 0xFF;
    m_nrpn_lsb = 0xFF;
    m_nrpn_msb = 0xFF;

    m_param.is_drum = (m_num % 16 == 9) ? 1 : 0;
    m_param.bank_msb = 0;
    m_param.bank_lsb = 0;
    m_param.prog_num = 0;
    program_change(0);
}

void
Channel::all_reset() {
    set_amp(m_param.vol, 100);
    set_pan(64);
    set_damper(0);

    m_param.pitch = 0;
    m_pitch = 1.0;

    m_rpn_lsb = 0xFF;
    m_rpn_msb = 0xFF;
    m_nrpn_lsb = 0xFF;
    m_nrpn_msb = 0xFF;
}

void
Channel::note_off(byte note_num) {
    for (int32 i = 0; i < SAMPLER_COUNT; ++i) {
        auto p_smpl = mp_synth->mpp_samplers[i];
        auto ch_param = *mp_synth->m_export_values.pp_channel_params[p_smpl->m_channel_num];
        if (p_smpl->m_state < Sampler::E_STATE::PRESS ||
            (ch_param.is_drum && !p_smpl->m_loop_enable)) {
            continue;
        }
        if (p_smpl->m_channel_num == m_num && p_smpl->m_note_num == note_num) {
            if (m_param.damper < 64) {
                p_smpl->m_state = Sampler::E_STATE::RELEASE;
            } else {
                p_smpl->m_state = Sampler::E_STATE::HOLD;
            }
        }
    }
    if (m_param.damper < 64) {
        m_param.p_keyboard[note_num] = static_cast<byte>(E_KEY_STATE::FREE);
    } else {
        m_param.p_keyboard[note_num] = static_cast<byte>(E_KEY_STATE::HOLD);
    }
}

void
Channel::note_on(byte note_num, byte velocity) {
    if (0 == velocity) {
        note_off(note_num);
        return;
    }
    if (E_STATE::FREE == m_state) {
        m_state = E_STATE::STANDBY;
    }
    m_param.p_keyboard[note_num] = static_cast<byte>(E_KEY_STATE::PRESS);
    for (uint32 i = 0; i < SAMPLER_COUNT; ++i) {
        auto p_smpl = mp_synth->mpp_samplers[i];
        if (p_smpl->m_channel_num == m_num && p_smpl->m_note_num == note_num &&
            Sampler::E_STATE::PRESS <= p_smpl->m_state) {
            p_smpl->m_state = Sampler::E_STATE::PURGE;
        }
    }
    auto p_inst_list = mp_synth->mp_inst_list;
    auto pp_layer = p_inst_list->mppLayerList + mp_inst->layerIndex;
    for (uint32 idxl = 0; idxl < mp_inst->layerCount; idxl++) {
        auto p_layer = pp_layer[idxl];
        auto pp_region = p_inst_list->mppRegionList + p_layer->regionIndex;
        for (uint32 idxr = 0; idxr < p_layer->regionCount; idxr++) {
            auto p_region = pp_region[idxr];
            if (p_region->keyLow <= note_num && note_num <= p_region->keyHigh &&
                p_region->velocityLow <= velocity && velocity <= p_region->velocityHigh) {
                for (uint32 idxs = 0; idxs < SAMPLER_COUNT; idxs++) {
                    auto p_smpl = mp_synth->mpp_samplers[idxs];
                    if (Sampler::E_STATE::FREE == p_smpl->m_state) {
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
        m_param.bank_msb = value;
        break;
    case E_CTRL_TYPE::BANK_LSB:
        m_param.bank_lsb = value;
        break;

    case E_CTRL_TYPE::VOLUME:
        set_amp(value, m_param.exp);
        break;
    case E_CTRL_TYPE::PAN:
        set_pan(value);
        break;
    case E_CTRL_TYPE::EXPRESSION:
        set_amp(m_param.vol, value);
        break;

    case E_CTRL_TYPE::MODULATION:
        m_param.mod = value;
        break;

    case E_CTRL_TYPE::HOLD:
        set_damper(value);
        break;
    case E_CTRL_TYPE::RELEACE:
        m_param.release = value;
        break;
    case E_CTRL_TYPE::ATTACK:
        m_param.attack = value;
        break;

    case E_CTRL_TYPE::RESONANCE:
        set_res(value);
        break;
    case E_CTRL_TYPE::CUTOFF:
        set_cut(value);
        break;

    case E_CTRL_TYPE::VIB_RATE:
        m_param.vib_rate = value;
        break;
    case E_CTRL_TYPE::VIB_DEPTH:
        m_param.vib_depth = value;
        break;
    case E_CTRL_TYPE::VIB_DELAY:
        m_param.vib_delay = value;
        break;

    case E_CTRL_TYPE::REVERB:
        m_param.rev_send = value;
        break;
    case E_CTRL_TYPE::CHORUS:
        m_param.cho_send = value;
        m_chorus.send = value / 127.0;
        break;
    case E_CTRL_TYPE::DELAY:
        m_param.del_send = value;
        m_delay.send = value / 127.0 * 0.9;
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

    case E_CTRL_TYPE::ALL_SOUND_OFF:
    case E_CTRL_TYPE::ALL_NOTE_OFF:
        m_param.enable = 64 <= value ? 0 : 1;
        break;

    case E_CTRL_TYPE::DRUM:
        m_param.is_drum = 64 <= value ? 1 : 0;
        break;
    }
}

void
Channel::program_change(byte value) {
    m_param.prog_num = value;
    mp_inst = mp_synth->mp_inst_list->GetInstInfo(
        m_param.is_drum,
        m_param.bank_lsb,
        m_param.bank_msb,
        m_param.prog_num
    );
    m_param.p_name = (byte*)mp_inst->pName;
}

void
Channel::pitch_bend(byte lsb, byte msb) {
    m_param.pitch = ((msb << 7) | lsb) - 8192;
    auto temp = m_param.pitch * m_param.bend_range;
    if (temp < 0) {
        temp = -temp;
        m_pitch = 1.0 / (SEMITONE[temp >> 13] * PITCH_MSB[(temp >> 7) % 64] * PITCH_LSB[temp % 128]);
    } else {
        m_pitch = SEMITONE[temp >> 13] * PITCH_MSB[(temp >> 7) % 64] * PITCH_LSB[temp % 128];
    }
}

void
Channel::step(double* p_output_l, double* p_output_r) {
    for (int32 i = 0; i < mp_synth->m_buffer_length; i++) {
        auto output_l = mp_buffer_l[i] * m_current_pan_re - mp_buffer_r[i] * m_current_pan_im;
        auto output_r = mp_buffer_l[i] * m_current_pan_im + mp_buffer_r[i] * m_current_pan_re;
        output_l *= m_current_amp;
        output_r *= m_current_amp;
        m_current_amp += (m_target_amp - m_current_amp) * 0.02;
        m_current_pan_re += (m_target_pan_re - m_current_pan_re) * 0.02;
        m_current_pan_im += (m_target_pan_im - m_current_pan_im) * 0.02;
        mp_buffer_l[i] = 0.0;
        mp_buffer_r[i] = 0.0;

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
                - ((0.5 + 0.5 * m_chorus.lfo_u) * m_chorus.depth * 0.999 + 0.001) * mp_synth->m_sample_rate;
            auto tv = m_delay.index
                - ((0.5 + 0.5 * m_chorus.lfo_v) * m_chorus.depth * 0.999 + 0.001) * mp_synth->m_sample_rate;
            auto tw = m_delay.index
                - ((0.5 + 0.5 * m_chorus.lfo_w) * m_chorus.depth * 0.999 + 0.001) * mp_synth->m_sample_rate;
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
            auto delta = RMS_ATTENUTE * mp_synth->m_delta_time;
            auto attenute = 1.0 - delta;
            auto rms_l = m_param.rms_l * attenute;
            auto rms_r = m_param.rms_r * attenute;
            m_param.rms_l = rms_l + output_l * output_l * delta;
            m_param.rms_r = rms_r + output_r * output_r * delta;
        }

        switch (m_state) {
        case E_STATE::STANDBY:
            if (ACTIVE_THRESHOLD <= sqrt(m_param.rms_l) || ACTIVE_THRESHOLD <= sqrt(m_param.rms_r)) {
                m_state = E_STATE::ACTIVE;
            }
            break;
        case E_STATE::ACTIVE:
            if (sqrt(m_param.rms_l) < FREE_THRESHOLD && sqrt(m_param.rms_r) < FREE_THRESHOLD) {
                m_state = E_STATE::FREE;
            }
            break;
        }
    }
}
