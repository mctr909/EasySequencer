#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#include "rgn.h"
#include "art.h"
#include "ins.h"

LINS::LINS(FILE *fp, long size, int32 count) : RIFF() {
    Count = 0;
    pcInst = (INS**)calloc(count, sizeof(INS*));
    Load(fp, size);
}

LINS::~LINS() {
    for (int32 i = 0; i < Count; i++) {
        if (nullptr != pcInst[i]) {
            delete pcInst[i];
        }
    }
    free(pcInst);
    pcInst = nullptr;
}

void
LINS::LoadChunk(FILE *fp, const char *type, long size) {
    if (0 == strcmp("ins ", type)) {
        pcInst[Count++] = new INS(fp, size);
        return;
    }
    fseek(fp, size, SEEK_CUR);
}

INS::INS(FILE *fp, long size) : RIFF() {
    Load(fp, size);
}

INS::~INS() {
    if (nullptr != cLrgn) {
        delete cLrgn;
        cLrgn = nullptr;
    }
    if (nullptr != cLart) {
        delete cLart;
        cLart = nullptr;
    }
}

void
INS::LoadChunk(FILE *fp, const char *type, long size) {
    if (0 == strcmp("insh", type)) {
        fread_s(&Header, sizeof(Header), size, 1, fp);
        return;
    }
    if (0 == strcmp("lrgn", type)) {
        cLrgn = new LRGN(fp, size, Header.regions);
        return;
    }
    if (0 == strcmp("lart", type) || 0 == strcmp("lar2", type)) {
        cLart = new LART(fp, size);
        return;
    }
    fseek(fp, size, SEEK_CUR);
}
