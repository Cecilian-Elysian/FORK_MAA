#pragma once

#include "Vision/VisionHelper.h"

#include <string>
#include <vector>

#include "Common/AsstTypes.h"

namespace asst
{
// 招募完成展示页识别结果
//
// 多通道识别（按 v6 方案 L0）：
// - L1:    OCR 名牌 + Levenshtein ≤ 2 匹配 RecruitConfig.m_all_opers
// - L1.5:  立绘 pHash 匹配 (距离 ≤ 8)
// - L2:    OCR/哈希都失败，但 ★ 模板成功 → 推算可能干员列表
// - L3:    OCR/哈希/模板全失败 → 截图导出
//
// downstream: 新增，feat/recruit-result-display 首次引入；与现有 RecruitImageAnalyzer
// （tags / 时间 / 刷新 / 许可）职责互补，本 analyzer 专注「完成展示页」。
struct RecruitResultInfo
{
    enum class OcrStatus
    {
        L1,    // OCR + Levenshtein 命中
        L1_5,  // 立绘 pHash 命中
        L2,    // OCR/哈希失败 + ★ 模板成功（推算兜底）
        L3,    // 全失败（截图导出）
    };

    int level = 0;                     // 实际 ★ 数；0 = 未知（L3）
    std::string operator_name;         // 实际干员名；空 = 未识别
    std::string operator_id;           // 干员 ID（RecruitConfig）；空 = 未识别
    OcrStatus ocr_status = OcrStatus::L3;

    std::string ocr_raw_text;          // OCR 原始输出（即便未匹配也保留）
    int ocr_match_distance = -1;       // Levenshtein 距离；-1 = 未尝试
    int phash_distance = -1;           // pHash 距离；-1 = 未尝试

    std::string screenshot_path;       // L3 时 save_draw 路径
    std::string screenshot_sha256;     // 截图完整性校验
};

class RecruitResultImageAnalyzer final : public VisionHelper
{
public:
    using VisionHelper::VisionHelper;
    RecruitResultImageAnalyzer(const cv::Mat& image, const Rect& roi) = delete;
    virtual ~RecruitResultImageAnalyzer() override = default;

    bool analyze();

    const RecruitResultInfo& get_result() const noexcept { return m_result; }

private:
    // 阶段 1：OCR 名牌 + Levenshtein
    bool try_ocr_name();
    // 阶段 2：立绘 pHash 匹配
    bool try_phash();
    // 阶段 3：★ 等级头部模板匹配
    bool try_star_template();
    // 阶段 4：全失败时截图导出
    bool fallback_screenshot();

    RecruitResultInfo m_result;
};
} // namespace asst