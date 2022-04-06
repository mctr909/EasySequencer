#include "inst_list.h"
#include "dls.h"
#include "channel.h"

/******************************************************************************/
#define INVALID_INDEX 0xFFFFFFFF
#define WAVMAX 32768.0
typedef short WAVDAT;

/******************************************************************************/
InstList::InstList(LPWSTR path) {
    swprintf_s(mWaveTablePath, sizeof(mWaveTablePath) / sizeof(mWaveTablePath[0]), TEXT("%s.bin"), path);
    mppSampler = (INST_SAMPLER**)malloc(sizeof(INST_SAMPLER*) * SAMPLER_COUNT);
    for (unsigned int i = 0; i < SAMPLER_COUNT; i++) {
        INST_SAMPLER smpl;
        mppSampler[i] = (INST_SAMPLER*)malloc(sizeof(INST_SAMPLER));
        memcpy_s(mppSampler[i], sizeof(INST_SAMPLER), &smpl, sizeof(INST_SAMPLER));
    }
    loadDls(path);
}

InstList::~InstList() {
    if (NULL != mppSampler) {
        for (unsigned int i = 0; i < SAMPLER_COUNT; i++) {
            free(mppSampler[i]);
        }
        free(mppSampler);
    }
    if (NULL != mppWaveList) {
        for (unsigned int i = 0; i < mWaveCount; i++) {
            free(mppWaveList[i]);
        }
        free(mppWaveList);
    }
    if (NULL != mppInstList) {
        for (unsigned int i = 0; i < mInstCount; i++) {
            free(mppInstList[i]);
        }
        free(mppInstList);
    }
    if (NULL != mppLayerList) {
        for (unsigned int i = 0; i < mLayerCount; i++) {
            free(mppLayerList[i]);
        }
        free(mppLayerList);
    }
    if (NULL != mppRegionList) {
        for (unsigned int i = 0; i < mRegionCount; i++) {
            free(mppRegionList[i]);
        }
        free(mppRegionList);
    }
    if (NULL != mppArtList) {
        for (unsigned int i = 0; i < mArtCount; i++) {
            free(mppArtList[i]);
        }
        free(mppArtList);
    }
    if (NULL != mpWaveTable) {
        free(mpWaveTable);
    }
}

INST_INFO *InstList::GetInstInfo(INST_ID *id) {
    for (unsigned int i = 0; i < mInstCount; i++) {
        auto listId = mppInstList[i]->id;
        if (listId.isDrum == id->isDrum &&
            listId.bankMSB == id->bankMSB &&
            listId.bankLSB == id->bankLSB &&
            listId.progNum == id->progNum) {
            return mppInstList[i];
        }
    }
    for (unsigned int i = 0; i < mInstCount; i++) {
        auto listId = mppInstList[i]->id;
        if (listId.isDrum == id->isDrum &&
            listId.bankMSB == 0 &&
            listId.bankLSB == 0 &&
            listId.progNum == id->progNum) {
            return mppInstList[i];
        }
    }
    return NULL;
}

INST_SAMPLER **InstList::GetSamplerPtr() {
    return mppSampler;
}

short *InstList::GetWaveTablePtr() {
    return mpWaveTable;
}

void InstList::SetSampler(INST_INFO *pInstInfo, unsigned char channelNum, unsigned char noteNum, unsigned char velocity) {
    for (unsigned int s = 0; s < SAMPLER_COUNT; s++) {
        auto pSmpl = mppSampler[s];
        if (pSmpl->channelNum == channelNum && pSmpl->noteNum == noteNum &&
            E_KEY_STATE::PRESS <= pSmpl->state) {
            pSmpl->state = E_KEY_STATE::PURGE;
        }
    }
    auto ppLayer = mppLayerList + pInstInfo->layerIndex;
    for (unsigned int l = 0; l < pInstInfo->layerCount; l++) {
        auto pLayer = ppLayer[l];
        auto ppRegion = mppRegionList + pLayer->regionIndex;
        for (unsigned int r = 0; r < pLayer->regionCount; r++) {
            auto pRegion = ppRegion[r];
            if (pRegion->keyLow <= noteNum && noteNum <= pRegion->keyHigh &&
                pRegion->velocityLow <= velocity && velocity <= pRegion->velocityHigh) {
                auto pWave = mppWaveList[pRegion->waveIndex];
                for (unsigned int s = 0; s < SAMPLER_COUNT; s++) {
                    auto pSmpl = mppSampler[s];
                    if (E_KEY_STATE::FREE == pSmpl->state) {
                        pSmpl->state = E_KEY_STATE::RESERVED;
                        pSmpl->channelNum = channelNum;
                        pSmpl->noteNum = noteNum;
                        pSmpl->index = 0.0;
                        pSmpl->time = 0.0;
                        pSmpl->pWave = pWave;
                        auto pitch = 1.0;
                        auto diffNote = noteNum - pWave->unityNote;
                        if (diffNote < 0) {
                            pitch = 1.0 / SemiTone[-diffNote];
                        } else {
                            pitch = SemiTone[diffNote];
                        }
                        pitch *= pWave->pitch;
                        pSmpl->gain = pWave->gain * velocity / 127.0 / 32768.0;
                        if (INVALID_INDEX != pInstInfo->artIndex) {
                            auto pArt = mppArtList[pInstInfo->artIndex];
                            pSmpl->pan += pArt->pan;
                            //pArt->transpose;
                            pitch *= pArt->pitch;
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
                            pitch *= pArt->pitch;
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
                            pitch *= pArt->pitch;
                            pSmpl->gain *= pArt->gain;
                            pSmpl->pEnv = &pArt->env;
                            pSmpl->egAmp = 0.0;
                            pSmpl->egCutoff = pArt->env.cutoffRise;
                            pSmpl->egPitch = pArt->env.pitchRise;
                        }
                        pSmpl->pitch = pitch * pWave->sampleRate;
                        pSmpl->state = E_KEY_STATE::PRESS;
                        break;
                    }
                }
                break;
            }
        }
    }
}

/******************************************************************************/
void InstList::loadDls(LPWSTR path) {
    auto cDls = new DLS(path);
    loadDlsWave(cDls);

    /* count layer, region, art */
    mInstCount = cDls->InstCount;
    for (unsigned int i = 0; i < mInstCount; i++) {
        auto cDlsInst = cDls->cLins->pcInst[i];
        if (NULL != cDlsInst->cLart) {
            mArtCount++;
        }
        unsigned int rgnLayer = 0;
        for (int r = 0; r < cDlsInst->cLrgn->Count; r++) {
            auto cDlsRgn = cDlsInst->cLrgn->pcRegion[r];
            if (NULL != cDlsRgn->cLart) {
                mArtCount++;
            }
            if (rgnLayer < cDlsRgn->Header.layer + 1) {
                rgnLayer = cDlsRgn->Header.layer + 1;
            }
            mRegionCount++;
        }
        mLayerCount += rgnLayer;
    }

    /* load inst, layer, region, art */
    unsigned int layerIndex = 0;
    unsigned int regionIndex = 0;
    unsigned int artIndex = 0;
    mppInstList = (INST_INFO**)malloc(sizeof(INST_INFO*) * mInstCount);
    memset(mppInstList, 0, sizeof(INST_INFO**));
    mppLayerList = (INST_LAYER**)malloc(sizeof(INST_LAYER*) * mLayerCount);
    memset(mppLayerList, 0, sizeof(INST_LAYER**));
    mppRegionList = (INST_REGION**)malloc(sizeof(INST_REGION*) * mRegionCount);
    memset(mppRegionList, 0, sizeof(INST_REGION**));
    mppArtList = (INST_ART**)malloc(sizeof(INST_ART*) * mArtCount);
    memset(mppArtList, 0, sizeof(INST_ART**));

    for (unsigned int i = 0; i < mInstCount; i++) {
        auto cDlsInst = cDls->cLins->pcInst[i];

        INST_INFO inst;
        mppInstList[i] = (INST_INFO*)malloc(sizeof(INST_INFO));
        memcpy_s(mppInstList[i], sizeof(INST_INFO), &inst, sizeof(INST_INFO));
        auto pInst = mppInstList[i];

        pInst->id.isDrum = cDlsInst->Header.bankFlags >= 0x80 ? 1 : 0;
        pInst->id.bankMSB = cDlsInst->Header.bankMSB;
        pInst->id.bankLSB = cDlsInst->Header.bankLSB;
        pInst->id.progNum = cDlsInst->Header.progNum;
        pInst->layerIndex = layerIndex;
        memcpy_s(pInst->name, sizeof(pInst->name), cDlsInst->Name, sizeof(cDlsInst->Name));
        memcpy_s(pInst->category, sizeof(pInst->category), cDlsInst->Category, sizeof(cDlsInst->Category));

        if (NULL == cDlsInst->cLart) {
            pInst->artIndex = INVALID_INDEX;
        } else {
            pInst->artIndex = artIndex;
            mppArtList[artIndex] = (INST_ART*)malloc(sizeof(INST_ART));
            loadDlsArt(cDlsInst->cLart, mppArtList[artIndex]);
            artIndex++;
        }

        INST_LAYER *pLayer = NULL;
        for (int r = 0; r < cDlsInst->cLrgn->Count; r++) {
            INST_REGION region;
            mppRegionList[regionIndex] = (INST_REGION*)malloc(sizeof(INST_REGION));
            memcpy_s(mppRegionList[regionIndex], sizeof(INST_REGION), &region, sizeof(INST_REGION));
            auto pRegion = mppRegionList[regionIndex];
            auto cDlsRgn = cDlsInst->cLrgn->pcRegion[r];
            pRegion->keyLow = (unsigned char)cDlsRgn->Header.keyLow;
            pRegion->keyHigh = (unsigned char)cDlsRgn->Header.keyHigh;
            pRegion->velocityLow = (unsigned char)cDlsRgn->Header.velocityLow;
            pRegion->velocityHigh = (unsigned char)cDlsRgn->Header.velocityHigh;
            pRegion->waveIndex = cDlsRgn->WaveLink.tableIndex;

            if (NULL == cDlsRgn->cLart) {
                pRegion->artIndex = INVALID_INDEX;
            } else {
                mppArtList[artIndex] = (INST_ART*)malloc(sizeof(INST_ART));
                loadDlsArt(cDlsRgn->cLart, mppArtList[artIndex]);
                pRegion->artIndex = artIndex;
                artIndex++;
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

    mpWaveTable = (short*)malloc(mWaveTableSize);
    FILE *fp = NULL;
    _wfopen_s(&fp, mWaveTablePath, TEXT("rb"));
    fread_s(mpWaveTable, mWaveTableSize, mWaveTableSize, 1, fp);
    fclose(fp);

    delete cDls;
}

void InstList::loadDlsWave(DLS *cDls) {
    FILE *fpWave = NULL;
    _wfopen_s(&fpWave, mWaveTablePath, TEXT("wb"));
    mWaveCount = cDls->WaveCount;
    mppWaveList = (INST_WAVE**)malloc(sizeof(INST_WAVE*) * mWaveCount);
    memset(mppWaveList, 0, sizeof(INST_WAVE**));
    unsigned int wavePos = 0;
    for (unsigned int w = 0; w < mWaveCount; w++) {
        mppWaveList[w] = (INST_WAVE*)malloc(sizeof(INST_WAVE));
        auto pWave = mppWaveList[w];
        auto cDlsWave = cDls->cWvpl->pcWave[w];

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
        pWave->unityNote = (unsigned char)cDlsWave->WaveSmpl.unityNote;
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
    mWaveTableSize = wavePos * sizeof(WAVDAT);
    fclose(fpWave);
}

void InstList::loadDlsArt(LART *cLart, INST_ART *pArt) {
    INST_ART art;
    memcpy_s(pArt, sizeof(INST_ART), &art, sizeof(INST_ART));

    for (int c = 0; c < cLart->cArt->Count; c++) {
        auto pConn = cLart->cArt->ppConnection[c];
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
            pArt->env.ampA = 1.0 / pConn->getValue();
            break;
        case E_DLS_DST::EG1_HOLD_TIME:
            pArt->env.ampH = pConn->getValue();
            break;
        case E_DLS_DST::EG1_DECAY_TIME:
            pArt->env.ampD = 1.0 / pConn->getValue();
            break;
        case E_DLS_DST::EG1_SUSTAIN_LEVEL:
            pArt->env.ampS = pConn->getValue();
            break;
        case E_DLS_DST::EG1_RELEASE_TIME:
            pArt->env.ampR = 1.0 / pConn->getValue();
            break;

        case E_DLS_DST::EG2_ATTACK_TIME:
            pArt->env.cutoffA = 1.0 / pConn->getValue();
            break;
        case E_DLS_DST::EG2_HOLD_TIME:
            pArt->env.cutoffH = pConn->getValue();
            break;
        case E_DLS_DST::EG2_DECAY_TIME:
            pArt->env.cutoffD = 1.0 / pConn->getValue();
            break;
        case E_DLS_DST::EG2_SUSTAIN_LEVEL:
            pArt->env.cutoffS = pConn->getValue();
            pArt->env.cutoffFall = pArt->env.cutoffS;
            break;
        case E_DLS_DST::EG2_RELEASE_TIME:
            pArt->env.cutoffR = 1.0 / pConn->getValue();
            break;

        default:
            break;
        }
    }
}

unsigned int InstList::writeWaveTable8(FILE *fp, unsigned char* pData, unsigned int size) {
    unsigned int samples = size;
    for (unsigned int i = 0; i < samples; i++) {
        auto tmp = (WAVDAT)((pData[i] - 128) * WAVMAX / 128);
        fwrite(&tmp, sizeof(WAVDAT), 1, fp);
    }
    return samples;
}

unsigned int InstList::writeWaveTable16(FILE *fp, unsigned char* pData, unsigned int size) {
    unsigned int samples = size / sizeof(short);
    auto pShort = (short*)pData;
    for (unsigned int i = 0; i < samples; i++) {
        auto tmp = (WAVDAT)(pShort[i] * WAVMAX / 0x8000);
        fwrite(&tmp, sizeof(WAVDAT), 1, fp);
    }
    return samples;
}

unsigned int InstList::writeWaveTable24(FILE *fp, unsigned char* pData, unsigned int size) {
    unsigned int samples = size / sizeof(RIFFint24);
    auto pInt24 = (RIFFint24*)pData;
    for (unsigned int i = 0; i < samples; i++) {
        auto tmp = (WAVDAT)(pInt24[i].msb * WAVMAX / 0x8000);
        fwrite(&tmp, sizeof(WAVDAT), 1, fp);
    }
    return samples;
}

unsigned int InstList::writeWaveTable32(FILE *fp, unsigned char* pData, unsigned int size) {
    unsigned int samples = size / sizeof(int);
    auto pInt32 = (int*)pData;
    for (unsigned int i = 0; i < samples; i++) {
        auto tmp = (WAVDAT)((pInt32[i] >> 16) * WAVMAX / 0x8000);
        fwrite(&tmp, sizeof(WAVDAT), 1, fp);
    }
    return samples;
}

unsigned int InstList::writeWaveTableFloat(FILE *fp, unsigned char* pData, unsigned int size) {
    unsigned int samples = size / sizeof(float);
    auto pFloat = (float*)pData;
    for (unsigned int i = 0; i < samples; i++) {
        auto tmpFloat = pFloat[i];
        if (1.0f < tmpFloat) tmpFloat = 1.0f;
        if (tmpFloat < -1.0f) tmpFloat = -1.0f;
        auto tmp = (WAVDAT)(tmpFloat * WAVMAX);
        fwrite(&tmp, sizeof(WAVDAT), 1, fp);
    }
    return samples;
}
