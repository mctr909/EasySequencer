#pragma once
typedef struct {
    double cut; //   0
    double res; //   8
    double bi;  //  16
    double a0;  //  24
    double b0;  //  32
    double a1;  //  40
    double b1;  //  48
    double a2;  //  56
    double b2;  //  64
    double a3;  //  72
    double b3;  //  80
    double a4;  //  88
    double b4;  //  96
    double a5;  // 104
    double b5;  // 112
    double a6;  // 120
    double b6;  // 128
    double a7;  // 136
} FILTER;

extern inline void filter_exec(FILTER *filter, double input);
