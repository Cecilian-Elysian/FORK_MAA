# 下游改动文件清单

本仓库相对上游 `MaaAssistantArknights/MaaAssistantArknights` 的所有改动文件清单，由 `tools/gen-downstream-changes.py` 自动从 [`LOG.md`](../LOG.md) 的实施完成表格提取。

## 用法

在修改任何文件 **之前** 先查阅此清单：

- [OK] **不在清单中** = 上游原装代码，改动需谨慎（可能破坏上游兼容性）
- [TGT] **在清单中** = 本仓库改动过，确认你的改动是否与已有下游逻辑冲突
- [HOT] **高敏感**（被改 >= 3 次）= 多轮 feat/fix 反复动过，改动前优先读相关 LOG 段落

## 维护

每次 feat/fix 合并后，重跑脚本刷新：

```
py tools/gen-downstream-changes.py
```

共扫描 220 个表格行，聚合出 36 个唯一源文件路径。

## 仓库根（2 个文件）

### [HOT] `.gitignore` (x5)

| 操作 | 说明 |
|------|------|
| 删除 | 移除 `LOG.md`、`AGENTS.md` 忽略规则 |
| 修改 | 注释改为 `# Feature/fix working documents (local only, never committed)` |
| 修改 | (a) 追加 `/installer-build.log`（2.6 MB 临时日志不入仓）；(b) 修正第 510 行规则 `DependencySetup_依赖库安装.bat` 误伤 `tools/`，改为 `install/DependencySetup_*.bat`（只屏蔽 install/ 副本） |
| 修改 | 末尾追加 `/installer/`（产物不入仓），与 `install/` 一致不污染 git |
| 修改 | 追加运行时缓存忽略规则；追加 `.crush/` / `.claude/` / `.cursor/` 规则；追加 `LOG.md` / `AGENTS.md` 忽略 |

### [TGT] `VERSION` (x2)

| 操作 | 说明 |
|------|------|
| git rm | 仅 release 脚本读取，不再需要 |
| 新建 | 内容 `v6.14.0-fork.20260714`，作为 `MAA_HASH_VERSION` 和 zip 文件名单一来源 |

## `.github/`（1 个文件）

### [TGT] `.github/workflows/release-fork.yml` 

| 操作 | 说明 |
|------|------|
| git rm | fork 的 GitHub CI，本地运行不需要 |

## `docs/`（1 个文件）

### [TGT] `docs/zh-cn/protocol/integration.md` 

| 操作 | 说明 |
|------|------|
| 修改 | +1 字段说明 `expedite_min_level`,含 0/4/5/6 语义 |

## `resource/`（1 个文件）

### [HOT] `resource/tasks/tasks.json` (x6)

| 操作 | 说明 |
|------|------|
| 修改 | `LoginOther.next` 追加 `"AccountManagerPageConfirm"`; 新增 `AccountManagerPageConfirm` task（`baseTask: AccountManagerListAccount`, `action: DoNothing`） |
| 修改 | `AccountManagerOfficial` 与 `AccountManagerBili` 的 `text` 从 `["登录记录"]` → `["登录记录", "上次登录"]`；Doc 同步更新 |
| 修改 | `SwitchAccount@StartToWakeUp.next` 追加 `SwitchAccount@StartToWakeUpOCR` 兜底；新增 `SwitchAccount@StartToWakeUpOCR` OCR 任务 |
| cherry-pick from `784d9005f6` | `AccountManagerOfficial` 由 `{"roi":[570,165,140,80]}` 补全为 `{"Doc":"...","algorithm":"OcrDetect","text":["登录记录"],"roi":[237,50,771,242]}`（与 `AccountManagerBili` 对齐） |
| 待修改 | `AccountManagerOfficial` 补 OcrDetect 识别「登录记录」 |
| 修改 | `AccountManagerOfficial` 由 `{"roi":[570,165,140,80]}` 补全为 `{"Doc":"官方服账号切换界面识别，与 B 服统一 OCR「登录记录」","algorithm":"OcrDetect","text":["登录记录"],"roi":[237,50,771,242]}`（与 B 服 `AccountManagerBili` 对齐） |

## `src/`（26 个文件）

### [TGT] `src/MaaCore/Assistant.cpp` 

| 操作 | 说明 |
|------|------|
| 修改 | `AllTasksCompleted` 后立即设 `m_thread_idle=true`，修复第二轮 `AsstStart` 因竞态返回 false 的 bug |

### [TGT] `src/MaaCore/Task/Interface/RecruitTask.cpp` (x2)

| 操作 | 说明 |
|------|------|
| 修改 | +1 参数解析 `expedite_min_level`(默认 0);链式调用透传给 AutoRecruitTask |
| 解析 `expedite_min_level` 参数 | 新参数透传 |

### [HOT] `src/MaaCore/Task/Interface/StartUpTask.cpp` (x3)

| 操作 | 说明 |
|------|------|
| 改回原序 | `start_game → account_switch → start_up`；restart 循环也改回原序 |
| 修改 | `StartUpTask::run` 重排：`start_game → start_up → account_switch → start_up`；5x restart_game 循环内同样改为先登录再切号 |
| 修改 | `.set_task_delay(Config.get_options().task_delay * 2)` → `.set_task_delay(Config.get_options().task_delay)`，删 `* 2` |

### [HOT] `src/MaaCore/Task/Miscellaneous/AccountSwitchTask.cpp` (x6)

| 操作 | 说明 |
|------|------|
| 回退 | `set_retry_times(5)` → `set_retry_times(30)` + 注释更新 |
| 修改 | `last_name` 白名单追加 `"AccountManagerPageConfirm"` |
| cherry-pick from `784d9005f6` | `navigate_to_start_page()` 加 `Log.info(... last matched task ...)` 诊断日志；4 个 `else if` 合并为单 `if (... \\|\\| ... \\|\\| ... \\|\\| ...)` |
| 待修改 | `navigate_to_start_page()` 加诊断日志 |
| 修改 | `navigate_to_start_page()` 在 `get_last_task_name()` 之后追加 `Log.info(__FUNCTION__, "last matched task:", last_name);`，便于后续识别失败时定位 |
| 修改 | 4 个 `else if` 合并为单 `if (... \|\| ... \\|\\| ... \\|\\| ...)`，减少分支嵌套 |

### [HOT] `src/MaaCore/Task/Miscellaneous/AutoRecruitTask.cpp` (x10)

| 操作 | 说明 |
|------|------|
| 删除 | 移除 `confirm()` 之前的加急块 |
| 插入 | 在 `confirm()` 成功后、`return` 前插入新加急块, 含「立即完成需在主页」注释 |
| 修改 | +1 setter 实现 `set_expedite_min_level` |
| 修改 | `_run` 主循环移除 `try_use_expedited` 局部变量,加急判定改为**每次进入前重新求值** `m_use_expedited && m_last_confirmed_min_level >= m_expedite_min_level`;加急成功后立即重置 `m_last_confirmed_min_level = 0` 防止陈旧状态被复用;加急失败时显式退出以避免阈值=0 时死循环 |
| 修改 | `recruit_one` 开头重置 `m_last_confirmed_min_level = 0`,仅当 `recruit_calc_task` 走到 success / nothing_to_select 路径时才会被重新赋值 |
| 修改 | `recruit_calc_task` 在 `nothing_to_select` 与 `success` 两条返回路径前赋值 `m_last_confirmed_min_level = final_combination.min_level` |
| 新增 `set_expedite_min_level` 实现 | setter |
| `_run()` 移除旧的 `try_use_expedited` 块 | 改由 `recruit_one()` 内逐槽判定 |
| `recruit_calc_task()` 写入 `m_last_confirmed_min_level` | 加急决策依据 |
| `recruit_one()` 加急分支 | 4★+ 时 `recruit_now()` 替代 `confirm()` |

### [HOT] `src/MaaCore/Task/Miscellaneous/AutoRecruitTask.h` (x3)

| 操作 | 说明 |
|------|------|
| 修改 | +1 setter 声明 `set_expedite_min_level`;+2 成员 `m_expedite_min_level`(默认 0)、`m_last_confirmed_min_level`(默认 0) |
| 新增 setter `set_expedite_min_level` | 加急门槛接口 |
| 新增成员 `m_expedite_min_level` / `m_last_confirmed_min_level` | 门槛值与最近确认星级 |

### [TGT] `src/MaaUtils` 

| 操作 | 说明 |
|------|------|
| 子模块初始化 | 引用上游 `MaaXYZ/MaaUtils`（HEAD `0c2556cfc`），提交至 feat/fix 索引 |

### [TGT] `src/MaaWpfGui/Configuration/Single/MaaTask/RecruitTask.cs` (x2)

| 操作 | 说明 |
|------|------|
| 修改 | +1 字段 `ExpediteMinLevel`(默认 0) |
| +`ExpediteMinLevel` 属性 | 配置模型 |

### [TGT] `src/MaaWpfGui/Configuration/Single/MaaTask/StartUpTask.cs` (x2)

| 操作 | 说明 |
|------|------|
| 修改 | 新增 `LateStageRogueAndReclamation : bool = false`,默认关闭以保持向后兼容 |
| 修改 | 添加 `AccountCycleEnabled` (bool, 默认 true) 和 `AccountNames` (List\<string\>, 默认 ["", ""]) |

### [TGT] `src/MaaWpfGui/MaaWpfGui.csproj` (x2)

| 操作 | 说明 |
|------|------|
| 修改 | `SelfContained` 改为 `false`，禁用 NetBeauty2 打包（不兼容 .NET 10.0.300） |
| 修改 | 版本号从 0.0.1 改为 6.14.0 |

### [TGT] `src/MaaWpfGui/Main/AsstProxy.cs` (x2)

| 操作 | 说明 |
|------|------|
| 修改 | `AllTasksCompleted` 回调中补上轮换推进逻辑：正常完成时调用 `MarkAccountCompleted` + `GetCurrentCycleAccount` + `LinkStart`，并 `break` 跳过标准完成日志，防止新一轮启动后仍打出"所有任务完成" |
| 修改 | `AllTasksCompleted` 回调调 `AdvanceAccountCycle` 替代 `SetStopped` |

### [TGT] `src/MaaWpfGui/Models/AccountCycleItem.cs` 

| 操作 | 说明 |
|------|------|
| 新建 | 轮换账号数据模型（DisplayName / AccountName / IsSelected / IsCompleted / Index） |

### [TGT] `src/MaaWpfGui/Models/AccountCycleStep.cs` 

| 操作 | 说明 |
|------|------|
| 新建 | `record AccountCycleStep(string AccountName, int Phase)`,步骤扁平列表的载体 |

### [TGT] `src/MaaWpfGui/Models/AsstTasks/AsstRecruitTask.cs` (x2)

| 操作 | 说明 |
|------|------|
| 修改 | +1 字段 `ExpediteMinLevel`;`Serialize` 始终写入 `expedite_min_level` 到 params |
| +`ExpediteMinLevel` 属性 + 序列化 | DTO |

### [HOT] `src/MaaWpfGui/Res/Localizations/en-us.xaml` (x3)

| 操作 | 说明 |
|------|------|
| 修改 | 同上(英文) |
| 修改 | 同上（英文） |
| +`ExpediteMinLevel*` 6 个 key | 五语本地化 |

### [HOT] `src/MaaWpfGui/Res/Localizations/ja-jp.xaml` (x3)

| 操作 | 说明 |
|------|------|
| 修改 | 同上(日文) |
| 修改 | 同上（日文） |
| +`ExpediteMinLevel*` 6 个 key | 五语本地化 |

### [HOT] `src/MaaWpfGui/Res/Localizations/ko-kr.xaml` (x3)

| 操作 | 说明 |
|------|------|
| 修改 | 同上(韩文) |
| 修改 | 同上（韩文） |
| +`ExpediteMinLevel*` 6 个 key | 五语本地化 |

### [HOT] `src/MaaWpfGui/Res/Localizations/zh-cn.xaml` (x4)

| 操作 | 说明 |
|------|------|
| 修改 | +5 string key:`ExpediteMinLevelLabel` / `ExpediteMinLevelTip` / `ExpediteMinLevel_4Plus` / `ExpediteMinLevel_5Plus` / `ExpediteMinLevel_6Plus` |
| 修改 | +2 string key:`LateStageRogueAndReclamation` / `LateStageRogueAndReclamationTip` |
| 修改 | 添加 7 个 AccountCycle 本地化 key |
| +`ExpediteMinLevel*` 6 个 key | 五语本地化 |

### [HOT] `src/MaaWpfGui/Res/Localizations/zh-tw.xaml` (x3)

| 操作 | 说明 |
|------|------|
| 修改 | 同上(繁体) |
| 修改 | 同上（繁体中文） |
| +`ExpediteMinLevel*` 6 个 key | 五语本地化 |

### [TGT] `src/MaaWpfGui/ViewModels/UI/RootViewModel.cs` (x2)

| 操作 | 说明 |
|------|------|
| 修改 | 版本比较时 `uiVersion` 也 `TrimStart('v', 'V')`，修复 UI 和 Core 版本号一致仍弹警告的 bug |
| 修改 | 版本比较忽略 `v` 前缀 |

### [HOT] `src/MaaWpfGui/ViewModels/UI/TaskQueueViewModel.cs` (x23)

| 操作 | 说明 |
|------|------|
| 修改 | `LinkStart` 改为 `RebuildCycleSteps` + 取 `CurrentStep` 决定首个账号 |
| 修改 | `LinkStartWithTasks` foreach 新增 Phase 过滤(`IsInCurrentPhase` 由 `lateStageOn` 闸门,LateStage 关闭时 no-op) |
| 修改 | `AdvanceAccountCycle` 全量重写:扁平步骤推进 + `needStartupSwitch` 显式切号 + 空步骤递归跳过 + `MarkAccountCompleted` 按 LateStage 状态差异化触发 |
| 修改 | 新增静态助手 `IsInCurrentPhase(TaskType, int phase)` |
| 修改 | 同一处版本比较，补上 `uiVersion.TrimStart` |
| 修改 | `SetStopped` 中 `IsCycling` 短路分支增加"是否被强制停止"判断：`runStopScript && _runningState.GetStopping()` 时落空 `IsCycling` 走完整重置流程,清 `Stopping` 标志;正常轮换推进路径保持不变(直接 return)。修复点停止按钮后 UI 永远卡在"正在停止"且按钮不可用的问题。 |
| 修改 | `AdvanceAccountCycle` 两个失败分支(`count == 0` 无任务被附加、`AsstStart()` 失败)改为调用 `SetStopped(runStopScript: false)`,统一重置 `Stopping/Idle/IsCycling`。修复"切换第二个账号任务出错"后状态卡住、按钮变灰、标题不恢复的问题。 |
| 修改 | `LinkStart` 补上 `AccountSwitchEnabled = true`；`TryStartNextCycleAccount` 处理 `cfg` 为 null 的边界情况；包裹 try-catch 防止 `async void` 静默吞异常；通过 `Execute.OnUIThreadAsync` 确保 UI 线程执行 |
| 修改 | 新增 `AdvanceAccountCycle()` 方法替代 `SetStopped` 做轮换推进；`SetStopped` 剥离轮换逻辑，只处理停止 |
| 修改 | `SetStopped` 新增轮换逻辑：完成任务后调用 `MarkAccountCompleted` 标记当前账号完成，若还有未完成账号则自动触发 `LinkStart` 继续下一账号 |
| 修改 | LinkStart 加入轮换判定，SetStopped 后调用 TryStartNextCycleAccount 自动推进 |
| 修改 | `AdvanceAccountCycle` 文档注释追加 fix/defer-rogue/1 段落 |
| 修改 | **A1**: 把 `prevStep = GetPreviousStep()` 移到 `nextStep == null` 早退分支**之前**;早退分支里先调用 `MarkPreviousStepCompleted(prevStep)` 再 `return` |
| 修改 | **A1**: 普通推进路径移除原 inline 块,改为调用 `MarkPreviousStepCompleted(prevStep)` |
| 修改 | **A1**: 新增私有方法 `MarkPreviousStepCompleted(AccountCycleStep?)`,语义与原 inline 块一致(`leftPhase2 \\|\\| lateStageOff`) |
| 修改 | **A8**: `LinkStart` 顶部加 `if (startUpConfig.IsCycling) { Release; return; }` guard,防止 Stop 后再次点击 / 定时器 / 快捷键在 cycle 中重置进度 |
| 修改 | **#6**: `SetStopped` 将 cycling 检查移到 idle 检查之前,当 `IsCycling=true && Idle=true`（LinkStartWithTasks 早退路径）时清理 cycling 状态,让正常停止接管,防止轮换永久卡住 |
| 修改 | **#5**: `AdvanceAccountCycle` 入口加 `_logger.Information` 日志记录 stepIdx/prev/next 信息 |
| 修改 | **#5**: AdvanceAccountCycle 循环后追加 `_logger.Information` 记录 phase/switch/count/ret |
| 修改 | **#5**: AdvanceAccountCycle 中 Append task 时记录 `[CycleAdv] Append task #Idx` 日志 |
| 修改 | **#5**: LinkStartWithTasks 中 Append task 时记录 `[LinkStart] Append task #Idx` 日志 |
| 修改 | **#2/#3**: AdvanceAccountCycle 的 Phase 任务循环由 foreach + `IndexOf` 改为 **for 循环** (`int index = i`),消除重复项/顺序变更时的索引错误;同时保持原有 Phase 过滤/StartUp 跳过/`SetTaskIds` 逻辑不变 |
| 修改 | **#4**: AdvanceAccountCycle 初始日志追加 `idx={CurrentStepIndex}/{CurrentStepCount}` 显示步骤位置 |

### [TGT] `src/MaaWpfGui/ViewModels/UserControl/TaskQueue/RecruitSettingsUserControlModel.cs` (x2)

| 操作 | 说明 |
|------|------|
| 修改 | +3 VM 成员:`ExpediteMinLevelEnabled`(布尔,setter 控制 0/4 切换)、`ExpediteMinLevel`(int,setter 白名单 0/4/5/6)、`ExpediteMinLevelOptions`(4/5/6 三档 ComboBox 选项);Serialize 阶段写入 `ExpediteMinLevel` |
| +`ExpediteMinLevelList` / `UseExpeditedMinLevel` / `UseExpeditedMinLevelVisible` | ViewModel |

### [HOT] `src/MaaWpfGui/ViewModels/UserControl/TaskQueue/StartUpSettingsUserControlModel.cs` (x5)

| 操作 | 说明 |
|------|------|
| 修改 | (a) `LateStageRogueAndReclamation` VM 属性(照搬 `AccountSwitchEnabled`);(b) 新增 `#region Late Stage` 含 `_cycleSteps` / `_currentStepIndex` / `RebuildCycleSteps` / `AdvanceStepIndex` / `CurrentStep` / `GetPreviousStep` / `CurrentPhase`;(c) `ResetCycle` 同步清空步骤列表 |
| 修改 | `GetCurrentCycleAccount` 简化：去掉 `_currentCycleIndex` 状态跟踪，改为直接取第一个符合条件的账号；去掉 `ResetCycleIndex` 方法 |
| 修改 | `SyncAccountNamesToItems` 保留已有项 `IsSelected` 状态，用户可自由勾选参与轮换的账号 |
| 修改 | 添加轮换 CRUD、GetCurrentCycleAccount、MarkAccountCompleted、SyncAccountNamesToItems 等方法 |
| 修改 | **#5**: 新增 `CurrentStepIndex` 公开属性支持日志 |

### [TGT] `src/MaaWpfGui/ViewModels/UserControl/TaskQueue/UserDataUpdateSettingsUserControlModel.cs` 

| 操作 | 说明 |
|------|------|
| 修改 | **#1**: cycle 中 (`GetAccountSwitchEnabled()`) 跳过 `IsTriggerDue` 检查,保证每个账号的 OperBox/Depot 子任务都被追加 |

### [TGT] `src/MaaWpfGui/Views/UserControl/TaskQueue/RecruitSettingsUserControl.xaml` (x2)

| 操作 | 说明 |
|------|------|
| 修改 | 高级设置区末尾追加 CheckBox + ComboBox;整行 Visibility 绑定到 `UseExpeditedWithNull == true` |
| +门槛下拉框 | UI |

### [HOT] `src/MaaWpfGui/Views/UserControl/TaskQueue/StartUpTaskUserControl.xaml` (x4)

| 操作 | 说明 |
|------|------|
| 修改 | AccountCycle 子面板末尾新增 CheckBox + TooltipBlock(长 Wrap + MaxWidth CalcBinding 防挤压) |
| 修改 | 添加/删除按钮图标统一字号和居中 |
| 修改 | 添加轮换 CheckBox、账号列表 ItemsControl、编辑模式 ComboBox、IsCompleted 蓝色高亮 |
| 修改 | **A7**: `LateStageRogueAndReclamation` CheckBox 加 `IsEnabled="{c:Binding '!IsCycling'}"`,Cycle 运行中灰显 |

## `tools/`（5 个文件）

### [TGT] `tools/DependencySetup_依赖库安装.bat` (x2)

| 操作 | 说明 |
|------|------|
| git rm | 终端用户依赖安装脚本，不再需要 |
| 还原 | 从 `install/` 副本拷回 `tools/`，恢复 `Copy-Item` 源；并入 git |

### [TGT] `tools/local-install-staging.bat` (x2)

| 操作 | 说明 |
|------|------|
| 新建 | 基于 `local-install.bat`，6 处 `install` 路径改为 `install-staging`（cmake `--prefix`、dotnet `-o`、nbeauty 补丁、清理 `*.h`/`msvc-debug`、robocopy `resource`） |
| 修改 | 同上 |

### [TGT] `tools/local-install.bat` 

| 操作 | 说明 |
|------|------|
| 修改 | `global.json` 注入从 `{"version":"10.0.203","rollForward":"disable"}` 改为 `{"version":"10.0.100","rollForward":"latestFeature"}` |

### [TGT] `tools/release-zip.bat` (x2)

| 操作 | 说明 |
|------|------|
| git rm | 发布打包脚本，不再需要 |
| 新建 | bat 外壳，调 ps1 后 `pause`；失败时 `errorlevel` 透传 |

### [HOT] `tools/release-zip.ps1` (x3)

| 操作 | 说明 |
|------|------|
| git rm | 发布打包脚本，不再需要 |
| 修改 | 步骤 2 改单目标 cmake build；步骤 6 改 global.json 为 `10.0.100 + latestFeature`；步骤 8 staging `/XD` 加 `.git`；脚本注释说明改动原因 |
| 新建 | 核心 PowerShell 脚本（~180 行），`-Version` / `-SkipBuild` / `-KeepInstallerDir` 三个开关 |

