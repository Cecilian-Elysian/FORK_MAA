# AGENTS

本文件面向在本仓库协作的 AI 代理与人类贡献者，约定工作流、文档规范与协作约束。

分支的存在性与生命周期记录在 `§6` 与 `§7`；本仓库**不使用** feat / fix 工作笔记（无 `feat_<name>.md` / `fix_<name>[_<n>].md`），所有变更通过 commit message 与 `LOG.md` 跟踪。

中文叙述为主，分支名 / 命令 / 协议字段等关键术语保留英文原文。无 emoji、无 vuepress 容器，遵循 `LOG.md` 既有的表格驱动风格。


## 1. 项目概述

### 1.1 一句话

基于 [MaaAssistantArknights/MaaAssistantArknights](https://github.com/MaaAssistantArknights/MaaAssistantArknights) 上游 `dev-v2` 的本地 fork，主仓 `branch` 累积本地下游增强。业务语义：基于 [MaaFramework](https://github.com/MaaXYZ/MaaFramework) 的图像识别明日方舟小助手，「一键完成全部日常任务」。

### 1.2 技术栈

| 层 | 语言 / 框架 | 位置 |
|----|------------|------|
| 核心引擎 | C++20 | `src/MaaCore/` |
| WPF GUI | C# / .NET 10 / WPF | `src/MaaWpfGui/` |
| 通用工具 | C++ | `src/MaaUtils/` (子模块) |
| Wine 桥 | C++ | `src/MaaWineBridge/` |
| 更新器 | C# | `src/MaaUpdater/` |
| 多语言绑定 | C / Python / Golang / Dart / Java / Rust / TypeScript / Woolang | `src/<Lang>/` |
| 图像识别 | OpenCV + PaddleOCR + FastDeploy + onnxruntime | `3rdparty/` |
| 资源数据 | JSON 任务流 + 模板图片 | `resource/` |

### 1.3 平台与许可

| 项 | 备注 |
|----|------|
| 平台 | Windows / Linux / macOS（含 Apple Silicon） |
| 协议 | [AGPL-3.0 only](https://spdx.org/licenses/AGPL-3.0-only.html) |
| 附加协议 | `terms-of-service.md` |
| Logo 保留 | 耗毛、vie 画师及全体开发者保留所有权利，禁商业用途 |


## 2. 仓库拓扑与分支模型

### 2.1 远程

| Remote | 用途 |
|--------|------|
| `upstream` | 上游 `MaaAssistantArknights/MaaAssistantArknights`，`HEAD` 跟踪 `dev-v2` |
| `Github` | 个人 fork 远端，分支同步发布用 |
| `origin` | （未配置 / 备用） |

### 2.2 本地分支

| 分支 | 角色 | 备注 |
|------|------|------|
| `master` | 上游镜像 | 长期与 `upstream/dev-v2` 保持一致 |
| `branch` | **稳定下游基线** | 从 `staging` 攒批晋升 + 与 `master` 上游同步 |
| `staging` | **待验证整合区** | 所有 feat / fix 的合并目标；攒批测试通过后晋升至 `branch`（详见 `§2.4`） |
| `feat/<name>` | 新功能 | 从 `branch` 拉出，合并到 `staging` |
| `fix/<name>` / `fix/<name>/<n>` | 修复分支 | **必须从对应 `feat/<name>` 拉出**（详见 `§3.3`）；合并到 `staging` 或对应 feat |

### 2.3 已完结 feat 处理

feat 合并到 `staging` 后：

| 操作 | 说明 |
|------|------|
| 本地 | `git branch -d feat/<name>`（已合入，安全删除） |
| 远端 | 保留不删（便于回溯 / cherry-pick / 行为对比） |
| 记录 | 写入本文件 `§7` 与 `LOG.md` |

### 2.4 staging 工作流

`staging` 是 `branch` 与 feat / fix 之间的**待验证整合区**，所有 feat / fix 必经此缓冲层才能晋升到 `branch`。

#### 拓扑

```
master (上游 dev-v2 镜像)
  │  (rebase / merge 同步节奏不变)
  ▼
branch (稳定下游基线) ◄──── 用户手动合并 staging
  │                                 ▲
  │                                 │ (合并目标)
  ▼                                 │
staging ──── feat/<name>, fix/<name> ← 从 staging 拉出
```

#### 规则

| 项 | 说明 |
|----|------|
| 合并目标 | 所有 feat / fix 一律合并到 `staging`（不直奔 `branch`） |
| 拉取源 | feat / fix 一律从 `staging` 拉出（继承最新整合区；与原 `branch` 拉出约定对比，详见 §6 `feat/recruit-result-display` 行） |
| 晋升时机 | `branch` 晋升由用户手动触发；不再自动攒批晋升（变更日 2026-07-30） |
| 晋升方式 | `staging` → `branch` 由用户执行 `git merge staging --no-ff`；feat/fix → `staging` 自动 `--no-ff` |
| 出问题回退 | 从远端保留的 `feat/<name>` / `fix/<name>` 重新拉 `fix/<name>/<n>`，仍合并到 `staging` |

#### 当前待验证内容（截至 2026-07-25）

`staging` 由 `fix/expedite-threshold` 重命名而来，当前领先 `branch` 11 commits、落后 2 commits（branch 上的 `784d9005f6` + `da157d163d` 与 staging 上的 cherry-pick `6011051af2` + `f241b2160b` 内容等价、SHA 不同）。

| 主题 | commit 数 | SHA |
|------|-----------|-----|
| StartUp 启动链（双重缓冲清理 / run 重排 / 恢复原序 + OCR 兜底 / 官方服账号识别补全） | 4 | `eb3cc67595` `3f411e494a` `2715162c3d` `6011051af2` |
| Account-switch OCR 适配 UI 改版（登录记录 / 上次登录 双文本兜底） | 1 | `f9668a0c9c` |
| Expedite-threshold `m_last_confirmed_min_level` 重置补回 | 1 | `d73f61adc1` |
| Recruit_now 顺序（移到 confirm 之后） | 1 | `2718046060` |
| Docs（启动 / 实施完成登记 / cherry-pick 同步 / 字段说明） | 4 | `301f90897a` `20cd79d4ca` `f241b2160b` `1249c9a4dd` |

晋升前需至少实测验证：多账号切号（官服 + B 服）+ 公招加急（4 / 5 / 6 星门槛）+ recruit_now 主页立即完成路径。


## 3. 工作流与文档规范

### 3.1 跟踪文档策略

| 文档 | 是否跟踪 | 用途 |
|------|----------|------|
| `AGENTS.md` | **所有分支跟踪** | 协作约定 / 分支生命周期 |
| `LOG.md` | **所有分支跟踪** | 文件修改流水（日期 + 文件路径 + 操作 + 说明） |
| `docs/` | 跟踪 | 用户文档（vuepress 站点源） |
| `CHANGELOG.md` | 跟踪 | 版本发布日志 |

> **不使用 feat / fix 工作笔记**：所有设计决策与踩坑沉淀直接写入 commit message 与 `LOG.md` 表格。

### 3.2 启动新 feat 标准流程

| # | 步骤 | 产物 |
|---|------|------|
| 1 | `git switch -c feat/<name> staging` | 新分支 |
| 1.5 | 查阅 [`docs/downstream-changes.md`](./docs/downstream-changes.md)，确认本次改动文件不在清单「高敏感」段（多轮 feat/fix 反复动过的代码改动需特别谨慎）；如要改动清单中的文件，commit message 注明「downstream: 该文件曾被 feat/fix X 改动，本次改动原因」 | 防回归 |
| 2 | `LOG.md` 追加「`feat/<name>` 启动」表格 | 启动记录 |
| 3 | 实施期间 commit message 记录关键决策 | commit 历史 |
| 4 | 实施完成 `LOG.md` 追加「`feat/<name>` 实施完成」表格（文件路径 + 行号 + commit） | 实施记录 |
| 5 | 编译 / 部署验证 | `install-staging/` |
| 6 | `--no-ff` 合并到 `staging`（合并目标固定为 staging） | merge commit |
| 7 | `LOG.md` 记录合并事件，按 `§2.3` 处理 feat 分支 | 生命周期 |
| 8 | 合并后重跑 `py tools/gen-downstream-changes.py` 刷新 [`docs/downstream-changes.md`](./docs/downstream-changes.md) | 清单维护 |

> **`docs/downstream-changes.md`** 由 `tools/gen-downstream-changes.py` 自动从 `LOG.md` 提取，含文件路径 + 改动次数 + 操作列 + 说明列。改前查它能看清「这个文件之前被谁改过、改过几次」。

### 3.3 fix 分支命名与合并目标

| 约束 | 说明 |
|------|------|
| 来源 | fix **必须从被修复的 `feat/<name>` 拉出**；或从 `branch` 拉出修复 `branch` 自身 |
| 合并目标 | 修 feat 的 fix → 合并到对应 `feat/<name>`；修 branch 自身的 fix → 合并到 `staging` |
| 跨多 feat 修复 | 合并目标选**依赖链最下游**的 feat；commit message 与 PR 列出所有涉及 feat |

参考：`fix/account_rotation/修改次数` 同时修复 `feat/account_rotation` 与 `feat/defer-rogue` 交互缺陷，合并目标为 `feat/defer-rogue`（下游），详见 `LOG.md` 2026-07-15 同章节。

### 3.4 本地化

`src/MaaWpfGui/Res/Localizations/` 五语同步：zh-cn / zh-tw / en-us / ja-jp / ko-kr。新增 UI 文案 / ToolTip 同步在五份 xaml 中新增对应 string key。


## 4. 构建、部署与发布

### 4.1 构建命令

| 端 | 命令 | 备注 |
|----|------|------|
| C++ 配置 | `cmake --preset windows-publish-x64` | 平台预设 |
| C++ 构建 | `cmake --build build --target MaaCore` | **推荐单目标**，绕开 cmake 触发 WPF MSBuild 评估时的 VS 2026 SDK 路径 bug（见 `LOG.md` 2026-07-14「实际跑通 release-zip + 4 个 bug 修复」第 1 条） |
| C++ 安装 | `cmake --install build` | 部署到 `install/` |
| WPF | `dotnet publish src/MaaWpfGui/MaaWpfGui.csproj -c Release -p:Platform=x64` | `global.json` 写 `10.0.100` + `rollForward:latestFeature`，本机 10.0.300 自动启用 |
| 本地一键 | `tools/local-install.bat` | cmake 装 C++ + dotnet publish WPF 双轨；启动 `install/MAA.exe` |
| 打包 zip | `tools/release-zip.{bat,ps1}` | 见 `§4.3` |

### 4.2 子模块

| 子模块 | 上游 |
|--------|------|
| `src/MaaUtils` | `MaaXYZ/MaaUtils` |
| `3rdparty/EmulatorExtras` | `MaaXYZ/EmulatorExtras` |

- 首次 clone：`git clone --recursive`
- 已 clone 补全：`git submodule update --init --recursive`

### 4.3 打包发布

| 项 | 说明 |
|----|------|
| 版本号 | `VERSION` 文件，格式 `vX.Y.Z-fork.YYYYMMDD`（SemVer prerelease） |
| 产物 | `installer/MAA-vX.Y.Z-fork.YYYYMMDD-win-x64.zip` |
| 流程 | 单目标 cmake build → MaaCore install → 临时改 csproj 4 个 Version 字段 → dotnet publish → 剥 `*.pdb` `*.h` `*.bak` → robocopy staging（排除 `cache/` `config/` `data/` `debug/` 用户数据）→ ZipFile 压缩 |
| try/finally 保护 | csproj 备份 `.bak` 后改，无论成功失败均还原；`global.json` 同处理 |
| 仓库状态 | 脚本运行不污染 git 工作区 |

### 4.4 辅助脚本（`tools/`）

| 脚本 | 用途 |
|------|------|
| `local-install.bat` | 本地构建并部署到 `install/` |
| `local-install-staging.bat` | 本地构建并部署到 `install-staging/` |
| `release-zip.{bat,ps1}` | 一键打包 zip |
| `add_maa_to_nahimic_whitelist.ps1` | MAA.exe 加入 Nahimic DLL 注入白名单 |
| `disable_nahimic.ps1` | 停用 NahimicService 开机自启 |
| `cmake_build_for_wpf.bat` | 仅触发 cmake 的 WPF 构建 |
| `maadeps-download.py` | 依赖库下载 |
| `ClangFormatter/` | clang-format 集成 |
| `OverseasClients/`、`Roguelike*/`、`SmokeTesting/`、`SyncTemplate/`、`TaskSorter/` | 功能性辅助工具 |


### 4.5 部署目录职责

| 目录 | 角色 | 来源分支 | 构建脚本 |
|------|------|----------|----------|
| `install/` | **生产版** | `branch` (稳定下游基线) | `tools/local-install.bat` |
| `install-staging/` | **测试版** | `staging` (待验证整合区) | `tools/local-install-staging.bat` |

**硬约束**:

- staging 上的改动（含 `feat/*` / `fix/*` 在晋升 `branch` 之前）**必须**输出到 `install-staging/`，**绝不**写到 `install/`
- `install/` 是 `branch` 的产物；本地调试 staging 改动务必先 `git switch staging`，再用 `tools/local-install-staging.bat`
- 误把 staging 代码写进 `install/` 时立即 `git switch branch` → 跑 `tools/local-install.bat` 从 `branch` 重建恢复
- 日常测试启动 `install-staging/MAA.exe`；发布与正式运行用 `install/MAA.exe`
- 不允许在 `staging` 上执行 `dotnet publish -o install` 或 `cmake --install build --prefix install`（这两个命令属于 branch 构建步骤）
- `dotnet publish` / `cmake --install` 的 `-o` / `--prefix` 必须与当前 git 分支匹配


## 5. 代码风格与质量

| 项 | 工具 / 约定 |
|----|------------|
| C++ 格式化 | `.clang-format`（项目根） + `tools/ClangFormatter/` pre-commit |
| C++ 文件组织 | 每个 Task 类独立 `.h` / `.cpp` 配对（参 `src/MaaCore/Task/Miscellaneous/AutoRecruitTask.*`） |
| C# 格式化 | `.editorconfig`（项目根） |
| StyleCop | `SA1503` 等 warning 不阻断（见 `LOG.md` 2026-07-15「feat/defer-rogue 实施完成」编译结果段） |
| WPF MVVM | [Stylet](https://github.com/canton7/Stylet) |
| WPF 控件 | [HandyControls](https://github.com/ghost1372/HandyControls) |
| JSON | Newtonsoft.Json + System.Text.Json |
| 日志 | Serilog |
| 提交前检查 | `pre-commit run --all-files`：clang-format（仅 C++）/ yaml/json 语法 / `LOG.md` 修改记录 |


## 6. 进行中分支速查

| 分支 | 角色 | 修复目标 |
|------|------|----------|
| `fix/account-switch-retry` | LoginOther OCR 不匹配时追加 AccountManagerPageConfirm 模板兜底（DoNothing），消除 ~18s retry 空等；retry_times 保持 30 不变 | `staging`（依赖其诊断日志改动） |
| `fix/account_rotation/6` | 从 `branch` 拉出，从属 `feat/account_rotation`（已合入） | 账号轮换切号时左侧任务面板不刷新：`AdvanceAccountCycle` 未重置任务行状态/进度计数 + StartUp 行 `_taskIds` 丢失；顺带加「当前账号」Header 解决「误以为同一账号跑两次」的视觉混淆 |
| `fix/account-switch-template-missing` | 从 `branch` 拉出，修正 `fix/account-switch-retry` 修正版（`41cfcb736b`）漏提交 `AccountManagerPageConfirm.png` 的资源完整性漏洞；`TemplResource::load` 期望 `task_name + .png` 存在，不依赖 `baseTask` 继承 | `staging`（连带修复资源损坏导致 MAA 无法启动的连锁错误） |
| `fix/recruit-now-text-aliases` | 修公招加急按钮 OCR 文本漂移（新版 Arknights CN UI「立即完成」不被旧 `text:["立即招"]` 命中 → 加急必失败降级为正常 9h）。修法同 YoStarKR 3 备选先例 | `staging`（不阻塞晋升），关键 commit `9179d5e3a6` → `2f84118b07` → `633e8523f4`，详见 `LOG.md` 2026-07-27 |
| `fix/post-battle-sanity-display` | 修 `FightTask` MissionStart 日志「理智: x/y」展示语义：原 `x = SanityCurrent`（OCR 战前快照）与上一行「开始行动 1~6 次, -126 理智」消耗语义不连贯 → `x = SanityCurrent - FightReport.SanityCost`（series 实际成本战后预测）。仅 MissionStart 一处，不动 `SanityInfo` / Toast / CompleteTask / AllTasksComplete | `staging`（不阻塞晋升），关键 commit `d4b23812d3`，详见 `LOG.md` 2026-07-27 |
| `fix/reception-clue-vacancy` | 修会客室「填充线索空位」失效：`proc_clue_vacancy()` 快捷置入路径 4 类失败（OCR / 数字 / vacancy_cnt / confirm_task 缺失）都 `return true` 跳过 legacy 循环；`remove_clue` suffix `{1..7}` 与 `use_clue`/`proc_clue_vacancy` `{No1..No7}` 不一致导致模板错配空位（`ClueVacancy*.png`）而非已放置线索（`ClueVacancyNo*.png`）；legacy 循环 `continue` 前未刷新 `image` 死循环 + 放置后未关面板；`UnlockClues.next` 含 `InfrastBottomLeftTab` 在 720p 误触底部 Tab | `staging`（修复 `branch` 自身，按 §3.3 合并目标为 staging），关键 commit `ad725916b4`，详见 `LOG.md` 2026-07-27 |
| `feat/auto-recruit-3star-to-4star` | 自动公招 3★ 组合里有 4★ 干员潜力时升级为 4★ 处理路径（默认开启）：3:50 计时 + 联动 `ExpediteMinLevel` 加急 + 沿用 `SelectExtraTagsMode` 选 tag。新增 UI 选项 `AutoUpgrade3StarWith4StarToLevel4`（默认 `true`），与现有 `UseLevel3PreferTags` / `RefreshLevel3` / `ChooseLevel3` / `ExpediteMinLevel` 全兼容 | `staging`（与 `fix/recruit-now-text-aliases` / `fix/post-battle-sanity-display` / `fix/reception-clue-vacancy` 一起攒批验证），关键 commit `a41ce91369`（4★ 潜力检测） + `bb3e2799eb`（clang-format），详见 `LOG.md` 2026-07-30 |
| `feat/recruit-result-display` | 自动加急公招后告诉用户实际招募到谁：hire_all() 后立刻对截屏做多通道识别 (L1 OCR/Levenshtein + L1.5 立绘 pHash + L2 ★ 模板成功 + L3 截图脱敏兜底)，发 `RecruitSlotCompleted` callback（文档早就定义但 MAA 一直未实现），WPF 任务队列日志追加 `[加急/常规 9h] 招募完成 + 实际干员名 + tags` 双行 + 6★ 弹 Toast。L0-L3 多通道 + 21 项优化项（见 v6 方案）+ 项目级约束变更（feat 拉取源从 `branch` 改为 `staging`，branch 晋升改为用户手动合并 staging），[HOT] `AutoRecruitTask.cpp` 在第 16 次改动中接入 | `staging`（不阻塞晋升），关键 commit `d408fde841`（多通道识别 + pHash + 五语 + result task） + `882d9b64bf`（experience + accuracy + screenshot monitor + ocr fallback + 7 JSON） + `9d8a43e9d7`（round summary + AutoRecruitTask 接入） + `94d1b16579`（WPF 接收 + 5 语 18 key + 6★ Toast） + `579bec6879`（history/exp/sanitize/crypto services + C1 脱敏 + .gitignore） + `f293a811f7`（B3 失败聚类） + `88b7a86028`（编译修复 + meojson 适配 + OpenSSL 替换），详见 `LOG.md` 2026-07-30 |


## 7. 分支生命周期记录

以下分支已完成使命，已于 2026-07-16 / 2026-07-23 / 2026-07-24 删除（本地删，远端保留）。

### 7.1 feat/account_rotation

| 项 | 内容 |
|----|------|
| 用途 | 多账号自动轮换日常任务 |
| 生命周期 | 2026-07-10 创建 → 2026-07-14 合入 `branch` |
| 关键 commit | `c8c8e75be5`（Initial commit） → `23b1bf3167`（最终版） |
| 子修复分支 | `fix/account_rotation/{1,2,3,修改次数,5}` |
| 详见 | `LOG.md` 2026-07-10 / 2026-07-11 / 2026-07-13 / 2026-07-14 / 2026-07-15（修改次数） |

### 7.2 feat/defer-rogue

| 项 | 内容 |
|----|------|
| 用途 | 将肉鸽与生息演算延后到所有账号基础任务完成后执行 |
| 生命周期 | 2026-07-15 创建 → 2026-07-15 合入 `branch` |
| 关键 commit | `31b84f44a3` → `a9bad61958`（合并 `fix/defer-rogue/2` 后最终版） |
| 子修复分支 | `fix/defer-rogue/{1,2}` |
| 详见 | `LOG.md` 2026-07-15 |

### 7.3 feat/expedite-threshold

| 项 | 内容 |
|----|------|
| 用途 | 公招加急门槛：仅在确认招募最低星级 ≥ 4/5/6 时使用加急许可 |
| 生命周期 | 2026-07-15 创建 → 2026-07-23 合入 `branch` |
| 关键 commit | `3529ab0f05` → `cbec3d1fb0`（最终版） |
| 子修复分支 | 无 |
| 详见 | `LOG.md` 2026-07-23 / 2026-07-15 |

### 7.4 feat/idea

| 项 | 内容 |
|----|------|
| 用途 | 未记录用途分支，与 `branch` 同指针（`dc2212d54b`），无独立 commit |
| 生命周期 | 创建时间未知 → 2026-07-23 删除 |
| 关键 commit | `dc2212d54b`（Merge branch 'feat/account_rotation' into branch） |
| 详见 | 无 |

### 7.5 fix/account-official-recognize

| 项 | 内容 |
|----|------|
| 用途 | 修复「开始唤醒」任务在 **官服（Official）+ 账号轮换** 场景下卡死 |
| 根因 | `resource/tasks/tasks.json` 第 805-807 行 `AccountManagerOfficial` 仅定义 `roi`、无 `algorithm`/`template`/`text`，MAA 无法识别官服账号切换界面，30 次 retry 全失败 → `Login failed, entering game-restart loop` |
| 修复 | `AccountManagerOfficial` 补全 `OcrDetect` + `text: ["登录记录"]`（与 `AccountManagerBili` 对齐）；`AccountSwitchTask::navigate_to_start_page()` 加 `Log.info("last matched task:", last_name)` 诊断日志 |
| 生命周期 | 2026-07-24 创建 → 2026-07-24 FF 合入 `branch` |
| 关键 commit | `784d9005f6`（fix(startup): 官方服账号切换界面识别补全 + 切号诊断日志）|
| 子修复分支 | 无 |
| 作用域 | 仅本仓库 `branch` 修复，不推 upstream |
| 详见 | `LOG.md` 2026-07-24 |

### 7.6 fix/account-switch-retry

| 项 | 内容 |
|----|------|
| 用途 | 修切号时 `AccountSwitchTask::navigate_to_start_page` 重试预算与 OCR 兜底 |
| 根因 | 导航首步（`SwitchAccount@StartUpBegin` 含 22 个 next 候选）最坏需 ~13 次 retry 才有 UI 元素可识别；初版误判 `LoginOther` OCR 30×0.6s = 18s 空等，将 retry_times 降至 5 后 `TaskChainError`；正确修法是保持 retry=30 + 在 `LoginOther.next` 追加 `AccountManagerPageConfirm` 模板兜底 |
| 修复 | `tasks.json:808-817`：`LoginOther.next` 追加 `AccountManagerPageConfirm`（`baseTask: AccountManagerListAccount` + `action: DoNothing`），OCR 失败时同 retry cycle 内模板命中；`AccountSwitchTask.cpp:68-77`：`set_retry_times(30)` + `last_name` 白名单加 `AccountManagerPageConfirm` |
| 生命周期 | 2026-07-25 创建 → 2026-07-25 修正版 `--no-ff` 合入 `staging`（`6260abf14a`） |
| 关键 commit | `cd704f8bbc`（初版 retry=5，`TaskChainError`） → `41cfcb736b`（修正版 retry=30 + 模板兜底） |
| 子修复分支 | 无（独立分支） |
| 作用域 | 仅本仓库 `branch` 修复，不推 upstream |
| 详见 | `LOG.md` 2026-07-25（fix/account-switch-retry LoginOther OCR 模板兜底 + retry_times 分析修正） |

### 7.7 fix/account_rotation/6

| 项 | 内容 |
|----|------|
| 用途 | 修账号轮换切号时左侧任务面板不刷新 + 「当前账号」Header 视觉混淆 |
| 根因 | `LinkStartWithTasks`（首账号）有 `MainTasksCompletedCount = 0` + `ResetTaskItemStatuses()`，但 `AdvanceAccountCycle`（后续账号）全程无等价重置，导致切号后左侧仍是上一账号绿色 Completed + 进度条不出现；`TaskItemViewModel.SetTaskIds` 不重置 `StatusDisplay`；显式切号 taskId 不绑回 StartUp 行 |
| 修复 | `TaskQueueViewModel.cs`：`AdvanceAccountCycle` 切号前调用 `MainTasksCompletedCount = 0` + `ResetTaskItemStatuses()`，显式切号 taskId 绑回 StartUp 行，新增 `CurrentCycleAccountName` 属性在 5 处路径同步维护；`TaskItemViewModel.SetTaskIds` 末尾重置 `StatusDisplay`；`TaskQueueView.xaml` 左侧 Grid 2 行改 3 行 + Header Border + DataTrigger；五语 xaml 加 `CurrentAccountLabel` |
| 生命周期 | 2026-07-25 创建 → 2026-07-25 `--no-ff` 合入 `staging`（`6260abf14a`） |
| 关键 commit | `c5e2ba3831`（代码：8 文件 +74 -2） + `520dab59be`（docs：LOG.md / AGENTS.md §6） |
| 子修复分支 | 无（独立 fix） |
| 作用域 | 仅本仓库 `branch` 修复，不推 upstream |
| 详见 | `LOG.md` 2026-07-25（fix/account_rotation/6 启动 + 实施完成） |

### 7.8 fix/account-switch-template-missing

| 项 | 内容 |
|----|------|
| 用途 | 修 `fix/account-switch-retry` 修正版（`41cfcb736b`）漏提交 `AccountManagerPageConfirm.png` 的资源完整性漏洞 |
| 根因 | `tasks.json:813-817` 新增 `AccountManagerPageConfirm` task（`baseTask: AccountManagerListAccount` + `action: DoNothing`）但未提交对应 PNG。MAA `TemplResource::load` 期望每个 task 有同名 PNG（不依赖 `baseTask` 继承），文件缺失导致 `Templ load failed, file not exists` 与连锁 `TaskData load failed` / `OnnxSessions load failed` / `WordOcr load failed`，UI 报「资源损坏」无法启动 |
| 修复 | `resource/template/WakeUp/AccountManager/AccountManagerPageConfirm.png`：复制 `AccountManagerListAccount.png`（149 字节）作为 sibling 占位。DoNothing 任务实际不调用模板匹配，仅需文件存在让加载器存在性检查通过 |
| 生命周期 | 2026-07-25 创建 → 2026-07-25 `--no-ff` 合入 `staging`（`9ac844c10a`） |
| 关键 commit | `ad03f949e4`（fix(switch-template): 补 AccountManagerPageConfirm.png 满足 TemplResource 存在性检查，2 文件 +32） |
| 子修复分支 | 无 |
| 作用域 | 仅本仓库 `branch` 修复，不推 upstream |
| 详见 | `LOG.md` 2026-07-25（fix/account-switch-template-missing 启动 + 实施完成） |


## 8. 关键参考链接

### 8.1 上游文档 / 协议

| 类型 | URL |
|------|-----|
| 官网 | https://maa.plus |
| 用户文档 | https://docs.maa.plus |
| 集成协议 | https://docs.maa.plus/zh-cn/protocol/integration.html |
| 任务流程协议 | https://docs.maa.plus/zh-cn/protocol/task-schema.html |
| 回调消息协议 | https://docs.maa.plus/zh-cn/protocol/callback-schema.html |
| 自动战斗协议 | https://docs.maa.plus/zh-cn/protocol/copilot-schema.html |
| 仓内文档源 | `docs/`（vuepress） |

### 8.2 本仓集成示例

| Lang | 接口 | 示例 |
|------|------|------|
| C | `include/AsstCaller.h` | `src/Cpp/main.cpp` |
| Python | `src/Python/asst/asst.py` | `src/Python/sample.py` |
| Golang | `src/Golang/maa/maa.go` | — |
| Dart | `src/Dart/` | — |
| Java | `src/Java/.../MaaCore.java` | `MaaJavaSample.java` |
| Java HTTP | `src/Java/Readme.md` | — |
| Rust | `src/Rust/src/maa_sys` | HTTP `src/Rust/` |
| TypeScript | [MaaX coreLoader](https://github.com/MaaAssistantArknights/MaaX/tree/main/packages/main/coreLoader) | — |
| Woolang | `src/Woolang/maa.wo` | `src/Woolang/demo.wo` |

### 8.3 上游关联项目

- 框架：[MaaXYZ/MaaFramework](https://github.com/MaaXYZ/MaaFramework)
- 作业站前端：[zoot-plus-frontend](https://github.com/ZOOT-Plus/zoot-plus-frontend)
- 作业站后端：[ZootPlusBackend](https://github.com/ZOOT-Plus/ZootPlusBackend)
- 官网前端：[maa-website](https://github.com/MaaAssistantArknights/maa-website)
- 深度学习：[MaaAI](https://github.com/MaaAssistantArknights/MaaAI)