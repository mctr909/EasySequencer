#include <stdio.h>
#include <stdlib.h>
#include <windows.h>

#include "../inst/inst_list.h"

#include "channel.h"
#include "sampler.h"
#include "synth.h"

void
Synth::dispose() {
    /* dispose channels */
    if (nullptr != mpp_channels) {
        for (int32 i = 0; i < CHANNEL_COUNT; i++) {
            delete mpp_channels[i];
        }
        free(mpp_channels);
        mpp_channels = nullptr;
    }
    /* dispose channel params */
    if (nullptr != m_export_values.pp_channel_params) {
        free(m_export_values.pp_channel_params);
        m_export_values.pp_channel_params = nullptr;
    }
    /* dispose samplers */
    if (nullptr != mpp_samplers) {
        for (uint32 i = 0; i < SAMPLER_COUNT; i++) {
            delete mpp_samplers[i];
        }
        free(mpp_samplers);
        mpp_samplers = nullptr;
    }
    /* dispose output buffer */
    if (nullptr != mp_buffer_l) {
        free(mp_buffer_l);
        mp_buffer_l = nullptr;
    }
    if (nullptr != mp_buffer_r) {
        free(mp_buffer_r);
        mp_buffer_r = nullptr;
    }
    /*** dispose inst list ***/
    if (nullptr != mp_inst_list) {
        delete mp_inst_list;
        mp_inst_list = nullptr;
    }
}

E_LOAD_STATUS
Synth::setup(STRING wave_table_path, int32 sample_rate, int32 buffer_length) {
    m_sample_rate = sample_rate;
    m_delta_time = 1.0 / sample_rate;
    m_buffer_length = buffer_length;
    /* Create inst list */
    mp_inst_list = new InstList();
    auto load_status = mp_inst_list->Load(wave_table_path);
    if (E_LOAD_STATUS::SUCCESS != load_status) {
        dispose();
        return load_status;
    }
    mp_wave_table = mp_inst_list->mpWaveTable;
    /* allocate output buffer */
    mp_buffer_l = (double*)calloc(buffer_length, sizeof(double));
    mp_buffer_r = (double*)calloc(buffer_length, sizeof(double));
    if (nullptr == mp_buffer_l || nullptr == mp_buffer_r) {
        dispose();
        return E_LOAD_STATUS::ALLOCATE_FAILED;
    }
    /* allocate samplers */
    mpp_samplers = (Sampler**)malloc(sizeof(Sampler*) * SAMPLER_COUNT);
    if (nullptr == mpp_samplers) {
        dispose();
        return E_LOAD_STATUS::ALLOCATE_FAILED;
    }
    for (uint32 i = 0; i < SAMPLER_COUNT; i++) {
        mpp_samplers[i] = new Sampler(this);
    }
    /* allocate channel params */
    m_export_values.pp_channel_params = (CHANNEL_PARAM**)malloc(sizeof(CHANNEL_PARAM*) * CHANNEL_COUNT);
    if (nullptr == m_export_values.pp_channel_params) {
        dispose();
        return E_LOAD_STATUS::ALLOCATE_FAILED;
    }
    /* allocate channels */
    mpp_channels = (Channel**)malloc(sizeof(Channel*) * CHANNEL_COUNT);
    if (nullptr == mpp_channels) {
        dispose();
        return E_LOAD_STATUS::ALLOCATE_FAILED;
    }
    for (int32 i = 0; i < CHANNEL_COUNT; i++) {
        mpp_channels[i] = new Channel(this, i);
        m_export_values.pp_channel_params[i] = &mpp_channels[i]->m_param;
    }
    /* set system values */
    auto inst_list = mp_inst_list->GetInstList();
    m_export_values.inst_count = inst_list->count;
    m_export_values.pp_inst_list = inst_list->ppData;
    m_export_values.p_active_counter = &m_active_count;
    return load_status;
}

void
Synth::write_buffer(WAVE_DATA* p_pcm, void* p_param) {
    auto p_this = (Synth*)p_param;
    /* sampler loop */
    int32 active_count = 0;
    for (int32 i = 0; i < SAMPLER_COUNT; i++) {
        auto p_smpl = p_this->mpp_samplers[i];
        if (p_smpl->m_state <= Sampler::E_STATE::RESERVED) {
            continue;
        }
        p_smpl->step();
        active_count++;
    }
    p_this->m_active_count = active_count;
    /* channel loop */
    for (int32 i = 0; i < CHANNEL_COUNT; i++) {
        auto p_ch = p_this->mpp_channels[i];
        if (Channel::E_STATE::FREE == p_ch->m_state) {
            continue;
        }
        p_ch->step(p_this->mp_buffer_l, p_this->mp_buffer_r);
    }
    /* write buffer */
    for (int32 i = 0, j = 0; i < p_this->m_buffer_length; i++, j += 2) {
        auto p_l = p_this->mp_buffer_l + i;
        auto p_r = p_this->mp_buffer_r + i;
        if (*p_l < -1.0) {
            *p_l = -1.0;
        }
        if (1.0 < *p_l) {
            *p_l = 1.0;
        }
        if (*p_r < -1.0) {
            *p_r = -1.0;
        }
        if (1.0 < *p_r) {
            *p_r = 1.0;
        }
        p_pcm[j] = (WAVE_DATA)(*p_l * WAVE_MAX);
        p_pcm[j + 1] = (WAVE_DATA)(*p_r * WAVE_MAX);
        *p_l = 0.0;
        *p_r = 0.0;
    }
}

bool
Synth::save_wav(STRING save_path, uint32 base_tick, uint32 event_size, byte* p_events, int32* p_progress) {
    const byte RIFF_ID[] = { 'R', 'I', 'F', 'F' };
    const byte FILE_ID[] = { 'W', 'A', 'V', 'E' };
    const byte FMT_ID[] = { 'f', 'm', 't', ' ' };
    const byte DATA_ID[] = { 'd', 'a', 't', 'a' };
    /* set riff wave format */
    const uint32 FMT_SIZE = sizeof(WAVEFORMATEX);
    WAVEFORMATEX fmt = { 0 };
    fmt.wFormatTag = 1;
    fmt.nChannels = 2;
    fmt.nSamplesPerSec = m_sample_rate;
    fmt.wBitsPerSample = (uint16)(sizeof(WAVE_DATA) << 3);
    fmt.nBlockAlign = fmt.nChannels * fmt.wBitsPerSample >> 3;
    fmt.nAvgBytesPerSec = fmt.nSamplesPerSec * fmt.nBlockAlign;
    /* allocate output buffer */
    const int32 BUFFER_SIZE = m_buffer_length * fmt.nBlockAlign;
    auto p_buffer = (WAVE_DATA*)calloc(m_buffer_length, fmt.nBlockAlign);
    if (nullptr == p_buffer) {
        return false;
    }
    /* open file */
    uint32 file_size = 0;
    uint32 data_size = 0;
    FILE* fp_out = nullptr;
    _wfopen_s(&fp_out, save_path, L"wb");
    if (nullptr == fp_out) {
        free(p_buffer);
        return false;
    }
    /* write header */
    fwrite(&RIFF_ID, sizeof(RIFF_ID), 1, fp_out);
    fwrite(&file_size, sizeof(file_size), 1, fp_out);
    fwrite(&FILE_ID, sizeof(FILE_ID), 1, fp_out);
    fwrite(&FMT_ID, sizeof(FMT_ID), 1, fp_out);
    fwrite(&FMT_SIZE, sizeof(FMT_SIZE), 1, fp_out);
    fwrite(&fmt, sizeof(fmt), 1, fp_out);
    fwrite(&DATA_ID, sizeof(DATA_ID), 1, fp_out);
    fwrite(&data_size, sizeof(data_size), 1, fp_out);
    //********************************
    // output wave
    //********************************
    const double DELTA_MINUTES = m_buffer_length * m_delta_time / 60.0;
    uint32 event_pos = 0;
    double tick = 0.0;
    while (event_pos < event_size) {
        auto ev_tick = *(int32*)(p_events + event_pos) / (double)base_tick;
        event_pos += 4;
        auto ev_value = p_events + event_pos;
        while (tick < ev_tick) {
            Synth::write_buffer(p_buffer, this);
            fwrite(p_buffer, BUFFER_SIZE, 1, fp_out);
            data_size += BUFFER_SIZE;
            tick += m_bpm * DELTA_MINUTES;
            *p_progress = event_pos;
        }
        event_pos += send_message(0, ev_value);
    }
    *p_progress = event_size;
    /* close file */
    file_size = data_size + sizeof(fmt) + 4;
    fseek(fp_out, 0, SEEK_SET);
    fwrite(&RIFF_ID, sizeof(RIFF_ID), 1, fp_out);
    fwrite(&file_size, sizeof(file_size), 1, fp_out);
    fwrite(&FILE_ID, sizeof(FILE_ID), 1, fp_out);
    fwrite(&FMT_ID, sizeof(FMT_ID), 1, fp_out);
    fwrite(&FMT_SIZE, sizeof(FMT_SIZE), 1, fp_out);
    fwrite(&fmt, sizeof(fmt), 1, fp_out);
    fwrite(&DATA_ID, sizeof(DATA_ID), 1, fp_out);
    fwrite(&data_size, sizeof(data_size), 1, fp_out);
    fclose(fp_out);
    /* dispose output buffer */
    free(p_buffer);
    return true;
}

int32
Synth::send_message(byte port, byte* p_msg) {
    auto type = (E_EVENT_TYPE)(*p_msg & 0xF0);
    auto ch = (port << 4) | (*p_msg & 0x0F);
    switch (type) {
    case E_EVENT_TYPE::NOTE_OFF:
        mpp_channels[ch]->note_off(p_msg[1]);
        return 3;
    case E_EVENT_TYPE::NOTE_ON:
        mpp_channels[ch]->note_on(p_msg[1], p_msg[2]);
        return 3;
    case E_EVENT_TYPE::POLY_KEY:
        return 3;
    case E_EVENT_TYPE::CTRL_CHG:
        mpp_channels[ch]->ctrl_change(p_msg[1], p_msg[2]);
        return 3;
    case E_EVENT_TYPE::PROG_CHG:
        mpp_channels[ch]->program_change(p_msg[1]);
        return 2;
    case E_EVENT_TYPE::CH_PRESS:
        return 2;
    case E_EVENT_TYPE::PITCH:
        mpp_channels[ch]->pitch_bend(p_msg[1], p_msg[2]);
        return 3;
    case E_EVENT_TYPE::SYS_EX:
        switch (p_msg[0]) {
        case 0xF0:
            return sys_ex(p_msg);
        case 0xFF:
            return meta_data(p_msg);
        default:
            return 0;
        }
    default:
        return 0;
    }
}

int32
Synth::sys_ex(byte* p_data) {
    for (int32 i = 0; i < 1024; i++) {
        if (0xF7 == p_data[i]) {
            if (5 <= i && 0x7E == p_data[2] && 0x7F == p_data[3] && 0x09 == p_data[4] && 0x01 == p_data[5]) {
                /* GM reset */
                for (int32 i = 0; i < CHANNEL_COUNT; i++) {
                    mpp_channels[i]->init_ctrl();
                }
            }
            if (5 <= i && 0x7E == p_data[2] && 0x7F == p_data[3] && 0x09 == p_data[4] && 0x03 == p_data[5]) {
                /* GM2 reset */
                for (int32 i = 0; i < CHANNEL_COUNT; i++) {
                    mpp_channels[i]->init_ctrl();
                }
            }
            if (10 <= i && 0x41 == p_data[2] && 0x42 == p_data[4] && 0x12 == p_data[5] && 0x40 == p_data[6] &&
                0x00 == p_data[7] && 0x7F == p_data[8] && 0x00 == p_data[9] && 0x41 == p_data[10]) {
                /* GS reset */
                for (int32 i = 0; i < CHANNEL_COUNT; i++) {
                    mpp_channels[i]->init_ctrl();
                }
            }
            if (8 <= i && 0x43 == p_data[2] && 0x4C == p_data[4] && 0x00 == p_data[5] && 0x00 == p_data[6] &&
                0x7E == p_data[7] && 0x00 == p_data[8]) {
                /* XG reset */
                for (int32 i = 0; i < CHANNEL_COUNT; i++) {
                    mpp_channels[i]->init_ctrl();
                }
            }
            return i;
        }
    }
    return 1;
}

int32
Synth::meta_data(byte* p_data) {
    auto type = (E_META_TYPE)p_data[1];
    auto size = p_data[2];

    switch (type) {
    case E_META_TYPE::TEMPO:
        m_bpm = 60000000.0 / ((p_data[3] << 16) | (p_data[4] << 8) | p_data[5]);
        break;
    case E_META_TYPE::KEY:
        break;
    case E_META_TYPE::MEASURE:
        break;
    default:
        break;
    }

    return size + 3;
}