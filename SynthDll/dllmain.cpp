#include <math.h>
#include <stdio.h>
#include <stdlib.h>

#include "synth/synth.h"
#include "waveout.h"
#include "dllmain.h"

#include <mmsystem.h>
#pragma comment (lib, "winmm.lib")

/******************************************************************************/
WaveOut* gp_waveout = nullptr;
Synth*   gp_synth = nullptr;

/******************************************************************************/
byte* WINAPI
synth_setup(
    LPWSTR wave_table_path,
    int32 sample_rate,
    int32 buffer_length,
    int32 buffer_count
) {
    synth_close();
    /*** Create system value ***/
    gp_synth = new Synth();
    auto load_status = gp_synth->setup(wave_table_path, sample_rate, buffer_length);
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
        delete gp_synth;
        gp_synth = nullptr;
        return nullptr;
    }
    /*** Open waveout ***/
    gp_waveout = new WaveOut();
    gp_waveout->open(sample_rate, buffer_length, buffer_count, &Synth::write_buffer, gp_synth);
    /*** Return system value ***/
    return (byte*)&gp_synth->m_system_value;
}

void WINAPI
synth_close() {
    /*** Release waveout ***/
    if (nullptr != gp_waveout) {
        gp_waveout->close();
        delete gp_waveout;
        gp_waveout = nullptr;
    }
    /*** Release system value ***/
    if (nullptr != gp_synth) {
        delete gp_synth;
        gp_synth = nullptr;
    }
}

void WINAPI
fileout(
    LPWSTR wave_table_path,
    LPWSTR save_path,
    uint32 sample_rate,
    uint32 base_tick,
    uint32 event_size,
    byte* p_events,
    int32* p_progress
) {
    /* set system value */
    auto p_synth = new Synth();
    auto load_status = p_synth->setup(wave_table_path, sample_rate, 256);
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
        delete p_synth;
        return;
    }
    //********************************
    // output wave
    //********************************
    if (!p_synth->save_wav(save_path, base_tick, event_size, p_events, p_progress)) {
        MessageBoxW(nullptr, L"wavファイル出力に失敗しました。", L"", 0);
    }
    /* dispose system value */
    delete p_synth;
}

void WINAPI
send_message(byte port, byte* p_msg) {
    if (nullptr != gp_synth) {
        gp_synth->send_message(port, p_msg);
    }
}