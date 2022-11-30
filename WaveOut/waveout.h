#pragma once
#include "type.h"
#include <windows.h>

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
    __declspec(dllexport) byte* WINAPI ptr_inst_list();
    __declspec(dllexport) CHANNEL_PARAM** WINAPI ptr_channel_params();
    __declspec(dllexport) int32* WINAPI ptr_active_counter();
    __declspec(dllexport) void WINAPI send_message(byte port, byte* pMsg);
#ifdef __cplusplus
}
#endif
