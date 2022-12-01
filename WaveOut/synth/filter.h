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
extern inline void filter_lpf(FILTER* pFilter, double input);

#endif /* __FILTER_H__ */
