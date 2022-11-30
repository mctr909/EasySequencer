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
waveout_create(InstList* pInst_list, int32 sample_rate, int32 buffer_length) {
    synth_create(&gSysValue, pInst_list, sample_rate, buffer_length);
}

void
waveout_dispose() {
    synth_dispose(&gSysValue);
}

void
synth_create(SYSTEM_VALUE* pSystemValue, InstList* pInst_list, int32 sample_rate, int32 buffer_length) {
    synth_dispose(pSystemValue);
    pSystemValue->sample_rate = sample_rate;
    pSystemValue->delta_time = 1.0 / sample_rate;
    pSystemValue->buffer_length = buffer_length;
    pSystemValue->active_count = 0;
    pSystemValue->bpm = 120.0;
    /* inst wave */
    pSystemValue->pInst_list = pInst_list;
    pSystemValue->pWave_table = (WAVDAT*)pSystemValue->pInst_list->GetWaveTablePtr();
    /* allocate output buffer */
    pSystemValue->pBuffer_l = (double*)calloc(buffer_length, sizeof(double));
    pSystemValue->pBuffer_r = (double*)calloc(buffer_length, sizeof(double));
    /* allocate samplers */
    pSystemValue->ppSampler = (Sampler**)calloc(SAMPLER_COUNT, sizeof(Sampler*));
    for (uint32 i = 0; i < SAMPLER_COUNT; i++) {
        pSystemValue->ppSampler[i] = new Sampler(pSystemValue);
    }
    /* allocate channels */
    pSystemValue->ppChannels = (Channel**)calloc(CHANNEL_COUNT, sizeof(Channel*));
    pSystemValue->ppChannel_params = (CHANNEL_PARAM**)calloc(CHANNEL_COUNT, sizeof(CHANNEL_PARAM*));
    for (int32 i = 0; i < CHANNEL_COUNT; i++) {
        pSystemValue->ppChannels[i] = new Channel(pSystemValue, i);
        pSystemValue->ppChannel_params[i] = &pSystemValue->ppChannels[i]->param;
    }
}

void
synth_dispose(SYSTEM_VALUE* pSystemValue) {
    /* dispose inst wave */
    if (NULL != pSystemValue->pInst_list) {
        delete pSystemValue->pInst_list;
        pSystemValue->pInst_list = NULL;
    }
    /* dispose output buffer */
    if (NULL != pSystemValue->pBuffer_l) {
        free(pSystemValue->pBuffer_l);
        pSystemValue->pBuffer_l = NULL;
    }
    if (NULL != pSystemValue->pBuffer_r) {
        free(pSystemValue->pBuffer_r);
        pSystemValue->pBuffer_r = NULL;
    }
    /* dispose samplers */
    if (NULL != pSystemValue->ppSampler) {
        for (uint32 i = 0; i < SAMPLER_COUNT; i++) {
            delete pSystemValue->ppSampler[i];
        }
        free(pSystemValue->ppSampler);
        pSystemValue->ppSampler = NULL;
    }
    /* dispose channels */
    if (NULL != pSystemValue->ppChannels) {
        for (int32 c = 0; c < CHANNEL_COUNT; c++) {
            delete pSystemValue->ppChannels[c];
            pSystemValue->ppChannels[c] = NULL;
        }
        free(pSystemValue->ppChannels);
        pSystemValue->ppChannels = NULL;
    }
    if (NULL != pSystemValue->ppChannel_params) {
        free(pSystemValue->ppChannel_params);
        pSystemValue->ppChannel_params = NULL;
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
    return (byte*)gSysValue.pInst_list->GetInstList();
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
