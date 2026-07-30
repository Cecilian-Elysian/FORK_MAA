#pragma once

#include <atomic>
#include <chrono>

#include "MaaUtils/NoWarningCVMat.hpp"

namespace asst
{
// 截图异常检测（C3）
//
// 检测：黑屏 / 全白 / 帧冻结（连续多帧几乎相同，疑似游戏卡死）
// 调用方：RecruitResultTask::analyze 之前先调 check()，
//   异常时发 RecruitScreenshotAnomaly callback 并跳过本次识别。
class RecruitScreenshotMonitor final
{
public:
    static RecruitScreenshotMonitor& get_instance();

    enum class Anomaly
    {
        None,
        Black,   // 平均亮度 < 10
        White,   // 平均亮度 > 245
        Frozen,  // 与上一帧几乎相同（差异 < 0.5%）
    };

    Anomaly check(const cv::Mat& image);

    // 连续异常帧数（用于避免单帧误报）
    bool is_anomaly_persistent(int required_frames = 3);

private:
    RecruitScreenshotMonitor() = default;

    cv::Mat m_last_frame;
    std::atomic<int> m_consecutive_anomaly { 0 };
};
} // namespace asst