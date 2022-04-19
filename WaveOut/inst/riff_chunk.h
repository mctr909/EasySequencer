#pragma once
#include <stdio.h>
#include <windows.h>
#include "../type.h"

class RiffChunk {
public:
    RiffChunk() {}

protected:
    void Load(FILE *fp, long size);
    E_LOAD_STATUS Load(LPWSTR path, long offset);
    virtual bool CheckFileType(const char *type, long size) { return false; }
    virtual void LoadChunk(FILE *fp, const char *type, long size) { fseek(fp, size, SEEK_CUR); }
    virtual void LoadList(FILE *fp, const char *type, long size) { fseek(fp, size, SEEK_CUR); }
    virtual void LoadInfo(FILE *fp, const char *type, long size) { fseek(fp, size, SEEK_CUR); }

private:
    void loop(FILE *fp, long size);
    void infoLoop(FILE *fp, long size);
};
