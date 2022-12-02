#include <math.h>
#include <stdio.h>
#include <stdlib.h>

#include "inst/inst_list.h"
#include "synth/synth.h"

#include "waveout.h"

#include <mmsystem.h>
#pragma comment (lib, "winmm.lib")

/******************************************************************************/
HWAVEOUT     waveout_handle = NULL;
WAVEFORMATEX waveout_fmt = { 0 };
WAVEHDR*     waveout_hdr = NULL;

HANDLE           waveout_thread = NULL;
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

Synth*    waveout_synth = NULL;
InstList* waveout_inst_list = NULL;

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
            auto pBuff = (WAVE_DATA*)waveout_hdr[waveout_write_index].lpData;
            memset(pBuff, 0, waveout_fmt.nBlockAlign * waveout_buffer_length);
            waveout_synth->write_buffer(pBuff);
            waveout_write_index = (waveout_write_index + 1) % waveout_buffer_count;
            waveout_write_count++;
        }
        LeaveCriticalSection((LPCRITICAL_SECTION)&waveout_buffer_lock);
    }
    return 0;
}

void
waveout_init(
    int32 sampleRate,
    int32 bufferLength,
    int32 bufferCount
) {
    /*** Init buffer counter ***/
    waveout_write_count = 0;
    waveout_write_index = 0;
    waveout_read_index = 0;
    waveout_buffer_count = bufferCount;
    waveout_buffer_length = bufferLength;
    /*** Set wave fmt ***/
    waveout_fmt.wFormatTag = 1;
    waveout_fmt.nChannels = 2;
    waveout_fmt.wBitsPerSample = (WORD)(sizeof(WAVE_DATA) << 3);
    waveout_fmt.nSamplesPerSec = (DWORD)sampleRate;
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
    if (NULL == waveout_hdr) {
        return;
    }
    for (int32 i = 0; i < waveout_buffer_count; ++i) {
        auto pWaveHdr = &waveout_hdr[i];
        pWaveHdr->dwBufferLength = (DWORD)bufferLength * waveout_fmt.nBlockAlign;
        pWaveHdr->dwFlags = WHDR_BEGINLOOP | WHDR_ENDLOOP;
        pWaveHdr->dwLoops = 0;
        pWaveHdr->dwUser = 0;
        pWaveHdr->lpData = (LPSTR)calloc(bufferLength, waveout_fmt.nBlockAlign);
    }
    /*** Prepare wave header ***/
    for (int32 i = 0; i < waveout_buffer_count; ++i) {
        waveOutPrepareHeader(waveout_handle, &waveout_hdr[i], sizeof(WAVEHDR));
        waveOutWrite(waveout_handle, &waveout_hdr[i], sizeof(WAVEHDR));
    }
    /*** Create buffer writing proc thread ***/
    waveout_thread = CreateThread(
        NULL,
        0,
        (LPTHREAD_START_ROUTINE)waveout_buffer_writing_task,
        NULL,
        0,
        &waveout_thread_id
    );
    if (NULL == waveout_thread) {
        waveout_close();
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
void WINAPI
waveout_open(
    LPWSTR filePath,
    int32 sampleRate,
    int32 bufferLength,
    int32 bufferCount
) {
    waveout_close();
    /*** Load system value ***/
    waveout_inst_list = new InstList();
    auto load_status = waveout_inst_list->Load(filePath);
    auto caption_err = L"ウェーブテーブル読み込み失敗";
    switch (load_status) {
    case E_LOAD_STATUS::FILE_OPEN_FAILED:
        MessageBoxW(NULL, L"ファイルが開けませんでした。", caption_err, 0);
        return;
    case E_LOAD_STATUS::ALLOCATE_FAILED:
        MessageBoxW(NULL, L"メモリの確保ができませんでした。", caption_err, 0);
        return;
    case E_LOAD_STATUS::UNKNOWN_FILE:
        MessageBoxW(NULL, L"対応していない形式です。", caption_err, 0);
        return;
    default:
        break;
    }
    if (E_LOAD_STATUS::SUCCESS != load_status) {
        delete waveout_inst_list;
        return;
    }
    /*** Create system value ***/
    waveout_synth = new Synth(waveout_inst_list, sampleRate, bufferLength);
    /*** Open waveout ***/
    waveout_init(sampleRate, bufferLength, bufferCount);
}

void WINAPI
waveout_close() {
    if (NULL == waveout_handle) {
        return;
    }
    waveout_stop();
    /*** Unprepare wave header ***/
    for (int32 i = 0; i < waveout_buffer_count; ++i) {
        waveOutUnprepareHeader(waveout_handle, &waveout_hdr[i], sizeof(WAVEHDR));
    }
    waveOutReset(waveout_handle);
    waveOutClose(waveout_handle);
    waveout_handle = NULL;
    /*** Release wave header ***/
    for (int32 i = 0; i < waveout_buffer_count; ++i) {
        free(waveout_hdr[i].lpData);
    }
    free(waveout_hdr);
    waveout_hdr = NULL;
    /*** Release system value ***/
    if (NULL != waveout_synth) {
        delete waveout_synth;
        waveout_synth = NULL;
    }
    /*** Release inst list ***/
    if (NULL != waveout_inst_list) {
        delete waveout_inst_list;
        waveout_inst_list = NULL;
    }
    /*** Delete buffer locker ***/
    if (NULL != waveout_buffer_lock.DebugInfo) {
        DeleteCriticalSection(&waveout_buffer_lock);
        waveout_buffer_lock.DebugInfo = NULL;
    }
}

byte* WINAPI
ptr_inst_list() {
    if (NULL == waveout_synth) {
        return NULL;
    } else {
        return (byte*)waveout_synth->p_inst_list->GetInstList();
    }
}

byte* WINAPI
ptr_channel_params() {
    return (byte*)waveout_synth->pp_channel_params;
}

int32* WINAPI
ptr_active_counter() {
    return &waveout_synth->active_count;
}

void WINAPI
send_message(byte port, byte* pMsg) {
    waveout_synth->send_message(port, pMsg);
}
