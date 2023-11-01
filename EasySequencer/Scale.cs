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
            new Degree(0,      "Ⅰ"),
            new Degree(1,   2, "#Ⅰ", "bⅡ"),
            new Degree(3,      "Ⅱ"),
            new Degree(4,   5, "#Ⅱ", "bⅢ"),
            new Degree(6,      "Ⅲ"),
            new Degree(7,      "Ⅳ"),
            new Degree(8,   9, "#Ⅳ", "bⅤ"),
            new Degree(10,     "Ⅴ"),
            new Degree(11, 12, "#Ⅴ", "bⅥ"),
            new Degree(13,     "Ⅵ"),
            new Degree(14, 15, "#Ⅵ", "bⅦ"),
            new Degree(16,     "Ⅶ")
        };
    }
    static readonly Dictionary<E_KEY, SCALE> Scales = new Dictionary<E_KEY, SCALE>() {
        //                               1     #1    b2    2     #2    b3    3     4     #4    b5    5     #5    b6    6     #6    b7    7
        { E_KEY.Cb_MAJOR, new SCALE(11, "Cb", "C",  "C",  "Db", "D",  "D",  "Eb", "Fb", "F",  "F",  "Gb", "G",  "G",  "Ab", "A",  "A",  "Bb" ) },
        { E_KEY.C_MAJOR,  new SCALE(0,  "C",  "C#", "Db", "D",  "D#", "Eb", "E",  "F",  "F#", "Gb", "G",  "G#", "Ab", "A",  "A#", "Bb", "B"  ) },
        { E_KEY.Cs_MAJOR, new SCALE(1,  "C#", "D",  "D",  "D#", "E",  "E",  "E#", "F#", "G",  "G",  "G#", "A",  "A",  "A#", "B",  "B",  "B#" ) },
        { E_KEY.Db_MAJOR, new SCALE(1,  "Db", "D",  "D",  "Eb", "E",  "E",  "F",  "Gb", "G",  "G",  "Ab", "A",  "A",  "Bb", "B",  "B",  "C"  ) },
        { E_KEY.D_MAJOR,  new SCALE(2,  "D",  "D#", "Eb", "E",  "F",  "F",  "F#", "G",  "G#", "Ab", "A",  "A#", "Bb", "B",  "C",  "C",  "C#" ) },
        { E_KEY.Eb_MAJOR, new SCALE(3,  "Eb", "E",  "E",  "F",  "F#", "Gb", "G",  "Ab", "A",  "A",  "Bb", "B",  "B",  "C",  "C#", "Db", "D"  ) },
        { E_KEY.E_MAJOR,  new SCALE(4,  "E",  "F",  "F",  "F#", "G",  "G",  "G#", "A",  "A#", "Bb", "B",  "C",  "C",  "C#", "D",  "D",  "D#" ) },
        { E_KEY.F_MAJOR,  new SCALE(5,  "F",  "F#", "Gb", "G",  "G#", "Ab", "A",  "Bb", "B",  "B",  "C",  "C#", "Db", "D",  "D#", "Eb", "E"  ) },
        { E_KEY.Fs_MAJOR, new SCALE(6,  "F#", "G",  "G",  "G#", "A",  "A",  "A#", "B",  "C",  "C",  "C#", "D",  "D",  "D#", "E",  "E",  "E#" ) },
        { E_KEY.Gb_MAJOR, new SCALE(6,  "Gb", "G",  "G",  "Ab", "A",  "A",  "Bb", "Cb", "C",  "C",  "Db", "D",  "D",  "Eb", "E",  "E",  "F"  ) },
        { E_KEY.G_MAJOR,  new SCALE(7,  "G",  "G#", "Ab", "A",  "A#", "Bb", "B",  "C",  "C#", "Db", "D",  "D#", "Eb", "E",  "F",  "F" , "F#" ) },
        { E_KEY.Ab_MAJOR, new SCALE(8,  "Ab", "A",  "A",  "Bb", "B",  "B",  "C",  "Db", "D",  "D",  "Eb", "E",  "E",  "F",  "F#", "Gb", "G"  ) },
        { E_KEY.A_MAJOR,  new SCALE(9,  "A",  "A#", "Bb", "B",  "C",  "C",  "C#", "D",  "D#", "Eb", "E",  "F",  "F",  "F#", "G",  "G",  "G#" ) },
        { E_KEY.Bb_MAJOR, new SCALE(10, "Bb", "B",  "B",  "C",  "C#", "Db", "D",  "Eb", "E",  "E",  "F",  "F#", "Gb", "G",  "G#", "Ab", "A"  ) },
        { E_KEY.B_MAJOR,  new SCALE(11, "B",  "C",  "C",  "C#", "D",  "D",  "D#", "E",  "F",  "F",  "F#", "G",  "G",  "G#", "A",  "A",  "A#" ) }
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
            .Replace("s", "#")
            .Replace("_MAJOR", "")
             + "/" + keyMin.ToString()
            .Replace("s", "#")
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
    public static Values GetName(int tone, bool flat) {
        var s = mCurrentScale;
        var t = (tone - s.mOffset + 12) % 12;
        var deg = Degree.List[t];
        if (2 <= deg.Index.Length && flat) {
            return new Values(deg.Name[1], s.mNames[deg.Index[1]]);
        } else {
            return new Values(deg.Name[0], s.mNames[deg.Index[0]]);
        }
    }
}
