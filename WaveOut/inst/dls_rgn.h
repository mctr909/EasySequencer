#pragma once
#include "dls_struct.h"
#include "riff_chunk.h"

class RGN_;
class LART;

class LRGN : public RiffChunk {
public:
    RGN_ **pcRegion = NULL;
    int Count;

public:
    LRGN(FILE *fp, long size, int count);
    ~LRGN();

protected:
    void LoadList(FILE *fp, const char *type, long size) override;
};

class RGN_ : public RiffChunk {
public:
    DLS_RGNH Header = { 0 };
    DLS_WLNK WaveLink = { 0 };
    DLS_WSMP *pWaveSmpl = NULL;
    DLS_LOOP **ppWaveLoop = NULL;
    LART *cLart = NULL;

public:
    RGN_(FILE *fp, long size);
    ~RGN_();

protected:
    void LoadChunk(FILE *fp, const char *type, long size) override;
    void LoadList(FILE *fp, const char *type, long size) override;
};
