#include "channel.h"
#include "channel_const.h"
#include "channel_params.h"
#include "sampler.h"
#include "filter.h"
#include "../inst/inst_list.h"

#include <math.h>

Channel::Channel(SYSTEM_VALUE *pSystemValue, int number) {
    mpSystemValue = pSystemValue;
    Number = (byte)number;
    Param.pKeyBoard = (byte*)calloc(1, sizeof(byte) * 128);

    pInput = (double*)calloc(pSystemValue->bufferLength, sizeof(double));

    delay.write_index = 0;
    delay.tap_length = pSystemValue->sampleRate;
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
    if (NULL != pInput) {
        free(pInput);
        pInput = NULL;
    }
    if (NULL != delay.pTap_l) {
        free(delay.pTap_l);
        delay.pTap_l = NULL;
    }
    if (NULL != delay.pTap_r) {
        free(delay.pTap_r);
        delay.pTap_r = NULL;
    }
    free(Param.pKeyBoard);
}

/******************************************************************************/
void
Channel::set_amp(byte vol, byte exp) {
    Param.Vol = vol;
    Param.Exp = exp;
    current_amp = vol * vol * exp * exp / 260144641.0;
}

void
Channel::set_pan(byte value) {
    Param.Pan = value;
    current_pan_re = Cos[value];
    current_pan_im = Sin[value];
}

void
Channel::set_hold(byte value) {
    if (value < 64) {
        for (int s = 0; s < SAMPLER_COUNT; ++s) {
            auto pSmpl = mpSystemValue->ppSampler[s];
            if (E_SAMPLER_STATE::HOLD == pSmpl->state) {
                pSmpl->state = E_SAMPLER_STATE::RELEASE;
            }
        }
        for (int n = 0; n < 128; ++n) {
            if ((byte)E_KEY_STATE::HOLD == Param.pKeyBoard[n]) {
                Param.pKeyBoard[n] = (byte)E_KEY_STATE::FREE;
            }
        }
    }
    Param.Hld = value;
}

void
Channel::set_res(byte value) {
    Param.Fq = value;
}

void
Channel::set_cut(byte value) {
    Param.Fc = value;
}

void
Channel::set_rpn() {
    switch (mRpnLSB | mRpnMSB << 8) {
    case 0x0000:
        Param.BendRange = mDataMSB;
        break;
    default:
        break;
    }
    mRpnLSB = 0xFF;
    mRpnMSB = 0xFF;
}

void
Channel::set_nrpn() {
    //switch (mNrpnLSB | mNrpnMSB << 8) {
    //default:
    //    break;
    //}
    mNrpnLSB = 0xFF;
    mNrpnMSB = 0xFF;
}

/******************************************************************************/
void
Channel::init_ctrl() {
    set_amp(100, 100);
    set_pan(64);

    set_hold(0);

    Param.Rev = 0;
    Param.Cho = 0;
    chorus.send = Param.Cho / 127.0;
    chorus.pan_a = (1.0 - Param.Rev / 127.0) / 3.0;
    chorus.pan_b = (1.0 + Param.Rev / 127.0) / 3.0;
    chorus.depth = 20 * 0.001;
    chorus.rate = 10 * 0.06283 / 1.732 * mpSystemValue->deltaTime;
    
    Param.Del = 0;
    delay.send = Param.Del / 128.0;
    delay.cross = 64 / 127.0;
    delay.time = static_cast<long>(mpSystemValue->sampleRate * 200 * 0.001);
    
    set_res(64);
    set_cut(64);

    Param.Rel = 64;
    Param.Atk = 64;

    Param.VibRate = 64;
    Param.VibDepth = 64;
    Param.VibDelay = 64;

    Param.BendRange = 2;
    Param.Pitch = 0;
    pitch = 1.0;

    mRpnLSB = 0xFF;
    mRpnMSB = 0xFF;
    mNrpnLSB = 0xFF;
    mNrpnMSB = 0xFF;

    Param.isDrum = Number == 9 ? 1 : 0;
    Param.bankMSB = 0;
    Param.bankLSB = 0;
    Param.progNum = 0;
    program_change(0);

    Param.Enable = 1;
}

void
Channel::all_reset() {
    set_amp(Param.Vol, 100);
    set_pan(64);
    set_hold(0);

    Param.Pitch = 0;
    pitch = 1.0;

    mRpnLSB = 0xFF;
    mRpnMSB = 0xFF;
    mNrpnLSB = 0xFF;
    mNrpnMSB = 0xFF;
}

void
Channel::note_off(byte noteNumber) {
    for (int s = 0; s < SAMPLER_COUNT; ++s) {
        auto pSmpl = mpSystemValue->ppSampler[s];
        auto pChParam = mpSystemValue->ppChannelParam[pSmpl->channelNum];
        if (pSmpl->state < E_SAMPLER_STATE::PRESS ||
            (pChParam->isDrum && !pSmpl->pWave->loopEnable)) {
            continue;
        }
        if (pSmpl->channelNum == Number && pSmpl->noteNum == noteNumber) {
            if (Param.Hld < 64) {
                pSmpl->state = E_SAMPLER_STATE::RELEASE;
            } else {
                pSmpl->state = E_SAMPLER_STATE::HOLD;
            }
        }
    }
    if (Param.Hld < 64) {
        Param.pKeyBoard[noteNumber] = (byte)E_KEY_STATE::FREE;
    } else {
        Param.pKeyBoard[noteNumber] = (byte)E_KEY_STATE::HOLD;
    }
}

void
Channel::note_on(byte noteNumber, byte velocity) {
    if (0 == velocity) {
        note_off(noteNumber);
        return;
    }
    Param.pKeyBoard[noteNumber] = (byte)E_KEY_STATE::PRESS;
    mpSystemValue->cInstList->SetSampler(mpInst, Number, noteNumber, velocity);
}

void
Channel::ctrl_change(byte type, byte b1) {
    switch ((E_CTRL_TYPE)type) {
    case E_CTRL_TYPE::BANK_MSB:
        Param.bankMSB = b1;
        break;
    case E_CTRL_TYPE::BANK_LSB:
        Param.bankLSB = b1;
        break;

    case E_CTRL_TYPE::VOLUME:
        set_amp(b1, Param.Exp);
        break;
    case E_CTRL_TYPE::PAN:
        set_pan(b1);
        break;
    case E_CTRL_TYPE::EXPRESSION:
        set_amp(Param.Vol, b1);
        break;

    case E_CTRL_TYPE::MODULATION:
        Param.Mod = b1;
        break;

    case E_CTRL_TYPE::HOLD:
        set_hold(b1);
        break;
    case E_CTRL_TYPE::RELEACE:
        Param.Rel = b1;
        break;
    case E_CTRL_TYPE::ATTACK:
        Param.Atk = b1;
        break;

    case E_CTRL_TYPE::RESONANCE:
        set_res(b1);
        break;
    case E_CTRL_TYPE::CUTOFF:
        set_cut(b1);
        break;

    case E_CTRL_TYPE::VIB_RATE:
        Param.VibRate = b1;
        break;
    case E_CTRL_TYPE::VIB_DEPTH:
        Param.VibDepth = b1;
        break;
    case E_CTRL_TYPE::VIB_DELAY:
        Param.VibDelay = b1;
        break;

    case E_CTRL_TYPE::REVERB:
        Param.Rev = b1;
        chorus.pan_a = (1.0 - b1 / 127.0) / 3.0;
        chorus.pan_b = (1.0 + b1 / 127.0) / 3.0;
        break;
    case E_CTRL_TYPE::CHORUS:
        Param.Cho = b1;
        chorus.send = b1 / 128.0;
        break;
    case E_CTRL_TYPE::DELAY:
        Param.Del = b1;
        delay.send = b1 / 127.0;
        break;

    case E_CTRL_TYPE::NRPN_LSB:
        mNrpnLSB = b1;
        break;
    case E_CTRL_TYPE::NRPN_MSB:
        mNrpnMSB = b1;
        break;
    case E_CTRL_TYPE::RPN_LSB:
        mRpnLSB = b1;
        break;
    case E_CTRL_TYPE::RPN_MSB:
        mRpnMSB = b1;
        break;
    case E_CTRL_TYPE::DATA_MSB:
        mDataMSB = b1;
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
    Param.progNum = value;
    mpInst = mpSystemValue->cInstList->GetInstInfo(Param.isDrum, Param.bankLSB, Param.bankMSB, Param.progNum);
    Param.pName = (byte*)mpInst->pName;
}

void
Channel::pitch_bend(short pitch) {
    Param.Pitch = pitch;
    auto temp = Param.Pitch * Param.BendRange;
    if (temp < 0) {
        temp = -temp;
        this->pitch = 1.0 / (SemiTone[temp >> 13] * PitchMSB[(temp >> 7) % 64] * PitchLSB[temp % 128]);
    } else {
        this->pitch = SemiTone[temp >> 13] * PitchMSB[(temp >> 7) % 64] * PitchLSB[temp % 128];
    }
}

void
Channel::step(double* pOutputL, double* pOutputR) {
    for (int i = 0; i < mpSystemValue->bufferLength; i++) {
        auto output_l = pInput[i] * current_amp * current_pan_re;
        auto output_r = pInput[i] * current_amp * current_pan_im;
        pInput[i] = 0.0;

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
                ((0.5 + 0.5 * chorus.lfo_u) * chorus.depth * 0.99 + 0.01) * mpSystemValue->sampleRate
            );
            auto idx_v = static_cast<long>(delay.write_index) - static_cast<long>(
                ((0.5 + 0.5 * chorus.lfo_v) * chorus.depth * 0.99 + 0.01) * mpSystemValue->sampleRate
            );
            auto idx_w = static_cast<long>(delay.write_index) - static_cast<long>(
                ((0.5 + 0.5 * chorus.lfo_w) * chorus.depth * 0.99 + 0.01) * mpSystemValue->sampleRate
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
            auto delta = RMS_ATTENUTE * mpSystemValue->deltaTime;
            auto attenute = 1.0 - delta;
            auto rms_l = Param.RmsL * attenute;
            auto rms_r = Param.RmsR * attenute;
            Param.RmsL = rms_l + output_l * output_l * delta;
            Param.RmsR = rms_r + output_r * output_r * delta;
            attenute = 1.0 - PEAK_ATTENUTE * mpSystemValue->deltaTime;
            auto peak_l = Param.PeakL * attenute;
            auto peak_r = Param.PeakR * attenute;
            Param.PeakL = fmax(peak_l, fabs(output_l));
            Param.PeakR = fmax(peak_r, fabs(output_r));
        }

        pOutputL[i] += output_l;
        pOutputR[i] += output_r;
    }
}
