#include "../WaveOut/sampler.h"
#include "../WaveOut/effect.h"
#include "main.h"
#include "channel.h"
#include <stdio.h>
#include <windows.h>
#include <mmsystem.h>

#pragma comment (lib, "winmm.lib")
#pragma comment (lib, "WaveOut.lib")

#define PARALLEL_MAX 16

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
Channel       **gppChannels = NULL;
CHANNEL_PARAM **gppChParam = NULL;

SYSTEM_VALUE  gSysValue;
int           gFileOutProgress = 0;
FMT_          gFmt = { 0 };
FILE          *gfpFileOut = NULL;

/******************************************************************************/
uint wavFileOutSend(Channel **ppCh, LPBYTE msg);
void wavFileOutWrite(SAMPLER **ppSmpl, CHANNEL_VALUE **ppCh, LPBYTE outBuffer);

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

int* midi_GetWavFileOutProgressPtr() {
    return &gFileOutProgress;
}

void midi_CreateChannels(INST_LIST *list, SAMPLER **ppSmpl, NOTE **ppNote, CHANNEL **ppCh, uint samplerCount) {
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
        gppChannels[i] = new Channel(list, ppSmpl, ppNote, ppCh[i], i, samplerCount);
    }
}

void midi_Send(LPBYTE msg) {
    auto type = (E_EVENT_TYPE)(*msg & 0xF0);
    auto ch = *msg & 0x0F;
    switch (type) {
    case E_EVENT_TYPE::NOTE_OFF:
        gppChannels[ch]->NoteOff(msg[1], E_NOTE_STATE_RELEASE);
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

void midi_WavFileOut(
    LPWSTR filePath,
    LPBYTE pWaveTable,
    INST_LIST *list,
    uint sampleRate,
    uint bitRate,
    LPBYTE pEvents,
    uint eventSize,
    uint baseTick
) {
    gpWaveTable = pWaveTable;
    // set system value
    gSysValue.bufferLength = 512;
    gSysValue.bufferCount = 16;
    gSysValue.channelCount = 16;
    gSysValue.samplerCount = 64;
    gSysValue.sampleRate = sampleRate;
    gSysValue.bits = bitRate;
    gSysValue.deltaTime = 1.0 / gSysValue.sampleRate;
    // riff wave format
    RIFF riff;
    riff.riff = 0x46464952;
    riff.fileSize = 0;
    riff.dataId = 0x45564157;
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
    // allocate out buffer
    LPBYTE pOutBuffer = (LPBYTE)malloc(gSysValue.bufferLength * gFmt.blockAlign);
    // allocate notes
    NOTE** ppNotes = createNotes(gSysValue.samplerCount);
    // allocate samplers
    SAMPLER **ppSamplers = createSamplers(gSysValue.samplerCount);
    // allocate channels
    Channel **ppChannels = (Channel**)malloc(sizeof(Channel*) * gSysValue.channelCount);
    CHANNEL_VALUE **ppChValues = createChannels(&gSysValue);
    for (int i = 0; i < gSysValue.channelCount; ++i) {
        ppChannels[i] = new Channel(list, ppSamplers, ppNotes, (CHANNEL*)ppChValues[i]->pParam, i, gSysValue.samplerCount);
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
    double delta_sec = gSysValue.bufferLength * gSysValue.deltaTime;
    while (curPos < eventSize) {
        auto evTime = (double)(*(uint*)(pEvents + curPos)) / baseTick;
        auto evValue = pEvents + curPos + 4;
        while (curTime < evTime) {
            wavFileOutWrite(ppSamplers, ppChValues, pOutBuffer);
            curTime += bpm * delta_sec / 60.0;
            gFileOutProgress = (int)curTime;
        }
        if (E_EVENT_TYPE::META == (E_EVENT_TYPE)evValue[0]) {
            if (E_META_TYPE::TEMPO == (E_META_TYPE)evValue[1]) {
                bpm = 60000000.0 / ((evValue[6] << 16) | (evValue[7] << 8) | evValue[8]);
            }
        }
        auto readSize = wavFileOutSend(ppChannels, evValue);
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
    disposeSamplers(ppSamplers, gSysValue.samplerCount);
    disposeChannels(ppChValues);
    for (int i = 0; i < gSysValue.channelCount; ++i) {
        delete ppChannels[i];
        ppChannels[i] = NULL;
    }
    free(ppChannels);
}

/******************************************************************************/
uint wavFileOutSend(Channel **ppCh, LPBYTE msg) {
    auto type = (E_EVENT_TYPE)(*msg & 0xF0);
    auto ch = *msg & 0x0F;
    switch (type) {
    case E_EVENT_TYPE::NOTE_OFF:
        ppCh[ch]->NoteOff(msg[1], E_NOTE_STATE_RELEASE);
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

void wavFileOutWrite(SAMPLER **ppSmpl, CHANNEL_VALUE **ppCh, LPBYTE outBuffer) {
    // sampler loop
    int buffSize = gSysValue.bufferLength * gFmt.blockAlign;
    memset(outBuffer, 0, buffSize);
    // channel loop
    fwrite(outBuffer, buffSize, 1, gfpFileOut);
    gFmt.dataSize += buffSize;
}
