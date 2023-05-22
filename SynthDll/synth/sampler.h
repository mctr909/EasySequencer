#ifndef __SAMPLER_H__
#define __SAMPLER_H__

#include "synth.h"

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
        double attack = 500.0;
        double hold = 0.0;
        double decay = 500.0;
        double sustain = 1.0;
        double release = 500.0;
    };
    struct EG_CUTOFF {
        double attack = 500.0;
        double hold = 0.0;
        double decay = 500.0;
        double sustain = 1.0;
        double release = 500.0;
        double top = 1.0;
        double fall = 1.0;
        double resonance = 0.0;
    };
    struct EG_PITCH {
        double attack = 500.0;
        double hold = 0.0;
        double decay = 500.0;
        double release = 500.0;
        double top = 1.0;
        double fall = 1.0;
    };

public:
    E_STATE m_state = E_STATE::FREE;
    byte m_channel_num = 0;
    byte m_note_num = 0;
    bool m_loop_enable = false;

private:
    double m_gain = 1.0;
    double m_pitch = 1.0;
    double m_index = 0.0;
    double m_time = 0.0;
    double m_hold = 0.01;
    double m_amp = 0.0;
    double m_epitch = 1.0;
    double m_cutoff = 1.0;
    long m_loop_length = 0;
    long m_loop_end = 0;
    EG_AMP m_eg_amp = { 0 };
    EG_PITCH m_eg_pitch = { 0 };
    EG_CUTOFF m_eg_cutoff = { 0 };
    Synth* mp_synth = nullptr;
    Channel* mp_channel = nullptr;
    WAVE_DATA* mp_wave_data = nullptr;

public:
    Sampler(Synth* p_synth);
    void note_on(Channel* p_channel, INST_LAYER* p_layer, INST_REGION *p_region, byte note_num, byte velocity);
    void step();

private:
    inline void gen_envelope() {
        switch (m_state) {
        case E_STATE::PRESS:
            if (m_time <= m_eg_amp.hold) {
                m_amp += (1.0 - m_amp) * m_eg_amp.attack;
            } else {
                m_amp += (m_eg_amp.sustain - m_amp) * m_eg_amp.decay;
            }
            if (m_time <= m_eg_pitch.hold) {
                m_epitch += (m_eg_pitch.top - m_epitch) * m_eg_pitch.attack;
            } else {
                m_epitch += (1.0 - m_epitch) * m_eg_pitch.decay;
            }
            if (m_time <= m_eg_cutoff.hold) {
                m_cutoff += (m_eg_cutoff.top - m_cutoff) * m_eg_cutoff.attack;
            } else {
                m_cutoff += (m_eg_cutoff.sustain - m_cutoff) * m_eg_cutoff.decay;
            }
            break;
        case E_STATE::RELEASE:
            m_amp -= m_amp * m_eg_amp.release;
            m_epitch += (m_eg_pitch.fall - m_epitch) * m_eg_pitch.release;
            m_cutoff += (m_eg_cutoff.fall - m_cutoff) * m_eg_cutoff.release;
            break;
        case E_STATE::HOLD:
            m_amp -= m_amp * m_hold;
            break;
        case E_STATE::PURGE:
            m_amp -= m_amp * PURGE_SPEED;
            break;
        }
    }
};

#endif /* __SAMPLER_H__ */
