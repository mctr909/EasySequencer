#ifndef __SYNTH_H__
#define __SYNTH_H__

#include "../type.h"

/******************************************************************************/
#define CHANNEL_COUNT    256
#define SAMPLER_COUNT    64
#define OVER_SAMPLING    8
#define PURGE_SPEED      0.125
#define FREE_THRESHOLD   (1 / 32768.0) /* -90db */
#define ACTIVE_THRESHOLD (1 / 1024.0)  /* -60db */
#define RMS_ATTENUTE     9.24          /* -40db/sec * -0.2310 */

/******************************************************************************/
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
struct CHANNEL_PARAM;
struct INST_INFO;
class InstList;
class Channel;
class Sampler;

/******************************************************************************/
class Synth {
public:
#pragma pack(push, 4)
    struct {
        int32 inst_count;
        INST_INFO** pp_inst_list;
        CHANNEL_PARAM** pp_channel_params;
        int32* p_active_counter;
    } m_export_values = { 0 };
#pragma pack(pop)

public:
    int32 m_buffer_length = 256;
    int32 m_sample_rate = 44100;
    double m_delta_time = 1.0 / 44100;
    double m_bpm = 120.0;
    InstList* mp_inst_list = nullptr;
    WAVE_DATA* mp_wave_table = nullptr;
    Sampler** mpp_samplers = nullptr;

private:
    int32 m_active_count = 0;
    double* mp_buffer_l = nullptr;
    double* mp_buffer_r = nullptr;
    Channel** mpp_channels = nullptr;

private:
    int32 sys_ex(byte* p_data);
    int32 meta_data(byte* p_data);
    void dispose();

public:
    Synth() {}
    ~Synth() { dispose(); }
    E_LOAD_STATUS setup(STRING wave_table_path, int32 sample_rate, int32 buffer_length);
    static void write_buffer(WAVE_DATA* p_pcm, void* p_param);
    bool save_wav(STRING save_path, uint32 base_tick, uint32 event_size, byte* p_events, int32* p_progress);
    int32 send_message(byte port, byte* p_msg);
};

#endif /* __SYNTH_H__ */
