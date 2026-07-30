#include "RecruitExperience.h"

#include <algorithm>
#include <chrono>
#include <fstream>
#include <iomanip>
#include <sstream>

#include <meojson/json.hpp>

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
    // FNV-1a 64-bit 哈希（避免 OpenSSL 依赖）
    std::vector<std::string> sorted = tags;
    std::ranges::sort(sorted);

    std::string joined;
    for (const auto& t : sorted) joined += t + "|";

    uint64_t hash = 0xcbf29ce484222325ULL;
    for (char c : joined) {
        hash ^= static_cast<uint64_t>(static_cast<unsigned char>(c));
        hash *= 0x100000001b3ULL;
    }

    std::ostringstream oss;
    oss << std::hex << std::setw(16) << std::setfill('0') << hash;
    return oss.str();
}

bool RecruitExperience::load(const std::filesystem::path& json_path)
{
    std::lock_guard<std::mutex> lock(m_mutex);

    std::ifstream f(json_path);
    if (!f) {
        Log.info(__FUNCTION__, "经验库文件不存在，跳过加载:", json_path.string());
        return false;
    }

    std::string content((std::istreambuf_iterator<char>(f)), std::istreambuf_iterator<char>());
    if (content.empty()) {
        Log.info(__FUNCTION__, "经验库文件为空:", json_path.string());
        return false;
    }

    json::value j;
    try {
        auto j_opt = json::parse(content);
        if (!j_opt) {
            Log.warn(__FUNCTION__, "经验库 JSON 解析失败");
            return false;
        }
        j = std::move(*j_opt);
    }
    catch (const std::exception& e) {
        Log.warn(__FUNCTION__, "经验库 JSON 解析失败:", e.what());
        return false;
    }

    if (!j.contains("experiences") || !j["experiences"].is_array()) return false;

    m_data.clear();
    for (const auto& item : j.at("experiences").as_array()) {
        Experience exp;
        exp.tags_hash = item.get("tags_hash", std::string {});
        if (item.contains("tags") && item.at("tags").is_array()) {
            for (const auto& t : item.at("tags").as_array()) exp.tags.push_back(t.as_string());
        }
        exp.level = item.get("level", 0);
        exp.oper_name = item.get("operator", std::string {});
        exp.oper_id = item.get("operator_id", std::string {});
        exp.count = item.get("count", 0);
        exp.confidence = item.get("confidence", std::string {});
        exp.last_seen = item.get("last_seen", 0LL);
        if (item.contains("accounts") && item.at("accounts").is_array()) {
            for (const auto& a : item.at("accounts").as_array()) exp.accounts.push_back(a.as_string());
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
    {
        json::array exps_arr;
        for (const auto& exp : m_data) {
            json::value item;
            item["tags_hash"] = exp.tags_hash;
            {
                json::array tags_arr;
                for (const auto& t : exp.tags) tags_arr.push_back(t);
                item["tags"] = std::move(tags_arr);
            }
            item["level"] = exp.level;
            item["operator"] = exp.oper_name;
            item["operator_id"] = exp.oper_id;
            item["count"] = exp.count;
            item["confidence"] = exp.confidence;
            item["last_seen"] = exp.last_seen;
            {
                json::array accounts_arr;
                for (const auto& a : exp.accounts) accounts_arr.push_back(a);
                item["accounts"] = std::move(accounts_arr);
            }
            exps_arr.push_back(item);
        }
        j["experiences"] = std::move(exps_arr);
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