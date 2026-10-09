# v6.18.0-beta.1 一键合入计划书（压缩版）

> **执行日期**：2026-09-16  
> **路径**：A（完整同步 v6.17.0 → v6.18.0-beta.1，含 v6.17.1~v6.17.5 patch）  
> **规模**：170 commit / 498 文件 / 112 新增 / 30-50 冲突（预估）  
> **预计耗时**：4-6h  
> **基线**：staging @ 0eb6cba26e / master @ c5251e220b（v6.17.0）/ upstream @ ec14df252ae（v6.18.0-beta.1）

---

## 0. 当前状态快照

```
HEAD = staging @ 0eb6cba26e
master = c5251e220b (v6.17.0, 9天陈旧)
upstream/master-v2 = ec14df252ae (v6.18.0-beta.1)
merge-base = c5251e220b
git replace --list = c8c8e75be5 → 6147357bd0 (graft 有效)
staging 私有 = 229 commit / upstream 新增 = 170 commit
工作树 = 干净
feat/* 分支现状:
  feat/upstream-v617-sync   → 0 ahead (已合)
  feat/sync-isolation       → 0 ahead (已合)
  feat/fork-externalization → 1 ahead (e079795071 仅 AGENTS/LOG)
tools/ 工具齐全:
  - auto-resolve-xaml-conflict.py ✓
  - inject-recruitnow-slot.py    ✓
  - post-merge-validate.ps1      ✓
  - local-install-staging.bat    ✓
  - gen-downstream-changes.py    ✓
NetBeauty2 = 2.1.5 ✓
.agents/skills/ = ✓ (fork 私有)
```

---

## 1. 关键决策

| # | 决策 | 备注 |
|---|------|------|
| D1 | v6.18.0-beta.1 入 staging 不入 branch | 等 stable 再晋升 |
| D2 | cherry-pick `e079795071` 到 staging | 1 commit，AGENTS/LOG 补 §2.4/§7.14/§7.15/§7.16/§7.17 + §6 清空 + LOG 2026-08-30 |
| D3 | `master` reset 到 upstream/master-v2 | 同步基线 |
| D4 | `feat/upstream-v617-sync` 阶段 0 删 | 远端保留 |
| D5 | `feat/sync-isolation` 阶段 0 删 | 远端保留 |
| D6 | `feat/fork-externalization` 阶段 0 cherry-pick 后删 | 远端保留 |
| D7 | `new-branch` 阶段 0 `git branch -f` 同步 | 临时对比分支 |
| D8 | 不自动晋升 branch | 等实测 |

---

## 2. 冲突区速查（按风险从低到高）

### 2.1 LOW (auto-merge 0 冲突，~356 个)
- `.github/workflows/**` / `.gitignore` / `.editorconfig` / `.clang-format` / `.gitmodules`
- `resource/template/**` 5 语 PNG
- `resource/tasks/Roguelike/**` 数值
- `docs/{5 语}/**` 全部 vuepress（#18030 大修订）
- `tools/ClangFormatter/` / `OptimizeTemplates/` / `ImageCropper/` / `OverseasClients/` / `RoguelikeRelicsExtractor/` / `ResourceUpdater/` / `SyncTemplate/` / `TaskSorter/` / `MaaWpfGui.Benchmarks/` / `maadeps-download.py` / `maafw-control-unit-download.py`
- `README.md`

### 2.2 MEDIUM (wholesale 接 upstream，需手解)

| 文件 | 操作 | 验证 |
|------|------|------|
| `src/MaaCore/Task/Infrast/InfrastReceptionTask.cpp` | wholesale + grep §7.15 4-point | vacancy_cnt==0 / click_performed / OCR parse failed 各 ≥1 |
| `src/MaaCore/Task/ProcessTask.{h,cpp}` | wholesale | RecruitNow 不依赖 Swipe，RecruitNow@Slot0..3 走 inject 脚本 |
| `src/MaaCore/Task/Infrast/InfrastProcessingTask.cpp` | wholesale | 新 AssistantChange 不影响换班主流程 |
| `src/MaaCore/Controller/SwipeHelper.hpp` (#18112) | wholesale | tasks.json specialParams[1] slope÷10 后续由脚本注入 |
| `src/MaaCore/Common/AsstTypes.h` + `AsstInfrastDef.h` + `AsstMsg.h` | wholesale | FacilityStep::AssistantChange 字段 |
| `src/MaaWpfGui/States/RunningState.cs` | wholesale + RunOwner | _timerLock NRE 修复 + ctor RunOwner |
| `src/MaaWpfGui/Main/Bootstrapper.cs` | wholesale + 核 maaDlls | maaDlls ≥47 项 |
| `src/MaaWpfGui/Configuration/Single/Settings/{Gui,RuntimeSettings,ThirdParty}.cs` | wholesale | HideHoverActionButtons + RunDeadlineMinutes + ThirdParty.PenguinId/EnablePenguin/EnableYituliu/YituliuOpenApiToken |
| `src/MaaWpfGui/Configuration/Single/MaaTask/StartUpTask.cs` | wholesale + 检 partial | AccountName/AccountSwitchEnabled 兼容字段 |
| `src/MaaWpfGui/Configuration/Single/MaaTask/RecruitTask.cs` | wholesale + 检 partial | ExpediteMinLevel 计算属性 + UseExpedited upstream 字段 |
| `src/MaaWpfGui/ViewModels/UI/TaskItemViewModel.cs` | wholesale | §7.7 4 行不冲突 |
| `src/MaaWpfGui/Main/AsstProxy.cs` | wholesale + **L1397 extension hook 保留** | OnAllTasksCompleted + 14 行 fork 加固 |
| `src/MaaWpfGui/Services/PendingUpdateApplier.cs` | wholesale | TryConsumeDelegatedUpdateFailure 拆 4 API |
| `src/MaaWpfGui/Models/OperBoxData.cs` | wholesale | +Skills/Equips 字段 |
| `src/MaaWpfGui/Views/UserControl/GuideUserControl.xaml` | wholesale | 913/-563 重构 |
| `src/MaaWpfGui/Views/UserControl/Settings/IssueReportUserControl.xaml` | wholesale + 核 fork §7.17 | 19 key + 布局保留 |
| `src/MaaWpfGui/Views/UserControl/TaskQueue/StartUpTaskUserControl.xaml` | wholesale + 核 fork §7.16 | 删除单账号 section + ComboBox + AddButton |
| `src/MaaWpfGui/ViewModels/UserControl/TaskQueue/StartUpSettingsUserControlModel.cs` | wholesale + 核 fork §7.16 | 删 #region Account Switch (Single) |
| `src/MaaWpfGui/ViewModels/UserControl/TaskQueue/RecruitSettingsUserControlModel.cs` | wholesale + 检 partial | Expedite partial 保留 |

### 2.3 HIGH (手解冲突，~20 个)

| 文件 | 上游变更 | fork 私有 | 解决 |
|------|---------|----------|------|
| `src/MaaWpfGui/ViewModels/UI/TaskQueueViewModel.cs` | CheckAfterCompleted 拆 3 方法 + Stop() 改 Task<bool> + Idle/Inited/Stopping→Run.* | OnAllTasksCompleted hook (L1397) + AccountCycle.cs partial | 接 upstream + 在新 RunStopScriptOnceAsync 之前保留 fork hook |
| `src/MaaWpfGui/ViewModels/UI/ToolboxViewModel.cs` | 删 Idle/Inited/Stopping + 加 Run.* | AccountScopedData.cs partial (43-222行) | 接 upstream + 39 处 fork 引用迁移 Run.* |
| `src/MaaWpfGui/ViewModels/UI/CopilotViewModel.cs` | 删 Idle/Inited/Stopping + Stop() 签名 | CopilotSet.cs partial + ClipboardLinkSet20Regular | 接 upstream + fork 引用迁移 + partial 保留 |
| `src/MaaWpfGui/Res/Localizations/{zh-cn,zh-tw,en-us,ja-jp,ko-kr}.xaml` | 5 语 +462~507/-37~45 | 41 fork key | 跑 auto-resolve + 手工校验 41 key + 补 ~30 upstream 新 key |
| `src/MaaWpfGui/Views/UI/TaskQueueView.xaml` | +446/-158 Idle or !Inited→Run.* | CurrentAccountLabel Border (L95-107) | 接 upstream + 保留 fork Border |
| `src/MaaWpfGui/Configuration/Single/MaaTask/RecruitTask.cs` + `RecruitTask.Expedite.cs` | upstream 字段微调 | ExpediteMinLevel + ExpediteMode 计算属性 | 接 upstream + 保留 fork partial |
| `src/MaaWpfGui/Res/Styles/Basic/Geometries.xaml` | upstream 调整 | ClipboardLinkSet20Regular | 接 upstream + 检 §7.13 Geometry |
| `resource/tasks/tasks.json` | Swipe specialParams[1] 0/1→0/1/2/3/4 + slope×10 + CharsNameOcrReplace | RecruitNow@Slot0..3 (L12014-12049) | **关键**：阶段 3 --remove → merge → 阶段 6 --inject |
| `src/MaaWpfGui/Views/UserControl/Settings/IssueReportUserControl.xaml` | tooltip 改 | fork §7.17 19 key + 布局 | 接 upstream + fork 保留 |

### 2.4 fork 私有文件清单（不在冲突区，接 upstream 时**绝对不能动**）

```
src/MaaCore/Task/Miscellaneous/AutoRecruitTask.{h,cpp}  # §7.11 expedite_min_level + slot_index_from_rect
src/MaaWpfGui/ViewModels/UI/TaskQueueViewModel.AccountCycle.cs
src/MaaWpfGui/ViewModels/UI/ToolboxViewModel.AccountScopedData.cs
src/MaaWpfGui/ViewModels/UI/CopilotViewModel.CopilotSet.cs
src/MaaWpfGui/Configuration/Single/MaaTask/RecruitTask.Expedite.cs
src/MaaWpfGui/ViewModels/UserControl/TaskQueue/RecruitSettingsUserControlModel.Expedite.cs
src/MaaWpfGui/ViewModels/UserControl/TaskQueue/StartUpSettingsUserControlModel.AccountCycle.cs
src/MaaWpfGui/Models/AccountCycleItem.cs + AccountCycleStep.cs + AccountCycleOrchestrator.cs
src/MaaWpfGui/Models/DiagnosticInfo.cs
src/MaaWpfGui/Res/Styles/Basic/Geometries.xaml  # ClipboardLinkSet20Regular
```

---

## 3. 14 批手解顺序（5.A → 5.N）

```
5.A  .gitignore / .gitmodules / .editorconfig / .clang-format → wholesale
5.B  wholesale 新增文件（30+）→ auto-accept theirs
5.C  C++ wholesale（ProcessTask / Infrast / SwipeHelper / Types）
5.D  Bootstrapper.cs wholesale + 核 maaDlls 白名单
5.E  5 语 xaml × 5: auto-resolve + 手工校验 41 fork key + 补 30+ upstream 新 key
5.F  WPF wholesale（TaskItemViewModel / OperBoxData / PendingUpdateApplier / 配置 Single/*）
5.G  ToolboxViewModel.cs + AccountScopedData.cs partial + 39 处 Run.* 迁移
5.H  RunningState.cs + RunControlState.cs + RunOwner.cs
5.I  AsstProxy.cs 保留 OnAllTasksCompleted extension hook (L1397)
5.J  CopilotViewModel.cs + CopilotSet.cs partial
5.K  XAML: TaskQueueView / GuideUserControl / IssueReportUserControl
5.L  RecruitTask.cs + RecruitTask.Expedite.cs + RecruitSettingsUserControlModel.Expedite.cs
5.M  tasks.json: wholesale 接 upstream + 跑 inject-recruitnow-slot.py --inject
5.N  docs/{5 语}/ wholesale + 保留 docs/downstream-changes.md
```

---

## 4. 阶段 0 → 11 执行脚本（cheat sheet）

```bash
# 阶段 0：仓库卫生
git branch backup/staging-pre-v618-sync-20260916 staging
git branch backup/master-pre-v618-sync-20260916 master
git switch master
git fetch upstream
git reset --hard upstream/master-v2
git branch -f new-branch master
git switch staging
git cherry-pick e079795071            # feat/fork-externalization 唯一未合 commit
git branch -D feat/fork-externalization
git branch -d feat/sync-isolation
git branch -d feat/upstream-v617-sync
# 手动改 AGENTS.md §6 / §2.4 / §7.19/§7.20/§7.21
# 手动改 CHANGELOG.md 加 v6.18 段占位
# 手动改 LOG.md 加 2026-09-16 章节
python tools/gen-downstream-changes.py
git add -A && git commit -m "docs(cleanup): v6.18 同步前仓库卫生..."
git push Github staging

# 阶段 1：WORKFLOW 准备
git replace --list                     # 验证 c8c8e75be5 → 6147357bd0
git merge-base HEAD upstream/master-v2 # = c5251e220b
# §2.5 fork marker 12 项 grep 核对

# 阶段 2：试合并
git merge --no-commit --no-ff upstream/master-v2
git diff --name-only --diff-filter=U   # 冲突清单

# 阶段 3：预操作（删 RecruitNow@Slot 4 变体）
python tools/inject-recruitnow-slot.py --remove
git add resource/tasks/tasks.json
git commit -m "chore(pre-merge): 移除 fork RecruitNow@Slot* 准备合入"

# 阶段 4：重新合并
git merge --continue                   # 或 git merge --abort + 重来

# 阶段 5：14 批手解
# 5.A → 5.N 顺序，每批 git add + 进度报告

# 阶段 6：commit merge
git add -A
git commit -m "merge: upstream/master-v2 (v6.18.0-beta.1) → staging"

# 阶段 6.5：post-merge 注回
python tools/inject-recruitnow-slot.py --inject
git add resource/tasks/tasks.json
git commit -m "fix(recruit-expedite): 注回 fork RecruitNow@Slot0..3 (post-merge)"
git replace --list                    # 验证 c8c8e75be5 仍在
git replace c8c8e75be5227d0fcc8d1ebe9fdbc462055cdfce 6147357bd06c4cecd60cfe38e1c66cd83f43d8fa

# 阶段 7：编译
$env:MSBuildSDKsPath = "C:\Program Files\dotnet\sdk\10.0.300\Sdks"
cmake --preset windows-publish-x64
cmake --build build --target MaaCore --config RelWithDebInfo --parallel
Stop-Process MAA -Force -EA SilentlyContinue
cmake --install build --prefix install-staging
dotnet publish src/MaaWpfGui/MaaWpfGui.csproj -c Release -r win-x64 -o install-staging /p:DisableBeauty=True
$nbeauty = "$env:USERPROFILE\.nuget\packages\nulastudio.netbeauty\2.1.5\tools\win-x64\nbeauty2.exe"
& $nbeauty --usepatch "$PWD\install-staging\." "./externals"
Select-String -Path install-staging/MAA.runtimeconfig.json -Pattern STARTUP_HOOKS

# 阶段 8：启动 + 冒烟
Start-Process install-staging/MAA.exe
# 8s 主窗口 + 8 项冒烟

# 阶段 9：fork marker 验证
powershell -ExecutionPolicy Bypass -File tools/post-merge-validate.ps1

# 阶段 10：文档收尾
# AGENTS §2.4 末尾追加 v6.18 晋升占位
# AGENTS §7.22 加 feat/upstream-v618-sync
# CHANGELOG 加 v6.18.0-beta.1-fork 段
# LOG 加 2026-09-16 阶段 0-10 记录
python tools/gen-downstream-changes.py
git add -A && git commit -m "docs(post-merge): v6.18.0-beta.1 合入收尾..."
git push Github staging
```

---

## 5. fork 标记验证（阶段 9 必跑）

```powershell
# 1. replace
git replace --list                                                       # 1 行 c8c8e75be5

# 2. expedite C++
git grep -c expedite_min_level src/MaaCore/                              # ≥ 8
git grep -c slot_index_from_rect src/MaaCore/                            # ≥ 1

# 3. §7.15 4-point fix
git grep -c 'vacancy_cnt == 0\|available != vacancy_cnt\|click_performed\|OCR parse failed' src/MaaCore/  # 各 ≥ 1

# 4. tasks.json RecruitNow@Slot
git grep -c RecruitNow@Slot resource/tasks/tasks.json                     # = 4

# 5. AccountManagerPageConfirm 应已不存在
git grep -c AccountManagerPageConfirm resource/                          # = 0

# 6. WPF AccountCycle
git grep -c AdvanceAccountCycle src/MaaWpfGui/                            # ≥ 3
git grep -c "AsstStart() || .*AsstRunning()" src/MaaWpfGui/               # ≥ 1
git grep -c SwitchDataAccount src/MaaWpfGui/                              # ≥ 3
git grep -c ExpediteMinLevel src/MaaWpfGui/                               # ≥ 5
git grep -c PasteClipboardCopilotSet src/MaaWpfGui/                       # ≥ 1

# 7. Geometries
git grep -c ClipboardLinkSet20Regular src/MaaWpfGui/Res/Styles/Basic/Geometries.xaml  # = 1

# 8. NetBeauty runtimeconfig
Select-String -Path install-staging/MAA.runtimeconfig.json -Pattern STARTUP_HOOKS

# 9. maaDlls 白名单
git grep -c "GpuDll\|SkiaSharp\|Easylogging\|libloader\|MaaFramework" src/MaaWpfGui/Main/Bootstrapper.cs  # ≥ 10

# 10. .gitmodules 4 项
git config -f .gitmodules --get-regexp path                               # 4 行
```

---

## 6. 回滚预案

| 阶段 | 回滚命令 |
|------|----------|
| 0-1 | `git checkout master && git reset --hard backup/master-pre-v618-sync-20260916` |
| 2-4 (merge 中) | `git merge --abort` |
| 4+ (merge 完成) | `git reset --hard backup/staging-pre-v618-sync-20260916` |
| 7 (编译失败) | `Stop-Process MAA -Force; rm -rf install-staging/*; git reset --hard HEAD~1` |
| 8-9 (运行异常) | 记录 + 切回 backup 分支 |

---

## 7. 时间预算

| 阶段 | 预计 | 累计 |
|------|------|------|
| 0 仓库卫生 | 30 min | 30 |
| 1 准备 | 5 min | 35 |
| 2-3 预操作 | 5 min | 40 |
| 4 试合并 | 5 min | 45 |
| 5 手解 14 批 | 120-180 min | 165-225 |
| 6 commit + 注回 | 5 min | 170-230 |
| 7 编译 | 30-60 min | 200-290 |
| 8 冒烟 | 15 min | 215-305 |
| 9 验证 | 5 min | 220-310 |
| 10 文档 | 10 min | 230-320 |

**总：3.8-5.3h**（下限：熟练无意外；上限：遇 1-2 处需查上游）

---

## 8. 必备工具确认

- [x] `tools/auto-resolve-xaml-conflict.py` 5-lang xaml 自动处理
- [x] `tools/inject-recruitnow-slot.py` tasks.json Slot 4 变体注回
- [x] `tools/post-merge-validate.ps1` 7 项验证
- [x] `tools/local-install-staging.bat` 一键部署
- [x] `tools/gen-downstream-changes.py` 文档清单重生成
- [x] `%USERPROFILE%\.nuget\packages\nulastudio.netbeauty\2.1.5\tools\win-x64\nbeauty2.exe`
- [x] `.agents/skills/` 目录
- [x] `global.json` SDK 10.0.300 via rollForward
- [x] `CMakePresets.json` windows-publish-x64
- [x] `install-staging/` 已存在

---

## 9. 待办 checklist（执行前最终核对）

- [ ] git status -s 空
- [ ] 当前分支 staging
- [ ] HEAD = 0eb6cba26e
- [ ] backup/staging-pre-v618-sync-20260916 已建
- [ ] backup/master-pre-v618-sync-20260916 已建
- [ ] master 已 reset upstream/master-v2
- [ ] feat/fork-externalization 已 cherry-pick
- [ ] feat/* 三个本地分支已删
- [ ] AGENTS §6 / §2.4 / §7.19/§7.20/§7.21 已改
- [ ] CHANGELOG v6.18 段占位已加
- [ ] LOG.md 2026-09-16 已写
- [ ] docs/downstream-changes.md 已重生成
- [ ] 阶段 0 commit 已 push Github staging
- [ ] git fetch upstream 成功（170 commit）
- [ ] merge-base 正确
- [ ] 阶段 0 → 10 已完成

---

## 10. 文件创建/修改/删除清单

**新增 wholesale (~112 个)**：见上文 2.4 fork 私有 + SwitchThemeTask.* / MaterialSynthesisTaskPlugin.* / InfrastAssistantChangeTask.* / BlackFlow*.* / RunControlState.cs / RunOwner.cs / ThirdParty*.* / YituliuApiService.cs / SwitchThemeTaskUserControlModel.* / MaterialSynthesisTaskModel.* / SwipeHelper.hpp / InjectRecruitNowSlot.py / maa_core_eval.py / .agents/skills/core-image-eval/ / docs/{5 语}/switch-theme.md / resource/tasks/Roguelike/BlackFlow.json

**修改 wholesale (~20 个)**：见上文 2.2 MEDIUM + 2.3 HIGH

**修改 LOW (~356 个)**：资源 PNG + 5 语文档 + ci workflows + 其他工具

**绝对保留**：
- AutoRecruitTask.{h,cpp}
- 7 个 partial class（AccountCycle/AccountScopedData/CopilotSet/RecruitTask.Expedite/RecruitSettingsUserControlModel.Expedite/StartUpSettingsUserControlModel.AccountCycle）
- AccountCycleItem.cs / AccountCycleStep.cs / AccountCycleOrchestrator.cs
- DiagnosticInfo.cs
- Geometries.xaml ClipboardLinkSet20Regular
- 5-lang xaml 41 fork key
- tasks.json RecruitNow@Slot0..3
- InfrastReceptionTask.cpp §7.15 4-point fix
- Roguelike/base.json + JieGarden.json +onErrorNext
- .gitmodules 4 项
- tools/inject-recruitnow-slot.py

**删除**：无（fork 端不动；上游删除的 ReportToPenguin/PenguinId 字段由 ThirdPartyMigrationConverter 迁移处理）

---

## 11. commit message 模板

```bash
# 阶段 0
docs(cleanup): v6.18 同步前仓库卫生 — feat/* 本地删 + AGENTS/CHANGELOG/LOG 漂移修复

# 阶段 3
chore(pre-merge): 移除 fork RecruitNow@Slot* 准备合入 upstream/master-v2 (v6.18.0-beta.1)

# 阶段 6
merge: upstream/master-v2 (v6.18.0-beta.1) → staging
拉取上游 v6.17.0 ~ v6.18.0-beta.1 共 170 commit
解决冲突：~40 个（C++ 4 + WPF ~20 + 5-lang xaml 5 + tasks.json 1 + docs ~10）
新增 wholesale：~30（SwitchTheme/MaterialSynthesis/Yituliu/RunControl/RunOwner/ThirdParty*）
fork 私有保留：AccountCycle/AccountScopedData/ExpediteMinLevel/RecruitNow@Slot/§7.15 4-point fix/§7.18 depot
API 迁移：Idle/Inited/Stopping → Run.Idle/Run.Inited/Run.Stopping（39 处）

# 阶段 6.5
fix(recruit-expedite): 注回 fork RecruitNow@Slot0..3 (post-merge)

# 阶段 10
docs(post-merge): v6.18.0-beta.1 合入收尾 — AGENTS/CHANGELOG/LOG/downstream 二次刷新
```