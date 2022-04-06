#pragma once
#include "fileout.h"
#include "sampler.h"
#include "effect.h"
#include "channel.h"
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
LPBYTE        gpFileOutWaveTable = NULL;
SYSTEM_VALUE  gFileOutSysValue = { 0 };
int           gFileOutProgress = 0;
FMT_          gFmt = { 0 };
FILE          *gfpFileOut = NULL;

/******************************************************************************/
uint fileOutSend(Channel **ppCh, LPBYTE msg);
void fileOutWrite(INST_SAMPLER **ppSmpl, EFFECT **ppCh, LPBYTE outBuffer);

/******************************************************************************/
int* WINAPI waveout_getFileOutProgressPtr() {
    return &gFileOutProgress;
}

void WINAPI waveout_fileOut(
    LPWSTR filePath,
    LPBYTE pWaveTable,
    uint sampleRate,
    uint bitRate,
    LPBYTE pEvents,
    uint eventSize,
    uint baseTick
) {
    gpFileOutWaveTable = pWaveTable;

    // set system value
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
    _wfopen_s(&gfpFileOut, filePath, L"wb");
    fwrite(&riff, sizeof(riff), 1, gfpFileOut);
    fwrite(&gFmt, sizeof(gFmt), 1, gfpFileOut);

    //********************************
    // output wave
    //********************************
    uint curPos = 0;
    double curTime = 0.0;
    double bpm = 120.0;
    double delta_sec = gFileOutSysValue.bufferLength * gFileOutSysValue.deltaTime;
    auto ppSampler = gFileOutSysValue.ppSampler;
    while (curPos < eventSize) {
        auto evTime = (double)(*(uint*)(pEvents + curPos)) / baseTick;
        auto evValue = pEvents + curPos + 4;
        while (curTime < evTime) {
            fileOutWrite(ppSampler, gFileOutSysValue.ppEffect, pOutBuffer);
            curTime += bpm * delta_sec / 60.0;
            gFileOutProgress = (int)curTime;
        }
        if (E_EVENT_TYPE::META == (E_EVENT_TYPE)evValue[0]) {
            if (E_META_TYPE::TEMPO == (E_META_TYPE)evValue[1]) {
                bpm = 60000000.0 / ((evValue[6] << 16) | (evValue[7] << 8) | evValue[8]);
            }
        }
        auto readSize = fileOutSend(ppChannels, evValue);
        if (0 == readSize) {
            break;
        }
        curPos += readSize + 4;
    }

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
uint fileOutSend(Channel **ppCh, LPBYTE msg) {
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
        if (E_EVENT_TYPE::META == (E_EVENT_TYPE)*msg) {
            return (msg[2] | (msg[3] << 8) | (msg[4] << 16) | (msg[5] << 24)) + 6;
        } else {
            return (msg[1] | (msg[2] << 8) | (msg[3] << 16) | (msg[4] << 24)) + 5;
        }
    default:
        return 0;
    }
}

void fileOutWrite(INST_SAMPLER **ppSmpl, EFFECT **ppCh, LPBYTE outBuffer) {
    // sampler loop
    int buffSize = gFileOutSysValue.bufferLength * gFmt.blockAlign;
    memset(outBuffer, 0, buffSize);
    // channel loop
    fwrite(outBuffer, buffSize, 1, gfpFileOut);
    gFmt.dataSize += buffSize;
}
