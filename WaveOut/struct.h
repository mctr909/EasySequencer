#pragma once
#include "filter.h"

typedef unsigned    int     UInt32;
typedef signed      int     SInt32;
typedef unsigned    short   UInt16;
typedef signed      short   SInt16;
typedef unsigned    char    bool;

#define true    ((bool)1)
#define false   ((bool)0)

#pragma pack(4)
typedef struct DELAY {
    double depth;
    double rate;
    double *pTapL;
    double *pTapR;
    SInt32 writeIndex;
    SInt32 readIndex;
} DELAY;
#pragma

#pragma pack(4)
typedef struct CHORUS {
    double depth;
    double rate;
    double lfoK;
    double *pPanL;
    double *pPanR;
    double *pLfoRe;
    double *pLfoIm;
} CHORUS;
#pragma

#pragma pack(4)
typedef struct ENVELOPE {
    double levelA;
    double levelD;
    double levelS;
    double levelR;
    double deltaA;
    double deltaD;
    double deltaR;
    double hold;
} ENVELOPE;
#pragma

#pragma pack(4)
typedef struct WAVE_LOOP {
    UInt32 start;
    UInt32 length;
    bool enable;
    byte reserved1;
    byte reserved2;
    byte reserved3;
} WAVE_LOOP;
#pragma

#pragma pack(4)
typedef struct CHANNEL {
    double wave;
    double waveL;
    double waveR;

    double pitch;
    double holdDelta;

    double panLeft;
    double panRight;

    double tarCutoff;
    double tarResonance;

    double tarAmp;
    double curAmp;

    FILTER eq;
    DELAY delay;
    CHORUS chorus;
} CHANNEL;
#pragma

#pragma pack(4)
typedef struct SAMPLER {
    UInt32 channelNo;
    UInt16 noteNo;
    bool onKey;
    bool isActive;

    UInt32 pcmAddr;
    UInt32 pcmLength;

    double gain;
    double delta;

    double index;
    double time;

    double tarAmp;
    double curAmp;

    WAVE_LOOP loop;
    ENVELOPE envAmp;
    ENVELOPE envEq;
    FILTER eq;
} SAMPLER;
#pragma

#pragma pack(4)
typedef struct {
    UInt32 riff;
    UInt32 fileSize;
    UInt32 dataId;
} RIFF;
#pragma

#pragma pack(4)
typedef struct {
    UInt32 chunkId;
    UInt32 chunkSize;
    UInt16 formatId;
    UInt16 channels;
    UInt32 sampleRate;
    UInt32 bytePerSec;
    UInt16 blockAlign;
    UInt16 bitPerSample;
    UInt32 dataId;
    UInt32 dataSize;
} FMT_;
#pragma