#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#include "rgn.h"
#include "art.h"
#include "ins.h"

LINS::LINS(FILE *fp, long size, int32 count) : RIFF() {
    m_count = 0;
    mpc_ins = (INS**)calloc(count, sizeof(INS*));
    load(fp, size);
}

LINS::~LINS() {
    for (int32 i = 0; i < m_count; i++) {
        if (nullptr != mpc_ins[i]) {
            delete mpc_ins[i];
        }
    }
    free(mpc_ins);
    mpc_ins = nullptr;
}

void
LINS::load_chunk(FILE *fp, const char *type, long size) {
    if (0 == strcmp("ins ", type)) {
        mpc_ins[m_count++] = new INS(fp, size);
        return;
    }
    fseek(fp, size, SEEK_CUR);
}

INS::INS(FILE *fp, long size) : RIFF() {
    load(fp, size);
}

INS::~INS() {
    if (nullptr != mc_lrgn) {
        delete mc_lrgn;
        mc_lrgn = nullptr;
    }
    if (nullptr != mc_lart) {
        delete mc_lart;
        mc_lart = nullptr;
    }
}

void
INS::load_chunk(FILE *fp, const char *type, long size) {
    if (0 == strcmp("insh", type)) {
        fread_s(&m_insh, sizeof(m_insh), size, 1, fp);
        return;
    }
    if (0 == strcmp("lrgn", type)) {
        mc_lrgn = new LRGN(fp, size, m_insh.regions);
        return;
    }
    if (0 == strcmp("lart", type) || 0 == strcmp("lar2", type)) {
        mc_lart = new LART(fp, size);
        return;
    }
    fseek(fp, size, SEEK_CUR);
}
