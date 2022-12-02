#include <math.h>
#include <stdio.h>
#include <stdlib.h>

#include "inst/inst_list.h"
#include "synth/synth.h"

#include "dllmain.h"

#include <mmsystem.h>
#pragma comment (lib, "winmm.lib")

/******************************************************************************/
struct SYSTEM_VALUE {
    int32 inst_count;
    byte* p_inst_list;
    byte* p_channel_params;
    int32* p_active_counter;
    int32* p_fileout_progress;
};
#pragma pack(push, 4)
struct RIFF {
    uint32 riff;
    uint32 file_size;
    uint32 id;
};
#pragma pack(pop)

/******************************************************************************/
HWAVEOUT     waveout_handle = nullptr;
WAVEFORMATEX waveout_fmt = { 0 };
WAVEHDR*     waveout_hdr = nullptr;

HANDLE           waveout_thread = nullptr;
DWORD            waveout_thread_id = 0;
CRITICAL_SECTION waveout_buffer_lock = { 0 };

bool waveout_dostop = true;
bool waveout_stopped = true;
bool waveout_thread_stopped = true;

int32  waveout_write_count = 0;
int32  waveout_write_index = 0;
int32  waveout_read_index = 0;
int32  waveout_buffer_count = 0;
int32  waveout_buffer_length = 0;

Synth*       waveout_synth = nullptr;
InstList*    waveout_inst_list = nullptr;
SYSTEM_VALUE system_value = { 0 };
int32        fileout_progress = 0;

/******************************************************************************/
void CALLBACK
waveout_callback(HWAVEOUT hwo, UINT uMsg, DWORD_PTR dwInstance, DWORD dwParam1, DWORD dwParam) {
    switch (uMsg) {
    case MM_WOM_OPEN:
        waveout_dostop = false;
        waveout_stopped = false;
        waveout_thread_stopped = false;
        break;
    case MM_WOM_CLOSE:
        break;
    case MM_WOM_DONE:
        if (waveout_dostop) {
            waveout_stopped = true;
            break;
        }
        EnterCriticalSection((LPCRITICAL_SECTION)&waveout_buffer_lock);
        if (waveout_write_count < 1) {
            /*** Buffer empty ***/
            waveOutWrite(waveout_handle, &waveout_hdr[waveout_read_index], sizeof(WAVEHDR));
            LeaveCriticalSection((LPCRITICAL_SECTION)&waveout_buffer_lock);
            break;
        } else {
            /*** Output wave ***/
            waveOutWrite(waveout_handle, &waveout_hdr[waveout_read_index], sizeof(WAVEHDR));
            waveout_read_index = (waveout_read_index + 1) % waveout_buffer_count;
            waveout_write_count--;
        }
        LeaveCriticalSection((LPCRITICAL_SECTION)&waveout_buffer_lock);
        break;
    default:
        break;
    }
}

DWORD
waveout_buffer_writing_task(LPVOID* param) {
    while (true) {
        if (waveout_dostop) {
            waveout_thread_stopped = true;
            break;
        }
        EnterCriticalSection((LPCRITICAL_SECTION)&waveout_buffer_lock);
        if (waveout_buffer_count <= waveout_write_count + 1) {
            /*** Buffer full ***/
            LeaveCriticalSection((LPCRITICAL_SECTION)&waveout_buffer_lock);
            Sleep(1);
            continue;
        } else {
            /*** Write Buffer ***/
            auto p_buff = (WAVE_DATA*)waveout_hdr[waveout_write_index].lpData;
            memset(p_buff, 0, waveout_fmt.nBlockAlign * waveout_buffer_length);
            waveout_synth->write_buffer(p_buff);
            waveout_write_index = (waveout_write_index + 1) % waveout_buffer_count;
            waveout_write_count++;
        }
        LeaveCriticalSection((LPCRITICAL_SECTION)&waveout_buffer_lock);
    }
    return 0;
}

void
waveout_init(
    int32 sample_rate,
    int32 buffer_length,
    int32 buffer_count
) {
    /*** Init buffer counter ***/
    waveout_write_count = 0;
    waveout_write_index = 0;
    waveout_read_index = 0;
    waveout_buffer_count = buffer_count;
    waveout_buffer_length = buffer_length;
    /*** Set wave fmt ***/
    waveout_fmt.wFormatTag = 1;
    waveout_fmt.nChannels = 2;
    waveout_fmt.wBitsPerSample = (WORD)(sizeof(WAVE_DATA) << 3);
    waveout_fmt.nSamplesPerSec = (DWORD)sample_rate;
    waveout_fmt.nBlockAlign = waveout_fmt.nChannels * waveout_fmt.wBitsPerSample / 8;
    waveout_fmt.nAvgBytesPerSec = waveout_fmt.nSamplesPerSec * waveout_fmt.nBlockAlign;
    /*** Open waveout ***/
    if (MMSYSERR_NOERROR != waveOutOpen(
        &waveout_handle,
        WAVE_MAPPER,
        &waveout_fmt,
        (DWORD_PTR)waveout_callback,
        (DWORD_PTR)waveout_hdr,
        CALLBACK_FUNCTION
    )) {
        return;
    }
    /*** Init buffer locker ***/
    InitializeCriticalSection(&waveout_buffer_lock);
    /*** Allocate wave header ***/
    waveout_hdr = (WAVEHDR*)calloc(waveout_buffer_count, sizeof(WAVEHDR));
    if (nullptr == waveout_hdr) {
        return;
    }
    for (int32 i = 0; i < waveout_buffer_count; ++i) {
        auto pWaveHdr = &waveout_hdr[i];
        pWaveHdr->dwBufferLength = (DWORD)buffer_length * waveout_fmt.nBlockAlign;
        pWaveHdr->dwFlags = WHDR_BEGINLOOP | WHDR_ENDLOOP;
        pWaveHdr->dwLoops = 0;
        pWaveHdr->dwUser = 0;
        pWaveHdr->lpData = (LPSTR)calloc(buffer_length, waveout_fmt.nBlockAlign);
    }
    /*** Prepare wave header ***/
    for (int32 i = 0; i < waveout_buffer_count; ++i) {
        waveOutPrepareHeader(waveout_handle, &waveout_hdr[i], sizeof(WAVEHDR));
        waveOutWrite(waveout_handle, &waveout_hdr[i], sizeof(WAVEHDR));
    }
    /*** Create buffer writing proc thread ***/
    waveout_thread = CreateThread(
        nullptr,
        0,
        (LPTHREAD_START_ROUTINE)waveout_buffer_writing_task,
        nullptr,
        0,
        &waveout_thread_id
    );
    if (nullptr == waveout_thread) {
        synth_close();
        return;
    }
    SetThreadPriority(waveout_thread, THREAD_PRIORITY_HIGHEST);
}

void
waveout_stop() {
    waveout_dostop = true;
    while (!waveout_stopped || !waveout_thread_stopped) {
        Sleep(100);
    }
}

/******************************************************************************/
byte* WINAPI
synth_setup(
    LPWSTR file_path,
    int32 sample_rate,
    int32 buffer_length,
    int32 buffer_count
) {
    synth_close();
    /*** Load system value ***/
    waveout_inst_list = new InstList();
    auto load_status = waveout_inst_list->Load(file_path);
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
        delete waveout_inst_list;
        waveout_inst_list = nullptr;
        return nullptr;
    }
    /*** Create system value ***/
    waveout_synth = new Synth(waveout_inst_list, sample_rate, buffer_length);
    /*** Open waveout ***/
    waveout_init(sample_rate, buffer_length, buffer_count);
    /*** Return system value ***/
    auto inst_list = waveout_synth->p_inst_list->GetInstList();
    system_value.inst_count = inst_list->count;
    system_value.p_inst_list = (byte*)inst_list->ppData;
    system_value.p_channel_params = (byte*)waveout_synth->pp_channel_params;
    system_value.p_active_counter = &waveout_synth->active_count;
    system_value.p_fileout_progress = &fileout_progress;
    return (byte*)&system_value;
}

void WINAPI
synth_close() {
    if (nullptr == waveout_handle) {
        return;
    }
    waveout_stop();
    /*** Unprepare wave header ***/
    for (int32 i = 0; i < waveout_buffer_count; ++i) {
        waveOutUnprepareHeader(waveout_handle, &waveout_hdr[i], sizeof(WAVEHDR));
    }
    waveOutReset(waveout_handle);
    waveOutClose(waveout_handle);
    waveout_handle = nullptr;
    /*** Release wave header ***/
    for (int32 i = 0; i < waveout_buffer_count; ++i) {
        free(waveout_hdr[i].lpData);
    }
    free(waveout_hdr);
    waveout_hdr = nullptr;
    /*** Release system value ***/
    if (nullptr != waveout_synth) {
        delete waveout_synth;
        waveout_synth = nullptr;
    }
    /*** Release inst list ***/
    if (nullptr != waveout_inst_list) {
        delete waveout_inst_list;
        waveout_inst_list = nullptr;
    }
    /*** Delete buffer locker ***/
    if (nullptr != waveout_buffer_lock.DebugInfo) {
        DeleteCriticalSection(&waveout_buffer_lock);
        waveout_buffer_lock.DebugInfo = nullptr;
    }
}

void WINAPI
fileout_save(
    LPWSTR wave_table_path,
    LPWSTR save_path,
    uint32 sample_rate,
    byte* p_events,
    uint32 event_size,
    uint32 base_tick
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
    RIFF riff;
    riff.riff = 0x46464952;
    riff.file_size = 0;
    riff.id = 0x45564157;
    const uint32 chunk_id = 0x20746D66;
    const uint32 chunk_size = 18;
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
    if (NULL == p_pcm_buffer) {
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
    fwrite(&riff, sizeof(riff), 1, fp_out);
    fwrite(&chunk_id, sizeof(chunk_id), 1, fp_out);
    fwrite(&chunk_size, sizeof(chunk_size), 1, fp_out);
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
            p_synth->write_buffer(p_pcm_buffer);
            fwrite(p_pcm_buffer, buff_size, 1, fp_out);
            data_size += buff_size;
            time += p_synth->bpm * delta_sec / 60.0;
            fileout_progress = event_pos;
        }
        event_pos += p_synth->send_message(0, ev_value);
    }
    fileout_progress = event_size;

    /* close file */
    riff.file_size = data_size + sizeof(fmt) + 4;
    fseek(fp_out, 0, SEEK_SET);
    fwrite(&riff, sizeof(riff), 1, fp_out);
    fwrite(&chunk_id, sizeof(chunk_id), 1, fp_out);
    fwrite(&chunk_size, sizeof(chunk_size), 1, fp_out);
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
    if (nullptr != waveout_synth) {
        waveout_synth->send_message(port, p_msg);
    }
}
