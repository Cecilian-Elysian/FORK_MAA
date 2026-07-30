#include "RecruitAccuracyTracker.h"

#include "Utils/Logger.hpp"

namespace asst
{
RecruitAccuracyTracker& RecruitAccuracyTracker::get_instance()
{
    static RecruitAccuracyTracker inst;
    return inst;
}

void RecruitAccuracyTracker::record(int ocr_status)
{
    if (ocr_status < 0 || ocr_status > 3) return;
    std::lock_guard<std::mutex> lock(m_mutex);
    m_counts[ocr_status] += 1;
    m_total += 1;
}

RecruitAccuracyTracker::Snapshot RecruitAccuracyTracker::snapshot() const
{
    std::lock_guard<std::mutex> lock(m_mutex);
    Snapshot s;
    s.l1 = m_counts[0];
    s.l1_5 = m_counts[1];
    s.l2 = m_counts[2];
    s.l3 = m_counts[3];
    s.total = m_total;
    return s;
}

void RecruitAccuracyTracker::reset()
{
    std::lock_guard<std::mutex> lock(m_mutex);
    m_counts.fill(0);
    m_total = 0;
    Log.info(__FUNCTION__, "准确率统计已重置");
}
} // namespace asst