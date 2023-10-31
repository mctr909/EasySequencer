using SMF;
using System.Collections.Generic;

class SCALE {
    struct Degree {
        public readonly int[] Index;
        public readonly string[] Name;
        Degree(int index, string name) {
            Index = new int[] { index };
            Name = new string[] { name };
        }
        Degree(int iSharp, int iFlat, string nSharp, string nFlat) {
            Index = new int[] { iSharp, iFlat };
            Name = new string[] { nSharp, nFlat };
        }
        public static readonly Degree[] List = new Degree[] {
            new Degree(0,      "I"),
            new Degree(1,   2, "#I",  "bII"),
            new Degree(3,      "II"),
            new Degree(4,   5, "#II", "bIII"),
            new Degree(6,      "III"),
            new Degree(7,      "IV"),
            new Degree(8,   9, "#IV", "bV"),
            new Degree(10,     "V"),
            new Degree(11, 12, "#V",  "bVI"),
            new Degree(13,     "VI"),
            new Degree(14,     "bVII"),
            new Degree(15,     "VII")
        };
    }
    static readonly Dictionary<E_KEY, SCALE> Scales = new Dictionary<E_KEY, SCALE>() {
        //                               1     #1    b2    2     #2    b3    3     4     #4    b5    5     #5    b6    6     b7    7
        { E_KEY.Cb_MAJOR, new SCALE(11, "Cb", "C",  "C",  "Db", "D",  "D",  "Eb", "Fb", "F",  "F",  "Gb", "G",  "G",  "Ab", "A",  "Bb" ) },
        { E_KEY.C_MAJOR,  new SCALE(0,  "C",  "C#", "Db", "D",  "D#", "Eb", "E",  "F",  "F#", "Gb", "G",  "G#", "Ab", "A",  "Bb", "B"  ) },
        { E_KEY.Cs_MAJOR, new SCALE(1,  "C#", "D",  "D",  "D#", "E",  "E",  "E#", "F#", "G",  "G",  "G#", "A",  "A",  "A#", "B",  "B#" ) },
        { E_KEY.Db_MAJOR, new SCALE(1,  "Db", "D",  "D",  "Eb", "E",  "E",  "F",  "Gb", "G",  "G",  "Ab", "A",  "A",  "Bb", "B",  "C"  ) },
        { E_KEY.D_MAJOR,  new SCALE(2,  "D",  "D#", "Eb", "E",  "F",  "F",  "F#", "G",  "G#", "Ab", "A",  "A#", "Bb", "B",  "C",  "C#" ) },
        { E_KEY.Eb_MAJOR, new SCALE(3,  "Eb", "E",  "E",  "F",  "F#", "Gb", "G",  "Ab", "A",  "A",  "Bb", "B",  "B",  "C",  "Db", "D"  ) },
        { E_KEY.E_MAJOR,  new SCALE(4,  "E",  "F",  "F",  "F#", "G",  "G",  "G#", "A",  "A#", "Bb", "B",  "C",  "C",  "C#", "D",  "D#" ) },
        { E_KEY.F_MAJOR,  new SCALE(5,  "F",  "F#", "Gb", "G",  "G#", "Ab", "A",  "Bb", "B",  "B",  "C",  "C#", "Db", "D",  "Eb", "E"  ) },
        { E_KEY.Fs_MAJOR, new SCALE(6,  "F#", "G",  "G",  "G#", "A",  "A",  "A#", "B",  "C",  "C",  "C#", "D",  "D",  "D#", "E",  "E#" ) },
        { E_KEY.Gb_MAJOR, new SCALE(6,  "Gb", "G",  "G",  "Ab", "A",  "A",  "Bb", "Cb", "C",  "C",  "Db", "D",  "D",  "Eb", "E",  "F"  ) },
        { E_KEY.G_MAJOR,  new SCALE(7,  "G",  "G#", "Ab", "A",  "A#", "Bb", "B",  "C",  "C#", "Db", "D",  "D#", "Eb", "E",  "F" , "F#" ) },
        { E_KEY.Ab_MAJOR, new SCALE(8,  "Ab", "A",  "A",  "Bb", "B",  "B",  "C",  "Db", "D",  "D",  "Eb", "E",  "E",  "F",  "Gb", "G"  ) },
        { E_KEY.A_MAJOR,  new SCALE(9,  "A",  "A#", "Bb", "B",  "C",  "C",  "C#", "D",  "D#", "Eb", "E",  "F",  "F",  "F#", "G",  "G#" ) },
        { E_KEY.Bb_MAJOR, new SCALE(10, "Bb", "B",  "B",  "C",  "C#", "Db", "D",  "Eb", "E",  "E",  "F",  "F#", "Gb", "G",  "Ab", "A"  ) },
        { E_KEY.B_MAJOR,  new SCALE(11, "B",  "C",  "C",  "C#", "D",  "D",  "D#", "E",  "F",  "F",  "F#", "G",  "G",  "G#", "A",  "A#" ) }
    };

    int mOffset;
    string[] mNames;
    SCALE(int offset, params string[] names) {
        mOffset = offset;
        mNames = names;
    }

    static E_KEY mCurrentKey = E_KEY.C_MAJOR;
    static SCALE mCurrentScale = Scales[E_KEY.C_MAJOR];
    public static string KeyName { get; private set; } = GetKeyName();

    static string GetKeyName() {
        var keyMaj = (E_KEY)((int)mCurrentKey & 0xFF00);
        var keyMin = (E_KEY)(((int)mCurrentKey & 0xFF00) | 1);
        return keyMaj.ToString()
            .Replace("S", "#")
            .Replace("_MAJOR", "")
             + "/" + keyMin.ToString()
            .Replace("S", "#")
            .Replace("_MINOR", "m");
    }

    public static void SetKey(E_KEY key) {
        if (key != mCurrentKey) {
            mCurrentKey = key;
            var ikey = (E_KEY)((int)key & 0xFF00);
            mCurrentScale = Scales[ikey];
            KeyName = GetKeyName();
        }
    }
    public struct Values {
        public readonly string Degree;
        public readonly string Tone;
        public Values(string degree, string tone) {
            Degree = degree;
            Tone = tone;
        }
    }
    public static Values GetName(int tone, bool maj) {
        var s = mCurrentScale;
        var t = (tone - s.mOffset + 12) % 12;
        var deg = Degree.List[t];
        if (2 <= deg.Index.Length && maj) {
            return new Values(deg.Name[1], s.mNames[deg.Index[1]]);
        } else {
            return new Values(deg.Name[0], s.mNames[deg.Index[0]]);
        }
    }
}
