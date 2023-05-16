#ifndef __RIFF_H__
#define __RIFF_H__

#include "type.h"

#include <stdio.h>
#include <windows.h>

/******************************************************************************/
#pragma pack(push, 4)
typedef struct WAVE_FMT {
    uint16 tag;
    uint16 channels;
    uint32 sampleRate;
    uint32 bytesPerSec;
    uint16 blockAlign;
    uint16 bits;
} WAVE_FMT;
#pragma pack(pop)

/******************************************************************************/
class Riff {
public:
    Riff() {}

protected:
    E_LOAD_STATUS Load(LPWSTR path, long offset);
    void Load(FILE* fp, long size);
    virtual bool CheckFileType(const char *type, long size) { return false; }
    virtual void LoadChunk(FILE *fp, const char *type, long size) { fseek(fp, size, SEEK_CUR); }
    virtual void LoadInfo(FILE *fp, const char *type, long size) { fseek(fp, size, SEEK_CUR); }

private:
    void infoLoop(FILE *fp, long size);
};

#endif /* __RIFF_H__ */
