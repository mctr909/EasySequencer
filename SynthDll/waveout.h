#pragma once

#include <Windows.h>

#include "type.h"

class WaveOut {
private:
    HWAVEOUT mp_handle = nullptr;
    WAVEFORMATEX m_fmt = { 0 };
    WAVEHDR* mp_hdr = nullptr;

    HANDLE m_thread = nullptr;
    DWORD m_thread_id = 0;
    CRITICAL_SECTION m_buffer_lock = { 0 };

    bool m_dostop = true;
    bool m_stopped = true;
    bool m_thread_stopped = true;

    int32 m_write_count = 0;
    int32 m_write_index = 0;
    int32 m_read_index = 0;
    int32 m_buffer_count = 0;
    int32 m_buffer_length = 0;

    void (*mfp_buffer_writer)(WAVE_DATA* p_data, void* p_param) = nullptr;
    void* mp_buffer_writer_param = nullptr;

private:
    static void callback(HWAVEOUT hwo, UINT uMsg, DWORD_PTR dwInstance, DWORD dwParam1, DWORD dwParam2);
    static DWORD buffer_writing_task(LPVOID* param);
    void stop();

public:
    void open(
        int32 sample_rate,
        int32 buffer_length,
        int32 buffer_count,
        void (*fp_buffer_writer)(WAVE_DATA* p_data, void* p_param),
        void* p_buffer_writer_param
    );
    void close();
};
