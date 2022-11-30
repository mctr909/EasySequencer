#include <stdio.h>
#include <stdlib.h>

#include "synth/channel.h"
#include "synth/channel_const.h"
#include "synth/channel_params.h"
#include "synth/sampler.h"
#include "inst/inst_list.h"
#include "message_reciever.h"

#include "fileout.h"

/******************************************************************************/
typedef struct INST_SAMPLER INST_SAMPLER;

/******************************************************************************/
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

/******************************************************************************/
int32* WINAPI
fileout_progress_ptr() {
    return &gFileOutProgress;
}

void WINAPI
fileout_save(
    LPWSTR waveTablePath,
    LPWSTR savePath,
    uint32 sampleRate,
    byte *pEvents,
    uint32 eventSize,
    uint32 baseTick
) {
    auto cInst = new InstList();
    auto load_status = cInst->Load(waveTablePath);
    auto caption_err = L"ウェーブテーブル読み込み失敗";
    switch (load_status) {
    case E_LOAD_STATUS::WAVE_TABLE_OPEN_FAILED:
        MessageBoxW(NULL, L"ファイルが開けませんでした。", caption_err, 0);
        return;
    case E_LOAD_STATUS::WAVE_TABLE_ALLOCATE_FAILED:
        MessageBoxW(NULL, L"メモリの確保ができませんでした。", caption_err, 0);
        return;
    case E_LOAD_STATUS::WAVE_TABLE_UNKNOWN_FILE:
        MessageBoxW(NULL, L"対応していない形式です。", caption_err, 0);
        return;
    default:
        break;
    }
    if (E_LOAD_STATUS::SUCCESS != load_status) {
        delete cInst;
        return;
    }

    /* set system value */
    gFileOutSysValue.cInst_list = cInst;
    gFileOutSysValue.pWave_table = gFileOutSysValue.cInst_list->GetWaveTablePtr();
    gFileOutSysValue.buffer_length = 256;
    gFileOutSysValue.buffer_count = 16;
    gFileOutSysValue.sample_rate = sampleRate;
    gFileOutSysValue.delta_time = 1.0 / gFileOutSysValue.sample_rate;
    gFileOutSysValue.bpm = 120.0;
    /* allocate output buffer */
    gFileOutSysValue.pBuffer_l = (double*)calloc(gFileOutSysValue.buffer_length, sizeof(double));
    gFileOutSysValue.pBuffer_r = (double*)calloc(gFileOutSysValue.buffer_length, sizeof(double));
    /* allocate samplers */
    gFileOutSysValue.ppSampler = (Sampler**)calloc(SAMPLER_COUNT, sizeof(Sampler*));
    for (uint32 i = 0; i < SAMPLER_COUNT; i++) {
        gFileOutSysValue.ppSampler[i] = new Sampler(&gFileOutSysValue);
    }
    /* allocate channels */
    gFileOutSysValue.ppChannels = (Channel**)calloc(CHANNEL_COUNT, sizeof(Channel*));
    gFileOutSysValue.ppChannel_params = (CHANNEL_PARAM**)calloc(CHANNEL_COUNT, sizeof(CHANNEL_PARAM*));
    for (int32 i = 0; i < CHANNEL_COUNT; i++) {
        gFileOutSysValue.ppChannels[i] = new Channel(&gFileOutSysValue, i);
        gFileOutSysValue.ppChannel_params[i] = &gFileOutSysValue.ppChannels[i]->param;
    }
    /* allocate pcm buffer */
    auto pPcmBuffer = (LPSTR)calloc(gFileOutSysValue.buffer_length, gFmt.blockAlign);

    /* riff wave format */
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

    /* open file */
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
    uint32 cur_pos = 0;
    double cur_time = 0.0;
    double delta_sec = gFileOutSysValue.buffer_length * gFileOutSysValue.delta_time;
    int32 buff_size = gFileOutSysValue.buffer_length * gFmt.blockAlign;
    while (cur_pos < eventSize) {
        auto evTime = (double)(*(int32*)(pEvents + cur_pos)) / baseTick;
        cur_pos += 4;
        auto evValue = pEvents + cur_pos;
        while (cur_time < evTime) {
            synth_write_buffer_perform(&gFileOutSysValue, pPcmBuffer);
            fwrite(pPcmBuffer, buff_size, 1, gfpFileOut);
            gFmt.dataSize += buff_size;

            cur_time += gFileOutSysValue.bpm * delta_sec / 60.0;
            gFileOutProgress = cur_pos;
        }
        cur_pos += message_perform(&gFileOutSysValue, evValue);
    }
    gFileOutProgress = eventSize;

    /* close file */
    riff.fileSize = gFmt.dataSize + sizeof(gFmt) + 4;
    fseek(gfpFileOut, 0, SEEK_SET);
    fwrite(&riff, sizeof(riff), 1, gfpFileOut);
    fwrite(&gFmt, sizeof(gFmt), 1, gfpFileOut);
    fclose(gfpFileOut);

    /* dispose pcm buffer */
    if (NULL != pPcmBuffer) {
        free(pPcmBuffer);
        pPcmBuffer = NULL;
    }
    /* dispose output buffer */
    if (NULL != gFileOutSysValue.pBuffer_l) {
        free(gFileOutSysValue.pBuffer_l);
        gFileOutSysValue.pBuffer_l = NULL;
    }
    if (NULL != gFileOutSysValue.pBuffer_r) {
        free(gFileOutSysValue.pBuffer_r);
        gFileOutSysValue.pBuffer_r = NULL;
    }
    /* dispose samplers */
    if (NULL != gFileOutSysValue.ppSampler) {
        for (uint32 i = 0; i < SAMPLER_COUNT; i++) {
            delete gFileOutSysValue.ppSampler[i];
        }
        free(gFileOutSysValue.ppSampler);
        gFileOutSysValue.ppSampler = NULL;
    }
    /* dispose channels */
    if (NULL != gFileOutSysValue.ppChannels) {
        for (int32 i = 0; i < CHANNEL_COUNT; i++) {
            delete gFileOutSysValue.ppChannels[i];
        }
        free(gFileOutSysValue.ppChannels);
        gFileOutSysValue.ppChannels = NULL;
    }
    free(gFileOutSysValue.ppChannel_params);
    gFileOutSysValue.ppChannel_params = NULL;
}
