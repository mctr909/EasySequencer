#ifndef __CHANNEL_H__
#define __CHANNEL_H__

#include "synth.h"

/******************************************************************************/
typedef struct INST_INFO INST_INFO;

/******************************************************************************/
#pragma pack(push, 4)
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
    byte reserved1;
    byte reserved2;
    byte reserved3;

    int32 pitch;

    double rms_l;
    double rms_r;
    double peak_l;
    double peak_r;

    byte* p_keyboard = nullptr;
    byte* p_name = nullptr;
};
#pragma pack(pop)

/******************************************************************************/
class Channel {
public:
    enum struct E_STATE {
        FREE,
        STANDBY,
        ACTIVE
    };

private:
    enum struct E_KEY_STATE : byte {
        FREE,
        PRESS,
        HOLD
    };
    enum struct E_RPN : uint16 {
        BEND_RANGE = 0x0000,
        VIB_DEPTH_RANGE = 0x0005
    };
    static const double PITCH_MSB[64];
    static const double PITCH_LSB[128];

private:
    struct DELAY {
        int32 index;
        int32 time;
        int32 tap_length;
        double send;
        double cross;
        double* p_tap_l;
        double* p_tap_r;
    };
    struct CHORUS {
        double send;
        double depth;
        double rate;
        double lfo_u;
        double lfo_v;
        double lfo_w;
    };

public:
    byte m_num = 0;
    E_STATE m_state = E_STATE::FREE;
    CHANNEL_PARAM m_param = { 0 };
    double m_pitch = 1.0;
    double* mp_buffer_l = nullptr;
    double* mp_buffer_r = nullptr;
    INST_INFO* mp_inst = nullptr;

private:
    byte m_rpn_lsb = 0xFF;
    byte m_rpn_msb = 0xFF;
    byte m_nrpn_lsb = 0xFF;
    byte m_nrpn_msb = 0xFF;
    byte m_data_lsb = 0;
    byte m_data_msb = 0;
    double m_current_amp = 10000 / 16129.0;
    double m_current_pan_re = 1.0;
    double m_current_pan_im = 0.0;
    double m_target_amp = 10000 / 16129.0;
    double m_target_pan_re = 1.0;
    double m_target_pan_im = 0.0;
    DELAY m_delay = { 0 };
    CHORUS m_chorus = { 0 };
    Synth* mp_synth = nullptr;

public:
    Channel(Synth* p_synth, int32 number);
    ~Channel();

public:
    void init_ctrl();
    void all_reset();
    void note_off(byte note_num);
    void note_on(byte note_num, byte velocity);
    void ctrl_change(byte type, byte value);
    void program_change(byte value);
    void pitch_bend(byte lsb, byte msb);
    void step(double* p_output_l, double* p_output_r);

private:
    Channel() {}
    void set_amp(byte vol, byte exp);
    void set_pan(byte value);
    void set_damper(byte value);
    void set_res(byte value);
    void set_cut(byte value);
    void set_rpn();
    void set_nrpn();
};

#endif /* __CHANNEL_H__ */
