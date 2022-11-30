#include <math.h>

#include "channel_const.h"
#include "channel_params.h"
#include "sampler.h"
#include "filter.h"
#include "../inst/inst_list.h"
#include "../message_reciever.h"

#include "channel.h"

Channel::Channel(SYSTEM_VALUE *pSystem_value, int32 number) {
    mpSystem_value = pSystem_value;
    this->number = (byte)number;
    param.pKeyboard = (byte*)calloc(1, sizeof(byte) * 128);

    pInput_l = (double*)calloc(pSystem_value->buffer_length, sizeof(double));
    pInput_r = (double*)calloc(pSystem_value->buffer_length, sizeof(double));

    delay.write_index = 0;
    delay.tap_length = pSystem_value->sample_rate;
    delay.pTap_l = (double*)calloc(delay.tap_length, sizeof(double));
    delay.pTap_r = (double*)calloc(delay.tap_length, sizeof(double));
    chorus.lfo_u = 1.0;
    chorus.lfo_v = -0.5;
    chorus.lfo_w = -0.5;
    current_amp = 10000 / 16129.0;
    current_pan_re = 1.0;
    current_pan_im = 0.0;

    init_ctrl();
}

Channel::~Channel() {
    if (NULL != pInput_l) {
        free(pInput_l);
        pInput_l = NULL;
    }
    if (NULL != pInput_r) {
        free(pInput_r);
        pInput_r = NULL;
    }
    if (NULL != delay.pTap_l) {
        free(delay.pTap_l);
        delay.pTap_l = NULL;
    }
    if (NULL != delay.pTap_r) {
        free(delay.pTap_r);
        delay.pTap_r = NULL;
    }
    if (NULL != param.pKeyboard) {
        free(param.pKeyboard);
        param.pKeyboard = NULL;
    }
}

/******************************************************************************/
void
Channel::set_amp(byte vol, byte exp) {
    param.vol = vol;
    param.exp = exp;
    target_amp = vol * vol * exp * exp / 260144641.0;
}

void
Channel::set_pan(byte value) {
    param.pan = value;
    target_pan_re = cos((value - 64) * 1.570796 / 127.0);
    target_pan_im = sin((value - 64) * 1.570796 / 127.0);
}

void
Channel::set_damper(byte value) {
    if (value < 64) {
        for (int32 s = 0; s < SAMPLER_COUNT; ++s) {
            auto pSmpl = mpSystem_value->ppSampler[s];
            if (Sampler::E_STATE::HOLD == pSmpl->state) {
                pSmpl->state = Sampler::E_STATE::RELEASE;
            }
        }
        for (int32 n = 0; n < 128; ++n) {
            if ((byte)E_KEY_STATE::HOLD == param.pKeyboard[n]) {
                param.pKeyboard[n] = (byte)E_KEY_STATE::FREE;
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
    switch (rpn_lsb | rpn_msb << 8) {
    case 0x0000:
        param.bend_range = data_msb;
        break;
    default:
        break;
    }
    rpn_lsb = 0xFF;
    rpn_msb = 0xFF;
}

void
Channel::set_nrpn() {
    //switch (mNrpnLSB | mNrpnMSB << 8) {
    //default:
    //    break;
    //}
    nrpn_lsb = 0xFF;
    nrpn_msb = 0xFF;
}

/******************************************************************************/
void
Channel::init_ctrl() {
    set_amp(100, 100);
    set_pan(64);

    set_damper(0);

    param.rev_send = 0;
    param.cho_send = 0;
    chorus.send = param.cho_send / 127.0;
    chorus.pan_a = (1.0 - param.rev_send / 127.0) / 3.0;
    chorus.pan_b = (1.0 + param.rev_send / 127.0) / 3.0;
    chorus.depth = 20 * 0.001;
    chorus.rate = 10 * 0.06283 / 1.732 * mpSystem_value->delta_time;
    
    param.del_send = 0;
    delay.send = param.del_send / 128.0;
    delay.cross = 64 / 127.0;
    delay.time = static_cast<long>(mpSystem_value->sample_rate * 200 * 0.001);
    
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

    rpn_lsb = 0xFF;
    rpn_msb = 0xFF;
    nrpn_lsb = 0xFF;
    nrpn_msb = 0xFF;

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

    rpn_lsb = 0xFF;
    rpn_msb = 0xFF;
    nrpn_lsb = 0xFF;
    nrpn_msb = 0xFF;
}

void
Channel::note_off(byte note_num) {
    for (int32 s = 0; s < SAMPLER_COUNT; ++s) {
        auto pSmpl = mpSystem_value->ppSampler[s];
        auto pChParam = mpSystem_value->ppChannel_params[pSmpl->channel_num];
        if (pSmpl->state < Sampler::E_STATE::PRESS ||
            (pChParam->is_drum && !pSmpl->loop_enable)) {
            continue;
        }
        if (pSmpl->channel_num == number && pSmpl->note_num == note_num) {
            if (param.damper < 64) {
                pSmpl->state = Sampler::E_STATE::RELEASE;
            } else {
                pSmpl->state = Sampler::E_STATE::HOLD;
            }
        }
    }
    if (param.damper < 64) {
        param.pKeyboard[note_num] = (byte)E_KEY_STATE::FREE;
    } else {
        param.pKeyboard[note_num] = (byte)E_KEY_STATE::HOLD;
    }
}

void
Channel::note_on(byte note_num, byte velocity) {
    if (0 == velocity) {
        note_off(note_num);
        return;
    }
    param.pKeyboard[note_num] = (byte)E_KEY_STATE::PRESS;
    for (uint32 idxS = 0; idxS < SAMPLER_COUNT; idxS++) {
        auto pSmpl = mpSystem_value->ppSampler[idxS];
        if (pSmpl->channel_num == number && pSmpl->note_num == note_num &&
            Sampler::E_STATE::PRESS <= pSmpl->state) {
            pSmpl->state = Sampler::E_STATE::PURGE;
        }
    }
    auto cInst = mpSystem_value->cInst_list;
    auto ppLayer = cInst->mppLayerList + mpInst->layerIndex;
    for (uint32 idxL = 0; idxL < mpInst->layerCount; idxL++) {
        auto pLayer = ppLayer[idxL];
        auto ppRegion = cInst->mppRegionList + pLayer->regionIndex;
        for (uint32 idxR = 0; idxR < pLayer->regionCount; idxR++) {
            auto pRegion = ppRegion[idxR];
            if (pRegion->keyLow <= note_num && note_num <= pRegion->keyHigh &&
                pRegion->velocityLow <= velocity && velocity <= pRegion->velocityHigh) {
                auto pWave = cInst->mppWaveList[pRegion->waveIndex];
                for (uint32 idxS = 0; idxS < SAMPLER_COUNT; idxS++) {
                    auto pSmpl = mpSystem_value->ppSampler[idxS];
                    if (Sampler::E_STATE::FREE == pSmpl->state) {
                        pSmpl->note_on(this, pLayer, pRegion, note_num, velocity);
                        break;
                    }
                }
                break;
            }
        }
    }
}

void
Channel::ctrl_change(byte type, byte b1) {
    switch ((E_CTRL_TYPE)type) {
    case E_CTRL_TYPE::BANK_MSB:
        param.bank_msb = b1;
        break;
    case E_CTRL_TYPE::BANK_LSB:
        param.bank_lsb = b1;
        break;

    case E_CTRL_TYPE::VOLUME:
        set_amp(b1, param.exp);
        break;
    case E_CTRL_TYPE::PAN:
        set_pan(b1);
        break;
    case E_CTRL_TYPE::EXPRESSION:
        set_amp(param.vol, b1);
        break;

    case E_CTRL_TYPE::MODULATION:
        param.mod = b1;
        break;

    case E_CTRL_TYPE::HOLD:
        set_damper(b1);
        break;
    case E_CTRL_TYPE::RELEACE:
        param.release = b1;
        break;
    case E_CTRL_TYPE::ATTACK:
        param.attack = b1;
        break;

    case E_CTRL_TYPE::RESONANCE:
        set_res(b1);
        break;
    case E_CTRL_TYPE::CUTOFF:
        set_cut(b1);
        break;

    case E_CTRL_TYPE::VIB_RATE:
        param.vib_rate = b1;
        break;
    case E_CTRL_TYPE::VIB_DEPTH:
        param.vib_depth = b1;
        break;
    case E_CTRL_TYPE::VIB_DELAY:
        param.vib_delay = b1;
        break;

    case E_CTRL_TYPE::REVERB:
        param.rev_send = b1;
        chorus.pan_a = (1.0 - b1 / 127.0) / 3.0;
        chorus.pan_b = (1.0 + b1 / 127.0) / 3.0;
        break;
    case E_CTRL_TYPE::CHORUS:
        param.cho_send = b1;
        chorus.send = b1 / 128.0;
        break;
    case E_CTRL_TYPE::DELAY:
        param.del_send = b1;
        delay.send = b1 / 127.0;
        break;

    case E_CTRL_TYPE::NRPN_LSB:
        nrpn_lsb = b1;
        break;
    case E_CTRL_TYPE::NRPN_MSB:
        nrpn_msb = b1;
        break;
    case E_CTRL_TYPE::RPN_LSB:
        rpn_lsb = b1;
        break;
    case E_CTRL_TYPE::RPN_MSB:
        rpn_msb = b1;
        break;
    case E_CTRL_TYPE::DATA_MSB:
        data_msb = b1;
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
    mpInst = mpSystem_value->cInst_list->GetInstInfo(param.is_drum, param.bank_lsb, param.bank_msb, param.prog_num);
    param.pName = (byte*)mpInst->pName;
}

void
Channel::pitch_bend(int16 pitch) {
    param.pitch = pitch;
    auto temp = param.pitch * param.bend_range;
    if (temp < 0) {
        temp = -temp;
        this->pitch = 1.0 / (SemiTone[temp >> 13] * PitchMSB[(temp >> 7) % 64] * PitchLSB[temp % 128]);
    } else {
        this->pitch = SemiTone[temp >> 13] * PitchMSB[(temp >> 7) % 64] * PitchLSB[temp % 128];
    }
}

void
Channel::step(double* pOutput_l, double* pOutput_r) {
    for (int32 i = 0; i < mpSystem_value->buffer_length; i++) {
        auto output_l = pInput_l[i] * current_pan_re - pInput_r[i] * current_pan_im;
        auto output_r = pInput_l[i] * current_pan_im + pInput_r[i] * current_pan_re;
        output_l *= current_amp;
        output_r *= current_amp;
        current_amp += (target_amp - current_amp) * 0.02;
        current_pan_re += (target_pan_re - current_pan_re) * 0.02;
        current_pan_im += (target_pan_im - current_pan_im) * 0.02;
        pInput_l[i] = 0.0;
        pInput_r[i] = 0.0;

        /* delay */
        {
            auto delay_index = static_cast<long>(delay.write_index) - static_cast<long>(delay.time);
            if (delay_index < 0) {
                delay_index += delay.tap_length;
            }
            auto tap_l = delay.pTap_l[delay_index];
            auto tap_r = delay.pTap_r[delay_index];
            auto del_l = tap_l * (1.0 - delay.cross) + tap_r * delay.cross;
            auto del_r = tap_r * (1.0 - delay.cross) + tap_l * delay.cross;
            output_l += del_l * delay.send;
            output_r += del_r * delay.send;
            delay.pTap_l[delay.write_index] = output_l;
            delay.pTap_r[delay.write_index] = output_r;
            delay.write_index++;
            if (delay.tap_length <= delay.write_index) {
                delay.write_index -= delay.tap_length;
            }
        }

        /* chorus */
        {
            auto idx_u = static_cast<long>(delay.write_index) - static_cast<long>(
                ((0.5 + 0.5 * chorus.lfo_u) * chorus.depth * 0.99 + 0.01) * mpSystem_value->sample_rate
            );
            auto idx_v = static_cast<long>(delay.write_index) - static_cast<long>(
                ((0.5 + 0.5 * chorus.lfo_v) * chorus.depth * 0.99 + 0.01) * mpSystem_value->sample_rate
            );
            auto idx_w = static_cast<long>(delay.write_index) - static_cast<long>(
                ((0.5 + 0.5 * chorus.lfo_w) * chorus.depth * 0.99 + 0.01) * mpSystem_value->sample_rate
            );
            if (idx_u < 0) {
                idx_u += delay.tap_length;
            }
            if (idx_v < 0) {
                idx_v += delay.tap_length;
            }
            if (idx_w < 0) {
                idx_w += delay.tap_length;
            }
            auto cho_l
                = delay.pTap_l[idx_u] / 3.0
                + delay.pTap_l[idx_v] * chorus.pan_a
                + delay.pTap_l[idx_w] * chorus.pan_b;
            auto cho_r
                = delay.pTap_r[idx_u] / 3.0
                + delay.pTap_r[idx_v] * chorus.pan_b
                + delay.pTap_r[idx_w] * chorus.pan_a;
            output_l += cho_l * chorus.send;
            output_r += cho_r * chorus.send;
            chorus.lfo_u += (chorus.lfo_v - chorus.lfo_w) * chorus.rate;
            chorus.lfo_v += (chorus.lfo_w - chorus.lfo_u) * chorus.rate;
            chorus.lfo_w += (chorus.lfo_u - chorus.lfo_v) * chorus.rate;
        }

        /* meter */
        {
            auto delta = RMS_ATTENUTE * mpSystem_value->delta_time;
            auto attenute = 1.0 - delta;
            auto rms_l = param.rms_l * attenute;
            auto rms_r = param.rms_r * attenute;
            param.rms_l = rms_l + output_l * output_l * delta;
            param.rms_r = rms_r + output_r * output_r * delta;
            attenute = 1.0 - PEAK_ATTENUTE * mpSystem_value->delta_time;
            auto peak_l = param.peak_l * attenute;
            auto peak_r = param.peak_r * attenute;
            param.peak_l = fmax(peak_l, fabs(output_l));
            param.peak_r = fmax(peak_r, fabs(output_r));
        }

        pOutput_l[i] += output_l;
        pOutput_r[i] += output_r;
    }
}
