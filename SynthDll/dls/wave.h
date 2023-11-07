#ifndef __DLS_WAVE_H__
#define __DLS_WAVE_H__

#include "../riff.h"

class WAVE;
class LART;
struct WSMP_VALUES;
struct WSMP_LOOP;

class WVPL : public RIFF {
public:
    int32 m_count = 0;
    WAVE **mpc_wave = nullptr;

public:
    WVPL(FILE *fp, long size, int32 count);
    ~WVPL();

protected:
    void load_chunk(FILE *fp, const char *type, long size) override;
};

class WAVE : public RIFF {
public:
    WAVE_FMT m_fmt = { 0 };
    WSMP_VALUES *mp_wsmp = nullptr;
    WSMP_LOOP **mpp_loop = nullptr;
    uint32 m_data_size = 0;
    byte *mp_data = nullptr;

public:
    WAVE(FILE *fp, long size);
    ~WAVE();

protected:
    void load_chunk(FILE *fp, const char *type, long size) override;
};

#endif /* __DLS_WAVE_H__ */
