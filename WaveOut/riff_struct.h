#pragma once
typedef struct WAVE_FMT {
    unsigned short tag;
    unsigned short channels;
    unsigned int sampleRate;
    unsigned int bytesPerSec;
    unsigned short blockAlign;
    unsigned short bits;
} WAVE_FMT;

#pragma pack(push, 1)
typedef struct RIFFint24 {
    unsigned char lsb;
    short msb;
} RIFFint24;
#pragma pack(pop)

/******************************************************************************/
extern inline void riff_setInt24(RIFFint24 *output, double value) {
    int tmp = (int)(value * 0x7FFFFFFF);
    output->lsb = (tmp & 0x0000FF00) >> 8;
    output->msb = (tmp & 0xFFFF0000) >> 16;
}

extern inline double riff_getDoubleFromInt24(RIFFint24 *value) {
    return (double)((value->msb << 8) | value->lsb) / 0x800000;
}
