using SMF;
using System.Collections.Generic;

class Scale {
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
    static readonly Dictionary<E_KEY, Scale> Scales = new Dictionary<E_KEY, Scale>() {
        //                               1     #1    b2    2     #2    b3    3     4     #4    b5    5     #5    b6    6     b7    7
        { E_KEY.CF_MAJOR, new Scale(11, "Cb", "C",  "C",  "Db", "D",  "D",  "Eb", "Fb", "F",  "F",  "Gb", "G",  "G",  "Ab", "A",  "Bb" ) },
        { E_KEY.C_MAJOR,  new Scale(0,  "C",  "C#", "Db", "D",  "D#", "Eb", "E",  "F",  "F#", "Gb", "G",  "G#", "Ab", "A",  "Bb", "B"  ) },
        { E_KEY.CS_MAJOR, new Scale(1,  "C#", "D",  "D",  "D#", "E",  "E",  "E#", "F#", "G",  "G",  "G#", "A",  "A",  "A#", "B",  "B#" ) },
        { E_KEY.DF_MAJOR, new Scale(1,  "Db", "D",  "D",  "Eb", "E",  "E",  "F",  "Gb", "G",  "G",  "Ab", "A",  "A",  "Bb", "B",  "C"  ) },
        { E_KEY.D_MAJOR,  new Scale(2,  "D",  "D#", "Eb", "E",  "F",  "F",  "F#", "G",  "G#", "Ab", "A",  "A#", "Bb", "B",  "C",  "C#" ) },
        { E_KEY.EF_MAJOR, new Scale(3,  "Eb", "E",  "E",  "F",  "F#", "Gb", "G",  "Ab", "A",  "A",  "Bb", "B",  "B",  "C",  "Db", "D"  ) },
        { E_KEY.E_MAJOR,  new Scale(4,  "E",  "F",  "F",  "F#", "G",  "G",  "G#", "A",  "A#", "Bb", "B",  "C",  "C",  "C#", "D",  "D#" ) },
        { E_KEY.F_MAJOR,  new Scale(5,  "F",  "F#", "Gb", "G",  "G#", "Ab", "A",  "Bb", "B",  "B",  "C",  "C#", "Db", "D",  "Eb", "E"  ) },
        { E_KEY.FS_MAJOR, new Scale(6,  "F#", "G",  "G",  "G#", "A",  "A",  "A#", "B",  "C",  "C",  "C#", "D",  "D",  "D#", "E",  "E#" ) },
        { E_KEY.GF_MAJOR, new Scale(6,  "Gb", "G",  "G",  "Ab", "A",  "A",  "Bb", "Cb", "C",  "C",  "Db", "D",  "D",  "Eb", "E",  "F"  ) },
        { E_KEY.G_MAJOR,  new Scale(7,  "G",  "G#", "Ab", "A",  "A#", "Bb", "B",  "C",  "C#", "Db", "D",  "D#", "Eb", "E",  "F" , "F#" ) },
        { E_KEY.AF_MAJOR, new Scale(8,  "Ab", "A",  "A",  "Bb", "B",  "B",  "C",  "Db", "D",  "D",  "Eb", "E",  "E",  "F",  "Gb", "G"  ) },
        { E_KEY.A_MAJOR,  new Scale(9,  "A",  "A#", "Bb", "B",  "C",  "C",  "C#", "D",  "D#", "Eb", "E",  "F",  "F",  "F#", "G",  "G#" ) },
        { E_KEY.BF_MAJOR, new Scale(10, "Bb", "B",  "B",  "C",  "C#", "Db", "D",  "Eb", "E",  "E",  "F",  "F#", "Gb", "G",  "Ab", "A"  ) },
        { E_KEY.B_MAJOR,  new Scale(11, "B",  "C",  "C",  "C#", "D",  "D",  "D#", "E",  "F",  "F",  "F#", "G",  "G",  "G#", "A",  "A#" ) }
    };

    int mOffset;
    string[] mNames;
    Scale(int offset, params string[] names) {
        mOffset = offset;
        mNames = names;
    }

    public static Scale Get(E_KEY key) {
        return Scales[(E_KEY)((int)key & 0xFF00)];
    }

    public struct Values {
        public readonly string Degree;
        public readonly string Tone;
        public Values(string degree, string tone) {
            Degree = degree;
            Tone = tone;
        }
    }
    public Values GetName(int tone, bool maj) {
        var t = (tone - mOffset + 12) % 12;
        var deg = Degree.List[t];
        if (2 <= deg.Index.Length && maj) {
            return new Values(deg.Name[1], mNames[deg.Index[1]]);
        } else {
            return new Values(deg.Name[0], mNames[deg.Index[0]]);
        }
    }
}
