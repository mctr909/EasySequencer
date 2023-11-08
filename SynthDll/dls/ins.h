#ifndef __DLS_INS_H__
#define __DLS_INS_H__

#include "../riff.h"

class INS;
class LRGN;
class LART;

class LINS : public RIFF {
public:
    int32 m_count = 0;
    INS **mpc_ins = nullptr;

public:
    LINS(FILE *fp, long size, int32 count);
    ~LINS();

protected:
    void load_chunk(FILE *fp, const char *type, long size) override;
};

class INS : public RIFF {
public:
#pragma pack(4)
    struct INSH {
        uint32 regions;
        byte bank_lsb;
        byte bank_msb;
        byte _reserve1;
        byte bank_flags;
        byte prog_num;
        byte _reserve2;
        byte _reserve3;
        byte _reserve4;
    };
#pragma pack()

public:
    INSH m_insh = { 0 };
    LRGN *mc_lrgn = nullptr;
    LART *mc_lart = nullptr;

public:
    INS(FILE *fp, long size);
    ~INS();

protected:
    void load_chunk(FILE *fp, const char *type, long size) override;
};

#endif /* __DLS_INS_H__ */
