﻿using System.Collections.Generic;

namespace EasySequencer {
    class ChordHelper {
        struct I {
            /// <summary>完全1度</summary>
            public const byte P1 = 0x0;
            /// <summary>長2度</summary>
            public const byte M2 = 0x2;
            /// <summary>長3度</summary>
            public const byte M3 = 0x4;
            /// <summary>完全4度</summary>
            public const byte P4 = 0x5;
            /// <summary>完全5度</summary>
            public const byte P5 = 0x7;
            /// <summary>長6度</summary>
            public const byte M6 = 0x9;
            /// <summary>長7度</summary>
            public const byte M7 = 0xB;
            /// <summary>短音程</summary>
            public const byte MIN = 0x10;
            /// <summary>短2度</summary>
            public const byte m2 = MIN | M2;
            /// <summary>短3度</summary>
            public const byte m3 = MIN | M3;
            /// <summary>短6度</summary>
            public const byte m6 = MIN | M6;
            /// <summary>短7度</summary>
            public const byte m7 = MIN | M7;
            /// <summary>減音程</summary>
            public const byte DIM = 0x20;
            /// <summary>減1度</summary>
            public const byte b1 = DIM | P1;
            /// <summary>減2度</summary>
            public const byte b2 = DIM | m2;
            /// <summary>減3度</summary>
            public const byte b3 = DIM | m3;
            /// <summary>減4度</summary>
            public const byte b4 = DIM | P4;
            /// <summary>減5度</summary>
            public const byte b5 = DIM | P5;
            /// <summary>減6度</summary>
            public const byte b6 = DIM | m6;
            /// <summary>減7度</summary>
            public const byte b7 = DIM | m7;
            /// <summary>増音程</summary>
            public const byte AUG = 0x40;
            /// <summary>増1度</summary>
            public const byte s1 = AUG | P1;
            /// <summary>増2度</summary>
            public const byte s2 = AUG | M2;
            /// <summary>増3度</summary>
            public const byte s3 = AUG | M3;
            /// <summary>増4度</summary>
            public const byte s4 = AUG | P4;
            /// <summary>増5度</summary>
            public const byte s5 = AUG | P5;
            /// <summary>増6度</summary>
            public const byte s6 = AUG | M6;
            /// <summary>増7度</summary>
            public const byte s7 = AUG | M7;
            /// <summary>テンション</summary>
            public const byte T = 0x80;
            /// <summary>短9度</summary>
            public const byte m9 = T | m2;
            /// <summary>長9度</summary>
            public const byte M9 = T | M2;
            /// <summary>完全11度</summary>
            public const byte P11 = T | P4;
            /// <summary>増11度</summary>
            public const byte s11 = T | s4;
            /// <summary>短13度</summary>
            public const byte m13 = T | m6;
            /// <summary>長13度</summary>
            public const byte M13 = T | M6;

            public readonly byte Id;
            public readonly int Tone;
            public I(byte id) {
                Id = id;
                Tone = id & 0xF;
                if (0 < (id & MIN)) {
                    Tone--;
                }
                if (0 < (id & DIM)) {
                    Tone--;
                }
                if (0 < (id & AUG)) {
                    Tone++;
                }
            }
        }
        struct Structure {
            public I[] Intervals;
            public string[] Names;
            Structure(I[] intervals, params string[] names) {
                Intervals = intervals;
                Names = names;
            }
            static I[] ToArray(params byte[] intervals) {
                var list = new List<I>();
                for (int i = 0; i < intervals.Length; i++) {
                    list.Add(new I(intervals[i]));
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

                new Structure(ToArray(I.m3, I.m7), "m7(omit5)"),
                new Structure(ToArray(I.M3, I.m7), "7(omit5)"),
                new Structure(ToArray(I.P5, I.m7), "7(omit3)"),

                new Structure(ToArray(I.m3, I.P5, I.M6), "m6", "m"),
                new Structure(ToArray(I.m3, I.P5, I.m7), "m7", "m"),
                new Structure(ToArray(I.m3, I.P5, I.M7), "mΔ", "m"),
                new Structure(ToArray(I.m3, I.P5, I.M9), "m(add9)", "m"),
                new Structure(ToArray(I.m3, I.P5, I.P11), "m(add11)", "m"),
                new Structure(ToArray(I.m3, I.b5, I.b7), "o", "m(b5)"),
                new Structure(ToArray(I.m3, I.b5, I.m7), "ø", "m(b5)"),
                new Structure(ToArray(I.M3, I.P5, I.M6), "6", ""),
                new Structure(ToArray(I.M3, I.P5, I.m7), "7", ""),
                new Structure(ToArray(I.M3, I.P5, I.M7), "Δ", ""),
                new Structure(ToArray(I.M3, I.P5, I.M9), "(add9)", ""),
                new Structure(ToArray(I.M3, I.P5, I.P11), "(add11)", ""),
                new Structure(ToArray(I.M3, I.b5, I.m7), "7(b5)", "(b5)"),
                new Structure(ToArray(I.M3, I.s5, I.m7), "aug7", "aug"),
                new Structure(ToArray(I.P4, I.P5, I.m7), "7sus4", "sus4"),

                new Structure(ToArray(I.m3, I.P5, I.M6, I.M9), "m69", "m(add9)", "m6"),
                new Structure(ToArray(I.m3, I.P5, I.m7, I.m9), "m7(b9)"),
                new Structure(ToArray(I.m3, I.P5, I.m7, I.M9), "m9", "m(add9)", "m7"),
                new Structure(ToArray(I.m3, I.P5, I.m7, I.P11), "m7(11)", "m(add11)", "m7"),
                new Structure(ToArray(I.m3, I.P5, I.m7, I.s11), "m7(#11)"),
                new Structure(ToArray(I.m3, I.P5, I.m7, I.m13), "m7(b13)"),
                new Structure(ToArray(I.m3, I.P5, I.m7, I.M13), "m7(13)", "m6", "m7"),
                new Structure(ToArray(I.m3, I.P5, I.M7, I.M9), "mΔ9", "m(add9)", "mΔ"),
                new Structure(ToArray(I.M3, I.P5, I.M6, I.M9), "69", "(add9)", "6"),
                new Structure(ToArray(I.M3, I.P5, I.m7, I.m9), "7(b9)"),
                new Structure(ToArray(I.M3, I.P5, I.m7, I.M9), "9", "(add9)", "7"),
                new Structure(ToArray(I.M3, I.P5, I.m7, I.P11), "7(11)", "(add11)", "7"),
                new Structure(ToArray(I.M3, I.P5, I.m7, I.s11), "7(#11)"),
                new Structure(ToArray(I.M3, I.P5, I.m7, I.m13), "7(b13)"),
                new Structure(ToArray(I.M3, I.P5, I.m7, I.M13), "7(13)", "6", "7"),
                new Structure(ToArray(I.M3, I.P5, I.M7, I.M9), "Δ9", "(add9)", "Δ")
            };
        }

        public static string[] GetName(int[] notes) {
            var bassTone = 127;
            foreach (var note in notes) {
                if (note < bassTone) {
                    bassTone = note;
                }
            }
            bassTone %= 12;
            var toneList = new List<int>();
            foreach (var note in notes) {
                var v = (note - bassTone) % 12;
                if (!toneList.Contains(v)) {
                    toneList.Add(v);
                }
            }
            toneList.Sort();

            var toneCount = toneList.Count;
            for (var t = 0; t < toneCount; t++) {
                var transList = new int[toneCount - 1];
                for (int i = 0; i < transList.Length; i++) {
                    transList[i] = (toneList[(i + t + 1) % toneCount] - toneList[t] + 12) % 12;
                }
                foreach (var structure in Structure.List) {
                    if ((toneCount - 1) != structure.Intervals.Length) {
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
                    var rootTone = (bassTone + toneList[t]) % 12;
                    var root = SCALE.GetName(rootTone, structure.Intervals[0].Id != I.m3);
                    if (t == 0) {
                        return new string[] { root.Degree, root.Tone, structure.Names[0] };
                    } else {
                        var structureName = structure.Names[0];
                        foreach (var interval in structure.Intervals) {
                            if (interval.Tone != (bassTone - rootTone + 12) % 12) {
                                continue;
                            }
                            switch (structure.Names.Length) {
                            case 2:
                                switch (interval.Id) {
                                case I.M6:
                                case I.b7:
                                case I.m7:
                                case I.M7:
                                case I.M9:
                                case I.P11:
                                    structureName = structure.Names[1];
                                    break;
                                }
                                break;
                            case 3:
                                switch (interval.Id) {
                                case I.M6:
                                case I.b7:
                                case I.m7:
                                case I.M7:
                                    structureName = structure.Names[1];
                                    break;
                                case I.M9:
                                case I.P11:
                                case I.M13:
                                    structureName = structure.Names[2];
                                    break;
                                }
                                break;
                            }
                            var v = interval.Id;
                            var flat = 1 != root.Tone.IndexOf("#") && (1 == root.Tone.IndexOf("b") || 0 < (v & I.MIN) || 0 < (v & I.DIM));
                            var bass = SCALE.GetName(bassTone, flat);
                            return new string[] { root.Degree, root.Tone, structureName + " on " + bass.Tone };
                        }
                    }
                }
            }
            return new string[] { "", "", "" };
        }
    }
}
