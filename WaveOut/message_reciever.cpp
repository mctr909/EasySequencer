#include <stdio.h>

#include "synth/channel.h"
#include "synth/channel_const.h"
#include "synth/channel_params.h"
#include "synth/sampler.h"
#include "inst/inst_list.h"

#include "message_reciever.h"

/******************************************************************************/
SYSTEM_VALUE gSysValue = { 0 };

/******************************************************************************/
void
synth_create(InstList* pInst_list, int32 sample_rate, int32 buffer_length, int32 buffer_count) {
    synth_dispose();
    gSysValue.active_count = 0;
    gSysValue.buffer_length = buffer_length;
    gSysValue.buffer_count = buffer_count;
    gSysValue.sample_rate = sample_rate;
    gSysValue.delta_time = 1.0 / sample_rate;
    gSysValue.bpm = 120.0;
    /* inst wave */
    gSysValue.cInst_list = pInst_list;
    gSysValue.pWave_table = (WAVDAT*)gSysValue.cInst_list->GetWaveTablePtr();
    /* allocate output buffer */
    gSysValue.pBuffer_l = (double*)calloc(buffer_length, sizeof(double));
    gSysValue.pBuffer_r = (double*)calloc(buffer_length, sizeof(double));
    /* allocate samplers */
    gSysValue.ppSampler = (Sampler**)calloc(SAMPLER_COUNT, sizeof(Sampler*));
    for (uint32 i = 0; i < SAMPLER_COUNT; i++) {
        gSysValue.ppSampler[i] = new Sampler(&gSysValue);
    }
    /* allocate channels */
    gSysValue.ppChannels = (Channel**)calloc(CHANNEL_COUNT, sizeof(Channel*));
    gSysValue.ppChannel_params = (CHANNEL_PARAM**)calloc(CHANNEL_COUNT, sizeof(CHANNEL_PARAM*));
    for (int32 i = 0; i < CHANNEL_COUNT; i++) {
        gSysValue.ppChannels[i] = new Channel(&gSysValue, i);
        gSysValue.ppChannel_params[i] = &gSysValue.ppChannels[i]->param;
    }
}

void
synth_dispose() {
    /* dispose inst wave */
    if (NULL != gSysValue.cInst_list) {
        delete gSysValue.cInst_list;
        gSysValue.cInst_list = NULL;
    }
    /* dispose output buffer */
    if (NULL != gSysValue.pBuffer_l) {
        free(gSysValue.pBuffer_l);
        gSysValue.pBuffer_l = NULL;
    }
    if (NULL != gSysValue.pBuffer_r) {
        free(gSysValue.pBuffer_r);
        gSysValue.pBuffer_r = NULL;
    }
    /* dispose samplers */
    if (NULL != gSysValue.ppSampler) {
        for (uint32 i = 0; i < SAMPLER_COUNT; i++) {
            delete gSysValue.ppSampler[i];
        }
        free(gSysValue.ppSampler);
        gSysValue.ppSampler = NULL;
    }
    /* dispose channels */
    if (NULL != gSysValue.ppChannels) {
        for (int32 c = 0; c < CHANNEL_COUNT; c++) {
            delete gSysValue.ppChannels[c];
            gSysValue.ppChannels[c] = NULL;
        }
        free(gSysValue.ppChannels);
        gSysValue.ppChannels = NULL;
    }
    if (NULL != gSysValue.ppChannel_params) {
        free(gSysValue.ppChannel_params);
        gSysValue.ppChannel_params = NULL;
    }
}

void
synth_write_buffer(LPSTR pData) {
    synth_write_buffer_perform(&gSysValue, pData);
}

void
synth_write_buffer_perform(SYSTEM_VALUE* pSystemValue, LPSTR pData) {
    /* sampler loop */
    int32 activeCount = 0;
    for (int32 i = 0; i < SAMPLER_COUNT; i++) {
        auto pSmpl = pSystemValue->ppSampler[i];
        if (pSmpl->state < Sampler::E_STATE::PURGE) {
            continue;
        }
        if (pSmpl->step()) {
            activeCount++;
        }
    }
    pSystemValue->active_count = activeCount;
    /* channel loop */
    for (int32 i = 0; i < CHANNEL_COUNT; i++) {
        auto pCh = pSystemValue->ppChannels[i];
        pCh->step(pSystemValue->pBuffer_l, pSystemValue->pBuffer_r);
    }
    /* write buffer */
    auto pOutput = (short*)pData;
    for (int32 i = 0, j = 0; i < pSystemValue->buffer_length; i++, j += 2) {
        auto pL = &pSystemValue->pBuffer_l[i];
        auto pR = &pSystemValue->pBuffer_r[i];
        if (*pL < -1.0) {
            *pL = -1.0;
        }
        if (1.0 < *pL) {
            *pL = 1.0;
        }
        if (*pR < -1.0) {
            *pR = -1.0;
        }
        if (1.0 < *pR) {
            *pR = 1.0;
        }
        pOutput[j] = (short)(*pL * 32767);
        pOutput[j + 1] = (short)(*pR * 32767);
        *pL = 0.0;
        *pR = 0.0;
    }
}

int32
message_perform(SYSTEM_VALUE *pSystemValue, byte* pMsg) {
    auto type = (E_EVENT_TYPE)(*pMsg & 0xF0);
    auto ch = *pMsg & 0x0F;
    switch (type) {
    case E_EVENT_TYPE::NOTE_OFF:
        pSystemValue->ppChannels[ch]->note_off(pMsg[1]);
        return 3;
    case E_EVENT_TYPE::NOTE_ON:
        pSystemValue->ppChannels[ch]->note_on(pMsg[1], pMsg[2]);
        return 3;
    case E_EVENT_TYPE::POLY_KEY:
        return 3;
    case E_EVENT_TYPE::CTRL_CHG:
        pSystemValue->ppChannels[ch]->ctrl_change(pMsg[1], pMsg[2]);
        return 3;
    case E_EVENT_TYPE::PROG_CHG:
        pSystemValue->ppChannels[ch]->program_change(pMsg[1]);
        return 2;
    case E_EVENT_TYPE::CH_PRESS:
        return 2;
    case E_EVENT_TYPE::PITCH:
        pSystemValue->ppChannels[ch]->pitch_bend(((pMsg[2] << 7) | pMsg[1]) - 8192);
        return 3;
    case E_EVENT_TYPE::SYS_EX:
        if (0xFF == pMsg[0]) {
            auto type = (E_META_TYPE)pMsg[1];
            auto size = pMsg[2];
            switch (type) {
            case E_META_TYPE::TEMPO:
                pSystemValue->bpm = 60000000.0 / ((pMsg[3] << 16) | (pMsg[4] << 8) | pMsg[5]);
                break;
            default:
                break;
            }
            return size + 3;
        } else {
            return 0;
        }
    default:
        return 0;
    }
}

/******************************************************************************/
byte* WINAPI
synth_inst_list_ptr() {
    return (byte*)gSysValue.cInst_list->GetInstList();
}

CHANNEL_PARAM** WINAPI
synth_channel_params_ptr() {
    return gSysValue.ppChannel_params;
}

int32* WINAPI
synth_active_counter_ptr() {
    return &gSysValue.active_count;
}

void WINAPI
message_send(byte *pMsg) {
    message_perform(&gSysValue, pMsg);
}
