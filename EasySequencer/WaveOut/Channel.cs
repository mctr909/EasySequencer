using System;
using System.Runtime.InteropServices;

using MIDI;

namespace WaveOut {
    unsafe public class Channel {
        private CHANNEL* mpChannel = null;
        private SAMPLER** mppSampler = null;
        private INST_ID mInstId;

        public INST_LIST InstList { get; private set; }

        public bool Enable;
        public byte No { get; private set; }
        public INST_ID InstId { get { return mInstId; } }
        public INST_REC* SelectedInst { get; private set; }

        public string InstName { get { return Marshal.PtrToStringAuto((IntPtr)SelectedInst->pName); } }
        public string InstCategory { get { return Marshal.PtrToStringAuto((IntPtr)SelectedInst->pCategory); } }

        public byte Vol { get; private set; }
        public byte Exp { get; private set; }
        public byte Pan { get; private set; }
        public byte Rev { get; private set; }
        public byte Del { get; private set; }
        public byte Cho { get; private set; }
        public byte Mod { get; private set; }
        public byte Hld { get; private set; }
        public byte Fc { get; private set; }
        public byte Fq { get; private set; }
        public byte Atk { get; private set; }
        public byte Rel { get; private set; }
        public byte VibRate { get; private set; }
        public byte VibDepth { get; private set; }
        public byte VibDelay { get; private set; }
        public byte BendRange { get; private set; }
        public int Pitch { get; private set; }

        private byte mRpnLSB;
        private byte mRpnMSB;
        private byte mNrpnLSB;
        private byte mNrpnMSB;

        public Channel(INST_LIST inst, SAMPLER** ppSampler, CHANNEL* pChannel, int no) {
            InstList = inst;
            mppSampler = ppSampler;
            mpChannel = pChannel;
            No = (byte)no;
            Enable = true;
            AllReset();
        }

        /******************************************************************************/
        public void AllReset() {
            mInstId = new INST_ID();

            setAmp(100, 100);
            setPan(64);

            setHld(0);

            Rev = 0;
            setCho(0);
            setDel(0);

            setRes(64);
            setCut(64);

            Rel = 64;
            Atk = 64;

            VibRate = 64;
            VibDepth = 64;
            VibDelay = 64;

            mpChannel->chorusRate = 0.5;
            mpChannel->chorusDepth = 0.005;
            mpChannel->delayTime = 0.2;
            mpChannel->delayCross = 0.375;
            mpChannel->holdDelta = Const.DeltaTime;

            mRpnLSB = 0xFF;
            mRpnMSB = 0xFF;
            mNrpnLSB = 0xFF;
            mNrpnMSB = 0xFF;

            BendRange = 2;
            Pitch = 0;
            mpChannel->pitch = 1.0;

            mInstId.bankMSB = 0;
            mInstId.bankLSB = 0;
            ProgramChange(0);
        }

        public void NoteOff(byte noteNo, E_KEY_STATE keyState) {
            for (var i = 0; i < Sender.SAMPLER_COUNT; ++i) {
                var pSmpl = mppSampler[i];
                if (pSmpl->state == E_KEY_STATE.STANDBY) {
                    continue;
                }
                if (pSmpl->channelNum == No && pSmpl->noteNum == noteNo) {
                    if (E_KEY_STATE.PURGE == keyState) {
                        pSmpl->state = E_KEY_STATE.PURGE;
                    } else {
                        if (!Enable || Hld < 64) {
                            pSmpl->state = E_KEY_STATE.RELEASE;
                        } else {
                            pSmpl->state = E_KEY_STATE.HOLD;
                        }
                    }
                }
            }
        }

        public void NoteOn(byte noteNo, byte velocity) {
            if (0 == velocity) {
                NoteOff(noteNo, E_KEY_STATE.RELEASE);
                return;
            } else {
                NoteOff(noteNo, E_KEY_STATE.PURGE);
            }
            for (int rgnIdx = 0; rgnIdx < SelectedInst->regionCount; rgnIdx++) {
                var pRegion = SelectedInst->ppRegions[rgnIdx];
                if (noteNo < pRegion->keyLo || pRegion->keyHi < noteNo ||
                    velocity < pRegion->velLo || pRegion->velHi < velocity) {
                    continue;
                }
                double pitch;
                var diffNote = noteNo - pRegion->waveInfo.unityNote;
                if (diffNote < 0) {
                    pitch = 1.0 / Const.SemiTone[-diffNote];
                } else {
                    pitch = Const.SemiTone[diffNote];
                }
                for (var j = 0; j < Sender.SAMPLER_COUNT; ++j) {
                    var pSmpl = mppSampler[j];
                    if (E_KEY_STATE.STANDBY != pSmpl->state) {
                        continue;
                    }
                    pSmpl->channelNum = No;
                    pSmpl->noteNum = noteNo;
                    pSmpl->waveInfo = pRegion->waveInfo;
                    pSmpl->waveInfo.delta = pRegion->waveInfo.delta * pitch;
                    pSmpl->index = 0.0;
                    pSmpl->time = 0.0;
                    pSmpl->velocity = velocity / 127.0;
                    pSmpl->egAmp = 0.0;
                    pSmpl->envAmp = pRegion->env;
                    pSmpl->state = E_KEY_STATE.PRESS;
                    break;
                }
                break;
            }
        }

        public void CtrlChange(E_CTRL_TYPE type, byte b1) {
            switch (type) {
                case E_CTRL_TYPE.BANK_MSB:
                    mInstId.bankMSB = b1;
                    break;
                case E_CTRL_TYPE.BANK_LSB:
                    mInstId.bankLSB = b1;
                    break;

                case E_CTRL_TYPE.VOLUME:
                    setAmp(b1, Exp);
                    break;
                case E_CTRL_TYPE.PAN:
                    setPan(b1);
                    break;
                case E_CTRL_TYPE.EXPRESSION:
                    setAmp(Vol, b1);
                    break;

                case E_CTRL_TYPE.MODULATION:
                    Mod = b1;
                    break;

                case E_CTRL_TYPE.HOLD:
                    setHld(b1);
                    break;
                case E_CTRL_TYPE.RELEACE:
                    Rel = b1;
                    break;
                case E_CTRL_TYPE.ATTACK:
                    Atk = b1;
                    break;

                case E_CTRL_TYPE.RESONANCE:
                    setRes(b1);
                    break;
                case E_CTRL_TYPE.CUTOFF:
                    setCut(b1);
                    break;

                case E_CTRL_TYPE.VIB_RATE:
                    VibRate = b1;
                    break;
                case E_CTRL_TYPE.VIB_DEPTH:
                    VibDepth = b1;
                    break;
                case E_CTRL_TYPE.VIB_DELAY:
                    VibDelay = b1;
                    break;

                case E_CTRL_TYPE.REVERB:
                    Rev = b1;
                    break;
                case E_CTRL_TYPE.CHORUS:
                    setCho(b1);
                    break;
                case E_CTRL_TYPE.DELAY:
                    setDel(b1);
                    break;

                case E_CTRL_TYPE.NRPN_LSB:
                    mNrpnLSB = b1;
                    break;
                case E_CTRL_TYPE.NRPN_MSB:
                    mNrpnMSB = b1;
                    break;
                case E_CTRL_TYPE.RPN_LSB:
                    mRpnLSB = b1;
                    break;
                case E_CTRL_TYPE.RPN_MSB:
                    mRpnMSB = b1;
                    break;
                case E_CTRL_TYPE.DATA_MSB:
                    setRpn(b1);
                    setNrpn(b1);
                    break;

                case E_CTRL_TYPE.ALL_RESET:
                    AllReset();
                    break;
            }
        }

        public void ProgramChange(byte value) {
            mInstId.isDrum = (byte)(9 == No ? 1 : 0);
            mInstId.programNo = value;
            if (null == searchInst(mInstId)) {
                mInstId.bankMSB = 0;
                mInstId.bankLSB = 0;
                if (null == searchInst(mInstId)) {
                    mInstId.programNo = 0;
                    if (null == searchInst(mInstId)) {
                        mInstId.isDrum = (byte)(mInstId.isDrum == 0 ? 1 : 0);
                    }
                }
            }
            SelectedInst = searchInst(mInstId);
        }

        public void ProgramChange(byte value, bool isDrum) {
            mInstId.isDrum = (byte)(isDrum ? 1 : 0);
            mInstId.programNo = value;
            if (null == searchInst(mInstId)) {
                mInstId.bankMSB = 0;
                mInstId.bankLSB = 0;
                if (null == searchInst(mInstId)) {
                    mInstId.programNo = 0;
                }
            }
            SelectedInst = searchInst(mInstId);
        }

        public void PitchBend(short pitch) {
            Pitch = pitch;
            var temp = Pitch * BendRange;
            if (temp < 0) {
                temp = -temp;
                mpChannel->pitch = 1.0 / (Const.SemiTone[temp >> 13] * Const.PitchMSB[(temp >> 7) % 64] * Const.PitchLSB[temp % 128]);
            }
            else {
                mpChannel->pitch = Const.SemiTone[temp >> 13] * Const.PitchMSB[(temp >> 7) % 64] * Const.PitchLSB[temp % 128];
            }
        }

        /******************************************************************************/
        private void setAmp(byte vol, byte exp) {
            Vol = vol;
            Exp = exp;
            mpChannel->amp = Const.Amp[vol] * Const.Amp[exp];
        }

        private void setPan(byte value) {
            Pan = value;
            mpChannel->panLeft = Const.Cos[value];
            mpChannel->panRight = Const.Sin[value];
        }

        private void setHld(byte value) {
            if (value < 64) {
                for (var s = 0; s < Sender.SAMPLER_COUNT; ++s) {
                    var pSmpl = mppSampler[s];
                    if (E_KEY_STATE.HOLD == pSmpl->state) {
                        pSmpl->state = E_KEY_STATE.RELEASE;
                    }
                }
            }
            Hld = value;
        }

        private void setRes(byte value) {
            Fq = value;
            mpChannel->resonance = (value < 64) ? 0.0 : ((value - 64) / 64.0);
        }

        private void setCut(byte value) {
            Fc = value;
            mpChannel->cutoff = (value < 64) ? Const.Level[(int)(2.0 * value)] : 1.0;
        }

        private void setDel(byte value) {
            Del = value;
            mpChannel->delaySend = 0.8 * Const.FeedBack[value];
        }

        private void setCho(byte value) {
            Cho = value;
            mpChannel->chorusSend = 3.0 * Const.FeedBack[value];
        }

        private void setRpn(byte b1) {
            switch (mRpnLSB | mRpnMSB << 8) {
                case 0x0000:
                    BendRange = b1;
                    break;
                default:
                    break;
            }
            mRpnMSB = 0xFF;
            mRpnLSB = 0xFF;
        }

        private void setNrpn(byte b1) {
            switch (mNrpnLSB | mNrpnMSB << 8) {
                default:
                    break;
            }
            mNrpnMSB = 0xFF;
            mNrpnLSB = 0xFF;
        }

        private INST_REC* searchInst(INST_ID id) {
            for (int i = 0; i < InstList.instCount; i++) {
                var listId = InstList.ppInst[i]->id;
                if (id.isDrum == listId.isDrum &&
                    id.bankMSB == listId.bankMSB &&
                    id.bankLSB == listId.bankLSB &&
                    id.programNo == listId.programNo) {
                    return InstList.ppInst[i];
                }
            }
            return null;
        }
    }
}
