#pragma once
#include "dls_struct.h"
#include "riff_chunk.h"

class WAVE;
class LART;

class WVPL : public RiffChunk {
public:
    WAVE **pcWave = NULL;
    int Count;

public:
    WVPL(FILE *fp, long size, int count);
    ~WVPL();

protected:
    void LoadList(FILE *fp, const char *type, long size) override;
};

class WAVE : public RiffChunk {
public:
    WAVE_FMT Format;
    DLS_WSMP WaveSmpl;
    DLS_LOOP **ppWaveLoop = NULL;
    unsigned int LoopCount = 0;
    unsigned char *pData = NULL;
    unsigned int DataSize = 0;
    char Name[32] = { 0 };
    char Category[32] = { 0 };

public:
    WAVE(FILE *fp, long size);
    ~WAVE();

protected:
    void LoadInfo(FILE *fp, const char *type, long size) override;
    void LoadChunk(FILE *fp, const char *type, long size) override;
};
