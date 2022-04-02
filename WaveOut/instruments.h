#pragma once
#include "type.h"

#pragma pack(push, 8)
typedef struct ENV_AMP {
    double attack;
    double hold;
    double decay;
    double sustain;
    double release;
} ENV_AMP;
#pragma pack(pop)

#pragma pack(push, 8)
typedef struct ENV_FILTER {
    double attack;
    double hold;
    double decay;
    double sustain;
    double release;
    double rise;
    double top;
    double fall;
} ENV_FILTER;
#pragma pack(pop)

#pragma pack(push, 8)
typedef struct ENV_PITCH {
    double attack;
    double release;
    double rise;
    double fall;
} ENV_PITCH;
#pragma pack(pop)

#pragma pack(push, 8)
typedef struct WAVE_INFO {
    uint waveOfs;
    uint loopBegin;
    uint loopLength;
    Bool loopEnable;
    byte unityNote;
    ushort reserved;
    double gain;
    double delta;
} WAVE_INFO;
#pragma pack(pop)

#pragma pack(push, 4)
typedef struct INST_ID {
    byte isDrum;
    byte programNo;
    byte bankMSB;
    byte bankLSB;
} INST_ID;
#pragma pack(pop)

#pragma pack(push, 4)
typedef struct REGION {
    byte keyLo;
    byte keyHi;
    byte velLo;
    byte velHi;
    WAVE_INFO waveInfo;
    ENV_AMP envAmp;
    ENV_FILTER envFilter;
    ENV_PITCH envPitch;
} REGION;
#pragma pack(pop)

#pragma pack(push, 4)
typedef struct INST_INFO {
    INST_ID id;
    int regionCount;
    byte* pName;
    byte* pCategory;
    REGION **ppRegions;
} INST_INFO;
#pragma pack(pop)

#pragma pack(push, 4)
typedef struct INST_LIST {
    int instCount;
    INST_INFO** ppInst;
} INST_LIST;
#pragma pack(pop)
