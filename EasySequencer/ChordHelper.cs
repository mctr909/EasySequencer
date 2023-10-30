using System;
using System.Collections.Generic;
using System.Linq;

namespace EasySequencer {
    class ChordHelper {
        enum I {
            /// <summary>完全1度</summary>
            P1 = 0,
            /// <summary>長2度</summary>
            M2 = 2,
            /// <summary>長3度</summary>
            M3 = 4,
            /// <summary>完全4度</summary>
            P4 = 5,
            /// <summary>完全5度</summary>
            P5 = 7,
            /// <summary>長6度</summary>
            M6 = 9,
            /// <summary>長7度</summary>
            M7 = 11,
            /// <summary>短音程</summary>
            MIN = 0x10,
            /// <summary>短2度</summary>
            m2 = MIN | M2,
            /// <summary>短3度</summary>
            m3 = MIN | M3,
            /// <summary>短6度</summary>
            m6 = MIN | M6,
            /// <summary>短7度</summary>
            m7 = MIN | M7,
            /// <summary>減音程</summary>
            DIM = 0x20,
            /// <summary>減1度</summary>
            b1 = DIM | P1,
            /// <summary>減2度</summary>
            b2 = DIM | m2,
            /// <summary>減3度</summary>
            b3 = DIM | m3,
            /// <summary>減4度</summary>
            b4 = DIM | P4,
            /// <summary>減5度</summary>
            b5 = DIM | P5,
            /// <summary>減6度</summary>
            b6 = DIM | m6,
            /// <summary>減7度</summary>
            b7 = DIM | m7,
            /// <summary>増音程</summary>
            AUG = 0x40,
            /// <summary>増1度</summary>
            s1 = AUG | P1,
            /// <summary>増2度</summary>
            s2 = AUG | M2,
            /// <summary>増3度</summary>
            s3 = AUG | M3,
            /// <summary>増4度</summary>
            s4 = AUG | P4,
            /// <summary>増5度</summary>
            s5 = AUG | P5,
            /// <summary>増6度</summary>
            s6 = AUG | M6,
            /// <summary>増7度</summary>
            s7 = AUG | M7,
            /// <summary>テンション</summary>
            T = 0x80,
            /// <summary>短9度</summary>
            m9 = T | m2,
            /// <summary>長9度</summary>
            M9 = T | M2,
            /// <summary>完全11度</summary>
            P11 = T | P4,
            /// <summary>増11度</summary>
            s11 = T | s4,
            /// <summary>短13度</summary>
            m13 = T | m6,
            /// <summary>長13度</summary>
            M13 = T | M6
        }
        static readonly Tuple<I[], string>[] Structs = new Tuple<I[], string>[] {
            new Tuple<I[], string>(new I[] { I.m3 }, "m(omit5)"),
            new Tuple<I[], string>(new I[] { I.M3 }, "(omit5)"),
            new Tuple<I[], string>(new I[] { I.P5 }, "5"),

            new Tuple<I[], string>(new I[] { I.M2, I.P5 }, "sus2"),
            new Tuple<I[], string>(new I[] { I.m3, I.b5 }, "m(b5)"),
            new Tuple<I[], string>(new I[] { I.m3, I.P5 }, "m"),
            new Tuple<I[], string>(new I[] { I.M3, I.b5 }, "(b5)"),
            new Tuple<I[], string>(new I[] { I.M3, I.P5 }, ""),
            new Tuple<I[], string>(new I[] { I.M3, I.s5 }, "aug"),
            new Tuple<I[], string>(new I[] { I.P4, I.P5 }, "sus4"),

            new Tuple<I[], string>(new I[] { I.m3, I.b5, I.b7 }, "dim7"),
            new Tuple<I[], string>(new I[] { I.m3, I.b5, I.m7 }, "m7(b5)"),
            new Tuple<I[], string>(new I[] { I.m3, I.P5, I.M6 }, "m6"),
            new Tuple<I[], string>(new I[] { I.m3, I.P5, I.m7 }, "m7"),
            new Tuple<I[], string>(new I[] { I.m3, I.P5, I.M7 }, "mΔ7"),
            new Tuple<I[], string>(new I[] { I.m3, I.P5, I.M9 }, "m(add9)"),
            new Tuple<I[], string>(new I[] { I.m3, I.P5, I.P11 }, "m(add11)"),
            new Tuple<I[], string>(new I[] { I.M3, I.b5, I.m7 }, "7(b5)"),
            new Tuple<I[], string>(new I[] { I.M3, I.P5, I.M6 }, "6"),
            new Tuple<I[], string>(new I[] { I.M3, I.P5, I.m7 }, "7"),
            new Tuple<I[], string>(new I[] { I.M3, I.P5, I.M7 }, "Δ7"),
            new Tuple<I[], string>(new I[] { I.M3, I.P5, I.M9 }, "(add9)"),
            new Tuple<I[], string>(new I[] { I.M3, I.P5, I.P11 }, "(add11)"),
            new Tuple<I[], string>(new I[] { I.M3, I.s5, I.m7 }, "aug7"),
            new Tuple<I[], string>(new I[] { I.P4, I.P5, I.m7 }, "7sus4"),

            new Tuple<I[], string>(new I[] { I.m3, I.P5, I.M6, I.M9 }, "m69"),
            new Tuple<I[], string>(new I[] { I.m3, I.P5, I.m7, I.m9 }, "m7(b9)"),
            new Tuple<I[], string>(new I[] { I.m3, I.P5, I.m7, I.M9 }, "m7(9)"),
            new Tuple<I[], string>(new I[] { I.m3, I.P5, I.m7, I.P11 }, "m7(11)"),
            new Tuple<I[], string>(new I[] { I.m3, I.P5, I.m7, I.s11 }, "m7(#11)"),
            new Tuple<I[], string>(new I[] { I.m3, I.P5, I.m7, I.m13 }, "m7(b13)"),
            new Tuple<I[], string>(new I[] { I.m3, I.P5, I.m7, I.M13 }, "m7(13)"),
            new Tuple<I[], string>(new I[] { I.m3, I.P5, I.M7, I.M9 }, "mΔ9"),
            new Tuple<I[], string>(new I[] { I.M3, I.P5, I.M6, I.M9 }, "69"),
            new Tuple<I[], string>(new I[] { I.M3, I.P5, I.m7, I.m9 }, "7(b9)"),
            new Tuple<I[], string>(new I[] { I.M3, I.P5, I.m7, I.M9 }, "7(9)"),
            new Tuple<I[], string>(new I[] { I.M3, I.P5, I.m7, I.P11 }, "7(11)"),
            new Tuple<I[], string>(new I[] { I.M3, I.P5, I.m7, I.s11 }, "7(#11)"),
            new Tuple<I[], string>(new I[] { I.M3, I.P5, I.m7, I.m13 }, "7(b13)"),
            new Tuple<I[], string>(new I[] { I.M3, I.P5, I.m7, I.M13 }, "7(13)"),
            new Tuple<I[], string>(new I[] { I.M3, I.P5, I.M7, I.M9 }, "Δ9")
        };

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
