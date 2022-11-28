#ifndef __CHANNEL_H__
#define __CHANNEL_H__

#include "../type.h"
#include "filter.h"
#include "channel_params.h"
#include <windows.h>

/******************************************************************************/
typedef struct SYSTEM_VALUE SYSTEM_VALUE;
typedef struct EFFECT_PARAM EFFECT_PARAM;
typedef struct INST_INFO INST_INFO;

/******************************************************************************/
class Channel {
private:
    struct DELAY {
        uint write_index;
        uint time;
        uint tap_length;
        double send;
        double cross;
        double* pTap_l;
        double* pTap_r;
    };
    struct CHORUS {
        double send;
        double depth;
        double rate;
        double pan_a;
        double pan_b;
        double lfo_u;
        double lfo_v;
        double lfo_w;
    };
    typedef struct EFFECT_PARAM {
        double amp;
        double pitch;
        double holdDelta;
        double panLeft;
        double panRight;
        double cutoff;
        double resonance;
        double delaySend;
        double delayTime;
        double delayCross;
        double chorusSend;
        double chorusRate;
        double chorusDepth;
    };
    typedef struct EFFECT {
        int writeIndex;
        double amp;
        double panL;
        double panR;
        double choLfoU;
        double choLfoV;
        double choLfoW;
        double choPanUL;
        double choPanUR;
        double choPanVL;
        double choPanVR;
        double choPanWL;
        double choPanWR;
        double* pDelTapL;
        double* pDelTapR;
        EFFECT_PARAM* pParam;
        SYSTEM_VALUE* pSystemValue;
        FILTER filter;
    };

public:
    byte Number;
    CHANNEL_PARAM Param = { 0 };
    double* pInput = 0;
    EFFECT_PARAM mEffectParam = { 0 };

private:
    SYSTEM_VALUE *mpSystemValue = NULL;
    INST_INFO *mpInst = NULL;

private:
    byte mRpnLSB;
    byte mRpnMSB;
    byte mNrpnLSB;
    byte mNrpnMSB;
    byte mDataLSB;
    byte mDataMSB;
    double current_amp;
    double current_pan_re;
    double current_pan_im;
    double target_amp;
    double target_pan_re;
    double target_pan_im;
    DELAY delay = { 0 };
    CHORUS chorus = { 0 };

public:
    Channel(SYSTEM_VALUE *pSystemValue, int number);
    ~Channel();

public:
    void AllInit();
    void AllReset();
    void NoteOff(byte noteNumber);
    void NoteOn(byte noteNumber, byte velocity);
    void CtrlChange(byte type, byte b1);
    void ProgramChange(byte value);
    void PitchBend(short pitch);
    void Step(double* pOutputL, double* pOutputR);

private:
    void setAmp(byte vol, byte exp);
    void setPan(byte value);
    void setHld(byte value);
    void setRes(byte value);
    void setCut(byte value);
    void setDel(byte value);
    void setCho(byte value);
    void setRpn();
    void setNrpn();
};

#endif /* __CHANNEL_H__ */
