#pragma once
#include "../type.h"
#include <windows.h>
#include <stdio.h>

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
    uint32 layerIndex = 0;
    uint32 layerCount = 0;
    uint32 artIndex = 0;
    char *pName = NULL;
    char *pCategory = NULL;
} INST_INFO;
#pragma pack()

#pragma pack(4)
typedef struct INST_LIST {
    uint32 count;
    INST_INFO** ppData;
} INST_LIST;
#pragma pack()

#pragma pack(4)
typedef struct INST_LAYER {
    uint32 regionIndex = 0;
    uint32 regionCount = 0;
    uint32 artIndex = 0;
} INST_LAYER;
#pragma pack()

#pragma pack(4)
typedef struct INST_REGION {
    byte keyLow = 0;
    byte keyHigh = 127;
    byte velocityLow = 0;
    byte velocityHigh = 127;
    uint32 waveIndex = 0;
    uint32 artIndex = 0;
    uint32 wsmpIndex = 0;
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
    int16 transpose = 0;
    int16 pan = 0;
    double gain = 1.0;
    double pitch = 1.0;
    INST_ENV env;
} INST_ART;
#pragma pack()

#pragma pack(4)
typedef struct INST_WAVE {
    uint32 offset = 0;
    uint32 sampleRate = 44100;
    uint32 loopBegin = 0;
    uint32 loopLength = 0;
    byte loopEnable = 0;
    byte unityNote = 0;
    uint16 reserved = 0;
    double pitch = 1.0;
    double gain = 1.0;
} INST_WAVE;
#pragma pack()

/******************************************************************************/
class DLS;
class LART;
typedef struct INST_SAMPLER INST_SAMPLER;

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
    uint32 mWaveCount = 0;
    uint32 mLayerCount = 0;
    uint32 mRegionCount = 0;
    uint32 mArtCount = 0;

public:
    InstList();
    ~InstList();

public:
    E_LOAD_STATUS Load(LPWSTR path);
    INST_LIST *GetInstList();
    INST_INFO *GetInstInfo(byte is_drum, byte bank_lsb, byte bank_msb, byte prog_num);
    INST_SAMPLER **GetSamplerPtr();
    WAVDAT *GetWaveTablePtr();
    void SetSampler(INST_INFO *pInstInfo, byte channelNum, byte noteNum, byte velocity);

private:
    E_LOAD_STATUS loadDls(LPWSTR path);
    E_LOAD_STATUS loadDlsWave(DLS *cDls);
    void loadDlsArt(LART *cLart, INST_ART *pArt);
    uint32 writeWaveTable8(FILE *fp, byte* pData, uint32 size);
    uint32 writeWaveTable16(FILE *fp, byte* pData, uint32 size);
    uint32 writeWaveTable24(FILE *fp, byte* pData, uint32 size);
    uint32 writeWaveTable32(FILE *fp, byte* pData, uint32 size);
    uint32 writeWaveTableFloat(FILE *fp, byte* pData, uint32 size);
};
