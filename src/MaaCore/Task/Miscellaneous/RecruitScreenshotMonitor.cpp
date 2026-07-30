#include "RecruitScreenshotMonitor.h"

#include <opencv2/core.hpp>
#include <opencv2/imgproc.hpp>

#include "Utils/Logger.hpp"

namespace asst
{
RecruitScreenshotMonitor& RecruitScreenshotMonitor::get_instance()
{
    static RecruitScreenshotMonitor inst;
    return inst;
}

RecruitScreenshotMonitor::Anomaly RecruitScreenshotMonitor::check(const cv::Mat& image)
{
    if (image.empty()) {
        m_consecutive_anomaly.fetch_add(1);
        return Anomaly::Black; // 空帧视为黑屏
    }

    cv::Mat gray;
    cv::cvtColor(image, gray, cv::COLOR_BGR2GRAY);

    // 平均亮度
    double mean_brightness = cv::mean(gray)[0];

    Anomaly detected = Anomaly::None;

    if (mean_brightness < 10.0) {
        detected = Anomaly::Black;
    }
    else if (mean_brightness > 245.0) {
        detected = Anomaly::White;
    }
    else if (!m_last_frame.empty() && m_last_frame.size() == gray.size()) {
        // 帧差异
        cv::Mat diff;
        cv::absdiff(gray, m_last_frame, diff);
        double mean_diff = cv::mean(diff)[0];
        if (mean_diff < 0.5) {
            detected = Anomaly::Frozen;
        }
    }

    if (detected != Anomaly::None) {
        m_consecutive_anomaly.fetch_add(1);
        Log.warn(__FUNCTION__, "截图异常:", static_cast<int>(detected));
    }
    else {
        m_consecutive_anomaly.store(0);
    }

    image.copyTo(m_last_frame);
    return detected;
}

bool RecruitScreenshotMonitor::is_anomaly_persistent(int required_frames)
{
    return m_consecutive_anomaly.load() >= required_frames;
}
} // namespace asst