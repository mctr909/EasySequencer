#ifndef __DLS_RGN_H__
#define __DLS_RGN_H__

#include "../type.h"
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
    DLS_RGNH Header;
    DLS_WLNK WaveLink;
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

#endif /* __DLS_RGN_H__ */
