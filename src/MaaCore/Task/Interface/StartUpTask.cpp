#include "StartUpTask.h"

#include <utility>

#include "Config/GeneralConfig.h"
#include "Task/Miscellaneous/AccountSwitchTask.h"
#include "Task/Miscellaneous/StartGameTaskPlugin.h"
#include "Task/ProcessTask.h"
#include "Utils/Logger.hpp"

asst::StartUpTask::StartUpTask(const AsstCallback& callback, Assistant* inst) :
    InterfaceTask(callback, inst, TaskType),
    m_start_game_task_ptr(std::make_shared<StartGameTaskPlugin>(m_callback, m_inst, TaskType)),
    m_start_up_task_ptr(std::make_shared<ProcessTask>(m_callback, m_inst, TaskType)),
    m_account_switch_task_ptr(std::make_shared<AccountSwitchTask>(m_callback, m_inst, TaskType))
{
    LogTraceFunction;

    // 前两项认为用户已手动启动至首页，如果识别到了切换但不认为在首页说明主题不支持
    m_start_up_task_ptr
        ->set_tasks({ "StartAtHome", "StartWithSanity", "SwitchTheme@ToggleSettingsMenu", "StartUpBegin" })
        .set_times_limit("ReturnButton", 0)
        .set_times_limit("StartButton1", 0)
        .set_task_delay(Config.get_options().task_delay)
        .set_retry_times(50);
    m_start_game_task_ptr->set_retry_times(0);
    m_account_switch_task_ptr->set_retry_times(0);
}

bool asst::StartUpTask::run()
{
    LogTraceFunction;

    if (!m_enable) {
        Log.info("task disabled, pass", basic_info().to_string());
        return true;
    }

    if (!m_start_game_task_ptr->run()) {
        return false;
    }

    if (need_exit()) {
        return false;
    }

    // 切号（navigate_to_start_page 内部处理鹰角登录弹窗 / 资源完整性 / Settings→Account 等）
    if (m_account_switch_task_ptr->run()) {
        if (!m_start_up_task_ptr->run()) {
            return false;
        }
        return true;
    }

    if (!m_start_game) {
        LogInfo << __FUNCTION__ << "| StartUpTask failed, start_game is not enabled, don't restart game and just fail";
        return false;
    }

    Log.warn(__FUNCTION__, "| Login failed, entering game-restart loop");

    // feat/startup-restart-breaker: 连续 OCR/识别失败熔断
    // account_switch 连续失败 (UI 与 OCR 词表不匹配场景) 时, 跳出 restart_game 循环,
    // 避免 5x restart_game + restart_game 自身 sleep 3s 的纯空等.
    // start_up.run() 失败不计为 OCR 失败 (游戏崩溃场景保留 restart_game 重试).
    int consecutive_ocr_failures = 0;
    constexpr int OcrCircuitBreakerThreshold = 3;

    for (int attempts = 0; attempts < MaxRestartAttempts && !need_exit(); ++attempts) {
        Log.info(__FUNCTION__, "| Restarting game client (attempt", attempts + 1, "/", MaxRestartAttempts, ")");
        if (!m_start_game_task_ptr->restart_game()) {
            Log.warn(__FUNCTION__, "| restart_game failed, retrying");
            sleep(3000);
            continue;
        }

        if (!m_account_switch_task_ptr->run()) {
            consecutive_ocr_failures++;
            if (consecutive_ocr_failures >= OcrCircuitBreakerThreshold) {
                Log.error(__FUNCTION__, "| Account switch failed", consecutive_ocr_failures,
                    "times consecutively, breaking restart loop (likely UI/OCR mismatch)");
                break;
            }

            Log.warn(__FUNCTION__, "| Account switch failed after restart (consec",
                consecutive_ocr_failures, "/", OcrCircuitBreakerThreshold, ")");
            continue;
        }

        // 切号成功清零 OCR 失败计数, 后续 start_up 失败不计入熔断
        consecutive_ocr_failures = 0;

        Log.info(__FUNCTION__, "| Game restarted, retrying login navigation");
        if (m_start_up_task_ptr->run()) {
            return true;
        }
        Log.warn(__FUNCTION__, "| Login navigation failed again, restarting game");
    }

    return false;
}

bool asst::StartUpTask::set_params(const json::value& params)
{
    LogTraceFunction;

    std::string account_name = params.get("account_name", std::string());
    std::string client_type = params.get("client_type", std::string());

    if (!Config.get_package_name(client_type)) {
        return false;
    }

    m_start_game = params.get("start_game_enabled", false);
    m_start_game_task_ptr->set_client_type(client_type).set_enable(m_start_game);
    m_account_switch_task_ptr->set_enable(!account_name.empty());
    m_account_switch_task_ptr->set_account(std::move(account_name));
    m_account_switch_task_ptr->set_client_type(std::move(client_type));
    return true;
}
