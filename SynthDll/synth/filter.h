#ifndef __FILTER_H__
#define __FILTER_H__

/******************************************************************************/
#pragma pack(push, 8)
struct FILTER {
    double cut;
    double res;
    double a00;
    double b00;
    double a01;
    double b01;
    double a10;
    double b10;
    double a11;
    double b11;
};
#pragma pack(pop)

/******************************************************************************/
inline void filter_lpf(FILTER* pFilter, double input) {
    #define ADJUST    0.975
    #define PI        3.14159265
    #define INV_FACT2 5.00000000e-01
    #define INV_FACT3 1.66666667e-01
    #define INV_FACT4 4.16666667e-02
    #define INV_FACT5 8.33333333e-03
    #define INV_FACT6 1.38888889e-03
    #define INV_FACT7 1.98412698e-04
    #define INV_FACT8 2.48015873e-05
    #define INV_FACT9 2.75573192e-06
    /* sin, cosの近似 */
    double c, s;
    {
        double rad = pFilter->cut * (PI * ADJUST);
        double rad_2 = rad * rad;
        c = INV_FACT8;
        c *= rad_2;
        c -= INV_FACT6;
        c *= rad_2;
        c += INV_FACT4;
        c *= rad_2;
        c -= INV_FACT2;
        c *= rad_2;
        c++;
        s = INV_FACT9;
        s *= rad_2;
        s -= INV_FACT7;
        s *= rad_2;
        s += INV_FACT5;
        s *= rad_2;
        s -= INV_FACT3;
        s *= rad_2;
        s++;
        s *= rad;
    }

    /* IIRローパスフィルタ パラメータ */
    double ka0, ka1, kb0, kb1;
    {
        double alpha = s / (pFilter->res * 4.0 + 1.0);
        double alpha_1 = alpha + 1.0;
        ka0 = -2.0 * c / alpha_1;
        kb0 = (1.0 - c) / alpha_1;
        ka1 = (1.0 - alpha) / alpha_1;
        kb1 = kb0 * 0.5;
    }

    /** フィルタ1段目 **/
    double output =
        kb1 * input
        + kb0 * pFilter->b00
        + kb1 * pFilter->b01
        - ka0 * pFilter->a00
        - ka1 * pFilter->a01;
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
        - ka1 * pFilter->a11;
    pFilter->b11 = pFilter->b10;
    pFilter->b10 = input;
    pFilter->a11 = pFilter->a10;
    pFilter->a10 = output;
}

#endif /* __FILTER_H__ */