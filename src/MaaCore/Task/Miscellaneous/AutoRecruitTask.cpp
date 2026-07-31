#include "AutoRecruitTask.h"

#include "Config/GeneralConfig.h"
#include "Config/Miscellaneous/RecruitAccuracyTracker.h"
#include "Config/Miscellaneous/RecruitConfig.h"
#include "Config/Miscellaneous/RecruitExperience.h"
#include "Config/TaskData.h"
#include "Controller/Controller.h"
#include "Task/Miscellaneous/RecruitResultTask.h"
#include "Task/Miscellaneous/RecruitScreenshotMonitor.h"
#include "Task/ProcessTask.h"
#include "Task/ReportDataTask.h"
#include "Utils/Logger.hpp"
#include "Vision/Miscellaneous/RecruitImageAnalyzer.h"
#include "Vision/MultiMatcher.h"
#include "Vision/OCRer.h"

#include <algorithm>
#include <boost/regex.hpp>
#include <ranges>

namespace asst::recruit_calc
{
// all combinations and their operator list, excluding empty set and 6-star operators while there is
// no senior tag
auto get_all_combs(
    const std::vector<RecruitConfig::TagId>& tags,
    const std::vector<Recruitment>& all_ops = RecruitData.get_all_opers())
{
    std::vector<RecruitCombs> rcs_with_single_tag;

    {
        rcs_with_single_tag.reserve(tags.size());
        std::ranges::transform(tags, std::back_inserter(rcs_with_single_tag), [](const RecruitConfig::TagId& t) {
            RecruitCombs result;
            result.tags = { t };
            result.min_level = 6;
            result.max_level = 0;
            result.avg_level = 0;
            return result;
        });

        for (const auto& op : all_ops) {
            for (auto& rc : rcs_with_single_tag) {
                if (!op.has_tag(rc.tags.front())) {
                    continue;
                }
                rc.opers.push_back(op);
                rc.min_level = (std::min)(rc.min_level, op.level);
                rc.max_level = (std::max)(rc.max_level, op.level);
                rc.avg_level += op.level;
            }
        }

        for (auto& rc : rcs_with_single_tag) {
            rc.avg_level /= static_cast<double>(rc.opers.size());
            // intersection and union are based on sorted container
            std::ranges::sort(rc.tags);
            std::ranges::sort(rc.opers);
        }
    }

    std::vector<RecruitCombs> result;
    const size_t tag_size = tags.size();
    result.reserve(tag_size * (tag_size * tag_size + 5) / 6); // C(size, 3) + C(size, 2) + C(size, 1)

    // select one tag first
    for (size_t i = 0; i < tag_size; ++i) {
        RecruitCombs temp1 = rcs_with_single_tag[i];
        if (temp1.opers.empty()) [[unlikely]] {
            continue;               // this is not possible
        }
        result.emplace_back(temp1); // that is it

        // but what if another tag is also selected
        for (size_t j = i + 1; j < tag_size; ++j) {
            RecruitCombs temp2 = temp1 * rcs_with_single_tag[j];
            if (temp2.opers.empty()) [[unlikely]] {
                continue;
            }
            result.emplace_back(temp2); // two tags only

            // select a third one
            for (size_t k = j + 1; k < tag_size; ++k) {
                RecruitCombs temp3 = temp2 * rcs_with_single_tag[k];
                if (temp3.opers.empty()) [[unlikely]] {
                    continue;
                }
                result.emplace_back(temp3);
            }
        }
    }

    static constexpr std::string_view SeniorOper = "高级资深干员";

    for (auto comb_iter = result.begin(); comb_iter != result.end();) {
        if (std::ranges::find(comb_iter->tags, RecruitConfig::TagId(SeniorOper)) != comb_iter->tags.end()) {
            ++comb_iter;
            continue;
        }
        // no senior tag, remove 6-star operators
        // assuming sorted by level
        auto iter = std::ranges::find_if(comb_iter->opers, [](const Recruitment& op) { return op.level >= 6; });
        if (iter == comb_iter->opers.end()) {
            ++comb_iter;
            continue;
        }
        comb_iter->opers.erase(iter, comb_iter->opers.end());
        if (comb_iter->opers.empty()) {
            comb_iter = result.erase(comb_iter);
            continue;
        }
        comb_iter->update_attributes();
        ++comb_iter;
    }

    return result;
}
} // namespace asst::recruit_calc

asst::AutoRecruitTask& asst::AutoRecruitTask::set_select_level(std::vector<int> select_level) noexcept
{
    m_select_level = std::move(select_level);
    return *this;
}

asst::AutoRecruitTask& asst::AutoRecruitTask::set_confirm_level(std::vector<int> confirm_level) noexcept
{
    m_confirm_level = std::move(confirm_level);
    return *this;
}

asst::AutoRecruitTask& asst::AutoRecruitTask::set_need_refresh(bool need_refresh) noexcept
{
    m_need_refresh = need_refresh;
    return *this;
}

asst::AutoRecruitTask& asst::AutoRecruitTask::set_max_times(int max_times) noexcept
{
    m_max_times = max_times;
    return *this;
}

asst::AutoRecruitTask& asst::AutoRecruitTask::set_use_expedited(bool use_or_not) noexcept
{
    m_use_expedited = use_or_not;
    return *this;
}

asst::AutoRecruitTask& asst::AutoRecruitTask::set_expedite_min_level(int level) noexcept
{
    m_expedite_min_level = level;
    return *this;
}

asst::AutoRecruitTask& asst::AutoRecruitTask::set_auto_upgrade_3star_with_4star(bool enable) noexcept
{
    m_auto_upgrade_3star_with_4star = enable;
    return *this;
}

asst::AutoRecruitTask& asst::AutoRecruitTask::set_select_extra_tags(ExtraTagsMode select_extra_tags_mode) noexcept
{
    m_select_extra_tags_mode = select_extra_tags_mode;
    return *this;
}

asst::AutoRecruitTask& asst::AutoRecruitTask::set_first_tags(std::vector<std::string> first_tags) noexcept
{
    m_first_tags = first_tags;
    return *this;
}

asst::AutoRecruitTask& asst::AutoRecruitTask::set_preserve_tags(std::vector<RecruitConfig::TagId> skip_tags) noexcept
{
    m_preserve_tags = std::move(skip_tags);
    return *this;
}

asst::AutoRecruitTask& asst::AutoRecruitTask::set_set_time(bool set_time) noexcept
{
    m_set_time = set_time;
    return *this;
}

asst::AutoRecruitTask& asst::AutoRecruitTask::set_force_refresh(bool force_refresh) noexcept
{
    m_force_refresh = force_refresh;
    return *this;
}

asst::AutoRecruitTask& asst::AutoRecruitTask::set_recruitment_time(std::unordered_map<int, int> time_map) noexcept
{
    m_desired_time_map = std::move(time_map);
    return *this;
}

asst::AutoRecruitTask& asst::AutoRecruitTask::set_penguin_enabled(bool enable, std::string penguin_id) noexcept
{
    m_upload_to_penguin = enable;
    if (!penguin_id.empty()) {
        m_penguin_id = std::move(penguin_id);
    }
    return *this;
}

asst::AutoRecruitTask& asst::AutoRecruitTask::set_yituliu_enabled(bool enable, std::string yituliu_id) noexcept
{
    m_upload_to_yituliu = enable;
    if (!yituliu_id.empty()) {
        m_yituliu_id = std::move(yituliu_id);
    }
    return *this;
}

asst::AutoRecruitTask& asst::AutoRecruitTask::set_server(std::string server) noexcept
{
    m_server = std::move(server);
    return *this;
}

bool asst::AutoRecruitTask::_run()
{
    // downstream: feat/recruit-result-display — 本轮汇总 (A1)
    m_round_summary = std::make_unique<RecruitRoundSummaryTask>(*this);
    auto summary_guard = std::shared_ptr<void>(nullptr, [this](void*) {
        if (m_round_summary) m_round_summary->emit_summary();
    });
    (void)summary_guard;

    if (is_calc_only_task()) {
        // 小工具中的公招计算，不需要点击确认按钮
        return recruit_calc_task().success;
    }

    if (!recruit_begin()) {
        return false;
    }

    {
        // 领取一下已完成的招募
        const auto image = ctrler()->get_image();
        // initialize_dirty_slot_info(image);
        m_dirty_slots = { 0, 1, 2, 3 };
        if (!hire_all(image)) {
            return false;
        }
    }

    static constexpr int slot_retry_limit = 3;

    while (m_cur_times < m_max_times) {
        auto start_rect = try_get_start_button(ctrler()->get_image());
        if (start_rect) {
            if (need_exit()) {
                return false;
            }
            if (m_slot_fail >= slot_retry_limit) {
                return false;
            }
            auto result = recruit_one(start_rect.value());
            // confirmed: real recruitment done, advance m_cur_times
            // skipped:   slot marked in m_force_skipped, try next slot without changing counters
            // failed:    recognition error / no permit / etc., bump m_slot_fail for retry limiting
            if (result == recruit_result::confirmed) {
                ++m_cur_times;
            }
            else if (result == recruit_result::failed) {
                ++m_slot_fail;
            }
            if (!m_has_permit && (!m_force_refresh || !m_has_refresh)) {
                return true;
            }
        }
        else {
            if (!check_recruit_home_page()) {
                return false;
            }
            Log.info("There is no available start button.");
            return true;
        }
    }
    return true;
}

void asst::AutoRecruitTask::click_return_button()
{
    ProcessTask(*this, { "RecruitContinue", "Return" }).run();
}

std::vector<asst::TextRect> asst::AutoRecruitTask::start_recruit_analyze(const cv::Mat& image)
{
    OCRer start_analyzer(image);
    start_analyzer.set_task_info("StartRecruit");
    if (!start_analyzer.analyze()) {
        return {};
    }
    return start_analyzer.get_result();
}

std::optional<asst::Rect> asst::AutoRecruitTask::try_get_start_button(const cv::Mat& image)
{
    const auto result = start_recruit_analyze(image);
    if (result.empty()) {
        return std::nullopt;
    }
    auto iter = std::ranges::find_if(result, [&](const TextRect& r) -> bool {
        return !m_force_skipped.contains(slot_index_from_rect(r.rect));
    });
    if (iter == result.cend()) {
        return std::nullopt;
    }
    Log.info("Found slot index", slot_index_from_rect(iter->rect), ".");
    return iter->rect;
}

/// Open a pending recruit slot, analyze tags, set timer and tags, then confirm or skip.
/// Returns:
/// - confirmed: RecruitConfirm was executed successfully
/// - skipped:   slot intentionally skipped (special/robot tag, level-based force skip, or
///              no recruit permit); slot is added to m_force_skipped so the loop tries the
///              next slot without changing any counters
/// - failed:    recognition error, timer mismatch, confirm failure, or exit requested
asst::AutoRecruitTask::recruit_result asst::AutoRecruitTask::recruit_one(const Rect& button)
{
    LogTraceFunction;

    // fix/expedite-threshold: recruit_one 入口重置 m_last_confirmed_min_level,
    // 避免上一槽位陈旧星级污染本槽位加急决策
    m_last_confirmed_min_level = 0;

    int delay = Config.get_options().task_delay;

    ctrler()->click(button);
    sleep(delay);

    auto calc_result = recruit_calc_task(slot_index_from_rect(button));
    sleep(delay);

    if (!calc_result.success) {
        {
            json::value info = basic_info();
            info["what"] = "RecruitError";
            info["why"] = "识别错误";
            callback(AsstMsg::SubTaskError, info);
        }
        if (!ProcessTask(*this, { "RecruitContinue", "Return" }).run()) {
            m_force_skipped.emplace(slot_index_from_rect(button));
        }
        return recruit_result::failed;
    }

    if (calc_result.for_special_tags_skip || calc_result.for_preserved_tags_skip) {
        m_force_skipped.emplace(slot_index_from_rect(button));
        click_return_button();
        return recruit_result::skipped;
    }

    if (calc_result.force_skip) {
        m_force_skipped.emplace(slot_index_from_rect(button));
        click_return_button();
        return recruit_result::skipped;
    }

    if (need_exit()) {
        return recruit_result::failed;
    }

    if (m_set_time && !check_timer(calc_result.recruitment_time)) {
        // timer was not set to 09:00:00 properly, likely the tag selection was also corrupted
        // see
        // https://github.com/MaaAssistantArknights/MaaAssistantArknights/pull/300#issuecomment-1073287984
        // return and try later
        Log.info("Timer of this slot has not been reduced as expected.");
        click_return_button();
        return recruit_result::failed;
    }

    // TODO: count blue pixels and compare with number of selected tags desired

    if (need_exit()) {
        return recruit_result::failed;
    }

    if (!confirm()) {
        Log.info("Failed to confirm current recruit config.");
        click_return_button();
        return recruit_result::failed;
    }

    // fix/expedite-threshold: 加急判定必须在 confirm() 之后。
    // confirm() 点击「开始招募」启动 9h 倒计时并返回公招主页,
    // 主页上该 slot 才会出现「立即招 / 立即完成」按钮供 recruit_now() 点击。
    // 原 feat 将 recruit_now() 放在 confirm() 之前, 详情页无此按钮, OCR 必失败。
    if (m_use_expedited && m_original_min_level >= m_expedite_min_level) {
        Log.info("Recruit slot level", m_last_confirmed_min_level, ">= expedite threshold",
                 m_expedite_min_level, ", using expedited plan.");
        if (recruit_now()) {
            hire_all();
            m_last_confirmed_min_level = 0;
            return recruit_result::confirmed;
        }
        Log.info("Failed to use expedited plan, slot already confirmed with normal 9h timer.");
    }

    return recruit_result::confirmed;
}

// set recruit timer and tags only
asst::AutoRecruitTask::calc_task_result_type asst::AutoRecruitTask::recruit_calc_task(slot_index index)
{
    LogTraceFunction;

    static constexpr size_t refresh_limit = 3;
    static constexpr size_t analyze_limit = 5;

    size_t refresh_count = 0;
    for (size_t image_analyzer_retry = 0; image_analyzer_retry < analyze_limit;) {
        ++image_analyzer_retry;

        RecruitImageAnalyzer image_analyzer(ctrler()->get_image());
        if (!image_analyzer.analyze()) {
            continue;
        }
        if (image_analyzer.get_tags_result().size() != RecruitConfig::CorrectNumberOfTags) {
            continue;
        }

#ifdef ASST_DEBUG
        // mock_test_001: 1/5/6 Star Operators appear when first recruited.
        static bool RunRecruitMockTest_001 = false;
        if (RunRecruitMockTest_001) {
            static int skip_once_001 = 0;
            if (skip_once_001 == 0) {
                // image_analyzer.mock_set_special(asst::RecruitImageAnalyzer::operator_type::robot);
                // image_analyzer.mock_set_special(asst::RecruitImageAnalyzer::operator_type::senior);
                // image_analyzer.mock_set_special(asst::RecruitImageAnalyzer::operator_type::top);
                // image_analyzer.mock_set_special(asst::RecruitImageAnalyzer::operator_type::highvalue);
                skip_once_001++;
            }
        }
        // mock_test_002: The high-star combination tag and the 1-star tag appear at the same time
        static bool RunRecruitMockTest_002 = false;
        if (RunRecruitMockTest_002) {
            static int skip_once_002 = 0;
            if (skip_once_002 == 0) {
                image_analyzer.mock_set_special(asst::RecruitImageAnalyzer::operator_type::combination_tag);
                skip_once_002++;
            }
        }
        // mock_test_003: 4 star tag appear
        static bool RunRecruitMockTest_003 = false;
        if (RunRecruitMockTest_003) {
            static int skip_once_003 = 0;
            if (skip_once_003 == 0) {
                image_analyzer.mock_set_special(asst::RecruitImageAnalyzer::operator_type::fourstar);
                skip_once_003++;
            }
        }
#endif

        const std::vector<TextRect>& tags = image_analyzer.get_tags_result(); // 中文的招募tag
        m_has_refresh = !image_analyzer.get_refresh_rect().empty();
        m_has_permit = image_analyzer.get_permit_rect().empty();

        std::vector<RecruitConfig::TagId> tag_ids;
        std::ranges::transform(tags, std::back_inserter(tag_ids), std::mem_fn(&TextRect::text));

        bool has_special_tag = false;
        bool has_skip_tag = false;
        bool has_preferred_tag = false;
        std::optional<RecruitConfig::TagId> preserved_tag;

        json::value info = basic_info();
        info["details"]["tags"] = json::array(get_tag_names(tag_ids));

        // tags result
        {
            json::value cb_info = info;
            cb_info["what"] = "RecruitTagsDetected";
            callback(AsstMsg::SubTaskExtraInfo, cb_info);
        }

        // special tags
        const std::vector<RecruitConfig::TagId> SpecialTags = { "高级资深干员", "资深干员" };
        if (auto special_iter = std::ranges::find_first_of(SpecialTags, tag_ids); special_iter != SpecialTags.cend())
            [[unlikely]] {
            has_special_tag = true;
            if (std::ranges::find(m_preserve_tags, *special_iter) != m_preserve_tags.cend()) {
                has_skip_tag = true;
                preserved_tag = *special_iter;
            }
            else {
                json::value cb_info = info;
                cb_info["what"] = "RecruitSpecialTag";
                cb_info["details"]["tag"] = RecruitData.get_tag_name(*special_iter);
                callback(AsstMsg::SubTaskExtraInfo, cb_info);
            }
        }

        if (!has_skip_tag && !m_preserve_tags.empty()) {
            if (auto skip_iter = std::ranges::find_first_of(tag_ids, m_preserve_tags); skip_iter != tag_ids.cend())
                [[unlikely]] {
                has_skip_tag = true;
                preserved_tag = *skip_iter;
            }
        }

        if (preserved_tag.has_value()) [[unlikely]] {
            json::value cb_info = info;
            cb_info["what"] = "RecruitPreservedTag";
            cb_info["details"]["tag"] = RecruitData.get_tag_name(preserved_tag.value());
            callback(AsstMsg::SubTaskExtraInfo, cb_info);
        }

        // preferred tags
        if (!m_first_tags.empty()) {
            for (const RecruitConfig::TagId& tag_id : tag_ids) {
                std::string tag_name = RecruitData.get_tag_name(tag_id);
                for (const std::string& preferred_tag : m_first_tags) {
                    if (preferred_tag.empty()) {
                        continue;
                    }
                    // the preferred tag is the tag's substring
                    if (tag_name.find(preferred_tag) != std::string::npos) {
                        has_preferred_tag = true;
                        break;
                    }
                }
            }
        }

        std::vector<RecruitCombs> result_vec = recruit_calc::get_all_combs(tag_ids);

        // assuming timer would be set to 09:00:00
        for (RecruitCombs& rc : result_vec) {
            if (rc.min_level < 3) {
                // find another min level (assuming operator list sorted in increment order by
                // level)
                auto sec = std::ranges::find_if(rc.opers, [](const Recruitment& op) { return op.level >= 3; });
                if (sec != rc.opers.end()) {
                    rc.min_level = sec->level;
                    rc.avg_level = std::transform_reduce(
                                       sec,
                                       rc.opers.end(),
                                       0.,
                                       std::plus<double> {},
                                       std::mem_fn(&Recruitment::level)) /
                                   static_cast<double>(std::distance(sec, rc.opers.end()));
                }
            }
        }

        // 3★ 组合里若能开 4★ 干员（如「费用回复 + 先锋干员」出桃金娘），升级为 4★ 处理路径
        for (RecruitCombs& rc : result_vec) {
            if (m_auto_upgrade_3star_with_4star && rc.min_level == 3 && rc.max_level >= 4) {
                auto first_4 = std::ranges::find_if(rc.opers, [](const Recruitment& op) { return op.level >= 4; });
                if (first_4 != rc.opers.end()) {
                    rc.min_level = first_4->level;
                    rc.avg_level = std::transform_reduce(
                                       first_4,
                                       rc.opers.end(),
                                       0.,
                                       std::plus<double> {},
                                       std::mem_fn(&Recruitment::level)) /
                                   static_cast<double>(std::distance(first_4, rc.opers.end()));
                }
            }
        }

        std::ranges::sort(result_vec, [&](const RecruitCombs& lhs, const RecruitCombs& rhs) -> bool {
            // prefer the one with special tag
            // workaround for
            // https://github.com/MaaAssistantArknights/MaaAssistantArknights/issues/1336
            if (has_special_tag) {
                bool l_has = std::ranges::find_first_of(lhs.tags, SpecialTags) != lhs.tags.cend();
                bool r_has = std::ranges::find_first_of(rhs.tags, SpecialTags) != rhs.tags.cend();
                if (l_has != r_has) {
                    return l_has > r_has;
                }
            }

            if (lhs.min_level != rhs.min_level) {
                return lhs.min_level > rhs.min_level; // 最小等级大的，排前面
            }
            else if (lhs.max_level != rhs.max_level) {
                return lhs.max_level > rhs.max_level; // 最大等级大的，排前面
            }
            else if (std::fabs(lhs.avg_level - rhs.avg_level) > DoubleDiff) {
                return lhs.avg_level > rhs.avg_level; // 平均等级高的，排前面
            }
            else {
                return lhs.tags.size() < rhs.tags.size(); // Tag数量少的，排前面
            }
        });

        if (result_vec.empty()) {
            continue;
        }

        const auto& final_combination = result_vec.front();
        m_last_confirmed_min_level = final_combination.min_level;

        // fix/auto-recruit-expedite-original-level: 保存 3→4 升级前的原始最低星级，
        // 用于加急判定。扫描 opers 中 ≥3★ 的最低干员等级，
        // 若组合含 3★ 干员（3→4 升级前为 3★），则原始等级为 3，
        // 避免「三星词条出四星」场景下误加急浪费许可。
        m_original_min_level = m_last_confirmed_min_level;
        if (m_auto_upgrade_3star_with_4star) {
            auto min_op = std::ranges::find_if(final_combination.opers,
                [](const Recruitment& op) { return op.level >= 3; });
            if (min_op != final_combination.opers.end()) {
                m_original_min_level = (std::min)(m_original_min_level, static_cast<int>(min_op->level));
            }
        }

        {
            json::object results_json;
            results_json["result"] = json::array();
            results_json["level"] = final_combination.min_level;
            for (const auto& comb : result_vec) {
                json::array opers_json;
                for (const Recruitment& oper_info : comb.opers | std::views::reverse) { // print reversely
                    opers_json.emplace_back(
                        json::object {
                            { "name", oper_info.name },
                            { "id", oper_info.id },
                            { "level", oper_info.level },
                        });
                }
                results_json["result"].as_array().emplace_back(
                    json::object {
                        { "tags", json::array(get_tag_names(comb.tags)) },
                        { "opers", opers_json },
                        { "level", comb.min_level },
                    });
            }
            info["details"] |= results_json;

            json::value cb_info = info;
            cb_info["what"] = "RecruitResult";
            callback(AsstMsg::SubTaskExtraInfo, cb_info);
        }

        bool to_report = false;
        if (!is_calc_only_task()) {
            // report if the slot is clean
            if (!m_dirty_slots.contains(index)) {
                to_report = true;
                m_dirty_slots.emplace(index); // mark as dirty
            }
            else {
                Log.info("will not report, dirty slots are", m_dirty_slots);
            }
        }

#ifdef ASST_DEBUG
        // to_report = true;
#endif
        if (to_report) {
            upload_result(tag_ids, info["details"]);
        }

        if (need_exit()) {
            return {};
        }

        // refresh
        // clang-format off
        if (m_need_refresh && m_has_refresh &&  // 基础条件
            !has_special_tag &&                 // 5 星以上 tag 不刷新
            !has_skip_tag &&                    // 保留词条时不刷新
            final_combination.min_level == 3)   // 如果只有 3 星 tag，即使有倾向 tag 也应该刷新
        // clang-format on
        {
            if (refresh_count > refresh_limit) [[unlikely]] {
                json::value cb_info = basic_info();
                cb_info["what"] = "RecruitError";
                cb_info["why"] = "刷新次数达到上限";
                cb_info["details"] = json::object { { "refresh_limit", refresh_limit } };
                callback(AsstMsg::SubTaskError, cb_info);
                return {};
            }

            refresh();

            ++refresh_count;

            // mark the slot clean after refreshed
            m_dirty_slots.erase(index);

            {
                json::value cb_info = basic_info();
                cb_info["what"] = "RecruitTagsRefreshed";
                cb_info["details"] = json::object {
                    { "count", refresh_count },
                    { "refresh_limit", refresh_limit },
                };
                callback(AsstMsg::SubTaskExtraInfo, cb_info);
                Log.trace("recruit tags refreshed", refresh_count, "times, rerunning recruit task");
            }

            // desired retry, not an error
            --image_analyzer_retry;
            continue;
        }

        if (need_exit()) {
            return {};
        }

        if (!m_has_permit) {
            bool continue_refresh = m_force_refresh && m_has_refresh;

            json::value cb_info = basic_info();
            cb_info["what"] = "RecruitNoPermit";
            cb_info["details"] = json::object {
                { "continue", continue_refresh },
            };
            callback(AsstMsg::SubTaskExtraInfo, cb_info);
            Log.trace("No recruit permit");

            calc_task_result_type result(calc_task_result::no_permit);
            return result;
        }

        if (final_combination.min_level > 4) {
            has_special_tag = true;
        }

        if (!is_calc_only_task()) {
            if (!(has_skip_tag || has_special_tag)) {
                // do not confirm 3 star, force skip
                if (!is_confirm_level_valid(3) && final_combination.min_level == 3 &&
                    !is_select_level_valid(final_combination.min_level)) {
                    calc_task_result_type result(calc_task_result::force_skip);
                    return result;
                }
            }
            // do not confirm 4 star
            if (!is_confirm_level_valid(4) && final_combination.min_level == 4 &&
                !is_select_level_valid(final_combination.min_level)) {
                calc_task_result_type result(calc_task_result::force_skip);
                return result;
            }
            // "Automatically recruit 5/6 Star operators" is not checked.
            if (has_special_tag && !is_confirm_level_valid(final_combination.min_level)) {
                calc_task_result_type result(calc_task_result::special_tag_skip);
                return result;
            }

            if (has_skip_tag) {
                calc_task_result_type result(calc_task_result::preserved_tag_skip);
                return result;
            }
        }

        int recruitment_time = m_desired_time_map[(std::max)(final_combination.min_level, 3)];
        if (recruitment_time == 0) {
            recruitment_time = 9 * 60;
        }

        // try to set the timer to desired value
        if (m_set_time) {
            Log.info("recruitment time:", recruitment_time, "min");
            const int desired_hour = recruitment_time / 60;
            const int desired_minute_div_10 = (recruitment_time % 60) / 10;
            const int temp = desired_hour + (desired_minute_div_10 != 0);
            const int hour_delta = (1 < temp) ? (1 + 9 - temp) : (temp - 1);
            const int minute_delta = (0 < desired_minute_div_10) ? (0 + 6 - desired_minute_div_10) : (0);
            for (int i = 0; i < hour_delta; ++i) {
                ctrler()->click(image_analyzer.get_hour_decrement_rect());
            }
            for (int i = 0; i < minute_delta; ++i) {
                ctrler()->click(image_analyzer.get_minute_decrement_rect());
            }
        }

        // nothing to select, leave the selection empty
        if (!(final_combination.min_level == 3 && has_preferred_tag) &&
            !is_select_level_valid(final_combination.min_level)) {
            calc_task_result_type result(calc_task_result::nothing_to_select, recruitment_time);
            return result;
        }

        // get selections
        auto final_select = get_select_tags(result_vec, tag_ids);

        // select tags
        for (const std::string& final_tag_name : final_select) {
            auto tag_rect_iter =
                std::ranges::find_if(tags, [&](const TextRect& r) { return r.text == final_tag_name; });
            if (tag_rect_iter != tags.cend()) {
                ctrler()->click(tag_rect_iter->rect);
            }
        }

        {
            json::value cb_info = basic_info();
            cb_info["what"] = "RecruitTagsSelected";
            cb_info["details"] = json::object { { "tags", json::array(get_tag_names(final_select)) } };
            callback(AsstMsg::SubTaskExtraInfo, cb_info);
        }

        calc_task_result_type result(
            calc_task_result::success,
            recruitment_time,
            static_cast<int>(final_combination.tags.size()));
        return result;
    }

    Log.error("Failed to analyze recruit tags.");
    save_img(utils::path("debug") / utils::path("recruit"));
    return {};
}

bool asst::AutoRecruitTask::recruit_begin()
{
    ProcessTask task(*this, { "RecruitBegin" });
    return task.run();
}

bool asst::AutoRecruitTask::check_timer(int minutes_expected)
{
    const auto image = ctrler()->get_image();
    const auto replace_map = Task.get<OcrTaskInfo>("NumberOcrReplace")->replace_map;

    {
        OCRer hour_ocr(image);
        hour_ocr.set_task_info("RecruitTimerH");
        hour_ocr.set_replace(replace_map);
        if (!hour_ocr.analyze()) {
            return false;
        }
        std::string desired_hour_str = std::string("0") + std::to_string(minutes_expected / 60);
        if (hour_ocr.get_result().front().text != desired_hour_str) {
            return false;
        }
    }
    if (minutes_expected % 60 == 0) {
        return true; // minute counter stays untouched
    }

    {
        OCRer minute_ocr(image);
        minute_ocr.set_task_info("RecruitTimerM");
        minute_ocr.set_replace(replace_map);
        if (!minute_ocr.analyze()) {
            return false;
        }
        std::string desired_minute_str = std::to_string((minutes_expected % 60) / 10) + "0";
        if (minute_ocr.get_result().front().text != desired_minute_str) {
            return false;
        }
    }
    return true;
}

bool asst::AutoRecruitTask::check_recruit_home_page()
{
    ProcessTask task(*this, { "RecruitFlag" });
    task.set_retry_times(2);
    return task.run();
}

bool asst::AutoRecruitTask::recruit_now()
{
    ProcessTask task(*this, { "RecruitNow" });
    return task.run();
}

bool asst::AutoRecruitTask::confirm()
{
    ProcessTask confirm_task(*this, { "RecruitConfirm" });
    return confirm_task.set_retry_times(5).run();
}

bool asst::AutoRecruitTask::refresh()
{
    ProcessTask refresh_task(*this, { "RecruitRefresh" });
    return refresh_task.run();
}

bool asst::AutoRecruitTask::hire_all(const cv::Mat& image)
{
    LogTraceFunction;
    std::vector<int> slots_to_identify;
    // mark slots with *Hire* button clean (regardless of whether hiring will success)
    {
        MultiMatcher hire_searcher(image);
        hire_searcher.set_task_info("RecruitFinish");
        hire_searcher.analyze();
        for (const MatchRect& r : hire_searcher.get_result()) {
            Log.info("Mark", slot_index_from_rect(r.rect), "clean");
            m_dirty_slots.erase(slot_index_from_rect(r.rect));
            // downstream: feat/recruit-result-display — 收集有「雇用」按钮的 slot
            slots_to_identify.push_back(static_cast<int>(slot_index_from_rect(r.rect)));
        }
        if (hire_searcher.get_result().empty()) {
            return true;
        }
    }
    // downstream: feat/recruit-result-display — 在「雇用」按钮被点击前识别每个 slot 的干员
    // 雇用按钮被点击后弹窗就消失了，必须在 ProcessTask "RecruitFinish" 之前截屏识别。
    for (int idx : slots_to_identify) {
        identify_recruit_result(image, idx, {}, 0, m_use_expedited, false, 9);
    }
    // hire all
    return ProcessTask { *this, { "RecruitFinish" } }.run();
}

/// search for blue *Hire* buttons in the recruit home page, mark those slot clean and do hiring
bool asst::AutoRecruitTask::hire_all()
{
    return hire_all(ctrler()->get_image());
}

// downstream: feat/recruit-result-display — 识别单个 slot 的招募完成展示页
void asst::AutoRecruitTask::identify_recruit_result(const cv::Mat& image, int slot_index,
                                                     const std::vector<std::string>& tags,
                                                     int level, bool expedited, bool is_refresh,
                                                     int recruit_hour)
{
    LogTraceFunction;
    (void)level;

    // C3 黑屏/全白检测：连续异常 ≥ 3 帧时跳过
    auto& monitor = RecruitScreenshotMonitor::get_instance();
    auto anomaly = monitor.check(image);
    if (anomaly != RecruitScreenshotMonitor::Anomaly::None) {
        if (monitor.is_anomaly_persistent(3)) {
            json::value cb = basic_info();
            cb["what"] = "RecruitScreenshotAnomaly";
            cb["details"]["slot_index"] = slot_index;
            cb["details"]["anomaly_type"] = [&] {
                switch (anomaly) {
                case RecruitScreenshotMonitor::Anomaly::Black: return "black";
                case RecruitScreenshotMonitor::Anomaly::White: return "white";
                case RecruitScreenshotMonitor::Anomaly::Frozen: return "frozen";
                default: return "unknown";
                }
            }();
            callback(AsstMsg::SubTaskExtraInfo, cb);
            Log.warn(__FUNCTION__, "截图异常持续，跳过 slot", slot_index);
            return;
        }
    }

    RecruitResultTask result_task(*this);
    RecruitResultTask::Context ctx;
    ctx.slot_index = slot_index;
    ctx.slot_total = 4;
    ctx.expedited = expedited;
    ctx.is_refresh = is_refresh;
    ctx.recruit_hour = recruit_hour;
    ctx.tags = tags;
    ctx.account_name = ""; // Phase 6 由 feat/account_rotation 填充
    ctx.locale = "zh-cn";   // Phase 5 由 WPF 侧注入
    ctx.channel = "Official";

    auto info_opt = result_task.analyze(image, ctx);
    if (info_opt) {
        result_task.emit_callback(*info_opt, ctx);

        // 准确率跟踪 (D1)
        RecruitAccuracyTracker::get_instance().record(static_cast<int>(info_opt->ocr_status));

        // 经验库反哺 (L3) — 仅 confidence = "high" 时反哺
        if (info_opt->level > 0 && !info_opt->operator_name.empty()) {
            RecruitExperience::get_instance().record(
                tags, info_opt->level, info_opt->operator_name, info_opt->operator_id);
        }

        // A1 本轮汇总
        if (m_round_summary) {
            RecruitRoundSummaryTask::SlotRecord slot;
            slot.slot_index = slot_index;
            slot.tags = tags;
            slot.level = info_opt->level;
            slot.oper_name = info_opt->operator_name;
            slot.expedited = expedited;
            slot.is_refresh = is_refresh;
            m_round_summary->add_slot(slot);
        }
    }
}

/// search for *RecruitNow* buttons before recruit and mark them as dirty
[[maybe_unused]] bool asst::AutoRecruitTask::initialize_dirty_slot_info(const cv::Mat& image)
{
    m_dirty_slots.clear();
    const auto result = start_recruit_analyze(image);
    for (const TextRect& r : result) {
        m_dirty_slots.emplace(slot_index_from_rect(r.rect));
    }
    Log.info("Dirty slots are", m_dirty_slots);
    return true;
}

std::vector<std::string> asst::AutoRecruitTask::get_tag_names(const std::vector<RecruitConfig::TagId>& ids) const
{
    std::vector<std::string> names;
    for (const RecruitConfig::TagId& id : ids) {
        names.emplace_back(RecruitData.get_tag_name(id));
    }
    return names;
}

std::vector<asst::RecruitConfig::TagId> asst::AutoRecruitTask::get_select_tags(
    const std::vector<RecruitCombs>& combinations,
    std::vector<RecruitConfig::TagId> tag_ids)
{
    LogTraceFunction;
    std::unordered_set<RecruitConfig::TagId> unique_tags;
    std::vector<RecruitConfig::TagId> select;

    if (combinations.front().min_level == 3) {
        // only run if we have certain preferred tags for 3★ tags
        if (!m_first_tags.empty()) {
            for (const RecruitConfig::TagId& tag_id : tag_ids) {
                std::string tag_name = RecruitData.get_tag_name(tag_id);
                for (const std::string& preferred_tag : m_first_tags) {
                    if (preferred_tag.empty()) {
                        continue;
                    }
                    // the preferred tag is the tag's substring
                    if (tag_name.find(preferred_tag) != std::string::npos) {
                        select.emplace_back(tag_id);
                        continue;
                    }
                }
                if (select.size() == 3) {
                    return select;
                }
            }
            return select;
        }
    }
    if (m_select_extra_tags_mode == ExtraTagsMode::NoExtra) {
        return combinations.front().tags;
    }
    else if (m_select_extra_tags_mode == ExtraTagsMode::Extra) {
        while (select.size() < 3) {
            for (const asst::RecruitCombs& comb : combinations) {
                for (const RecruitConfig::TagId& tag : comb.tags) {
                    if (unique_tags.find(tag) == unique_tags.cend()) {
                        unique_tags.insert(tag);
                        select.emplace_back(tag);
                        if (select.size() == 3) {
                            return select;
                        }
                    }
                }
            }
        }
    }
    else if (m_select_extra_tags_mode == ExtraTagsMode::ExtraOnlyRare) {
        // only select rare tags ( > 3 rank) and select as many as possible.

        // do not select lower rank tags when higher rank tags exist.
        int min_level = combinations.front().min_level;
        // tag combo will be either full selected, or abandoned.
        int emplace_back_count = 0;
        if (min_level == 3) {
            return select;
        }
        for (const asst::RecruitCombs& comb : combinations) {
            if (comb.min_level < min_level) {
                return select;
            }
            emplace_back_count = 0;
            for (const RecruitConfig::TagId& tag : comb.tags) {
                if (unique_tags.find(tag) == unique_tags.cend()) {
                    unique_tags.insert(tag);
                    select.emplace_back(tag);
                    ++emplace_back_count;
                }
            }
            if (select.size() > 3) {
                while (emplace_back_count--) {
                    unique_tags.erase(select.back());
                    select.pop_back();
                }
            }
        }
    }
    return select;
}

template <typename Rng>
void asst::AutoRecruitTask::upload_result(const Rng& tag_ids, const json::value& yituliu_details)
{
    LogTraceFunction;
    if (m_upload_to_penguin) {
        upload_to_penguin(tag_ids);
    }
    if (m_upload_to_yituliu) {
        upload_to_yituliu(yituliu_details);
    }
}

template <typename Rng>
void asst::AutoRecruitTask::upload_to_penguin(Rng&& tags)
{
    LogTraceFunction;

    json::value body;
    body["server"] = m_server;
    body["stageId"] = "recruit";
    auto& all_drops = body["drops"];
    for (const auto& tag : tags) {
        all_drops.emplace(
            json::object {
                { "dropType", "NORMAL_DROP" },
                { "itemId", tag },
                { "quantity", 1 },
            });
    }
    body["source"] = UploadDataSource;
    body["version"] = MAA_VERSION;

    std::unordered_map<std::string, std::string> extra_headers;
    if (!m_penguin_id.empty()) {
        extra_headers = { { "authorization", "PenguinID " + m_penguin_id } };
    }

    std::string version = MAA_VERSION;
    if (version.find("DEBUG_VERSION") != std::string::npos) {
        version = "dev";
    }
    else if (!version.empty() && version[0] == 'v') {
        version.erase(0, 1);
    }

    version.erase(std::ranges::remove(version, ' ').begin(), version.end());

    extra_headers.insert({ "User-Agent", std::string("MaaAssistantArknights/") + version });

    if (!m_report_penguin_task_ptr) {
        m_report_penguin_task_ptr = std::make_shared<ReportDataTask>(report_penguin_callback, this);
    }

    m_report_penguin_task_ptr->set_report_type(ReportType::PenguinStats)
        .set_body(body.to_string())
        .set_extra_headers(extra_headers)
        .set_retry_times(3)
        .run();
}

void asst::AutoRecruitTask::upload_to_yituliu(const json::value& details)
{
    LogTraceFunction;

    json::value body = details;
    body["server"] = m_server;
    body["source"] = UploadDataSource;
    body["version"] = MAA_VERSION;
    body["uuid"] = m_yituliu_id;

    if (!m_report_yituliu_task_ptr) {
        m_report_yituliu_task_ptr = std::make_shared<ReportDataTask>(report_yituliu_callback, this);
    }

    m_report_yituliu_task_ptr->set_report_type(ReportType::YituliuBigDataAutoRecruit)
        .set_body(body.to_string())
        .set_retry_times(0)
        .run();
}

void asst::AutoRecruitTask::report_penguin_callback(AsstMsg msg, const json::value& detail, AbstractTask* task_ptr)
{
    LogTraceFunction;

    auto p_this = dynamic_cast<AutoRecruitTask*>(task_ptr);
    if (!p_this) {
        return;
    }

    p_this->callback(msg, detail);
}

void asst::AutoRecruitTask::report_yituliu_callback(AsstMsg msg, const json::value& detail, AbstractTask* task_ptr)
{
    LogTraceFunction;

    auto p_this = dynamic_cast<AutoRecruitTask*>(task_ptr);
    if (!p_this) {
        return;
    }

    p_this->callback(msg, detail);
}
