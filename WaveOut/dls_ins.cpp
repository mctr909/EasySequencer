#include "dls_ins.h"
#include "dls_rgn.h"
#include "dls_art.h"
#include <string.h>

LINS::LINS(FILE *fp, long size, int count) : RiffChunk() {
    Count = 0;
    pcInst = (INS_**)malloc(sizeof(INS_*) * count);
    memset(pcInst, 0, sizeof(INS_*) * count);
    Load(fp, size);
}

LINS::~LINS() {
    for (int i = 0; i < Count; i++) {
        if (NULL != pcInst[i]) {
            delete pcInst[i];
        }
    }
    free(pcInst);
    pcInst = NULL;
}

void LINS::LoadList(FILE *fp, const char *type, long size) {
    if (0 == strcmp("ins ", type)) {
        pcInst[Count++] = new INS_(fp, size);
        return;
    }
    fseek(fp, size, SEEK_CUR);
}

INS_::INS_(FILE *fp, long size) : RiffChunk() {
    Load(fp, size);
}

INS_::~INS_() {
    if (NULL != cLrgn) {
        delete cLrgn;
        cLrgn = NULL;
    }
    if (NULL != cLart) {
        delete cLart;
        cLart = NULL;
    }
}

void INS_::LoadInfo(FILE *fp, const char *type, long size) {
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

void INS_::LoadChunk(FILE *fp, const char *type, long size) {
    if (0 == strcmp("insh", type)) {
        fread_s(&Header, sizeof(Header), size, 1, fp);
        return;
    }
    fseek(fp, size, SEEK_CUR);
}

void INS_::LoadList(FILE *fp, const char *type, long size) {
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