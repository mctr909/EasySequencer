#include "riff.h"

using std::string;
using std::vector;

/******************************************************************************/
const char RIFF::INFO::IARL[] = "IARL";
const char RIFF::INFO::IART[] = "IART";
const char RIFF::INFO::ICAT[] = "ICAT";
const char RIFF::INFO::ICMS[] = "ICMS";
const char RIFF::INFO::ICMT[] = "ICMT";
const char RIFF::INFO::ICOP[] = "ICOP";
const char RIFF::INFO::ICRD[] = "ICRD";
const char RIFF::INFO::IENG[] = "IENG";
const char RIFF::INFO::IGNR[] = "IGNR";
const char RIFF::INFO::IKEY[] = "IKEY";
const char RIFF::INFO::IMED[] = "IMED";
const char RIFF::INFO::INAM[] = "INAM";
const char RIFF::INFO::IPRD[] = "IPRD";
const char RIFF::INFO::ISFT[] = "ISFT";
const char RIFF::INFO::ISRC[] = "ISRC";
const char RIFF::INFO::ISRF[] = "ISRF";
const char RIFF::INFO::ISBJ[] = "ISBJ";
const char RIFF::INFO::ITCH[] = "ITCH";

void
RIFF::INFO::load(FILE* fp, long size) {
    char type[5] = { 0 };
    int text_size;
    long pos = 0;
    while (pos < size) {
        fread_s(type, 4, 4, 1, fp);
        fread_s(&text_size, 4, 4, 1, fp);

        text_size += (0 == text_size % 2) ? 0 : 1;
        pos += text_size + 8;

        auto buff = new char[text_size + 1];
        memset(buff, '\0', text_size + 1);
        fread_s(buff, text_size, text_size, 1, fp);

        string value = buff;
        set(type, value);
        delete[] buff;
    }
}

string
RIFF::INFO::get(const char key[]) {
    for (auto i = static_cast<int>(m_list.size()) - 1; 0 <= i; i--) {
        if (0 == strcmp(m_list[i].key, key)) {
            return m_list[i].value;
        }
    }
    return "";
}

void
RIFF::INFO::set(const char key[], string value) {
    for (auto i = static_cast<int>(m_list.size()) - 1; 0 <= i; i--) {
        if (0 == strcmp(m_list[i].key, key)) {
            m_list[i].value = value.c_str();
            return;
        }
    }
    KV kv;
    strcpy_s(kv.key, key);
    kv.value = value.c_str();
    m_list.push_back(kv);
}

void
RIFF::INFO::copy_from(INFO* p_info) {
    m_list.clear();
    for (auto i = static_cast<int>(p_info->m_list.size()) - 1; 0 <= i; i--) {
        KV kv;
        strcpy_s(kv.key, p_info->m_list[i].key);
        kv.value = p_info->m_list[i].value.c_str();
        m_list.push_back(kv);
    }
}

/******************************************************************************/
const char RIFF::RIFF_ID[] = "RIFF";
const char RIFF::LIST_ID[] = "LIST";
const char RIFF::INFO_ID[] = "INFO";

E_LOAD_STATUS
RIFF::load(STRING path, long offset) {
    FILE* fp = nullptr;
    _wfopen_s(&fp, path, L"rb");
    if (nullptr == fp) {
        return E_LOAD_STATUS::FILE_OPEN_FAILED;
    }

    fseek(fp, offset, SEEK_SET);

    char riff[5] = { 0 };
    uint32 size;
    char type[5] = { 0 };

    fread_s(&riff, 4, 4, 1, fp);
    fread_s(&size, 4, 4, 1, fp);
    fread_s(&type, 4, 4, 1, fp);

    if (0 == strcmp(RIFF_ID, riff) && check_file_type(type, size)) {
        load(fp, size - 4);
    } else {
        return E_LOAD_STATUS::UNKNOWN_FILE;
    }

    fclose(fp);

    return E_LOAD_STATUS::SUCCESS;
}

void
RIFF::load(FILE* fp, long size) {
    char chunk_type[5] = { 0 };
    uint32 chunk_size;
    char list_type[5] = { 0 };
    long pos = 0;
    while (pos < size) {
        fread_s(&chunk_type, 4, 4, 1, fp);
        fread_s(&chunk_size, 4, 4, 1, fp);
        pos += chunk_size + 8;

        if (0 == strcmp(LIST_ID, chunk_type)) {
            fread_s(&list_type, 4, 4, 1, fp);
            if (0 == strcmp(INFO_ID, list_type)) {
                mp_info->load(fp, chunk_size - 4);
            } else {
                load_chunk(fp, list_type, chunk_size - 4);
            }
        } else {
            load_chunk(fp, chunk_type, chunk_size);
        }
    }
}
