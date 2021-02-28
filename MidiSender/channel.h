#pragma once
#include "../WaveOut/sampler.h"
#include <windows.h>

/******************************************************************************/
enum struct E_CTRL_TYPE : byte {
    BANK_MSB = 0,
    MODULATION = 1,
    PORTA_TIME = 5,
    DATA_MSB = 6,
    VOLUME = 7,
    PAN = 10,
    EXPRESSION = 11,
    BANK_LSB = 32,
    HOLD = 64,
    PORTAMENTO = 65,
    RESONANCE = 71,
    RELEACE = 72,
    ATTACK = 73,
    CUTOFF = 74,
    VIB_RATE = 76,
    VIB_DEPTH = 77,
    VIB_DELAY = 78,
    REVERB = 91,
    CHORUS = 93,
    DELAY = 94,
    NRPN_LSB = 98,
    NRPN_MSB = 99,
    RPN_LSB = 100,
    RPN_MSB = 101,
    ALL_RESET = 121,
    INVALID = 255
};

enum struct E_EVENT_TYPE : byte {
    INVALID = 0x00,
    NOTE_OFF = 0x80,
    NOTE_ON = 0x90,
    POLY_KEY = 0xA0,
    CTRL_CHG = 0xB0,
    PROG_CHG = 0xC0,
    CH_PRESS = 0xD0,
    PITCH = 0xE0,
    SYS_EX = 0xF0,
    META = 0xFF
};

enum struct E_META_TYPE : byte {
    SEQ_NO = 0x00,
    TEXT = 0x01,
    COMPOSER = 0x02,
    SEQ_NAME = 0x03,
    INST_NAME = 0x04,
    LYRIC = 0x05,
    MARKER = 0x06,
    QUEUE = 0x07,
    PRG_NAME = 0x08,
    DEVICE = 0x09,
    CH_PREFIX = 0x20,
    PORT = 0x21,
    TRACK_END = 0x2F,
    TEMPO = 0x51,
    SMPTE = 0x54,
    MEASURE = 0x58,
    KEY = 0x59,
    META = 0x7F,
    INVALID = 0xFF
};

/******************************************************************************/
#pragma pack(push, 4)
typedef struct INST_ID {
    byte isDrum;
    byte programNo;
    byte bankMSB;
    byte bankLSB;
} INST_ID;
#pragma pack(pop)

#pragma pack(push, 4)
typedef struct REGION {
    byte keyLo;
    byte keyHi;
    byte velLo;
    byte velHi;
    WAVE_INFO waveInfo;
    ENVELOPE env;
} REGION;
#pragma pack(pop)

#pragma pack(push, 4)
typedef struct INST_REC {
    INST_ID id;
    int regionCount;
    byte* pName;
    byte* pCategory;
    REGION **ppRegions;
} INST_REC;
#pragma pack(pop)

#pragma pack(push, 4)
typedef struct INST_LIST {
    int instCount;
    INST_REC** ppInst;
} INST_LIST;
#pragma pack(pop)

#pragma pack(push, 1)
typedef struct CHANNEL_PARAM {
    Bool Enable;
    Bool IsOsc;
    INST_ID InstId;
    byte* Name;
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
    INST_LIST *InstList = NULL;
    byte No;
    CHANNEL_PARAM Param = { 0 };

private:
    CHANNEL* mpChannel = NULL;
    SAMPLER** mppSampler = NULL;
    REGION** mppRegions = NULL;
    int mRegionCount = 0;
    int mSamplerCount = 0;

private:
    byte mRpnLSB;
    byte mRpnMSB;
    byte mNrpnLSB;
    byte mNrpnMSB;

public:
    Channel(INST_LIST *inst, SAMPLER** ppSampler, CHANNEL* pChannel, int no, int samplerCount);

public:
    void AllReset();
    void NoteOff(byte noteNo, E_NOTE_STATE keyState);
    void NoteOn(byte noteNo, byte velocity);
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
    INST_REC* searchInst(INST_ID id);
};
