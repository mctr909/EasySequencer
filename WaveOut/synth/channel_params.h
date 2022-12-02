#ifndef __CHANNEL_PARAMS_H__
#define __CHANNEL_PARAMS_H__

#include "../type.h"

/******************************************************************************/
#pragma pack(push, 1)
struct CHANNEL_PARAM {
    byte is_drum;
    byte bank_msb;
    byte bank_lsb;
    byte prog_num;
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
    byte* p_name = nullptr;
    byte* p_keyboard = nullptr;
};
#pragma pack(pop)

#endif /* __CHANNEL_PARAMS_H__ */
