# 修改日志

日志规范：每次修改文件后，在此记录修改内容。

## 2026-07-27

### tools/local-install-staging.bat 创建

`branch` 分支使用 `install/` 作为构建输出目录。新增 `tools/local-install-staging.bat`，结构与 `local-install.bat` 一致，仅将 6 处 `install` 路径改为 `install-staging`，输出到独立目录与 `install/` 并存，互不覆盖。`branch` 与 `staging` 分支各自独立构建，桌面分别快捷。

| # | 文件 | 操作 | 说明 |
|---|------|------|------|
| 1 | `tools/local-install-staging.bat` | 新建 | 基于 `local-install.bat`，6 处 `install` 路径改为 `install-staging`（cmake `--prefix`、dotnet `-o`、nbeauty 补丁、清理 `*.h`/`msvc-debug`、robocopy `resource`） |
| 2 | `LOG.md` | 修改 | 本节 |

**用法**:
- `branch` 分支：`tools\local-install.bat` → `install\MAA.exe`
- `staging` 分支：`tools\local-install-staging.bat` → `install-staging\MAA.exe`

### staging 桌面快捷方式

`%USERPROFILE%\Desktop\MAA (staging).lnk`：指向 `install-staging\MAA.exe`，起始目录 `install-staging\`，图标复用 MAA.exe 自身图标。首次执行前需在 `staging` 分支上先运行 `tools\local-install-staging.bat` 生成构建产物，否则双击报错「找不到文件」。

| # | 路径 | 操作 | 说明 |
|---|------|------|------|
| 1 | `%USERPROFILE%\Desktop\MAA (staging).lnk` | 创建 | PowerShell `WScript.Shell.CreateShortcut()`，目标 `install-staging\MAA.exe`，起始目录 `install-staging\` |
| 2 | `LOG.md` | 修改 | 本节 |

### tools/local-install{,-staging}.bat global.json 版本锁定修复

首次跑 `tools\local-install-staging.bat` 构建 `install-staging/` 时，**C++ 端构建成功但 WPF `dotnet restore`/`publish` 全失败**，脚本走到 `:error` 退出，`install-staging/` 未生成。撞到 AGENTS.md §4.1 记录的 VS 2026 SDK 路径 bug 与一个之前未记录的脚本坑叠加。

**根因**：`local-install.bat` 第 11 行（`local-install-staging.bat` 同位置）原写：

```bat
> ".\global.json" echo {"sdk":{"version":"10.0.203","rollForward":"disable"}}
```

- `"version":"10.0.203"` 锁死 10.0.203，但本机只装 10.0.300 SDK（`dotnet --list-sdks`）
- `"rollForward":"disable"` 禁用 fallback，连 `latestPatch` / `latestFeature` / `latestMajor` 都不允许
- MSBuild 找不到 SDK → `MSB4276` / `无法解析 SDK"Microsoft.NET.Sdk"` 报错
- 该 `global.json` 由脚本注入到仓库根目录，覆盖所有子目录（`src/MaaWpfGui/`）
- cmake `--build build --parallel` 触发 WPF MSBuild 评估（已知 bug），整链路连锁失败

**修复**：将两个 bat 的第 11 行改为与 AGENTS.md §4.1 描述一致的写法：

```bat
> ".\global.json" echo {"sdk":{"version":"10.0.100","rollForward":"latestFeature"}}
```

`10.0.100` + `latestFeature` 允许自动晋升到 10.0.x（包含本机的 10.0.300），向后兼容所有 dotnet 10.x 装机版本。

**首次成功构建 `install-staging/` 的绕过方法**（在脚本修复前手动执行）：

```bash
cmake --install build --config RelWithDebInfo --prefix install-staging
dotnet restore src/MaaWpfGui/MaaWpfGui.csproj
dotnet publish src/MaaWpfGui/MaaWpfGui.csproj -c Release -r win-x64 -o install-staging /p:DisableBeauty=True
& "$env:USERPROFILE\.nuget\packages\nulastudio.netbeauty\2.1.5\tools\win-x86\nbeauty2.exe" --usepatch "$PWD\install-staging\." ./externals
# 清理 *.h / msvc-debug / robocopy resource（与 bat 一致）
```

绕开脚本注入的 global.json，让 dotnet 自动选用本机最高 SDK。

| # | 文件 | 操作 | 说明 |
|---|------|------|------|
| 1 | `tools/local-install.bat:11` | 修改 | `global.json` 注入从 `{"version":"10.0.203","rollForward":"disable"}` 改为 `{"version":"10.0.100","rollForward":"latestFeature"}` |
| 2 | `tools/local-install-staging.bat:11` | 修改 | 同上 |
| 3 | `install-staging/` | 构建产出 | C++ / WPF 全量构建成功，8775 文件与 `install/resource/` 完全一致；`MAA.exe` 339 KB / `MaaCore.dll` 4.0 MB / `MAA.dll` 3.6 MB |
| 4 | `LOG.md` | 修改 | 本节 |

## 2026-07-25

### staging 分支引入 + fix/expedite-threshold 重命名

**背景**：`fix/expedite-threshold` 累积了 11 个跨方向 commit（启动链 / 切号 OCR / 加急门槛 / recruit_now 顺序 / docs），已不适合作为单一 fix 分支命名。引入 `staging` 层作为 feat / fix 的合并目标与 `branch` 之间的待验证整合区，攒批测试通过后晋升至 `branch`。

**工作流变更**：

```
master (上游 dev-v2 镜像)
  │  (rebase / merge 同步节奏不变)
  ▼
branch (稳定下游基线) ◄──── staging 晋升 (--no-ff, 攒批)
  │                                 ▲
  │ (feat / fix 拉取源)              │ (合并目标)
  ▼                                 │
feat/<name>, fix/<name> ────────────┘
```

- 所有 feat / fix 一律从 `branch` 拉出，合并到 `staging`
- `staging` 攒一批（建议 3-5 个）测试通过后再晋升至 `branch`
- `branch` ↔ `master` 上游同步节奏不变
- 当前 `staging` 领先 `branch` 11 commits、落后 2 commits（branch 上的 `784d9005f6` + `da157d163d` 与 staging 上的 cherry-pick `6011051af2` + `f241b2160b` 内容等价、SHA 不同），首次晋升将无法 FF，需 `--no-ff`

| # | 文件 | 操作 | 说明 |
|---|------|------|------|
| 1 | 本地 git 分支 | 重命名 | `fix/expedite-threshold` → `staging` |
| 2 | `Github` 远端 | 推送 | 新增 `staging` 分支；旧名 `fix/expedite-threshold` 未推过远端，无需删除 |
| 3 | `AGENTS.md §2.2` | 修改 | 新增 `staging` 行；`branch` 备注「本地下游整合」→「稳定下游基线」 |
| 4 | `AGENTS.md §2.3` | 修改 | 「feat 合并到 `branch`」→「feat 合并到 `staging`」 |
| 5 | `AGENTS.md §2.4` | 新增 | staging 工作流（拓扑 / 规则 / 当前待验证内容） |
| 6 | `AGENTS.md §3.2` | 修改 | feat 流程步骤 6 合并目标 `branch` → `staging` |
| 7 | `AGENTS.md §3.3` | 修改 | fix 合并目标补「修 branch 自身的 fix → 合并到 `staging`」 |
| 8 | `AGENTS.md §6` | 修改 | 删除 `fix/expedite-threshold` 行（staging 是长期角色，不属于 feat/fix 速查） |
| 9 | `LOG.md` | 修改 | 本节 |

**推 upstream**：仅本 fork 工作流调整，不推。

### fix/expedite-threshold recruit_now 调用顺序修复

`feat/expedite-threshold`（`7df4e94e3f`）重构时把 `recruit_now()` 从 `_run()` 外层循环挪进 `recruit_one()`,但挪到了 `confirm()` 之前。游戏 UI 规则:「立即招 / 立即完成」按钮只存在于公招主页(slot 已开始 9h 倒计时), 详情页(confirm 之前)无此按钮。导致 `RecruitNow` task 的 OCR `["立即招"]` 在 ROI `[0,300,1280,420]` 内 4 次 retry 全空, `recruit_now()` 必失败, 加急判定通过却实际未加急, slot 始终走 9h 倒计时。

`install/debug/asst.bak.log` 多日复现(line 158873-158897, 2026-07-25 14:03:46-48):
- 加急判定日志 `Recruit slot level 4 >= expedite threshold 4 , using expedited plan.` 正常打印
- OCR 实际识别文本为「已招募干员 / 远程位 / 近战/回复 / 开始刷新标签 / 招募预期」等详情页元素
- 4 次 retry 后 `SubTaskError`, `Failed to use expedited plan, fall back to normal confirm.`
- 随后 `check_timer` + `RecruitConfirm` 走完正常 9h 确认流程

修复: 把加急块从 `confirm()` 之前挪到 `confirm()` 之后, 恢复 `3529ab0f05` 原版的「先确认启动 9h, 再主页点立即完成」语义。`fix/expedite-threshold` 既有的 `m_last_confirmed_min_level` 两处重置(line 312-314 / 358-359)保持不动。

| # | 文件 | 操作 | 说明 |
|---|------|------|------|
| 1 | `src/MaaCore/Task/Miscellaneous/AutoRecruitTask.cpp:353-365` | 删除 | 移除 `confirm()` 之前的加急块 |
| 2 | `src/MaaCore/Task/Miscellaneous/AutoRecruitTask.cpp:389` | 插入 | 在 `confirm()` 成功后、`return` 前插入新加急块, 含「立即完成需在主页」注释 |
| 3 | `LOG.md` | 修改 | 本节 |
| 4 | `AGENTS.md §6` | 修改 | `fix/expedite-threshold` 角色描述追加「；recruit_now 移到 confirm 之后（修详情页无「立即招」按钮导致加急必失败）」 |

**推 upstream**: 仅本 fork 修复, 不推。

### fix/account-switch-retry LoginOther OCR 模板兜底 + retry_times 分析修正

**初版误判**（`cd704f8bbc`）: 将 `navigate_to_start_page` 的 `retry_times` 从 30 降至 5, 以为 30 次 retry 全花在 LoginOther OCR。实测失败（`asst.log` 19:18:57）: `last matched task: SwitchAccount@StartUpBegin`, 导航首步就耗光 5 次 retry, `TaskChainError`。

**根因修正**: 阅读 `ProcessTask::find_and_run_task()`（`ProcessTask.cpp:336-380`）发现 `cur_retry` 是**局部变量**, 每次 `run()` 循环调用 `find_and_run_task()` 时**独立从 0 开始**。链路每一步各自享有完整的 `m_retry_times` 预算。导航首步（`SwitchAccount@StartUpBegin` → 22 个 `next` 候选）最坏需要 ~13 次 retry 才有 UI 元素可识别, 5 次远远不够。

**正确修法**: 保留 `retry_times=30`（导航余量不变）, 在 `LoginOther.next` 追加 `AccountManagerPageConfirm`（`baseTask: AccountManagerListAccount` + `action: DoNothing`）。OCR 失败时同一 retry cycle 内模板匹配兜底命中（日志历史 score 0.93 稳定）, 不再空耗 30 × 0.6s = 18s retry。由于 `action: DoNothing`, 不改变 UI 状态, 后续 `equal_current_account()` / `show_account_list()` 不受影响。

| # | 文件 | 操作 | 说明 |
|---|------|------|------|
| 1 | `src/MaaCore/Task/Miscellaneous/AccountSwitchTask.cpp:68` | 回退 | `set_retry_times(5)` → `set_retry_times(30)` + 注释更新 |
| 2 | `src/MaaCore/Task/Miscellaneous/AccountSwitchTask.cpp:74` | 修改 | `last_name` 白名单追加 `"AccountManagerPageConfirm"` |
| 3 | `resource/tasks/tasks.json:808-817` | 修改 | `LoginOther.next` 追加 `"AccountManagerPageConfirm"`; 新增 `AccountManagerPageConfirm` task（`baseTask: AccountManagerListAccount`, `action: DoNothing`） |
| 4 | `LOG.md` | 修改 | 本节（替换初版的 retry_times=5 描述） |
| 5 | `AGENTS.md §6` | 修改 | `fix/account-switch-retry` 描述更新 |

**预期效果**: 每次 `navigate_to_start_page` 的 LoginOther 阶段从 ~18s（30 retry × 0.6s）降至 ~0.1s（首个 cycle 模板命中）, 每次 -18s, 两账号 -36s。导航阶段不受影响（retry_times=30 不变）。

**推 upstream**: 仅本 fork 修复, 不推。

## 2026-07-24

### fix/expedite-threshold 账号列表 OCR 适配 UI 改版

**修订上一个 commit (`2715162c3d`)** 的切号链修复。上一版在切号链加了 `SwitchAccount@StartToWakeUpOCR` OCR 兜底以处理鹰角登录弹窗场景，但**主路径 `AccountManagerOfficial` 的 OCR 文本仍是单文本「登录记录」**，与鹰角登录账号列表页改版后的实际 UI 不匹配——用户实际账号列表显示的是「**上次登录 X 分钟前**」而非「登录记录」，导致切号链最终 OCR 检查失败、30 retry 全失败、5x restart_game 死循环。

用户提供的截图证据：
- 截图 1（主菜单）：「开始唤醒」+「账号管理」按钮
- 截图 2（鹰角登录弹窗）：「192****6952 (最近)」+「登录」
- 截图 3（账号列表）：**3 个账号行**：
  - 192****6952 (最近)
  - 192****6952 (上次登录 9分钟前)
  - 189****0830 (上次登录 39分钟前)

**用户提示关键差异**：
- 「上次登录」字样：经常登录的账号显示
- 「登录记录」字样：长时间不登陆的账号显示

**修复**：`AccountManagerOfficial.text` 与 `AccountManagerBili.text` 从单文本 `["登录记录"]` 改为双文本 `["登录记录", "上次登录"]`，覆盖新旧 UI 两种显示模式。

| # | 文件 | 操作 | 说明 |
|---|------|------|------|
| 1 | `resource/tasks/tasks.json:813-824` | 修改 | `AccountManagerOfficial` 与 `AccountManagerBili` 的 `text` 从 `["登录记录"]` → `["登录记录", "上次登录"]`；Doc 同步更新 |
| 2 | `AGENTS.md §6` | 修改 | `fix/expedite-threshold` 角色描述追加「;账号列表 OCR 适配 UI 改版（登录记录 / 上次登录 双文本兜底）」 |
| 3 | `LOG.md` | 修改 | 本节 |

**行为矩阵**：

| 账号使用频率 | 显示文本 | 改前 | 改后 |
|-------------|---------|------|------|
| 经常登录 | 「上次登录 X 分钟前」 | ✗ OCR 不命中 → 30 retry 失败 | ✓ 命中 |
| 长时间未登录 | 「登录记录」 | ✓ 命中（保留） | ✓ 命中 |
| 混合账号列表 | 同时出现两种 | 部分命中 | 全部命中 |

**风险评估**：
- 双文本误匹配：`fullMatch: true` 保留 + ROI `[237, 50, 771, 242]` 限定顶部标题栏；其他页面无「登录记录」/「上次登录 X 分钟前」字样
- 与 upstream 偏离：upstream 仍单文本 `["登录记录"]`；本 fork 因 UI 改版适配，**不推 upstream**
- ROI 微调：当前 ROI 与原版一致；如新 UI 账号位置超出 ROI 再单独调整

**预期效果**：
- 多账号切号 + 主菜单（截图 1）→ 点击「账号管理」→ 账号列表（截图 3）→ OCR 命中「上次登录」→ 选号 → 「登录」→ home
- 全程 30-40s 内完成，无 20s 等待

**推 upstream**：仅本 fork 修复，不推。

### fix/expedite-threshold StartUp::run 恢复原序 + 切号链 OCR 兜底

**修订上一个 commit (`3f411e494a`)**。上一版将 `StartUpTask::run` 重排为 `start_game → start_up → account_switch → start_up`，在 StartUpTask 层显式调用 `start_up` 处理登录前的鹰角弹窗。但用户反馈该架构过于侵入式——切号应该「先进入账号管理再切账号」，由 `AccountSwitchTask::navigate_to_start_page` 内部负责导航到 AccountManager，而非在 StartUpTask 层拆出登录步骤。

**新架构（恢复切号原序 + 切号链补 OCR 兜底）**：

```cpp
// StartUpTask::run 主流程（恢复原序）
start_game → account_switch → start_up

// restart_game 循环（恢复原序）
restart_game → account_switch → start_up
```

切号 `navigate_to_start_page` 内部走 `SwitchAccount@StartUpBegin` 链，原链中 `SwitchAccount@StartToWakeUp` 仅模板匹配（`tasks.json:789-794`），**缺 OCR 兜底**——当游戏停在鹰角登录弹窗（`StartToWakeUp.png` 不匹配）时无 fallback，导致 30 retry 全失败、5x restart_game 死循环、20s 等待。

**修复**：参考 `StartUpThemes` 的 `StartToWakeUp` + `StartToWakeUpOCR` 配对模式，让切号链也支持 OCR 兜底：

```json
"SwitchAccount@StartToWakeUp": {
    "template": "StartToWakeUp.png",
    "action": "DoNothing",
    "next": ["AccountManager", "SwitchAccount@StartToWakeUpOCR"]   // 新增 OCR 兜底
},
"SwitchAccount@StartToWakeUpOCR": {
    "baseTask": "SwitchAccount@StartToWakeUp",
    "algorithm": "OcrDetect",
    "text": ["开始唤醒", "登录", "登", "录"],
    "fullMatch": true,
    "roi": [373, 145, 535, 430]
}
```

`next` 顺序：`AccountManager` 在前（模板匹配时直接走），`SwitchAccount@StartToWakeUpOCR` 在后（兜底）；与 `StartUpThemes` 一致。

**4 类场景行为矩阵**：

| 场景 | 上一个 commit (`3f411e494a`) | 本 commit |
|------|------------------------------|----------|
| 多账号切号 + 鹰角登录弹窗 | 显式 start_up 点「登录」→ 切号 → 再次登录 | 切号链 OCR 命中「登录」→ 点击 → AccountManager → 切号 → 登录 |
| 多账号切号 + 已在主界面 | 显式 start_up 走完 → 切号 → 再次登录 | 切号链直接 Settings → Account → 切号 → 登录（少一次 start_up） |
| 单账号 + 鹰角登录弹窗 | start_up 先登录 → 切号（disabled）→ 再次登录 | 切号链 OCR 命中 → AccountManager（disabled）→ 切号（disabled）→ 登录 |
| 单账号 + 已在主界面 | 显式 start_up → 切号（disabled）→ 再次登录 | 切号链 Settings → Account（disabled）→ 切号（disabled）→ 登录 |

| # | 文件 | 操作 | 说明 |
|---|------|------|------|
| 1 | `src/MaaCore/Task/Interface/StartUpTask.cpp:30-81` | 改回原序 | `start_game → account_switch → start_up`；restart 循环也改回原序 |
| 2 | `resource/tasks/tasks.json:789-803` | 修改 | `SwitchAccount@StartToWakeUp.next` 追加 `SwitchAccount@StartToWakeUpOCR` 兜底；新增 `SwitchAccount@StartToWakeUpOCR` OCR 任务 |
| 3 | `AGENTS.md §6` | 修改 | `fix/expedite-threshold` 角色描述更新 |
| 4 | `LOG.md` | 修改 | 本节 |

**风险评估**：
- 编译风险：0
- 行为回归：原序 = 与 upstream 架构一致；单账号 / 多账号 / 已在主界面场景均已覆盖
- OCR 误识别风险：`SwitchAccount@StartToWakeUpOCR.fullMatch: true` 限定词表「开始唤醒/登录/登/录」；`next` 顺序先模板后 OCR，模板匹配时直接走 AccountManager，不会触发 OCR
- ROI 微调：当前 ROI `[373, 145, 535, 430]` 与 `StartUpThemes@StartToWakeUpOCR` 一致；如鹰角弹窗「登录」按钮位置超出 ROI，再单独调整

**预期效果**：
- 多账号切号 + 鹰角登录弹窗：30-40s（含 20s 等待）→ **20-30s**（消除 20s 等待）
- 单账号 / 已在主界面场景：与原序等价

**推 upstream**：仅本 fork 修复，不推。

### fix/expedite-threshold StartUp::run 重排：先登录再切号

多账号切号场景实测发现 20+ 秒等待：游戏启动后停在鹰角登录弹窗（HyperGryph server auth popup，显示 `192****6952` + 「登录」按钮），MAA 的 `AccountSwitchTask::navigate_to_start_page` 立即尝试切号，但 `SwitchAccount@StartUpBegin` 链（`tasks.json:729-737`）只识别**游戏内账号管理界面**（`AccountManagerOfficial` / `AccountManagerBili` / `Txwy`），不识别鹰角登录弹窗；30 retry 全部失败后进入 `Login failed, entering game-restart loop` 5x restart_game 死循环。

实测日志证据（`install/debug/asst.log` 13:47:40-13:47:54）：

```
13:47:45  GameStartCheckResourceOCR 命中 (5s postDelay)
13:47:51  GameStart.png 命中 → click (628, 685)
13:47:54  last matched task: SwitchAccount@GameStart
13:47:54  WRN "Account switch failed after restart, retrying game restart"
```

**根因（架构问题，非性能问题）**：`StartUpTask::run` 顺序为 `start_game → account_switch → start_up`，**假设游戏已登录到主界面**。当游戏在鹰角登录弹窗时：
- `m_account_switch_task_ptr->run()` 立即失败（找不到 AccountManager 页面）
- 整个 StartUp 任务进入 5x restart_game 循环，每次 restart 同样的事情再发生

**修复**：重排为 `start_game → start_up → account_switch → start_up`，先确保游戏登录到主界面，再切号，再登录。restart_game 循环内也相应先 `m_start_up_task_ptr->run()` 后 `m_account_switch_task_ptr->run()`。

`cherry-pick 784d9005f6`（`AccountManagerOfficial` 残缺 OCR 补全）解决的是「切号场景 30 retry 找不到 AccountManager」的不同子问题，**没有改变架构顺序**；本 commit 才是**架构层面的修复**。

| # | 文件 | 操作 | 说明 |
|---|------|------|------|
| 1 | `src/MaaCore/Task/Interface/StartUpTask.cpp:30-81` | 修改 | `StartUpTask::run` 重排：`start_game → start_up → account_switch → start_up`；5x restart_game 循环内同样改为先登录再切号 |
| 2 | `AGENTS.md §6` | 修改 | `fix/expedite-threshold` 角色描述追加「;StartUp::run 重排（修切号前未登录的 20s 等待）」 |
| 3 | `LOG.md` | 修改 | 本节 |

**4 类场景行为矩阵**：

| 场景 | 改前 | 改后 |
|------|------|------|
| 多账号切号 + 鹰角登录弹窗 | 切号失败 → 5x restart → 失败（20s 等待） | 自动登录 → 切号 → 再次登录 → 成功 |
| 多账号切号 + 已在主界面 | 切号 → 登录 → 成功 | fast-path 登录 → 切号 → 登录 → 成功（多 0.5s fast-path 开销） |
| 单账号 + 鹰角登录弹窗 | 切号（disabled）→ 登录 → 成功 | 登录 → 切号（disabled）→ fast-path 登录 → 成功（多 0.5s） |
| 单账号 + 已在主界面 | 切号（disabled）→ 登录 → 成功 | 登录（fast-path 跳过）→ 切号（disabled）→ 登录（fast-path）→ 成功（多 0.5s） |

**风险评估**：
- 编译风险：0（仅调整控制流）
- 单账号流程：多 0.5s fast-path 开销（StartAtHome 模板命中即跳过）
- 切号流程：fast-path 命中 → 跳过 `start_up` 任务链 → 直接走切号 → 0 影响
- 与 upstream 偏离：upstream 仍 `切号优先于登录` 架构；本 fork 因多账号场景修正
- 推 upstream：仅本 fork 修复，不推

**预期效果**：
- 多账号切号场景：30-40s → **20-30s**（消除 20s 等待）
- 单账号场景：5-20s → 3-15s（StartAtHome fast-path 多走一次，可忽略）

**手动验收**：
1. 多账号切号 + 鹰角登录弹窗 → 启动 MAA → 观察自动点「登录」→ 进主界面 → 切号 → 再次登录
2. 多账号切号 + 已在主界面 → 启动 MAA → 直接切号
3. 单账号 → 启动 MAA → 正常登录

### fix/expedite-threshold StartUp 双重缓冲清理

调研 `upstream/dev-v2` 与本仓库对比后确认：`src/MaaCore/Task/Interface/StartUpTask.cpp:24` 的 `.set_task_delay(Config.get_options().task_delay * 2)` 双重缓冲在默认 `task_delay=0`（`GeneralConfig.h:34`）下无任何效果（`0*2=0`），且与 upstream `dev-v2` 一致（无 PR 推动调整）。本次清理纯粹是「删无意义代码」，不修改任何 `postDelay` / `preDelay` / `retry_times` / ROI / OCR 算法，遵循「稳定优先」原则，follow upstream 基线。

| # | 文件 | 操作 | 说明 |
|---|------|------|------|
| 1 | `src/MaaCore/Task/Interface/StartUpTask.cpp:24` | 修改 | `.set_task_delay(Config.get_options().task_delay * 2)` → `.set_task_delay(Config.get_options().task_delay)`，删 `* 2` |
| 2 | `AGENTS.md §6` | 修改 | `fix/expedite-threshold` 角色描述追加 `;StartUp 双重缓冲清理` |
| 3 | `LOG.md` | 修改 | 本节 |

**行为变化**：默认 `task_delay=0` 时新旧完全等价（`0*2==0`）；仅当用户在 WPF 把 `task_delay` 调到 >0 时，StartUp 阶段不再比日常任务多等一倍，更符合直觉。

**预期效果**：无任何可观测的运行时差异；仅清理一行无意义代码 + 文档同步。

**风险评估**：
- 编译风险：0（删 4 字符）
- 运行时回归：0（默认 task_delay=0 等价）
- 已有用户配置：仅影响手动调高 task_delay 的用户，行为更直观
- 回退成本：`git revert` 单 commit 即还原

**未做项**（明确排除，遵循 upstream 基线）：
- `GameStartCheckResourceOCR.postDelay: 5000` / `GameStartUpdateOCR.postDelay: 5000` / `LoginOther.preDelay: 3000` 等 tasks.json 延迟
- `set_retry_times(50)` / `set_retry_times(30)` 切号重试上限
- `MaxRestartAttempts=5` 重启循环
- ROI 缩窄 / OCR 算法调整

### fix/account-official-recognize cherry-pick 同步到 fix/expedite-threshold

`fix/expedite-threshold`（HEAD `301f90897a`，branch point = `9d8d021610`）branch point 早于 `branch` 上今天 12:46 的官方服账号切换识别补全 `784d9005f6`，导致该分支部署的 `install/MaaCore.dll` 仍带 `AccountManagerOfficial` 残缺定义 bug —— 官服 + 账号轮换场景下 `ProcessTask` 30 次 retry 全失败，进 `Login failed, entering game-restart loop` 卡在登录页。实测环境（MAA 主界面日志 12:19: `StartToWakeUp.png` 命中、登录页 OCR 不识别）确认复现。

upstream `MaaAssistantArknights/MaaAssistantArknights` `dev-v2` 仍带同款 bug，无对应 PR；本 fork `branch` 领先 upstream。

| # | 文件 | 操作 | 说明 |
|---|------|------|------|
| 1 | `resource/tasks/tasks.json:805-810` | cherry-pick from `784d9005f6` | `AccountManagerOfficial` 由 `{"roi":[570,165,140,80]}` 补全为 `{"Doc":"...","algorithm":"OcrDetect","text":["登录记录"],"roi":[237,50,771,242]}`（与 `AccountManagerBili` 对齐） |
| 2 | `src/MaaCore/Task/Miscellaneous/AccountSwitchTask.cpp:68-83` | cherry-pick from `784d9005f6` | `navigate_to_start_page()` 加 `Log.info(... last matched task ...)` 诊断日志；4 个 `else if` 合并为单 `if (... \|\| ... \|\| ... \|\| ...)` |
| 3 | `LOG.md` | cherry-pick from `784d9005f6` | 同步 `### fix/account-official-recognize 启动` 与 `### fix/account-official-recognize 实施完成` 两节；冲突解决：保留本分支 `fix/expedite-threshold` 两节，追加新两节（无内容丢失） |
| 4 | `AGENTS.md §7.5` | cherry-pick from `da157d163d` | 新增 `fix/account-official-recognize` 生命周期记录小节 |
| 5 | `AGENTS.md §6` | 不变 | `fix/expedite-threshold` 仍为进行中分支（未合入 branch），待本分支合入 `branch` 时再清除 |
| 6 | `LOG.md` | 修改 | 本节（cherry-pick 同步事件说明） |

**Commit 链**：

| SHA | 来源 | 标题 |
|-----|------|------|
| `6011051af2` | cherry-pick from `784d9005f6` | `fix(startup): 官方服账号切换界面识别补全 + 切号诊断日志` |
| `f241b2160b` | cherry-pick from `da157d163d` | `docs: 登记 fix/account-official-recognize 分支生命周期` |

**作用域声明**：
- 与 `§7.5 fix/account-official-recognize` 同名不同分支生命周期——本节记录的是「`branch` 上游修复 cherry-pick 到 `fix/expedite-threshold`」的同步事件，不重复登记上游生命周期。
- §6 `fix/expedite-threshold` 仍标进行中；待用户后续决定 FF / `--no-ff` 合入 `branch` 时一并清除。

## 2026-07-15

### 分支工作文档约束调整

LOG.md / AGENTS.md 从 `.gitignore` 移除，改为所有分支跟踪；feat*.md / fix*.md 保留 gitignore，永不提交。

| # | 文件 | 操作 | 说明 |
|---|------|------|------|
| 1 | `.gitignore:515-516` | 删除 | 移除 `LOG.md`、`AGENTS.md` 忽略规则 |
| 2 | `.gitignore:514` | 修改 | 注释改为 `# Feature/fix working documents (local only, never committed)` |
| 3 | `LOG.md` | 修改 | 本节 |

### feat/expedite-threshold 启动

公招加急(`expedite`)原本只有"开/关"两种状态，与星级判断完全脱钩。新增**加急门槛**机制:可仅在本次确认的招募组合最低星级 ≥ 4/5/6 时才使用加急许可，3★ 不浪费加急许可。

| # | 文件/对象 | 操作 | 说明 |
|---|----------|------|------|
| 1 | `feat/expedite-threshold` | 新建分支 | 从 `branch` 拉出 |
| 2 | `feat_expedite-threshold.md` | 新建 | 工作文档(已 .gitignore) |

### feat/expedite-threshold 实施完成

| # | 文件 | 操作 | 说明 |
|---|------|------|------|
| 1 | `src/MaaCore/Task/Miscellaneous/AutoRecruitTask.h:28, 173, 186` | 修改 | +1 setter 声明 `set_expedite_min_level`;+2 成员 `m_expedite_min_level`(默认 0)、`m_last_confirmed_min_level`(默认 0) |
| 2 | `src/MaaCore/Task/Miscellaneous/AutoRecruitTask.cpp:147-152` | 修改 | +1 setter 实现 `set_expedite_min_level` |
| 3 | `src/MaaCore/Task/Miscellaneous/AutoRecruitTask.cpp:228-303` | 修改 | `_run` 主循环移除 `try_use_expedited` 局部变量,加急判定改为**每次进入前重新求值** `m_use_expedited && m_last_confirmed_min_level >= m_expedite_min_level`;加急成功后立即重置 `m_last_confirmed_min_level = 0` 防止陈旧状态被复用;加急失败时显式退出以避免阈值=0 时死循环 |
| 4 | `src/MaaCore/Task/Miscellaneous/AutoRecruitTask.cpp:340-347` | 修改 | `recruit_one` 开头重置 `m_last_confirmed_min_level = 0`,仅当 `recruit_calc_task` 走到 success / nothing_to_select 路径时才会被重新赋值 |
| 5 | `src/MaaCore/Task/Miscellaneous/AutoRecruitTask.cpp:740-743, 770-772` | 修改 | `recruit_calc_task` 在 `nothing_to_select` 与 `success` 两条返回路径前赋值 `m_last_confirmed_min_level = final_combination.min_level` |
| 6 | `src/MaaCore/Task/Interface/RecruitTask.cpp:54, 87` | 修改 | +1 参数解析 `expedite_min_level`(默认 0);链式调用透传给 AutoRecruitTask |
| 7 | `src/MaaWpfGui/Configuration/Single/MaaTask/RecruitTask.cs:38-42` | 修改 | +1 字段 `ExpediteMinLevel`(默认 0) |
| 8 | `src/MaaWpfGui/Models/AsstTasks/AsstRecruitTask.cs:69-72, 161` | 修改 | +1 字段 `ExpediteMinLevel`;`Serialize` 始终写入 `expedite_min_level` 到 params |
| 9 | `src/MaaWpfGui/ViewModels/UserControl/TaskQueue/RecruitSettingsUserControlModel.cs:142-174, 329` | 修改 | +3 VM 成员:`ExpediteMinLevelEnabled`(布尔,setter 控制 0/4 切换)、`ExpediteMinLevel`(int,setter 白名单 0/4/5/6)、`ExpediteMinLevelOptions`(4/5/6 三档 ComboBox 选项);Serialize 阶段写入 `ExpediteMinLevel` |
| 10 | `src/MaaWpfGui/Views/UserControl/TaskQueue/RecruitSettingsUserControl.xaml:162-182` | 修改 | 高级设置区末尾追加 CheckBox + ComboBox;整行 Visibility 绑定到 `UseExpeditedWithNull == true` |
| 11 | `src/MaaWpfGui/Res/Localizations/zh-cn.xaml:1474-1479` | 修改 | +5 string key:`ExpediteMinLevelLabel` / `ExpediteMinLevelTip` / `ExpediteMinLevel_4Plus` / `ExpediteMinLevel_5Plus` / `ExpediteMinLevel_6Plus` |
| 12 | `docs/zh-cn/protocol/integration.md:263-276` | 修改 | +1 字段说明 `expedite_min_level`,含 0/4/5/6 语义 |
| 13 | `feat_expedite-threshold.md` | 修改 | 追加章节 三/四/五,记录实施结果与踩坑 |
| 14 | `LOG.md` | 修改 | 本节 |

**编译/部署结果**: (待补充)

**兼容性核查**:
- 旧 API 用户不传 `expedite_min_level` → C++ 端默认 0 = 不限 → 全加急,行为不变
- 旧 GUI 用户配置文件中无该字段 → JSON 反序列化默认 0 + CheckBox 未勾选 → 全加急,行为不变
- 新用户首次启动 → CheckBox 未勾选(默认 0) → 全加急,等同旧行为

**待手动验证 (需模拟器环境)**:
1. 准备 4★ 组合 → 选 Tag → 确认 → 立即完成 → 循环到下一栏
2. 准备 5★ 组合 → 选 Tag → 确认 → 立即完成 → 循环到下一栏
3. 准备 3★ 组合 → 选 Tag → 确认 → 等待 9 小时(不加急)
4. 关掉"自动加急" → 所有栏位走自然倒计时
5. 下拉框改 "5★+" → 4★ 栏位走自然倒计时,5★+ 立即完成
6. 下拉框改 "6★+" → 4★/5★ 都走自然倒计时,6★ 立即完成

## 2026-07-15

### feat/defer-rogue 启动

启用账号轮换时把肉鸽 (Roguelike) 与生息演算 (Reclamation) 延后到所有账号基础任务完成后执行。执行顺序: A-1 → B-1 → A-2 → B-2 (跨账号轮转 Phase)。默认关闭以保持向后兼容。

| # | 文件/对象 | 操作 | 说明 |
|---|----------|------|------|
| 1 | `feat/defer-rogue` | 新建分支 | 从 `branch` 拉出,本地工作分支 |
| 2 | `feat_defer-rogue.md` | 新建 | 工作文档(已 .gitignore) |

### feat/defer-rogue 实施完成

| # | 文件 | 操作 | 说明 |
|---|------|------|------|
| 1 | `src/MaaWpfGui/Configuration/Single/MaaTask/StartUpTask.cs:36-44` | 修改 | 新增 `LateStageRogueAndReclamation : bool = false`,默认关闭以保持向后兼容 |
| 2 | `src/MaaWpfGui/Models/AccountCycleStep.cs` | 新建 | `record AccountCycleStep(string AccountName, int Phase)`,步骤扁平列表的载体 |
| 3 | `src/MaaWpfGui/ViewModels/UserControl/TaskQueue/StartUpSettingsUserControlModel.cs:95-176` | 修改 | (a) `LateStageRogueAndReclamation` VM 属性(照搬 `AccountSwitchEnabled`);(b) 新增 `#region Late Stage` 含 `_cycleSteps` / `_currentStepIndex` / `RebuildCycleSteps` / `AdvanceStepIndex` / `CurrentStep` / `GetPreviousStep` / `CurrentPhase`;(c) `ResetCycle` 同步清空步骤列表 |
| 4 | `src/MaaWpfGui/ViewModels/UI/TaskQueueViewModel.cs:1813-1851` | 修改 | `LinkStart` 改为 `RebuildCycleSteps` + 取 `CurrentStep` 决定首个账号 |
| 5 | `src/MaaWpfGui/ViewModels/UI/TaskQueueViewModel.cs:1939-1982` | 修改 | `LinkStartWithTasks` foreach 新增 Phase 过滤(`IsInCurrentPhase` 由 `lateStageOn` 闸门,LateStage 关闭时 no-op) |
| 6 | `src/MaaWpfGui/ViewModels/UI/TaskQueueViewModel.cs:2172-2355` | 修改 | `AdvanceAccountCycle` 全量重写:扁平步骤推进 + `needStartupSwitch` 显式切号 + 空步骤递归跳过 + `MarkAccountCompleted` 按 LateStage 状态差异化触发 |
| 7 | `src/MaaWpfGui/ViewModels/UI/TaskQueueViewModel.cs:2359-2367` | 修改 | 新增静态助手 `IsInCurrentPhase(TaskType, int phase)` |
| 8 | `src/MaaWpfGui/Views/UserControl/TaskQueue/StartUpTaskUserControl.xaml:134-149` | 修改 | AccountCycle 子面板末尾新增 CheckBox + TooltipBlock(长 Wrap + MaxWidth CalcBinding 防挤压) |
| 9 | `src/MaaWpfGui/Res/Localizations/zh-cn.xaml:696-697` | 修改 | +2 string key:`LateStageRogueAndReclamation` / `LateStageRogueAndReclamationTip` |
| 10 | `src/MaaWpfGui/Res/Localizations/zh-tw.xaml:696-697` | 修改 | 同上(繁体) |
| 11 | `src/MaaWpfGui/Res/Localizations/en-us.xaml:696-697` | 修改 | 同上(英文) |
| 12 | `src/MaaWpfGui/Res/Localizations/ja-jp.xaml:696-697` | 修改 | 同上(日文) |
| 13 | `src/MaaWpfGui/Res/Localizations/ko-kr.xaml:696-697` | 修改 | 同上(韩文) |
| 14 | `install/MAA.dll` | 更新 | `dotnet publish` 部署时间 2026-07-15 11:48 |
| 15 | `install/MaaCore.dll`, `install/MaaUtils.dll`, `install/MAA.Updater.exe` | 更新 | `cmake --install` 部署 |
| 16 | `feat_defer-rogue.md` | 修改 | 追加章节 三/四/五,记录实施结果与踩坑 |
| 17 | `LOG.md` | 修改 | 本节 |

**编译/部署结果**: `dotnet build -c Release` 0 error, 3 warning (SA1503, 与原版同款写法);`cmake --build` + `cmake --install` + `dotnet publish` 全部通过;最终 `install/MAA.dll` 时间戳为今日。

**踩坑**:
- cmake 触发 WPF MSBuild 评估时撞到 LOG.md 记录的 VS2026 SDK 路径 bug,绕用 standalone `dotnet publish`
- Phase 2 不包含 StartUp 任务,跨账号切号需在 AdvanceAccountCycle 顶部显式补 `StartGame=false StartUp`,避免 Phase 2 步骤卡死在错误账号

**待手动验证 (需模拟器环境)**:
1. 2 账号 + 开关 ON + 全勾 → 日志应见 `[Cycle] Account=A, Phase=1` 后切号 → Account=B, Phase=1 → 切号 → A, Phase=2 → B, Phase=2 → 全部完成
2. 2 账号 + 开关 ON + 不勾肉鸽生息 → Phase 2 自动跳过
3. 2 账号 + 开关 OFF → 旧版行为
4. 1 账号 + 开关 ON → A-1 → A-2 (同账号不切号)
5. 1 账号 + 开关 OFF → 单账号旧行为

## 2026-07-14

### fix/account_rotation/3 — 版本不一致误报修复

| # | 文件/操作 | 说明 |
|---|----------|------|
| 1 | cmake 缓存重置 | `cmake -DMAA_HASH_VERSION=DEBUG_VERSION`，清除上次 release 脚本遗留的 `v6.14.0-fork.20260714` |
| 2 | `src/MaaWpfGui/ViewModels/UI/RootViewModel.cs:117` | `uiVersion` 追加 `.TrimStart('v', 'V')` |
| 3 | `src/MaaWpfGui/ViewModels/UI/TaskQueueViewModel.cs:1882` | 同上 |
| 4 | `feat/account_rotation` | FF 合并 `fix/account_rotation/3` |
| 5 | `branch` | 合并 `feat/account_rotation`，同步修复 |
| 6 | `Github/branch`, `Github/feat/account_rotation` | 推送至远程 |

### 移除发布打包文件 + 清理 install 目录

从仓库和 `install/` 中移除不再需要的发布打包相关文件。后续本地运行直接从 `build/bin/RelWithDebInfo/` 启动。

| # | 文件 | 操作 | 说明 |
|---|------|------|------|
| 1 | `VERSION` | git rm | 仅 release 脚本读取，不再需要 |
| 2 | `.github/workflows/release-fork.yml` | git rm | fork 的 GitHub CI，本地运行不需要 |
| 3 | `tools/release-zip.ps1` | git rm | 发布打包脚本，不再需要 |
| 4 | `tools/release-zip.bat` | git rm | 发布打包脚本，不再需要 |
| 5 | `tools/DependencySetup_依赖库安装.bat` | git rm | 终端用户依赖安装脚本，不再需要 |
| 6 | `install/DependencySetup_依赖库安装.bat` | 删除 | install 目录副本同步清理 |
| 7 | `install/filelist.txt` | 删除 | 打包校验清单，运行时无用 |
| 8 | `install/Python/` | 删除 | Python 绑定，WPF GUI 不需要 |
| 9 | `install/.gitignore` | 删除 | 产物目录中不应有 git 忽略规则 |
| 10 | `src/MaaWpfGui/ViewModels/UI/RootViewModel.cs` | 修改 | 版本比较时 `uiVersion` 也 `TrimStart('v', 'V')`，修复 UI 和 Core 版本号一致仍弹警告的 bug |
| 11 | `src/MaaWpfGui/ViewModels/UI/TaskQueueViewModel.cs` | 修改 | 同一处版本比较，补上 `uiVersion.TrimStart` |

### 实际跑通 release-zip + 4 个 bug 修复

按上一节方案第一次运行 `tools\release-zip.bat` 跑通，产出 `installer/MAA-v6.14.0-fork.20260714-win-x64.zip`（250.5 MB，9297 entries）。过程中踩到 4 个本机环境问题，均已修复并入脚本或 .gitignore。

| # | 问题 | 修复 | 影响 |
|---|------|------|------|
| 1 | `cmake --build --preset windows-publish-x64` 触发 cmake MSBuild 评估 MaaWpfGui.csproj 时报 `Microsoft.NET.Sdk` 找不到：VS 2026 装在 `E:\visual stduio community 2026\VIUAL\`（拼写错），其 `MSBuild\Sdks\Microsoft.NET.Sdk\Sdk` 目录缺失。 | 改用 `cmake --build build --target MaaCore` 单目标 C++ 构建；WPF 改走 standalone `dotnet publish`（用 `C:\Program Files\dotnet` 的 SDK，与 cmake 带的 MSBuild 解耦）。脚本改一处（2 行 cmake 命令）。 | 跳过 cmake 的 WPF 评估，走和 `local-install.bat` 一致的"先 cmake 装 C++，再 dotnet publish WPF"双轨流程。 |
| 2 | 临时 `global.json` 强制 `10.0.203` + `rollForward:disable`，但本机 `dotnet --list-sdks` 只装 `10.0.300`，SDK 解析失败。 | 改为 `10.0.100` + `rollForward:latestFeature`：要求 .NET 10 特性带内（10.0.x.x），但不锁小版本。 | 任何装 10.0.x SDK 的机器都能跑；本机 10.0.300 自动启用。 |
| 3 | `install\.git\` 是个真实 git 子目录（含 hooks/info/objects/refs），原 staging 漏排，导致它被打进 zip（约 50 个无关文件）。 | robocopy `/XD` 列表追加 `.git`。 | zip 不再携带意外目录。 |
| 4 | `.gitignore` 第 510 行 `DependencySetup_依赖库安装.bat` 无 `install/` 前缀，误伤 `tools/DependencySetup_依赖库安装.bat`，导致源文件长期无法入仓。 | 规则改为 `install/DependencySetup_*.bat`，只屏蔽 install/ 副本。 | tools/ 源可入仓，build 可复现。 |

附加修复：发现 `tools\DependencySetup_依赖库安装.bat` 在 git 跟踪中实际已不存在（仅 `install/` 里有副本），从 `install/` 还原回 `tools/`，脚本的 `Copy-Item` 才有源。

最终 8 步全过日志：`<install>/installer-build.log`（9362 行，2.6 MB）。

**zip 结构 vs `D:\MAA\MAA-v5.21.2-win-x64` 对照**：

- 共同：16 个顶层 DLL/EXE 文件（`MAA.exe` / `MAA.dll` / `MaaCore.dll` / `MaaUtils.dll` / 4 个 ControlUnit / DirectML / fastdeploy_ppocr_maa / onnxruntime_maa / opencv_world4_maa / `MAA.Updater.exe` 等）+ 4 个顶层目录（`externals/`、`Python/`、`Res/`、`resource/`）+ `DependencySetup_依赖库安装.bat`
- 差异（正向）：我们额外有 `libloader.dll`（libloader.dll 启动钩子）、`res_updater.exe`（资源自更新器）— 本仓库历史加入
- 差异（负向）：官方有 `hostfxr.dll` / `hostpolicy.dll`（dotnet 启动器）、`filelist.txt`（CI 产物清单）— 本机用 SelfContained 发布模式不需要，`filelist.txt` 在 .gitignore 已排
- 用户数据目录（`cache/` / `config/` / `data/` / `debug/`）：我们**正确排除** ✓（官方不知为何保留在 release 解压后的目录里，可能是发布前被预热过）

| # | 文件 | 操作 | 说明 |
|---|------|------|------|
| 1 | `tools/release-zip.ps1` | 修改 | 步骤 2 改单目标 cmake build；步骤 6 改 global.json 为 `10.0.100 + latestFeature`；步骤 8 staging `/XD` 加 `.git`；脚本注释说明改动原因 |
| 2 | `tools/DependencySetup_依赖库安装.bat` | 还原 | 从 `install/` 副本拷回 `tools/`，恢复 `Copy-Item` 源；并入 git |
| 3 | `.gitignore` | 修改 | (a) 追加 `/installer-build.log`（2.6 MB 临时日志不入仓）；(b) 修正第 510 行规则 `DependencySetup_依赖库安装.bat` 误伤 `tools/`，改为 `install/DependencySetup_*.bat`（只屏蔽 install/ 副本） |
| 4 | `LOG.md` | 修改 | 本节记录首次跑通与 4 个 bug 修复 |



### 一键发布包脚本（installer/）

新增 `tools/release-zip.{bat,ps1}`，对照上游 `.github/workflows/ci.yml` 的 `Build for Windows` 作业实现本地等价流程：从 `VERSION` 读版本号 → `cmake --preset windows-publish-x64 -DMAA_HASH_VERSION=<v>` → 构建 `MAA.Updater` → `cmake --install` → 同步 `resource/` → 临时改 csproj 4 个 Version 字段后 `dotnet publish` → 剥 `*.pdb` `*.h` `*.bak` `msvc-debug/` `MAAComponent-DebugSymbol-*.zip` → 拷 `DependencySetup_依赖库安装.bat` → 用 `System.IO.Compression.ZipFile` 压缩到 `installer/MAA-<v>-win-x64.zip`，staging 目录排除 `cache/` `config/` `data/` `debug/`（用户数据不入包）。

| # | 文件 | 操作 | 说明 |
|---|------|------|------|
| 1 | `VERSION` | 新建 | 内容 `v6.14.0-fork.20260714`，作为 `MAA_HASH_VERSION` 和 zip 文件名单一来源 |
| 2 | `tools/release-zip.bat` | 新建 | bat 外壳，调 ps1 后 `pause`；失败时 `errorlevel` 透传 |
| 3 | `tools/release-zip.ps1` | 新建 | 核心 PowerShell 脚本（~180 行），`-Version` / `-SkipBuild` / `-KeepInstallerDir` 三个开关 |
| 4 | `.gitignore` | 修改 | 末尾追加 `/installer/`（产物不入仓），与 `install/` 一致不污染 git |
| 5 | `LOG.md` | 修改 | 本节 |

**关键设计**：

- **try/finally 保护仓库状态**：csproj 备份到 `.bak` 后改 4 个 Version 字段，无论 dotnet publish 成功或失败都还原；`global.json` 同样处理（按 AGENTS.md 约定写 10.0.203）→ 仓库**永远不变脏**
- **不联网拉 MaaFramework**：复用 install/ 现有的 `MaaAdbControlUnit.dll` / `MaaWin32ControlUnit.dll`，缺则报错（用户在 install/ 里已有这两个文件）
- **staging 目录临时构造**：避免把 `cache/config/data/debug` 这 4 个用户数据目录打进 zip；用 `robocopy /MIR /XD` 实现
- **`-SkipBuild` 开关**：只想重新打 zip 不重编译时可用
- **不裁剪 `externals/`**：与上游 CI 行为一致，零风险
- **不生成 DebugSymbol 副包**：用户确认不需要
- **脚本命名 `release-zip.*` 而非 `build-release-zip.*`**：根 `.gitignore` 第 6 行有 `build-*` 规则（CMake 构建产物），原命名会被吞掉；改名前 git check-ignore 验证确认

**版本号格式**：`vX.Y.Z-fork.YYYYMMDD`（如 `v6.14.0-fork.20260714`）。SemVer 解析为 prerelease，SemVer 严格支持；上游新版本（如 `v6.14.1`）按 SemVer 排序能正确触发更新提示。

**使用方式**：
```bat
:: 标准用法（双击或命令行）
tools\release-zip.bat

:: 跳过编译、只重新打包（install/ 已是最新时）
powershell -File tools\release-zip.ps1 -SkipBuild

:: 指定版本（跳过 VERSION 文件）
powershell -File tools\release-zip.ps1 -Version v6.14.0-fork.20260715
```

**与传统 local-install.bat 的区别**：

| 项 | `local-install.bat` | `release-zip.{bat,ps1}` |
|----|---------------------|-------------------------|
| 用途 | 本地开发自用 | **打包给其他人** |
| 产物 | `install/` 直接可跑 | `installer/MAA-vX.X.X-fork.YYYYMMDD-win-x64.zip` |
| `csproj` 改 | 不改 | 临时改 4 个 Version 字段再还原 |
| `*.pdb` `*.h` | 保留 | 剥 |
| NetBeauty | 启用 | 启用 |
| 资源 | 同步到 `install/` | 同步到 `install/` 然后排除用户数据后打 zip |

### 工作区清理 + 工具脚本归档 + 子模块初始化

| # | 文件 | 操作 | 说明 |
|---|------|------|------|
| 1 | `true` | 删除 | 0 字节空文件，误产生，清理 |
| 2 | `add_maa_to_nahimic_whitelist.ps1` | 移动 | 从根目录移入 `tools/`，脚本用途：将 MAA.exe 添加到 Nahimic DLL 注入白名单，绕过 Nahimic 拦截 |
| 3 | `disable_nahimic.ps1` | 移动 | 从根目录移入 `tools/`，脚本用途：停止并禁用 NahimicService 开机自启，彻底阻止 DLL 注入 |
| 4 | `src/MaaUtils` | 子模块初始化 | 引用上游 `MaaXYZ/MaaUtils`（HEAD `0c2556cfc`），提交至 feat/fix 索引 |
| 5 | `3rdparty/EmulatorExtras` | 子模块初始化 | 引用上游 `MaaXYZ/EmulatorExtras`（HEAD `54d3a3ad4`），提交至 feat/fix 索引 |

### feat/account_rotation 分支收尾

将 `fix/account_rotation/1` + `fix/account_rotation/2` 合并至 `feat/account_rotation`，形成单一主分支。

## 2026-07-13

### 账号轮换：停止卡死 + 切换报错后无法恢复

| # | 文件 | 操作 | 说明 |
|---|------|------|------|
| 1 | `src/MaaWpfGui/ViewModels/UI/TaskQueueViewModel.cs` | 修改 | `SetStopped` 中 `IsCycling` 短路分支增加"是否被强制停止"判断：`runStopScript && _runningState.GetStopping()` 时落空 `IsCycling` 走完整重置流程,清 `Stopping` 标志;正常轮换推进路径保持不变(直接 return)。修复点停止按钮后 UI 永远卡在"正在停止"且按钮不可用的问题。 |
| 2 | `src/MaaWpfGui/ViewModels/UI/TaskQueueViewModel.cs` | 修改 | `AdvanceAccountCycle` 两个失败分支(`count == 0` 无任务被附加、`AsstStart()` 失败)改为调用 `SetStopped(runStopScript: false)`,统一重置 `Stopping/Idle/IsCycling`。修复"切换第二个账号任务出错"后状态卡住、按钮变灰、标题不恢复的问题。 |

## 2026-07-11

### 漏洞修复与配置调整

| # | 文件 | 操作 | 说明 |
|---|------|------|------|
| 1 | `src/MaaWpfGui/ViewModels/UI/TaskQueueViewModel.cs` | 修改 | `LinkStart` 补上 `AccountSwitchEnabled = true`；`TryStartNextCycleAccount` 处理 `cfg` 为 null 的边界情况；包裹 try-catch 防止 `async void` 静默吞异常；通过 `Execute.OnUIThreadAsync` 确保 UI 线程执行 |
| 2 | `src/MaaWpfGui/ViewModels/UserControl/TaskQueue/StartUpSettingsUserControlModel.cs` | 修改 | `GetCurrentCycleAccount` 简化：去掉 `_currentCycleIndex` 状态跟踪，改为直接取第一个符合条件的账号；去掉 `ResetCycleIndex` 方法 |
| 3 | `src/MaaWpfGui/Views/UserControl/TaskQueue/StartUpTaskUserControl.xaml` | 修改 | 添加/删除按钮图标统一字号和居中 |
| 4 | `.gitignore` | 修改 | 追加运行时缓存忽略规则；追加 `.crush/` / `.claude/` / `.cursor/` 规则；追加 `LOG.md` / `AGENTS.md` 忽略 |
| 5 | `src/MaaWpfGui/Main/AsstProxy.cs` | 修改 | `AllTasksCompleted` 回调中补上轮换推进逻辑：正常完成时调用 `MarkAccountCompleted` + `GetCurrentCycleAccount` + `LinkStart`，并 `break` 跳过标准完成日志，防止新一轮启动后仍打出"所有任务完成" |

### install 目录重构

| # | 文件 | 操作 | 说明 |
|---|------|------|------|
| 1 | `install/debug/oper` | 删除 | 空目录清理 |
| 2 | `install/debug/drops` | 删除 | 空目录清理 |
| 3 | `install/debug/other` | 删除 | 空目录清理 |
| 4 | `install/debug/infrast` | 删除 | 空目录清理 |
| 5 | `install/debug/interface` | 删除 | 空目录清理 |
| 6 | `install/cache/avatars` | 删除 | 空目录清理 |
| 7 | `install/filelist.txt` | 修改 | 重新生成，与实际文件同步 |
| 8 | `install/MAA.dll`, `install/MAA.pdb`, `install/MAA.exe`, `install/MAA.deps.json`, `install/MAA.runtimeconfig.json` | 更新 | 编译 MaaWpfGui 后部署；修复 `AsstProxy.cs` 中 `StartUpTask` 静态属性用实例访问的编译错误；修复 `TaskQueueViewModel.cs` 缩进和多余空行 |
| 9 | `install/MaaCore.dll`, `install/MaaUtils.dll` | 更新 | 完整 CMake 构建后部署 （RelWithDebInfo）|
| 10 | `src/MaaWpfGui/MaaWpfGui.csproj` | 修改 | `SelfContained` 改为 `false`，禁用 NetBeauty2 打包（不兼容 .NET 10.0.300） |
| 11 | `install/msvc-debug/` | 删除 | CMake 安装产生的 debug 符号目录，非必需 |
| 12 | `install/filelist.txt` | 更新 | 重新生成 |
| 13 | `src/MaaCore/Assistant.cpp` | 修改 | `AllTasksCompleted` 后立即设 `m_thread_idle=true`，修复第二轮 `AsstStart` 因竞态返回 false 的 bug |
| 14 | `src/MaaWpfGui/ViewModels/UI/TaskQueueViewModel.cs` | 修改 | 新增 `AdvanceAccountCycle()` 方法替代 `SetStopped` 做轮换推进；`SetStopped` 剥离轮换逻辑，只处理停止 |
| 15 | `src/MaaWpfGui/Main/AsstProxy.cs` | 修改 | `AllTasksCompleted` 回调调 `AdvanceAccountCycle` 替代 `SetStopped` |
| 16 | `src/MaaWpfGui/ViewModels/UserControl/TaskQueue/StartUpSettingsUserControlModel.cs` | 修改 | `SyncAccountNamesToItems` 保留已有项 `IsSelected` 状态，用户可自由勾选参与轮换的账号 |

### install 目录标准化

| # | 文件 | 操作 | 说明 |
|---|------|------|------|
| 1 | `install/MAA.runtimeconfig.json` | 修改 | 添加 `additionalProbingPaths: ["./externals"]` 和 `STARTUP_HOOKS: libloader` |
| 2 | `install/MAA.deps.json` | 替换 | 替换为参考版（NetBeauty2 正确修补的版本） |
| 3 | `install/externals/` | 重建 | 将 279 个托管 DLL 移入 `externals/`；区域卫星程序集移至 `externals/locales/{lang}/` |
| 4 | `install/` 根目录 | 清理 | 仅保留 11 个核心 DLL + 2 个 EXE + 配置文件 |
| 5 | `install/filelist.txt` | 新增 | 从参考版复制 |
| 6 | `install/MAA.dll` | 更新 | 多次重建部署 |
| 7 | 空目录清理 | 删除 | 删除 10 个空目录（子模块占位等） |
| 8 | 冗余 DLL 清理 | 删除 | 删除根目录 33 个 .NET runtime DLL + install 目录 245 个 |

### Skills 迁移

| # | 文件 | 操作 | 说明 |
|---|------|------|------|
| 1 | `.crush/skills/` | 新建 | 从 `.claude/skills/` 和 `.cursor/skills/` 迁入 5 个 skill |
| 2 | `.claude/` | 删除 | 空目录清理 |
| 3 | `.cursor/` | 删除 | 空目录清理 |

### branch 接受 feat/account_rotation 合并

将 `feat/account_rotation` 通过 Fast-forward 方式合入 `branch`，完成该功能的正式发布流程。

| # | 文件/对象 | 操作 | 说明 |
|---|-----------|------|------|
| 1 | `branch` | FF 合并 | 接收 `feat/account_rotation` 全部 7 个新提交，HEAD 由 `c8c8e75be5` → `23b1bf3167`。`merge-base` 等于 `branch` 旧 HEAD，无分叉、无合并提交、无冲突 |
| 2 | `Github/branch` | 推送 | 远程 `branch` 同步到 `23b1bf3167`（`git push Github branch`）|
| 3 | `Github/feat/account_rotation` | 删除 | feat 功能正式合入 `branch`，远程 feat 分支清理（`git push Github --delete feat/account_rotation`）。本地 `feat/account_rotation` 保留以备回溯与对照 |
| 4 | `feat_account_rotation.md` | 修改 | 追加 `## 十一、feat→branch 合并完成` 沉淀段落，更新 `## 九、待办` 完成项 |
| 5 | `LOG.md` | 修改 | 新增本节，记录合并事件 |

合并前后 `branch` 对比：

```
c8c8e75be5  Initial commit: MAA fork base with account rotation feature
   ↓ +7 commits
23b1bf3167  fix: 停止卡死 + 切换报错后无法恢复; chore: 归档 Nahimic 脚本到 tools/; build: 初始化 MaaUtils/EmulatorExtras 子模块
```

关键决策：

- **FF 而非 --no-ff**：`branch` 是 `feat/account_rotation` 的严格祖先，无任何分叉。`--no-ff` 会产生无信息增量的冗余 merge commit，与 AGENTS.md 推崇的"PR 简洁历史"相悖
- **本地保留 feat 分支**：feat 功能虽已合并到 `branch`，但保留本地 `feat/account_rotation` 指针便于回溯（如对比 feat 行为差异、cherry-pick 修复等）。`fix/account_rotation/1` 和 `fix/account_rotation/2` 保留同样理由
- **远程删除 feat**：远端 feat 分支已无存在价值（功能在 `branch` 中），清理可减少远程分支列表噪音

### 远程 feat/account_rotation 恢复（决策修正）

事后澄清：`fix/account_rotation/{1,2}` 与 `feat/account_rotation` 是从属关系，远程 fix/* 不应孤立存在。修正先前"删除远程 feat"的决策，将远程 feat 恢复。

| # | 文件/对象 | 操作 | 说明 |
|---|-----------|------|------|
| 1 | `Github/feat/account_rotation` | 恢复 | `git push Github feat/account_rotation`，在远端重建 `refs/heads/feat/account_rotation` → `23b1bf3167` |
| 2 | `feat/account_rotation` | 设置 upstream | `git branch --set-upstream-to=Github/feat/account_rotation feat/account_rotation`，后续 push/pull 无需指定远端 |
| 3 | `Github/fix/account_rotation/{1,2}` | 不动 | 按用户确认保留两个远程 fix 分支，不删除 |
| 4 | `feat_account_rotation.md` | 修改 | 追加 `## 十二、远程 feat 恢复记录`，记录决策修正与修复后分支层级 |

修复后远程分支结构：

```
branch                          (23b1bf3167, 生产就绪)
└─ feat/account_rotation        (23b1bf3167, 从属, 恢复)
   ├─ fix/account_rotation/1    (f3413f24f5, 从属, 落后 feat 1 commit)
   └─ fix/account_rotation/2    (23b1bf3167, 从属, 与 feat 同位)
```

### 工作文档命名规范化

按 AGENTS.md 分支工作流约定，feat/fix 工作文档名应对齐分支名。中文文件名在跨平台 / 终端场景下存在编码兼容问题，统一改为英文命名。

| # | 文件 | 操作 | 说明 |
|---|------|------|------|
| 1 | `feat_账号轮换.md` | 删除 | 中文文件名；内容已拆分：上半段（章节 一~十一 合并日志）已存在于 `feat_account_rotation.md`、无重复；末尾独立设计文档（标题"账号轮换功能设计与实现"，11 章 ~9000 字）被合入 `feat_account_rotation.md` 作为新章节十二 |
| 2 | `feat_account_rotation.md` | 修改 | 新增 `## 十二、账号轮换功能设计文档`：覆盖功能架构、数据模型、编辑模式、轮换生命周期、UI 布局与交互、本地化字符串、手动切换、C++ 后端流程、默认配置、文件清单、约束与注意事项。原 `## 十二、远程 feat 恢复记录` 重编号为 `## 十三、`（含 `### 12.x` → `### 13.x`） |
| 3 | `fix_账号轮换.md` | 重命名 | → `fix_account_rotation_1.md`，对齐分支名 `fix/account_rotation/1`，证明是 `feat/account_rotation` 分支的第一次修复 |

**文件整理前后**：

```
整理前                                    整理后
feat_account_rotation.md  (16050 B)       feat_account_rotation.md  (31223 B, 章节 11 → 13)
feat_账号轮换.md          (14834 B)  →  fix_account_rotation_1.md  (2154 B)
fix_账号轮换.md           ( 2154 B)
```

**为什么新增的是章节十二而非其他位置**：

- 设计文档是 feat 分支的**核心交付物**（UI 架构 / 数据流 / 约束），理应作为长期知识沉淀，位置应靠前
- 原章节十二（远程 feat 恢复记录）是**操作流水**，时效性强、长期参考价值低，重编号为十三不影响阅读
- 章节十一（feat→branch 合并完成）是分支生命周期记录，作为收官章节保持不动

**为什么 fix 文件用 `fix_account_rotation_1.md`（下划线）而非目录形式**：

- Windows 文件名不支持 `/`，无法使用 `fix_account_rotation/1.md` 路径分两段
- 下划线 `_` 与 AGENTS.md "feat_*.md / fix_*.md" 现有约定一致（如 `feat_账号轮换.md`）
- 阿拉伯数字后缀 `_1` 隐含序列语义，未来 `fix/account_rotation/2` 对应 `fix_account_rotation_2.md`，一一对应

## 2026-07-10

### 账号轮换漏洞修复

| # | 文件 | 操作 | 说明 |
|---|------|------|------|
| 1 | `src/MaaWpfGui/ViewModels/UI/TaskQueueViewModel.cs` | 修改 | `SetStopped` 新增轮换逻辑：完成任务后调用 `MarkAccountCompleted` 标记当前账号完成，若还有未完成账号则自动触发 `LinkStart` 继续下一账号 |

## 2026-07-10

### 账号轮换功能实现

实现了完整的账号轮换功能，允许用户配置多个账号，MAA 在每次完整任务队列执行完毕后自动切换到列表中的下一个未完成账号，并重新启动任务队列，直到所有账号全部完成一轮。

| # | 文件 | 操作 | 说明 |
|---|------|------|------|
| 1 | `src/MaaWpfGui/Configuration/Single/MaaTask/StartUpTask.cs` | 修改 | 添加 `AccountCycleEnabled` (bool, 默认 true) 和 `AccountNames` (List\<string\>, 默认 ["", ""]) |
| 2 | `src/MaaWpfGui/Models/AccountCycleItem.cs` | 新建 | 轮换账号数据模型（DisplayName / AccountName / IsSelected / IsCompleted / Index） |
| 3 | `src/MaaWpfGui/Res/Localizations/zh-cn.xaml` | 修改 | 添加 7 个 AccountCycle 本地化 key |
| 4 | `src/MaaWpfGui/Res/Localizations/en-us.xaml` | 修改 | 同上（英文） |
| 5 | `src/MaaWpfGui/Res/Localizations/ja-jp.xaml` | 修改 | 同上（日文） |
| 6 | `src/MaaWpfGui/Res/Localizations/ko-kr.xaml` | 修改 | 同上（韩文） |
| 7 | `src/MaaWpfGui/Res/Localizations/zh-tw.xaml` | 修改 | 同上（繁体中文） |
| 8 | `src/MaaWpfGui/ViewModels/UserControl/TaskQueue/StartUpSettingsUserControlModel.cs` | 修改 | 添加轮换 CRUD、GetCurrentCycleAccount、MarkAccountCompleted、SyncAccountNamesToItems 等方法 |
| 9 | `src/MaaWpfGui/ViewModels/UI/TaskQueueViewModel.cs` | 修改 | LinkStart 加入轮换判定，SetStopped 后调用 TryStartNextCycleAccount 自动推进 |
| 10 | `src/MaaWpfGui/ViewModels/UI/RootViewModel.cs` | 修改 | 版本比较忽略 `v` 前缀 |
| 11 | `src/MaaWpfGui/Views/UserControl/TaskQueue/StartUpTaskUserControl.xaml` | 修改 | 添加轮换 CheckBox、账号列表 ItemsControl、编辑模式 ComboBox、IsCompleted 蓝色高亮 |
| 12 | `src/MaaWpfGui/MaaWpfGui.csproj` | 修改 | 版本号从 0.0.1 改为 6.14.0 |
| 13 | `install/config/gui.new.json` | 修改 | StartUpTask 添加 AccountCycleEnabled / AccountNames 默认字段 |
| 14 | `install/config/gui.new.json.bak` | 修改 | 同上（备份文件同步） |

### fix/defer-rogue/1 启动

基于 `feat/defer-rogue` (commit `31b84f44a3`) 的 code review,发现 3 个问题:

| ID | 严重度 | 主题 |
|----|--------|------|
| A1 | CRITICAL | 步骤耗尽时,最后一个账号不会被 `MarkAccountCompleted` |
| A7 | WARNING | Cycle 运行中 `LateStageRogueAndReclamation` CheckBox 仍可点击 |
| A8 | WARNING | `LinkStart` 无 re-entrancy guard,运行中再次点击会重置 _cycleSteps/_currentStepIndex |

按 AGENTS.md 新约定(`fix/*` 必须从对应 `feat/*` 拉出),从 `feat/defer-rogue` 拉出 `fix/defer-rogue/1`。

| # | 文件/对象 | 操作 | 说明 |
|---|----------|------|------|
| 1 | `fix/defer-rogue/1` | 新建分支 | 从 `feat/defer-rogue` 拉出 |
| 2 | `fix_defer-rogue_1.md` | 新建 | 工作文档(已 .gitignore) |
| 3 | `AGENTS.md` | 修改 | 把"修复分支挂在对应 feat 下"写入正式约定:更新分支命名表 + 增加"修复分支必须挂在对应 feat 之下"小节 + 更新工作流示意图 |

### fix/defer-rogue/1 实施完成

| # | 文件 | 操作 | 说明 |
|---|------|------|------|
| 1 | `src/MaaWpfGui/ViewModels/UI/TaskQueueViewModel.cs:2195-2200` | 修改 | `AdvanceAccountCycle` 文档注释追加 fix/defer-rogue/1 段落 |
| 2 | `src/MaaWpfGui/ViewModels/UI/TaskQueueViewModel.cs:2208-2222` | 修改 | **A1**: 把 `prevStep = GetPreviousStep()` 移到 `nextStep == null` 早退分支**之前**;早退分支里先调用 `MarkPreviousStepCompleted(prevStep)` 再 `return` |
| 3 | `src/MaaWpfGui/ViewModels/UI/TaskQueueViewModel.cs:2235-2236` | 修改 | **A1**: 普通推进路径移除原 inline 块,改为调用 `MarkPreviousStepCompleted(prevStep)` |
| 4 | `src/MaaWpfGui/ViewModels/UI/TaskQueueViewModel.cs:2382-2400` | 修改 | **A1**: 新增私有方法 `MarkPreviousStepCompleted(AccountCycleStep?)`,语义与原 inline 块一致(`leftPhase2 \|\| lateStageOff`) |
| 5 | `src/MaaWpfGui/ViewModels/UI/TaskQueueViewModel.cs:1813-1826` | 修改 | **A8**: `LinkStart` 顶部加 `if (startUpConfig.IsCycling) { Release; return; }` guard,防止 Stop 后再次点击 / 定时器 / 快捷键在 cycle 中重置进度 |
| 6 | `src/MaaWpfGui/Views/UserControl/TaskQueue/StartUpTaskUserControl.xaml:140` | 修改 | **A7**: `LateStageRogueAndReclamation` CheckBox 加 `IsEnabled="{c:Binding '!IsCycling'}"`,Cycle 运行中灰显 |
| 7 | `LOG.md` | 修改 | 本节 |

**编译结果**:
```
dotnet build src/MaaWpfGui/MaaWpfGui.csproj -c Release -p:Platform=x64
0 个错误, 6 个 warning (3 个 SA1503 来自原 feat/defer-rogue 代码 2279/2329/2333 行,与本次修复无关)
```

**兼容性核查**:
- A1 修复仅改变 last step 路径的标记时机,不影响中间步骤
- A7 仅 UI 层禁用,VM 行为不变
- A8 仅在 `IsCycling == true` 时早退,不进入 LinkStart 主流程,不会改变已有行为;用户体感为"轮换运行时再点开始按钮没反应"(符合预期)
- AGENTS.md 仅文档改动,无代码影响

**部署验证**: (待补充,需在 `install/` 启动 MAA 走一遍 2 账号 cycle,确认最后一个账号 IsCompleted 变蓝)
**部署验证**: (待补充,需在 `install/` 启动 MAA 走一遍 2 账号 cycle,确认最后一个账号 IsCompleted 变蓝)

### fix/account_rotation/修改次数 — 测试发现的 6 项 cycle 行为异常修复

从 `feat/defer-rogue` 拉出 `fix/account_rotation/修改次数`。该 fix 同时修复 `feat/defer-rogue` 与 `feat/account_rotation` 交互产生的缺陷，合并目标为 `feat/defer-rogue`（在依赖链中位于下游）。

**测试发现的问题**:
- A 账号 Phase 1 显示两个"更新数据"（实为 OperBox+Depot 子任务共用 UserDataUpdate 名称）
- B 账号 Phase 1 显示带括号的"(干员识别)/(仓库识别)"（TaskItemViewModel taskId 查找失败 → 索引不一致）
- 肉鸽跑错阶段（可能是上号状态残留）
- UserDataUpdate 的 `IsTriggerDue` 跨账号跳过 → 子任务丢失

| # | 文件 | 操作 | 说明 |
|---|------|------|------|
| 1 | `src/MaaWpfGui/ViewModels/UserControl/TaskQueue/UserDataUpdateSettingsUserControlModel.cs:91-97` | 修改 | **#1**: cycle 中 (`GetAccountSwitchEnabled()`) 跳过 `IsTriggerDue` 检查,保证每个账号的 OperBox/Depot 子任务都被追加 |
| 2 | `src/MaaWpfGui/ViewModels/UI/TaskQueueViewModel.cs:2155-2192` | 修改 | **#6**: `SetStopped` 将 cycling 检查移到 idle 检查之前,当 `IsCycling=true && Idle=true`（LinkStartWithTasks 早退路径）时清理 cycling 状态,让正常停止接管,防止轮换永久卡住 |
| 3 | `src/MaaWpfGui/ViewModels/UI/TaskQueueViewModel.cs:2232-2234` | 修改 | **#5**: `AdvanceAccountCycle` 入口加 `_logger.Information` 日志记录 stepIdx/prev/next 信息 |
| 4 | `src/MaaWpfGui/ViewModels/UserControl/TaskQueue/StartUpSettingsUserControlModel.cs:326` | 修改 | **#5**: 新增 `CurrentStepIndex` 公开属性支持日志 |
| 5 | `src/MaaWpfGui/ViewModels/UI/TaskQueueViewModel.cs:2360-2362` | 修改 | **#5**: AdvanceAccountCycle 循环后追加 `_logger.Information` 记录 phase/switch/count/ret |
| 6 | `src/MaaWpfGui/ViewModels/UI/TaskQueueViewModel.cs:2337-2339` | 修改 | **#5**: AdvanceAccountCycle 中 Append task 时记录 `[CycleAdv] Append task #Idx` 日志 |
| 7 | `src/MaaWpfGui/ViewModels/UI/TaskQueueViewModel.cs:1983-1985` | 修改 | **#5**: LinkStartWithTasks 中 Append task 时记录 `[LinkStart] Append task #Idx` 日志 |
| 8 | `src/MaaWpfGui/ViewModels/UI/TaskQueueViewModel.cs:2287-2358` | 修改 | **#2/#3**: AdvanceAccountCycle 的 Phase 任务循环由 foreach + `IndexOf` 改为 **for 循环** (`int index = i`),消除重复项/顺序变更时的索引错误;同时保持原有 Phase 过滤/StartUp 跳过/`SetTaskIds` 逻辑不变 |
| 9 | `src/MaaWpfGui/ViewModels/UI/TaskQueueViewModel.cs:2256` | 修改 | **#4**: AdvanceAccountCycle 初始日志追加 `idx={CurrentStepIndex}/{CurrentStepCount}` 显示步骤位置 |
| 10 | `AGENTS.md` | 修改 | 新增跨多个 feat 的 fix 分支命名约束:合并目标选依赖链最下游的 feat,PR 说明列出所有涉及 feat |
| 11 | `LOG.md` | 修改 | 本节 |

**编译结果**: `dotnet build -c Release` 0 error, 0 warning

**兼容性核查**:
- #1 仅在 cycle 运行时跳过 `IsTriggerDue`,非 cycle 路径行为不变
- #6 仅在 `IsCycling=true` 时按新顺序命中,非 cycle 路径完全等价
- #2/#3 for 循环与 foreach 行为在无重复项时完全相同;原 foreach + IndexOf 在有重复项时会返回首个匹配索引导致错误 UI 显示,for 循环修复此问题
- AGENTS.md 仅文档改动,无代码影响

## 2026-07-16

### 已完结功能分支本地删除

将已合入 `branch` 的功能分支从本地删除，远端保留。

| # | 分支 | 操作 | 说明 |
|---|------|------|------|
| 1 | `feat/account_rotation` | `git branch -d` | 已合入 branch，本地删除 |
| 2 | `feat/defer-rogue` | `git branch -d` | 已合入 branch，本地删除 |
| 3 | `AGENTS.md` | 修改 | 追加「分支生命周期记录」章节 |
| 4 | `LOG.md` | 修改 | 本节 |

### AGENTS.md 全量重写

工作区中 `AGENTS.md` 长期处于 modified 状态（仅 24 行，只保留分支生命周期记录），缺失命名约定 / 模板 / 工作流 / 构建 / 代码风格 / 进行中分支速查等关键章节。按用户请求**全量重写**为 9 章节 ~318 行结构：项目概述 / 仓库拓扑 / 工作流与文档规范 / feat·fix 工作笔记模板 / 构建·部署·发布 / 代码风格与质量 / 进行中分支速查 / 分支生命周期记录 / 关键参考链接。模板章节恢复自 HEAD 上一个版本（`2b6517fa7a`）的内容并扩充「实施后追加」段落。

| # | 文件 | 操作 | 说明 |
|---|------|------|------|
| 1 | `AGENTS.md.local.bak` | 新建 → 删除 | 备份旧 24 行工作区版本；新版本写入并自检后删除（避免污染 git 工作区，新内容已写入正式文件） |
| 2 | `AGENTS.md:1-389` | 全量重写 | 9 章节结构；§4 模板含「实施记录 / 踩坑 / 待手动验证」「Code Review 议题 / 兼容性核查 / 编译部署结果」扩展段 |
| 3 | `LOG.md` | 修改 | 本节 |

**章节连续性自检**：

- §1 项目概述（§1.1 定位 / §1.2 技术栈 / §1.3 平台与许可证）
- §2 仓库拓扑与分支模型（§2.1 远程 / §2.2 本地分支 / §2.3 已完结 feat 分支处理约定）
- §3 工作流与文档规范（§3.1 工作笔记命名与保留 / §3.2 跟踪文档策略 / §3.3 启动新 feat 标准流程 / §3.4 fix 分支命名与合并目标 / §3.5 本地化）
- §4 feat / fix 工作笔记模板（§4.1 feat 模板 / §4.2 fix 模板）
- §5 构建、部署与发布（§5.1 C++ 端 / §5.2 WPF 端 / §5.3 本地运行 / §5.4 打包发布 / §5.5 子模块 / §5.6 辅助工具脚本）
- §6 代码风格与质量（§6.1 C++ / §6.2 C# / §6.3 提交前检查）
- §7 进行中分支速查（feat/expedite-threshold 一行速查表）
- §8 分支生命周期记录（§8.1 feat/account_rotation / §8.2 feat/defer-rogue / §8.3 feat/expedite-threshold 占位）
- §9 关键参考链接（§9.1 上游文档 / §9.2 本仓集成示例 / §9.3 上游关联项目）

**兼容性核查**：

- 与 HEAD 上一个 AGENTS.md 版本（`2b6517fa7a`）相比：§4 模板章节扩展 8 个「实施后追加」段落（原版仅 5 个），便于记录 commit / 兼容性核查 / 编译日志
- §7「进行中分支速查」为新增章节，与 §8「分支生命周期记录」形成「进行中 / 已完结」对照
- §5 / §6 / §9 全部为本版本新增内容
- 既有 §8 两个已完结 feat 条目（feat/account_rotation / feat/defer-rogue）原样保留

**未提交原因**：用户未明确要求 commit，按 AGENTS.md「提交策略」约定保持工作区状态。

### AGENTS.md 去除 feat / fix 工作笔记（第二次重写）

按用户进一步指令：**不要 feat 或 fix 工作笔记**，仅在 AGENTS.md 中记录分支存在性即可。
将上一版 9 章节结构缩减为 8 章节，移除以下内容：

- §3.1「feat / fix 工作笔记」（命名约定 / 各分支保留规则 / 提交策略整段删除）
- §4「feat / fix 工作笔记模板」（feat 模板 + fix 模板两个代码块整段删除）
- §3.2「跟踪文档策略」中 `feat_<name>.md` / `fix_<name>[_<n>].md` 行（在「是否跟踪」表中）
- §3.2「启动新 feat 标准流程」原步骤 3/4（创建与维护工作笔记）删除，步骤编号压缩
- §7「进行中分支速查」表头删除「工作文档」列（无工作笔记后此列无意义）

新增：开头一段摘要明确「本仓库不使用 feat / fix 工作笔记」，§3.1 加 blockquote 强调，所有变更通过 commit message 与 LOG.md 跟踪。

| # | 文件 | 操作 | 说明 |
|---|------|------|------|
| 1 | `AGENTS.md:1-269` | 二次重写 | 9 章节 → 8 章节；§3 由 5 小节压缩为 4 小节；§4-§9 重新编号为 §4-§8 |
| 2 | `LOG.md` | 修改 | 本节追加「AGENTS.md 去除 feat / fix 工作笔记」记录；同时修正前节「AGENTS.md 全量重写」中的描述 |

**新结构自检**：

- §1 项目概述（§1.1 定位 / §1.2 技术栈 / §1.3 平台与许可证）
- §2 仓库拓扑与分支模型（§2.1 远程 / §2.2 本地分支 / §2.3 已完结 feat 分支处理约定）
- §3 工作流与文档规范（§3.1 跟踪文档策略 / §3.2 启动新 feat 标准流程 / §3.3 fix 分支命名与合并目标 / §3.4 本地化）
- §4 构建、部署与发布（§4.1 C++ 端 / §4.2 WPF 端 / §4.3 本地运行 / §4.4 打包发布 / §4.5 子模块 / §4.6 辅助工具脚本）
- §5 代码风格与质量（§5.1 C++ / §5.2 C# / §5.3 提交前检查）
- §6 进行中分支速查（feat/expedite-threshold 一行速查表，已去除工作文档列）
- §7 分支生命周期记录（§7.1 feat/account_rotation / §7.2 feat/defer-rogue / §7.3 feat/expedite-threshold 占位）
- §8 关键参考链接（§8.1 上游文档 / §8.2 本仓集成示例 / §8.3 上游关联项目）

**兼容性核查**：

- 删除「feat_<name>.md / fix_<name>[_<n>].md 命名约定」「模板章节」意味着后续若有人想建工作笔记将无脚手架可用——按用户要求直接砍掉，不再保留
- 旧 commit `2b6517fa7a` / `5fcff1e27f` 引入的模板彻底从文档移除，但 git 历史中可回溯
- `.gitignore` 中保留的 `feat*.md` / `fix*.md` 规则变为「曾使用过」的死代码；按用户「不要 feat 或 fix 工作笔记」的范围外，未触动 `.gitignore`
- §7「分支生命周期记录」原样保留三个条目（account_rotation / defer-rogue / expedite-threshold 占位）
- §6「进行中分支速查」保留一行表，去掉「工作文档」列

### AGENTS.md 风格对齐原项目（第三次重写）

按用户强调：「请强调原项目风格和书写习惯，这点很重要」。
对齐目标为 `LOG.md`（仓库作者 levellmy 既有的条目风格）与上游 `docs/zh-cn/develop/*.md`（vuepress 站点语料），要点：

| 维度 | 调整前 | 调整后 |
|------|--------|--------|
| 段落 vs 表格 | 大量 prose 段落描述 | 优先 markdown 表格，行内 `**key**：value` 仅作脚注 |
| 章节深度 | 3 级（§1.1.1） | 2 级（§1.1），减少嵌套 |
| 文件引用 | 完整路径散落段中 | `path:line` 集中放在表格「文件 / 操作」列 |
| emoji / vuepress 容器 | 偶有 `> [!note]` 试探 | 全部移除，无 emoji、无 `::: tip` |
| 关键术语 | 中英混杂，无明确保留规则 | 明确「branch / feat / fix / FF / PR / 子模块 / cmake / dotnet」等保留英文 |
| 章节大段 | 整段「启动新 feat 流程」用 prose 串接 | 改为步骤表（`\| # \| 步骤 \| 产物 \|`），与 LOG.md 表格列一致 |
| 章节小结 | 末尾散文总结 | 删除，直接以表格收束 |

| # | 文件 | 操作 | 说明 |
|---|------|------|------|
| 1 | `AGENTS.md:1-242` | 三次重写 | 8 章节 + 20 二级小节，18 张表格；行数 212 → 224（+12 行，但 prose 行数从 ~70 降至 ~30，表格行数从 ~70 增至 ~140）；表格密度（`\|` 起始行）从 71 增至 128 |
| 2 | `LOG.md` | 修改 | 本节追加「AGENTS.md 风格对齐原项目」记录 |

**章节连续性自检**：

- §1 项目概述（§1.1 一句话 / §1.2 技术栈 / §1.3 平台与许可）
- §2 仓库拓扑与分支模型（§2.1 远程 / §2.2 本地分支 / §2.3 已完结 feat 处理）
- §3 工作流与文档规范（§3.1 跟踪文档策略 / §3.2 启动新 feat 标准流程 / §3.3 fix 分支命名与合并目标 / §3.4 本地化）
- §4 构建、部署与发布（§4.1 构建命令 / §4.2 子模块 / §4.3 打包发布 / §4.4 辅助脚本）
- §5 代码风格与质量
- §6 进行中分支速查
- §7 分支生命周期记录（§7.1 account_rotation / §7.2 defer-rogue / §7.3 expedite-threshold 占位）
- §8 关键参考链接（§8.1 上游文档 / §8.2 本仓集成示例 / §8.3 上游关联项目）

**兼容性核查**：

- 章节编号与上一版完全一致（8 章节，仅 §1.1 / §1.3 标题文案从「定位 / 平台与许可证」微调为「一句话 / 平台与许可」），无引用失效
- 二级小节数从 21 减至 20（合并 §5 三个 prose 段落为单一表格）
- 所有「文件路径 + 行号」描述已迁移至表格列，prose 中不再出现
- §3.2「启动新 feat」由 7 步 prose 改为 7 行步骤表，与 LOG.md 表格列一致
- §4.1「构建命令」由 prose bullet 改为单表，与 LOG.md 操作列一致

**未提交原因**：用户未明确要求 commit。

## 2026-07-23

### 删除无用分支

| # | 操作 | 说明 |
|---|------|------|
| 1 | 删除分支 `feat/expedite-threshold` | 本地已删除，远端保留（HEAD `5fcff1e27f`，未合入）|
| 2 | 删除分支 `feat/idea` | 本地已删除，远端保留（HEAD `dc2212d54b`，无独立 commit）|
| 3 | 删除分支 `fix/account_rotation/5` | 本地已删除，远端保留（HEAD `83a5dc36c1`）|
| 4 | 更新 AGENTS.md §6 / §7 | `feat/expedite-threshold` 移入生命周期记录；新增 `feat/idea` 记录；`fix/account_rotation/5` 记入 §7.1 子修复分支 |

### feat/expedite-threshold 启动

| # | 文件路径 | 操作 | 说明 |
|---|----------|------|------|
| 1 | `src/MaaCore/Task/Miscellaneous/AutoRecruitTask.h` | 新增 setter `set_expedite_min_level` | 加急门槛接口 |
| 2 | `src/MaaCore/Task/Miscellaneous/AutoRecruitTask.h` | 新增成员 `m_expedite_min_level` / `m_last_confirmed_min_level` | 门槛值与最近确认星级 |
| 3 | `src/MaaCore/Task/Miscellaneous/AutoRecruitTask.cpp` | 新增 `set_expedite_min_level` 实现 | setter |
| 4 | `src/MaaCore/Task/Miscellaneous/AutoRecruitTask.cpp` | `_run()` 移除旧的 `try_use_expedited` 块 | 改由 `recruit_one()` 内逐槽判定 |
| 5 | `src/MaaCore/Task/Miscellaneous/AutoRecruitTask.cpp` | `recruit_calc_task()` 写入 `m_last_confirmed_min_level` | 加急决策依据 |
| 6 | `src/MaaCore/Task/Miscellaneous/AutoRecruitTask.cpp` | `recruit_one()` 加急分支 | 4★+ 时 `recruit_now()` 替代 `confirm()` |
| 7 | `src/MaaCore/Task/Interface/RecruitTask.cpp` | 解析 `expedite_min_level` 参数 | 新参数透传 |
| 8 | `src/MaaWpfGui/Configuration/Single/MaaTask/RecruitTask.cs` | +`ExpediteMinLevel` 属性 | 配置模型 |
| 9 | `src/MaaWpfGui/Models/AsstTasks/AsstRecruitTask.cs` | +`ExpediteMinLevel` 属性 + 序列化 | DTO |
| 10 | `src/MaaWpfGui/ViewModels/UserControl/TaskQueue/RecruitSettingsUserControlModel.cs` | +`ExpediteMinLevelList` / `UseExpeditedMinLevel` / `UseExpeditedMinLevelVisible` | ViewModel |
| 11 | `src/MaaWpfGui/Views/UserControl/TaskQueue/RecruitSettingsUserControl.xaml` | +门槛下拉框 | UI |
| 12 | `src/MaaWpfGui/Res/Localizations/{zh-cn,en-us,ja-jp,ko-kr,zh-tw}.xaml` | +`ExpediteMinLevel*` 6 个 key | 五语本地化 |

### feat/expedite-threshold 合并入 branch

在 `feat/expedite-threshold` 上补齐未暂存的 UI/本地化适配细节后，FF 合并到 `branch`，本地删除 feat 分支。

| # | 文件/对象 | 操作 | 说明 |
|---|----------|------|------|
| 1 | `branch` | FF 合并 | `feat/expedite-threshold` 3 个新 commit（`7df4e94e3f`（feat 初始）+ `9c02b9e41a`（暂存前置变更）+ `cbec3d1fb0`（完善五语本地化与 ViewModel 适配）），HEAD `dc2212d54b` → `cbec3d1fb0` |
| 2 | `feat/expedite-threshold` | `git branch -d` | 已合入 branch，本地删除 |
| 3 | `AGENTS.md` | 修改 | §6 清空（无进行中分支）；§7.3 更新为已合入状态；§7 开头补充 2026-07-23 删除日期 |
| 4 | `LOG.md` | 修改 | 本节 |

## 2026-07-24

### fix/account-official-recognize 启动

「开始唤醒」任务在 **官服（Official）+ 账号轮换** 场景下卡死。`install/debug/asst.log` 实测：MAA 成功识别 `StartToWakeUp.png`（score 0.926）与 `AccountManager.png`（score 0.904），但 `LoginOther` 之后 `AccountManagerOfficial` 与 `AccountManagerBili` 30 次 retry 全失败，`AccountSwitchTask::navigate_to_start_page()` 走不到任一合法返回路径，最终 `Login failed, entering game-restart loop`。

根因（`resource/tasks/tasks.json:805-807`）：

```json
"AccountManagerOfficial": {
    "roi": [570, 165, 140, 80]    ← 只有 roi，无 template/algorithm/text
}
```

对比 `AccountManagerBili`（第 808-813 行）已有 `algorithm: "OcrDetect"` + `text: ["登录记录"]`，官服定义残缺。修复方案 A+C：补全官服 OCR 识别 + `AccountSwitchTask::navigate_to_start_page()` 加诊断日志。仅在本分支 `fix/account-official-recognize` 修复，不推上游。

| # | 文件/对象 | 操作 | 说明 |
|---|----------|------|------|
| 1 | `fix/account-official-recognize` | 新建分支 | 从 `branch` 拉出 |
| 2 | `resource/tasks/tasks.json:805-807` | 待修改 | `AccountManagerOfficial` 补 OcrDetect 识别「登录记录」 |
| 3 | `src/MaaCore/Task/Miscellaneous/AccountSwitchTask.cpp:65-84` | 待修改 | `navigate_to_start_page()` 加诊断日志 |

### fix/account-official-recognize 实施完成

| # | 文件 | 操作 | 说明 |
|---|------|------|------|
| 1 | `resource/tasks/tasks.json:805-810` | 修改 | `AccountManagerOfficial` 由 `{"roi":[570,165,140,80]}` 补全为 `{"Doc":"官方服账号切换界面识别，与 B 服统一 OCR「登录记录」","algorithm":"OcrDetect","text":["登录记录"],"roi":[237,50,771,242]}`（与 B 服 `AccountManagerBili` 对齐） |
| 2 | `src/MaaCore/Task/Miscellaneous/AccountSwitchTask.cpp:71` | 修改 | `navigate_to_start_page()` 在 `get_last_task_name()` 之后追加 `Log.info(__FUNCTION__, "last matched task:", last_name);`，便于后续识别失败时定位 |
| 3 | `src/MaaCore/Task/Miscellaneous/AccountSwitchTask.cpp:71-77` | 修改 | 4 个 `else if` 合并为单 `if (... || ... \|\| ... \|\| ...)`，减少分支嵌套 |
| 4 | `install/MaaCore.dll` | 部署 | Release 编译产物，时间戳 2026/7/24 12:45:38，字节特征串 `last matched task:` 命中（offset 3384944 / size 4188160） |
| 5 | `install/resource/tasks/tasks.json` | 部署 | 同步源端 SHA256（`39972BD09F9EDEA0E7B7D71F8E084071126521E333F9DEDD3296115BC6F5C027`），177232 字节，字节特征串 `官方服账号切换` 命中（offset 25825） |
| 6 | `LOG.md` | 修改 | 本节 |

**编译结果**: `cmake --build build --target MaaCore -j 4 --config Release` 成功，仅遗留标准 `LNK4098` 默认库警告（与上游一致）。`cmake --install build --config Release` 成功（`MaaUpdater` 报错为 AGENTS.md §4.1 已知 VS 2026 SDK 路径 bug，不影响 C++ 端部署）。

**部署产物验证**:
- `install/MaaCore.dll` 2026/7/24 12:45:38（4188160 字节）
- `install/resource/tasks/tasks.json` 2026/7/24 12:45:11（177232 字节）
- 源端 `resource/tasks/tasks.json` ↔ 安装端 SHA256 完全一致

**预期效果**:
1. 官服账号切换时，`LoginOther` → 识别 `登录记录` OCR → 命中 `AccountManagerOfficial` → 返回 `true` → 进入 `AccountSwitchTask::select_account()` 选择目标账号
2. 即便 OCR 仍未命中，`Log.info("last matched task:", last_name)` 输出实际最后匹配节点，便于排错
3. 单账号场景（`AccountName` 为空时 `m_account_switch_task_ptr` 仍被 disable）行为不变

**待手动验证（需模拟器环境）**:
1. 单账号 + StartGame=true 跑完整日常 → StartUp 完成进入首页
2. 双账号轮换（192→189）→ 验证 `AccountManagerOfficial` 命中，控制台/日志显示 `last matched task: AccountManagerOfficial`
3. B 服回归测试 → 不破坏 B 服原有 `登录记录` OCR 行为
4. 切号中途异常 → 看 `last matched task:` 输出是否仍有诊断信息

**未推送上游**: 仅本仓库 `branch` 修复，不向 upstream 提 PR。

## 2026-07-29

### feat/diagnostic-export 启动

在 IssueReport 页面新增「导出诊断包」功能，支持按日期范围选择性导出日志 + `diagnostic.json` 系统信息 + 可选配置文件/缓存/自定义资源。

| # | 文件/对象 | 操作 | 说明 |
|---|----------|------|------|
| 1 | `feat/diagnostic-export` | 新建分支 | 从 `branch` 拉出 |
| 2 | `LOG.md` | 修改 | 本节（启动记录） |

### feat/diagnostic-export 实施完成

| # | 文件 | 操作 | 说明 |
|---|------|------|------|
| 1 | `src/MaaWpfGui/Models/DiagnosticInfo.cs` | 新建 | 系统信息数据模型 + `Collect()` 静态收集方法：OS/.NET 版本/架构、GPU、管理员、Wine、MAA 版本（UI/Core/Resource） |
| 2 | `src/MaaWpfGui/ViewModels/UserControl/Settings/IssueReportUserControlModel.cs:42-70` | 修改 | 新增诊断导出属性：`DiagnosticDateRange`(默认 7 天)、`IncludeConfig`/`IncludeCache`/`IncludeCustomResource` 三个 CheckBox、`DateRangeOption` record 与 `DateRangeOptions` 懒加载列表 |
| 3 | `src/MaaWpfGui/ViewModels/UserControl/Settings/IssueReportUserControlModel.cs:292-399` | 修改 | 新增 `ExportDiagnosticPackage()` 方法：收集系统信息 → diagnostic.json → 逐日志文件按日期范围过滤行 → 可选目录复制 → zip 打包 → growl + 打开 reports 目录 |
| 4 | `src/MaaWpfGui/ViewModels/UserControl/Settings/IssueReportUserControlModel.cs:401-443` | 修改 | 新增 `CopyFilteredLog()` 辅助方法：正则 `^\[\d{4}-\d{2}-\d{2}` 逐行解析日志时间戳，仅保留日期范围内行；非时间戳行（异常栈）自动保留 |
| 5 | `src/MaaWpfGui/Views/UserControl/Settings/IssueReportUserControl.xaml:97-165` | 修改 | IssueReport 页面新增诊断导出区域：日期范围 ComboBox + 3 个 CheckBox + 导出按钮 |
| 6 | `src/MaaWpfGui/Res/Localizations/zh-cn.xaml:1457-1473` | 修改 | +13 个中文 localization key（`ExportDiagnosticPackage*` / `DiagnosticDateRange` / `DiagnosticInclude*` / `DiagnosticLast*`） |
| 7 | `src/MaaWpfGui/Res/Localizations/en-us.xaml:1456-1472` | 修改 | +13 个英文 localization key |
| 8 | `src/MaaWpfGui/Res/Localizations/zh-tw.xaml:1457-1473` | 修改 | +13 个繁中 localization key |
| 9 | `src/MaaWpfGui/Res/Localizations/ja-jp.xaml:1457-1473` | 修改 | +13 个日文 localization key |
| 10 | `src/MaaWpfGui/Res/Localizations/ko-kr.xaml:1458-1474` | 修改 | +13 个韩文 localization key |
| 11 | `LOG.md` | 修改 | 本节（实施完成记录） |

**编译/部署结果**: `dotnet build -c Release` 0 error, 50 warning（全为 StyleCop 规则与 `#nullable` 上下文内 `string?` 注释，与 AGENTS.md §5 一致，不阻断）；C++ 端未改动，无需 cmake。

**关键设计**:
- 日志过滤使用 `StreamReader.ReadLine()` 逐行流式读取，避免 64MB+ 大日志（asst.log）加载到内存
- 非时间戳行（C++ 异常栈回溯、Serilog 多行异常）自动保留，附在最后一条有时间戳的日志后
- `DiagnosticInfo.Collect()` 通过 `RuntimeInformation` / `WineRuntimeInformation` / `GpuOption.GetCurrent()` 收集系统信息，不引入新外部依赖
- `DateRangeOptions` 使用懒加载（`Lazy<T>` 模式），避免 `LocalizationHelper` 在静态初始化时未就绪
- 重用已有 `CopyDirectoryIfExists()` 私有方法复制可选目录
- 日志始终包含 crash.log 和 dumps/ 目录（即使超出日期范围，因通常很小且对排错关键）

