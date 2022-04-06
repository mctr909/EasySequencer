#include "dls_wave.h"
#include <string.h>

WVPL::WVPL(FILE *fp, long size, int count) : RiffChunk() {
    Count = 0;
    pcWave = (WAVE**)malloc(sizeof(WAVE*) * count);
    memset(pcWave, 0, sizeof(WAVE*) * count);
    Load(fp, size);
}

WVPL::~WVPL() {
    for (int i = 0; i < Count; i++) {
        if (NULL != pcWave[i]) {
            delete pcWave[i];
        }
    }
    free(pcWave);
    pcWave = NULL;
}

void WVPL::LoadList(FILE *fp, const char *type, long size) {
    if (0 == strcmp("wave", type)) {
        pcWave[Count++] = new WAVE(fp, size);
        return;
    }
    fseek(fp, size, SEEK_CUR);
}

WAVE::WAVE(FILE *fp, long size) : RiffChunk() {
    Load(fp, size);
}

WAVE::~WAVE() {
    if (NULL != pData) {
        free(pData);
        pData = NULL;
    }
    if (NULL != ppWaveLoop) {
        for (unsigned int i = 0; i < WaveSmpl.loopCount; ++i) {
            free(ppWaveLoop[i]);
        }
        free(ppWaveLoop);
        ppWaveLoop = NULL;
    }
}

void WAVE::LoadInfo(FILE *fp, const char *type, long size) {
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

void WAVE::LoadChunk(FILE *fp, const char *type, long size) {
    if (0 == strcmp("fmt ", type)) {
        fread_s(&Format, sizeof(Format), size, 1, fp);
        return;
    }
    if (0 == strcmp("data", type)) {
        DataSize = (unsigned int)size;
        pData = (byte*)malloc(size);
        fread_s(pData, size, size, 1, fp);
        return;
    }
    if (0 == strcmp("wsmp", type)) {
        fread_s(&WaveSmpl, sizeof(WaveSmpl), sizeof(WaveSmpl), 1, fp);
        LoopCount = WaveSmpl.loopCount;
        ppWaveLoop = (DLS_LOOP**)malloc(sizeof(DLS_LOOP*) * LoopCount);
        for (unsigned int i = 0; i < LoopCount; ++i) {
            ppWaveLoop[i] = (DLS_LOOP*)malloc(sizeof(DLS_LOOP));
            fread_s(ppWaveLoop[i], sizeof(DLS_LOOP), sizeof(DLS_LOOP), 1, fp);
        }
        return;
    }
    fseek(fp, size, SEEK_CUR);
}