#include "RecruitResultTask.h"

#include <chrono>

#include "Common/AsstTypes.h"
#include "Utils/Logger.hpp"
#include "Vision/Miscellaneous/RecruitLocaleAdapter.h"

namespace asst
{
RecruitResultTask::RecruitResultTask(AbstractTask& parent) : m_parent(parent) {}

std::optional<RecruitResultInfo>
RecruitResultTask::analyze(const cv::Mat& image, const Context& ctx)
{
    (void)ctx;
    auto t0 = std::chrono::steady_clock::now();

    RecruitResultImageAnalyzer analyzer(image);
    bool ok = analyzer.analyze();
    auto info = analyzer.get_result();

    auto t1 = std::chrono::steady_clock::now();
    int elapsed_ms =
        static_cast<int>(std::chrono::duration_cast<std::chrono::milliseconds>(t1 - t0).count());

    Log.info(
        __FUNCTION__,
        "recruit result analyze: status=", static_cast<int>(info.ocr_status),
        "level=", info.level,
        "operator=", info.operator_name,
        "duration_ms=", elapsed_ms);

    return ok ? std::optional<RecruitResultInfo> { info } : std::nullopt;
}

void RecruitResultTask::emit_callback(const RecruitResultInfo& info, const Context& ctx)
{
    json::value cb = m_parent.basic_info();
    cb["what"] = "RecruitSlotCompleted";

    auto& details = cb["details"];
    details["tags"] = json::array(ctx.tags);
    details["level"] = info.level;
    details["operator"] = info.operator_name;
    details["operator_id"] = info.operator_id;
    details["is_robot"] = false; // TODO: 支援机械识别（Phase 3）

    details["slot_index"] = ctx.slot_index;
    details["slot_total"] = ctx.slot_total;
    details["expedited"] = ctx.expedited;
    details["is_refresh"] = ctx.is_refresh;
    details["recruit_hour"] = ctx.recruit_hour;

    details["ocr_status"] = [&] {
        switch (info.ocr_status) {
        case RecruitResultInfo::OcrStatus::L1: return "L1";
        case RecruitResultInfo::OcrStatus::L1_5: return "L1.5";
        case RecruitResultInfo::OcrStatus::L2: return "L2";
        case RecruitResultInfo::OcrStatus::L3: return "L3";
        }
        return "L3";
    }();

    details["ocr_raw_text"] = info.ocr_raw_text;
    details["ocr_match_distance"] = info.ocr_match_distance;
    details["phash_distance"] = info.phash_distance;
    details["possible_operators"] = json::array {};
    details["screenshot_path"] = info.screenshot_path;
    details["screenshot_sha256"] = info.screenshot_sha256;

    auto now = std::chrono::system_clock::now();
    details["timestamp"] =
        std::chrono::duration_cast<std::chrono::seconds>(now.time_since_epoch()).count();

    details["account_name"] = ctx.account_name;
    details["locale"] = ctx.locale;
    details["channel"] = ctx.channel;

    m_parent.callback(AsstMsg::SubTaskExtraInfo, cb);
}
} // namespace asst