#include "message_reciever.h"
#include "synth/channel.h"
#include "synth/channel_const.h"
#include "synth/channel_params.h"
#include "synth/sampler.h"
#include "inst/inst_list.h"

#include <stdio.h>

Channel **message_ppChannels = NULL;

/******************************************************************************/
void message_createChannels(SYSTEM_VALUE *pSystemValue) {
    message_disposeChannels(pSystemValue);
    //
    pSystemValue->ppChannels = (Channel**)calloc(CHANNEL_COUNT, sizeof(Channel*));
    message_ppChannels = pSystemValue->ppChannels;
    for (int32 c = 0; c < CHANNEL_COUNT; c++) {
        pSystemValue->ppChannels[c] = new Channel(pSystemValue, c);
    }
    //
    pSystemValue->ppChannel_params = (CHANNEL_PARAM**)calloc(CHANNEL_COUNT, sizeof(CHANNEL_PARAM*));
    for (int32 i = 0; i < CHANNEL_COUNT; i++) {
        pSystemValue->ppChannel_params[i] = &pSystemValue->ppChannels[i]->param;
    }
}

void message_disposeChannels(SYSTEM_VALUE *pSystemValue) {
    if (NULL != pSystemValue->ppChannel_params) {
        free(pSystemValue->ppChannel_params);
        pSystemValue->ppChannel_params = NULL;
    }
    if (NULL != pSystemValue->ppChannels) {
        for (int32 c = 0; c < CHANNEL_COUNT; c++) {
            delete pSystemValue->ppChannels[c];
            pSystemValue->ppChannels[c] = NULL;
        }
        free(pSystemValue->ppChannels);
        pSystemValue->ppChannels = NULL;
    }
    message_ppChannels = NULL;
}

void message_set_sampler(SYSTEM_VALUE* pSystemValue, INST_INFO* pInstInfo, byte channelNum, byte noteNum, byte velocity) {
    for (uint32 idxS = 0; idxS < SAMPLER_COUNT; idxS++) {
        auto pSmpl = pSystemValue->ppSampler[idxS];
        if (pSmpl->channel_num == channelNum && pSmpl->note_num == noteNum &&
            E_SAMPLER_STATE::PRESS <= pSmpl->state) {
            pSmpl->state = E_SAMPLER_STATE::PURGE;
        }
    }
    auto cInst = pSystemValue->cInst_list;
    auto ppLayer = cInst->mppLayerList + pInstInfo->layerIndex;
    for (uint32 idxL = 0; idxL < pInstInfo->layerCount; idxL++) {
        auto pLayer = ppLayer[idxL];
        auto ppRegion = cInst->mppRegionList + pLayer->regionIndex;
        for (uint32 idxR = 0; idxR < pLayer->regionCount; idxR++) {
            auto pRegion = ppRegion[idxR];
            if (pRegion->keyLow <= noteNum && noteNum <= pRegion->keyHigh &&
                pRegion->velocityLow <= velocity && velocity <= pRegion->velocityHigh) {
                auto pWave = cInst->mppWaveList[pRegion->waveIndex];
                for (uint32 idxS = 0; idxS < SAMPLER_COUNT; idxS++) {
                    auto pSmpl = pSystemValue->ppSampler[idxS];
                    if (E_SAMPLER_STATE::FREE == pSmpl->state) {
                        pSmpl->state = E_SAMPLER_STATE::RESERVED;
                        pSmpl->channel_num = channelNum;
                        pSmpl->note_num = noteNum;
                        pSmpl->index = 0.0;
                        pSmpl->time = 0.0;
                        pSmpl->pWave = pWave;

                        pSmpl->pitch = 1.0;
                        pSmpl->gain = velocity / 127.0 / 32768.0;

                        if (UINT_MAX != pInstInfo->artIndex) {
                            auto pArt = cInst->mppArtList[pInstInfo->artIndex];
                            pSmpl->pan += pArt->pan;
                            //pArt->transpose;
                            pSmpl->pitch *= pArt->pitch;
                            pSmpl->gain *= pArt->gain;
                            pSmpl->pEnv = &pArt->env;
                            pSmpl->eg_amp = 0.0;
                            pSmpl->eg_cutoff = pArt->env.cutoffRise;
                            pSmpl->eg_pitch = pArt->env.pitchRise;
                        }
                        if (UINT_MAX != pLayer->artIndex) {
                            auto pArt = cInst->mppArtList[pLayer->artIndex];
                            pSmpl->pan += pArt->pan;
                            //pArt->transpose;
                            pSmpl->pitch *= pArt->pitch;
                            pSmpl->gain *= pArt->gain;
                            pSmpl->pEnv = &pArt->env;
                            pSmpl->eg_amp = 0.0;
                            pSmpl->eg_cutoff = pArt->env.cutoffRise;
                            pSmpl->eg_pitch = pArt->env.pitchRise;
                        }
                        if (UINT_MAX != pRegion->artIndex) {
                            auto pArt = cInst->mppArtList[pRegion->artIndex];
                            pSmpl->pan += pArt->pan;
                            //pArt->transpose;
                            pSmpl->pitch *= pArt->pitch;
                            pSmpl->gain *= pArt->gain;
                            pSmpl->pEnv = &pArt->env;
                            pSmpl->eg_amp = 0.0;
                            pSmpl->eg_cutoff = pArt->env.cutoffRise;
                            pSmpl->eg_pitch = pArt->env.pitchRise;
                        }

                        auto diffNote = 0;
                        if (UINT_MAX == pRegion->wsmpIndex) {
                            diffNote = noteNum - pWave->unityNote;
                            pSmpl->pitch *= pWave->pitch;
                            pSmpl->gain *= pWave->gain;
                        } else {
                            auto pWsmp = cInst->mppWaveList[pRegion->wsmpIndex];
                            diffNote = noteNum - pWsmp->unityNote;
                            pSmpl->pitch *= pWsmp->pitch;
                            pSmpl->gain *= pWsmp->gain;
                        }

                        if (diffNote < 0) {
                            pSmpl->pitch *= 1.0 / SemiTone[-diffNote];
                        } else {
                            pSmpl->pitch *= SemiTone[diffNote];
                        }

                        pSmpl->pitch *= pWave->sampleRate;
                        pSmpl->state = E_SAMPLER_STATE::PRESS;
                        break;
                    }
                }
                break;
            }
        }
    }
}

/******************************************************************************/
void WINAPI message_send(byte *pMsg) {
    auto type = (E_EVENT_TYPE)(*pMsg & 0xF0);
    auto ch = *pMsg & 0x0F;
    switch (type) {
    case E_EVENT_TYPE::NOTE_OFF:
        message_ppChannels[ch]->note_off(pMsg[1]);
        break;
    case E_EVENT_TYPE::NOTE_ON:
        message_ppChannels[ch]->note_on(pMsg[1], pMsg[2]);
        break;
    case E_EVENT_TYPE::CTRL_CHG:
        message_ppChannels[ch]->ctrl_change(pMsg[1], pMsg[2]);
        break;
    case E_EVENT_TYPE::PROG_CHG:
        message_ppChannels[ch]->program_change(pMsg[1]);
        break;
    case E_EVENT_TYPE::PITCH:
        message_ppChannels[ch]->pitch_bend(((pMsg[2] << 7) | pMsg[1]) - 8192);
        break;
    }
}
