#pragma once
#include "type.h"
#include <windows.h>

/******************************************************************************/
#ifdef __cplusplus
extern "C" {
#endif
__declspec(dllexport) int32* WINAPI fileout_progress_ptr();
__declspec(dllexport) void WINAPI fileout_save(
    LPWSTR wave_table_path,
    LPWSTR save_path,
    uint32 sample_rate,
    byte *pEvents,
    uint32 event_size,
    uint32 base_tick
);
#ifdef __cplusplus
}
#endif
