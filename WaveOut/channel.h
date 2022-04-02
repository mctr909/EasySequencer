#pragma once
#include "inst_ck.h"
#include "channel_const.h"
#include <windows.h>

/******************************************************************************/
typedef struct SYSTEM_VALUE SYSTEM_VALUE;
typedef struct EFFECT_PARAM EFFECT_PARAM;

/******************************************************************************/
#pragma pack(push, 1)
typedef struct CHANNEL_PARAM {
    E_KEY_STATE KeyBoard[128] = { };
    INST_ID InstId;
    byte* Name;
    byte Enable;
    byte Vol;
    byte Exp;
    byte Pan;
    byte Rev;
    byte Del;
    byte Cho;
    byte Mod;
    byte Hld;
    byte Fc;
    byte Fq;
    byte Atk;
    byte Rel;
    byte VibRate;
    byte VibDepth;
    byte VibDelay;
    byte BendRange;
    int Pitch;
} CHANNEL_PARAM;
#pragma pack(pop)

/******************************************************************************/
class Channel {
public:
    byte Number;
    CHANNEL_PARAM Param = { };

private:
    SYSTEM_VALUE *mpSystemValue = NULL;
    EFFECT_PARAM *mpEffectParam = NULL;
    REGION** mppRegions = NULL;
    int mRegionCount = 0;

private:
    byte mRpnLSB;
    byte mRpnMSB;
    byte mNrpnLSB;
    byte mNrpnMSB;

public:
    Channel(SYSTEM_VALUE *pSystemValue, int number);

public:
    void AllReset();
    void NoteOff(byte noteNumber);
    void NoteOn(byte noteNumber, byte velocity);
    void CtrlChange(byte type, byte b1);
    void ProgramChange(byte value);
    void PitchBend(short pitch);

private:
    void setAmp(byte vol, byte exp);
    void setPan(byte value);
    void setHld(byte value);
    void setRes(byte value);
    void setCut(byte value);
    void setDel(byte value);
    void setCho(byte value);
    void setRpn(byte b1);
    void setNrpn(byte b1);
    INST_INFO* searchInst(INST_ID id);
};
