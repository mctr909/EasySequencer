#pragma once
#include "type.h"
#include <windows.h>

/******************************************************************************/
#ifdef __cplusplus
extern "C" {
#endif
    __declspec(dllexport) byte* WINAPI synth_setup(
        LPWSTR file_path,
        int32 sample_rate,
        int32 buffer_length,
        int32 buffer_count
    );
    __declspec(dllexport) void WINAPI synth_close();
    __declspec(dllexport) void WINAPI fileout(
        LPWSTR wave_table_path,
        LPWSTR save_path,
        uint32 sample_rate,
        uint32 base_tick,
        uint32 event_size,
        byte* p_events
    );
    __declspec(dllexport) void WINAPI send_message(byte port, byte* p_msg);
#ifdef __cplusplus
}
#endif
