#ifndef __DLS_WAVE_H__
#define __DLS_WAVE_H__

#include "../type.h"

class WAVE;
class LART;

class WVPL : public RIFF {
public:
    int32 Count = 0;
    WAVE **pcWave = nullptr;

public:
    WVPL(FILE *fp, long size, int32 count);
    ~WVPL();

protected:
    void LoadChunk(FILE *fp, const char *type, long size) override;
};

class WAVE : public RIFF {
public:
    WAVE_FMT Format = { 0 };
    DLS_WSMP *pWaveSmpl = nullptr;
    DLS_LOOP **ppWaveLoop = nullptr;
    uint32 DataSize = 0;
    byte *pData = nullptr;

public:
    WAVE(FILE *fp, long size);
    ~WAVE();

protected:
    void LoadChunk(FILE *fp, const char *type, long size) override;
};

#endif /* __DLS_WAVE_H__ */
