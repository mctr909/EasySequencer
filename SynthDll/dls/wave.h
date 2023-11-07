#ifndef __DLS_WAVE_H__
#define __DLS_WAVE_H__

#include "../riff.h"

class WAVE;
class LART;
struct WSMP_VALUES;
struct WSMP_LOOP;

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
    WSMP_VALUES *pWaveSmpl = nullptr;
    WSMP_LOOP **ppWaveLoop = nullptr;
    uint32 DataSize = 0;
    byte *pData = nullptr;

public:
    WAVE(FILE *fp, long size);
    ~WAVE();

protected:
    void LoadChunk(FILE *fp, const char *type, long size) override;
};

#endif /* __DLS_WAVE_H__ */
