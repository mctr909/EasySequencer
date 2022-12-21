#include <string.h>
#include "../riff.h"

#include "dls_art.h"

LART::LART(FILE *fp, long size) : Riff() {
    Load(fp, size);
}

LART::~LART() {
    delete cArt;
    cArt = NULL;
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

    ppConnection = (DLS_CONN**)calloc(Count, sizeof(DLS_CONN*));

    for (uint32 i = 0; i < Count; i++) {
        ppConnection[i] = (DLS_CONN*)malloc(sizeof(DLS_CONN));
        fread_s(ppConnection[i], sizeof(DLS_CONN), sizeof(DLS_CONN), 1, fp);
    }
}

ART_::~ART_() {
    for (uint32 i = 0; i < Count; i++) {
        free(ppConnection[i]);
    }
    free(ppConnection);
    ppConnection = NULL;
}
