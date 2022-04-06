#include "dls.h"
#include <string.h>

DLS::DLS(LPWSTR path) : RiffChunk() {
    Load(path, 0);
}

DLS::~DLS() {
    if (NULL != cLins) {
        delete cLins;
        cLins = NULL;
    }
    if (NULL != cWvpl) {
        delete cWvpl;
        cWvpl = NULL;
    }
}

bool DLS::CheckFileType(const char *type, long size) {
    return 0 == strcmp("DLS ", type);
}

void DLS::LoadChunk(FILE *fp, const char *type, long size) {
    if (0 == strcmp("colh", type)) {
        fread_s(&InstCount, 4, 4, 1, fp);
        fseek(fp, size - 4, SEEK_CUR);
        return;
    }
    if (0 == strcmp("ptbl", type)) {
        fseek(fp, 4, SEEK_CUR);
        fread_s(&WaveCount, 4, 4, 1, fp);
        fseek(fp, size - 8, SEEK_CUR);
        return;
    }
    if (0 == strcmp("vers", type)) {
        fseek(fp, size, SEEK_CUR);
        return;
    }
    if (0 == strcmp("msyn", type)) {
        fseek(fp, size, SEEK_CUR);
        return;
    }
    if (0 == strcmp("DLID", type)) {
        fseek(fp, size, SEEK_CUR);
        return;
    }
    fseek(fp, size, SEEK_CUR);
}

void DLS::LoadList(FILE *fp, const char *type, long size) {
    if (0 == strcmp("lins", type)) {
        cLins = new LINS(fp, size, InstCount);
        return;
    }
    if (0 == strcmp("wvpl", type)) {
        cWvpl = new WVPL(fp, size, WaveCount);
        return;
    }
    fseek(fp, size, SEEK_CUR);
}
