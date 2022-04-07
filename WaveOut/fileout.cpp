#pragma once
#include "fileout.h"
#include "synth/channel.h"
#include "synth/sampler.h"
#include "synth/effect.h"
#include <stdio.h>

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
int fileOutSend(Channel **ppCh, LPBYTE msg);
void fileOutWrite(INST_SAMPLER **ppSmpl, EFFECT **ppCh, LPBYTE outBuffer);

/******************************************************************************/
int* WINAPI fileout_getProgressPtr() {
    return &gFileOutProgress;
}

void WINAPI fileout_save(
    LPWSTR waveTablePath,
    LPWSTR savePath,
    uint sampleRate,
    uint bitRate,
    LPBYTE pEvents,
    uint eventSize,
    uint baseTick
) {
    // set system value
    if (NULL != gFileOutSysValue.cInstList) {
        delete gFileOutSysValue.cInstList;
    }
    gFileOutSysValue.cInstList = new InstList(waveTablePath);
    gFileOutSysValue.pWaveTable = gFileOutSysValue.cInstList->GetWaveTablePtr();
    gFileOutSysValue.ppSampler = gFileOutSysValue.cInstList->GetSamplerPtr();
    gFileOutSysValue.bufferLength = 512;
    gFileOutSysValue.bufferCount = 16;
    gFileOutSysValue.sampleRate = sampleRate;
    gFileOutSysValue.bits = bitRate;
    gFileOutSysValue.deltaTime = 1.0 / gFileOutSysValue.sampleRate;

    // riff wave format
    RIFF riff;
    riff.riff = 0x46464952;
    riff.fileSize = 0;
    riff.dataId = 0x45564157;
    gFmt.chunkId = 0x20746D66;
    gFmt.chunkSize = 16;
    gFmt.formatId = 32 == bitRate ? 3 : 1;
    gFmt.channels = 2;
    gFmt.sampleRate = gFileOutSysValue.sampleRate;
    gFmt.bitPerSample = (ushort)bitRate;
    gFmt.blockAlign = gFmt.channels * gFmt.bitPerSample >> 3;
    gFmt.bytePerSec = gFmt.sampleRate * gFmt.blockAlign;
    gFmt.dataId = 0x61746164;
    gFmt.dataSize = 0;

    // allocate out buffer
    auto pOutBuffer = (LPBYTE)malloc(gFileOutSysValue.bufferLength * gFmt.blockAlign);

    // allocate effects
    effect_create(&gFileOutSysValue);

    // allocate channels
    auto **ppChannels = (Channel**)malloc(sizeof(Channel*) * CHANNEL_COUNT);
    for (int c = 0; c < CHANNEL_COUNT; c++) {
        ppChannels[c] = new Channel(&gFileOutSysValue, c);
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
    auto ppSampler = gFileOutSysValue.ppSampler;
    while (curPos < eventSize) {
        auto evTime = (double)(*(int*)(pEvents + curPos)) / baseTick;
        curPos += 4;
        auto evValue = pEvents + curPos;
        while (curTime < evTime) {
            fileOutWrite(ppSampler, gFileOutSysValue.ppEffect, pOutBuffer);
            curTime += gBpm * delta_sec / 60.0;
            gFileOutProgress = curPos;
        }
        curPos += fileOutSend(ppChannels, evValue);
    }
    gFileOutProgress = eventSize;

    // close file
    riff.fileSize = gFmt.dataSize + sizeof(gFmt) + 4;
    fseek(gfpFileOut, 0, SEEK_SET);
    fwrite(&riff, sizeof(riff), 1, gfpFileOut);
    fwrite(&gFmt, sizeof(gFmt), 1, gfpFileOut);
    fclose(gfpFileOut);

    // dispose
    effect_dispose(&gFileOutSysValue);
    for (int c = 0; c < CHANNEL_COUNT; c++) {
        delete ppChannels[c];
        ppChannels[c] = NULL;
    }
    free(ppChannels);
    ppChannels = NULL;
}

/******************************************************************************/
int fileOutSend(Channel **ppCh, LPBYTE msg) {
    auto type = (E_EVENT_TYPE)(*msg & 0xF0);
    auto ch = *msg & 0x0F;
    switch (type) {
    case E_EVENT_TYPE::NOTE_OFF:
        ppCh[ch]->NoteOff(msg[1]);
        return 3;
    case E_EVENT_TYPE::NOTE_ON:
        ppCh[ch]->NoteOn(msg[1], msg[2]);
        return 3;
    case E_EVENT_TYPE::POLY_KEY:
        return 3;
    case E_EVENT_TYPE::CTRL_CHG:
        ppCh[ch]->CtrlChange(msg[1], msg[2]);
        return 3;
    case E_EVENT_TYPE::PROG_CHG:
        ppCh[ch]->ProgramChange(msg[1]);
        return 2;
    case E_EVENT_TYPE::CH_PRESS:
        return 2;
    case E_EVENT_TYPE::PITCH:
        ppCh[ch]->PitchBend(((msg[2] << 7) | msg[1]) - 8192);
        return 3;
    case E_EVENT_TYPE::SYS_EX:
        if (0xFF == msg[0]) {
            auto type = (E_META_TYPE)msg[1];
            auto size = msg[2];
            switch (type) {
            case E_META_TYPE::TEMPO:
                gBpm = 60000000.0 / ((msg[3] << 16) | (msg[4] << 8) | msg[5]);
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

void fileOutWrite(INST_SAMPLER **ppSmpl, EFFECT **ppCh, LPBYTE outBuffer) {
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
    memset(outBuffer, 0, buffSize);

    /* channel loop */
    for (int c = 0; c < CHANNEL_COUNT; c++) {
        auto pEffect = gFileOutSysValue.ppEffect[c];
        auto pInputBuff = pEffect->pOutput;
        auto pInputBuffTerm = pInputBuff + pEffect->pSystemValue->bufferLength;
        auto pBuff = (short*)outBuffer;
        for (; pInputBuff < pInputBuffTerm; pInputBuff++, pBuff += 2) {
            double tempL, tempR;
            // effect
            effect(pEffect, pInputBuff, &tempL, &tempR);
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

    fwrite(outBuffer, buffSize, 1, gfpFileOut);
    gFmt.dataSize += buffSize;
}
