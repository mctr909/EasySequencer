#include <math.h>
#include "wsmp.h"

double
WSMP_VALUES::fine_tune() {
    return pow(2.0, _fine_tune / 1200.0);
}

double
WSMP_VALUES::gain() {
    return pow(10.0, _gain / (200 * 65536.0));
}
