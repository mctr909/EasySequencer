namespace MIDI {
    public enum FORMAT : ushort {
        FORMAT0 = 0x0000,
        FORMAT1 = 0x0001,
        FORMAT2 = 0x0002,
        INVALID = 0xFFFF
    }

    public enum EVENT_TYPE : byte {
        INVALID  = 0x00,
        NOTE_OFF = 0x80,
        NOTE_ON  = 0x90,
        POLY_KEY = 0xA0,
        CTRL_CHG = 0xB0,
        PRGM_CHG = 0xC0,
        CH_PRESS = 0xD0,
        PITCH    = 0xE0,
        SYS_EX   = 0xF0,
        META     = 0xFF
    }

    public enum CTRL_TYPE : byte {
        BANK_MSB   = 0,
        MODULATION = 1,
        PORTA_TIME = 5,
        DATA_MSB   = 6,
        VOLUME     = 7,
        PAN        = 10,
        EXPRESSION = 11,
        BANK_LSB   = 32,
        HOLD       = 64,
        PORTAMENTO = 65,
        RESONANCE  = 71,
        RELEACE    = 72,
        ATTACK     = 73,
        CUTOFF     = 74,
        VIB_RATE   = 76,
        VIB_DEPTH  = 77,
        VIB_DELAY  = 78,
        REVERB     = 91,
        CHORUS     = 93,
        DELAY      = 94,
        NRPN_LSB   = 98,
        NRPN_MSB   = 99,
        RPN_LSB    = 100,
        RPN_MSB    = 101,
        ALL_RESET  = 121
    }

    public enum META_TYPE : byte {
        SEQ_NO    = 0x00,
        TEXT      = 0x01,
        COMPOSER  = 0x02,
        SEQ_NAME  = 0x03,
        INST_NAME = 0x04,
        LYRIC     = 0x05,
        MARKER    = 0x06,
        QUEUE     = 0x07,
        PRG_NAME  = 0x08,
        DEVICE    = 0x09,
        CH_PREFIX = 0x20,
        PORT      = 0x21,
        TRACK_END = 0x2F,
        TEMPO     = 0x51,
        SMPTE     = 0x54,
        MEASURE   = 0x58,
        KEY       = 0x59,
        META      = 0x7F,
        INVALID   = 0xFF
    }

    public enum KEY : ushort {
        //
        G_MAJOR  = 0x0100,
        E_MINOR  = 0x0101,
        D_MAJOR  = 0x0200,
        B_MINOR  = 0x0201,
        A_MAJOR  = 0x0300,
        Fs_MINOR = 0x0301,
        E_MAJOR  = 0x0400,
        Cs_MINOR = 0x0401,
        B_MAJOR  = 0x0500,
        Gs_MINOR = 0x0501,
        Fs_MAJOR = 0x0600,
        Ds_MINOR = 0x0601,
        Cs_MAJOR = 0x0700,
        As_MINOR = 0x0701,
        //
        C_MAJOR = 0x0000,
        A_MINOR = 0x0001,
        //
        F_MAJOR  = 0xFF00,
        D_MINOR  = 0xFF01,
        Bb_MAJOR = 0xFE00,
        G_MINOR  = 0xFE01,
        Eb_MAJOR = 0xFD00,
        C_MINOR  = 0xFD01,
        Ab_MAJOR = 0xFC00,
        F_MINOR  = 0xFC01,
        Db_MAJOR = 0xFB00,
        Bb_MINOR = 0xFB01,
        Gb_MAJOR = 0xFA00,
        Eb_MINOR = 0xFA01,
        Cb_MAJOR = 0xF900,
        Ab_MINOR = 0xF901
    }
}
