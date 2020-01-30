#include "filter.h"

/******************************************************************************/
#define ADJUST_CUTOFF 0.98

#define PI        3.14159265
#define INV_FACT2 5.00000000e-01
#define INV_FACT3 1.66666667e-01
#define INV_FACT4 4.16666667e-02
#define INV_FACT5 8.33333333e-03
#define INV_FACT6 1.38888889e-03
#define INV_FACT7 1.98412698e-04
#define INV_FACT8 2.48015873e-05
#define INV_FACT9 2.75573192e-06

/******************************************************************************/
inline void filter(FILTER *pFilter, double input) {
    /** sin, cosの近似 **/
    double rad = pFilter->cut * PI * ADJUST_CUTOFF;
    double rad2 = rad * rad;
    double c = INV_FACT8;
    c *= rad2;
    c -= INV_FACT6;
    c *= rad2;
    c += INV_FACT4;
    c *= rad2;
    c -= INV_FACT2;
    c *= rad2;
    c++;
    double s = INV_FACT9;
    s *= rad2;
    s -= INV_FACT7;
    s *= rad2;
    s += INV_FACT5;
    s *= rad2;
    s -= INV_FACT3;
    s *= rad2;
    s++;
    s *= rad;

    /** IIRローパスフィルタ パラメータ設定 **/
    double a = s / (pFilter->res * 4.0 + 1.0);
    double m = 1.0 / (a + 1.0);
    double ka0 = -2.0 * c  * m;
    double kb0 = (1.0 - c) * m;
    double ka1 = (1.0 - a) * m;
    double kb1 = kb0 * 0.5;

    /** フィルタ1段目 **/
    double output =
        kb1 * input
        + kb0 * pFilter->b00
        + kb1 * pFilter->b01
        - ka0 * pFilter->a00
        - ka1 * pFilter->a01
        ;
    pFilter->b01 = pFilter->b00;
    pFilter->b00 = input;
    pFilter->a01 = pFilter->a00;
    pFilter->a00 = output;

    /** フィルタ2段目 **/
    input = output;
    output =
        kb1 * input
        + kb0 * pFilter->b10
        + kb1 * pFilter->b11
        - ka0 * pFilter->a10
        - ka1 * pFilter->a11
        ;
    pFilter->b11 = pFilter->b10;
    pFilter->b10 = input;
    pFilter->a11 = pFilter->a10;
    pFilter->a10 = output;
}
