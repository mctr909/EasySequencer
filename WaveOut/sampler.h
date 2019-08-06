#pragma once
#include "windows.h"
#include "struct.h"

/******************************************************************************/
extern CHANNEL** createChannels(UInt32 count, UInt32 sampleRate);
extern SAMPLER** createSamplers(UInt32 count);

/******************************************************************************/
extern inline void channel(CHANNEL *ch, double *waveL, double *waveR);
extern inline void sampler(CHANNEL **chs, SAMPLER *smpl, LPBYTE pDlsBuffer);

/******************************************************************************/
inline void delay(CHANNEL *ch, DELAY *delay, double *waveL, double *waveR);
inline void chorus(CHANNEL *ch, DELAY *delay, CHORUS *chorus, double *waveL, double *waveR);
inline void filter(FILTER *param, double input);
