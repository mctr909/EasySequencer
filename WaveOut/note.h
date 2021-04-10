#pragma once
#include "type.h"
#include "sampler.h"

#define UNISON_COUNT 8

/******************************************************************************/
enum struct E_NOTE_STATE : byte {
    FREE,
    RESERVED,
    PRESS,
    RELEASE,
    HOLD,
    PURGE
};

class Note {
public:
    byte mChannelNum;
    byte mNum;
    E_NOTE_STATE mState;
    byte mReserved;
    double mVelocity;
    void* mpChannel;
    void* mppSamplers[UNISON_COUNT];
private:
    Note();
public:
    __declspec(dllexport) static Note** create(int count);
};
