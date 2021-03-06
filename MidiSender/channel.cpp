#include <math.h>
#include "channel.h"
#include "channel_const.h"

Channel::Channel(INST_LIST *inst, SAMPLER** ppSampler, Note** ppNote, CHANNEL_PARAM* pChannel, int no, int samplerCount) {
    InstList = inst;
    mppSampler = ppSampler;
    mppNote = ppNote;
    mpChannel = pChannel;
    mSamplerCount = samplerCount;
    No = (byte)no;
    Param.Enable = true;
    AllReset();
}

/******************************************************************************/
void
Channel::AllReset() {
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

    mpChannel->chorusRate = 0.5;
    mpChannel->chorusDepth = 0.005;
    mpChannel->delayTime = 0.2;
    mpChannel->delayCross = 0.375;
    mpChannel->holdDelta = DeltaTime;

    mRpnLSB = 0xFF;
    mRpnMSB = 0xFF;
    mNrpnLSB = 0xFF;
    mNrpnMSB = 0xFF;

    Param.BendRange = 2;
    Param.Pitch = 0;
    mpChannel->pitch = 1.0;

    Param.InstId.programNo = 0;
    Param.InstId.bankMSB = 0;
    Param.InstId.bankLSB = 0;
    ProgramChange(0);
}

void
Channel::NoteOff(byte noteNo, E_NOTE_STATE keyState) {
    for (auto i = 0; i < mSamplerCount; ++i) {
        auto pNote = mppNote[i];
        if (pNote->mState < E_NOTE_STATE::PRESS) {
            continue;
        }
        if (pNote->mChannelNum == No && pNote->mNum == noteNo) {
            if (E_NOTE_STATE::PURGE == keyState) {
                pNote->mState = E_NOTE_STATE::PURGE;
            } else {
                if (E_NOTE_STATE::PRESS == pNote->mState) {
                    pNote->mState = Param.Hld < 64 ? E_NOTE_STATE::RELEASE : E_NOTE_STATE::HOLD;
                }
            }
        }
    }
}

void
Channel::NoteOn(byte noteNo, byte velocity) {
    if (0 == velocity) {
        NoteOff(noteNo, E_NOTE_STATE::RELEASE);
        return;
    } else {
        NoteOff(noteNo, E_NOTE_STATE::PURGE);
    }

    Note* pSetNote = NULL;
    for (auto j = 0; j < mSamplerCount; ++j) {
        if (E_NOTE_STATE::FREE == mppNote[j]->mState) {
            mppNote[j]->mState = E_NOTE_STATE::RESERVED;
            pSetNote = mppNote[j];
            break;
        }
    }
    if (NULL == pSetNote) {
        return;
    }

    pSetNote->mChannelNum = No;
    pSetNote->mNum = noteNo;
    pSetNote->mVelocity = velocity / 127.0;

    int unisonCount = 0;

    for (int rgnIdx = 0; rgnIdx < mRegionCount; rgnIdx++) {
        auto pRegion = mppRegions[rgnIdx];
        if (noteNo < pRegion->keyLo || pRegion->keyHi < noteNo ||
            velocity < pRegion->velLo || pRegion->velHi < velocity) {
            continue;
        }

        double pitch;
        auto diffNote = noteNo - pRegion->waveInfo.originNote;
        if (diffNote < 0) {
            pitch = 1.0 / SemiTone[-diffNote];
        } else {
            pitch = SemiTone[diffNote];
        }
        pitch *= pRegion->waveInfo.delta;

        for (auto j = 0; j < mSamplerCount; ++j) {
            auto pSmpl = mppSampler[j];
            if (NULL == pSmpl->pNote) {
                pSmpl->pNote = pSetNote;
                pSmpl->unisonNum = unisonCount;
                pSmpl->delta = pitch;
                pSmpl->index = 0.0;
                pSmpl->time = 0.0;
                pSmpl->egAmp = 0.0;
                pSmpl->pWaveInfo = &pRegion->waveInfo;
                pSmpl->pEnvAmp = &pRegion->env;
                pSetNote->mppSamplers[unisonCount] = pSmpl;
                unisonCount++;
                break;
            }
        }

        if (UNISON_COUNT <= unisonCount) {
            break;
        }
    }
    pSetNote->mState = unisonCount ? E_NOTE_STATE::PRESS : E_NOTE_STATE::FREE;
}

void
Channel::CtrlChange(byte type, byte b1) {
    switch ((E_CTRL_TYPE)type) {
    case E_CTRL_TYPE::BANK_MSB:
        Param.InstId.bankMSB = b1;
        break;
    case E_CTRL_TYPE::BANK_LSB:
        Param.InstId.bankLSB = b1;
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
        setRpn(b1);
        setNrpn(b1);
        break;

    case E_CTRL_TYPE::ALL_RESET:
        AllReset();
        break;
    }
}

void
Channel::ProgramChange(byte value) {
    Param.InstId.isDrum = (byte)(9 == No ? 1 : 0);
    Param.InstId.programNo = value;
    if (NULL == searchInst(Param.InstId)) {
        Param.InstId.bankMSB = 0;
        Param.InstId.bankLSB = 0;
        if (NULL == searchInst(Param.InstId)) {
            Param.InstId.programNo = 0;
            if (NULL == searchInst(Param.InstId)) {
                Param.InstId.isDrum = (byte)(Param.InstId.isDrum == 0 ? 1 : 0);
            }
        }
    }
    auto tmp = searchInst(Param.InstId);
    mRegionCount = tmp->regionCount;
    mppRegions = tmp->ppRegions;
    Param.Name = tmp->pName;
}

void
Channel::PitchBend(short pitch) {
    Param.Pitch = pitch;
    auto temp = Param.Pitch * Param.BendRange;
    if (temp < 0) {
        temp = -temp;
        mpChannel->pitch = 1.0 / (SemiTone[temp >> 13] * PitchMSB[(temp >> 7) % 64] * PitchLSB[temp % 128]);
    } else {
        mpChannel->pitch = SemiTone[temp >> 13] * PitchMSB[(temp >> 7) % 64] * PitchLSB[temp % 128];
    }
}

/******************************************************************************/
void
Channel::setAmp(byte vol, byte exp) {
    Param.Vol = vol;
    Param.Exp = exp;
    mpChannel->amp = Amp[vol] * Amp[exp];
}

void
Channel::setPan(byte value) {
    Param.Pan = value;
    mpChannel->panLeft = Cos[value];
    mpChannel->panRight = Sin[value];
}

void
Channel::setHld(byte value) {
    if (value < 64) {
        for (auto s = 0; s < mSamplerCount; ++s) {
            auto pNote = mppNote[s];
            if (E_NOTE_STATE::HOLD == pNote->mState) {
                pNote->mState = E_NOTE_STATE::RELEASE;
            }
        }
    } else {
        for (auto s = 0; s < mSamplerCount; ++s) {
            auto pNote = mppNote[s];
            if (E_NOTE_STATE::RELEASE == pNote->mState) {
                pNote->mState = E_NOTE_STATE::HOLD;
            }
        }
    }
    Param.Hld = value;
}

void
Channel::setRes(byte value) {
    Param.Fq = value;
    mpChannel->resonance = (value < 64) ? 0.0 : ((value - 64) / 64.0);
}

void
Channel::setCut(byte value) {
    Param.Fc = value;
    mpChannel->cutoff = (value < 64) ? Level[(int)(2.0 * value)] : 1.0;
}

void
Channel::setDel(byte value) {
    Param.Del = value;
    mpChannel->delaySend = 0.8 * FeedBack[value];
}

void
Channel::setCho(byte value) {
    Param.Cho = value;
    mpChannel->chorusSend = 3.0 * FeedBack[value];
}

void
Channel::setRpn(byte b1) {
    switch (mRpnLSB | mRpnMSB << 8) {
    case 0x0000:
        Param.BendRange = b1;
        break;
    default:
        break;
    }
    mRpnMSB = 0xFF;
    mRpnLSB = 0xFF;
}

void
Channel::setNrpn(byte b1) {
    //switch (mNrpnLSB | mNrpnMSB << 8) {
    //default:
    //    break;
    //}
    mNrpnMSB = 0xFF;
    mNrpnLSB = 0xFF;
}

INST_REC*
Channel::searchInst(INST_ID id) {
    for (int i = 0; i < InstList->instCount; i++) {
        auto listId = InstList->ppInst[i]->id;
        if (id.isDrum == listId.isDrum &&
            id.bankMSB == listId.bankMSB &&
            id.bankLSB == listId.bankLSB &&
            id.programNo == listId.programNo) {
            return InstList->ppInst[i];
        }
    }
    return NULL;
}
