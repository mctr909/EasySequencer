#ifndef __DLS_INS_H__
#define __DLS_INS_H__

#include "../type.h"
#include "dls_struct.h"

class INS_;
class LRGN;
class LART;

class LINS : public Riff {
public:
    int32 Count = 0;
    INS_ **pcInst = nullptr;

public:
    LINS(FILE *fp, long size, int32 count);
    ~LINS();

protected:
    void LoadChunk(FILE *fp, const char *type, long size) override;
};

class INS_ : public Riff {
public:
    char Name[32] = { 0 };
    char Category[32] = { 0 };
    DLS_INSH Header = { 0 };
    LRGN *cLrgn = nullptr;
    LART *cLart = nullptr;

public:
    INS_(FILE *fp, long size);
    ~INS_();

protected:
    void LoadInfo(FILE *fp, const char *type, long size) override;
    void LoadChunk(FILE *fp, const char *type, long size) override;
};

#endif /* __DLS_INS_H__ */
