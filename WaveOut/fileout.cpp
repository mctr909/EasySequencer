#pragma once
#include <stdio.h>
#include "fileout.h"
#include "synth/channel.h"
#include "synth/channel_const.h"
#include "synth/channel_params.h"
#include "synth/sampler.h"
#include "inst/inst_list.h"
#include "message_reciever.h"

/******************************************************************************/
typedef struct INST_SAMPLER INST_SAMPLER;

#pragma pack(push, 4)
typedef struct {
    uint32 riff;
    uint32 fileSize;
    uint32 dataId;
} RIFF;
#pragma pack(pop)

#pragma pack(push, 4)
typedef struct {
    uint32 chunkId;
    uint32 chunkSize;
    uint16 formatId;
    uint16 channels;
    uint32 sampleRate;
    uint32 bytePerSec;
    uint16 blockAlign;
    uint16 bitPerSample;
    uint32 dataId;
    uint32 dataSize;
} FMT_;
#pragma pack(pop)

/******************************************************************************/
SYSTEM_VALUE  gFileOutSysValue = { 0 };
FILE          *gfpFileOut = NULL;
FMT_          gFmt = { 0 };
int32         gFileOutProgress = 0;
double        gBpm = 120.0;

/******************************************************************************/
int32 fileout_send(byte *pMsg);
void fileout_write(byte* pOutBuffer);

/******************************************************************************/
int32* WINAPI fileout_getProgressPtr() {
    return &gFileOutProgress;
}

void WINAPI fileout_save(
    LPWSTR waveTablePath,
    LPWSTR savePath,
    uint32 sampleRate,
    byte *pEvents,
    uint32 eventSize,
    uint32 baseTick
) {
    // set system value
    if (NULL != gFileOutSysValue.cInst_list) {
        delete gFileOutSysValue.cInst_list;
    }

    auto cInst = new InstList();
    cInst->Load(waveTablePath);

    gFileOutSysValue.cInst_list = cInst;
    gFileOutSysValue.pWave_table = gFileOutSysValue.cInst_list->GetWaveTablePtr();
    gFileOutSysValue.ppSampler = (INST_SAMPLER**)calloc(SAMPLER_COUNT, sizeof(INST_SAMPLER*));
    for (uint32 i = 0; i < SAMPLER_COUNT; i++) {
        INST_SAMPLER smpl;
        gFileOutSysValue.ppSampler[i] = (INST_SAMPLER*)malloc(sizeof(INST_SAMPLER));
        memcpy_s(gFileOutSysValue.ppSampler[i], sizeof(INST_SAMPLER), &smpl, sizeof(INST_SAMPLER));
    }
    gFileOutSysValue.buffer_length = 256;
    gFileOutSysValue.buffer_count = 16;
    gFileOutSysValue.sample_rate = sampleRate;
    gFileOutSysValue.delta_time = 1.0 / gFileOutSysValue.sample_rate;
    gFileOutSysValue.pBuffer_l = (double*)calloc(gFileOutSysValue.buffer_length, sizeof(double));
    gFileOutSysValue.pBuffer_r = (double*)calloc(gFileOutSysValue.buffer_length, sizeof(double));

    // riff wave format
    RIFF riff;
    riff.riff = 0x46464952;
    riff.fileSize = 0;
    riff.dataId = 0x45564157;
    gFmt.chunkId = 0x20746D66;
    gFmt.chunkSize = 16;
    gFmt.formatId = 1;
    gFmt.channels = 2;
    gFmt.sampleRate = gFileOutSysValue.sample_rate;
    gFmt.bitPerSample = (uint16)16;
    gFmt.blockAlign = gFmt.channels * gFmt.bitPerSample >> 3;
    gFmt.bytePerSec = gFmt.sampleRate * gFmt.blockAlign;
    gFmt.dataId = 0x61746164;
    gFmt.dataSize = 0;

    // allocate out buffer
    auto pOutBuffer = (byte*)calloc(gFmt.blockAlign, gFileOutSysValue.buffer_length);

    // allocate channels
    gFileOutSysValue.ppChannels = (Channel**)calloc(CHANNEL_COUNT, sizeof(Channel*));
    gFileOutSysValue.ppChannel_params = (CHANNEL_PARAM**)calloc(CHANNEL_COUNT, sizeof(CHANNEL_PARAM*));
    for (int32 c = 0; c < CHANNEL_COUNT; c++) {
        gFileOutSysValue.ppChannels[c] = new Channel(&gFileOutSysValue, c);
        gFileOutSysValue.ppChannel_params[c] = &gFileOutSysValue.ppChannels[c]->param;
    }

    // open file
    if (NULL != gfpFileOut) {
        fclose(gfpFileOut);
        gfpFileOut = NULL;
    }
    _wfopen_s(&gfpFileOut, savePath, L"wb");
    fwrite(&riff, sizeof(riff), 1, gfpFileOut);
    fwrite(&gFmt, sizeof(gFmt), 1, gfpFileOut);

    //********************************
    // output wave
    //********************************
    uint32 curPos = 0;
    double curTime = 0.0;
    double delta_sec = gFileOutSysValue.buffer_length * gFileOutSysValue.delta_time;
    while (curPos < eventSize) {
        auto evTime = (double)(*(int32*)(pEvents + curPos)) / baseTick;
        curPos += 4;
        auto evValue = pEvents + curPos;
        while (curTime < evTime) {
            fileout_write(pOutBuffer);
            curTime += gBpm * delta_sec / 60.0;
            gFileOutProgress = curPos;
        }
        curPos += fileout_send(evValue);
    }
    gFileOutProgress = eventSize;

    // close file
    riff.fileSize = gFmt.dataSize + sizeof(gFmt) + 4;
    fseek(gfpFileOut, 0, SEEK_SET);
    fwrite(&riff, sizeof(riff), 1, gfpFileOut);
    fwrite(&gFmt, sizeof(gFmt), 1, gfpFileOut);
    fclose(gfpFileOut);

    /* dispose channels */
    for (int32 c = 0; c < CHANNEL_COUNT; c++) {
        delete gFileOutSysValue.ppChannels[c];
        gFileOutSysValue.ppChannels[c] = NULL;
    }
    if (NULL != gFileOutSysValue.pBuffer_l) {
        free(gFileOutSysValue.pBuffer_l);
        gFileOutSysValue.pBuffer_l = NULL;
    }
    if (NULL != gFileOutSysValue.pBuffer_r) {
        free(gFileOutSysValue.pBuffer_r);
        gFileOutSysValue.pBuffer_r = NULL;
    }
    if (NULL != gFileOutSysValue.ppSampler) {
        for (uint32 i = 0; i < SAMPLER_COUNT; i++) {
            free(gFileOutSysValue.ppSampler[i]);
        }
        free(gFileOutSysValue.ppSampler);
        gFileOutSysValue.ppSampler = NULL;
    }
    free(gFileOutSysValue.ppChannels);
    gFileOutSysValue.ppChannels = NULL;
    free(gFileOutSysValue.ppChannel_params);
    gFileOutSysValue.ppChannel_params = NULL;
}

/******************************************************************************/
int32 fileout_send(byte *pMsg) {
    auto type = (E_EVENT_TYPE)(*pMsg & 0xF0);
    auto ch = *pMsg & 0x0F;
    switch (type) {
    case E_EVENT_TYPE::NOTE_OFF:
        gFileOutSysValue.ppChannels[ch]->note_off(pMsg[1]);
        return 3;
    case E_EVENT_TYPE::NOTE_ON:
        gFileOutSysValue.ppChannels[ch]->note_on(pMsg[1], pMsg[2]);
        return 3;
    case E_EVENT_TYPE::POLY_KEY:
        return 3;
    case E_EVENT_TYPE::CTRL_CHG:
        gFileOutSysValue.ppChannels[ch]->ctrl_change(pMsg[1], pMsg[2]);
        return 3;
    case E_EVENT_TYPE::PROG_CHG:
        gFileOutSysValue.ppChannels[ch]->program_change(pMsg[1]);
        return 2;
    case E_EVENT_TYPE::CH_PRESS:
        return 2;
    case E_EVENT_TYPE::PITCH:
        gFileOutSysValue.ppChannels[ch]->pitch_bend(((pMsg[2] << 7) | pMsg[1]) - 8192);
        return 3;
    case E_EVENT_TYPE::SYS_EX:
        if (0xFF == pMsg[0]) {
            auto type = (E_META_TYPE)pMsg[1];
            auto size = pMsg[2];
            switch (type) {
            case E_META_TYPE::TEMPO:
                gBpm = 60000000.0 / ((pMsg[3] << 16) | (pMsg[4] << 8) | pMsg[5]);
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

void fileout_write(byte* pOutBuffer) {
    /* sampler loop */
    for (int32 i = 0; i < SAMPLER_COUNT; i++) {
        auto pSmpl = gFileOutSysValue.ppSampler[i];
        if (pSmpl->state < E_SAMPLER_STATE::PURGE) {
            continue;
        }
        sampler(&gFileOutSysValue, pSmpl);
    }   
    /* channel loop */
    for (int32 i = 0; i < CHANNEL_COUNT; i++) {
        auto pCh = gFileOutSysValue.ppChannels[i];
        pCh->step(gFileOutSysValue.pBuffer_l, gFileOutSysValue.pBuffer_r);
    }
    /* write buffer */
    auto pBuff = (short*)pOutBuffer;
    for (int32 i = 0, j = 0; i < gFileOutSysValue.buffer_length; i++, j += 2) {
        auto pL = &gFileOutSysValue.pBuffer_l[i];
        auto pR = &gFileOutSysValue.pBuffer_r[i];
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
        pBuff[j] = (short)(*pL * 32767);
        pBuff[j + 1] = (short)(*pR * 32767);
        *pL = 0.0;
        *pR = 0.0;
    }
    int32 buffSize = gFileOutSysValue.buffer_length * gFmt.blockAlign;
    fwrite(pOutBuffer, buffSize, 1, gfpFileOut);
    gFmt.dataSize += buffSize;
}
