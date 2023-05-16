#pragma once

/******************************************************************************/
#define WAVE_MAX 32767.0

/******************************************************************************/
typedef unsigned char  byte;
typedef unsigned short uint16;
typedef unsigned int   uint32;
typedef signed   char  sbyte;
typedef signed   short int16;
typedef signed   int   int32;

typedef int16 WAVE_DATA;

/******************************************************************************/
#pragma pack(push, 1)
typedef struct {
    byte lsb;
    short msb;
} int24;
#pragma pack(pop)

/******************************************************************************/
enum struct E_LOAD_STATUS : int32 {
    SUCCESS,
    FILE_OPEN_FAILED,
    ALLOCATE_FAILED,
    UNKNOWN_FILE
};

/******************************************************************************/
inline void
int24_set(int24 *output, double value) {
    auto tmp = (int32)(value * 0x7FFFFFFF);
    output->lsb = (tmp & 0x0000FF00) >> 8;
    output->msb = (tmp & 0xFFFF0000) >> 16;
}

inline double
int24_get(int24 *value) {
    return (double)((value->msb << 8) | value->lsb) / 0x800000;
}
