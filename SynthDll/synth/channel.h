#ifndef __CHANNEL_H__
#define __CHANNEL_H__

#include "../type.h"
#include "filter.h"

/******************************************************************************/
#define CHANNEL_COUNT 256

/******************************************************************************/
const double SEMITONE[128] = {
    1.000000, 1.059463, 1.122462, 1.189207, 1.259921, 1.334840, 1.414214, 1.498307,
    1.587401, 1.681793, 1.781797, 1.887749, 2.000000, 2.118926, 2.244924, 2.378414,
    2.519842, 2.669680, 2.828427, 2.996614, 3.174802, 3.363586, 3.563595, 3.775497,
    4.000000, 4.237852, 4.489848, 4.756828, 5.039684, 5.339359, 5.656854, 5.993228,
    6.349604, 6.727171, 7.127190, 7.550995, 8.000000, 8.475705, 8.979696, 9.513657,
    10.07936, 10.67871, 11.31370, 11.98645, 12.69920, 13.45434, 14.25437, 15.10198,
    16.00000, 16.95141, 17.95939, 19.02731, 20.15873, 21.35743, 22.62741, 23.97291,
    25.39841, 26.90868, 28.50875, 30.20397, 32.00000, 33.90281, 35.91878, 38.05462,
    40.31747, 42.71487, 45.25483, 47.94582, 50.79683, 53.81737, 57.01751, 60.40795,
    64.00000, 67.80563, 71.83757, 76.10925, 80.63494, 85.42975, 90.50966, 95.89165,
    101.5936, 107.6347, 114.0350, 120.8159, 128.0000, 135.6112, 143.6751, 152.2185,
    161.2698, 170.8595, 181.0193, 191.7833, 203.1873, 215.2694, 228.0700, 241.6318,
    256.0000, 271.2225, 287.3502, 304.4370, 322.5397, 341.7190, 362.0386, 383.5666,
    406.3746, 430.5389, 456.1401, 483.2636, 512.0000, 542.4451, 574.7005, 608.8740,
    645.0795, 683.4380, 724.0773, 767.1332, 812.7493, 861.0779, 912.2802, 966.5272,
    1024.000, 1084.890, 1149.401, 1217.748, 1290.159, 1366.876, 1448.154, 1534.266
};

/******************************************************************************/
class Synth;
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
    enum struct E_CTRL_TYPE : byte {
        BANK_MSB = 0,
        MODULATION = 1,
        PORTA_TIME = 5,
        DATA_MSB = 6,
        VOLUME = 7,
        PAN = 10,
        EXPRESSION = 11,
        BANK_LSB = 32,
        DATA_LSB = 38,
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
        ALL_SOUND_OFF = 120,
        ALL_RESET = 121,
        ALL_NOTE_OFF = 123,

        DRUM = 254,
        INVALID = 255
    };
    enum struct E_RPN : uint16 {
        BEND_RANGE = 0x0000,
        VIB_DEPTH_RANGE = 0x0005
    };
    const double PITCH_MSB[64] = {
        1.00000000, 1.00090294, 1.00180670, 1.00271128, 1.00361667, 1.00452287, 1.00542990, 1.00633775,
        1.00724641, 1.00815590, 1.00906621, 1.00997733, 1.01088929, 1.01180206, 1.01271566, 1.01363008,
        1.01454533, 1.01546141, 1.01637831, 1.01729605, 1.01821461, 1.01913400, 1.02005422, 1.02097527,
        1.02189715, 1.02281986, 1.02374341, 1.02466779, 1.02559301, 1.02651906, 1.02744595, 1.02837367,
        1.02930224, 1.03023164, 1.03116188, 1.03209296, 1.03302488, 1.03395764, 1.03489125, 1.03582569,
        1.03676098, 1.03769712, 1.03863410, 1.03957193, 1.04051060, 1.04145012, 1.04239049, 1.04333171,
        1.04427378, 1.04521670, 1.04616047, 1.04710510, 1.04805057, 1.04899690, 1.04994409, 1.05089213,
        1.05184102, 1.05279077, 1.05374138, 1.05469285, 1.05564518, 1.05659837, 1.05755241, 1.05850732
    };
    const double PITCH_LSB[128] = {
        1.00000000, 1.00000705, 1.00001410, 1.00002115, 1.00002820, 1.00003526, 1.00004231, 1.00004936,
        1.00005641, 1.00006346, 1.00007051, 1.00007756, 1.00008462, 1.00009167, 1.00009872, 1.00010577,
        1.00011282, 1.00011988, 1.00012693, 1.00013398, 1.00014103, 1.00014808, 1.00015514, 1.00016219,
        1.00016924, 1.00017629, 1.00018334, 1.00019040, 1.00019745, 1.00020450, 1.00021155, 1.00021861,
        1.00022566, 1.00023271, 1.00023976, 1.00024682, 1.00025387, 1.00026092, 1.00026798, 1.00027503,
        1.00028208, 1.00028914, 1.00029619, 1.00030324, 1.00031029, 1.00031735, 1.00032440, 1.00033145,
        1.00033851, 1.00034556, 1.00035262, 1.00035967, 1.00036672, 1.00037378, 1.00038083, 1.00038788,
        1.00039494, 1.00040199, 1.00040904, 1.00041610, 1.00042315, 1.00043021, 1.00043726, 1.00044432,
        1.00045137, 1.00045842, 1.00046548, 1.00047253, 1.00047959, 1.00048664, 1.00049370, 1.00050075,
        1.00050781, 1.00051486, 1.00052191, 1.00052897, 1.00053602, 1.00054308, 1.00055013, 1.00055719,
        1.00056424, 1.00057130, 1.00057835, 1.00058541, 1.00059246, 1.00059952, 1.00060657, 1.00061363,
        1.00062069, 1.00062774, 1.00063480, 1.00064185, 1.00064891, 1.00065596, 1.00066302, 1.00067007,
        1.00067713, 1.00068419, 1.00069124, 1.00069830, 1.00070535, 1.00071241, 1.00071947, 1.00072652,
        1.00073358, 1.00074064, 1.00074769, 1.00075475, 1.00076180, 1.00076886, 1.00077592, 1.00078297,
        1.00079003, 1.00079709, 1.00080414, 1.00081120, 1.00081826, 1.00082531, 1.00083237, 1.00083943,
        1.00084648, 1.00085354, 1.00086060, 1.00086766, 1.00087471, 1.00088177, 1.00088883, 1.00089589
    };

private:
    const double STOP_AMP = 1 / 32768.0; /* -90db */
    const double START_AMP = 1 / 1024.0; /* -60db */
    const double RMS_ATTENUTE = 9.24;    /* -40db/sec * -0.2310 */

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
    E_STATE state = E_STATE::FREE;
    byte number = 0;
    CHANNEL_PARAM param = { 0 };
    double pitch = 1.0;
    double* p_input_l = nullptr;
    double* p_input_r = nullptr;
    INST_INFO* p_inst = nullptr;

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
