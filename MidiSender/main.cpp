#include "../WaveOut/sampler.h"
#include "main.h"
#include "channel.h"
#include <stdio.h>
#include <windows.h>
#include <mmsystem.h>

#pragma comment (lib, "winmm.lib")
#pragma comment (lib, "WaveOut.lib")

/******************************************************************************/
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
LPBYTE        gpWaveTable = NULL;
SYSTEM_VALUE  gSysValue;
CHANNEL_VALUE **gppFileOutChValues = NULL;
CHANNEL       **gppFileOutChParams = NULL;
SAMPLER       **gppFileOutSamplers = NULL;

LPBYTE        gpFileOutBuffer = NULL;
FILE          *gfpFileOut = NULL;
RIFF          gRiff;
FMT_          gFmt;
Channel       **gppChannels = NULL;
CHANNEL_PARAM **gppChParam = NULL;

/******************************************************************************/
CHANNEL** wavfileout_GetChannelPtr() {
    return gppFileOutChParams;
}

SAMPLER** wavfileout_GetSamplerPtr() {
    return gppFileOutSamplers;
}

void wavfileout_Open(LPWSTR filePath, LPBYTE pWaveTable, uint sampleRate, uint bitRate) {
    gpWaveTable = pWaveTable;
    disposeSamplers(gppFileOutSamplers, gSysValue.samplerCount);
    disposeChannels(gppFileOutChValues);
    //
    gSysValue.bufferLength = 512;
    gSysValue.bufferCount = 16;
    gSysValue.channelCount = 16;
    gSysValue.samplerCount = 64;
    gSysValue.sampleRate = sampleRate;
    gSysValue.bits = bitRate;
    gSysValue.deltaTime = 1.0 / gSysValue.sampleRate;
    //
    free(gpFileOutBuffer);
    gpFileOutBuffer = (LPBYTE)malloc(gSysValue.bufferLength * gFmt.blockAlign);
    //
    free(gppFileOutChParams);
    gppFileOutChValues = createChannels(&gSysValue);
    gppFileOutChParams = (CHANNEL**)malloc(sizeof(CHANNEL*) * gSysValue.channelCount);
    for (int i = 0; i < gSysValue.channelCount; ++i) {
        gppFileOutChParams[i] = gppFileOutChValues[i]->pParam;
    }
    //
    gppFileOutSamplers = createSamplers(gSysValue.samplerCount);
    //
    if (NULL != gfpFileOut) {
        fclose(gfpFileOut);
        gfpFileOut = NULL;
    }
    _wfopen_s(&gfpFileOut, filePath, L"wb");
    //
    gRiff.riff = 0x46464952;
    gRiff.fileSize = 0;
    gRiff.dataId = 0x45564157;
    //
    gFmt.chunkId = 0x20746D66;
    gFmt.chunkSize = 16;
    gFmt.formatId = 32 == bitRate ? 3 : 1;
    gFmt.channels = 2;
    gFmt.sampleRate = gSysValue.sampleRate;
    gFmt.bitPerSample = (ushort)bitRate;
    gFmt.blockAlign = gFmt.channels * gFmt.bitPerSample >> 3;
    gFmt.bytePerSec = gFmt.sampleRate * gFmt.blockAlign;
    gFmt.dataId = 0x61746164;
    gFmt.dataSize = 0;
    //
    fwrite(&gRiff, sizeof(gRiff), 1, gfpFileOut);
    fwrite(&gFmt, sizeof(gFmt), 1, gfpFileOut);
}

void wavfileout_Close() {
    if (NULL == gfpFileOut) {
        return;
    }
    //
    gRiff.fileSize = gFmt.dataSize + sizeof(gFmt) + 4;
    //
    fseek(gfpFileOut, 0, SEEK_SET);
    fwrite(&gRiff, sizeof(gRiff), 1, gfpFileOut);
    fwrite(&gFmt, sizeof(gFmt), 1, gfpFileOut);
    //
    fclose(gfpFileOut);
    gfpFileOut = NULL;
}

void wavfileout_Write() {
    for (int s = 0; s < gSysValue.samplerCount; ++s) {
        if (E_KEY_STATE_STANDBY == gppFileOutSamplers[s]->state) {
            continue;
        }
        sampler(gppFileOutChValues, gppFileOutSamplers[s], gpWaveTable);
    }

    int buffSize = gSysValue.bufferLength * gFmt.blockAlign;
    memset(gpFileOutBuffer, 0, buffSize);

    switch (gSysValue.bits) {
    case 16:
        for (int c = 0; c < gSysValue.channelCount; ++c) {
            channel16(gppFileOutChValues[c], (short*)gpFileOutBuffer);
        }
        break;
    case 24:
        for (int c = 0; c < gSysValue.channelCount; ++c) {
            channel24(gppFileOutChValues[c], (int24*)gpFileOutBuffer);
        }
        break;
    case 32:
        for (int c = 0; c < gSysValue.channelCount; ++c) {
            channel32(gppFileOutChValues[c], (float*)gpFileOutBuffer);
        }
        break;
    }
    fwrite(gpFileOutBuffer, buffSize, 1, gfpFileOut);
    gFmt.dataSize += buffSize;
}

/******************************************************************************/
CHANNEL_PARAM** midi_GetChannelParamPtr() {
    if (NULL == gppChParam) {
        gppChParam = (CHANNEL_PARAM**)malloc(sizeof(CHANNEL_PARAM*) * 16);
        for (int i = 0; i < 16; i++) {
            gppChParam[i] = &gppChannels[i]->Param;
        }
    }
    return gppChParam;
}

void midi_CreateChannels(INST_LIST *list, SAMPLER **ppSmpl, CHANNEL **ppCh, uint samplerCount) {
    if (NULL != gppChannels) {
        for (int i = 0; i < 16; i++) {
            delete gppChannels[i];
            gppChannels[i] = NULL;
        }
        free(gppChannels);
        gppChannels = NULL;
    }
    //
    gppChannels = (Channel**)malloc(sizeof(Channel*) * 16);
    for (int i = 0; i < 16; i++) {
        gppChannels[i] = new Channel(list, ppSmpl, ppCh[i], i, samplerCount);
    }
}

void midi_Send(LPBYTE msg) {
    auto type = (E_EVENT_TYPE)(*msg & 0xF0);
    auto ch = *msg & 0x0F;
    switch (type) {
    case E_EVENT_TYPE::NOTE_OFF:
        gppChannels[ch]->NoteOff(msg[1], E_KEY_STATE_RELEASE);
        break;
    case E_EVENT_TYPE::NOTE_ON:
        gppChannels[ch]->NoteOn(msg[1], msg[2]);
        break;
    case E_EVENT_TYPE::CTRL_CHG:
        gppChannels[ch]->CtrlChange(msg[1], msg[2]);
        break;
    case E_EVENT_TYPE::PROG_CHG:
        gppChannels[ch]->ProgramChange(msg[1]);
        break;
    case E_EVENT_TYPE::PITCH:
        gppChannels[ch]->PitchBend(((msg[2] << 7) | msg[1]) - 8192);
        break;
    }
}
