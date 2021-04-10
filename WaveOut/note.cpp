#include <stdlib.h>
#include <string.h>
#include "note.h"

NOTE** createNotes(int count) {
    auto notes = (NOTE**)malloc(sizeof(NOTE*) * count);
    for (int i = 0; i < count; ++i) {
        notes[i] = (NOTE*)malloc(sizeof(NOTE));
        memset(notes[i], 0, sizeof(NOTE));
    }
    return notes;
}
