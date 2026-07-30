#pragma once

#include <filesystem>
#include <mutex>
#include <optional>
#include <string>
#include <unordered_map>
#include <vector>

namespace asst
{
// 公招经验库（v6 方案 L3）
//
// 路径：config/recruit_experience.json
// 格式：{ "experiences": [...] }
// 每条经验含 tags_hash / tags / level / operator / operator_id / count / last_seen
//
// 置信度：B1 — count ≥ 3 = high (反哺生效)，count 1-2 = low（仅记录）
// 写入：每次 L1/L1.5/L2 命中或用户手动补充时调 record()
//
// 线程安全：所有公开方法均加锁。
class RecruitExperience final
{
public:
    struct Experience
    {
        std::string tags_hash;   // tags 排序后 hash
        std::vector<std::string> tags;
        int level = 0;
        std::string oper_name;
        std::string oper_id;
        int count = 0;
        std::string confidence; // "high" / "medium" / "low"
        long long last_seen = 0; // unix timestamp
        std::vector<std::string> accounts;
    };

    static RecruitExperience& get_instance();

    // 加载/保存
    bool load(const std::filesystem::path& json_path);
    bool save(const std::filesystem::path& json_path);

    // 记录一次招募结果（命中或用户手动）
    void record(const std::vector<std::string>& tags, int level, const std::string& oper_name,
                const std::string& oper_id, const std::string& account = {});

    // 查询：tags + level 匹配的最强经验
    std::optional<Experience> query(const std::vector<std::string>& tags, int level) const;

    size_t size() const { return m_data.size(); }

    const std::vector<Experience>& all() const { return m_data; }

private:
    RecruitExperience() = default;
    static std::string compute_tags_hash(const std::vector<std::string>& tags);

    mutable std::mutex m_mutex;
    std::vector<Experience> m_data;
};
} // namespace asst