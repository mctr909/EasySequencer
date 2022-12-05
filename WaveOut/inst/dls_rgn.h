#pragma once
#include "../riff.h"
#include "dls_struct.h"

class RGN_;
class LART;

class LRGN : public Riff {
public:
    RGN_ **pcRegion = NULL;
    int32 Count;

public:
    LRGN(FILE *fp, long size, int32 count);
    ~LRGN();

protected:
    void LoadList(FILE *fp, const char *type, long size) override;
};

class RGN_ : public Riff {
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
