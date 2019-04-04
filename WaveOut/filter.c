#include "filter.h"

static const double PI      = 3.14159265;
static const double INV2    = 0.50000000;
static const double INV6    = 0.16666667;
static const double INV24   = 0.04166667;
static const double INV120  = 0.00833333;
static const double INV720  = 0.00138889;
static const double INV5040 = 0.00019841;

inline void filter_exec(FILTER *filter, double input) {
    double w = filter->cut * PI * 0.875;
    double c = 1.0;
    double s = w;
    c -= w*w * INV2;
    s -= w*w*w * INV6;
    c += w*w*w*w * INV24;
    s += w*w*w*w*w * INV120;
    c -= w*w*w*w*w*w * INV720;
    s -= w*w*w*w*w*w*w * INV5040;

    double a = s / (filter->res * 16.0 + 1.0);
    double m = 1.0 / (a + 1.0);
    double ka0 = -2.0 * c  * m;
    double ka1 = (1.0 - a) * m;
    double kb0 = (1.0 - c) * m;
    double kb1 = kb0;
    kb1 *= 0.5;

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
