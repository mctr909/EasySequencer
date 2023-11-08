#ifndef __DLS_WSMP_H__
#define __DLS_WSMP_H__

#include "../type.h"

#pragma pack(4)
struct WSMP_VALUES {
    uint32 size;
    uint16 unity_note;
    int16 _fine_tune;
    int32 _gain;
    uint32 options;
    uint32 loop_count;
    double fine_tune();
    double gain();
};
#pragma pack()
#pragma pack(8)
struct WSMP_LOOP {
    uint32 size;
    uint32 type;
    uint32 start;
    uint32 length;
};
#pragma pack()

#endif /* __DLS_WSMP_H__ */
