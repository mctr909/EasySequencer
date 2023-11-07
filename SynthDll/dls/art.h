#ifndef __DLS_ART_H__
#define __DLS_ART_H__

#include "../type.h"
#include "../riff.h"
#include "struct.h"

class ART_;

class LART : public RIFF {
public:
    ART_ *cArt = nullptr;

public:
    LART(FILE *fp, long size);
    ~LART();

protected:
    void LoadChunk(FILE *fp, const char *type, long size) override;
};

class ART_ {
public:
    uint32 Count = 0;
    DLS_CONN **ppConnection = nullptr;

public:
    ART_(FILE *fp, long size);
    ~ART_();
};

#endif /* __DLS_ART_H__ */
