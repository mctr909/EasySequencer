using System.Collections.Generic;

namespace ChordHelper {
	public class Chord {
		struct Interval {
			public readonly I Id;
			public readonly int Tone;
			public Interval(I id) {
				Id = id;
				var v = (byte)id;
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
			public string[] Names;
			Structure(Interval[] intervals, params string[] names) {
				Intervals = intervals;
				Names = names;
			}
			static Interval[] ToArray(params I[] intervals) {
				var list = new List<Interval>();
				for (int i = 0; i < intervals.Length; i++) {
					list.Add(new Interval(intervals[i]));
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
				new Structure(ToArray(I.M3, I.s5), "(#5)"),
				new Structure(ToArray(I.P4, I.P5), "sus4"),

				new Structure(ToArray(I.m3, I.m7), "m7(omit5)"),
				new Structure(ToArray(I.M3, I.m7), "7(omit5)"),
				new Structure(ToArray(I.P5, I.m7), "7(omit3)"),

				new Structure(ToArray(I.m3, I.P5, I.M6), "m6", "m"),
				new Structure(ToArray(I.m3, I.P5, I.m7), "m7", "m"),
				new Structure(ToArray(I.m3, I.P5, I.M7), "mΔ7", "m"),
				new Structure(ToArray(I.m3, I.P5, I.M9), "m(add9)", "m"),
				new Structure(ToArray(I.m3, I.P5, I.P11), "m(add11)", "m"),
				new Structure(ToArray(I.m3, I.b5, I.b7), "dim7", "m(b5)"),
				new Structure(ToArray(I.m3, I.b5, I.m7), "m7(b5)", "m(b5)"),
				new Structure(ToArray(I.M3, I.P5, I.M6), "6", ""),
				new Structure(ToArray(I.M3, I.P5, I.m7), "7", ""),
				new Structure(ToArray(I.M3, I.P5, I.M7), "Δ7", ""),
				new Structure(ToArray(I.M3, I.P5, I.M9), "(add9)", ""),
				new Structure(ToArray(I.M3, I.P5, I.P11), "(add11)", ""),
				new Structure(ToArray(I.M3, I.b5, I.m7), "7(b5)", "(b5)"),
				new Structure(ToArray(I.M3, I.s5, I.m7), "7(#5)", "(#5)"),
				new Structure(ToArray(I.P4, I.P5, I.m7), "7sus4", "sus4"),

				new Structure(ToArray(I.m3, I.P5, I.M6, I.M9), "m69", "m(add9)", "m6"),
				new Structure(ToArray(I.m3, I.P5, I.m7, I.m9), "m7(b9)"),
				new Structure(ToArray(I.m3, I.P5, I.m7, I.M9), "m9", "m(add9)", "m7"),
				new Structure(ToArray(I.m3, I.P5, I.m7, I.P11), "m7(11)", "m(add11)", "m7"),
				new Structure(ToArray(I.m3, I.P5, I.m7, I.s11), "m7(#11)"),
				new Structure(ToArray(I.m3, I.P5, I.m7, I.m13), "m7(b13)"),
				new Structure(ToArray(I.m3, I.P5, I.m7, I.M13), "m7(13)", "m6", "m7"),
				new Structure(ToArray(I.m3, I.P5, I.M7, I.M9), "mΔ9", "m(add9)", "mΔ7"),
				new Structure(ToArray(I.M3, I.P5, I.M6, I.M9), "69", "(add9)", "6"),
				new Structure(ToArray(I.M3, I.P5, I.m7, I.m9), "7(b9)"),
				new Structure(ToArray(I.M3, I.P5, I.m7, I.M9), "9", "(add9)", "7"),
				new Structure(ToArray(I.M3, I.P5, I.m7, I.P11), "7(11)", "(add11)", "7"),
				new Structure(ToArray(I.M3, I.P5, I.m7, I.s11), "7(#11)"),
				new Structure(ToArray(I.M3, I.P5, I.m7, I.m13), "7(b13)"),
				new Structure(ToArray(I.M3, I.P5, I.m7, I.M13), "7(13)", "6", "7"),
				new Structure(ToArray(I.M3, I.P5, I.M7, I.M9), "Δ9", "(add9)", "Δ7")
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
					var sharp = structure.Intervals[0].Id == I.m3 && rootTone != 10;
					var root = Scale.GetName(rootTone, !sharp);
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
							var bass = Scale.GetName(bassTone, flat);
							return new string[] { root.Degree, root.Tone, structureName + " on " + bass.Tone };
						}
					}
				}
			}
			return new string[] { "-", "-", "" };
		}
	}
}
