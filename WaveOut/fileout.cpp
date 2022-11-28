#pragma once
#include <stdio.h>
#include "fileout.h"
#include "synth/channel.h"
#include "synth/channel_const.h"
#include "synth/channel_params.h"
#include "synth/sampler.h"
#include "inst/inst_list.h"

/******************************************************************************/
typedef struct INST_SAMPLER INST_SAMPLER;

#pragma pack(push, 4)
typedef struct {
    uint riff;
    uint fileSize;
    uint dataId;
} RIFF;
#pragma pack(pop)

#pragma pack(push, 4)
typedef struct {
    uint chunkId;
    uint chunkSize;
    ushort formatId;
    ushort channels;
    uint sampleRate;
    uint bytePerSec;
    ushort blockAlign;
    ushort bitPerSample;
    uint dataId;
    uint dataSize;
} FMT_;
#pragma pack(pop)

/******************************************************************************/
SYSTEM_VALUE  gFileOutSysValue = { 0 };
FILE          *gfpFileOut = NULL;
FMT_          gFmt = { 0 };
int           gFileOutProgress = 0;
double        gBpm = 120.0;

/******************************************************************************/
int fileout_send(byte *pMsg);
inline void fileout_write16(byte* pOutBuffer);

/******************************************************************************/
int* WINAPI fileout_getProgressPtr() {
    return &gFileOutProgress;
}

void WINAPI fileout_save(
    LPWSTR waveTablePath,
    LPWSTR savePath,
    uint sampleRate,
    byte *pEvents,
    uint eventSize,
    uint baseTick
) {
    // set system value
    if (NULL != gFileOutSysValue.cInstList) {
        delete gFileOutSysValue.cInstList;
    }

    auto cInst = new InstList();
    cInst->Load(waveTablePath);

    gFileOutSysValue.cInstList = cInst;
    gFileOutSysValue.pWaveTable = gFileOutSysValue.cInstList->GetWaveTablePtr();
    gFileOutSysValue.ppSampler = gFileOutSysValue.cInstList->GetSamplerPtr();
    gFileOutSysValue.bufferLength = 256;
    gFileOutSysValue.bufferCount = 16;
    gFileOutSysValue.sampleRate = sampleRate;
    gFileOutSysValue.deltaTime = 1.0 / gFileOutSysValue.sampleRate;

    // riff wave format
    RIFF riff;
    riff.riff = 0x46464952;
    riff.fileSize = 0;
    riff.dataId = 0x45564157;
    gFmt.chunkId = 0x20746D66;
    gFmt.chunkSize = 16;
    gFmt.formatId = 1;
    gFmt.channels = 2;
    gFmt.sampleRate = gFileOutSysValue.sampleRate;
    gFmt.bitPerSample = (ushort)16;
    gFmt.blockAlign = gFmt.channels * gFmt.bitPerSample >> 3;
    gFmt.bytePerSec = gFmt.sampleRate * gFmt.blockAlign;
    gFmt.dataId = 0x61746164;
    gFmt.dataSize = 0;

    // allocate out buffer
    auto pOutBuffer = (byte*)calloc(gFmt.blockAlign, gFileOutSysValue.bufferLength);

    // allocate channels
    gFileOutSysValue.ppChannels = (Channel**)calloc(CHANNEL_COUNT, sizeof(Channel*));
    gFileOutSysValue.ppChannelParam = (CHANNEL_PARAM**)calloc(CHANNEL_COUNT, sizeof(CHANNEL_PARAM*));
    for (int c = 0; c < CHANNEL_COUNT; c++) {
        gFileOutSysValue.ppChannels[c] = new Channel(&gFileOutSysValue, c);
        gFileOutSysValue.ppChannelParam[c] = &gFileOutSysValue.ppChannels[c]->Param;
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
    uint curPos = 0;
    double curTime = 0.0;
    double delta_sec = gFileOutSysValue.bufferLength * gFileOutSysValue.deltaTime;
    while (curPos < eventSize) {
        auto evTime = (double)(*(int*)(pEvents + curPos)) / baseTick;
        curPos += 4;
        auto evValue = pEvents + curPos;
        while (curTime < evTime) {
            fileout_write16(pOutBuffer);
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
    for (int c = 0; c < CHANNEL_COUNT; c++) {
        delete gFileOutSysValue.ppChannels[c];
        gFileOutSysValue.ppChannels[c] = NULL;
    }
    free(gFileOutSysValue.ppChannels);
    gFileOutSysValue.ppChannels = NULL;
    free(gFileOutSysValue.ppChannelParam);
    gFileOutSysValue.ppChannelParam = NULL;
}

/******************************************************************************/
int fileout_send(byte *pMsg) {
    auto type = (E_EVENT_TYPE)(*pMsg & 0xF0);
    auto ch = *pMsg & 0x0F;
    switch (type) {
    case E_EVENT_TYPE::NOTE_OFF:
        gFileOutSysValue.ppChannels[ch]->NoteOff(pMsg[1]);
        return 3;
    case E_EVENT_TYPE::NOTE_ON:
        gFileOutSysValue.ppChannels[ch]->NoteOn(pMsg[1], pMsg[2]);
        return 3;
    case E_EVENT_TYPE::POLY_KEY:
        return 3;
    case E_EVENT_TYPE::CTRL_CHG:
        gFileOutSysValue.ppChannels[ch]->CtrlChange(pMsg[1], pMsg[2]);
        return 3;
    case E_EVENT_TYPE::PROG_CHG:
        gFileOutSysValue.ppChannels[ch]->ProgramChange(pMsg[1]);
        return 2;
    case E_EVENT_TYPE::CH_PRESS:
        return 2;
    case E_EVENT_TYPE::PITCH:
        gFileOutSysValue.ppChannels[ch]->PitchBend(((pMsg[2] << 7) | pMsg[1]) - 8192);
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

inline void fileout_write16(byte* pOutBuffer) {
    /* sampler loop */
    for (int s = 0; s < SAMPLER_COUNT; s++) {
        auto pSmpl = gFileOutSysValue.ppSampler[s];
        if (pSmpl->state < E_SAMPLER_STATE::PURGE) {
            continue;
        }
        sampler(&gFileOutSysValue, pSmpl);
    }

    /* buffer clear */
    int buffSize = gFileOutSysValue.bufferLength * gFmt.blockAlign;
    memset(pOutBuffer, 0, buffSize);

    /* channel loop */
    for (int c = 0; c < CHANNEL_COUNT; c++) {
        auto pCh = gFileOutSysValue.ppChannels[c];
        auto pInputBuff = pCh->pInput;
        auto pInputBuffTerm = pInputBuff + gFileOutSysValue.bufferLength;
        auto pBuff = (short*)pOutBuffer;
        for (; pInputBuff < pInputBuffTerm; pInputBuff++, pBuff += 2) {
            double tempL = 0.0, tempR = 0.0;
            // effect
            pCh->Step(&tempL, &tempR);
            // output
            tempL *= 32767.0;
            tempR *= 32767.0;
            tempL += *(pBuff + 0);
            tempR += *(pBuff + 1);
            if (32767.0 < tempL) tempL = 32767.0;
            if (tempL < -32767.0) tempL = -32767.0;
            if (32767.0 < tempR) tempR = 32767.0;
            if (tempR < -32767.0) tempR = -32767.0;
            *(pBuff + 0) = (short)tempL;
            *(pBuff + 1) = (short)tempR;
            *pInputBuff = 0.0;
        }
    }

    fwrite(pOutBuffer, buffSize, 1, gfpFileOut);
    gFmt.dataSize += buffSize;
}
