#include "RecruitExperience.h"

#include <algorithm>
#include <chrono>
#include <fstream>

#include <openssl/sha.h>

#include "Utils/Logger.hpp"

namespace asst
{
namespace
{
constexpr int kConfidenceHighThreshold = 3;
constexpr int kConfidenceMediumThreshold = 2;

std::string confidence_for_count(int count)
{
    if (count >= kConfidenceHighThreshold) return "high";
    if (count >= kConfidenceMediumThreshold) return "medium";
    return "low";
}
} // namespace

RecruitExperience& RecruitExperience::get_instance()
{
    static RecruitExperience inst;
    return inst;
}

std::string RecruitExperience::compute_tags_hash(const std::vector<std::string>& tags)
{
    std::vector<std::string> sorted = tags;
    std::ranges::sort(sorted);

    std::string joined;
    for (const auto& t : sorted) joined += t + "|";

    unsigned char digest[SHA_DIGEST_LENGTH];
    SHA1(reinterpret_cast<const unsigned char*>(joined.data()), joined.size(), digest);

    static const char* hex = "0123456789abcdef";
    std::string out(SHA_DIGEST_LENGTH * 2, '\0');
    for (size_t i = 0; i < SHA_DIGEST_LENGTH; ++i) {
        out[2 * i] = hex[(digest[i] >> 4) & 0xF];
        out[2 * i + 1] = hex[digest[i] & 0xF];
    }
    return out;
}

bool RecruitExperience::load(const std::filesystem::path& json_path)
{
    std::lock_guard<std::mutex> lock(m_mutex);

    std::ifstream f(json_path);
    if (!f) {
        Log.info(__FUNCTION__, "经验库文件不存在，跳过加载:", json_path.string());
        return false;
    }

    json::value j;
    try {
        f >> j;
    }
    catch (const std::exception& e) {
        Log.warn(__FUNCTION__, "经验库 JSON 解析失败:", e.what());
        return false;
    }

    if (!j.contains("experiences") || !j["experiences"].is_array()) return false;

    m_data.clear();
    for (const auto& item : j["experiences"].as_array()) {
        Experience exp;
        exp.tags_hash = item.value("tags_hash", std::string {});
        if (item.contains("tags") && item["tags"].is_array()) {
            for (const auto& t : item["tags"].as_array()) exp.tags.push_back(t.as_string());
        }
        exp.level = item.value("level", 0);
        exp.oper_name = item.value("operator", std::string {});
        exp.oper_id = item.value("operator_id", std::string {});
        exp.count = item.value("count", 0);
        exp.confidence = item.value("confidence", std::string {});
        exp.last_seen = item.value("last_seen", 0LL);
        if (item.contains("accounts") && item["accounts"].is_array()) {
            for (const auto& a : item["accounts"].as_array()) exp.accounts.push_back(a.as_string());
        }
        m_data.push_back(std::move(exp));
    }

    Log.info(__FUNCTION__, "经验库加载完成:", m_data.size(), "条");
    return true;
}

bool RecruitExperience::save(const std::filesystem::path& json_path)
{
    std::lock_guard<std::mutex> lock(m_mutex);

    json::value j;
    j["experiences"] = json::array();
    for (const auto& exp : m_data) {
        json::value item;
        item["tags_hash"] = exp.tags_hash;
        item["tags"] = json::array();
        for (const auto& t : exp.tags) item["tags"].push_back(t);
        item["level"] = exp.level;
        item["operator"] = exp.oper_name;
        item["operator_id"] = exp.oper_id;
        item["count"] = exp.count;
        item["confidence"] = exp.confidence;
        item["last_seen"] = exp.last_seen;
        item["accounts"] = json::array();
        for (const auto& a : exp.accounts) item["accounts"].push_back(a);
        j["experiences"].push_back(item);
    }

    std::ofstream f(json_path);
    if (!f) {
        Log.error(__FUNCTION__, "经验库保存失败:", json_path.string());
        return false;
    }
    f << j.to_string();
    return true;
}

void RecruitExperience::record(const std::vector<std::string>& tags, int level,
                                const std::string& oper_name, const std::string& oper_id,
                                const std::string& account)
{
    if (level <= 0 || oper_name.empty()) return;

    std::lock_guard<std::mutex> lock(m_mutex);

    std::string hash = compute_tags_hash(tags);

    auto it = std::ranges::find_if(m_data, [&](const Experience& e) {
        return e.tags_hash == hash && e.level == level;
    });

    auto now = std::chrono::system_clock::now();
    auto ts = std::chrono::duration_cast<std::chrono::seconds>(now.time_since_epoch()).count();

    if (it == m_data.end()) {
        Experience exp;
        exp.tags_hash = std::move(hash);
        exp.tags = tags;
        exp.level = level;
        exp.oper_name = oper_name;
        exp.oper_id = oper_id;
        exp.count = 1;
        exp.confidence = confidence_for_count(1);
        exp.last_seen = ts;
        if (!account.empty()) exp.accounts.push_back(account);
        m_data.push_back(std::move(exp));
    }
    else {
        it->count += 1;
        it->confidence = confidence_for_count(it->count);
        it->last_seen = ts;
        if (!account.empty() &&
            std::ranges::find(it->accounts, account) == it->accounts.end()) {
            it->accounts.push_back(account);
        }
        // oper 信息若之前缺失则补
        if (it->oper_name.empty()) it->oper_name = oper_name;
        if (it->oper_id.empty()) it->oper_id = oper_id;
    }

    Log.info(__FUNCTION__, "记录经验:", oper_name, "(", level, "★) count=",
             it == m_data.end() ? 1 : it->count, "confidence=",
             it == m_data.end() ? "low" : it->confidence);
}

std::optional<RecruitExperience::Experience>
RecruitExperience::query(const std::vector<std::string>& tags, int level) const
{
    std::lock_guard<std::mutex> lock(m_mutex);

    std::string hash = compute_tags_hash(tags);

    std::optional<Experience> best;
    for (const auto& exp : m_data) {
        if (exp.tags_hash != hash || exp.level != level) continue;
        if (exp.confidence != "high") continue; // 仅 high 反哺（B1）
        if (!best || exp.count > best->count) best = exp;
    }
    return best;
}
} // namespace asst