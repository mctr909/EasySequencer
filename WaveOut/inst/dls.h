#pragma once
#include "riff_chunk.h"
#include "dls_ins.h"
#include "dls_wave.h"

class DLS : public RiffChunk {
public:
    LINS *cLins = NULL;
    WVPL *cWvpl = NULL;
    uint32 InstCount = 0;
    uint32 WaveCount = 0;

public:
    DLS() : RiffChunk() {}
    ~DLS();

public:
    E_LOAD_STATUS Load(LPWSTR path);

protected:
    bool CheckFileType(const char *type, long size) override;
    void LoadChunk(FILE *fp, const char *type, long size) override;
    void LoadList(FILE *fp, const char *type, long size) override;
};
