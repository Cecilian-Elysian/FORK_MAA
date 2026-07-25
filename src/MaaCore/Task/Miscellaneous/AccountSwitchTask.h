#pragma once
#include "Task/AbstractTask.h"

#include <vector>

namespace asst
{
class AccountSwitchTask : public AbstractTask
{
public:
    using AbstractTask::AbstractTask;
    virtual ~AccountSwitchTask() noexcept override = default;

    void set_account(std::string account) { m_account = std::move(account); }

    void set_client_type(std::string client_type) { m_client_type = std::move(client_type); }

private:
    virtual bool _run() override;

    // 导航至账号管理页
    bool navigate_to_start_page();
    bool equal_current_account();
    bool equal_current_account_b();
    // 账号列表里面点登录
    bool click_manager_login_button();
    // 打开账号列表
    bool show_account_list();
    bool swipe_and_select(bool to_top = false);
    // 往下滑账号列表
    void swipe_account_list(bool to_top = false);
    // 识别并选择m_account
    bool select_account();

    // chore/account-cycle-cleanup: 集中可调参数 (D1)
    static constexpr int NavigateRetryTimes = 30;          // navigate_to_start_page 重试预算, 覆盖整个 SwitchAccount@StartUpBegin 链路
    static constexpr int LoginButtonRetryTimes = 3;         // 账号列表点登录按钮重试
    static constexpr int MaxSwipeAttempts = 20;             // 滑动找账号上限 (swipe_and_select 循环)
    static constexpr int SwipeIntervalMs = 200;             // select_account 之间的固定 sleep

    std::string m_account;
    std::string m_target_account;
    std::string m_client_type; // 客户端类型
    const std::vector<std::string> SupportedClientType = { "Official",
                                                           "Bilibili",
                                                           "txwy" /*, "YoStarEN", "YoStarJP", "YoStarKR" */ };
};
}
