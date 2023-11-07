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
    LINS *cLins = nullptr;
    WVPL *cWvpl = nullptr;
    uint32 InstCount = 0;
    uint32 WaveCount = 0;

public:
    DLS();
    ~DLS();
    E_LOAD_STATUS Load(STRING path);

protected:
    bool CheckFileType(const char *type, long size) override;
    void LoadChunk(FILE *fp, const char *type, long size) override;
};

#endif /* __DLS_H__ */
