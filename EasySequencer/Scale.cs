using System.Collections.Generic;
using SMF;

class Scale {
    static readonly int[][] ToneToDeg = new int[][] {
        new int[] { 0 },
        new int[] { 1, 2 },
        new int[] { 3 },
        new int[] { 4, 5 },
        new int[] { 6 },
        new int[] { 7 },
        new int[] { 8, 9 },
        new int[] { 10 },
        new int[] { 11, 12 },
        new int[] { 13 },
        new int[] { 14 },
        new int[] { 15 }
    };
    static readonly Dictionary<E_KEY, Scale> NAMES = new Dictionary<E_KEY, Scale>() {
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
        return NAMES[(E_KEY)((int)key & 0xFF00)];
    }
    public string GetName(int tone, bool maj) {
        var k = (tone - mOffset + 12) % 12;
        var d = ToneToDeg[k];
        if (2 <= d.Length && maj) {
            return mNames[d[1]];
        } else {
            return mNames[d[0]];
        }
    }
}