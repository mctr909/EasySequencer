#ifndef __DLS_H__
#define __DLS_H__

#include "../riff.h"

#include "wsmp.h"
#include "ins.h"
#include "rgn.h"
#include "art.h"
#include "wave.h"

class DLS : public RIFF {
public:
    uint32 m_inst_count = 0;
    uint32 m_wave_count = 0;
    LINS *mc_lins = nullptr;
    WVPL *mc_wvpl = nullptr;

public:
    DLS();
    ~DLS();
    E_LOAD_STATUS load(STRING path);

protected:
    bool check_file_type(const char *type, long size) override;
    void load_chunk(FILE *fp, const char *type, long size) override;
};

#endif /* __DLS_H__ */
