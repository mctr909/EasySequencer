#ifndef __RIFF_H__
#define __RIFF_H__

#include <stdio.h>

#include "../type.h"

/******************************************************************************/
class Riff {
public:
    Riff() {}

protected:
    E_LOAD_STATUS Load(STRING path, long offset);
    void Load(FILE* fp, long size);
    virtual bool CheckFileType(const char *type, long size) { return false; }
    virtual void LoadChunk(FILE *fp, const char *type, long size) { fseek(fp, size, SEEK_CUR); }
    virtual void LoadInfo(FILE *fp, const char *type, long size) { fseek(fp, size, SEEK_CUR); }

private:
    void infoLoop(FILE *fp, long size);
};

#endif /* __RIFF_H__ */
