#pragma once
#include "riff_chunk.h"
#include "dls_ins.h"
#include "dls_wave.h"

class DLS : public RiffChunk {
public:
    LINS *cLins = NULL;
    WVPL *cWvpl = NULL;
    int InstCount;
    int WaveCount;

private:
    DLS() {}

public:
    DLS(LPWSTR path);
    ~DLS();

protected:
    bool CheckFileType(const char *type, long size) override;
    void LoadChunk(FILE *fp, const char *type, long size) override;
    void LoadList(FILE *fp, const char *type, long size) override;
};
