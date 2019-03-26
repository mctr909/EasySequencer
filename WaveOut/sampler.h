#pragma once
#include "struct.h"

/******************************************************************************/
LPBYTE loadDLS(LPWSTR filePath, UInt32 *size, UInt32 sampleRate);
CHANNEL** createChannels(UInt32 count);
SAMPLER** createSamplers(UInt32 count);

/******************************************************************************/
inline extern void channel(CHANNEL *ch, double *waveL, double *waveR);
inline extern void sampler(CHANNEL **chs, SAMPLER *smpl);

/******************************************************************************/
inline void delay(CHANNEL *ch, DELAY *delay);
inline void chorus(CHANNEL *ch, DELAY *delay, CHORUS *chorus);
//inline void filter(FILTER *filter, double input);
