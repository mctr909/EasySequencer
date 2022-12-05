#ifndef __DLS_INS_H__
#define __DLS_INS_H__

#include "../type.h"
#include "dls_struct.h"

class INS_;
class LRGN;
class LART;

class LINS : public Riff {
public:
    INS_ **pcInst = NULL;
    int32 Count;

public:
    LINS(FILE *fp, long size, int32 count);
    ~LINS();

protected:
    void LoadList(FILE *fp, const char *type, long size) override;
};

class INS_ : public Riff {
public:
    char Name[32] = { 0 };
    char Category[32] = { 0 };
    DLS_INSH Header;
    LRGN *cLrgn = NULL;
    LART *cLart = NULL;

public:
    INS_(FILE *fp, long size);
    ~INS_();

protected:
    void LoadInfo(FILE *fp, const char *type, long size) override;
    void LoadChunk(FILE *fp, const char *type, long size) override;
    void LoadList(FILE *fp, const char *type, long size) override;
};

#endif /* __DLS_INS_H__ */
