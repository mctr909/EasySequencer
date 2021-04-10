#include <stdlib.h>
#include <string.h>
#include "note.h"

Note::Note() :
    mChannelNum(0),
    mNum(0),
    mState(E_NOTE_STATE::FREE),
    mReserved(0),
    mVelocity(0),
    mpChannel(0),
    mppSamplers()
{}

Note** Note::create(int count) {
    auto notes = (Note**)malloc(sizeof(Note*) * count);
    for (int i = 0; i < count; i++) {
        notes[i] = new Note();
    }
    return notes;
}
