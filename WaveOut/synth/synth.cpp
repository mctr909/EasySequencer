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
    mpInst_list = pInst_list;
    mpWave_table = pInst_list->mpWaveTable;
    /* allocate samplers */
    mppSampler = (Sampler**)calloc(SAMPLER_COUNT, sizeof(Sampler*));
    for (uint32 i = 0; i < SAMPLER_COUNT; i++) {
        mppSampler[i] = new Sampler(this);
    }
    /* allocate channels */
    mppChannels = (Channel**)calloc(CHANNEL_COUNT, sizeof(Channel*));
    mppChannel_params = (CHANNEL_PARAM**)calloc(CHANNEL_COUNT, sizeof(CHANNEL_PARAM*));
    for (int32 i = 0; i < CHANNEL_COUNT; i++) {
        mppChannels[i] = new Channel(this, i);
        mppChannel_params[i] = &mppChannels[i]->param;
    }
    /* allocate output buffer */
    mpBuffer_l = (double*)calloc(buffer_length, sizeof(double));
    mpBuffer_r = (double*)calloc(buffer_length, sizeof(double));
}

Synth::~Synth() {
    /* dispose samplers */
    if (NULL != mppSampler) {
        for (uint32 i = 0; i < SAMPLER_COUNT; i++) {
            delete mppSampler[i];
        }
        free(mppSampler);
        mppSampler = NULL;
    }
    /* dispose channels */
    if (NULL != mppChannels) {
        for (int32 c = 0; c < CHANNEL_COUNT; c++) {
            delete mppChannels[c];
            mppChannels[c] = NULL;
        }
        free(mppChannels);
        mppChannels = NULL;
    }
    if (NULL != mppChannel_params) {
        free(mppChannel_params);
        mppChannel_params = NULL;
    }
    /* dispose output buffer */
    if (NULL != mpBuffer_l) {
        free(mpBuffer_l);
        mpBuffer_l = NULL;
    }
    if (NULL != mpBuffer_r) {
        free(mpBuffer_r);
        mpBuffer_r = NULL;
    }
}

void
Synth::write_buffer(byte* pData) {
    /* sampler loop */
    int32 activeCount = 0;
    for (int32 i = 0; i < SAMPLER_COUNT; i++) {
        auto pSmpl = mppSampler[i];
        if (pSmpl->state < Sampler::E_STATE::PURGE) {
            continue;
        }
        pSmpl->step();
        activeCount++;
    }
    this->active_count = activeCount;
    /* channel loop */
    for (int32 i = 0; i < CHANNEL_COUNT; i++) {
        auto pCh = mppChannels[i];
        if (Channel::E_STATE::FREE == pCh->state) {
            continue;
        }
        pCh->step(mpBuffer_l, mpBuffer_r);
    }
    /* write buffer */
    auto pOutput = (WAVDAT*)pData;
    for (int32 i = 0, j = 0; i < buffer_length; i++, j += 2) {
        auto pL = &mpBuffer_l[i];
        auto pR = &mpBuffer_r[i];
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
Synth::send_message(byte port, byte* pMsg) {
    auto type = (E_EVENT_TYPE)(*pMsg & 0xF0);
    auto ch = (port << 4) | (*pMsg & 0x0F);
    switch (type) {
    case E_EVENT_TYPE::NOTE_OFF:
        mppChannels[ch]->note_off(pMsg[1]);
        return 3;
    case E_EVENT_TYPE::NOTE_ON:
        mppChannels[ch]->note_on(pMsg[1], pMsg[2]);
        return 3;
    case E_EVENT_TYPE::POLY_KEY:
        return 3;
    case E_EVENT_TYPE::CTRL_CHG:
        mppChannels[ch]->ctrl_change(pMsg[1], pMsg[2]);
        return 3;
    case E_EVENT_TYPE::PROG_CHG:
        mppChannels[ch]->program_change(pMsg[1]);
        return 2;
    case E_EVENT_TYPE::CH_PRESS:
        return 2;
    case E_EVENT_TYPE::PITCH:
        mppChannels[ch]->pitch_bend(((pMsg[2] << 7) | pMsg[1]) - 8192);
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
