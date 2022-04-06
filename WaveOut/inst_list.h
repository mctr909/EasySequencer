#pragma once
#include <windows.h>
#include <stdio.h>

#define SAMPLER_COUNT 64

enum struct E_KEY_STATE : unsigned char {
    FREE,
    RESERVED,
    PURGE,
    PRESS,
    RELEASE,
    HOLD
};

#pragma pack(4)
typedef struct INST_ID {
    unsigned char isDrum = 0;
    unsigned char bankMSB = 0;
    unsigned char bankLSB = 0;
    unsigned char progNum = 0;
} INST_ID;
#pragma pack()

#pragma pack(4)
typedef struct INST_INFO {
    INST_ID id;
    unsigned int layerIndex = 0;
    unsigned int layerCount = 0;
    unsigned int artIndex = 0;
    char name[32] = { 0 };
    char category[32] = { 0 };
} INST_INFO;
#pragma pack()

#pragma pack(4)
typedef struct INST_LIST {
    unsigned int count;
    INST_INFO** ppData;
} INST_LIST;
#pragma pack()

#pragma pack(4)
typedef struct INST_LAYER {
    unsigned int regionIndex = 0;
    unsigned int regionCount = 0;
    unsigned int artIndex = 0;
} INST_LAYER;
#pragma pack()

#pragma pack(4)
typedef struct INST_REGION {
    unsigned char keyLow = 0;
    unsigned char keyHigh = 127;
    unsigned char velocityLow = 0;
    unsigned char velocityHigh = 127;
    unsigned int waveIndex = 0;
    unsigned int artIndex = 0;
} INST_REGION;
#pragma pack()

typedef struct INST_ENV {
    double ampA = 0.5;
    double ampH = 0.0;
    double ampD = 0.5;
    double ampS = 1.0;
    double ampR = 0.5;

    double cutoffA = 0.5;
    double cutoffH = 0.0;
    double cutoffD = 0.5;
    double cutoffS = 1.0;
    double cutoffR = 0.5;
    double cutoffRise = 1.0;
    double cutoffTop = 1.0;
    double cutoffFall = 1.0;
    double resonance = 0.0;

    double pitchA = 0.5;
    double pitchR = 0.5;
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
    unsigned int offset = 0;
    unsigned int sampleRate = 44100;
    unsigned int loopBegin = 0;
    unsigned int loopLength = 0;
    unsigned char loopEnable = 0;
    unsigned char unityNote = 0;
    unsigned short reserved = 0;
    double pitch = 1.0;
    double gain = 1.0;
} INST_WAVE;
#pragma pack()

#pragma pack(4)
typedef struct INST_SAMPLER {
    E_KEY_STATE state = E_KEY_STATE::FREE;
    unsigned char channelNum = 0;
    unsigned char noteNum = 0;
    unsigned char reserved1 = 0;
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

class DLS;
class LART;

class InstList {
private:
    INST_SAMPLER **mppSampler = NULL;
    INST_WAVE **mppWaveList = NULL;
    INST_LAYER **mppLayerList = NULL;
    INST_REGION **mppRegionList = NULL;
    INST_ART **mppArtList = NULL;
    short *mpWaveTable = NULL;
    INST_LIST mInstList;
    WCHAR mWaveTablePath[256] = { 0 };
    unsigned int mWaveTableSize = 0;
    unsigned int mWaveCount = 0;
    unsigned int mLayerCount = 0;
    unsigned int mRegionCount = 0;
    unsigned int mArtCount = 0;

public:
    InstList(LPWSTR path);
    ~InstList();

public:
    INST_LIST *GetInstList();
    INST_INFO *GetInstInfo(INST_ID *id);
    INST_SAMPLER **GetSamplerPtr();
    short *GetWaveTablePtr();
    void SetSampler(INST_INFO *pInstInfo, unsigned char  channelNum, unsigned char noteNum, unsigned char velocity);

private:
    void loadDls(LPWSTR path);
    void loadDlsWave(DLS *cDls);
    void loadDlsArt(LART *cLart, INST_ART *pArt);
    unsigned int writeWaveTable8(FILE *fp, unsigned char* pData, unsigned int size);
    unsigned int writeWaveTable16(FILE *fp, unsigned char* pData, unsigned int size);
    unsigned int writeWaveTable24(FILE *fp, unsigned char* pData, unsigned int size);
    unsigned int writeWaveTable32(FILE *fp, unsigned char* pData, unsigned int size);
    unsigned int writeWaveTableFloat(FILE *fp, unsigned char* pData, unsigned int size);
};
