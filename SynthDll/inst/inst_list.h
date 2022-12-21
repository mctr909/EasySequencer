#ifndef __INST_LIST_H__
#define __INST_LIST_H__

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

typedef struct INST_EG_AMP {
    double attack = 500.0;
    double hold = 0.0;
    double decay = 500.0;
    double sustain = 1.0;
    double release = 500.0;
} INST_EG_AMP;

typedef struct INST_EG_CUTOFF {
    double attack = 500.0;
    double hold = 0.0;
    double decay = 500.0;
    double sustain = 1.0;
    double release = 500.0;
    double rise = 1.0;
    double top = 1.0;
    double fall = 1.0;
    double resonance = 0.0;
} INST_EG_CUTOFF;

typedef struct INST_EG_PITCH {
    double attack = 500.0;
    double hold = 0.0;
    double decay = 500.0;
    double release = 500.0;
    double rise = 1.0;
    double top = 1.0;
    double fall = 1.0;
} INST_EG_PITCH;

#pragma pack(4)
typedef struct INST_ART {
    int16 transpose = 0;
    int16 pan = 0;
    double gain = 1.0;
    double pitch = 1.0;
    INST_EG_AMP eg_amp;
    INST_EG_CUTOFF eg_cutoff;
    INST_EG_PITCH eg_pitch;
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

/******************************************************************************/
class InstList {
private:
    INST_LIST mInstList;
    WCHAR mWaveTablePath[256] = { 0 };
    uint32 mWaveCount = 0;
    uint32 mLayerCount = 0;
    uint32 mRegionCount = 0;
    uint32 mArtCount = 0;

public:
    INST_WAVE** mppWaveList = NULL;
    INST_LAYER** mppLayerList = NULL;
    INST_REGION** mppRegionList = NULL;
    INST_ART** mppArtList = NULL;
    WAVE_DATA* mpWaveTable = NULL;

public:
    InstList() {}
    ~InstList();

public:
    E_LOAD_STATUS Load(LPWSTR path);
    INST_LIST *GetInstList();
    INST_INFO *GetInstInfo(byte is_drum, byte bank_lsb, byte bank_msb, byte prog_num);

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

#endif /* __INST_LIST_H__ */
