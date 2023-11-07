#ifndef __DLS_RGN_H__
#define __DLS_RGN_H__

#include "../riff.h"

class RGN;
class LART;
struct WSMP_VALUES;
struct WSMP_LOOP;

class LRGN : public RIFF {
public:
    int32 m_count = 0;
    RGN **mpc_rgn = nullptr;

public:
    LRGN(FILE *fp, long size, int32 count);
    ~LRGN();

protected:
    void load_chunk(FILE *fp, const char *type, long size) override;
};

class RGN : public RIFF {
public:
    struct RGNH {
        uint16 key_low;
        uint16 key_high;
        uint16 velo_low;
        uint16 velo_high;
        uint16 options;
        uint16 key_group;
        uint16 layer;
    };
    struct WLNK {
        uint16 options;
        uint16 phase_group;
        uint32 channel;
        uint32 table_index;
    };

public:
    RGNH m_rgnh = { 0 };
    WLNK m_wlnk = { 0 };
    WSMP_VALUES *mp_wsmp = nullptr;
    WSMP_LOOP *mp_loop = nullptr;
    LART* mc_lart = nullptr;

public:
    RGN(FILE *fp, long size);
    ~RGN();

protected:
    void load_chunk(FILE *fp, const char *type, long size) override;
};

#endif /* __DLS_RGN_H__ */
