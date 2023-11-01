using System;
using System.Collections.Generic;

namespace EasySequencer {
    class ChordHelper {
        enum I {
            /// <summary>完全1度</summary>
            P1 = 0x0,
            /// <summary>長2度</summary>
            M2 = 0x2,
            /// <summary>長3度</summary>
            M3 = 0x4,
            /// <summary>完全4度</summary>
            P4 = 0x5,
            /// <summary>完全5度</summary>
            P5 = 0x7,
            /// <summary>長6度</summary>
            M6 = 0x9,
            /// <summary>長7度</summary>
            M7 = 0xB,
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
        struct Interval {
            public I Id;
            public int Tone;
            public Interval(I id) {
                Id = id;
                var v = (int)id;
                Tone = v & 0xF;
                if (0 < (v & (int)I.MIN)) {
                    Tone--;
                }
                if (0 < (v & (int)I.DIM)) {
                    Tone--;
                }
                if (0 < (v & (int)I.AUG)) {
                    Tone++;
                }
            }
        }
        struct Structure {
            public Interval[] Intervals;
            public string Name;
            Structure(Interval[] intervals, string name) {
                Intervals = intervals;
                Name = name;
            }
            static Interval[] ToArray(params I[] structure) {
                var list = new List<Interval>();
                for (int i = 0; i < structure.Length; i++) {
                    list.Add(new Interval(structure[i]));
                }
                list.Sort((a, b) => { return a.Tone - b.Tone; });
                return list.ToArray();
            }
            public static readonly Structure[] List = new Structure[] {
                new Structure(ToArray(I.m3), "m(omit5)"),
                new Structure(ToArray(I.M3), "(omit5)"),
                new Structure(ToArray(I.P5), "5"),

                new Structure(ToArray(I.M2, I.P5), "sus2"),
                new Structure(ToArray(I.m3, I.b5), "m(b5)"),
                new Structure(ToArray(I.m3, I.P5), "m"),
                new Structure(ToArray(I.M3, I.b5), "(b5)"),
                new Structure(ToArray(I.M3, I.P5), ""),
                new Structure(ToArray(I.M3, I.s5), "aug"),
                new Structure(ToArray(I.P4, I.P5), "sus4"),

                new Structure(ToArray(I.m3, I.b5, I.b7), "dim7"),
                new Structure(ToArray(I.m3, I.b5, I.m7), "m7(b5)"),
                new Structure(ToArray(I.m3, I.P5, I.M6), "m6"),
                new Structure(ToArray(I.m3, I.P5, I.m7), "m7"),
                new Structure(ToArray(I.m3, I.P5, I.M7), "mΔ7"),
                new Structure(ToArray(I.m3, I.P5, I.M9), "m(add9)"),
                new Structure(ToArray(I.m3, I.P5, I.P11), "m(add11)"),
                new Structure(ToArray(I.M3, I.b5, I.m7), "7(b5)"),
                new Structure(ToArray(I.M3, I.P5, I.M6), "6"),
                new Structure(ToArray(I.M3, I.P5, I.m7), "7"),
                new Structure(ToArray(I.M3, I.P5, I.M7), "Δ7"),
                new Structure(ToArray(I.M3, I.P5, I.M9), "(add9)"),
                new Structure(ToArray(I.M3, I.P5, I.P11), "(add11)"),
                new Structure(ToArray(I.M3, I.s5, I.m7), "aug7"),
                new Structure(ToArray(I.P4, I.P5, I.m7), "7sus4"),

                new Structure(ToArray(I.m3, I.P5, I.M6, I.M9), "m69"),
                new Structure(ToArray(I.m3, I.P5, I.m7, I.m9), "m7(b9)"),
                new Structure(ToArray(I.m3, I.P5, I.m7, I.M9), "m7(9)"),
                new Structure(ToArray(I.m3, I.P5, I.m7, I.P11), "m7(11)"),
                new Structure(ToArray(I.m3, I.P5, I.m7, I.s11), "m7(#11)"),
                new Structure(ToArray(I.m3, I.P5, I.m7, I.m13), "m7(b13)"),
                new Structure(ToArray(I.m3, I.P5, I.m7, I.M13), "m7(13)"),
                new Structure(ToArray(I.m3, I.P5, I.M7, I.M9), "mΔ9"),
                new Structure(ToArray(I.M3, I.P5, I.M6, I.M9), "69"),
                new Structure(ToArray(I.M3, I.P5, I.m7, I.m9), "7(b9)"),
                new Structure(ToArray(I.M3, I.P5, I.m7, I.M9), "7(9)"),
                new Structure(ToArray(I.M3, I.P5, I.m7, I.P11), "7(11)"),
                new Structure(ToArray(I.M3, I.P5, I.m7, I.s11), "7(#11)"),
                new Structure(ToArray(I.M3, I.P5, I.m7, I.m13), "7(b13)"),
                new Structure(ToArray(I.M3, I.P5, I.m7, I.M13), "7(13)"),
                new Structure(ToArray(I.M3, I.P5, I.M7, I.M9), "Δ9")
            };
        }

        public static string[] GetName(int[] notes) {
            var bassTone = 127;
            foreach (var n in notes) {
                if (n < bassTone) {
                    bassTone = n;
                }
            }
            bassTone %= 12;
            for (int i = 0; i < notes.Length; i++) {
                notes[i] -= bassTone;
                notes[i] %= 12;
            }
            Array.Sort(notes);
            var noteList = new List<int>();
            foreach (var n in notes) {
                if (!noteList.Contains(n)) {
                    noteList.Add(n);
                }
            }

            var noteCount = noteList.Count;
            for (var t = 0; t < noteCount; t++) {
                var transList = new int[noteCount - 1];
                for (int i = 0; i < transList.Length; i++) {
                    transList[i] = (noteList[(i + t + 1) % noteCount] - noteList[t] + 12) % 12;
                }
                var rootTone = (bassTone + noteList[t]) % 12;
                foreach (var structure in Structure.List) {
                    if (transList.Length != structure.Intervals.Length) {
                        continue;
                    }
                    var unmatch = false;
                    for (int i = 0; i < structure.Intervals.Length; i++) {
                        if (transList[i] != structure.Intervals[i].Tone) {
                            unmatch = true;
                            break;
                        }
                    }
                    if (unmatch) {
                        continue;
                    }
                    var maj = 0 != structure.Name.IndexOf("m");
                    var root = SCALE.GetName(rootTone, maj);
                    if (t == 0) {
                        return new string[] { root.Degree, root.Tone, structure.Name };
                    } else {
                        var sharp = 1 == root.Tone.IndexOf("#") ||
                            root.Tone == "D" ||
                            root.Tone == "E" ||
                            root.Tone == "A" ||
                            root.Tone == "B";
                        var bass = SCALE.GetName(bassTone, !sharp);
                        return new string[] { root.Degree, root.Tone, structure.Name + " on " + bass.Tone };
                    }
                }
            }
            return new string[] { "", "", "" };
        }
    }
}
