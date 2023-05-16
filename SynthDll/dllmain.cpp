#include <math.h>
#include <stdio.h>
#include <stdlib.h>

#include "inst/inst_list.h"
#include "synth/synth.h"

#include "waveout.h"
#include "dllmain.h"

#include <mmsystem.h>
#pragma comment (lib, "winmm.lib")

/******************************************************************************/
#pragma pack(push, 4)
struct SYSTEM_VALUE {
    int32 inst_count;
    byte* p_inst_list;
    byte* p_channel_params;
    int32* p_active_counter;
    int32* p_fileout_progress;
};
#pragma pack(pop)

/******************************************************************************/
WaveOut*     gp_waveout = nullptr;
Synth*       gp_synth = nullptr;
InstList*    gp_inst_list = nullptr;
int32        g_fileout_progress = 0;
SYSTEM_VALUE g_system_value = { 0 };

/******************************************************************************/
byte* WINAPI
synth_setup(
    LPWSTR file_path,
    int32 sample_rate,
    int32 buffer_length,
    int32 buffer_count
) {
    if (nullptr == gp_waveout) {
        gp_waveout = new WaveOut();
    } else {
        gp_waveout->close();
    }
    /*** Load system value ***/
    gp_inst_list = new InstList();
    auto load_status = gp_inst_list->Load(file_path);
    auto caption_err = L"ウェーブテーブル読み込み失敗";
    switch (load_status) {
    case E_LOAD_STATUS::FILE_OPEN_FAILED:
        MessageBoxW(nullptr, L"ファイルが開けませんでした。", caption_err, 0);
        break;
    case E_LOAD_STATUS::ALLOCATE_FAILED:
        MessageBoxW(nullptr, L"メモリの確保ができませんでした。", caption_err, 0);
        break;
    case E_LOAD_STATUS::UNKNOWN_FILE:
        MessageBoxW(nullptr, L"対応していない形式です。", caption_err, 0);
        break;
    default:
        break;
    }
    if (E_LOAD_STATUS::SUCCESS != load_status) {
        delete gp_inst_list;
        gp_inst_list = nullptr;
        return nullptr;
    }
    /*** Create system value ***/
    gp_synth = new Synth(gp_inst_list, sample_rate, buffer_length);
    /*** Open waveout ***/
    gp_waveout->open(sample_rate, buffer_length, buffer_count, &Synth::write_buffer, gp_synth);
    /*** Return system value ***/
    auto inst_list = gp_synth->p_inst_list->GetInstList();
    g_system_value.inst_count = inst_list->count;
    g_system_value.p_inst_list = (byte*)inst_list->ppData;
    g_system_value.p_channel_params = (byte*)gp_synth->pp_channel_params;
    g_system_value.p_active_counter = &gp_synth->active_count;
    g_system_value.p_fileout_progress = &g_fileout_progress;
    return (byte*)&g_system_value;
}

void WINAPI
synth_close() {
    if (nullptr == gp_waveout) {
        return;
    }
    gp_waveout->close();
    /*** Release system value ***/
    if (nullptr != gp_synth) {
        delete gp_synth;
        gp_synth = nullptr;
    }
    /*** Release inst list ***/
    if (nullptr != gp_inst_list) {
        delete gp_inst_list;
        gp_inst_list = nullptr;
    }
}

void WINAPI
fileout(
    LPWSTR wave_table_path,
    LPWSTR save_path,
    uint32 sample_rate,
    uint32 base_tick,
    uint32 event_size,
    byte* p_events
) {
    auto p_inst_list = new InstList();
    auto load_status = p_inst_list->Load(wave_table_path);
    auto caption_err = L"ウェーブテーブル読み込み失敗";
    switch (load_status) {
    case E_LOAD_STATUS::FILE_OPEN_FAILED:
        MessageBoxW(nullptr, L"ファイルが開けませんでした。", caption_err, 0);
        return;
    case E_LOAD_STATUS::ALLOCATE_FAILED:
        MessageBoxW(nullptr, L"メモリの確保ができませんでした。", caption_err, 0);
        return;
    case E_LOAD_STATUS::UNKNOWN_FILE:
        MessageBoxW(nullptr, L"対応していない形式です。", caption_err, 0);
        return;
    default:
        break;
    }
    if (E_LOAD_STATUS::SUCCESS != load_status) {
        delete p_inst_list;
        return;
    }

    /* set system value */
    auto p_synth = new Synth(p_inst_list, sample_rate, 256);

    /* riff wave format */
    uint32 riff_id = 0x46464952;
    uint32 file_size = 0;
    uint32 file_id = 0x45564157;
    const uint32 fmt_id = 0x20746D66;
    const uint32 fmt_size = 18;
    WAVEFORMATEX fmt;
    fmt.wFormatTag = 1;
    fmt.nChannels = 2;
    fmt.nSamplesPerSec = sample_rate;
    fmt.wBitsPerSample = (uint16)(sizeof(WAVE_DATA) << 3);
    fmt.nBlockAlign = fmt.nChannels * fmt.wBitsPerSample >> 3;
    fmt.nAvgBytesPerSec = fmt.nSamplesPerSec * fmt.nBlockAlign;
    const uint32 data_id = 0x61746164;
    uint32 data_size = 0;

    /* allocate pcm buffer */
    auto p_pcm_buffer = (WAVE_DATA*)calloc(p_synth->buffer_length, fmt.nBlockAlign);
    if (nullptr == p_pcm_buffer) {
        delete p_synth;
        delete p_inst_list;
        return;
    }

    /* open file */
    FILE* fp_out = nullptr;
    _wfopen_s(&fp_out, save_path, L"wb");
    if (nullptr == fp_out) {
        delete p_synth;
        delete p_inst_list;
        free(p_pcm_buffer);
        MessageBoxW(nullptr, L"wavファイルが作成できませんでした。", L"wavファイル出力エラー", 0);
        return;
    }
    fwrite(&riff_id, sizeof(riff_id), 1, fp_out);
    fwrite(&file_size, sizeof(file_size), 1, fp_out);
    fwrite(&file_id, sizeof(file_id), 1, fp_out);
    fwrite(&fmt_id, sizeof(fmt_id), 1, fp_out);
    fwrite(&fmt_size, sizeof(fmt_size), 1, fp_out);
    fwrite(&fmt, sizeof(fmt), 1, fp_out);
    fwrite(&data_id, sizeof(data_id), 1, fp_out);
    fwrite(&data_size, sizeof(data_size), 1, fp_out);

    //********************************
    // output wave
    //********************************
    const int32 buff_size = p_synth->buffer_length * fmt.nBlockAlign;
    const double delta_sec = p_synth->buffer_length * p_synth->delta_time;
    uint32 event_pos = 0;
    double time = 0.0;
    while (event_pos < event_size) {
        auto ev_time = (double)(*(int32*)(p_events + event_pos)) / base_tick;
        event_pos += 4;
        auto ev_value = p_events + event_pos;
        while (time < ev_time) {
            Synth::write_buffer(p_pcm_buffer, p_synth);
            fwrite(p_pcm_buffer, buff_size, 1, fp_out);
            data_size += buff_size;
            time += p_synth->bpm * delta_sec / 60.0;
            g_fileout_progress = event_pos;
        }
        event_pos += p_synth->send_message(0, ev_value);
    }
    g_fileout_progress = event_size;

    /* close file */
    file_size = data_size + sizeof(fmt) + 4;
    fseek(fp_out, 0, SEEK_SET);
    fwrite(&riff_id, sizeof(riff_id), 1, fp_out);
    fwrite(&file_size, sizeof(file_size), 1, fp_out);
    fwrite(&file_id, sizeof(file_id), 1, fp_out);
    fwrite(&fmt_id, sizeof(fmt_id), 1, fp_out);
    fwrite(&fmt_size, sizeof(fmt_size), 1, fp_out);
    fwrite(&fmt, sizeof(fmt), 1, fp_out);
    fwrite(&data_id, sizeof(data_id), 1, fp_out);
    fwrite(&data_size, sizeof(data_size), 1, fp_out);
    fclose(fp_out);

    /* dispose system value */
    delete p_synth;
    /* dispose inst list */
    delete p_inst_list;
    /* dispose pcm buffer */
    free(p_pcm_buffer);
}

void WINAPI
send_message(byte port, byte* p_msg) {
    if (nullptr != gp_synth) {
        gp_synth->send_message(port, p_msg);
    }
}
