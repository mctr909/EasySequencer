#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#include "../riff.h"

#include "wsmp.h"
#include "art.h"
#include "rgn.h"

LRGN::LRGN(FILE *fp, long size, int32 count) : RIFF() {
    Count = 0;
    pcRegion = (RGN_**)calloc(count, sizeof(RGN_*));
    Load(fp, size);
}

LRGN::~LRGN() {
    for (int32 i = 0; i < Count; i++) {
        if (nullptr != pcRegion[i]) {
            delete pcRegion[i];
        }
    }
    free(pcRegion);
    pcRegion = nullptr;
}

void
LRGN::LoadChunk(FILE *fp, const char *type, long size) {
    if (0 == strcmp("rgn ", type)) {
        pcRegion[Count++] = new RGN_(fp, size);
        return;
    }
    fseek(fp, size, SEEK_CUR);
}

RGN_::RGN_(FILE *fp, long size) : RIFF() {
    Load(fp, size);
}

RGN_::~RGN_() {
    if (nullptr != ppWaveLoop) {
        for (uint32 i = 0; i < pWaveSmpl->loopCount; ++i) {
            free(ppWaveLoop[i]);
        }
        free(ppWaveLoop);
        ppWaveLoop = nullptr;
    }
    if (nullptr != pWaveSmpl) {
        free(pWaveSmpl);
        pWaveSmpl = nullptr;
    }
    if (nullptr != cLart) {
        delete cLart;
        cLart = nullptr;
    }
}

void
RGN_::LoadChunk(FILE *fp, const char *type, long size) {
    if (0 == strcmp("rgnh", type)) {
        fread_s(&Header, sizeof(Header), size, 1, fp);
        Header.layer = 0;
        return;
    }
    if (0 == strcmp("wlnk", type)) {
        fread_s(&WaveLink, sizeof(WaveLink), size, 1, fp);
        return;
    }
    if (0 == strcmp("wsmp", type)) {
        if (nullptr != ppWaveLoop) {
            for (uint32 i = 0; i < pWaveSmpl->loopCount; ++i) {
                free(ppWaveLoop[i]);
            }
            free(ppWaveLoop);
            ppWaveLoop = nullptr;
        }
        if (nullptr != pWaveSmpl) {
            free(pWaveSmpl);
            pWaveSmpl = nullptr;
        }
        pWaveSmpl = (WSMP_VALUES*)malloc(sizeof(WSMP_VALUES));
        fread_s(pWaveSmpl, sizeof(WSMP_VALUES), sizeof(WSMP_VALUES), 1, fp);
        ppWaveLoop = (WSMP_LOOP**)calloc(pWaveSmpl->loopCount, sizeof(WSMP_LOOP*));
        for (uint32 i = 0; i < pWaveSmpl->loopCount; ++i) {
            ppWaveLoop[i] = (WSMP_LOOP*)malloc(sizeof(WSMP_LOOP));
            fread_s(ppWaveLoop[i], sizeof(WSMP_LOOP), sizeof(WSMP_LOOP), 1, fp);
        }
        return;
    }
    if (0 == strcmp("lart", type) || 0 == strcmp("lar2", type)) {
        cLart = new LART(fp, size);
        return;
    }
    fseek(fp, size, SEEK_CUR);
}
