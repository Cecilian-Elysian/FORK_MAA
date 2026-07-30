#include "RecruitResultImageAnalyzer.h"

#include <algorithm>
#include <array>
#include <cmath>
#include <filesystem>
#include <fstream>
#include <iomanip>
#include <opencv2/core.hpp>
#include <opencv2/imgcodecs.hpp>
#include <opencv2/imgproc.hpp>
#include <ranges>
#include <sstream>

#include "Config/Miscellaneous/RecruitConfig.h"
#include "Config/TaskData.h"
#include "Utils/Logger.hpp"
#include "Vision/Matcher.h"
#include "Vision/OCRer.h"

namespace asst
{
namespace
{
// ----------------------------------------------------------------------------
// Levenshtein 距离
// ----------------------------------------------------------------------------
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

// ----------------------------------------------------------------------------
// pHash（perceptual hash）— 32x32 → DCT → 8x8 平均阈值 → 64-bit
// ----------------------------------------------------------------------------
uint64_t perceptual_hash(const cv::Mat& image)
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

int hamming_distance(uint64_t a, uint64_t b)
{
    int count = 0;
    uint64_t diff = a ^ b;
    while (diff) {
        count += static_cast<int>(diff & 1);
        diff >>= 1;
    }
    return count;
}

// FNV-1a 64-bit 文件 hash hex（避免 OpenSSL 依赖；非密码学安全但足够用作完整性校验）
std::string file_hash_hex(const std::filesystem::path& path)
{
    std::ifstream f(path, std::ios::binary);
    if (!f) return {};

    uint64_t hash = 0xcbf29ce484222325ULL;
    std::array<char, 4096> buf {};
    while (f.read(buf.data(), buf.size()) || f.gcount() > 0) {
        for (std::streamsize i = 0; i < f.gcount(); ++i) {
            hash ^= static_cast<uint64_t>(static_cast<unsigned char>(buf[i]));
            hash *= 0x100000001b3ULL;
        }
    }

    std::ostringstream oss;
    oss << std::hex << std::setw(16) << std::setfill('0') << hash;
    return oss.str();
}
} // namespace

bool RecruitResultImageAnalyzer::analyze()
{
    m_result = RecruitResultInfo {};

    if (try_ocr_name()) return true;
    if (try_phash()) return true;
    if (try_star_template()) return true;
    fallback_screenshot();
    return false;
}

bool RecruitResultImageAnalyzer::try_ocr_name()
{
    // 多 ROI 候选：名牌区 / 立绘底部署名 / 代号条
    // 优先匹配 RecruitConfig.m_all_opers（限制 required，缩小候选）
    static const std::vector<Rect> name_rois = {
        Rect { 200, 380, 880, 80 }, // 名牌中央（默认）
        Rect { 200, 360, 880, 120 }, // 名牌 + 代号
        Rect { 100, 200, 1080, 480 }, // 立绘整图兜底
    };

    std::vector<std::string> opers_name;
    for (const auto& oper : RecruitData.get_all_opers()) opers_name.push_back(oper.name);

    std::string best_match;
    int best_distance = INT_MAX;
    std::string best_raw;
    bool l1_hit = false;

    for (const Rect& roi : name_rois) {
        set_roi(roi);

        OCRer ocrer(m_image, roi);
        ocrer.set_task_info("RecruitResultNameOCR"); // tasks.json 占位，Phase 4 实际定义
        ocrer.set_required(opers_name);

        auto result_opt = ocrer.analyze();
        if (!result_opt) continue;

        for (const auto& res : *result_opt) {
            best_raw = res.text;
            // 短字符串匹配（干员名通常 2-4 字）
            if (best_raw.empty()) continue;

            for (const auto& cand : opers_name) {
                int d = levenshtein_distance(best_raw, cand);
                if (d < best_distance) {
                    best_distance = d;
                    best_match = cand;
                }
            }

            // Levenshtein ≤ 2 → L1 命中
            if (best_distance <= 2) {
                l1_hit = true;
                break;
            }
        }
        if (l1_hit) break;
    }

    if (l1_hit) {
        m_result.ocr_status = RecruitResultInfo::OcrStatus::L1;
        m_result.operator_name = best_match;
        m_result.ocr_raw_text = best_raw;
        m_result.ocr_match_distance = best_distance;
        // 查 ID
        for (const auto& oper : RecruitData.get_all_opers()) {
            if (oper.name == best_match) {
                m_result.operator_id = oper.id;
                break;
            }
        }
        Log.info(__FUNCTION__, "L1 OCR 命中:", best_match, "raw:", best_raw, "dist:", best_distance);
        return true;
    }

    if (!best_raw.empty()) m_result.ocr_raw_text = best_raw;
    m_result.ocr_match_distance = best_distance;
    return false;
}

bool RecruitResultImageAnalyzer::try_phash()
{
    // 立绘 ROI（中央偏上，避免名牌/底部 UI）
    const Rect portrait_roi { 380, 100, 520, 360 };
    set_roi(portrait_roi);

    cv::Mat portrait = m_image(make_rect<cv::Rect>(portrait_roi));
    if (portrait.empty()) return false;

    uint64_t target_hash = perceptual_hash(portrait);

    // 哈希库加载（首次）
    static bool loaded = false;
    static std::unordered_map<std::string, uint64_t> hash_db; // oper.id → hash
    if (!loaded) {
        // Phase 3 由 RecruitOperImageHasher 实际加载；此处占位
        loaded = true;
    }

    if (hash_db.empty()) return false;

    std::string best_id;
    int best_dist = INT_MAX;
    for (const auto& [id, h] : hash_db) {
        int d = hamming_distance(target_hash, h);
        if (d < best_dist) {
            best_dist = d;
            best_id = id;
        }
    }

    if (best_dist <= 8) {
        // 查 ID → name
        for (const auto& oper : RecruitData.get_all_opers()) {
            if (oper.id == best_id) {
                m_result.ocr_status = RecruitResultInfo::OcrStatus::L1_5;
                m_result.operator_id = best_id;
                m_result.operator_name = oper.name;
                m_result.phash_distance = best_dist;
                Log.info(__FUNCTION__, "L1.5 哈希命中:", oper.name, "dist:", best_dist);
                return true;
            }
        }
    }

    m_result.phash_distance = best_dist;
    return false;
}

bool RecruitResultImageAnalyzer::try_star_template()
{
    // ★ 等级头部模板（tasks.json: RecruitStar3~6）
    static const std::vector<std::pair<int, std::string>> star_tasks = {
        { 3, "RecruitStar3" },
        { 4, "RecruitStar4" },
        { 5, "RecruitStar5" },
        { 6, "RecruitStar6" },
    };

    for (const auto& [level, task] : star_tasks) {
        Matcher matcher(m_image);
        matcher.set_task_info(task);
        if (auto hit = matcher.analyze()) {
            m_result.level = level;
            m_result.ocr_status = RecruitResultInfo::OcrStatus::L2;
            Log.info(__FUNCTION__, "L2 ★ 模板命中:", level);
            return true;
        }
    }
    return false;
}

bool RecruitResultImageAnalyzer::fallback_screenshot()
{
    m_result.ocr_status = RecruitResultInfo::OcrStatus::L3;

    // C1 截图脱敏：只保留名牌 + ★ 头部，丢弃立绘（避免肖像权 + 减小体积）
    cv::Mat sanitized;
    try {
        cv::Rect name_roi(200, 380, 880, 120);
        cv::Rect star_roi(400, 80, 480, 80);
        // 裁剪到 ROI 范围内
        name_roi &= cv::Rect(0, 0, m_image.cols, m_image.rows);
        star_roi &= cv::Rect(0, 0, m_image.cols, m_image.rows);

        const int H = name_roi.height + star_roi.height + 20;
        const int W = std::max(name_roi.width, star_roi.width);
        sanitized = cv::Mat(H, W, m_image.type(), cv::Scalar(0, 0, 0));

        if (star_roi.area() > 0) {
            m_image(star_roi).copyTo(sanitized(cv::Rect(0, 0, star_roi.width, star_roi.height)));
        }
        if (name_roi.area() > 0) {
            m_image(name_roi).copyTo(
                sanitized(cv::Rect(0, star_roi.height + 20, name_roi.width, name_roi.height)));
        }
    }
    catch (const cv::Exception&) {
        // 脱敏失败时回退到原图
        sanitized = m_image;
    }

    // save_draw 路径：<install>/debug/recruit_<slot>_<ts>.png
    const auto debug_dir = utils::path("debug");
    std::error_code ec;
    std::filesystem::create_directories(debug_dir, ec);

    auto now = std::chrono::system_clock::now();
    auto ts = std::chrono::duration_cast<std::chrono::seconds>(now.time_since_epoch()).count();
    auto filename = std::string("recruit_") + std::to_string(ts) + ".png";
    auto full_path = debug_dir / filename;

    cv::imwrite(full_path.string(), sanitized);
    m_result.screenshot_path = full_path.string();
    m_result.screenshot_sha256 = file_hash_hex(full_path);

    Log.warn(__FUNCTION__, "L3 全失败，截图已保存(脱敏):", m_result.screenshot_path);
    return false;
}
} // namespace asst