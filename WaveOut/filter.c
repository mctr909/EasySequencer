#include "filter.h"

#define USE_MMX

#ifdef USE_MMX
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

static const double DOUBLE_1 = 1.0;
#endif

inline void filter_exec(FILTER *filter, double input) {
#ifdef USE_MMX
    __asm {
        mov    eax, dword ptr filter
        movsd  xmm1, CUT
        movsd  xmm2, RES
        movsd  xmm6, DOUBLE_1
        subsd  xmm6, xmm1
        mulsd  xmm6, xmm1
        addsd  xmm6, xmm1
        movsd  xmm7, xmm6
        addsd  xmm7, xmm6
        subsd  xmm7, DOUBLE_1
        movsd  xmm1, A7
        mulsd  xmm1, xmm2
        movsd  xmm0, input
        subsd  xmm0, xmm1

        movsd  xmm1, xmm0
        addsd  xmm0, BI
        movsd  BI  , xmm1
        mulsd  xmm0, xmm6
        movsd  xmm1, A0
        mulsd  xmm1, xmm7
        subsd  xmm0, xmm1
        movsd  A0  , xmm0

        movsd  xmm1, xmm0
        addsd  xmm0, B0
        movsd  B0  , xmm1
        mulsd  xmm0, xmm6
        movsd  xmm1, A1
        mulsd  xmm1, xmm7
        subsd  xmm0, xmm1
        movsd  A1  , xmm0

        movsd  xmm1, xmm0
        addsd  xmm0, B1
        movsd  B1  , xmm1
        mulsd  xmm0, xmm6
        movsd  xmm1, A2
        mulsd  xmm1, xmm7
        subsd  xmm0, xmm1
        movsd  A2  , xmm0

        movsd  xmm1, xmm0
        addsd  xmm0, B2
        movsd  B2  , xmm1
        mulsd  xmm0, xmm6
        movsd  xmm1, A3
        mulsd  xmm1, xmm7
        subsd  xmm0, xmm1
        movsd  A3  , xmm0

        movsd  xmm1, xmm0
        addsd  xmm0, B3
        movsd  B3  , xmm1
        mulsd  xmm0, xmm6
        movsd  xmm1, A4
        mulsd  xmm1, xmm7
        subsd  xmm0, xmm1
        movsd  A4  , xmm0

        movsd  xmm1, xmm0
        addsd  xmm0, B4
        movsd  B4  , xmm1
        mulsd  xmm0, xmm6
        movsd  xmm1, A5
        mulsd  xmm1, xmm7
        subsd  xmm0, xmm1
        movsd  A5  , xmm0

        movsd  xmm1, xmm0
        addsd  xmm0, B5
        movsd  B5  , xmm1
        mulsd  xmm0, xmm6
        movsd  xmm1, A6
        mulsd  xmm1, xmm7
        subsd  xmm0, xmm1
        movsd  A6  , xmm0

        movsd  xmm1, xmm0
        addsd  xmm0, B6
        movsd  B6  , xmm1
        mulsd  xmm0, xmm6
        movsd  xmm1, A7
        mulsd  xmm1, xmm7
        subsd  xmm0, xmm1
        movsd  A7  , xmm0
    }
#else
    double fi = 1.0 - filter->cut;
    double p = filter->cut * fi + filter->cut;
    double q = p + p - 1.0;

    input -= filter->res * filter->a7;

    filter->a0 = (input      + filter->bi) * p - filter->a0 * q;
    filter->a1 = (filter->a0 + filter->b0) * p - filter->a1 * q;
    filter->a2 = (filter->a1 + filter->b1) * p - filter->a2 * q;
    filter->a3 = (filter->a2 + filter->b2) * p - filter->a3 * q;
    filter->a4 = (filter->a3 + filter->b3) * p - filter->a4 * q;
    filter->a5 = (filter->a4 + filter->b4) * p - filter->a5 * q;
    filter->a6 = (filter->a5 + filter->b5) * p - filter->a6 * q;
    filter->a7 = (filter->a6 + filter->b6) * p - filter->a7 * q;

    filter->bi = input;
    filter->b0 = filter->a0;
    filter->b1 = filter->a1;
    filter->b2 = filter->a2;
    filter->b3 = filter->a3;
    filter->b4 = filter->a4;
    filter->b5 = filter->a5;
    filter->b6 = filter->a6;
#endif
}
