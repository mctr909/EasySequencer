#include "dls_rgn.h"
#include "dls_art.h"
#include <string.h>

LRGN::LRGN(FILE *fp, long size, int32 count) : RiffChunk() {
    Count = 0;
    pcRegion = (RGN_**)calloc(count, sizeof(RGN_*));
    Load(fp, size);
}

LRGN::~LRGN() {
    for (int32 i = 0; i < Count; i++) {
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
    if (NULL != ppWaveLoop) {
        for (uint32 i = 0; i < pWaveSmpl->loopCount; ++i) {
            free(ppWaveLoop[i]);
        }
        free(ppWaveLoop);
        ppWaveLoop = NULL;
    }
    if (NULL != pWaveSmpl) {
        free(pWaveSmpl);
        pWaveSmpl = NULL;
    }
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
        if (NULL != pWaveSmpl) {
            free(pWaveSmpl);
            pWaveSmpl = NULL;
        }
        if (NULL != ppWaveLoop) {
            for (uint32 i = 0; i < pWaveSmpl->loopCount; ++i) {
                free(ppWaveLoop[i]);
            }
            free(ppWaveLoop);
            ppWaveLoop = NULL;
        }
        pWaveSmpl = (DLS_WSMP*)malloc(sizeof(DLS_WSMP));
        fread_s(pWaveSmpl, sizeof(DLS_WSMP), sizeof(DLS_WSMP), 1, fp);
        ppWaveLoop = (DLS_LOOP**)calloc(pWaveSmpl->loopCount, sizeof(DLS_LOOP*));
        for (uint32 i = 0; i < pWaveSmpl->loopCount; ++i) {
            ppWaveLoop[i] = (DLS_LOOP*)malloc(sizeof(DLS_LOOP));
            fread_s(ppWaveLoop[i], sizeof(DLS_LOOP), sizeof(DLS_LOOP), 1, fp);
        }
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
