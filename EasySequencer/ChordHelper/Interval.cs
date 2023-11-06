namespace ChordHelper {
	public enum I {
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
		/// <summary>増9度</summary>
		s9 = T | s2,
		/// <summary>完全11度</summary>
		P11 = T | P4,
		/// <summary>増11度</summary>
		s11 = T | s4,
		/// <summary>短13度</summary>
		m13 = T | m6,
		/// <summary>長13度</summary>
		M13 = T | M6
	}
}
