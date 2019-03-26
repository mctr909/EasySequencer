#include "filter.h"

#define CUT xmmword ptr[eax+0]
#define RES xmmword ptr[eax+8]
#define BI  xmmword ptr[eax+16]
#define A0  xmmword ptr[eax+24]
#define B0  xmmword ptr[eax+32]
#define A1  xmmword ptr[eax+40]
#define B1  xmmword ptr[eax+48]
#define A2  xmmword ptr[eax+56]
#define B2  xmmword ptr[eax+64]
#define A3  xmmword ptr[eax+72]
#define B3  xmmword ptr[eax+80]
#define A4  xmmword ptr[eax+88]
#define B4  xmmword ptr[eax+96]
#define A5  xmmword ptr[eax+104]
#define B5  xmmword ptr[eax+112]
#define A6  xmmword ptr[eax+120]
#define B6  xmmword ptr[eax+128]
#define A7  xmmword ptr[eax+136]

inline void filter_exec(FILTER *filter, double input) {
    __asm {
        mov    eax , 1
        cvtsi2sd xmm0, eax
        mov    eax , dword ptr filter
        movsd  xmm3, CUT
        movsd  xmm4, RES
        movsd  xmm5, xmm0
        subsd  xmm5, xmm3
        mulsd  xmm5, xmm3
        addsd  xmm5, xmm3
        movsd  xmm6, xmm5
        addsd  xmm6, xmm5
        subsd  xmm6, xmm0
        movsd  xmm0, A7
        mulsd  xmm0, xmm4
        movsd  xmm7, input
        subsd  xmm7, xmm0
        movsd  xmm0, xmm7
        addsd  xmm0, BI
        mulsd  xmm0, xmm5
        movsd  xmm1, A0
        mulsd  xmm1, xmm6
        subsd  xmm0, xmm1
        movsd  A0  , xmm0
        movsd  xmm0, A0
        addsd  xmm0, B0
        mulsd  xmm0, xmm5
        movsd  xmm1, A1
        mulsd  xmm1, xmm6
        subsd  xmm0, xmm1
        movsd  A1  , xmm0
        movsd  xmm0, A1
        addsd  xmm0, B1
        mulsd  xmm0, xmm5
        movsd  xmm1, A2
        mulsd  xmm1, xmm6
        subsd  xmm0, xmm1
        movsd  A2  , xmm0
        movsd  xmm0, A2
        addsd  xmm0, B2
        mulsd  xmm0, xmm5
        movsd  xmm1, A3
        mulsd  xmm1, xmm6
        subsd  xmm0, xmm1
        movsd  A3  , xmm0
        movsd  xmm0, A3
        addsd  xmm0, B3
        mulsd  xmm0, xmm5
        movsd  xmm1, A4
        mulsd  xmm1, xmm6
        subsd  xmm0, xmm1
        movsd  A4  , xmm0
        movsd  xmm0, A4
        addsd  xmm0, B4
        mulsd  xmm0, xmm5
        movsd  xmm1, A5
        mulsd  xmm1, xmm6
        subsd  xmm0, xmm1
        movsd  A5  , xmm0
        movsd  xmm0, A5
        addsd  xmm0, B5
        mulsd  xmm0, xmm5
        movsd  xmm1, A6
        mulsd  xmm1, xmm6
        subsd  xmm0, xmm1
        movsd  A6  , xmm0
        movsd  xmm0, A6
        addsd  xmm0, B6
        mulsd  xmm0, xmm5
        movsd  xmm1, A7
        mulsd  xmm1, xmm6
        subsd  xmm0, xmm1
        movsd  A7  , xmm0
        movsd  BI  , xmm7
        movsd  xmm0, A0
        movsd  B0  , xmm0
        movsd  xmm0, A1
        movsd  B1  , xmm0
        movsd  xmm0, A2
        movsd  B2  , xmm0
        movsd  xmm0, A3
        movsd  B3  , xmm0
        movsd  xmm0, A4
        movsd  B4  , xmm0
        movsd  xmm0, A5
        movsd  B5  , xmm0
        movsd  xmm0, A6
        movsd  B6  , xmm0
    }
}
