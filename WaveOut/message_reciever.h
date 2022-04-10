#pragma once
#include <windows.h>

/******************************************************************************/
typedef struct SYSTEM_VALUE SYSTEM_VALUE;
typedef struct CHANNEL_PARAM CHANNEL_PARAM;

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
