using System.Collections.Generic;

namespace MIDI {
    unsafe public class Channel {
        private CHANNEL_PARAM* mpChannel = null;
        private Instruments mInstruments = null;
        private INST_ID mInstId;

        public Dictionary<INST_ID, string[]> InstList { get { return mInstruments.Names; } }

        public bool Enable;

        public byte No { get; private set; }

        public INST_ID InstId { get { return mInstId; } }

        public string InstName { get; private set; }

        public KEY_STATUS[] KeyBoard { get; private set; }

        public WAVE_INFO[] WaveInfo { get; private set; }

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

        public Channel(Instruments inst, CHANNEL_PARAM* pChannel, int no) {
            mInstruments = inst;
            mpChannel = pChannel;
            No = (byte)no;
            KeyBoard = new KEY_STATUS[128];
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

            mpChannel->chorusRate = 0.01;
            mpChannel->delayTime = 0.2;

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

        public void CtrlChange(CTRL_TYPE type, byte b1) {
            switch (type) {
                case CTRL_TYPE.BANK_MSB:
                    mInstId.bankMSB = b1;
                    break;
                case CTRL_TYPE.BANK_LSB:
                    mInstId.bankLSB = b1;
                    break;

                case CTRL_TYPE.VOLUME:
                    setAmp(b1, Exp);
                    break;
                case CTRL_TYPE.PAN:
                    setPan(b1);
                    break;
                case CTRL_TYPE.EXPRESSION:
                    setAmp(Vol, b1);
                    break;

                case CTRL_TYPE.MODULATION:
                    Mod = b1;
                    break;

                case CTRL_TYPE.HOLD:
                    setHld(b1);
                    break;
                case CTRL_TYPE.RELEACE:
                    Rel = b1;
                    break;
                case CTRL_TYPE.ATTACK:
                    Atk = b1;
                    break;

                case CTRL_TYPE.RESONANCE:
                    setRes(b1);
                    break;
                case CTRL_TYPE.CUTOFF:
                    setCut(b1);
                    break;

                case CTRL_TYPE.VIB_RATE:
                    VibRate = b1;
                    break;
                case CTRL_TYPE.VIB_DEPTH:
                    VibDepth = b1;
                    break;
                case CTRL_TYPE.VIB_DELAY:
                    VibDelay = b1;
                    break;

                case CTRL_TYPE.REVERB:
                    Rev = b1;
                    break;
                case CTRL_TYPE.CHORUS:
                    setCho(b1);
                    break;
                case CTRL_TYPE.DELAY:
                    setDel(b1);
                    break;

                case CTRL_TYPE.NRPN_LSB:
                    mNrpnLSB = b1;
                    break;
                case CTRL_TYPE.NRPN_MSB:
                    mNrpnMSB = b1;
                    break;
                case CTRL_TYPE.RPN_LSB:
                    mRpnLSB = b1;
                    break;
                case CTRL_TYPE.RPN_MSB:
                    mRpnMSB = b1;
                    break;
                case CTRL_TYPE.DATA:
                    setRpn(b1);
                    setNrpn(b1);
                    break;

                case CTRL_TYPE.ALL_RESET:
                    AllReset();
                    break;
            }
        }

        public void ProgramChange(byte value) {
            mInstId.isDrum = (byte)(9 == No ? 0x80 : 0x00);
            mInstId.programNo = value;

            if (!mInstruments.List.ContainsKey(mInstId)) {
                mInstId.bankMSB = 0;
                mInstId.bankLSB = 0;
                if (!mInstruments.List.ContainsKey(mInstId)) {
                    mInstId.programNo = 0;
                }
            }

            WaveInfo = mInstruments.List[mInstId];
            InstName = mInstruments.Names[mInstId][0];
        }

        public void ProgramChange(byte value, bool isDrum) {
            mInstId.isDrum = (byte)(isDrum ? 0x80 : 0x00);
            mInstId.programNo = value;

            if (!mInstruments.List.ContainsKey(mInstId)) {
                mInstId.bankMSB = 0;
                mInstId.bankLSB = 0;
                if (!mInstruments.List.ContainsKey(mInstId)) {
                    mInstId.programNo = 0;
                }
            }

            WaveInfo = mInstruments.List[mInstId];
            InstName = mInstruments.Names[mInstId][0];
        }

        public void PitchBend(byte lsb, byte msb) {
            Pitch = (lsb | (msb << 7)) - 8192;

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
                for (byte k = 0; k < 128; ++k) {
                    if (KEY_STATUS.HOLD == KeyBoard[k]) {
                        KeyBoard[k] = KEY_STATUS.OFF;
                    }
                }
            }
            Hld = value;
            mpChannel->holdDelta = (value < 64 ? 1.0 : 1.0 * Const.DeltaTime);
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
            mpChannel->delayDepth = 0.8 * Const.FeedBack[value];
        }

        private void setCho(byte value) {
            Cho = value;
            mpChannel->chorusDepth = 2.0 * Const.FeedBack[value];
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
    }
}
