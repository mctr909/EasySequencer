#ifndef __DLS_ART_H__
#define __DLS_ART_H__

#include "../type.h"
#include "dls_struct.h"

class ART_;

class LART : public Riff {
public:
    ART_ *cArt = NULL;

public:
    LART(FILE *fp, long size);
    ~LART();

protected:
    void LoadChunk(FILE *fp, const char *type, long size) override;
};

class ART_ {
public:
    uint32 Count = 0;
    DLS_CONN **ppConnection;

public:
    ART_(FILE *fp, long size);
    ~ART_();
};

#endif /* __DLS_ART_H__ */
