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

size_t
RIFF::INFO::write(FILE* fp) {
    if (0 == m_list.size()) {
        return 0;
    }
    fwrite(LIST_ID, sizeof(LIST_ID) - 1, 1, fp);
    fwrite(&DEFAULT_SIZE, sizeof(DEFAULT_SIZE), 1, fp);
    fwrite(INFO_ID, sizeof(INFO_ID) - 1, 1, fp);
    uint32 size = 4;
    for (int i = 0; i < m_list.size(); i++) {
        size += put_text(fp, m_list[i]);
    }
    fseek(fp, -static_cast<long>(size), SEEK_CUR);
    fwrite(&size, sizeof(size), 1, fp);
    fseek(fp, size - 4, SEEK_CUR);
    return size + sizeof(DEFAULT_SIZE) + sizeof(LIST_ID) - 1;
}

uint32
RIFF::INFO::put_text(FILE* fp, KV kv) {
    auto length = static_cast<int32>(kv.value.size());
    auto pad = 2 - (length % 2);
    string value = kv.value.c_str();
    for (int i = 0; i < pad; ++i) {
        value += '\0';
    }
    auto bytes = static_cast<uint32>(value.size());
    fwrite(kv.key, sizeof(kv.key) - 1, 1, fp);
    fwrite(&length, sizeof(length), 1, fp);
    fwrite(value.c_str(), bytes, 1, fp);
    return static_cast<uint32>(bytes + sizeof(length) + sizeof(kv.key) - 1);
}

/******************************************************************************/
const char RIFF::RIFF_ID[] = "RIFF";
const char RIFF::LIST_ID[] = "LIST";
const char RIFF::INFO_ID[] = "INFO";
const uint32 RIFF::DEFAULT_SIZE = 0xFFFFFFFF;

E_LOAD_STATUS
RIFF::Load(STRING path, long offset) {
    FILE *fp = nullptr;
    _wfopen_s(&fp, path, L"rb");
    if (nullptr == fp) {
        return E_LOAD_STATUS::FILE_OPEN_FAILED;
    }

    fseek(fp, offset, SEEK_SET);

    char riffId[5] = { 0 };
    unsigned int riffSize;
    char riffType[5] = { 0 };

    fread_s(&riffId, 4, 4, 1, fp);
    fread_s(&riffSize, 4, 4, 1, fp);
    fread_s(&riffType, 4, 4, 1, fp);

    if (0 == strcmp(RIFF_ID, riffId) && CheckFileType(riffType, riffSize)) {
        Load(fp, riffSize - 4);
    } else {
        return E_LOAD_STATUS::UNKNOWN_FILE;
    }

    fclose(fp);

    return E_LOAD_STATUS::SUCCESS;
}

void
RIFF::Load(FILE* fp, long size) {
    char chunkId[5] = { 0 };
    unsigned int chunkSize;
    char listType[5] = { 0 };

    long pos = 0;
    while (pos < size) {
        fread_s(&chunkId, 4, 4, 1, fp);
        fread_s(&chunkSize, 4, 4, 1, fp);
        pos += chunkSize + 8;

        if (0 == strcmp(LIST_ID, chunkId)) {
            fread_s(&listType, 4, 4, 1, fp);
            if (0 == strcmp(INFO_ID, listType)) {
                mp_info->load(fp, chunkSize - 4);
            } else {
                LoadChunk(fp, listType, chunkSize - 4);
            }
        } else {
            LoadChunk(fp, chunkId, chunkSize);
        }
    }
}
