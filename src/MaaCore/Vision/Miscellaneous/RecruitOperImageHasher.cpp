#include "RecruitOperImageHasher.h"

#include <fstream>
#include <sstream>

#include <openssl/sha.h>

#include "Config/Miscellaneous/RecruitConfig.h"
#include "Utils/Logger.hpp"

namespace asst
{
namespace
{
// 共享 Levenshtein + pHash 实现：复制自 RecruitResultImageAnalyzer.cpp，
// Phase 3 重构时统一抽到 RecruitHashUtils.{h,cpp}（TODO）
int levenshtein_distance(const std::string& a, const std::string& b)
{
    const int n = static_cast<int>(a.size());
    const int m = static_cast<int>(b.size());
    if (n == 0) return m;
    if (m == 0) return n;

    std::vector<int> prev(m + 1), curr(m + 1);
    for (int j = 0; j <= m; ++j) prev[j] = j;
    for (int i = 1; i <= n; ++i) {
        curr[0] = i;
        for (int j = 1; j <= m; ++j) {
            const int cost = (a[i - 1] == b[j - 1]) ? 0 : 1;
            curr[j] = std::min({ prev[j] + 1, curr[j - 1] + 1, prev[j - 1] + cost });
        }
        std::swap(prev, curr);
    }
    return prev[m];
}

uint64_t hash_to_uint64(const std::string& hex)
{
    if (hex.size() < 16) return 0;
    uint64_t v = 0;
    for (size_t i = 0; i < 16; ++i) {
        char c = hex[i];
        uint64_t nibble = 0;
        if (c >= '0' && c <= '9') nibble = c - '0';
        else if (c >= 'a' && c <= 'f') nibble = c - 'a' + 10;
        else if (c >= 'A' && c <= 'F') nibble = c - 'A' + 10;
        v = (v << 4) | nibble;
    }
    return v;
}
} // namespace

RecruitOperImageHasher& RecruitOperImageHasher::get_instance()
{
    static RecruitOperImageHasher inst;
    return inst;
}

bool RecruitOperImageHasher::load(const std::filesystem::path& json_path)
{
    std::ifstream f(json_path);
    if (!f) {
        Log.warn(__FUNCTION__, "哈希库文件不存在:", json_path.string());
        m_loaded = false;
        return false;
    }

    json::value j;
    try {
        f >> j;
    }
    catch (const std::exception& e) {
        Log.error(__FUNCTION__, "哈希库 JSON 解析失败:", e.what());
        m_loaded = false;
        return false;
    }

    if (!j.is_object()) {
        Log.error(__FUNCTION__, "哈希库 JSON 顶层非 object");
        m_loaded = false;
        return false;
    }

    m_hash_db.clear();
    for (auto it = j.object_begin(); it != j.object_end(); ++it) {
        const std::string& id = it->key();
        const std::string hex = it->value().as_string();
        m_hash_db.emplace(id, hash_to_uint64(hex));
    }

    m_loaded = true;
    Log.info(__FUNCTION__, "哈希库加载完成:", m_hash_db.size(), "干员");
    return true;
}

uint64_t RecruitOperImageHasher::compute_hash(const cv::Mat& image)
{
    cv::Mat resized;
    cv::resize(image, resized, cv::Size(32, 32), 0, 0, cv::INTER_AREA);

    cv::Mat gray;
    cv::cvtColor(resized, gray, cv::COLOR_BGR2GRAY);
    gray.convertTo(gray, CV_32F);

    cv::Mat dct;
    cv::dct(gray, dct);

    constexpr int kBlock = 8;
    double avg = 0.0;
    for (int y = 0; y < kBlock; ++y)
        for (int x = 0; x < kBlock; ++x) avg += dct.at<float>(y, x);
    avg /= static_cast<double>(kBlock * kBlock);

    uint64_t hash = 0;
    int bit = 0;
    for (int y = 0; y < kBlock; ++y)
        for (int x = 0; x < kBlock; ++x) {
            if (dct.at<float>(y, x) > avg) hash |= (1ULL << bit);
            ++bit;
        }
    return hash;
}

std::optional<RecruitOperImageHasher::MatchResult>
RecruitOperImageHasher::find_nearest(uint64_t target_hash, int max_distance) const
{
    if (!m_loaded || m_hash_db.empty()) return std::nullopt;

    MatchResult best;
    best.distance = INT_MAX;
    bool found = false;

    for (const auto& [id, h] : m_hash_db) {
        int d = __builtin_popcountll(target_hash ^ h);
        if (d < best.distance) {
            best.oper_id = id;
            best.distance = d;
            found = true;
        }
    }

    if (!found || best.distance > max_distance) return std::nullopt;

    // 查 oper_id → name
    for (const auto& oper : RecruitData.get_all_opers()) {
        if (oper.id == best.oper_id) {
            best.oper_name = oper.name;
            break;
        }
    }
    return best;
}
} // namespace asst