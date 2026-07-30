#include "RecruitRoundSummaryTask.h"

#include <algorithm>
#include <chrono>

#include "Utils/Logger.hpp"

namespace asst
{
RecruitRoundSummaryTask::RecruitRoundSummaryTask(AbstractTask& parent)
    : m_parent(parent), m_start(std::chrono::steady_clock::now())
{}

void RecruitRoundSummaryTask::add_slot(const SlotRecord& slot) { m_slots.push_back(slot); }

void RecruitRoundSummaryTask::emit_summary()
{
    auto now = std::chrono::steady_clock::now();
    auto duration_ms =
        std::chrono::duration_cast<std::chrono::milliseconds>(now - m_start).count();

    std::vector<std::string> six_stars, five_stars, four_stars, three_stars;
    int expedited_count = 0;
    int refresh_count = 0;
    int l1 = 0, l1_5 = 0, l2 = 0, l3 = 0;

    for (const auto& s : m_slots) {
        switch (s.level) {
        case 6: six_stars.push_back(s.oper_name); break;
        case 5: five_stars.push_back(s.oper_name); break;
        case 4: four_stars.push_back(s.oper_name); break;
        case 3: three_stars.push_back(s.oper_name); break;
        }
        if (s.expedited) expedited_count++;
        if (s.is_refresh) refresh_count++;
    }

    json::value cb = m_parent.basic_info();
    cb["what"] = "RecruitRoundSummary";
    auto& d = cb["details"];
    d["total_slots"] = static_cast<int>(m_slots.size());
    d["six_stars"] = json::array();
    for (const auto& n : six_stars) d["six_stars"].push_back(n);
    d["five_stars"] = json::array();
    for (const auto& n : five_stars) d["five_stars"].push_back(n);
    d["four_stars"] = json::array();
    for (const auto& n : four_stars) d["four_stars"].push_back(n);
    d["three_stars"] = json::array();
    for (const auto& n : three_stars) d["three_stars"].push_back(n);
    d["expedited_count"] = expedited_count;
    d["refresh_count"] = refresh_count;
    d["duration_total_ms"] = static_cast<int>(duration_ms);

    m_parent.callback(AsstMsg::SubTaskExtraInfo, cb);
    Log.info(__FUNCTION__, "本轮汇总: 6★", six_stars.size(), "5★", five_stars.size(),
             "4★", four_stars.size(), "3★", three_stars.size(),
             "expedited=", expedited_count);
}
} // namespace asst