#include <stdlib.h>
#include <string.h>

#include "dls/main.h"

#include "inst_list.h"

/******************************************************************************/
InstList::~InstList() {
    if (nullptr != mppWaveList) {
        for (uint32 i = 0; i < mWaveCount; i++) {
            free(mppWaveList[i]);
        }
        free(mppWaveList);
        mppWaveList = nullptr;
    }
    if (nullptr != mInstList.ppData) {
        for (uint32 i = 0; i < mInstList.count; i++) {
            auto pInst = mInstList.ppData[i];
            free(pInst->pName);
            free(pInst->pCategory);
            free(pInst);
        }
        free(mInstList.ppData);
        mInstList.ppData = nullptr;
    }
    if (nullptr != mppLayerList) {
        for (uint32 i = 0; i < mLayerCount; i++) {
            free(mppLayerList[i]);
        }
        free(mppLayerList);
        mppLayerList = nullptr;
    }
    if (nullptr != mppRegionList) {
        for (uint32 i = 0; i < mRegionCount; i++) {
            free(mppRegionList[i]);
        }
        free(mppRegionList);
        mppRegionList = nullptr;
    }
    if (nullptr != mppArtList) {
        for (uint32 i = 0; i < mArtCount; i++) {
            free(mppArtList[i]);
        }
        free(mppArtList);
        mppArtList = nullptr;
    }
    if (nullptr != mpWaveTable) {
        free(mpWaveTable);
        mpWaveTable = nullptr;
    }
}

E_LOAD_STATUS InstList::Load(STRING path) {
    return loadDls(path);
}

INST_LIST *InstList::GetInstList() {
    return &mInstList;
}

INST_INFO *InstList::GetInstInfo(byte is_drum, byte bank_lsb, byte bank_msb, byte prog_num) {
    for (uint32 i = 0; i < mInstList.count; i++) {
        auto listId = mInstList.ppData[i]->id;
        if (listId.isDrum == is_drum &&
            listId.bankMSB == bank_msb &&
            listId.bankLSB == bank_lsb &&
            listId.progNum == prog_num) {
            return mInstList.ppData[i];
        }
    }
    for (uint32 i = 0; i < mInstList.count; i++) {
        auto listId = mInstList.ppData[i]->id;
        if (listId.isDrum == is_drum &&
            listId.bankMSB == 0 &&
            listId.bankLSB == 0 &&
            listId.progNum == prog_num) {
            return mInstList.ppData[i];
        }
    }
    for (uint32 i = 0; i < mInstList.count; i++) {
        auto listId = mInstList.ppData[i]->id;
        if (listId.isDrum == is_drum &&
            listId.bankMSB == 0 &&
            listId.bankLSB == 0 &&
            listId.progNum == 0) {
            return mInstList.ppData[i];
        }
    }
    return mInstList.ppData[0];
}

/******************************************************************************/
E_LOAD_STATUS InstList::loadDls(STRING path) {
    auto cDls = new DLS();
    auto loadState = cDls->load(path);
    if (E_LOAD_STATUS::SUCCESS != loadState) {
        return loadState;
    }

    /* count layer/region/art/wave */
    mWaveCount = cDls->m_wave_count;
    mInstList.count = cDls->m_inst_count;
    for (uint32 idxI = 0; idxI < mInstList.count; idxI++) {
        auto cDlsInst = cDls->mc_lins->mpc_ins[idxI];
        if (nullptr != cDlsInst->mc_lart) {
            mArtCount++;
        }
        uint32 rgnLayer = 0;
        for (int32 idxR = 0; idxR < cDlsInst->mc_lrgn->m_count; idxR++) {
            auto cDlsRgn = cDlsInst->mc_lrgn->mpc_rgn[idxR];
            if (nullptr != cDlsRgn->mc_lart) {
                mArtCount++;
            }
            if (nullptr != cDlsRgn->mp_wsmp) {
                mWaveCount++;
            }
            if (rgnLayer < static_cast<uint32>(cDlsRgn->m_rgnh.layer + 1)) {
                rgnLayer = cDlsRgn->m_rgnh.layer + 1;
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
    swprintf_s(mWaveTablePath, sizeof(mWaveTablePath) / sizeof(mWaveTablePath[0]), L"%s.bin", path);
    auto loadWaveState = loadDlsWave(cDls);
    if (E_LOAD_STATUS::SUCCESS != loadWaveState) {
        return loadWaveState;
    }

    /* load inst/layer/region/art */
    uint32 layerIndex = 0;
    uint32 regionIndex = 0;
    uint32 artIndex = 0;
    uint32 waveIndex = cDls->m_wave_count;
    for (uint32 idxI = 0; idxI < mInstList.count; idxI++) {
        auto cDlsInst = cDls->mc_lins->mpc_ins[idxI];

        INST_INFO inst;
        mInstList.ppData[idxI] = (INST_INFO*)calloc(1, sizeof(INST_INFO));
        memcpy_s(mInstList.ppData[idxI], sizeof(INST_INFO), &inst, sizeof(INST_INFO));
        auto pInst = mInstList.ppData[idxI];

        pInst->id.isDrum = cDlsInst->m_insh.bank_flags >= 0x80 ? 1 : 0;
        pInst->id.bankMSB = cDlsInst->m_insh.bank_msb;
        pInst->id.bankLSB = cDlsInst->m_insh.bank_lsb;
        pInst->id.progNum = cDlsInst->m_insh.prog_num;
        pInst->layerIndex = layerIndex;
        auto name = cDlsInst->mp_info->get(RIFF::INFO::INAM);
        auto name_len = name.size() + 1;
        pInst->pName = (char*)calloc(name_len, 1);
        strcpy_s(pInst->pName, name_len, name.c_str());
        auto category = cDlsInst->mp_info->get(RIFF::INFO::ICAT);
        auto category_len = category.size() + 1;
        pInst->pCategory = (char*)calloc(category_len, 1);
        strcpy_s(pInst->pCategory, category_len, category.c_str());

        if (nullptr == cDlsInst->mc_lart) {
            pInst->artIndex = UINT_MAX;
        } else {
            pInst->artIndex = artIndex;
            mppArtList[artIndex] = (INST_ART*)calloc(1, sizeof(INST_ART));
            loadDlsArt(cDlsInst->mc_lart, mppArtList[artIndex]);
            artIndex++;
        }

        INST_LAYER *pLayer = nullptr;
        for (int32 idxR = 0; idxR < cDlsInst->mc_lrgn->m_count; idxR++) {
            INST_REGION region;
            mppRegionList[regionIndex] = (INST_REGION*)malloc(sizeof(INST_REGION));
            memcpy_s(mppRegionList[regionIndex], sizeof(INST_REGION), &region, sizeof(INST_REGION));
            auto pRegion = mppRegionList[regionIndex];
            auto cDlsRgn = cDlsInst->mc_lrgn->mpc_rgn[idxR];
            pRegion->keyLow = (byte)cDlsRgn->m_rgnh.key_low;
            pRegion->keyHigh = (byte)cDlsRgn->m_rgnh.key_high;
            pRegion->velocityLow = (byte)cDlsRgn->m_rgnh.velo_low;
            pRegion->velocityHigh = (byte)cDlsRgn->m_rgnh.velo_high;
            pRegion->waveIndex = cDlsRgn->m_wlnk.table_index;

            if (nullptr == cDlsRgn->mc_lart) {
                pRegion->artIndex = UINT_MAX;
            } else {
                mppArtList[artIndex] = (INST_ART*)calloc(1, sizeof(INST_ART));
                loadDlsArt(cDlsRgn->mc_lart, mppArtList[artIndex]);
                pRegion->artIndex = artIndex;
                artIndex++;
            }

            if (nullptr == cDlsRgn->mp_wsmp) {
                pRegion->wsmpIndex = UINT_MAX;
            } else {
                INST_WAVE wave;
                mppWaveList[waveIndex] = (INST_WAVE*)malloc(sizeof(INST_WAVE));
                memcpy_s(mppWaveList[waveIndex], sizeof(INST_WAVE), &region, sizeof(INST_WAVE));
                auto pWave = mppWaveList[waveIndex];
                pWave->unityNote = (byte)cDlsRgn->mp_wsmp->unity_note;
                pWave->pitch = cDlsRgn->mp_wsmp->fine_tune();
                pWave->gain = cDlsRgn->mp_wsmp->gain();
                if (0 == cDlsRgn->mp_wsmp->loop_count) {
                    auto cDlsWave = cDls->mc_wvpl->mpc_wave[pRegion->waveIndex];
                    pWave->loopEnable = 0;
                    pWave->loopBegin = 0;
                    pWave->loopLength = cDlsWave->m_data_size * 8 / cDlsWave->m_fmt.wBitsPerSample;
                } else {
                    auto loop = cDlsRgn->mp_loop[0];
                    pWave->loopEnable = 1;
                    pWave->loopBegin = loop.start;
                    pWave->loopLength = loop.length;
                }
                pRegion->wsmpIndex = waveIndex;
                waveIndex++;
            }

            if (pInst->layerCount < static_cast<uint32>(cDlsRgn->m_rgnh.layer + 1)) {
                pInst->layerCount = cDlsRgn->m_rgnh.layer + 1;
                INST_LAYER layer;
                mppLayerList[layerIndex] = (INST_LAYER*)malloc(sizeof(INST_LAYER));
                memcpy_s(mppLayerList[layerIndex], sizeof(INST_LAYER), &layer, sizeof(INST_LAYER));
                pLayer = mppLayerList[layerIndex];
                pLayer->regionIndex = regionIndex;
                pLayer->artIndex = UINT_MAX;
            }
            pLayer->regionCount++;
            regionIndex++;
        }
        layerIndex += pInst->layerCount;
    }

    delete cDls;

    return E_LOAD_STATUS::SUCCESS;
}

E_LOAD_STATUS InstList::loadDlsWave(DLS *cDls) {
    FILE *fpWave = nullptr;
    _wfopen_s(&fpWave, mWaveTablePath, L"wb");
    uint32 wavePos = 0;
    for (uint32 idxW = 0; idxW < cDls->m_wave_count; idxW++) {
        mppWaveList[idxW] = (INST_WAVE*)calloc(1, sizeof(INST_WAVE));
        auto pWave = mppWaveList[idxW];
        auto cDlsWave = cDls->mc_wvpl->mpc_wave[idxW];

        pWave->offset = wavePos;
        pWave->sampleRate = cDlsWave->m_fmt.nSamplesPerSec;
        if (nullptr == cDlsWave->mp_wsmp) {
            pWave->unityNote = 64;
            pWave->pitch = 1.0;
            pWave->gain = 1.0;
            pWave->loopEnable = 0;
            pWave->loopBegin = 0;
            pWave->loopLength = cDlsWave->m_data_size * 8 / cDlsWave->m_fmt.wBitsPerSample;
        } else {
            pWave->unityNote = (byte)cDlsWave->mp_wsmp->unity_note;
            pWave->pitch = cDlsWave->mp_wsmp->fine_tune();
            pWave->gain = cDlsWave->mp_wsmp->gain();
            if (cDlsWave->mp_wsmp->loop_count) {
                pWave->loopEnable = 1;
                pWave->loopBegin = cDlsWave->mp_loop[0].start;
                pWave->loopLength = cDlsWave->mp_loop[0].length;
            } else {
                pWave->loopEnable = 0;
                pWave->loopBegin = 0;
                pWave->loopLength = cDlsWave->m_data_size * 8 / cDlsWave->m_fmt.wBitsPerSample;
            }
        }

        /* output wave table */
        switch (cDlsWave->m_fmt.wBitsPerSample) {
        case 8:
            wavePos += writeWaveTable8(fpWave, cDlsWave->mp_data, cDlsWave->m_data_size);
            break;
        case 16:
            wavePos += writeWaveTable16(fpWave, cDlsWave->mp_data, cDlsWave->m_data_size);
            break;
        case 24:
            wavePos += writeWaveTable24(fpWave, cDlsWave->mp_data, cDlsWave->m_data_size);
            break;
        case 32:
            if (3 == cDlsWave->m_fmt.wFormatTag) {
                wavePos += writeWaveTableFloat(fpWave, cDlsWave->mp_data, cDlsWave->m_data_size);
            } else {
                wavePos += writeWaveTable32(fpWave, cDlsWave->mp_data, cDlsWave->m_data_size);
            }
            break;
        default:
            break;
        }
    }
    fclose(fpWave);

    auto waveTableSize = wavePos * sizeof(WAVE_DATA);
    mpWaveTable = (WAVE_DATA*)malloc(waveTableSize);
    if (nullptr == mpWaveTable) {
        return E_LOAD_STATUS::ALLOCATE_FAILED;
    }

    fpWave = nullptr;
    _wfopen_s(&fpWave, mWaveTablePath, L"rb");
    fread_s(mpWaveTable, waveTableSize, waveTableSize, 1, fpWave);
    fclose(fpWave);
    _wremove(mWaveTablePath);

    return E_LOAD_STATUS::SUCCESS;
}

void InstList::loadDlsArt(LART *cLart, INST_ART *pArt) {
    INST_ART art;
    memcpy_s(pArt, sizeof(INST_ART), &art, sizeof(INST_ART));

    auto ampA = 0.002;

    for (uint32 idxC = 0; idxC < cLart->mc_art->m_count; idxC++) {
        auto conn = cLart->mc_art->mp_conn[idxC];
        switch (conn.destination) {
        case ART::E_DST::PAN:
            pArt->pan = (short)conn.value();
            break;
        case ART::E_DST::PITCH:
            pArt->pitch = pow(2.0, conn.value() / 1200.0);
            break;
        case ART::E_DST::ATTENUATION:
            pArt->gain = conn.value();
            break;

        case ART::E_DST::EG1_ATTACK_TIME:
            ampA = conn.value();
            pArt->eg_amp.attack = 48.0 / ampA;
            break;
        case ART::E_DST::EG1_HOLD_TIME:
            pArt->eg_amp.hold = conn.value();
            break;
        case ART::E_DST::EG1_DECAY_TIME:
            pArt->eg_amp.decay = 32.0 / conn.value();
            break;
        case ART::E_DST::EG1_SUSTAIN_LEVEL:
            pArt->eg_amp.sustain = conn.value();
            break;
        case ART::E_DST::EG1_RELEASE_TIME:
            pArt->eg_amp.release = 24.0 / conn.value();
            break;

        default:
            break;
        }
    }

    pArt->eg_amp.hold += ampA;
}

/******************************************************************************/
uint32 InstList::writeWaveTable8(FILE *fp, byte* pData, uint32 size) {
    uint32 samples = size;
    for (uint32 i = 0; i < samples; i++) {
        auto tmp = (WAVE_DATA)((pData[i] - 128) * WAVE_MAX / 128);
        fwrite(&tmp, sizeof(WAVE_DATA), 1, fp);
    }
    return samples;
}

uint32 InstList::writeWaveTable16(FILE *fp, byte* pData, uint32 size) {
    uint32 samples = size / sizeof(int16);
    auto pShort = (int16*)pData;
    for (uint32 i = 0; i < samples; i++) {
        auto tmp = (WAVE_DATA)(pShort[i] * WAVE_MAX / 0x8000);
        fwrite(&tmp, sizeof(WAVE_DATA), 1, fp);
    }
    return samples;
}

uint32 InstList::writeWaveTable24(FILE *fp, byte* pData, uint32 size) {
    uint32 samples = size / sizeof(int24);
    auto pInt24 = (int24*)pData;
    for (uint32 i = 0; i < samples; i++) {
        auto tmp = (WAVE_DATA)(pInt24[i].msb * WAVE_MAX / 0x8000);
        fwrite(&tmp, sizeof(WAVE_DATA), 1, fp);
    }
    return samples;
}

uint32 InstList::writeWaveTable32(FILE *fp, byte* pData, uint32 size) {
    uint32 samples = size / sizeof(int);
    auto pInt32 = (int*)pData;
    for (uint32 i = 0; i < samples; i++) {
        auto tmp = (WAVE_DATA)((pInt32[i] >> 16) * WAVE_MAX / 0x8000);
        fwrite(&tmp, sizeof(WAVE_DATA), 1, fp);
    }
    return samples;
}

uint32 InstList::writeWaveTableFloat(FILE *fp, byte* pData, uint32 size) {
    uint32 samples = size / sizeof(float);
    auto pFloat = (float*)pData;
    for (uint32 i = 0; i < samples; i++) {
        auto tmpFloat = pFloat[i];
        if (1.0f < tmpFloat) tmpFloat = 1.0f;
        if (tmpFloat < -1.0f) tmpFloat = -1.0f;
        auto tmp = (WAVE_DATA)(tmpFloat * WAVE_MAX);
        fwrite(&tmp, sizeof(WAVE_DATA), 1, fp);
    }
    return samples;
}
