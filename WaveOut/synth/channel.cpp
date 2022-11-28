#include "channel.h"
#include "channel_const.h"
#include "channel_params.h"
#include "sampler.h"
#include "effect.h"
#include "../inst/inst_list.h"

#include <math.h>

Channel::Channel(SYSTEM_VALUE *pSystemValue, int number) {
    mpSystemValue = pSystemValue;
    mpEffectParam = pSystemValue->ppEffect[number]->pParam;
    Number = (byte)number;
    Param.pKeyBoard = (byte*)calloc(1, sizeof(byte) * 128);

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

    AllInit();
}

Channel::~Channel() {
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
Channel::AllInit() {
    setAmp(100, 100);
    setPan(64);

    setHld(0);

    Param.Rev = 0;
    setCho(0);
    setDel(0);

    setRes(64);
    setCut(64);

    Param.Rel = 64;
    Param.Atk = 64;

    Param.VibRate = 64;
    Param.VibDepth = 64;
    Param.VibDelay = 64;

    mpEffectParam->chorusRate = 0.5;
    mpEffectParam->chorusDepth = 0.005;
    mpEffectParam->delayTime = 0.2;
    mpEffectParam->delayCross = 0.375;
    mpEffectParam->holdDelta = mpSystemValue->deltaTime;

    Param.BendRange = 2;
    Param.Pitch = 0;
    mpEffectParam->pitch = 1.0;

    mRpnLSB = 0xFF;
    mRpnMSB = 0xFF;
    mNrpnLSB = 0xFF;
    mNrpnMSB = 0xFF;

    Param.isDrum = Number == 9 ? 1 : 0;
    Param.bankMSB = 0;
    Param.bankLSB = 0;
    Param.progNum = 0;
    ProgramChange(0);

    Param.Enable = 1;
}

void
Channel::AllReset() {
    setAmp(Param.Vol, 100);
    setPan(64);
    setHld(0);

    Param.Pitch = 0;
    mpEffectParam->pitch = 1.0;

    mRpnLSB = 0xFF;
    mRpnMSB = 0xFF;
    mNrpnLSB = 0xFF;
    mNrpnMSB = 0xFF;
}

void
Channel::NoteOff(byte noteNumber) {
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
Channel::NoteOn(byte noteNumber, byte velocity) {
    if (0 == velocity) {
        NoteOff(noteNumber);
        return;
    }
    Param.pKeyBoard[noteNumber] = (byte)E_KEY_STATE::PRESS;
    mpSystemValue->cInstList->SetSampler(mpInst, Number, noteNumber, velocity);
}

void
Channel::CtrlChange(byte type, byte b1) {
    switch ((E_CTRL_TYPE)type) {
    case E_CTRL_TYPE::BANK_MSB:
        Param.bankMSB = b1;
        break;
    case E_CTRL_TYPE::BANK_LSB:
        Param.bankLSB = b1;
        break;

    case E_CTRL_TYPE::VOLUME:
        setAmp(b1, Param.Exp);
        break;
    case E_CTRL_TYPE::PAN:
        setPan(b1);
        break;
    case E_CTRL_TYPE::EXPRESSION:
        setAmp(Param.Vol, b1);
        break;

    case E_CTRL_TYPE::MODULATION:
        Param.Mod = b1;
        break;

    case E_CTRL_TYPE::HOLD:
        setHld(b1);
        break;
    case E_CTRL_TYPE::RELEACE:
        Param.Rel = b1;
        break;
    case E_CTRL_TYPE::ATTACK:
        Param.Atk = b1;
        break;

    case E_CTRL_TYPE::RESONANCE:
        setRes(b1);
        break;
    case E_CTRL_TYPE::CUTOFF:
        setCut(b1);
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
        break;
    case E_CTRL_TYPE::CHORUS:
        setCho(b1);
        break;
    case E_CTRL_TYPE::DELAY:
        setDel(b1);
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
        setRpn();
        setNrpn();
        break;

    case E_CTRL_TYPE::ALL_RESET:
        AllReset();
        break;
    }
}

void
Channel::ProgramChange(byte value) {
    Param.progNum = value;
    mpInst = mpSystemValue->cInstList->GetInstInfo(Param.isDrum, Param.bankLSB, Param.bankMSB, Param.progNum);
    Param.pName = (byte*)mpInst->pName;
}

void
Channel::PitchBend(short pitch) {
    Param.Pitch = pitch;
    auto temp = Param.Pitch * Param.BendRange;
    if (temp < 0) {
        temp = -temp;
        mpEffectParam->pitch = 1.0 / (SemiTone[temp >> 13] * PitchMSB[(temp >> 7) % 64] * PitchLSB[temp % 128]);
    } else {
        mpEffectParam->pitch = SemiTone[temp >> 13] * PitchMSB[(temp >> 7) % 64] * PitchLSB[temp % 128];
    }
}

void
Channel::Step(double* pOutputL, double* pOutputR) {

}

/******************************************************************************/
void
Channel::setAmp(byte vol, byte exp) {
    Param.Vol = vol;
    Param.Exp = exp;
    mpEffectParam->amp = vol * vol * exp * exp / 260144641.0;
}

void
Channel::setPan(byte value) {
    Param.Pan = value;
    mpEffectParam->panLeft = Cos[value];
    mpEffectParam->panRight = Sin[value];
}

void
Channel::setHld(byte value) {
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
Channel::setRes(byte value) {
    Param.Fq = value;
    mpEffectParam->resonance = (value < 64) ? 0.0 : ((value - 64) / 64.0);
}

void
Channel::setCut(byte value) {
    Param.Fc = value;
    mpEffectParam->cutoff = (value < 64) ? Level[(int)(2.0 * value)] : 1.0;
}

void
Channel::setDel(byte value) {
    Param.Del = value;
    mpEffectParam->delaySend = 0.8 * FeedBack[value];
}

void
Channel::setCho(byte value) {
    Param.Cho = value;
    mpEffectParam->chorusSend = 3.0 * FeedBack[value];
}

void
Channel::setRpn() {
    switch (mRpnLSB | mRpnMSB << 8) {
    case 0x0000:
        Param.BendRange = mDataMSB;
        break;
    default:
        break;
    }
}

void
Channel::setNrpn() {
    //switch (mNrpnLSB | mNrpnMSB << 8) {
    //default:
    //    break;
    //}
}
