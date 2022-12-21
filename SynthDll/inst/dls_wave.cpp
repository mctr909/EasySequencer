#include <string.h>
#include "../riff.h"
#include "../riff_struct.h"

#include "dls_struct.h"

#include "dls_wave.h"

WVPL::WVPL(FILE *fp, long size, int32 count) : Riff() {
    Count = 0;
    pcWave = (WAVE**)calloc(count, sizeof(WAVE*));
    Load(fp, size);
}

WVPL::~WVPL() {
    for (int32 i = 0; i < Count; i++) {
        if (nullptr != pcWave[i]) {
            delete pcWave[i];
        }
    }
    free(pcWave);
    pcWave = nullptr;
}

void
WVPL::LoadChunk(FILE *fp, const char *type, long size) {
    if (0 == strcmp("wave", type)) {
        pcWave[Count++] = new WAVE(fp, size);
        return;
    }
    fseek(fp, size, SEEK_CUR);
}

WAVE::WAVE(FILE *fp, long size) : Riff() {
    Load(fp, size);
}

WAVE::~WAVE() {
    if (nullptr != pData) {
        free(pData);
        pData = nullptr;
    }
    if (nullptr != ppWaveLoop) {
        for (uint32 i = 0; i < WaveSmpl.loopCount; ++i) {
            free(ppWaveLoop[i]);
        }
        free(ppWaveLoop);
        ppWaveLoop = nullptr;
    }
}

void
WAVE::LoadInfo(FILE *fp, const char *type, long size) {
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
WAVE::LoadChunk(FILE *fp, const char *type, long size) {
    if (0 == strcmp("fmt ", type)) {
        fread_s(&Format, sizeof(Format), sizeof(Format), 1, fp);
        fseek(fp, size - sizeof(Format), SEEK_CUR);
        return;
    }
    if (0 == strcmp("data", type)) {
        DataSize = (uint32)size;
        pData = (byte*)malloc(size);
        fread_s(pData, size, size, 1, fp);
        return;
    }
    if (0 == strcmp("wsmp", type)) {
        fread_s(&WaveSmpl, sizeof(WaveSmpl), sizeof(WaveSmpl), 1, fp);
        ppWaveLoop = (DLS_LOOP**)calloc(WaveSmpl.loopCount, sizeof(DLS_LOOP*));
        for (uint32 i = 0; i < WaveSmpl.loopCount; ++i) {
            ppWaveLoop[i] = (DLS_LOOP*)malloc(sizeof(DLS_LOOP));
            fread_s(ppWaveLoop[i], sizeof(DLS_LOOP), sizeof(DLS_LOOP), 1, fp);
        }
        return;
    }
    fseek(fp, size, SEEK_CUR);
}