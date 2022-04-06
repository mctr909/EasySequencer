#include "dls_rgn.h"
#include "dls_art.h"
#include <string.h>

LRGN::LRGN(FILE *fp, long size, int count) : RiffChunk() {
    Count = 0;
    pcRegion = (RGN_**)malloc(sizeof(RGN_*) * count);
    memset(pcRegion, 0, sizeof(RGN_*) * count);
    Load(fp, size);
}

LRGN::~LRGN() {
    for (int i = 0; i < Count; i++) {
        if (NULL != pcRegion[i]) {
            delete pcRegion[i];
        }
    }
    free(pcRegion);
    pcRegion = NULL;
}

void LRGN::LoadList(FILE *fp, const char *type, long size) {
    if (0 == strcmp("rgn ", type)) {
        pcRegion[Count++] = new RGN_(fp, size);
        return;
    }
    fseek(fp, size, SEEK_CUR);
}

RGN_::RGN_(FILE *fp, long size) : RiffChunk() {
    Load(fp, size);
}

RGN_::~RGN_() {
    if (NULL != cLart) {
        delete cLart;
        cLart = NULL;
    }
}

void RGN_::LoadChunk(FILE *fp, const char *type, long size) {
    if (0 == strcmp("rgnh", type)) {
        fread_s(&Header, sizeof(Header), size, 1, fp);
        return;
    }
    if (0 == strcmp("wlnk", type)) {
        fread_s(&WaveLink, sizeof(WaveLink), size, 1, fp);
        return;
    }
    if (0 == strcmp("wsmp", type)) {
        fread_s(&WaveSmpl, sizeof(WaveSmpl), sizeof(WaveSmpl), 1, fp);
        fseek(fp, size - sizeof(WaveSmpl), SEEK_CUR);
        return;
    }
    fseek(fp, size, SEEK_CUR);
}

void RGN_::LoadList(FILE *fp, const char *type, long size) {
    if (0 == strcmp("lart", type) || 0 == strcmp("lar2", type)) {
        cLart = new LART(fp, size);
        return;
    }
    fseek(fp, size, SEEK_CUR);
}
