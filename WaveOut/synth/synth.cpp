#include <stdio.h>

#include "channel.h"
#include "sampler.h"
#include "../inst/inst_list.h"

#include "synth.h"

/******************************************************************************/
Synth::Synth(InstList* pInst_list, int32 sample_rate, int32 buffer_length) {
    this->sample_rate = sample_rate;
    delta_time = 1.0 / sample_rate;
    this->buffer_length = buffer_length;
    active_count = 0;
    bpm = 120.0;
    /* inst wave */
    this->pInst_list = pInst_list;
    pWave_table = pInst_list->mpWaveTable;
    /* allocate samplers */
    ppSampler = (Sampler**)calloc(SAMPLER_COUNT, sizeof(Sampler*));
    for (uint32 i = 0; i < SAMPLER_COUNT; i++) {
        ppSampler[i] = new Sampler(this);
    }
    /* allocate channels */
    ppChannels = (Channel**)calloc(CHANNEL_COUNT, sizeof(Channel*));
    ppChannel_params = (CHANNEL_PARAM**)calloc(CHANNEL_COUNT, sizeof(CHANNEL_PARAM*));
    for (int32 i = 0; i < CHANNEL_COUNT; i++) {
        ppChannels[i] = new Channel(this, i);
        ppChannel_params[i] = &ppChannels[i]->param;
    }
    /* allocate output buffer */
    pBuffer_l = (double*)calloc(buffer_length, sizeof(double));
    pBuffer_r = (double*)calloc(buffer_length, sizeof(double));
}

Synth::~Synth() {
    /* dispose samplers */
    if (NULL != ppSampler) {
        for (uint32 i = 0; i < SAMPLER_COUNT; i++) {
            delete ppSampler[i];
        }
        free(ppSampler);
        ppSampler = NULL;
    }
    /* dispose channels */
    if (NULL != ppChannels) {
        for (int32 c = 0; c < CHANNEL_COUNT; c++) {
            delete ppChannels[c];
            ppChannels[c] = NULL;
        }
        free(ppChannels);
        ppChannels = NULL;
    }
    if (NULL != ppChannel_params) {
        free(ppChannel_params);
        ppChannel_params = NULL;
    }
    /* dispose output buffer */
    if (NULL != pBuffer_l) {
        free(pBuffer_l);
        pBuffer_l = NULL;
    }
    if (NULL != pBuffer_r) {
        free(pBuffer_r);
        pBuffer_r = NULL;
    }
}

void
Synth::write_buffer(LPSTR pData) {
    /* sampler loop */
    int32 activeCount = 0;
    for (int32 i = 0; i < SAMPLER_COUNT; i++) {
        auto pSmpl = ppSampler[i];
        if (pSmpl->state < Sampler::E_STATE::PURGE) {
            continue;
        }
        pSmpl->step();
        activeCount++;
    }
    this->active_count = activeCount;
    /* channel loop */
    for (int32 i = 0; i < CHANNEL_COUNT; i++) {
        auto pCh = ppChannels[i];
        pCh->step(pBuffer_l, pBuffer_r);
    }
    /* write buffer */
    auto pOutput = (WAVDAT*)pData;
    for (int32 i = 0, j = 0; i < buffer_length; i++, j += 2) {
        auto pL = &pBuffer_l[i];
        auto pR = &pBuffer_r[i];
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
        pOutput[j] = (WAVDAT)(*pL * WAVMAX);
        pOutput[j + 1] = (WAVDAT)(*pR * WAVMAX);
        *pL = 0.0;
        *pR = 0.0;
    }
}

int32
Synth::send_message(byte* pMsg) {
    auto type = (E_EVENT_TYPE)(*pMsg & 0xF0);
    auto ch = *pMsg & 0x0F;
    switch (type) {
    case E_EVENT_TYPE::NOTE_OFF:
        ppChannels[ch]->note_off(pMsg[1]);
        return 3;
    case E_EVENT_TYPE::NOTE_ON:
        ppChannels[ch]->note_on(pMsg[1], pMsg[2]);
        return 3;
    case E_EVENT_TYPE::POLY_KEY:
        return 3;
    case E_EVENT_TYPE::CTRL_CHG:
        ppChannels[ch]->ctrl_change(pMsg[1], pMsg[2]);
        return 3;
    case E_EVENT_TYPE::PROG_CHG:
        ppChannels[ch]->program_change(pMsg[1]);
        return 2;
    case E_EVENT_TYPE::CH_PRESS:
        return 2;
    case E_EVENT_TYPE::PITCH:
        ppChannels[ch]->pitch_bend(((pMsg[2] << 7) | pMsg[1]) - 8192);
        return 3;
    case E_EVENT_TYPE::SYS_EX:
        if (0xFF == pMsg[0]) {
            auto type = (E_META_TYPE)pMsg[1];
            auto size = pMsg[2];
            switch (type) {
            case E_META_TYPE::TEMPO:
                bpm = 60000000.0 / ((pMsg[3] << 16) | (pMsg[4] << 8) | pMsg[5]);
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
