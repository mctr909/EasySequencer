#ifndef __DLS_WSMP_H__
#define __DLS_WSMP_H__

#include "../type.h"

struct WSMP_VALUES {
    uint32 size;
    uint16 unityNote;
    int16 fineTune;
    int32 gainInt;
    uint32 options;
    uint32 loopCount;
    double getFineTune();
    double getGain();
};

struct WSMP_LOOP {
    uint32 size;
    uint32 type;
    uint32 start;
    uint32 length;
};

#endif /* __DLS_WSMP_H__ */
