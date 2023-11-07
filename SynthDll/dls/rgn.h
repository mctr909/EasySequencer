#ifndef __DLS_RGN_H__
#define __DLS_RGN_H__

#include "../type.h"
#include "../riff.h"

class RGN;
class LART;
struct WSMP_VALUES;
struct WSMP_LOOP;

class LRGN : public RIFF {
public:
    int32 Count = 0;
    RGN **pcRegion = nullptr;

public:
    LRGN(FILE *fp, long size, int32 count);
    ~LRGN();

protected:
    void LoadChunk(FILE *fp, const char *type, long size) override;
};

class RGN : public RIFF {
public:
    struct RGNH {
        uint16 keyLow;
        uint16 keyHigh;
        uint16 velocityLow;
        uint16 velocityHigh;
        uint16 options;
        uint16 keyGroup;
        uint16 layer;
    };
    struct WLNK {
        uint16 options;
        uint16 phaseGroup;
        uint32 channel;
        uint32 tableIndex;
    };

public:
    RGNH Header = { 0 };
    WLNK WaveLink = { 0 };
    WSMP_VALUES *pWaveSmpl = nullptr;
    WSMP_LOOP **ppWaveLoop = nullptr;
    LART *cLart = nullptr;

public:
    RGN(FILE *fp, long size);
    ~RGN();

protected:
    void LoadChunk(FILE *fp, const char *type, long size) override;
};

#endif /* __DLS_RGN_H__ */
