#pragma once

/******************************************************************************/
#pragma pack(8)
typedef struct FILTER {
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
} FILTER;
#pragma

/******************************************************************************/
extern inline void filter(FILTER *pFilter, double input);
