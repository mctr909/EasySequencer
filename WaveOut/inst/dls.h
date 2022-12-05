#pragma once
#include "../type.h"
#include "../riff.h"
#include "dls_ins.h"
#include "dls_wave.h"

class DLS : public Riff {
public:
    LINS *cLins = NULL;
    WVPL *cWvpl = NULL;
    uint32 InstCount = 0;
    uint32 WaveCount = 0;

public:
    DLS() : Riff() {}
    ~DLS();

public:
    E_LOAD_STATUS Load(LPWSTR path);

protected:
    bool CheckFileType(const char *type, long size) override;
    void LoadChunk(FILE *fp, const char *type, long size) override;
    void LoadList(FILE *fp, const char *type, long size) override;
};
