#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#include "../riff.h"

#include "wsmp.h"
#include "art.h"
#include "rgn.h"

LRGN::LRGN(FILE *fp, long size, int32 count) : RIFF() {
    m_count = 0;
    mpc_rgn = (RGN**)calloc(count, sizeof(RGN*));
    load(fp, size);
}

LRGN::~LRGN() {
    for (int32 i = 0; i < m_count; i++) {
        if (nullptr != mpc_rgn[i]) {
            delete mpc_rgn[i];
        }
    }
    free(mpc_rgn);
    mpc_rgn = nullptr;
}

void
LRGN::load_chunk(FILE *fp, const char *type, long size) {
    if (0 == strcmp("rgn ", type)) {
        mpc_rgn[m_count++] = new RGN(fp, size);
        return;
    }
    fseek(fp, size, SEEK_CUR);
}

RGN::RGN(FILE *fp, long size) : RIFF() {
    load(fp, size);
}

RGN::~RGN() {
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
    if (nullptr != mc_lart) {
        delete mc_lart;
        mc_lart = nullptr;
    }
}

void
RGN::load_chunk(FILE *fp, const char *type, long size) {
    if (0 == strcmp("rgnh", type)) {
        fread_s(&m_rgnh, sizeof(m_rgnh), size, 1, fp);
        m_rgnh.layer = 0;
        return;
    }
    if (0 == strcmp("wlnk", type)) {
        fread_s(&m_wlnk, sizeof(m_wlnk), size, 1, fp);
        return;
    }
    if (0 == strcmp("wsmp", type)) {
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
        mp_wsmp = (WSMP_VALUES*)malloc(sizeof(WSMP_VALUES));
        fread_s(mp_wsmp, sizeof(WSMP_VALUES), sizeof(WSMP_VALUES), 1, fp);
        mpp_loop = (WSMP_LOOP**)calloc(mp_wsmp->loop_count, sizeof(WSMP_LOOP*));
        for (uint32 i = 0; i < mp_wsmp->loop_count; ++i) {
            mpp_loop[i] = (WSMP_LOOP*)malloc(sizeof(WSMP_LOOP));
            fread_s(mpp_loop[i], sizeof(WSMP_LOOP), sizeof(WSMP_LOOP), 1, fp);
        }
        return;
    }
    if (0 == strcmp("lart", type) || 0 == strcmp("lar2", type)) {
        mc_lart = new LART(fp, size);
        return;
    }
    fseek(fp, size, SEEK_CUR);
}
