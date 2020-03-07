#pragma once
typedef unsigned char  Bool;
typedef unsigned char  byte;
typedef signed   char  sbyte;
typedef unsigned short ushort;
typedef unsigned int   uint;

#pragma pack(push, 1)
typedef struct {
    byte lsb;
    short msb;
} int24;
#pragma pack(pop)

#define true  ((Bool)1)
#define false ((Bool)0)

extern inline void setInt24(int24 *output, double value) {
    int tmp = (int)(value * 0x7FFFFFFF);
    output->lsb = (tmp & 0x0000FF00) >> 8;
    output->msb = (tmp & 0xFFFF0000) >> 16;
}

extern inline double fromInt24(int24 *value) {
    return (double)((value->msb << 8) | value->lsb) / 0x800000;
}
