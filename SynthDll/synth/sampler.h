#ifndef __SAMPLER_H__
#define __SAMPLER_H__

#include "synth.h"
#include "filter.h"

/******************************************************************************/
typedef struct INST_LAYER INST_LAYER;
typedef struct INST_REGION INST_REGION;
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

private:
    struct EG_AMP {
        double value = 0.0;
        double attack = 0.5;
        double decay = 0.5;
        double release = 0.5;
        double sustain = 1.0;
        bool enable_decay = false;
    };
    struct EG_LPF {
        double value = 1.0;
        double attack = 0.5;
        double decay = 0.5;
        double release = 0.5;
        double top = 1.0;
        double sustain = 1.0;
        double fall = 1.0;
        double resonance = 0.0;
        bool enable_decay = false;
    };
    struct EG_PITCH {
        double value = 1.0;
        double attack = 0.5;
        double decay = 0.5;
        double release = 0.5;
        double top = 1.0;
        double fall = 1.0;
        bool enable_decay = false;
    };
    struct WAVE_INFO {
        long loop_length = 0;
        long loop_end = 0;
        bool enable_loop = false;
        double index = 0.0;
        WAVE_DATA* p_data = nullptr;
    };

public:
    E_STATE m_state = E_STATE::FREE;
    byte m_channel_num = 0;
    byte m_note_num = 0;
    WAVE_INFO m_wave_info = { 0 };

private:
    double m_gain = 1.0;
    double m_tune = 1.0;
    double m_hold = 0.01;
    double m_current_pan_re = 1.0;
    double m_current_pan_im = 0.0;

    EG_AMP m_eg_amp = { 0 };
    EG_PITCH m_eg_pitch = { 0 };
    EG_LPF m_eg_lpf = { 0 };
    LPF24 m_filter = { 0 };
    Synth* mp_synth = nullptr;
    Channel* mp_channel = nullptr;

public:
    Sampler(Synth* p_synth);
    ~Sampler();
    void note_on(Channel* p_channel, INST_LAYER* p_layer, INST_REGION *p_region, byte note_num, byte velocity);
    void step();
};

#endif /* __SAMPLER_H__ */
