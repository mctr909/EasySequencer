#pragma once
#include <windows.h>

/******************************************************************************/
typedef struct SYSTEM_VALUE SYSTEM_VALUE;
typedef struct CHANNEL_PARAM CHANNEL_PARAM;

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
#ifdef __cplusplus
extern "C" {
#endif
    void message_createChannels(SYSTEM_VALUE *pSystemValue);
    void message_disposeChannels(SYSTEM_VALUE *pSystemValue);
    __declspec(dllexport) void WINAPI message_send(byte *pMsg);
#ifdef __cplusplus
}
#endif
