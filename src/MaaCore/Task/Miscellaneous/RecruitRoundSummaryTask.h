#pragma once

#include <string>
#include <vector>

#include "Task/AbstractTask.h"

namespace asst
{
// 公招本轮汇总（A1）
//
// 由 AutoRecruitTask 在 _run() 结尾调用，统计本轮 4 个 slot 的实际招募结果，
// 发 RecruitRoundSummary callback（WPF 用于 Toast 聚合 + 任务队列汇总行）。
class RecruitRoundSummaryTask final
{
public:
    struct SlotRecord
    {
        int slot_index = 0;
        std::vector<std::string> tags;
        int level = 0;
        std::string oper_name;
        bool expedited = false;
        bool is_refresh = false;
    };

    explicit RecruitRoundSummaryTask(AbstractTask& parent);

    void add_slot(const SlotRecord& slot);
    void emit_summary();

private:
    AbstractTask& m_parent;
    std::vector<SlotRecord> m_slots;
    std::chrono::steady_clock::time_point m_start;
};
} // namespace asst