#ifndef __CHANNEL_PARAMS_H__
#define __CHANNEL_PARAMS_H__

#include "../type.h"

/******************************************************************************/
#pragma pack(push, 1)
typedef struct CHANNEL_PARAM {
    byte isDrum = 0;
    byte bankMSB = 0;
    byte bankLSB = 0;
    byte progNum = 0;
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
    double PeakL;
    double PeakR;
    double RmsL;
    double RmsR;
    byte* pName = 0;
    byte* pKeyBoard = 0;
} CHANNEL_PARAM;
#pragma pack(pop)

#endif /* __CHANNEL_PARAMS_H__ */
