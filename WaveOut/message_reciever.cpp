#include "message_reciever.h"
#include "synth/channel.h"
#include "synth/sampler.h"
#include <stdio.h>

Channel **message_ppChannels = NULL;

/******************************************************************************/
void message_createChannels(SYSTEM_VALUE *pSystemValue) {
    message_disposeChannels(pSystemValue);
    //
    pSystemValue->ppChannels = (Channel**)calloc(CHANNEL_COUNT, sizeof(Channel*));
    message_ppChannels = pSystemValue->ppChannels;
    for (int c = 0; c < CHANNEL_COUNT; c++) {
        pSystemValue->ppChannels[c] = new Channel(pSystemValue, c);
    }
    //
    pSystemValue->ppChannelParam = (CHANNEL_PARAM**)calloc(CHANNEL_COUNT, sizeof(CHANNEL_PARAM*));
    for (int i = 0; i < CHANNEL_COUNT; i++) {
        pSystemValue->ppChannelParam[i] = &pSystemValue->ppChannels[i]->Param;
    }
}

void message_disposeChannels(SYSTEM_VALUE *pSystemValue) {
    if (NULL != pSystemValue->ppChannelParam) {
        free(pSystemValue->ppChannelParam);
        pSystemValue->ppChannelParam = NULL;
    }
    if (NULL != pSystemValue->ppChannels) {
        for (int c = 0; c < CHANNEL_COUNT; c++) {
            delete pSystemValue->ppChannels[c];
            pSystemValue->ppChannels[c] = NULL;
        }
        free(pSystemValue->ppChannels);
        pSystemValue->ppChannels = NULL;
    }
    message_ppChannels = NULL;
}

/******************************************************************************/
void WINAPI message_send(byte *pMsg) {
    auto type = (E_EVENT_TYPE)(*pMsg & 0xF0);
    auto ch = *pMsg & 0x0F;
    switch (type) {
    case E_EVENT_TYPE::NOTE_OFF:
        message_ppChannels[ch]->NoteOff(pMsg[1]);
        break;
    case E_EVENT_TYPE::NOTE_ON:
        message_ppChannels[ch]->NoteOn(pMsg[1], pMsg[2]);
        break;
    case E_EVENT_TYPE::CTRL_CHG:
        message_ppChannels[ch]->CtrlChange(pMsg[1], pMsg[2]);
        break;
    case E_EVENT_TYPE::PROG_CHG:
        message_ppChannels[ch]->ProgramChange(pMsg[1]);
        break;
    case E_EVENT_TYPE::PITCH:
        message_ppChannels[ch]->PitchBend(((pMsg[2] << 7) | pMsg[1]) - 8192);
        break;
    }
}
