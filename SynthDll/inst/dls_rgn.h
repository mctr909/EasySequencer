#ifndef __DLS_RGN_H__
#define __DLS_RGN_H__

#include "../type.h"
#include "dls_struct.h"

class RGN_;
class LART;

class LRGN : public RIFF {
public:
    int32 Count = 0;
    RGN_ **pcRegion = nullptr;

public:
    LRGN(FILE *fp, long size, int32 count);
    ~LRGN();

protected:
    void LoadChunk(FILE *fp, const char *type, long size) override;
};

class RGN_ : public RIFF {
public:
    DLS_RGNH Header = { 0 };
    DLS_WLNK WaveLink = { 0 };
    DLS_WSMP *pWaveSmpl = nullptr;
    DLS_LOOP **ppWaveLoop = nullptr;
    LART *cLart = nullptr;

public:
    RGN_(FILE *fp, long size);
    ~RGN_();

protected:
    void LoadChunk(FILE *fp, const char *type, long size) override;
};

#endif /* __DLS_RGN_H__ */
