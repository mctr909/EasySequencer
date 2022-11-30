#include <stdio.h>
#include <stdlib.h>

#include "inst/inst_list.h"
#include "message_reciever.h"

#include "fileout.h"

/******************************************************************************/
#pragma pack(push, 4)
typedef struct {
    uint32 riff;
    uint32 file_size;
    uint32 id;
} RIFF;
#pragma pack(pop)

#pragma pack(push, 4)
typedef struct {
    uint32 chunk_id;
    uint32 chunk_size;
    uint16 format_id;
    uint16 channels;
    uint32 sample_rate;
    uint32 byte_per_sec;
    uint16 block_align;
    uint16 bit_per_sample;
    uint32 data_id;
    uint32 data_size;
} FMT_;
#pragma pack(pop)

/******************************************************************************/
SYSTEM_VALUE  gFileOutSysValue = { 0 };
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
    synth_create(&gFileOutSysValue, cInst, sampleRate, 256);

    /* riff wave format */
    RIFF riff;
    riff.riff = 0x46464952;
    riff.file_size = 0;
    riff.id = 0x45564157;
    FMT_ fmt;
    fmt.chunk_id = 0x20746D66;
    fmt.chunk_size = 16;
    fmt.format_id = 1;
    fmt.channels = 2;
    fmt.sample_rate = gFileOutSysValue.sample_rate;
    fmt.bit_per_sample = (uint16)16;
    fmt.block_align = fmt.channels * fmt.bit_per_sample >> 3;
    fmt.byte_per_sec = fmt.sample_rate * fmt.block_align;
    fmt.data_id = 0x61746164;
    fmt.data_size = 0;

    /* allocate pcm buffer */
    auto pPcm_buffer = (LPSTR)calloc(gFileOutSysValue.buffer_length, fmt.block_align);

    /* open file */
    FILE* fp_out = NULL;
    _wfopen_s(&fp_out, savePath, L"wb");
    fwrite(&riff, sizeof(riff), 1, fp_out);
    fwrite(&fmt, sizeof(fmt), 1, fp_out);

    //********************************
    // output wave
    //********************************
    uint32 cur_pos = 0;
    double cur_time = 0.0;
    double delta_sec = gFileOutSysValue.buffer_length * gFileOutSysValue.delta_time;
    int32 buff_size = gFileOutSysValue.buffer_length * fmt.block_align;
    while (cur_pos < eventSize) {
        auto ev_time = (double)(*(int32*)(pEvents + cur_pos)) / baseTick;
        cur_pos += 4;
        auto ev_value = pEvents + cur_pos;
        while (cur_time < ev_time) {
            synth_write_buffer_perform(&gFileOutSysValue, pPcm_buffer);
            fwrite(pPcm_buffer, buff_size, 1, fp_out);
            fmt.data_size += buff_size;

            cur_time += gFileOutSysValue.bpm * delta_sec / 60.0;
            gFileOutProgress = cur_pos;
        }
        cur_pos += message_perform(&gFileOutSysValue, ev_value);
    }
    gFileOutProgress = eventSize;

    /* close file */
    riff.file_size = fmt.data_size + sizeof(fmt) + 4;
    fseek(fp_out, 0, SEEK_SET);
    fwrite(&riff, sizeof(riff), 1, fp_out);
    fwrite(&fmt, sizeof(fmt), 1, fp_out);
    fclose(fp_out);

    /* dispose system value */
    synth_dispose(&gFileOutSysValue);

    /* dispose pcm buffer */
    if (NULL != pPcm_buffer) {
        free(pPcm_buffer);
        pPcm_buffer = NULL;
    }
}
