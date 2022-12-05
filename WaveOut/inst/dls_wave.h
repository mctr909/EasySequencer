#pragma once
#include "../riff.h"
#include "../riff_struct.h"
#include "dls_struct.h"

class WAVE;
class LART;

class WVPL : public Riff {
public:
    WAVE **pcWave = NULL;
    int32 Count;

public:
    WVPL(FILE *fp, long size, int32 count);
    ~WVPL();

protected:
    void LoadList(FILE *fp, const char *type, long size) override;
};

class WAVE : public Riff {
public:
    WAVE_FMT Format;
    DLS_WSMP WaveSmpl;
    DLS_LOOP **ppWaveLoop = NULL;
    uint32 LoopCount = 0;
    byte *pData = NULL;
    uint32 DataSize = 0;
    char Name[32] = { 0 };
    char Category[32] = { 0 };

public:
    WAVE(FILE *fp, long size);
    ~WAVE();

protected:
    void LoadInfo(FILE *fp, const char *type, long size) override;
    void LoadChunk(FILE *fp, const char *type, long size) override;
};
