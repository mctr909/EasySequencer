#pragma once
#include "dls_struct.h"
#include "riff_chunk.h"

class ART_;

class LART : public RiffChunk {
public:
    ART_ *cArt = NULL;

public:
    LART(FILE *fp, long size);
    ~LART();

protected:
    void LoadChunk(FILE *fp, const char *type, long size) override;
};

class ART_ {
public:
    int Count = 0;
    DLS_CONN **ppConnection;

public:
    ART_(FILE *fp, long size);
    ~ART_();
};
