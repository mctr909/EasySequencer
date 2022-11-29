#pragma once
#include "dls_struct.h"
#include "dls_rgn.h"
#include "dls_art.h"
#include "riff_chunk.h"

class INS_;
class LRGN;
class LART;

class LINS : public RiffChunk {
public:
    INS_ **pcInst = NULL;
    int32 Count;

public:
    LINS(FILE *fp, long size, int32 count);
    ~LINS();

protected:
    void LoadList(FILE *fp, const char *type, long size) override;
};

class INS_ : public RiffChunk {
public:
    char Name[32] = { 0 };
    char Category[32] = { 0 };
    DLS_INSH Header;
    LRGN *cLrgn = NULL;
    LART *cLart = NULL;

public:
    INS_(FILE *fp, long size);
    ~INS_();

protected:
    void LoadInfo(FILE *fp, const char *type, long size) override;
    void LoadChunk(FILE *fp, const char *type, long size) override;
    void LoadList(FILE *fp, const char *type, long size) override;
};
