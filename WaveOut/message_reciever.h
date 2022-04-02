#pragma once
#include <windows.h>

/******************************************************************************/
typedef struct SYSTEM_VALUE SYSTEM_VALUE;
typedef struct CHANNEL_PARAM CHANNEL_PARAM;

/******************************************************************************/
#ifdef __cplusplus
extern "C" {
#endif
    void WINAPI message_createChannels(SYSTEM_VALUE *pSystemValue);
    __declspec(dllexport) CHANNEL_PARAM** WINAPI message_getChannelParamPtr();
    __declspec(dllexport) void WINAPI message_send(LPBYTE msg);
#ifdef __cplusplus
}
#endif
