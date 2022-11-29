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
    enum struct E_KEY_STATE : byte {
        FREE,
        PRESS,
        HOLD
    };
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

public:
    byte number;
    CHANNEL_PARAM param = { 0 };
    double pitch = 1.0;
    double* pInput_l = 0;
    double* pInput_r = 0;

private:
    SYSTEM_VALUE *mpSystem_value = NULL;
    INST_INFO *mpInst = NULL;

    byte rpn_lsb;
    byte rpn_msb;
    byte nrpn_lsb;
    byte nrpn_msb;
    byte data_lsb;
    byte data_msb;
    double current_amp;
    double current_pan_re;
    double current_pan_im;
    double target_amp;
    double target_pan_re;
    double target_pan_im;
    DELAY delay = { 0 };
    CHORUS chorus = { 0 };

public:
    Channel(SYSTEM_VALUE *pSystem_value, int number);
    ~Channel();

public:
    void init_ctrl();
    void all_reset();
    void note_off(byte note_num);
    void note_on(byte note_num, byte velocity);
    void ctrl_change(byte type, byte b1);
    void program_change(byte value);
    void pitch_bend(short pitch);
    void step(double* pOutput_l, double* pOutput_r);

private:
    void set_amp(byte vol, byte exp);
    void set_pan(byte value);
    void set_hold(byte value);
    void set_res(byte value);
    void set_cut(byte value);
    void set_rpn();
    void set_nrpn();
};

#endif /* __CHANNEL_H__ */
