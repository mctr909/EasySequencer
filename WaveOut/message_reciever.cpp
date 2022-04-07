#include "message_reciever.h"
#include "synth/channel.h"
#include "synth/sampler.h"
#include <stdio.h>

/******************************************************************************/
Channel **gppChannels = NULL;
CHANNEL_PARAM **gppChParam = NULL;

/******************************************************************************/
void message_createChannels(SYSTEM_VALUE *pSystemValue) {
    message_disposeChannels(pSystemValue);
    //
    gppChannels = (Channel**)malloc(sizeof(Channel*) * CHANNEL_COUNT);
    for (int c = 0; c < CHANNEL_COUNT; c++) {
        gppChannels[c] = new Channel(pSystemValue, c);
    }
    //
    gppChParam = (CHANNEL_PARAM**)malloc(sizeof(CHANNEL_PARAM*) * CHANNEL_COUNT);
    for (int i = 0; i < CHANNEL_COUNT; i++) {
        gppChParam[i] = &gppChannels[i]->Param;
    }
}

void message_disposeChannels(SYSTEM_VALUE *pSystemValue) {
    if (NULL != gppChParam) {
        free(gppChParam);
        gppChParam = NULL;
    }
    if (NULL != gppChannels) {
        for (int c = 0; c < CHANNEL_COUNT; c++) {
            delete gppChannels[c];
            gppChannels[c] = NULL;
        }
        free(gppChannels);
        gppChannels = NULL;
    }
}

/******************************************************************************/
CHANNEL_PARAM** WINAPI message_getChannelParamPtr() {
    return gppChParam;
}

void WINAPI message_send(LPBYTE msg) {
    auto type = (E_EVENT_TYPE)(*msg & 0xF0);
    auto ch = *msg & 0x0F;
    switch (type) {
    case E_EVENT_TYPE::NOTE_OFF:
        gppChannels[ch]->NoteOff(msg[1]);
        break;
    case E_EVENT_TYPE::NOTE_ON:
        gppChannels[ch]->NoteOn(msg[1], msg[2]);
        break;
    case E_EVENT_TYPE::CTRL_CHG:
        gppChannels[ch]->CtrlChange(msg[1], msg[2]);
        break;
    case E_EVENT_TYPE::PROG_CHG:
        gppChannels[ch]->ProgramChange(msg[1]);
        break;
    case E_EVENT_TYPE::PITCH:
        gppChannels[ch]->PitchBend(((msg[2] << 7) | msg[1]) - 8192);
        break;
    }
}
