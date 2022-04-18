#include "inst_list.h"
#include "dls.h"
#include "../synth/channel.h"

/******************************************************************************/
#define INVALID_INDEX 0xFFFFFFFF

/******************************************************************************/
InstList::InstList() {
    mppSampler = (INST_SAMPLER**)calloc(SAMPLER_COUNT, sizeof(INST_SAMPLER*));
    for (uint i = 0; i < SAMPLER_COUNT; i++) {
        INST_SAMPLER smpl;
        mppSampler[i] = (INST_SAMPLER*)malloc(sizeof(INST_SAMPLER));
        memcpy_s(mppSampler[i], sizeof(INST_SAMPLER), &smpl, sizeof(INST_SAMPLER));
    }
}

InstList::~InstList() {
    if (NULL != mppSampler) {
        for (uint i = 0; i < SAMPLER_COUNT; i++) {
            free(mppSampler[i]);
        }
        free(mppSampler);
    }
    if (NULL != mppWaveList) {
        for (uint i = 0; i < mWaveCount; i++) {
            free(mppWaveList[i]);
        }
        free(mppWaveList);
    }
    if (NULL != mInstList.ppData) {
        for (uint i = 0; i < mInstList.count; i++) {
            auto pInst = mInstList.ppData[i];
            free(pInst->pName);
            free(pInst->pCategory);
            free(mInstList.ppData[i]);
        }
        free(mInstList.ppData);
    }
    if (NULL != mppLayerList) {
        for (uint i = 0; i < mLayerCount; i++) {
            free(mppLayerList[i]);
        }
        free(mppLayerList);
    }
    if (NULL != mppRegionList) {
        for (uint i = 0; i < mRegionCount; i++) {
            free(mppRegionList[i]);
        }
        free(mppRegionList);
    }
    if (NULL != mppArtList) {
        for (uint i = 0; i < mArtCount; i++) {
            free(mppArtList[i]);
        }
        free(mppArtList);
    }
    if (NULL != mpWaveTable) {
        free(mpWaveTable);
    }
}

E_LOAD_STATUS InstList::Load(LPWSTR path) {
    return loadDls(path);
}

INST_LIST *InstList::GetInstList() {
    return &mInstList;
}

INST_INFO *InstList::GetInstInfo(INST_ID *id) {
    for (uint i = 0; i < mInstList.count; i++) {
        auto listId = mInstList.ppData[i]->id;
        if (listId.isDrum == id->isDrum &&
            listId.bankMSB == id->bankMSB &&
            listId.bankLSB == id->bankLSB &&
            listId.progNum == id->progNum) {
            return mInstList.ppData[i];
        }
    }
    for (uint i = 0; i < mInstList.count; i++) {
        auto listId = mInstList.ppData[i]->id;
        if (listId.isDrum == id->isDrum &&
            listId.bankMSB == 0 &&
            listId.bankLSB == 0 &&
            listId.progNum == id->progNum) {
            return mInstList.ppData[i];
        }
    }
    for (uint i = 0; i < mInstList.count; i++) {
        auto listId = mInstList.ppData[i]->id;
        if (listId.isDrum == id->isDrum &&
            listId.bankMSB == 0 &&
            listId.bankLSB == 0 &&
            listId.progNum == 0) {
            return mInstList.ppData[i];
        }
    }
    return mInstList.ppData[0];
}

INST_SAMPLER **InstList::GetSamplerPtr() {
    return mppSampler;
}

WAVDAT *InstList::GetWaveTablePtr() {
    return mpWaveTable;
}

void InstList::SetSampler(INST_INFO *pInstInfo, byte channelNum, byte noteNum, byte velocity) {
    for (uint idxS = 0; idxS < SAMPLER_COUNT; idxS++) {
        auto pSmpl = mppSampler[idxS];
        if (pSmpl->channelNum == channelNum && pSmpl->noteNum == noteNum &&
            E_SAMPLER_STATE::PRESS <= pSmpl->state) {
            pSmpl->state = E_SAMPLER_STATE::PURGE;
        }
    }
    auto ppLayer = mppLayerList + pInstInfo->layerIndex;
    for (uint idxL = 0; idxL < pInstInfo->layerCount; idxL++) {
        auto pLayer = ppLayer[idxL];
        auto ppRegion = mppRegionList + pLayer->regionIndex;
        for (uint idxR = 0; idxR < pLayer->regionCount; idxR++) {
            auto pRegion = ppRegion[idxR];
            if (pRegion->keyLow <= noteNum && noteNum <= pRegion->keyHigh &&
                pRegion->velocityLow <= velocity && velocity <= pRegion->velocityHigh) {
                auto pWave = mppWaveList[pRegion->waveIndex];
                for (uint idxS = 0; idxS < SAMPLER_COUNT; idxS++) {
                    auto pSmpl = mppSampler[idxS];
                    if (E_SAMPLER_STATE::FREE == pSmpl->state) {
                        pSmpl->state = E_SAMPLER_STATE::RESERVED;
                        pSmpl->channelNum = channelNum;
                        pSmpl->noteNum = noteNum;
                        pSmpl->index = 0.0;
                        pSmpl->time = 0.0;
                        pSmpl->pWave = pWave;

                        pSmpl->pitch = 1.0;
                        pSmpl->gain = velocity / 127.0 / 32768.0;

                        if (INVALID_INDEX != pInstInfo->artIndex) {
                            auto pArt = mppArtList[pInstInfo->artIndex];
                            pSmpl->pan += pArt->pan;
                            //pArt->transpose;
                            pSmpl->pitch *= pArt->pitch;
                            pSmpl->gain *= pArt->gain;
                            pSmpl->pEnv = &pArt->env;
                            pSmpl->egAmp = 0.0;
                            pSmpl->egCutoff = pArt->env.cutoffRise;
                            pSmpl->egPitch = pArt->env.pitchRise;
                        }
                        if (INVALID_INDEX != pLayer->artIndex) {
                            auto pArt = mppArtList[pLayer->artIndex];
                            pSmpl->pan += pArt->pan;
                            //pArt->transpose;
                            pSmpl->pitch *= pArt->pitch;
                            pSmpl->gain *= pArt->gain;
                            pSmpl->pEnv = &pArt->env;
                            pSmpl->egAmp = 0.0;
                            pSmpl->egCutoff = pArt->env.cutoffRise;
                            pSmpl->egPitch = pArt->env.pitchRise;
                        }
                        if (INVALID_INDEX != pRegion->artIndex) {
                            auto pArt = mppArtList[pRegion->artIndex];
                            pSmpl->pan += pArt->pan;
                            //pArt->transpose;
                            pSmpl->pitch *= pArt->pitch;
                            pSmpl->gain *= pArt->gain;
                            pSmpl->pEnv = &pArt->env;
                            pSmpl->egAmp = 0.0;
                            pSmpl->egCutoff = pArt->env.cutoffRise;
                            pSmpl->egPitch = pArt->env.pitchRise;
                        }

                        auto diffNote = 0;
                        if (INVALID_INDEX == pRegion->wsmpIndex) {
                            diffNote = noteNum - pWave->unityNote;
                            pSmpl->pitch *= pWave->pitch;
                            pSmpl->gain *= pWave->gain;
                        } else {
                            auto pWsmp = mppWaveList[pRegion->wsmpIndex];
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
E_LOAD_STATUS InstList::loadDls(LPWSTR path) {
    auto cDls = new DLS();

    if (!cDls->Load(path)) {
        return E_LOAD_STATUS::WAVE_TABLE_OPEN_FAILED;
    }

    /* count layer/region/art/wave */
    mWaveCount = cDls->WaveCount;
    mInstList.count = cDls->InstCount;
    for (uint idxI = 0; idxI < mInstList.count; idxI++) {
        auto cDlsInst = cDls->cLins->pcInst[idxI];
        if (NULL != cDlsInst->cLart) {
            mArtCount++;
        }
        uint rgnLayer = 0;
        for (int idxR = 0; idxR < cDlsInst->cLrgn->Count; idxR++) {
            auto cDlsRgn = cDlsInst->cLrgn->pcRegion[idxR];
            if (NULL != cDlsRgn->cLart) {
                mArtCount++;
            }
            if (NULL != cDlsRgn->pWaveSmpl) {
                mWaveCount++;
            }
            if (rgnLayer < cDlsRgn->Header.layer + 1) {
                rgnLayer = cDlsRgn->Header.layer + 1;
            }
            mRegionCount++;
        }
        mLayerCount += rgnLayer;
    }

    /* allocate inst/layer/region/art/wave */
    mInstList.ppData = (INST_INFO**)calloc(mInstList.count, sizeof(INST_INFO*));
    mppLayerList = (INST_LAYER**)calloc(mLayerCount, sizeof(INST_LAYER*));
    mppRegionList = (INST_REGION**)calloc(mRegionCount, sizeof(INST_REGION*));
    mppArtList = (INST_ART**)calloc(mArtCount, sizeof(INST_ART*));
    mppWaveList = (INST_WAVE**)calloc(mWaveCount, sizeof(INST_WAVE*));

    /* load wave */
    swprintf_s(mWaveTablePath, sizeof(mWaveTablePath) / sizeof(mWaveTablePath[0]), TEXT("%s.bin"), path);
    loadDlsWave(cDls);

    /* load inst/layer/region/art */
    uint layerIndex = 0;
    uint regionIndex = 0;
    uint artIndex = 0;
    uint waveIndex = cDls->WaveCount;
    for (uint idxI = 0; idxI < mInstList.count; idxI++) {
        auto cDlsInst = cDls->cLins->pcInst[idxI];

        INST_INFO inst;
        mInstList.ppData[idxI] = (INST_INFO*)malloc(sizeof(INST_INFO));
        memcpy_s(mInstList.ppData[idxI], sizeof(INST_INFO), &inst, sizeof(INST_INFO));
        auto pInst = mInstList.ppData[idxI];

        pInst->id.isDrum = cDlsInst->Header.bankFlags >= 0x80 ? 1 : 0;
        pInst->id.bankMSB = cDlsInst->Header.bankMSB;
        pInst->id.bankLSB = cDlsInst->Header.bankLSB;
        pInst->id.progNum = cDlsInst->Header.progNum;
        pInst->layerIndex = layerIndex;
        pInst->pName = (char*)malloc(sizeof(cDlsInst->Name));
        memcpy_s(pInst->pName, sizeof(cDlsInst->Name), cDlsInst->Name, sizeof(cDlsInst->Name));
        pInst->pCategory = (char*)malloc(sizeof(cDlsInst->Category));
        memcpy_s(pInst->pCategory, sizeof(cDlsInst->Category), cDlsInst->Category, sizeof(cDlsInst->Category));

        if (NULL == cDlsInst->cLart) {
            pInst->artIndex = INVALID_INDEX;
        } else {
            pInst->artIndex = artIndex;
            mppArtList[artIndex] = (INST_ART*)calloc(1, sizeof(INST_ART));
            loadDlsArt(cDlsInst->cLart, mppArtList[artIndex]);
            artIndex++;
        }

        INST_LAYER *pLayer = NULL;
        for (int idxR = 0; idxR < cDlsInst->cLrgn->Count; idxR++) {
            INST_REGION region;
            mppRegionList[regionIndex] = (INST_REGION*)malloc(sizeof(INST_REGION));
            memcpy_s(mppRegionList[regionIndex], sizeof(INST_REGION), &region, sizeof(INST_REGION));
            auto pRegion = mppRegionList[regionIndex];
            auto cDlsRgn = cDlsInst->cLrgn->pcRegion[idxR];
            pRegion->keyLow = (byte)cDlsRgn->Header.keyLow;
            pRegion->keyHigh = (byte)cDlsRgn->Header.keyHigh;
            pRegion->velocityLow = (byte)cDlsRgn->Header.velocityLow;
            pRegion->velocityHigh = (byte)cDlsRgn->Header.velocityHigh;
            pRegion->waveIndex = cDlsRgn->WaveLink.tableIndex;

            if (NULL == cDlsRgn->cLart) {
                pRegion->artIndex = INVALID_INDEX;
            } else {
                mppArtList[artIndex] = (INST_ART*)calloc(1, sizeof(INST_ART));
                loadDlsArt(cDlsRgn->cLart, mppArtList[artIndex]);
                pRegion->artIndex = artIndex;
                artIndex++;
            }

            if (NULL == cDlsRgn->pWaveSmpl) {
                pRegion->wsmpIndex = INVALID_INDEX;
            } else {
                INST_WAVE wave;
                mppWaveList[waveIndex] = (INST_WAVE*)malloc(sizeof(INST_WAVE));
                memcpy_s(mppWaveList[waveIndex], sizeof(INST_WAVE), &region, sizeof(INST_WAVE));
                auto pWave = mppWaveList[waveIndex];
                pWave->unityNote = (byte)cDlsRgn->pWaveSmpl->unityNote;
                pWave->pitch = cDlsRgn->pWaveSmpl->getFileTune();
                pWave->gain = cDlsRgn->pWaveSmpl->getGain();
                if (0 == cDlsRgn->pWaveSmpl->loopCount) {
                    auto cDlsWave = cDls->cWvpl->pcWave[pRegion->waveIndex];
                    pWave->loopEnable = 0;
                    pWave->loopBegin = 0;
                    pWave->loopLength = cDlsWave->DataSize * 8 / cDlsWave->Format.bits;
                } else {
                    auto pLoop = cDlsRgn->ppWaveLoop[0];
                    pWave->loopEnable = 1;
                    pWave->loopBegin = pLoop->start;
                    pWave->loopLength = pLoop->length;
                }
                pRegion->wsmpIndex = waveIndex;
                waveIndex++;
            }

            if (pInst->layerCount < cDlsRgn->Header.layer + 1) {
                pInst->layerCount = cDlsRgn->Header.layer + 1;
                INST_LAYER layer;
                mppLayerList[layerIndex] = (INST_LAYER*)malloc(sizeof(INST_LAYER));
                memcpy_s(mppLayerList[layerIndex], sizeof(INST_LAYER), &layer, sizeof(INST_LAYER));
                pLayer = mppLayerList[layerIndex];
                pLayer->regionIndex = regionIndex;
                pLayer->artIndex = INVALID_INDEX;
            }
            pLayer->regionCount++;
            regionIndex++;
        }
        layerIndex += pInst->layerCount;
    }

    delete cDls;

    return E_LOAD_STATUS::SUCCESS;
}

void InstList::loadDlsWave(DLS *cDls) {
    FILE *fpWave = NULL;
    _wfopen_s(&fpWave, mWaveTablePath, TEXT("wb"));
    uint wavePos = 0;
    for (int idxW = 0; idxW < cDls->WaveCount; idxW++) {
        mppWaveList[idxW] = (INST_WAVE*)calloc(1, sizeof(INST_WAVE));
        auto pWave = mppWaveList[idxW];
        auto cDlsWave = cDls->cWvpl->pcWave[idxW];

        pWave->offset = wavePos;
        pWave->sampleRate = cDlsWave->Format.sampleRate;
        if (cDlsWave->LoopCount) {
            pWave->loopEnable = 1;
            pWave->loopBegin = cDlsWave->ppWaveLoop[0]->start;
            pWave->loopLength = cDlsWave->ppWaveLoop[0]->length;
        } else {
            pWave->loopEnable = 0;
            pWave->loopBegin = 0;
            pWave->loopLength = cDlsWave->DataSize * 8 / cDlsWave->Format.bits;
        }
        pWave->unityNote = (byte)cDlsWave->WaveSmpl.unityNote;
        pWave->pitch = cDlsWave->WaveSmpl.getFileTune();
        pWave->gain = cDlsWave->WaveSmpl.getGain();

        /* output wave table */
        switch (cDlsWave->Format.bits) {
        case 8:
            wavePos += writeWaveTable8(fpWave, cDlsWave->pData, cDlsWave->DataSize);
            break;
        case 16:
            wavePos += writeWaveTable16(fpWave, cDlsWave->pData, cDlsWave->DataSize);
            break;
        case 24:
            wavePos += writeWaveTable24(fpWave, cDlsWave->pData, cDlsWave->DataSize);
            break;
        case 32:
            if (3 == cDlsWave->Format.tag) {
                wavePos += writeWaveTableFloat(fpWave, cDlsWave->pData, cDlsWave->DataSize);
            } else {
                wavePos += writeWaveTable32(fpWave, cDlsWave->pData, cDlsWave->DataSize);
            }
            break;
        default:
            break;
        }
    }
    fclose(fpWave);

    auto waveTableSize = wavePos * sizeof(WAVDAT);
    mpWaveTable = (WAVDAT*)malloc(waveTableSize);
    fpWave = NULL;
    _wfopen_s(&fpWave, mWaveTablePath, TEXT("rb"));
    fread_s(mpWaveTable, waveTableSize, waveTableSize, 1, fpWave);
    fclose(fpWave);
    _wremove(mWaveTablePath);
}

void InstList::loadDlsArt(LART *cLart, INST_ART *pArt) {
    INST_ART art;
    memcpy_s(pArt, sizeof(INST_ART), &art, sizeof(INST_ART));

    auto ampA = 0.002;

    for (int idxC = 0; idxC < cLart->cArt->Count; idxC++) {
        auto pConn = cLart->cArt->ppConnection[idxC];
        switch (pConn->destination) {
        case E_DLS_DST::PAN:
            pArt->pan = (short)pConn->getValue();
            break;
        case E_DLS_DST::PITCH:
            pArt->pitch = pow(2.0, pConn->getValue() / 1200.0);
            break;
        case E_DLS_DST::ATTENUATION:
            pArt->gain = pConn->getValue();
            break;

        case E_DLS_DST::EG1_ATTACK_TIME:
            ampA = pConn->getValue();
            pArt->env.ampA = 48.0 / ampA;
            break;
        case E_DLS_DST::EG1_HOLD_TIME:
            pArt->env.ampH = pConn->getValue();
            break;
        case E_DLS_DST::EG1_DECAY_TIME:
            pArt->env.ampD = 32.0 / pConn->getValue();
            break;
        case E_DLS_DST::EG1_SUSTAIN_LEVEL:
            pArt->env.ampS = pConn->getValue();
            break;
        case E_DLS_DST::EG1_RELEASE_TIME:
            pArt->env.ampR = 24.0 / pConn->getValue();
            break;

        default:
            break;
        }
    }

    pArt->env.ampH += ampA;
}

/******************************************************************************/
uint InstList::writeWaveTable8(FILE *fp, byte* pData, uint size) {
    uint samples = size;
    for (uint i = 0; i < samples; i++) {
        auto tmp = (WAVDAT)((pData[i] - 128) * WAVMAX / 128);
        fwrite(&tmp, sizeof(WAVDAT), 1, fp);
    }
    return samples;
}

uint InstList::writeWaveTable16(FILE *fp, byte* pData, uint size) {
    uint samples = size / sizeof(short);
    auto pShort = (short*)pData;
    for (uint i = 0; i < samples; i++) {
        auto tmp = (WAVDAT)(pShort[i] * WAVMAX / 0x8000);
        fwrite(&tmp, sizeof(WAVDAT), 1, fp);
    }
    return samples;
}

uint InstList::writeWaveTable24(FILE *fp, byte* pData, uint size) {
    uint samples = size / sizeof(RIFFint24);
    auto pInt24 = (RIFFint24*)pData;
    for (uint i = 0; i < samples; i++) {
        auto tmp = (WAVDAT)(pInt24[i].msb * WAVMAX / 0x8000);
        fwrite(&tmp, sizeof(WAVDAT), 1, fp);
    }
    return samples;
}

uint InstList::writeWaveTable32(FILE *fp, byte* pData, uint size) {
    uint samples = size / sizeof(int);
    auto pInt32 = (int*)pData;
    for (uint i = 0; i < samples; i++) {
        auto tmp = (WAVDAT)((pInt32[i] >> 16) * WAVMAX / 0x8000);
        fwrite(&tmp, sizeof(WAVDAT), 1, fp);
    }
    return samples;
}

uint InstList::writeWaveTableFloat(FILE *fp, byte* pData, uint size) {
    uint samples = size / sizeof(float);
    auto pFloat = (float*)pData;
    for (uint i = 0; i < samples; i++) {
        auto tmpFloat = pFloat[i];
        if (1.0f < tmpFloat) tmpFloat = 1.0f;
        if (tmpFloat < -1.0f) tmpFloat = -1.0f;
        auto tmp = (WAVDAT)(tmpFloat * WAVMAX);
        fwrite(&tmp, sizeof(WAVDAT), 1, fp);
    }
    return samples;
}
