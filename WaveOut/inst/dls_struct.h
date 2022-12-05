#ifndef __DLS_STRUCT_H__
#define __DLS_STRUCT_H__

#include <math.h>
#include "../type.h"

typedef struct DLS_INSH {
    uint32 regions;
    byte bankLSB;
    byte bankMSB;
    byte reserve1;
    byte bankFlags;
    byte progNum;
    byte reserve2;
    byte reserve3;
    byte reserve4;
} DLS_INSH;

typedef struct DLS_RGNH {
    uint16 keyLow;
    uint16 keyHigh;
    uint16 velocityLow;
    uint16 velocityHigh;
    uint16 options;
    uint16 keyGroup;
    uint16 layer;
} DLS_RGNH;

typedef struct DLS_WLNK {
    uint16 options;
    uint16 phaseGroup;
    uint32 channel;
    uint32 tableIndex;
} DLS_WLNK;

typedef struct DLS_WSMP {
    uint32 size;
    uint16 unityNote;
    int16 fineTune;
    int32 gainInt;
    uint32 options;
    uint32 loopCount;

    double getFileTune() {
        return pow(2.0, fineTune / 1200.0);
    }
    double getGain() {
        return pow(10.0, gainInt / (200 * 65536.0));
    }
} DLS_WSMP;

typedef struct DLS_LOOP {
    uint32 size;
    uint32 type;
    uint32 start;
    uint32 length;
} DLS_LOOP;

enum struct E_DLS_SRC : uint16 {
    // MODULATOR SOURCES
    NONE = 0x0000,
    LFO = 0x0001,
    KEY_ON_VELOCITY = 0x0002,
    KEY_NUMBER = 0x0003,
    EG1 = 0x0004,
    EG2 = 0x0005,
    PITCH_WHEEL = 0x0006,
    POLY_PRESSURE = 0x0007,
    CHANNEL_PRESSURE = 0x0008,
    VIBRATO = 0x0009,

    // MIDI CONTROLLER SOURCES
    CC1 = 0x0081,
    CC7 = 0x0087,
    CC10 = 0x008A,
    CC11 = 0x008B,
    CC91 = 0x00DB,
    CC93 = 0x00DD,

    // REGISTERED PARAMETER NUMBERS
    RPN0 = 0x0100,
    RPN1 = 0x0101,
    RPN2 = 0x0102
};

enum struct E_DLS_DST : uint16 {
    // GENERIC DESTINATIONS
    NONE = 0x0000,
    ATTENUATION = 0x0001,
    RESERVED = 0x0002,
    PITCH = 0x0003,
    PAN = 0x0004,
    KEY_NUMBER = 0x0005,

    // CHANNEL OUTPUT DESTINATIONS
    LEFT = 0x0010,
    RIGHT = 0x0011,
    CENTER = 0x0012,
    LFET_CHANNEL = 0x0013,
    LEFT_REAR = 0x0014,
    RIGHT_REAR = 0x0015,
    CHORUS = 0x0080,
    REVERB = 0x0081,

    // MODULATOR LFO DESTINATIONS
    LFO_FREQUENCY = 0x0104,
    LFO_START_DELAY = 0x0105,

    // VIBRATO LFO DESTINATIONS
    VIB_FREQUENCY = 0x0114,
    VIB_START_DELAY = 0x0115,

    // EG1 DESTINATIONS
    EG1_ATTACK_TIME = 0x0206,
    EG1_DECAY_TIME = 0x0207,
    EG1_RESERVED = 0x0208,
    EG1_RELEASE_TIME = 0x0209,
    EG1_SUSTAIN_LEVEL = 0x020A,
    EG1_DELAY_TIME = 0x020B,
    EG1_HOLD_TIME = 0x020C,
    EG1_SHUTDOWN_TIME = 0x020D,

    // EG2 DESTINATIONS
    EG2_ATTACK_TIME = 0x030A,
    EG2_DECAY_TIME = 0x030B,
    EG2_RESERVED = 0x030C,
    EG2_RELEASE_TIME = 0x030D,
    EG2_SUSTAIN_LEVEL = 0x030E,
    EG2_DELAY_TIME = 0x030F,
    EG2_HOLD_TIME = 0x0310,

    // FILTER DESTINATIONS
    FILTER_CUTOFF = 0x0500,
    FILTER_Q = 0x0501
};

enum struct E_DLS_TRN : uint16 {
    NONE = 0x0000,
    CONCAVE = 0x0001,
    CONVEX = 0x0002,
    SWITCH = 0x0003
};

typedef struct DLS_CONN {
    E_DLS_SRC source;
    E_DLS_SRC control;
    E_DLS_DST destination;
    E_DLS_TRN transform;
    int32 scale;

    double getValue() {
        switch (destination) {
        case E_DLS_DST::ATTENUATION:
        case E_DLS_DST::FILTER_Q:
            return pow(10.0, scale / (200 * 65536.0));

        case E_DLS_DST::PAN:
            return (scale / 655360.0) - 0.5;

        case E_DLS_DST::LFO_START_DELAY:
        case E_DLS_DST::VIB_START_DELAY:
        case E_DLS_DST::EG1_ATTACK_TIME:
        case E_DLS_DST::EG1_DECAY_TIME:
        case E_DLS_DST::EG1_RELEASE_TIME:
        case E_DLS_DST::EG1_DELAY_TIME:
        case E_DLS_DST::EG1_HOLD_TIME:
        case E_DLS_DST::EG1_SHUTDOWN_TIME:
        case E_DLS_DST::EG2_ATTACK_TIME:
        case E_DLS_DST::EG2_DECAY_TIME:
        case E_DLS_DST::EG2_RELEASE_TIME:
        case E_DLS_DST::EG2_DELAY_TIME:
        case E_DLS_DST::EG2_HOLD_TIME:
        {
            auto tmp = pow(2.0, scale / (1200 * 65536.0));
            if (tmp < 0.001) {
                return 0.001;
            } else {
                return tmp;
            }
        }

        case E_DLS_DST::EG1_SUSTAIN_LEVEL:
        case E_DLS_DST::EG2_SUSTAIN_LEVEL:
            return pow(2.0, -0.0005 * scale / 65536.0);

        case E_DLS_DST::PITCH:
        case E_DLS_DST::LFO_FREQUENCY:
        case E_DLS_DST::VIB_FREQUENCY:
        case E_DLS_DST::FILTER_CUTOFF:
            return pow(2.0, (scale / 65536.0 - 6900) / 1200.0) * 440;

        default:
            return 0.0;
        }
    }
} DLS_CONN;

#endif /* __DLS_STRUCT_H__ */
