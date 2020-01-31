#pragma once
#include "type.h"
#include "filter.h"
#include "channel.h"

/******************************************************************************/
enum E_KEY_STATE {
    E_KEY_STATE_WAIT,
    E_KEY_STATE_PURGE,
    E_KEY_STATE_RELEASE,
    E_KEY_STATE_HOLD,
    E_KEY_STATE_PRESS
};

/******************************************************************************/
#pragma pack(push, 8)
typedef struct ENVELOPE {
    double deltaA;
    double deltaD;
    double deltaR;
    double levelR;
    double levelT;
    double levelS;
    double levelF;
    double holdTime;
} ENVELOPE;
#pragma pack(pop)

#pragma pack(push, 4)
typedef struct WAVE_LOOP {
    UInt32 begin;
    UInt32 length;
    bool enable;
    byte type;
    byte reserved1;
    byte reserved2;
} WAVE_LOOP;
#pragma pack(pop)

#pragma pack(push, 4)
typedef struct SAMPLER {
    UInt16 channelNo;
    byte   noteNo;
    byte   state;
    UInt32 waveOfs;

    double gain;
    double delta;
    double index;
    double time;
    double amp;
    double velocity;

    WAVE_LOOP loop;
    ENVELOPE envAmp;
    ENVELOPE envEq;
    FILTER filter;
} SAMPLER;
#pragma pack(pop)

/******************************************************************************/
extern SAMPLER* createSampler();
extern void releaseSampler(SAMPLER *pSmpl);
extern inline void sampler(CHANNEL **ppCh, SAMPLER *pSmpl, byte *pWaveBuffer);
