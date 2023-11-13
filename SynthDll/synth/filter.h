#ifndef __FILTER_H__
#define __FILTER_H__

#include <math.h>

/******************************************************************************/
#pragma pack(push, 8)
struct LPF24 {
    float l[6] = { 0 };
    float r[6] = { 0 };
};
#pragma pack(pop)

/******************************************************************************/
inline void filter_lpf24(LPF24* pFilter, double cutoff, double resonance, double left, double right) {
    /* IIRローパスフィルタ パラメータ */
    float ka1, ka2, kb1, kb2;
    {
        /* sin, cosの近似 */
        float c, s;
        {
            constexpr auto ADJUST = 0.975f;
            constexpr auto PI = 3.14159265f;
            constexpr auto INV_FACT2 = 5.00000000e-01f;
            constexpr auto INV_FACT3 = 1.66666667e-01f;
            constexpr auto INV_FACT4 = 4.16666667e-02f;
            constexpr auto INV_FACT5 = 8.33333333e-03f;
            constexpr auto INV_FACT6 = 1.38888889e-03f;
            constexpr auto INV_FACT7 = 1.98412698e-04f;
            constexpr auto INV_FACT8 = 2.48015873e-05f;
            constexpr auto INV_FACT9 = 2.75573192e-06f;
            float rad = (float)cutoff * (PI * ADJUST);
            float rad_2 = rad * rad;
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
        float alpha = s / ((float)resonance * 4.0f + 1.0f);
        float alpha_1 = 1.0f / (alpha + 1.0f);
        ka1 = -2.0f * c * alpha_1;
        kb1 = (1.0f - c) * alpha_1;
        ka2 = (1.0f - alpha) * alpha_1;
        kb2 = kb1 * 0.5f;
    }

    /** フィルタ **/
    {
        float l1
            = kb2 * ((float)left + pFilter->l[0])
            + kb1 * pFilter->l[1]
            - ka2 * pFilter->l[2]
            - ka1 * pFilter->l[3];
        float l2
            = kb2 * (l1 + pFilter->l[2])
            + kb1 * pFilter->l[3]
            - ka2 * pFilter->l[4]
            - ka1 * pFilter->l[5];
        float r1
            = kb2 * ((float)right + pFilter->r[0])
            + kb1 * pFilter->r[1]
            - ka2 * pFilter->r[2]
            - ka1 * pFilter->r[3];
        float r2
            = kb2 * (r1 + pFilter->r[2])
            + kb1 * pFilter->r[3]
            - ka2 * pFilter->r[4]
            - ka1 * pFilter->r[5];
        pFilter->l[0] = pFilter->l[1];
        pFilter->l[1] = (float)left;
        pFilter->l[2] = pFilter->l[3];
        pFilter->l[3] = l1;
        pFilter->l[4] = pFilter->l[5];
        pFilter->l[5] = l2;
        pFilter->r[0] = pFilter->r[1];
        pFilter->r[1] = (float)right;
        pFilter->r[2] = pFilter->r[3];
        pFilter->r[3] = r1;
        pFilter->r[4] = pFilter->r[5];
        pFilter->r[5] = r2;
    }
}

#endif /* __FILTER_H__ */