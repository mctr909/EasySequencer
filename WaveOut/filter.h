#pragma once
typedef struct {
    double cut; //   0
    double res; //   8
    double a0;  //  16
    double b0;  //  24
    double a1;  //  32
    double b1;  //  40
    double a2;  //  48
    double b2;  //  56
    double a3;  //  64
    double b3;  //  72
} FILTER;

extern inline void filter_exec(FILTER *filter, double input);
