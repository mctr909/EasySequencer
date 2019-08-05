#pragma once
#include "type.h"

#pragma pack(4)
typedef struct RIFF {
    UInt32 riff;
    UInt32 fileSize;
    UInt32 dataId;
} RIFF;
#pragma

#pragma pack(4)
typedef struct FMT_ {
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

#pragma pack(8)
typedef struct DELAY {
    SInt32 writeIndex;
    SInt32 readIndex;
    double *pTapL;
    double *pTapR;
} DELAY;
#pragma

#pragma pack(8)
typedef struct CHORUS {
    double lfoK;
    double *pPanL;
    double *pPanR;
    double *pLfoRe;
    double *pLfoIm;
} CHORUS;
#pragma

#pragma pack(8)
typedef struct FILTER {
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
#pragma

#pragma pack(8)
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
    byte type;
    byte reserved1;
    byte reserved2;
} WAVE_LOOP;
#pragma

#pragma pack(8)
typedef struct CHANNEL_PARAM {
    double amp;
    double pitch;
    double holdDelta;
    double panLeft;
    double panRight;
    double cutoff;
    double resonance;

    double delayDepth;
    double delayTime;
    double chorusDepth;
    double chorusRate;
} CHANNEL_PARAM;
#pragma

#pragma pack(4)
typedef struct CHANNEL {
    CHANNEL_PARAM *param;

    double wave;
    double waveL;
    double waveR;

    double amp;
    double panLeft;
    double panRight;

    FILTER eq;
    DELAY delay;
    CHORUS chorus;
} CHANNEL;
#pragma

#pragma pack(4)
typedef struct SAMPLER {
    UInt32 channelNo;
    UInt16 noteNo;
    byte keyState;
    bool isActive;

    UInt32 pcmAddr;
    UInt32 pcmLength;

    double gain;
    double delta;

    double index;
    double time;

    double velocity;
    double amp;

    WAVE_LOOP loop;
    ENVELOPE envAmp;
    ENVELOPE envEq;
    FILTER eq;
} SAMPLER;
#pragma
