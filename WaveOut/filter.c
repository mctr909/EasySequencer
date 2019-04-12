#include "filter.h"

static const double PI        = 3.14159265;
static const double INV_FACT2 = 0.50000000;
static const double INV_FACT3 = 0.16666667;
static const double INV_FACT4 = 0.04166667;
static const double INV_FACT5 = 0.00833333;
static const double INV_FACT6 = 0.00138889;
static const double INV_FACT7 = 0.00019841;
static const double INV_FACT8 = 0.00002480;
static const double INV_FACT9 = 0.00000276;

inline void filter_exec(FILTER *filter, double input) {
    double w = filter->cut * PI * 0.97;
    double w2 = w * w;
    double c = INV_FACT8;
    double s = INV_FACT9;
    c *= w2;
    s *= w2;
    c -= INV_FACT6;
    s -= INV_FACT7;
    c *= w2;
    s *= w2;
    c += INV_FACT4;
    s += INV_FACT5;
    c *= w2;
    s *= w2;
    c -= INV_FACT2;
    s -= INV_FACT3;
    c *= w2;
    s *= w2;
    c += 1.0;
    s += 1.0;
    s *= w;

    double a = s / (filter->res * 4.0 + 1.0);
    double m = 1.0 / (a + 1.0);
    double ka0 = -2.0 * c  * m;
    double ka1 = (1.0 - a) * m;
    double kb0 = (1.0 - c) * m;
    double kb1 = kb0 * 0.5;

    double output =
          kb1 * input
        + kb0 * filter->b0
        + kb1 * filter->b1
        - ka0 * filter->a0
        - ka1 * filter->a1
    ;
    filter->b1 = filter->b0;
    filter->b0 = input;
    filter->a1 = filter->a0;
    filter->a0 = output;

    input = output;
    output =
          kb1 * input
        + kb0 * filter->b2
        + kb1 * filter->b3
        - ka0 * filter->a2
        - ka1 * filter->a3
    ;
    filter->b3 = filter->b2;
    filter->b2 = input;
    filter->a3 = filter->a2;
    filter->a2 = output;
}
