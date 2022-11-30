#ifndef __SYNTH_H__
#define __SYNTH_H__

#include "../type.h"

#include <windows.h>

/******************************************************************************/
struct CHANNEL_PARAM;
class InstList;
class Channel;
class Sampler;

/******************************************************************************/
class Synth {
private:
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

public:
    int32 active_count;
    int32 buffer_length;
    int32 sample_rate;
    double delta_time;
    double bpm;
    double* pBuffer_l = 0;
    double* pBuffer_r = 0;
    InstList* pInst_list;
    WAVDAT* pWave_table;
    Sampler** ppSampler;
    Channel** ppChannels;
    CHANNEL_PARAM** ppChannel_params;

public:
    Synth(InstList* pInst_list, int32 sample_rate, int32 buffer_length);
    ~Synth();
    void write_buffer(LPSTR pData);
    int32 send_message(byte* pMsg);
};

#endif /* __SYNTH_H__ */
