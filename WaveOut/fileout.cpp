#include <stdio.h>
#include <stdlib.h>

#include "inst/inst_list.h"
#include "synth/synth.h"

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
int32  gFileOutProgress = 0;

/******************************************************************************/
int32* WINAPI
fileout_progress_ptr() {
    return &gFileOutProgress;
}

void WINAPI
fileout_save(
    LPWSTR wave_table_path,
    LPWSTR save_path,
    uint32 sample_rate,
    byte* pEvents,
    uint32 event_size,
    uint32 base_tick
) {
    auto pInst_list = new InstList();
    auto load_status = pInst_list->Load(wave_table_path);
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
        delete pInst_list;
        return;
    }

    /* set system value */
    auto pSynth = new Synth(pInst_list, sample_rate, 256);

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
    fmt.sample_rate = sample_rate;
    fmt.bit_per_sample = (uint16)(sizeof(WAVDAT) << 3);
    fmt.block_align = fmt.channels * fmt.bit_per_sample >> 3;
    fmt.byte_per_sec = fmt.sample_rate * fmt.block_align;
    fmt.data_id = 0x61746164;
    fmt.data_size = 0;

    /* allocate pcm buffer */
    auto pPcm_buffer = (byte*)calloc(pSynth->buffer_length, fmt.block_align);
    if (NULL == pPcm_buffer) {
        delete pSynth;
        delete pInst_list;
        return;
    }

    /* open file */
    FILE* fp_out = NULL;
    _wfopen_s(&fp_out, save_path, L"wb");
    if (NULL == fp_out) {
        delete pSynth;
        delete pInst_list;
        free(pPcm_buffer);
        MessageBoxW(NULL, L"wavファイルが作成できませんでした。", L"wavファイル出力エラー", 0);
        return;
    }
    fwrite(&riff, sizeof(riff), 1, fp_out);
    fwrite(&fmt, sizeof(fmt), 1, fp_out);

    //********************************
    // output wave
    //********************************
    const int32 buff_size = pSynth->buffer_length * fmt.block_align;
    const double delta_sec = pSynth->buffer_length * pSynth->delta_time;
    uint32 event_pos = 0;
    double time = 0.0;
    while (event_pos < event_size) {
        auto ev_time = (double)(*(int32*)(pEvents + event_pos)) / base_tick;
        event_pos += 4;
        auto ev_value = pEvents + event_pos;
        while (time < ev_time) {
            pSynth->write_buffer(pPcm_buffer);
            fwrite(pPcm_buffer, buff_size, 1, fp_out);
            fmt.data_size += buff_size;
            time += pSynth->bpm * delta_sec / 60.0;
            gFileOutProgress = event_pos;
        }
        event_pos += pSynth->send_message(0, ev_value);
    }
    gFileOutProgress = event_size;

    /* close file */
    riff.file_size = fmt.data_size + sizeof(fmt) + 4;
    fseek(fp_out, 0, SEEK_SET);
    fwrite(&riff, sizeof(riff), 1, fp_out);
    fwrite(&fmt, sizeof(fmt), 1, fp_out);
    fclose(fp_out);

    /* dispose system value */
    delete pSynth;
    /* dispose inst list */
    delete pInst_list;
    /* dispose pcm buffer */
    if (NULL != pPcm_buffer) {
        free(pPcm_buffer);
        pPcm_buffer = NULL;
    }
}
