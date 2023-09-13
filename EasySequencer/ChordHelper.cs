using System;
using System.Collections.Generic;
using System.Linq;

namespace EasySequencer {
    class ChordHelper {
        static readonly string[,] NoteNames = {
            { "C", "Db", "D", "Eb", "E", "F", "Gb", "G", "Ab", "A", "Bb", "B" },
            { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" }
        };
        static readonly string[,] DegreeNames = {
            { "Ⅰ", "♭Ⅱ", "Ⅱ", "♭Ⅲ", "Ⅲ", "Ⅳ", "♭V",  "Ⅴ", "♭Ⅵ", "Ⅵ", "♭Ⅶ", "Ⅶ" },
            { "ⅰ", "♯ⅰ", "ⅱ", "♯ⅱ", "ⅲ", "ⅳ", "♯ⅳ", "ⅴ", "♯ⅴ", "ⅵ", "♭ⅶ", "ⅶ" }
        };
        static readonly int[] KeyIndex = {
            11, 6, 1, 8, 3, 10, 5, 0, 7, 2, 9, 4, 11, 6, 1
        };

        static readonly Tuple<int[], string>[] ChordStructs = new Tuple<int[], string>[] {
            new Tuple<int[], string>(new int[] {  0, 3           }, "m(omit5)"),
            new Tuple<int[], string>(new int[] {  0, 4           }, "(omit5)"),
            new Tuple<int[], string>(new int[] {  0, 7           }, "5"),
            new Tuple<int[], string>(new int[] {  7, 5           }, "5"),
            new Tuple<int[], string>(new int[] {  0, 1, 3, 7, 10 }, "m7(b9)"),
            new Tuple<int[], string>(new int[] {  0, 1, 3, 7, 11 }, "mΔ7(b9)"),
            new Tuple<int[], string>(new int[] {  0, 1, 4, 7, 10 }, "7(b9)"),
            new Tuple<int[], string>(new int[] {  0, 1, 4, 7, 11 }, "Δ7(b9)"),
            new Tuple<int[], string>(new int[] {  0, 2, 3, 7     }, "m(add9)"),
            new Tuple<int[], string>(new int[] {  0, 2, 3, 7, 9  }, "m69"),
            new Tuple<int[], string>(new int[] {  0, 2, 3, 7, 10 }, "m9"),
            new Tuple<int[], string>(new int[] {  0, 2, 3, 7, 11 }, "mΔ9"),
            new Tuple<int[], string>(new int[] {  0, 2, 4, 7     }, "(add9)"),
            new Tuple<int[], string>(new int[] {  0, 2, 4, 7, 9  }, "69"),
            new Tuple<int[], string>(new int[] {  0, 2, 4, 7, 10 }, "9"),
            new Tuple<int[], string>(new int[] {  0, 2, 4, 7, 11 }, "Δ9"),
            new Tuple<int[], string>(new int[] {  0, 2, 7        }, "sus2"),
            new Tuple<int[], string>(new int[] {  0, 3, 4, 7, 10 }, "(#9)"),
            new Tuple<int[], string>(new int[] {  0, 3, 4, 7, 11 }, "Δ(#9)"),
            new Tuple<int[], string>(new int[] {  0, 3, 6        }, "m(b5)"),
            new Tuple<int[], string>(new int[] {  0, 3, 6, 9     }, "dim7"),
            new Tuple<int[], string>(new int[] {  0, 3, 6, 10    }, "m7(b5)"),
            new Tuple<int[], string>(new int[] {  0, 3, 5, 7     }, "m(add11)"),
            new Tuple<int[], string>(new int[] {  0, 3, 5, 7, 10 }, "m7(11)"),
            new Tuple<int[], string>(new int[] {  0, 3, 5, 7, 11 }, "mΔ7(11)"),
            new Tuple<int[], string>(new int[] {  0, 3, 6, 7, 10 }, "m7(#11)"),
            new Tuple<int[], string>(new int[] {  0, 3, 6, 7, 11 }, "mΔ7(#11)"),
            new Tuple<int[], string>(new int[] {  0, 3, 7        }, "m"),
            new Tuple<int[], string>(new int[] {  0, 3, 7, 9     }, "m6"),
            new Tuple<int[], string>(new int[] {  0, 3, 7, 10    }, "m7"),
            new Tuple<int[], string>(new int[] {  0, 3, 7, 11    }, "mΔ7"),
            new Tuple<int[], string>(new int[] {  0, 4, 5, 7     }, "(add11)"),
            new Tuple<int[], string>(new int[] {  0, 4, 5, 7, 10 }, "7(11)"),
            new Tuple<int[], string>(new int[] {  0, 4, 5, 7, 11 }, "Δ7(11)"),
            new Tuple<int[], string>(new int[] {  0, 4, 6, 7, 10 }, "7(#11)"),
            new Tuple<int[], string>(new int[] {  0, 4, 6, 7, 11 }, "Δ7(#11)"),
            new Tuple<int[], string>(new int[] {  0, 4, 7        }, ""),
            new Tuple<int[], string>(new int[] {  0, 4, 7, 9     }, "6"),
            new Tuple<int[], string>(new int[] {  0, 4, 7, 10    }, "7"),
            new Tuple<int[], string>(new int[] {  0, 4, 7, 11    }, "Δ7"),
            new Tuple<int[], string>(new int[] {  0, 4, 8        }, "aug"),
            new Tuple<int[], string>(new int[] {  0, 4, 8, 10    }, "aug7"),
            new Tuple<int[], string>(new int[] {  0, 5, 7        }, "sus4"),
            new Tuple<int[], string>(new int[] {  0, 5, 7, 10    }, "7sus4"),
            new Tuple<int[], string>(new int[] {  2, 1, 5, 10    }, "m"),
            new Tuple<int[], string>(new int[] {  2, 2, 5, 10    }, ""),
            new Tuple<int[], string>(new int[] {  3, 3, 9        }, "m(b5)"),
            new Tuple<int[], string>(new int[] {  3, 4, 6, 9     }, "m6"),
            new Tuple<int[], string>(new int[] {  3, 4, 8, 9     }, "mΔ7"),
            new Tuple<int[], string>(new int[] {  3, 4, 9        }, "m"),
            new Tuple<int[], string>(new int[] {  3, 4, 9, 11    }, "m(add9)"),
            new Tuple<int[], string>(new int[] {  4, 3, 6, 8     }, "7"),
            new Tuple<int[], string>(new int[] {  4, 3, 7, 8     }, "Δ7"),
            new Tuple<int[], string>(new int[] {  4, 3, 8        }, ""),
            new Tuple<int[], string>(new int[] {  4, 3, 8, 10    }, "(add9)"),
            new Tuple<int[], string>(new int[] {  5, 2, 5, 7     }, "7sus4"),
            new Tuple<int[], string>(new int[] {  6, 6, 9        }, "m(b5)"),
            new Tuple<int[], string>(new int[] {  7, 2, 5, 8     }, "m6"),
            new Tuple<int[], string>(new int[] {  7, 3, 5, 8     }, "m7"),
            new Tuple<int[], string>(new int[] {  7, 3, 5, 9     }, "7"),
            new Tuple<int[], string>(new int[] {  7, 3, 5, 10    }, "7sus4"),
            new Tuple<int[], string>(new int[] {  7, 4, 5, 8     }, "mΔ7"),
            new Tuple<int[], string>(new int[] {  7, 4, 5, 9     }, "Δ7"),
            new Tuple<int[], string>(new int[] {  7, 5, 7, 8     }, "m(add9)"),
            new Tuple<int[], string>(new int[] {  7, 5, 7, 9     }, "(add9)"),
            new Tuple<int[], string>(new int[] {  7, 5, 8        }, "m"),
            new Tuple<int[], string>(new int[] {  7, 5, 9        }, ""),
            new Tuple<int[], string>(new int[] {  7, 5, 10       }, "sus4"),
            new Tuple<int[], string>(new int[] { 10, 2, 5, 9     }, "m"),
            new Tuple<int[], string>(new int[] { 10, 2, 6, 9     }, ""),
            new Tuple<int[], string>(new int[] { 10, 2, 7, 9     }, "sus4"),
            new Tuple<int[], string>(new int[] { 11, 1, 4, 8     }, "m"),
            new Tuple<int[], string>(new int[] { 11, 1, 5, 8     }, "")
        };

        public static string[] GetName(int[] notes, int key) {
            key += 7;
            var baseNote = 127;
            foreach (var n in notes) {
                if (n < baseNote) {
                    baseNote = n;
                }
            }
            baseNote %= 12;

            for (int i = 0; i < notes.Length; i++) {
                notes[i] -= baseNote;
                notes[i] %= 12;
            }
            Array.Sort(notes);

            var noteList = new List<int>();
            foreach (var n in notes) {
                if (!noteList.Contains(n)) {
                    noteList.Add(n);
                }
            }

            foreach (var st in ChordStructs) {
                if (noteList.Count != st.Item1.Count()) {
                    continue;
                }
                var isMatch = true;
                for (int i = 1; i < noteList.Count; i++) {
                    if (noteList[i] != st.Item1[i]) {
                        isMatch = false;
                        break;
                    }
                }
                if (isMatch) {
                    var keyOffset = KeyIndex[key];
                    var structure = st.Item2;
                    var root = (12 + baseNote - st.Item1[0]) % 12;
                    var majMin = 0 == structure.IndexOf("m") ? 1 : 0;
                    var degreeName = DegreeNames[majMin, (12 + root - keyOffset) % 12];
                    var sf = 0 <= degreeName.IndexOf("♭") ? 0 : 1;
                    var rootName = NoteNames[sf, root];
                    if (0 == st.Item1[0]) {
                        return new string[] { degreeName, rootName, structure };
                    } else {
                        return new string[] { degreeName, rootName, structure + " on " + NoteNames[sf, baseNote] };
                    }
                }
            }
            return new string[] { "", "", "" };
        }
    }
}
