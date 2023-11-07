#ifndef __RIFF_H__
#define __RIFF_H__

#include <stdio.h>
#include <string>
#include <vector>

#include "../type.h"

/******************************************************************************/
class RIFF {
public:
	class INFO {
	public:
		/* ArchivalLocation */
		static const char IARL[5];
		/* Artists */
		static const char IART[5];
		/* Category */
		static const char ICAT[5];
		/* Commissioned */
		static const char ICMS[5];
		/* Comments */
		static const char ICMT[5];
		/* Copyright */
		static const char ICOP[5];
		/* CreationDate */
		static const char ICRD[5];
		/* Engineer */
		static const char IENG[5];
		/* Genre */
		static const char IGNR[5];
		/* Keywords */
		static const char IKEY[5];
		/* Medium */
		static const char IMED[5];
		/* Name */
		static const char INAM[5];
		/* Product */
		static const char IPRD[5];
		/* Software */
		static const char ISFT[5];
		/* Source */
		static const char ISRC[5];
		/* SourceForm */
		static const char ISRF[5];
		/* Subject */
		static const char ISBJ[5];
		/* Technician */
		static const char ITCH[5];

	private:
		struct KV {
			char key[5] = { 0 };
			std::string value;
		};

	private:
		std::vector<KV> m_list;

	public:
		std::string get(const char key[]);
		void set(const char key[], std::string value);
		void copy_from(INFO* p_info);
		void load(FILE* fp, long size);
		size_t write(FILE* fp);

	private:
		uint32 put_text(FILE* fp, KV kv);
	};

private:
	static const char RIFF_ID[5];
	static const char LIST_ID[5];
	static const char INFO_ID[5];
	static const uint32 DEFAULT_SIZE;

public:
	INFO* mp_info;

public:
	RIFF() {
		mp_info = new INFO();
	}

protected:
	E_LOAD_STATUS Load(STRING path, long offset);
	void Load(FILE* fp, long size);
	virtual bool CheckFileType(const char* type, long size) { return false; }
	virtual void LoadChunk(FILE* fp, const char* type, long size) { fseek(fp, size, SEEK_CUR); }
};

#endif /* __RIFF_H__ */
