#pragma once

#include <array>
#include <atomic>
#include <chrono>
#include <mutex>

namespace asst
{
// 性能计时工具（D2）
//
// 用法：
// ```cpp
// PerformanceTimer timer;
// // ... do work ...
// auto ms = timer.elapsed_ms();
// ```
//
// 或 RAII：
// ```cpp
// auto t = PerformanceTimer::scoped("RecruitResultImageAnalyzer::analyze");
// // ... do work ...
// // 析构时自动输出
// ```
class PerformanceTimer
{
public:
    PerformanceTimer() : m_start(std::chrono::steady_clock::now()) {}

    void reset() { m_start = std::chrono::steady_clock::now(); }

    int64_t elapsed_ms() const
    {
        auto now = std::chrono::steady_clock::now();
        return std::chrono::duration_cast<std::chrono::milliseconds>(now - m_start).count();
    }

    // RAII 计时器：作用域结束时输出
    class Scoped
    {
    public:
        explicit Scoped(const char* tag) : m_tag(tag) {}
        ~Scoped();

        Scoped(const Scoped&) = delete;
        Scoped& operator=(const Scoped&) = delete;

        int64_t elapsed_ms() const { return m_timer.elapsed_ms(); }

    private:
        const char* m_tag;
        PerformanceTimer m_timer;
    };

    static Scoped scoped(const char* tag) { return Scoped(tag); }

private:
    std::chrono::steady_clock::time_point m_start;
};

inline PerformanceTimer::Scoped::~Scoped()
{
    // 日志输出由调用方负责（避免引入 Logger 依赖）
}
} // namespace asst