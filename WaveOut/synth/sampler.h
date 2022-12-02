#ifndef __SAMPLER_H__
#define __SAMPLER_H__

#include "../type.h"

/******************************************************************************/
#define SAMPLER_COUNT 128

/******************************************************************************/
typedef struct INST_LAYER INST_LAYER;
typedef struct INST_REGION INST_REGION;
typedef struct INST_ENV INST_ENV;
class Synth;
class Channel;

/******************************************************************************/
class Sampler {
public:
    enum struct E_STATE : byte {
        FREE,
        RESERVED,
        PURGE,
        PRESS,
        RELEASE,
        HOLD
    };

public:
    E_STATE state = E_STATE::FREE;
    byte channel_num = 0;
    byte note_num = 0;
    bool loop_enable = false;

private:
    double m_gain = 1.0;
    double m_pitch = 1.0;
    double m_index = 0.0;
    double m_time = 0.0;
    double m_eg_amp = 0.0;
    double m_eg_pitch = 1.0;
    double m_eg_cutoff = 1.0;
    long m_loop_length = 0;
    long m_loop_end = 0;
    Synth* mp_synth = nullptr;
    Channel* mp_channel = nullptr;
    INST_ENV* mp_eg = nullptr;
    WAVE_DATA* mp_wave_data = nullptr;

public:
    Sampler(Synth* p_synth);
    void note_on(Channel* p_channel, INST_LAYER* p_layer, INST_REGION *p_region, byte note_num, byte velocity);
    void step();
};

#endif /* __SAMPLER_H__ */
