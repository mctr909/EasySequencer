#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#include "riff.h"

#include "dls_rgn.h"
#include "dls_art.h"

#include "dls_ins.h"

LINS::LINS(FILE *fp, long size, int32 count) : Riff() {
    Count = 0;
    pcInst = (INS_**)calloc(count, sizeof(INS_*));
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
        pcInst[Count++] = new INS_(fp, size);
        return;
    }
    fseek(fp, size, SEEK_CUR);
}

INS_::INS_(FILE *fp, long size) : Riff() {
    Load(fp, size);
}

INS_::~INS_() {
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
INS_::LoadInfo(FILE *fp, const char *type, long size) {
    if (0 == strcmp("INAM", type)) {
        fread_s(&Name, sizeof(Name), size, 1, fp);
        return;
    }
    if (0 == strcmp("ICAT", type)) {
        fread_s(&Category, sizeof(Category), size, 1, fp);
        return;
    }
    fseek(fp, size, SEEK_CUR);
}

void
INS_::LoadChunk(FILE *fp, const char *type, long size) {
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
