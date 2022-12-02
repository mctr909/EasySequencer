#pragma once
#include "type.h"
#include <windows.h>

/******************************************************************************/
struct SYSTEM_VALUE {
    int32 inst_count;
    byte* p_inst_list;
    byte* p_channel_params;
    int32* p_active_counter;
};

/******************************************************************************/
#ifdef __cplusplus
extern "C" {
#endif
    __declspec(dllexport) void WINAPI waveout_open(
        LPWSTR filePath,
        int32 sampleRate,
        int32 bufferLength,
        int32 bufferCount
    );
    __declspec(dllexport) void WINAPI waveout_close();
    __declspec(dllexport) byte* WINAPI synth_system_value();
    __declspec(dllexport) void WINAPI send_message(byte port, byte* pMsg);
#ifdef __cplusplus
}
#endif
