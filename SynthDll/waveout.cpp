#include "waveout.h"

#include <mmsystem.h>
#pragma comment (lib, "winmm.lib")

void
WaveOut::callback(HWAVEOUT hwo, UINT uMsg, DWORD_PTR dwInstance, DWORD dwParam1, DWORD dwParam2) {
    auto p_hdr = (WAVEHDR*)dwInstance;
    auto p_this = (WaveOut*)p_hdr->dwUser;
    switch (uMsg) {
    case MM_WOM_OPEN:
        p_this->m_callback_stopped = false;
        break;
    case MM_WOM_CLOSE:
        break;
    case MM_WOM_DONE:
        if (p_this->m_stop) {
            p_this->m_callback_stopped = true;
            break;
        }
        EnterCriticalSection((LPCRITICAL_SECTION)&p_this->m_buffer_lock);
        waveOutWrite(p_this->mp_handle, p_hdr + p_this->m_read_index, sizeof(WAVEHDR));
        if (p_this->m_write_count > 0) {
            p_this->m_read_index = ++p_this->m_read_index % p_this->m_buffer_count;
            p_this->m_write_count--;
        }
        LeaveCriticalSection((LPCRITICAL_SECTION)&p_this->m_buffer_lock);
        break;
    default:
        break;
    }
}

DWORD
WaveOut::buffer_writing_task(LPVOID* param) {
    auto p_hdr = (WAVEHDR*)param;
    auto p_this = (WaveOut*)p_hdr->dwUser;
    p_this->m_thread_stopped = false;
    int32 write_index = 0;
    while (true) {
        if (p_this->m_stop) {
            p_this->m_thread_stopped = true;
            break;
        }
        bool enable_sleep;
        EnterCriticalSection((LPCRITICAL_SECTION)&p_this->m_buffer_lock);
        if (p_this->m_buffer_count <= p_this->m_write_count + 1) {
            /*** Buffer full ***/
            enable_sleep = true;
        } else {
            /*** Write Buffer ***/
            enable_sleep = false;
            auto p_buff = (WAVE_DATA*)(p_hdr + write_index)->lpData;
            memset(p_buff, 0, p_this->m_fmt.nBlockAlign * p_this->m_buffer_length);
            p_this->mfp_buffer_writer(p_buff, p_this->mp_buffer_writer_param);
            write_index = ++write_index % p_this->m_buffer_count;
            p_this->m_write_count++;
        }
        LeaveCriticalSection((LPCRITICAL_SECTION)&p_this->m_buffer_lock);
        if (enable_sleep) {
            Sleep(1);
        }
    }
    return 0;
}

void
WaveOut::stop() {
    m_stop = true;
    while (!m_callback_stopped || !m_thread_stopped) {
        Sleep(100);
    }
}

void
WaveOut::open(
    int32 sample_rate,
    int32 buffer_length,
    int32 buffer_count,
    void (*fp_buffer_writer)(WAVE_DATA* p_data, void* p_param),
    void* p_buffer_writer_param
) {
    if (nullptr != mp_handle) {
        close();
    }
    /*** Init buffer counter/writer ***/
    m_stop = false;
    m_write_count = 0;
    m_read_index = 0;
    m_buffer_count = buffer_count;
    m_buffer_length = buffer_length;
    mfp_buffer_writer = fp_buffer_writer;
    mp_buffer_writer_param = p_buffer_writer_param;
    /*** Set wave fmt ***/
    m_fmt.wFormatTag = 1;
    m_fmt.nChannels = 2;
    m_fmt.wBitsPerSample = (WORD)(sizeof(WAVE_DATA) << 3);
    m_fmt.nSamplesPerSec = (DWORD)sample_rate;
    m_fmt.nBlockAlign = m_fmt.nChannels * m_fmt.wBitsPerSample / 8;
    m_fmt.nAvgBytesPerSec = m_fmt.nSamplesPerSec * m_fmt.nBlockAlign;
    /*** Allocate wave header ***/
    mp_hdr = (WAVEHDR*)calloc(m_buffer_count, sizeof(WAVEHDR));
    if (nullptr == mp_hdr) {
        return;
    }
    for (int32 i = 0; i < m_buffer_count; ++i) {
        auto pWaveHdr = mp_hdr + i;
        pWaveHdr->dwBufferLength = (DWORD)buffer_length * m_fmt.nBlockAlign;
        pWaveHdr->dwFlags = WHDR_BEGINLOOP | WHDR_ENDLOOP;
        pWaveHdr->dwLoops = 0;
        pWaveHdr->dwUser = (DWORD_PTR)this;
        pWaveHdr->lpData = (LPSTR)calloc(buffer_length, m_fmt.nBlockAlign);
    }
    /*** Init buffer locker ***/
    InitializeCriticalSection(&m_buffer_lock);
    /*** Open waveout ***/
    if (MMSYSERR_NOERROR != waveOutOpen(
        &mp_handle,
        WAVE_MAPPER,
        &m_fmt,
        (DWORD_PTR)callback,
        (DWORD_PTR)mp_hdr,
        CALLBACK_FUNCTION
    )) {
        close();
        return;
    }
    /*** Prepare wave header ***/
    for (int32 i = 0; i < m_buffer_count; ++i) {
        waveOutPrepareHeader(mp_handle, mp_hdr + i, sizeof(WAVEHDR));
        waveOutWrite(mp_handle, mp_hdr + i, sizeof(WAVEHDR));
    }
    /*** Create buffer writing proc thread ***/
    m_thread = CreateThread(
        nullptr,
        0,
        (LPTHREAD_START_ROUTINE)buffer_writing_task,
        mp_hdr,
        0,
        &m_thread_id
    );
    if (nullptr == m_thread) {
        close();
        return;
    }
    SetThreadPriority(m_thread, THREAD_PRIORITY_HIGHEST);
}

void
WaveOut::close() {
    if (nullptr == mp_handle) {
        return;
    }
    stop();
    /*** Unprepare wave header ***/
    for (int32 i = 0; i < m_buffer_count; ++i) {
        waveOutUnprepareHeader(mp_handle, mp_hdr + i, sizeof(WAVEHDR));
    }
    waveOutReset(mp_handle);
    waveOutClose(mp_handle);
    mp_handle = nullptr;
    /*** Release wave header ***/
    for (int32 i = 0; i < m_buffer_count; ++i) {
        free(mp_hdr[i].lpData);
    }
    free(mp_hdr);
    mp_hdr = nullptr;
    /*** Delete buffer locker ***/
    if (nullptr != m_buffer_lock.DebugInfo) {
        DeleteCriticalSection(&m_buffer_lock);
        m_buffer_lock.DebugInfo = nullptr;
    }
}