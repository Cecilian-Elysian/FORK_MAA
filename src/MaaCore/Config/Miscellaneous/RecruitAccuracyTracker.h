#pragma once

#include <array>
#include <atomic>
#include <cstdint>
#include <mutex>

namespace asst
{
// 招募完成识别准确率跟踪（D1）
//
// 滑动窗口：最近 N 次（默认 100）L1/L1.5/L2/L3 命中比例
// 每 window_size 次累计后由 emit_report() 触发 RecruitAccuracyReport callback。
class RecruitAccuracyTracker final
{
public:
    static constexpr int kWindowSize = 100;

    static RecruitAccuracyTracker& get_instance();

    // 记录一次结果
    void record(int ocr_status); // 0=L1, 1=L1.5, 2=L2, 3=L3

    // 滑窗统计
    struct Snapshot
    {
        int l1 = 0;
        int l1_5 = 0;
        int l2 = 0;
        int l3 = 0;
        int total = 0;
        double accuracy() const { return total == 0 ? 0.0 : (l1 + l1_5) * 100.0 / total; }
    };

    Snapshot snapshot() const;

    // 重置（清空滑窗）
    void reset();

private:
    RecruitAccuracyTracker() = default;

    mutable std::mutex m_mutex;
    std::array<int, 4> m_counts { 0, 0, 0, 0 }; // L1/L1.5/L2/L3
    int m_total = 0;
};
} // namespace asst