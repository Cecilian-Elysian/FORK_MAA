#pragma once

#include <cstdint>
#include <optional>
#include <string>
#include <unordered_map>

#include "Common/AsstTypes.h"
#include "MaaUtils/NoWarningCVMat.hpp"

namespace asst
{
// 立绘 pHash 库管理
//
// JSON 路径：resource/recruit/oper_phash.json
// 格式：{ "<oper_id>": "<16 hex>", ... }（64-bit → 16 hex）
//
// 调用方：RecruitResultImageAnalyzer 在 L1 OCR 失败时调用 find_nearest()。
class RecruitOperImageHasher final
{
public:
    static RecruitOperImageHasher& get_instance();

    // 从 resource/recruit/oper_phash.json 加载（首次调用时执行）
    bool load(const std::filesystem::path& json_path);

    bool loaded() const noexcept { return m_loaded; }

    // 计算给定图片的 pHash（公开，供构建脚本复用）
    static uint64_t compute_hash(const cv::Mat& image);

    // 查最近（汉明距离）
    struct MatchResult
    {
        std::string oper_id;
        std::string oper_name;
        int distance = INT_MAX;
    };

    std::optional<MatchResult> find_nearest(uint64_t target_hash, int max_distance = 8) const;

    size_t size() const noexcept { return m_hash_db.size(); }

private:
    RecruitOperImageHasher() = default;
    bool m_loaded = false;
    std::unordered_map<std::string, uint64_t> m_hash_db; // oper_id → hash
};
} // namespace asst