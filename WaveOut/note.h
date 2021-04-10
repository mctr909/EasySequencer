#pragma once
#include "type.h"

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

#pragma pack(push, 4)
typedef struct NOTE {
    byte channelNum;
    byte num;
    E_NOTE_STATE state;
    byte reserved;
    double velocity;
    void* pChannel;
    void* ppSamplers[UNISON_COUNT];
} NOTE;
#pragma pack(pop)

#ifdef __cplusplus
extern "C" {
#endif
    __declspec(dllexport) NOTE** createNotes(int count);
#ifdef __cplusplus
}
#endif
