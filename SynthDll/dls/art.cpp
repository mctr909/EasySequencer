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
        cArt = new ART_(fp, size);
        return;
    }
    fseek(fp, size, SEEK_CUR);
}

ART_::ART_(FILE *fp, long size) {
    fseek(fp, 4, SEEK_CUR);
    fread_s(&Count, 4, 4, 1, fp);

    ppConnection = (CONN**)calloc(Count, sizeof(CONN*));

    for (uint32 i = 0; i < Count; i++) {
        ppConnection[i] = (CONN*)malloc(sizeof(CONN));
        fread_s(ppConnection[i], sizeof(CONN), sizeof(CONN), 1, fp);
    }
}

ART_::~ART_() {
    for (uint32 i = 0; i < Count; i++) {
        free(ppConnection[i]);
    }
    free(ppConnection);
    ppConnection = nullptr;
}
