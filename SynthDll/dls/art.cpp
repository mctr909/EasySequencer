#include <stdlib.h>
#include <string.h>

#include "art.h"

LART::LART(FILE *fp, long size) : RIFF() {
    Load(fp, size);
}

LART::~LART() {
    delete cArt;
    cArt = nullptr;
}

void
LART::LoadChunk(FILE *fp, const char *type, long size) {
    if (0 == strcmp("art1", type) || 0 == strcmp("art2", type)) {
        cArt = new ART(fp, size);
        return;
    }
    fseek(fp, size, SEEK_CUR);
}

ART::ART(FILE *fp, long size) {
    fseek(fp, 4, SEEK_CUR);
    fread_s(&Count, 4, 4, 1, fp);

    ppConnection = (CONN**)calloc(Count, sizeof(CONN*));

    for (uint32 i = 0; i < Count; i++) {
        ppConnection[i] = (CONN*)malloc(sizeof(CONN));
        fread_s(ppConnection[i], sizeof(CONN), sizeof(CONN), 1, fp);
    }
}

ART::~ART() {
    for (uint32 i = 0; i < Count; i++) {
        free(ppConnection[i]);
    }
    free(ppConnection);
    ppConnection = nullptr;
}

double
ART::CONN::getValue() {
    switch (destination) {
    case E_DST::ATTENUATION:
    case E_DST::FILTER_Q:
        return pow(10.0, scale / (200 * 65536.0));

    case E_DST::PAN:
        return (scale / 655360.0) - 0.5;

    case E_DST::LFO_START_DELAY:
    case E_DST::VIB_START_DELAY:
    case E_DST::EG1_ATTACK_TIME:
    case E_DST::EG1_DECAY_TIME:
    case E_DST::EG1_RELEASE_TIME:
    case E_DST::EG1_DELAY_TIME:
    case E_DST::EG1_HOLD_TIME:
    case E_DST::EG1_SHUTDOWN_TIME:
    case E_DST::EG2_ATTACK_TIME:
    case E_DST::EG2_DECAY_TIME:
    case E_DST::EG2_RELEASE_TIME:
    case E_DST::EG2_DELAY_TIME:
    case E_DST::EG2_HOLD_TIME: {
        auto tmp = pow(2.0, scale / (1200 * 65536.0));
        if (tmp < 0.001) {
            return 0.001;
        } else {
            return tmp;
        }
    }

    case E_DST::EG1_SUSTAIN_LEVEL:
    case E_DST::EG2_SUSTAIN_LEVEL:
        return pow(2.0, -0.0005 * scale / 65536.0);

    case E_DST::PITCH:
    case E_DST::LFO_FREQUENCY:
    case E_DST::VIB_FREQUENCY:
    case E_DST::FILTER_CUTOFF:
        return pow(2.0, (scale / 65536.0 - 6900) / 1200.0) * 440;

    default:
        return 0.0;
    }
}