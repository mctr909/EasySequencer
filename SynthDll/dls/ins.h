#ifndef __DLS_INS_H__
#define __DLS_INS_H__

#include "../type.h"
#include "../riff.h"

class INS_;
class LRGN;
class LART;

class LINS : public RIFF {
public:
    int32 Count = 0;
    INS_ **pcInst = nullptr;

public:
    LINS(FILE *fp, long size, int32 count);
    ~LINS();

protected:
    void LoadChunk(FILE *fp, const char *type, long size) override;
};

class INS_ : public RIFF {
public:
    struct INSH {
        uint32 regions;
        byte bankLSB;
        byte bankMSB;
        byte reserve1;
        byte bankFlags;
        byte progNum;
        byte reserve2;
        byte reserve3;
        byte reserve4;
    };

public:
    INSH Header = { 0 };
    LRGN *cLrgn = nullptr;
    LART *cLart = nullptr;

public:
    INS_(FILE *fp, long size);
    ~INS_();

protected:
    void LoadChunk(FILE *fp, const char *type, long size) override;
};

#endif /* __DLS_INS_H__ */
