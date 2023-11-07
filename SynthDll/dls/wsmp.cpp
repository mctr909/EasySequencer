#include <math.h>
#include "wsmp.h"

double
WSMP_VALUES::getFineTune() {
    return pow(2.0, fineTune / 1200.0);
}

double
WSMP_VALUES::getGain() {
    return pow(10.0, gainInt / (200 * 65536.0));
}
