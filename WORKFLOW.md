# 上游拉取新功能操作流程（SOP）

> 基于 2026-08-06 fork 同步上游 v6.16.5（master-v2 `a02e95a9af`）的实际经验总结。
> 适用场景：从上游 `MaaAssistantArknights/MaaAssistantArknights` 拉取新功能到本地 fork。

## §0 适用范围与前置假设

| 项 | 说明 |
|----|------|
| **适用对象** | 本 fork 仓库所有 maintainer |
| **前置假设 1** | `master` 分支永远保持与 `upstream/master-v2` 同步（镜像） |
| **前置假设 2** | `staging` 分支累积本地 fork 工作（162 个 commit 量级） |
| **前置假设 3** | `branch` 分支为稳定下游基线，由用户手动晋升 staging |
| **触发时机** | 上游发布新版本（如 v6.17.0）或累积 N 个 commit 后 |
| **预估耗时** | 完整流程 1-2 小时（含冲突手解与编译） |

---

## §1 预检清单（5 min）

```powershell
# 1.1 确认 remote 配置
git remote -v
# 应包含：
#   Github    git@github.com:Cecilian-Elysian/FORK_MAA.git   （fork 远端，非 origin）
#   upstream  https://github.com/MaaAssistantArknights/MaaAssistantArknights.git

# 1.2 备份当前 staging
$date = Get-Date -Format "yyyyMMdd"
git branch backup/staging-pre-merge-$date staging

# 1.3 统计 fork commits 与上游 commits 数量
$forkCount = (git log master..staging --oneline).Count
$upstreamCount = (git log staging..upstream/master-v2 --oneline).Count
Write-Host "Fork commits: $forkCount, Upstream new: $upstreamCount"

# 1.4 拉取上游最新引用（即使本地已缓存，再 fetch 一次确保最新）
git fetch upstream
```

**完成标志**：看到最新的 upstream commits 列表（如包含 v6.17.0 release）。

---

## §2 评估冲突矩阵（10 min）

```powershell
# 2.1 fork 修改的文件清单（排除 resource/）
$forkFiles = git diff master..staging --name-only --diff-filter=ACD | Where-Object { $_ -notmatch '^resource/' }

# 2.2 上游近期修改的文件清单（限定时间窗口）
$since = "2026-07-27"  # 调整为上次合并的时间
$upstreamFiles = git log staging..upstream/master-v2 --no-merges --since=$since --name-only --format="" |
    Where-Object { $_ -ne "" -and $_ -notmatch '^resource/' } | Select-Object -Unique

# 2.3 重叠文件（fork 与 upstream 都改了）→ 冲突候选
$overlap = $forkFiles | Where-Object { $upstreamFiles -contains $_ }
Write-Host "潜在冲突文件数: $($overlap.Count)"
$overlap | ForEach-Object { Write-Host "  - $_" }

# 2.4 分类决策表
# - fork-only（保留）: $forkFiles | Where-Object { $upstreamFiles -notcontains $_ }
# - upstream-only（接收）: $upstreamFiles | Where-Object { $forkFiles -notcontains $_ }
# - 双方修改（手解）: $overlap
```

**完成标志**：识别出 fork-only / upstream-only / 双方修改三类文件，并就「双方修改」类文件准备好手解策略。

### §2.5 fork 标记基线核对（5 min）

> 对照 §6.5.1-§6.5.4 的 fork 私有标记，merge 前先跑一遍 §6.5.8 验证命令组记下命中数（行号可以漂移，**命中数是刚性基线**）；merge 手解后重跑，命中数不得低于基线。任何一项归零 = 对应 fork 功能在合并中被吞，先回查再编译。

**完成标志**：§6.5.8 全部标记命中数与 merge 前基线一致或被替代（被上游吸收时在 commit message 记录）。

---

## §3 删除本地不成熟功能（10 min，可选）

> 仅当用户明确要求删除某些 fork 独有特性时执行。本范例为 2026-08-06 删 `feat/auto-recruit-3star-to-4star`。

```powershell
# 3.1 确定要删除的 fork 特性（用户决策）
# 例如：删 "3★→4★ 自动升级" + 相关 fix

# 3.2 列出受影响的文件
git grep -l "auto_upgrade_3star_with_4star\|set_auto_upgrade_3star_with_4star\|m_auto_upgrade_3star_with_4star\|m_original_min_level" src/ docs/

# 预期清单（参考 2026-08-06 实例）：
# - src/MaaCore/Task/Miscellaneous/AutoRecruitTask.h
# - src/MaaCore/Task/Miscellaneous/AutoRecruitTask.cpp
# - src/MaaCore/Task/Interface/RecruitTask.cpp
# - docs/{zh-cn,zh-tw,en-us,ja-jp,ko-kr}/protocol/integration.md
# - src/MaaWpfGui/Res/Localizations/{5 语}.xaml
# - src/MaaWpfGui/Configuration/Single/MaaTask/RecruitTask.cs
# - src/MaaWpfGui/Models/AsstTasks/AsstRecruitTask.cs
# - src/MaaWpfGui/ViewModels/UserControl/TaskQueue/RecruitSettingsUserControlModel.cs
# - src/MaaWpfGui/Views/UserControl/TaskQueue/RecruitSettingsUserControl.xaml

# 3.3 编辑删除（按文件清单逐个手动 edit）
# 3.4 完成标志
git grep "auto_upgrade_3star_with_4star" src/ docs/   # 应 0 result
git grep "m_original_min_level" src/MaaCore/             # 应 0 result

# 3.5 提交
git add -u
git commit -m "revert: 移除 <feature-name> 及依赖（<理由>）"
```

**完成标志**：`git grep` 在目标文件返回 0 result，commit 成功。

---

## §4 文档同步（5 min）

```powershell
# 4.1 AGENTS.md §6：移除已不存在的分支条目
# 检查 §6 中是否有已删除分支名
git branch | Out-Null  # 列出当前分支
# 手动编辑 AGENTS.md §6 表格，删除任何不在 `git branch` 输出中的分支名

# 4.2 AGENTS.md §2.4：时间戳更新
# 找到 §2.4 "当前待验证内容" 子节，更新日期 + 验证项

# 4.3 csproj Version 字段
# 选择策略：跟随上游 0.0.1（CI 注入）或保留 fork 标识
# 路径：src/MaaWpfGui/MaaWpfGui.csproj
git diff src/MaaWpfGui/MaaWpfGui.csproj | grep -A1 "Version"

# 4.4 .gitignore
# 添加 dotnet publish 产物（如未添加）
# MAA.deps.json
# MAA.runtimeconfig.json

# 4.5 CHANGELOG.md
# 追加 fork 累积条目 + 删除清单

# 4.6 提交
git add AGENTS.md src/MaaWpfGui/MaaWpfGui.csproj .gitignore CHANGELOG.md
git commit -m "docs(cleanup): 同步仓库清理（AGENTS/csproj/CHANGELOG）"
```

**完成标志**：`git status -s` 为空（除本步骤新增 commit）。

---

## §5 假历史关联（5 min，关键步骤）

> **不执行此步将导致 merge 失败**，报 "refusing to merge unrelated histories" 或大量 add/add 冲突。

### §5.1 问题原因

fork base commit `c8c8e75be5` 在 2026-07-11 初始化时无父节点（孤儿 root），无法与 upstream 历史建立 3-way merge 关系。

**原始错误输出**（2026-08-06 实测）：

```
$ git merge --no-commit --no-ff upstream/master-v2
fatal: refusing to merge unrelated histories
```

若强行加 `--allow-unrelated-histories`，会触发 249 个 add/add 冲突，**不可控**：

```
CONFLICT (add/add): Merge conflict in .github/workflows/ci.yml
CONFLICT (add/add): Merge conflict in CHANGELOG.md
... (共 249 个)
```

### §5.2 解决：graft

```powershell
# 5.2.1 识别 fork root 对应的上游版本
$forkRoot = "c8c8e75be5227d0fcc8d1ebe9fdbc462055cdfce"
$version = git show $forkRoot:src/MaaWpfGui/MaaWpfGui.csproj | Select-String "Version" | Select-Object -First 1
Write-Host "Fork base Version: $version"
# 2026-08-06 实例输出: <Version>6.14.0</Version>

# 5.2.2 在上游历史中找对应 release
git log upstream/master-v2 --grep="Release v6.14.0" --oneline | Select-Object -First 1
# 2026-08-06 实例输出:
#   6147357bd0 Release v6.14.0 (#17291)

# 5.2.3 写 graft 文件
$upstreamAnchor = "6147357bd074d926047d1e391ba3759f40477ef1"
"$forkRoot $upstreamAnchor" | Out-File ".git/info/grafts" -Encoding utf8 -NoNewline

# 5.2.4 验证 merge-base
git merge-base HEAD upstream/master-v2
# 预期输出: c8c8e75be5227d0fcc8d1ebe9fdbc462055cdfce
# 若输出为空或不同，graft 未生效，检查拼写
```

**完成标志**：`git merge-base HEAD upstream/master-v2` 返回 fork root SHA。

---

## §6 合并 upstream（30 min）

### §6.1 试合并

```powershell
git switch staging
git merge --no-commit --no-ff upstream/master-v2
```

### §6.2 真实 conflict 输出示例（2026-08-06 v6.16.5 合入，实际数据）

```
Auto-merging .gitignore
CONFLICT (content): Merge conflict in .gitignore
Auto-merging CHANGELOG.md
CONFLICT (content): Merge conflict in CHANGELOG.md
Auto-merging docs/en-us/protocol/integration.md
... (5 语种都冲突)
CONFLICT (content): Merge conflict in resource/tasks/tasks.json
CONFLICT (content): Merge conflict in resource/version.json
CONFLICT (modify/delete): src/MaaMacGui deleted in HEAD and modified in upstream/master-v2.
... (MaaMacGui + maa-cli)
CONFLICT (content): Merge conflict in src/MaaWpfGui/Res/Localizations/en-us.xaml
... (5 语种都冲突)
CONFLICT (content): Merge conflict in src/MaaWpfGui/ViewModels/UI/CopilotViewModel.cs
CONFLICT (content): Merge conflict in src/MaaWpfGui/ViewModels/UI/TaskQueueViewModel.cs
CONFLICT (content): Merge conflict in .../StartUpSettingsUserControlModel.cs
CONFLICT (content): Merge conflict in src/MaaWpfGui/Views/UI/CopilotView.xaml
CONFLICT (content): Merge conflict in tools/OptimizeTemplates/optimize_templates.json
CONFLICT (content): Merge conflict in tools/local-install.bat
Automatic merge failed; fix conflicts and then commit the result.
```

冲突文件数：**~20 个真实冲突**（不是 249 个 — graft 起效）。

### §6.3 列出冲突

```powershell
git diff --name-only --diff-filter=U
```

### §6.4 冲突分类与处理

| 冲突类型 | 范例 | 处理 |
|----------|------|------|
| **rename/delete** | `.claude/skills/*` → `.agents/skills/*`（fork 删除，上游重命名） | 接 upstream rename：`git checkout --theirs .agents/skills/*` |
| **modify/delete (submodule)** | `src/MaaMacGui`, `src/maa-cli` | 接 upstream：`git update-index --cacheinfo 160000,<new_sha>,<path>` |
| **content conflict** | 5 语种 xaml / docs / cs 文件 | 手动 edit（按 AGENTS.md §2.6 决策） |
| **namespace rename** | `MaaWpfGui.Services.Notification` → `ExternalNotification` | `git grep` + sed/edit |

### §6.5 关键冲突手解范例（2026-08-06 实例）

#### 范例 1：5 语种 xaml 本地优先合并

冲突标记示例：

```
<<<<<<< HEAD
    <system:String x:Key="PasteClipboardCopilotSetTip">读取剪贴板并添加为作业集</system:String>
=======
>>>>>>> upstream/master-v2
    <system:String x:Key="ImportFilesTip">批量导入</system:String>
```

**手解**（保留 fork 的 `PasteClipboardCopilotSetTip`）：

```powershell
# 使用 edit 工具删除 <<<<<<< HEAD 与 ======= 之间的内容
# 保留 fork 行，移除 upstream 部分
```

#### 范例 2：namespace 迁移（上游 rft: Wpf配置全迁移 #17392）

**错误输出**（实际）：

```
src/MaaWpfGui/ViewModels/UI/TaskQueueViewModel.cs(41,26): error CS0234:
命名空间"MaaWpfGui.Services"中不存在类型或命名空间名"Notification"
```

**诊断**：

```powershell
git grep -n "MaaWpfGui.Services.Notification" src/MaaWpfGui/
# 输出: TaskQueueViewModel.cs:41
```

**修复**：

```csharp
// src/MaaWpfGui/ViewModels/UI/TaskQueueViewModel.cs:41
-using MaaWpfGui.Services.Notification;
+using MaaWpfGui.Services.ExternalNotification;
```

```powershell
git add src/MaaWpfGui/ViewModels/UI/TaskQueueViewModel.cs
```

#### 范例 3：CHANGELOG.md 双章节并存

冲突标记：

```
<<<<<<< HEAD
## v6.14.0-fork.20260806

基于上游 v6.16.5...
=======
## v6.16.5
>>>>>>> upstream/master-v2
```

**手解**：保留 fork 章节 + 移除冲突标记 + 在后面追加 upstream 章节标题。

### §6.5.1 fork 私有功能完整冲突地图（2026-08-08 更新）

> 本节是 fork 存活功能的**完整登记**，按功能分区。merge 遇到相关文件冲突时，对照本表手解且必须保留 fork 侧，否则对应功能退化。merge 前跑 §2.5 基线核对，merge 后跑 §6.5.8 验证。

#### A. 账号轮换核心 — `src/MaaWpfGui/ViewModels/UI/TaskQueueViewModel.cs`

| fork 私有标记（git grep 可定位） | 位置（行号可以漂移） | 内容 |
|------|------|------|
| `fix/account-cycle-start-race` | `AdvanceAccountCycle` 内 `AsstStart()` 调用处（约 2557-2575） | `AllTasksCompleted` 后 Core 工作线程仍处于 `wait_for(task_delay)` 睡眠窗口（约 500ms），此时 `AsstStart()` 必返回 false；用 `startOk = taskRet && (AsstStart() || AsstRunning())` 兜底判定 + 失败分支 `AsstStop()` 清队列。**若丢失：每次轮换推进误报「出现未知错误」+ 误停轮换**（详见 `LOG.md` 2026-08-07） |
| `fix/account_rotation/6` | `AdvanceAccountCycle` 切号前重置 / `SetTaskIds` / `CurrentCycleAccountName`（5 处维护）；`TaskItemViewModel.cs` `SetTaskIds` 末尾 `fix/account_rotation/6` 重置；`TaskQueueView.xaml` Header Block；5 语 xaml `CurrentAccountLabel` | 切号时左侧任务面板重置 + 当前账号 Header |
| `fix/account_rotation/修改次数` | `AdvanceAccountCycle` / `SetStopped` / `UserDataUpdateSettingsUserControlModel.cs:96` | 轮换状态机幂等保护、失败即停轮换 |
| `[CycleAdv]` 日志 | `AdvanceAccountCycle` / `LinkStartWithTasks`（约 2359 / 2501 / 2532 三个日志点） | 轮换 append 任务诊断日志 |

merge 后验证：`git grep -c "AdvanceAccountCycle" src/MaaWpfGui/` ≥ 3；`git grep -c "fix/account-cycle-start-race" src/MaaWpfGui/` ≥ 1。

### §6.5.2 公招加急 C++（fork 现存全部 C++ 定制）— `src/MaaCore/Task/Miscellaneous/AutoRecruitTask.{h,cpp}` + `src/MaaCore/Task/Interface/RecruitTask.cpp`

> 上游 v6.16.5 已重构公招加急路径（删除了旧的 `try_use_expedited` 机制），本区块是 fork 8/7 恢复后的唯一 C++ 私有逻辑，**每次上游 recruit 相关改动都是高危冲突**。

| fork 私有标记（git grep 可定位） | 位置 | 内容 | 丢失后果 |
|------|------|------|------|
| `expedite_min_level` / `set_expedite_min_level` | `RecruitTask.cpp:54,87`（参数透传）+ `AutoRecruitTask.h/.cpp`（setter + `m_expedite_min_level`） | 公招加急门槛：确认组合最低星级 ≥ 门槛才用加急许可（0=不限） | 加急门槛 UI 失效，三星也加急 |
| `fix/expedite-threshold`（注释标记） | `AutoRecruitTask.cpp` `recruit_one()` 入口重置 `m_last_confirmed_min_level`；加急判定位于 `confirm()` **之后** | 判定必须在 confirm 之后（主页才有「立即招」按钮）；星级状态不得跨槽位污染 | 加急必失败 / 串槽判定 |
| `fix/recruit-expedite-slot-target`（注释标记） | `AutoRecruitTask.cpp` `recruit_now(size_t index)` + `slot_index_from_rect()` | 加急点击按槽位限定 roi（`RecruitNow@Slot0..3`），多槽位同时进行时不串位 | 加急串到左上槽位（三星被急、四星留 9h） |

merge 后验证：`git grep -c "expedite_min_level" src/MaaCore/` ≥ 8；`git grep -c "RecruitNow@Slot" resource/` ≥ 4。

### §6.5.3 `resource/tasks/tasks.json`（注意：双向约束）

| 区块 | 标记 | 处理 |
|------|------|------|
| `RecruitNow@Slot0..3` 四个变体（+16 行，位于 `RecruitNow` 与 `RecruitNowConfirm` 之间） | `"RecruitNow@Slot"` | **必须保留**（fix/recruit-expedite-slot-target 产物）；缺失 = 加急串位回归 |
| account-switch 区块（`LoginOther` / `AccountManagerOfficial` 等） | `AccountSwitch` 相关 task 名 | **必须保持 master 原样**（8/7 `c34403ac94` 已回归）。上游若改该区块，直接取 theirs，**绝不能复活 fork 的 `AccountManagerPageConfirm` 任务或模板** |

merge 后验证：`git grep -c "RecruitNow@Slot" resource/tasks/tasks.json` = 4；`git grep -c "AccountManagerPageConfirm" resource/` = 0。

### §6.5.4 Roguelike onErrorNext 加固 — `resource/tasks/Roguelike/base.json` + `JieGarden.json`

> 注意：`ExitThenAbandon` 相关 `onErrorNext` 在全部主题 JSON 里大量存在，**绝大多数是上游自带**。fork 私有新增只有 2 处（2026-07 `fix(rogue)` 产物，8/7 重建时保留）：

| 标记 | 位置 | 内容 |
|------|------|------|
| `"onErrorNext": ["Roguelike@ExitThenAbandon"]` | `base.json` 约 401 行（任务是 `next` 含 `"Roguelike@StartUpThemes#next"` 的关卡选择任务） | fork 兜底：关卡选择失败时退出肉鸽，防卡死 |
| `"onErrorNext": ["JieGarden@Roguelike@ExitThenAbandon"]` | `JieGarden.json` 约 1211 行（任务 `next` 含 `"JieGarden@Roguelike@Stages_leaveBoskyPassage#next"` 的关卡选择任务） | 同上，JieGarden 主题内层入口 |

merge 后验证：两处 `onErrorNext` 仍存在（`git grep -n '"onErrorNext": \["Roguelike@ExitThenAbandon"\]' base.json` 命中 1；JieGarden 同理）。

### §6.5.5 账号轮换外围（WPF）

| 文件 | fork 内容 | 冲突风险 |
|------|------|------|
| `src/MaaWpfGui/Main/AsstProxy.cs`（+14） | ① `AllTasksCompleted` 回调尾部轮换推进分支（`Instances.TaskQueueViewModel.AdvanceAccountCycle()`，约 1313-1321）；② `MissionStart` 战后理智换算（`postBattleSanity`，约 1634） | 高（上游回调区常有调整） |
| `src/MaaWpfGui/Configuration/Single/MaaTask/StartUpTask.cs`（+17） | 账号轮换配置字段（多账号列表、切号开关） | 中 |
| `src/MaaWpfGui/Configuration/Single/MaaTask/RecruitTask.cs`（+31） | `ExpediteMinLevel` 配置 + 传递 | 中 |
| `src/MaaWpfGui/ViewModels/UserControl/TaskQueue/StartUpSettingsUserControlModel.cs`（+404） | 账号轮换 UI / 循环配置大部头 | 高 |
| `src/MaaWpfGui/ViewModels/UserControl/TaskQueue/RecruitSettingsUserControlModel.cs`（+64） | `ExpediteMinLevel` UI | 高 |
| `src/MaaWpfGui/Models/AccountCycleItem.cs` / `AccountCycleStep.cs` / `DiagnosticInfo.cs`；`src/MaaWpfGui/ViewModels/Orchestration/AccountCycleOrchestrator.cs` | fork 独创文件（上游不存在） | 低（除非上游撞名） |
| `src/MaaWpfGui/Res/Localizations/{5 语}.xaml`（各 +42） | `CurrentAccountLabel` / 加急门槛等 key | 中（冲突就保留 fork key + 追加 upstream key） |
| `src/MaaWpfGui/Views/UI/TaskQueueView.xaml`（+34）/ `StartUpTaskUserControl.xaml`（+102）/ `RecruitSettingsUserControl.xaml`（+13） | 账号轮换 UI / 加急门槛 CheckBox | 中 |
| `src/MaaWpfGui/ViewModels/UI/CopilotViewModel.cs`（+50）/ `RootViewModel.cs`（+2）/ `TaskItemViewModel.cs`（+4，标记 `fix/account_rotation/6`） | 轮换相关少量改动 | 低 |
| `src/MaaWpfGui/Main/Bootstrapper.cs`（+44） | `maaDlls` 白名单 36 个 NuGet DLL | 低 |
| `src/MaaWpfGui/MaaWpfGui.csproj` | fork 版本字段 | 低 |

### §6.5.6 构建 / 文档侧（fork 特有）

- `tools/local-install*.bat`（staging 版）、`tools/gen-downstream-changes.py`（fork 独创，从 `LOG.md` 生成 `docs/downstream-changes.md`）、`tools/add_maa_to_nahimic_whitelist.ps1`、`tools/disable_nahimic.ps1` — 上游无同名文件，merge 一般无冲突
- `AGENTS.md` / `LOG.md` / `WORKFLOW.md` / `CHANGELOG.md` / `docs/downstream-changes.md` — fork 全局跟踪，merge 后重跑 `py tools/gen-downstream-changes.py`
- `docs/{5 语}/protocol/integration.md`（各 +5）：`expedite_min_level` 字段说明——上游改 integration.md 时必然冲突，手解保留 fork 段落
- `.gitmodules`：fork 保持 **4 项**（上游 6 项：`test`、`src/MAAUnified` 无 gitlink 已删）；merge 时以上游为准但**不要找回已删的 2 项**

### §6.5.7 已回退历史功能（代码中已无实际，merge 时勿复活）

> 8/7 `c951f239c1` 已把 fork 私有 C++ 全部回归 master，`resource` 侧也已回归。以下功能只存在于历史（LOG.md + 远端分支），**merge 时这些文件取上游即可，无需手解保留**：

| 已回退功能 | 原位置 | 现状 |
|------|------|------|
| fix/account-switch-retry + 模板兜底 | `AccountSwitchTask.cpp` + `tasks.json` + `AccountManagerPageConfirm.png` | 全量回归 master（`c34403ac94` 删任务；模板已删） |
| fix/startup OCR 熔断 / 先登录再切号（切号链 OCR 兜底） | `StartUpTask.cpp` | 回归 master |
| fix/reception-clue-vacancy（线索填坑） | `InfrastReceptionTask.cpp` | 回归 master |
| feat/auto-recruit-3star-to-4star | C++/WPF/资源 | 2026-08-06 删除 |
| feat/recruit-result-display（招募结果识别）+ history-tab | C++/WPF/资源/tools | 2026-08-02 回退 |

merge 时若上游动到这些区域，**直接取 theirs（upstream 版本）**，不要手解回 fork 逻辑。

### §6.5.8 merge 后一键验证（放 §8 编译前后各跑一次）

```powershell
# —— 账号轮换 ——
git grep -c "fix/account-cycle-start-race" src/MaaWpfGui/          # 预期 ≥ 1
git grep -c "AdvanceAccountCycle" src/MaaWpfGui/                    # 预期 ≥ 3
git grep -c "fix/account_rotation" src/MaaWpfGui/                   # 预期 ≥ 5
# —— 公招加急 ——
git grep -c "expedite_min_level" src/MaaCore/                       # 预期 ≥ 8
git grep -c "fix/expedite-threshold\|fix/recruit-expedite-slot-target" src/MaaCore/  # 预期 ≥ 2
git grep -c "RecruitNow@Slot" resource/tasks/tasks.json             # 预期 = 4
（注意：tasks.json 的 account-switch 区块应保持 master 原样）
# —— Roguelike ——
git grep -n '"onErrorNext": \["Roguelike@ExitThenAbandon"\]' resource/tasks/Roguelike/base.json     # 预期命中 fork 加固行
git grep -n '"onErrorNext": \["JieGarden@Roguelike@ExitThenAbandon"\]' resource/tasks/Roguelike/JieGarden.json  # 预期命中
```

全部命中才算 merge 完整；任何一项为 0 → 回查对应区块手解遗漏。

### §6.6 提交

```powershell
git add -u  # 或单独 git add <resolved-files>
git commit -m "merge: upstream/master-v2 (vX.Y.Z) → staging

拉取上游 v6.X.0 ~ vX.Y.Z 共 N commit
解决冲突文件：~20 个
fork-specific 保留：feat/... / fix/...
fork-specific 删除：feat/...（理由）"
```

**完成标志**：`git status -s` 为空，最后 commit 是 merge commit。

---

## §7 转 git replace（1 min）

> graft 仅本地生效，push 到 Github 后下游无法识别。转 `git replace` 可保留假历史关联。

```powershell
# 7.1 安装 replace ref
$forkRoot = "c8c8e75be5227d0fcc8d1ebe9fdbc462055cdfce"
$upstreamAnchor = "6147357bd074d926047d1e391ba3759f40477ef1"
git replace --graft $forkRoot $upstreamAnchor

# 7.2 删除 graft 文件
Remove-Item ".git/info/grafts" -Force

# 7.3 关闭 deprecation 警告
git config --local advice.graftFileDeprecated false

# 7.4 验证
git merge-base HEAD upstream/master-v2
# 预期: c8c8e75be5227d0fcc8d1ebe9fdbc462055cdfce

git replace --list
# 预期: c8c8e75be5227d0fcc8d1ebe9fdbc462055cdfce

git cat-file -p refs/replace/c8c8e75be5227d0fcc8d1ebe9fdbc462055cdfce | Select-Object -First 3
# 预期:
#   tree 3b29ddda78ecb6516c1e057eb2e2ec1ae32e5f60
#   parent 6147357bd074d926047d1e391ba3759f40477ef1
#   ...
```

---

## §8 编译验证（10 min）

```powershell
# 8.1 C++ 配置 + 构建
cmake --preset windows-publish-x64
# 预期: "Build files have been written to: .../build"

cmake --build build --target MaaCore --config RelWithDebInfo --parallel
# 预期: "MaaCore.vcxproj -> ...MaaCore.dll"

cmake --build build --target MAA.Updater --config RelWithDebInfo
# 预期: "MAA.Updater.vcxproj -> ...MAA.Updater.exe"

cmake --install build --prefix install-staging
# 预期: install-staging/ 包含 MaaCore.dll, MAA.dll, MAA.exe, MAA.Updater.exe

# 8.2 WPF 构建
dotnet restore src/MaaWpfGui/MaaWpfGui.csproj
dotnet publish src/MaaWpfGui/MaaWpfGui.csproj -c Release -r win-x64 `
    -o install-staging /p:DisableBeauty=True
# 预期: "MaaWpfGui -> install-staging/"
# 注意: 可能存在 StyleCop 警告（不阻断）
```

### §8.3 启动验证

```powershell
# 启动 MAA.exe（应无 "未知 DLL" 错误对话框）
Start-Process install-staging/MAA.exe -Wait
```

### §8.4 DLL 白名单扩展（如出现错误）

**错误对话框完整列表**（2026-08-06 实测，35 个）：

```
BouncyCastle.Cryptography.dll
CalcBinding.dll
DynamicExpresso.Core.dll
GlobalHotKey.dll
GongSolutions.WPF.DragDrop.dll
HandyControl.dll
HtmlAgilityPack.dll
ICSharpCode.AvalonEdit.dll
JetBrains.Annotations.dll
MailKit.dll
MdXaml.dll
MdXaml.Html.dll
MdXaml.Plugins.dll
Microsoft.Extensions.Primitives.dll
Microsoft.Toolkit.Uwp.Notifications.dll
Microsoft.Windows.SDK.NET.dll
Microsoft.WindowsAPICodePack.Core.dll
Microsoft.WindowsAPICodePack.dll
Microsoft.WindowsAPICodePack.ExtendedLinguisticServices.dll
Microsoft.WindowsAPICodePack.Sensors.dll
Microsoft.WindowsAPICodePack.Shell.dll
Microsoft.WindowsAPICodePack.ShellExtensions.dll
Microsoft.Xaml.Behaviors.dll
MimeKit.dll
Newtonsoft.Json.dll
Notification.Wpf.dll
ObservableCollections.dll
Semver.dll
Serilog.dll
Serilog.Enrichers.Thread.dll
Serilog.Sinks.Debug.dll
Serilog.Sinks.File.dll
Stylet.dll
System.Management.dll
WinRT.Runtime.dll
```

**修复**（编辑 `src/MaaWpfGui/Main/Bootstrapper.cs` 第 86-96 行 maaDlls 列表）：

```csharp
var maaDlls = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
{
    // .NET runtime
    "hostfxr.dll", "hostpolicy.dll",
    // MAA native
    "libloader.dll", "DirectML.dll", "fastdeploy_ppocr_maa.dll",
    "MaaCore.dll", "MaaUtils.dll", "onnxruntime_maa.dll",
    "opencv_world4_maa.dll",
    // NuGet dependencies（完整 36 项）
    "BouncyCastle.Cryptography.dll", "CalcBinding.dll",
    "DynamicExpresso.Core.dll", "GlobalHotKey.dll",
    "GongSolutions.WPF.DragDrop.dll", "HandyControl.dll",
    "HtmlAgilityPack.dll", "ICSharpCode.AvalonEdit.dll",
    "JetBrains.Annotations.dll", "MailKit.dll",
    "MdXaml.dll", "MdXaml.Html.dll", "MdXaml.Plugins.dll",
    "Microsoft.Extensions.Primitives.dll",
    "Microsoft.Toolkit.Uwp.Notifications.dll",
    "Microsoft.Windows.SDK.NET.dll",
    "Microsoft.WindowsAPICodePack.Core.dll",
    "Microsoft.WindowsAPICodePack.dll",
    "Microsoft.WindowsAPICodePack.ExtendedLinguisticServices.dll",
    "Microsoft.WindowsAPICodePack.Sensors.dll",
    "Microsoft.WindowsAPICodePack.Shell.dll",
    "Microsoft.WindowsAPICodePack.ShellExtensions.dll",
    "Microsoft.Xaml.Behaviors.dll", "MimeKit.dll",
    "Newtonsoft.Json.dll", "Notification.Wpf.dll",
    "ObservableCollections.dll", "Semver.dll",
    "Serilog.dll", "Serilog.Enrichers.Thread.dll",
    "Serilog.Sinks.Debug.dll", "Serilog.Sinks.File.dll",
    "Stylet.dll", "System.Management.dll", "WinRT.Runtime.dll",
};
```

**验证**：

```powershell
$dlls = Get-ChildItem install-staging/*.dll | Select-Object -ExpandProperty Name
$whitelist = <上文的 44 项>
$unknown = $dlls | Where-Object { $whitelist -notcontains $_ -and $_ -notlike "*maa*" }
Write-Host "Unknown DLL count: $($unknown.Count)"  # 预期: 0
```

---

## §9 错误速查 FAQ

### 错误 1: refusing to merge unrelated histories

**症状**：

```
fatal: refusing to merge unrelated histories
```

**诊断**：

```powershell
git merge-base HEAD upstream/master-v2
# 若输出为空 → 历史无关联
```

**修复**：执行 §5 graft 流程。

### 错误 2: 冲突数量爆炸（>100）

**症状**：

```powershell
git diff --name-only --diff-filter=U | Measure-Object
# Count: 249
```

**原因**：未做 §5 graft，merge-base 为空，git 把所有文件当作 add/add。

**修复**：回滚 merge（`git merge --abort`），执行 §5 graft 后重试。

### 错误 3: CS0234 命名空间不存在

**症状**：

```
src/MaaWpfGui/ViewModels/UI/TaskQueueViewModel.cs(41,26): error CS0234:
命名空间"MaaWpfGui.Services"中不存在类型或命名空间名"Notification"
```

**诊断**：

```powershell
git grep "<OldNamespace>" src/MaaWpfGui/
# 找到 using 语句行号
```

**修复**：编辑替换为新命名空间（参考 §6.5 范例 2）。

### 错误 4: 启动报"未知 DLL"

**症状**：MAA.exe 弹窗列出 30+ DLL。

**修复**：参考 §8.4 扩展 maaDlls 白名单。

### 错误 5: 误在 master 编辑

**症状**：

```powershell
git status
# On branch master
# Changes not staged for commit:
#   modified: ...
```

**修复**：

```powershell
git stash push -u -m "phase-X-temp"
git switch staging
git stash pop
# 若有冲突:
git checkout --theirs <conflict-files>
# 重新应用丢失的编辑（如有）
```

### 错误 6: stash pop 后部分编辑丢失

**原因**：stash 自动合并成功但结果未包含所有更改。

**诊断**：

```powershell
git status -s  # 检查修改
```

**修复**：手动重新应用丢失的编辑（参考 §6.5 手解流程）。

### 错误 7: git 警告 "Support for grafts is deprecated"

**症状**：

```
hint: Support for <GIT_DIR>/info/grafts is deprecated
```

**修复**：

```powershell
git replace --convert-graft-file
git config --local advice.graftFileDeprecated false
```

---

## §10 完整端到端 PowerShell 脚本

```powershell
# ============================================================
# WORKFLOW.md - 上游拉取新功能完整脚本
# 用法：在仓库根目录运行此脚本
# 前提：master 已 reset 到 upstream/master-v2
# ============================================================

$ErrorActionPreference = "Stop"
Set-Location (Split-Path -Parent $PSCommandPath)

# --- §0 前置 ---
Write-Host "=== §0 前置检查 ===" -ForegroundColor Cyan
$upstreamUrl = git remote get-url upstream 2>$null
if (-not $upstreamUrl) {
    Write-Error "upstream remote 未配置"; exit 1
}

# --- §1 预检 ---
Write-Host "`n=== §1 预检 ===" -ForegroundColor Cyan
$date = Get-Date -Format "yyyyMMdd"
git branch "backup/staging-pre-merge-$date" staging
git fetch upstream | Out-Null

$forkCount = (git log master..staging --oneline).Count
$upstreamCount = (git log staging..upstream/master-v2 --oneline).Count
Write-Host "Fork commits: $forkCount, Upstream new: $upstreamCount"

# --- §2 评估 ---
Write-Host "`n=== §2 评估冲突 ===" -ForegroundColor Cyan
$forkFiles = git diff master..staging --name-only --diff-filter=ACD |
    Where-Object { $_ -notmatch '^resource/' } | Select-Object -Unique
$upstreamFiles = git log staging..upstream/master-v2 --no-merges --name-only --format="" |
    Where-Object { $_ -ne "" -and $_ -notmatch '^resource/' } | Select-Object -Unique
$overlap = $forkFiles | Where-Object { $upstreamFiles -contains $_ }
Write-Host "潜在冲突文件: $($overlap.Count)"

# --- §5 graft（关键） ---
Write-Host "`n=== §5 graft ===" -ForegroundColor Cyan
$forkRoot = "c8c8e75be5227d0fcc8d1ebe9fdbc462055cdfce"
$upstreamAnchor = "6147357bd074d926047d1e391ba3759f40477ef1"
$forkRootSha = git rev-parse $forkRoot 2>$null
if (-not $forkRootSha) {
    Write-Warning "Fork root $forkRoot 不存在，本脚本仅适用于当前 fork 结构"
} else {
    "$forkRootSha $upstreamAnchor" | Out-File ".git/info/grafts" -Encoding utf8 -NoNewline
    $mergeBase = git merge-base HEAD upstream/master-v2
    if ($mergeBase -eq $forkRootSha) {
        Write-Host "✅ graft 设置成功" -ForegroundColor Green
    } else {
        Write-Warning "⚠️ graft 验证失败，请检查 .git/info/grafts"
    }
}

# --- §6 merge ---
Write-Host "`n=== §6 合并 upstream ===" -ForegroundColor Cyan
git switch staging | Out-Null
git merge --no-commit --no-ff upstream/master-v2 2>&1 | Out-Null

$conflictFiles = git diff --name-only --diff-filter=U
Write-Host "冲突文件数: $($conflictFiles.Count)"

if ($conflictFiles.Count -gt 100) {
    Write-Error "冲突过多（$($conflictFiles.Count)），可能未生效 graft。请中止并检查。`n执行: git merge --abort"
    git merge --abort
    exit 1
}

# 此时需要手动解决冲突
Write-Host "`n请手动解决以下冲突文件:" -ForegroundColor Yellow
$conflictFiles | ForEach-Object { Write-Host "  - $_" }
Write-Host "`n完成后继续执行：git add -u && git commit -m 'merge: upstream/master-v2 ...'" -ForegroundColor Yellow
```

---

## §11 验证清单 + 回滚策略

### §11.1 完成验证清单

```
□ git log master..staging --oneline | head -20（可见 fork commits）
□ git status -s（应 clean）
□ install-staging/MAA.exe 启动无错误对话框
□ git config advice.graftFileDeprecated = false
□ git replace --list 包含 fork_root
□ git merge-base HEAD upstream/master-v2 返回 fork_root
□ 所有 DLL 在 maaDlls 白名单内（§8.4 验证）
```

### §11.2 回滚策略

每个 Phase 单独 commit，回滚按以下优先级：

| 阶段 | 回滚命令 | 影响 |
|------|----------|------|
| §3（删功能） | `git revert -n HEAD && git commit` 或 `git reset --hard HEAD~1` | 仅回滚删功能 commit |
| §4（文档） | `git reset --hard HEAD~1` | 仅回滚文档 commit |
| §6（merge） | `git merge --abort`（若未 commit）<br>`git reset --hard HEAD~1`（若已 commit） | 回滚 merge commit，工作区回到 merge 前 |
| §8（编译修复） | `git reset --hard HEAD~1` 或单独 revert | 回滚 build fix commit |

**注意**：若 §7 已执行（git replace），回滚 merge 后仍需保留 replace ref（不影响后续工作）。

---

## §12 相关文件索引

| 文件 | 作用 |
|------|------|
| `AGENTS.md` §2.4 | staging 工作流约定 |
| `LOG.md` | 文件修改流水 |
| `tools/local-install*.bat` | 本地构建脚本 |
| `tools/local-install-staging.bat` | staging 专用构建脚本 |
| `src/MaaWpfGui/Main/Bootstrapper.cs` | maaDlls 白名单（核心安全检查） |
| `.git/info/grafts`（已弃用） | 假历史关联（建议转 git replace） |
| `.git/config` `advice.graftFileDeprecated` | 抑制 deprecation 警告 |