#pragma once

#include <string>
#include <vector>

#include "Common/AsstTypes.h"
#include "Task/AbstractTask.h"
#include "Vision/Miscellaneous/RecruitResultImageAnalyzer.h"

namespace asst
{
// 招募完成展示页识别 + callback 封装
//
// 用法（由 AutoRecruitTask::hire_all() 在「雇用」按钮点击后调用）：
// ```cpp
// RecruitResultTask result_task(*this);
// auto info = result_task.analyze(image, slot_index, slot_total,
//                                  expedited, is_refresh, recruit_hour, tags);
// if (info) result_task.emit_callback(*info);
// ```
//
// 内部委托 RecruitResultImageAnalyzer 多通道识别 (L0/L1/L2/L3)，
// 并在 L3 时调 save_draw 截图（C1 脱敏）。
class RecruitResultTask final
{
public:
    explicit RecruitResultTask(AbstractTask& parent);

    struct Context
    {
        int slot_index = 0;
        int slot_total = 0;
        bool expedited = false;
        bool is_refresh = false;
        int recruit_hour = 9;
        std::vector<std::string> tags; // 已选 tag（来自 recruit_calc_task）
        std::string account_name;
        std::string locale = "zh-cn";
        std::string channel = "Official";
    };

    std::optional<RecruitResultInfo> analyze(const cv::Mat& image, const Context& ctx);

    void emit_callback(const RecruitResultInfo& info, const Context& ctx);

private:
    AbstractTask& m_parent;
};
} // namespace asst