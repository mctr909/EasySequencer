#include <string.h>

#include "../riff.h"

#include "ins.h"
#include "wave.h"
#include "main.h"

DLS::DLS() : RIFF() { }

DLS::~DLS() {
    if (nullptr != mc_lins) {
        delete mc_lins;
        mc_lins = nullptr;
    }
    if (nullptr != mc_wvpl) {
        delete mc_wvpl;
        mc_wvpl = nullptr;
    }
}

E_LOAD_STATUS DLS::load(STRING path) {
    return RIFF::load(path, 0);
}

bool
DLS::check_file_type(const char *type, long size) {
    return 0 == strcmp("DLS ", type);
}

void
DLS::load_chunk(FILE *fp, const char *type, long size) {
    if (0 == strcmp("colh", type)) {
        fread_s(&m_inst_count, 4, 4, 1, fp);
        fseek(fp, size - 4, SEEK_CUR);
        return;
    }
    if (0 == strcmp("ptbl", type)) {
        fseek(fp, 4, SEEK_CUR);
        fread_s(&m_wave_count, 4, 4, 1, fp);
        fseek(fp, size - 8, SEEK_CUR);
        return;
    }
    if (0 == strcmp("lins", type)) {
        mc_lins = new LINS(fp, size, m_inst_count);
        return;
    }
    if (0 == strcmp("wvpl", type)) {
        mc_wvpl = new WVPL(fp, size, m_wave_count);
        return;
    }
    fseek(fp, size, SEEK_CUR);
}
