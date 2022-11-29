#ifndef __CHANNEL_PARAMS_H__
#define __CHANNEL_PARAMS_H__

#include "../type.h"

/******************************************************************************/
#pragma pack(push, 1)
typedef struct CHANNEL_PARAM {
    byte is_drum = 0;
    byte bank_msb = 0;
    byte bank_lsb = 0;
    byte prog_num = 0;
    byte enable;
    byte vol;
    byte exp;
    byte pan;
    byte rev_send;
    byte del_send;
    byte cho_send;
    byte mod;
    byte damper;
    byte cutoff;
    byte resonance;
    byte attack;
    byte release;
    byte vib_rate;
    byte vib_depth;
    byte vib_delay;
    byte bend_range;
    int32 pitch;
    double peak_l;
    double peak_r;
    double rms_l;
    double rms_r;
    byte* pName = 0;
    byte* pKeyboard = 0;
} CHANNEL_PARAM;
#pragma pack(pop)

#endif /* __CHANNEL_PARAMS_H__ */
