#include <stdlib.h>
#include <string.h>

#include "../riff.h"

#include "wsmp.h"
#include "wave.h"

WVPL::WVPL(FILE *fp, long size, int32 count) : RIFF() {
    m_count = 0;
    mpc_wave = (WAVE**)calloc(count, sizeof(WAVE*));
    load(fp, size);
}

WVPL::~WVPL() {
    for (int32 i = 0; i < m_count; i++) {
        if (nullptr != mpc_wave[i]) {
            delete mpc_wave[i];
        }
    }
    free(mpc_wave);
    mpc_wave = nullptr;
}

void
WVPL::load_chunk(FILE *fp, const char *type, long size) {
    if (0 == strcmp("wave", type)) {
        mpc_wave[m_count++] = new WAVE(fp, size);
        return;
    }
    fseek(fp, size, SEEK_CUR);
}

WAVE::WAVE(FILE *fp, long size) : RIFF() {
    load(fp, size);
}

WAVE::~WAVE() {
    if (nullptr != mp_data) {
        free(mp_data);
        mp_data = nullptr;
    }
    if (nullptr != mpp_loop) {
        for (uint32 i = 0; i < mp_wsmp->loop_count; ++i) {
            free(mpp_loop[i]);
        }
        free(mpp_loop);
        mpp_loop = nullptr;
    }
    if (nullptr != mp_wsmp) {
        free(mp_wsmp);
        mp_wsmp = nullptr;
    }
}

void
WAVE::load_chunk(FILE *fp, const char *type, long size) {
    if (0 == strcmp("fmt ", type)) {
        fread_s(&m_fmt, sizeof(m_fmt), sizeof(m_fmt), 1, fp);
        fseek(fp, size - sizeof(m_fmt), SEEK_CUR);
        return;
    }
    if (0 == strcmp("data", type)) {
        m_data_size = (uint32)size;
        mp_data = (byte*)malloc(size);
        fread_s(mp_data, size, size, 1, fp);
        return;
    }
    if (0 == strcmp("wsmp", type)) {
        mp_wsmp = (WSMP_VALUES*)malloc(sizeof(WSMP_VALUES));
        fread_s(mp_wsmp, sizeof(WSMP_VALUES), sizeof(WSMP_VALUES), 1, fp);
        mpp_loop = (WSMP_LOOP**)calloc(mp_wsmp->loop_count, sizeof(WSMP_LOOP*));
        for (uint32 i = 0; i < mp_wsmp->loop_count; ++i) {
            mpp_loop[i] = (WSMP_LOOP*)malloc(sizeof(WSMP_LOOP));
            fread_s(mpp_loop[i], sizeof(WSMP_LOOP), sizeof(WSMP_LOOP), 1, fp);
        }
        return;
    }
    fseek(fp, size, SEEK_CUR);
}
