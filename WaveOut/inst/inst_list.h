#pragma once
#include "../type.h"
#include <windows.h>
#include <stdio.h>

/******************************************************************************/
#define SAMPLER_COUNT 64

enum struct E_SAMPLER_STATE : byte {
    FREE,
    RESERVED,
    PURGE,
    PRESS,
    RELEASE,
    HOLD
};

/******************************************************************************/
#pragma pack(4)
typedef struct INST_ID {
    byte isDrum = 0;
    byte bankMSB = 0;
    byte bankLSB = 0;
    byte progNum = 0;
} INST_ID;
#pragma pack()

#pragma pack(4)
typedef struct INST_INFO {
    INST_ID id;
    uint layerIndex = 0;
    uint layerCount = 0;
    uint artIndex = 0;
    char *pName = NULL;
    char *pCategory = NULL;
} INST_INFO;
#pragma pack()

#pragma pack(4)
typedef struct INST_LIST {
    uint count;
    INST_INFO** ppData;
} INST_LIST;
#pragma pack()

#pragma pack(4)
typedef struct INST_LAYER {
    uint regionIndex = 0;
    uint regionCount = 0;
    uint artIndex = 0;
} INST_LAYER;
#pragma pack()

#pragma pack(4)
typedef struct INST_REGION {
    byte keyLow = 0;
    byte keyHigh = 127;
    byte velocityLow = 0;
    byte velocityHigh = 127;
    uint waveIndex = 0;
    uint artIndex = 0;
    uint wsmpIndex = 0;
} INST_REGION;
#pragma pack()

typedef struct INST_ENV {
    double ampA = 500.0;
    double ampH = 0.0;
    double ampD = 500.0;
    double ampS = 1.0;
    double ampR = 500.0;

    double cutoffA = 500.0;
    double cutoffH = 0.0;
    double cutoffD = 500.0;
    double cutoffS = 1.0;
    double cutoffR = 500.0;
    double cutoffRise = 1.0;
    double cutoffTop = 1.0;
    double cutoffFall = 1.0;
    double resonance = 0.0;

    double pitchA = 500.0;
    double pitchR = 500.0;
    double pitchRise = 1.0;
    double pitchFall = 1.0;
} INST_ENV;

#pragma pack(4)
typedef struct INST_ART {
    short transpose = 0;
    short pan = 0;
    double gain = 1.0;
    double pitch = 1.0;
    INST_ENV env;
} INST_ART;
#pragma pack()

#pragma pack(4)
typedef struct INST_WAVE {
    uint offset = 0;
    uint sampleRate = 44100;
    uint loopBegin = 0;
    uint loopLength = 0;
    byte loopEnable = 0;
    byte unityNote = 0;
    ushort reserved = 0;
    double pitch = 1.0;
    double gain = 1.0;
} INST_WAVE;
#pragma pack()

#pragma pack(4)
typedef struct INST_SAMPLER {
    E_SAMPLER_STATE state = E_SAMPLER_STATE::FREE;
    byte channelNum = 0;
    byte noteNum = 0;
    byte reserved1 = 0;
    short pan = 0;
    short reserved2 = 0;
    double gain = 1.0;
    double index = 0.0;
    double time = 0.0;
    double pitch = 1.0;
    double egAmp = 0.0;
    double egCutoff = 1.0;
    double egPitch = 1.0;
    INST_ENV *pEnv = NULL;
    INST_WAVE *pWave = NULL;
} INST_SAMPLER;
#pragma pack()

/******************************************************************************/
class DLS;
class LART;

/******************************************************************************/
class InstList {
private:
    INST_SAMPLER **mppSampler = NULL;
    INST_WAVE **mppWaveList = NULL;
    INST_LAYER **mppLayerList = NULL;
    INST_REGION **mppRegionList = NULL;
    INST_ART **mppArtList = NULL;
    WAVDAT *mpWaveTable = NULL;
    INST_LIST mInstList;
    WCHAR mWaveTablePath[256] = { 0 };
    uint mWaveCount = 0;
    uint mLayerCount = 0;
    uint mRegionCount = 0;
    uint mArtCount = 0;

public:
    InstList(LPWSTR path);
    ~InstList();

public:
    INST_LIST *GetInstList();
    INST_INFO *GetInstInfo(INST_ID *id);
    INST_SAMPLER **GetSamplerPtr();
    WAVDAT *GetWaveTablePtr();
    void SetSampler(INST_INFO *pInstInfo, byte channelNum, byte noteNum, byte velocity);

private:
    void loadDls(LPWSTR path);
    void loadDlsWave(DLS *cDls);
    void loadDlsArt(LART *cLart, INST_ART *pArt);
    uint writeWaveTable8(FILE *fp, byte* pData, uint size);
    uint writeWaveTable16(FILE *fp, byte* pData, uint size);
    uint writeWaveTable24(FILE *fp, byte* pData, uint size);
    uint writeWaveTable32(FILE *fp, byte* pData, uint size);
    uint writeWaveTableFloat(FILE *fp, byte* pData, uint size);
};
