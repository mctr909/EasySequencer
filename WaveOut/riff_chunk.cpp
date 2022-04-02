#include "riff_chunk.h"
#include <string.h>

void RiffChunk::Load(FILE *fp, long size) {
    loop(fp, size);
}

void RiffChunk::Load(LPWSTR path, long offset) {
    FILE *fp = NULL;
    _wfopen_s(&fp, path, TEXT("rb"));
    fseek(fp, offset, SEEK_SET);

    char riffId[5] = { 0 };
    unsigned int riffSize;
    char riffType[5] = { 0 };

    fread_s(&riffId, 4, 4, 1, fp);
    fread_s(&riffSize, 4, 4, 1, fp);
    fread_s(&riffType, 4, 4, 1, fp);

    if (0 == strcmp("RIFF", riffId) && CheckFileType(riffType, riffSize)) {
        loop(fp, riffSize - 4);
    }

    fclose(fp);
}

void RiffChunk::loop(FILE *fp, long size) {
    char chunkId[5] = { 0 };
    unsigned int chunkSize;
    char listType[5] = { 0 };

    long pos = 0;
    while (pos < size) {
        fread_s(&chunkId, 4, 4, 1, fp);
        fread_s(&chunkSize, 4, 4, 1, fp);
        pos += chunkSize + 8;

        if (0 == strcmp("LIST", chunkId)) {
            fread_s(&listType, 4, 4, 1, fp);
            if (0 == strcmp("INFO", listType)) {
                infoLoop(fp, chunkSize - 4);
            } else {
                LoadList(fp, listType, chunkSize - 4);
            }
        } else {
            LoadChunk(fp, chunkId, chunkSize);
        }
    }
}

void RiffChunk::infoLoop(FILE *fp, long size) {
    char infoType[5] = { 0 };
    unsigned int infoSize;

    long pos = 0;
    while (pos < size) {
        fread_s(&infoType, 4, 4, 1, fp);
        fread_s(&infoSize, 4, 4, 1, fp);
        pos += infoSize + 8;

        LoadInfo(fp, infoType, infoSize);
        pos += (unsigned int)(infoSize % 2 == 0 ? 0 : 1);
    } 
}
