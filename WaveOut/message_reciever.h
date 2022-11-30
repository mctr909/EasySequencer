#pragma once
#include "type.h"
#include <windows.h>

/******************************************************************************/
typedef struct CHANNEL_PARAM CHANNEL_PARAM;
class InstList;
class Channel;
class Sampler;

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
#pragma pack(push, 8)
struct SYSTEM_VALUE {
    InstList* cInst_list;
    Sampler** ppSampler;
    Channel** ppChannels;
    CHANNEL_PARAM** ppChannel_params;
    WAVDAT* pWave_table;
    int32 active_count;
    int32 buffer_length;
    int32 buffer_count;
    int32 sample_rate;
    double delta_time;
    double bpm;
    double* pBuffer_l = 0;
    double* pBuffer_r = 0;
};
#pragma pack(pop)

/******************************************************************************/
#ifdef __cplusplus
extern "C" {
#endif
    void synth_create(InstList* pInst_list, int32 sample_rate, int32 buffer_length, int32 buffer_count);
    void synth_dispose();
    void synth_write_buffer(LPSTR pData);
    void synth_write_buffer_perform(SYSTEM_VALUE* pSystemValue, LPSTR pData);
    int32 message_perform(SYSTEM_VALUE* pSystemValue, byte* pMsg);
    __declspec(dllexport) byte* WINAPI synth_inst_list_ptr();
    __declspec(dllexport) CHANNEL_PARAM** WINAPI synth_channel_params_ptr();
    __declspec(dllexport) int32* WINAPI synth_active_counter_ptr();
    __declspec(dllexport) void WINAPI message_send(byte *pMsg);
#ifdef __cplusplus
}
#endif
