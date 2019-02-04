﻿namespace MIDI {
    unsafe public class Channel {
        private CHANNEL* mpChannel = null;
        private Instruments mInst = null;
        private INST_ID mInstId;
        private CONTROL ctrl;

        public bool Enable;

        public byte No { get; private set; }

        public INST_ID InstId { get { return mInstId; } }

        public KEY_STATUS[] KeyBoard { get; set; }

        public WAVE_INFO[] WaveInfo { get; private set; }

        public byte Vol {
            get { return ctrl.vol; }
        }

        public byte Exp {
            get { return ctrl.exp; }
        }

        public byte Pan {
            get { return ctrl.pan; }
        }

        public byte Rev {
            get { return ctrl.rev; }
        }

        public byte Del {
            get { return ctrl.del; }
        }

        public byte Cho {
            get { return ctrl.cho; }
        }

        public byte Hld {
            get { return ctrl.hold; }
        }

        public byte Fc {
            get { return ctrl.cut; }
        }

        public byte Fq {
            get { return ctrl.res; }
        }

        public byte BendRange {
            get { return ctrl.bendRange; }
        }

        public int Pitch { get; private set; }

        public Channel(Instruments inst, CHANNEL* pChannel, int no) {
            mInst = inst;
            mpChannel = pChannel;
            No = (byte)no;
            KeyBoard = new KEY_STATUS[128];
            Enable = true;
            AllReset();
        }

        /******************************************************************************/
        public void AllReset() {
            mpChannel->wave = 0.0;

            mInstId = new INST_ID();

            setAmp(100, 100);
            setPan(64);

            setHold(0);

            setRes(64);
            setCut(64);
            mpChannel->eq.pole00 = 0.0;
            mpChannel->eq.pole01 = 0.0;
            mpChannel->eq.pole02 = 0.0;
            mpChannel->eq.pole03 = 0.0;
            mpChannel->eq.pole10 = 0.0;
            mpChannel->eq.pole11 = 0.0;
            mpChannel->eq.pole12 = 0.0;
            mpChannel->eq.pole13 = 0.0;

            ctrl.rel = 64;
            ctrl.atk = 64;

            ctrl.vibRate = 64;
            ctrl.vibDepth = 64;
            ctrl.vibDelay = 64;

            ctrl.rev = 0;
            setChorusDepth(0);
            setDelayDepath(0);

            mpChannel->chorus.rate = 0.01;
            mpChannel->delay.rate = 0.2;

            ctrl.nrpnLSB = 0xFF;
            ctrl.nrpnMSB = 0xFF;
            ctrl.rpnLSB = 0xFF;
            ctrl.rpnMSB = 0xFF;

            ctrl.bendRange = 2;

            mpChannel->pitch = 1.0;
            Pitch = 0;

            ProgramChange(0);
        }

        public void CtrlChange(byte type, byte b1) {
            switch ((CTRL_TYPE)type) {
                case CTRL_TYPE.BANK_MSB:
                    mInstId.bankMSB = b1;
                    break;
                case CTRL_TYPE.BANK_LSB:
                    mInstId.bankLSB = b1;
                    break;

                case CTRL_TYPE.DATA:
                    rpn(b1);
                    break;

                case CTRL_TYPE.EXPRESSION:
                    setAmp(ctrl.vol, b1);
                    break;
                case CTRL_TYPE.PAN:
                    setPan(b1);
                    break;
                case CTRL_TYPE.VOLUME:
                    setAmp(b1, ctrl.exp);
                    break;

                case CTRL_TYPE.HOLD:
                    setHold(b1);
                    break;

                case CTRL_TYPE.RESONANCE:
                    setRes(b1);
                    break;
                case CTRL_TYPE.CUTOFF:
                    setCut(b1);
                    break;

                case CTRL_TYPE.RELEACE:
                    ctrl.rel = b1;
                    break;
                case CTRL_TYPE.ATTACK:
                    ctrl.atk = b1;
                    break;

                case CTRL_TYPE.VIB_RATE:
                    ctrl.vibRate = b1;
                    break;
                case CTRL_TYPE.VIB_DEPTH:
                    ctrl.vibDepth = b1;
                    break;
                case CTRL_TYPE.VIB_DELAY:
                    ctrl.vibDelay = b1;
                    break;

                case CTRL_TYPE.REVERB:
                    ctrl.rev = b1;
                    break;
                case CTRL_TYPE.CHORUS:
                    setChorusDepth(b1);
                    break;
                case CTRL_TYPE.DELAY:
                    setDelayDepath(b1);
                    break;

                case CTRL_TYPE.RPN_LSB:
                    ctrl.rpnLSB = b1;
                    break;
                case CTRL_TYPE.RPN_MSB:
                    ctrl.rpnMSB = b1;
                    break;

                case CTRL_TYPE.ALL_RESET:
                    AllReset();
                    break;
            }
        }

        public void ProgramChange(byte value) {
            mInstId.isDrum = (byte)(9 == No ? 0x80 : 0x00);
            mInstId.programNo = value;

            if (!mInst.List.ContainsKey(mInstId)) {
                mInstId.bankMSB = 0;
                mInstId.bankLSB = 0;
                if (!mInst.List.ContainsKey(mInstId)) {
                    mInstId.programNo = 0;
                }
            }

            WaveInfo = mInst.List[mInstId];
        }

        public void PitchBend(byte lsb, byte msb) {
            Pitch = (lsb | (msb << 7)) - 8192;

            var temp = Pitch * ctrl.bendRange;
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
            ctrl.vol = vol;
            ctrl.exp = exp;
            mpChannel->tarAmp = Const.Amp[vol] * Const.Amp[exp];
        }

        private void setPan(byte value) {
            ctrl.pan = value;
            mpChannel->panLeft = Const.Cos[value];
            mpChannel->panRight = Const.Sin[value];
        }

        private void setHold(byte value) {
            if (value < 64) {
                for (byte k = 0; k < 128; ++k) {
                    if (KEY_STATUS.HOLD == KeyBoard[k]) {
                        KeyBoard[k] = KEY_STATUS.OFF;
                    }
                }
            }

            ctrl.hold = value;
            mpChannel->holdDelta = (value < 64 ? 1.0 : 1.0 * Const.DeltaTime);
        }

        private void setRes(byte value) {
            ctrl.res = value;
            mpChannel->tarResonance = (value < 64) ? 0.0 : ((value - 64) / 64.0);
        }

        private void setCut(byte value) {
            ctrl.cut = value;
            mpChannel->tarCutoff = (value < 80) ? Const.Amp[(int)(1.6 * value)] : 1.0;
        }

        private void setDelayDepath(byte value) {
            ctrl.del = value;
            mpChannel->delay.depth = 0.8 * Const.FeedBack[value];
        }

        private void setChorusDepth(byte value) {
            ctrl.cho = value;
            mpChannel->chorus.depth = 2.0 * Const.FeedBack[value];
        }

        private void rpn(byte b1) {
            switch (ctrl.rpnLSB | ctrl.rpnMSB << 8) {
                case 0x0000:
                    ctrl.bendRange = b1;
                    break;
                default:
                    break;
            }

            ctrl.rpnMSB = 0xFF;
            ctrl.rpnLSB = 0xFF;
        }
    }
}
