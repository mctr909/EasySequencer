#include <string.h>

#include "riff.h"

#include "dls_ins.h"
#include "dls_wave.h"

#include "dls.h"

DLS::DLS() : Riff() { }

DLS::~DLS() {
    if (nullptr != cLins) {
        delete cLins;
        cLins = nullptr;
    }
    if (nullptr != cWvpl) {
        delete cWvpl;
        cWvpl = nullptr;
    }
}

E_LOAD_STATUS DLS::Load(STRING path) {
    return Riff::Load(path, 0);
}

bool
DLS::CheckFileType(const char *type, long size) {
    return 0 == strcmp("DLS ", type);
}

void
DLS::LoadChunk(FILE *fp, const char *type, long size) {
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
