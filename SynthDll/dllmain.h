#pragma once
#include "type.h"

/******************************************************************************/
#ifdef __cplusplus
extern "C" {
#endif
    __declspec(dllexport) byte* WINAPI synth_setup(
        STRING wave_table_path,
        int32 sample_rate,
        int32 buffer_length,
        int32 buffer_count
    );
    __declspec(dllexport) void WINAPI synth_close();
    __declspec(dllexport) void WINAPI fileout(
        STRING wave_table_path,
        STRING save_path,
        uint32 sample_rate,
        uint32 base_tick,
        uint32 event_size,
        byte* p_events,
        int32* p_progress
    );
    __declspec(dllexport) void WINAPI send_message(byte port, byte* p_msg);
#ifdef __cplusplus
}
#endif
