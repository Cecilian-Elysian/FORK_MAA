# AGENTS

本文件面向在本仓库协作的 AI 代理与人类贡献者，约定工作流、文档规范与协作约束。

分支的存在性与生命周期记录在 `§6` 与 `§7`；本仓库**不使用** feat / fix 工作笔记（无 `feat_<name>.md` / `fix_<name>[_<n>].md`），所有变更通过 commit message 与 `LOG.md` 跟踪。

中文叙述为主，分支名 / 命令 / 协议字段等关键术语保留英文原文。无 emoji、无 vuepress 容器，遵循 `LOG.md` 既有的表格驱动风格。上游开发约定全文见 `§9`。

> **重要约束**：拉取上游新功能（merge `upstream/master-v2`）**必须**按 [`WORKFLOW.md`](./WORKFLOW.md) 流程执行（§5 graft 假历史关联 + §6 合并手解 + §7 转 git replace + §8 编译验证）。该流程基于 2026-08-06 v6.16.5 合入经验总结，绕过 fork root 无父节点导致的 unrelated histories 问题。
>


## 1. 项目概述

### 1.1 一句话

基于 [MaaAssistantArknights/MaaAssistantArknights](https://github.com/MaaAssistantArknights/MaaAssistantArknights) 上游 `master-v2`（稳定 release 分支）的本地 fork，主仓 `branch` 累积本地下游增强。业务语义：基于 [MaaFramework](https://github.com/MaaXYZ/MaaFramework) 的图像识别明日方舟小助手，「一键完成全部日常任务」。

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
| `upstream` | 上游 `MaaAssistantArknights/MaaAssistantArknights`，本 fork 以 `master-v2`（稳定 release 分支）为基线跟踪对象（注意：`remotes/upstream/HEAD` 实际指向 `dev-v2`，勿以 HEAD 为准） |
| `Github` | 个人 fork 远端，分支同步发布用 |
| `origin` | （未配置 / 备用） |

### 2.2 本地分支

| 分支 | 角色 | 备注 |
|------|------|------|
| `master` | **上游镜像** | 仅 `git fetch upstream && git reset --hard upstream/master-v2`，与上游 `master-v2` 保持一致；**不推送** |
| `new-branch` | **临时对比分支** | 仅在拉上游时创建：从本地 `master` 拉出（先 `git reset --hard upstream/master-v2` 再 `git branch -f new-branch master`）；用于与 `branch` 对比落后上游多少，用完可删 |
| `branch` | **稳定下游基线** | 从 `staging` 晋升（`git merge --no-ff staging`）+ 部署 `install/`（`tools/local-install.bat`） |
| `staging` | **待验证整合区** | 所有 feat / fix 的合并目标；测试通过后晋升至 `branch`（详见 `§2.4`）；部署 `install-staging/` |
| `feat/<name>` | 新功能 | 从 `staging` 拉出，合并到 `staging` |
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
master (上游 master-v2 镜像)
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
| **上游同步 SOP** | **拉取上游新功能（merge `upstream/master-v2`）必须按 `WORKFLOW.md` 流程执行**（§5 graft + §6 合并 + §7 replace + §8 编译验证） |

#### 当前 staging 内容（截至 2026-08-13 staging → branch 晋升完成）

`staging` 通过 commit `706f8babf4` merge `upstream/master-v2`（v6.16.5 release），已含：
- Phase A: 移除 `feat/auto-recruit-3star-to-4star`（commit `3fd8903115`）
- Phase B: AGENTS/CHANGELOG/csproj/.gitignore 清理（commit `dcb3cc6cb4`）
- Phase D: 上游 v6.14.0 ~ v6.16.5 共 4467 commit 合入
- 2026-08-07 master-v2 对齐：`c951f239c1` 放弃全部 fork 私有 C++ 回归 master；`c34403ac94` tasks.json account-switch 区块回归 master（删除 fork `AccountManagerPageConfirm` task + 模板）；`3904577917` 仅恢复 expedite_min_level 阈值 C++（对齐 fork WPF UI）
- 2026-08-08：`1ea65d0aed` 加急点击按槽位限定 RecruitNow roi（`RecruitNow@Slot0..3` 四变体）；`70b3d63770` fix/account-cycle-start-race 提交落地（AsstStart 竞态 AsstRunning 兜底 + AsstStop 清队列）
- 2026-08-09：`265bd875a4` 恢复 ClipboardLinkSet20Regular Geometry 资源（修 CopilotView 启动闪退）
- 2026-09-07：`f580d7ed6d` fix/depot-special-memorial-tab 手摘上游 `da5ccfe4ed` 仓库「特别纪念」tab 修复（§7.18）
- 2026-09-16：`6d32144858` merge 上游 v6.18.0-beta.1（§7.22 阶段 0-10）；`ae24fd2851` 补 CurrentAccountLabel 5 语 key；`16b11a0e31` 清 ToolboxTabRecruitHistory dead key（§10.2）
- 2026-09-25：`39a7b516e7` fix/sanity-auto-series-adapt（AUTO 倍率按理智自适应）；`0a3884231d` merge 上游 v6.18.0 release（134 commit，§7.22 阶段 11）+ `1b7b087aae` 编译修复三连；MaaUtils 子模块 bump `0c2556cf` → `6e9ba33`

历史假关联：fork base `c8c8e75be5` 无父节点，通过 `git replace --graft` 接 `6147357bd0`（v6.14.0 upstream release），merge-base = `c8c8e75be5` 走 3-way 合并。

#### staging → branch 晋升记录

| 日期 | merge commit | 来源 commit 数 | 验证策略 | 备注 |
|------|--------------|----------------|----------|------|
| 2026-08-07 | `4d862ec98a` | （master-v2 基线重建首晋升） | 完整实测 | `c951f239c1` 放弃 fork C++ 回归 master |
| 2026-08-13 | `1774d128a2` | 17 commit（4 修复 + 3 文档 + 10 间接） | trust staging | §7.10/§7.11/§7.12/§7.13 全部 fix 走 staging 验证流程后晋升 |
| 2026-08-30 | `a0f530912c` | 20 commit（2 修复 + 1 feat + 3 工具/重构 + 14 间接） | trust staging | §7.14/§7.15/§7.16/§7.17 全部 fix/feat 走 staging 验证流程后晋升；install-staging 2026-08-28 09:28 内核指纹覆盖全部变更 |
| 2026-09-16 | _待晋升_ | v6.17.0 → v6.18.0-beta.1 全量同步 merge `6d32144858`（staging 已含，branch 未晋升） | trust staging | §7.22 `feat/upstream-v618-sync` 合入 staging；§7.18 已先手摘上游 `da5ccfe4ed` depot 修复入 staging（9/7）；编译/冒烟/fork marker 全过 |
| 2026-09-25 | _待晋升_ | v6.18.0 release 跟进 merge `0a3884231d` + `1b7b087aae`（staging 已含，branch 未晋升） | trust staging | §7.22 阶段 11：上游 134 commit + MaaUtils bump `6e9ba33` + 冒烟/总检全过；晋升实测项与上行一致 |

晋升后实测验证项（仅适用于 `staging → branch` 晋升决策；本次晋升 trust staging）：
- 多账号切号（官服 + B 服，含新增繁中服支援）
- 公招加急门槛（`expedite_min_level` fork 字段 + 上游 `expedite`/`expedite_times` 双轨）
- 公招加急按槽位定位（四槽位同时进行时加急不串位到左上槽位，`RecruitNow@Slot0..3`）
- 轮换推进无「出现未知错误」误报（fix/account-cycle-start-race：AsstStart 竞态 AsstRunning 兜底）
- 招募流程（无 3→4 升级的回归 + 新上游 sortIndex + 库存重构）
- 刷理智代理倍率 7~10 校验（v6.16.5 新增）
- LUID GPU OCR + Win32IO 竞态修复（v6.16.5）
- 资源包拖入更新资源版本（v6.16.5 新增）
- 工具：`tools/local-install-staging.bat` 部署到 `install-staging/`，启动 `MAA.exe` 跑一遍日常（晋升前实测通过）


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

### 3.5 识别类故障排查 SOP：先查上游修复（2026-09-07 沉淀）

**适用症状**：某识别 / 导航功能突然持续失败（多账号多主题全量复现），而 MAA 侧代码、模板、配置、分辨率均零改动。

**排查顺序**（先 2 后 3，避免重复造轮子）：

| # | 步骤 | 说明 |
|---|------|------|
| 1 | 本地排除 | 确认 install 与 repo 资源哈希一致、构建未变、分辨率 / 模拟器未变、cache 在线资源不含相关任务定义 → 锁定「游戏侧画面变了」 |
| 2 | **查上游是否已修** | GitHub API 按路径过滤 commits（最高效）：`https://api.github.com/repos/MaaAssistantArknights/MaaAssistantArknights/commits?sha=dev-v2&path=resource/template/<功能目录>&per_page=10`，对比 commit 时间与故障起点；同时搜上游 issues（游戏 UI 改版通常会引发用户反馈）。修复常在故障起点 1~2 天内出现（上游维护者也在玩） |
| 3 | 摘取或自截 | 上游已修 → 新建 fix 分支手动摘取（ROI 数值照抄 + raw.githubusercontent.com 下载 `<commit>/resource/template/...` PNG，优先取其后 Auto Templates Optimization 终版）；上游未修 → 从 `debug/interface/` 失败帧自截模板，并用 NCC 离线验证 ≥0.9 再部署 |

**案例**：`fix/depot-special-memorial-tab`（§7.18）——游戏新增「特别纪念」tab 致仓库识别全挂，上游 `da5ccfe4ed` 在故障起点次日即修复（且不在 v6.17.0 release 内，master-v2 拿不到，必须手动摘）。注意：**模板 PNG 不走在线资源更新**（cache 仅 tasks.json），等自更新等不来模板修复。

**离线验证技巧**：PowerShell + System.Drawing 自写 NCC（归一化互相关），对 `debug/interface/` 失败帧 × 新模板算分，部署前即可确认模板有效性；失败帧摄于「初始未点击」态时，选中态模板（如 `DepotAllTab`）测不出高分属正常，信任上游已实测路径。


## 4. 构建、部署与发布

### 4.1 构建命令

| 端 | 命令 | 备注 |
|----|------|------|
| C++ 配置 | `cmake --preset windows-publish-x64` | 平台预设 |
| C++ 构建 | `cmake --build build --target MaaCore` | **推荐单目标**，绕开 cmake 触发 WPF MSBuild 评估时的 VS 2026 SDK 路径 bug（见 `LOG.md` 2026-07-14「实际跑通 release-zip + 4 个 bug 修复」第 1 条） |
| C++ 安装 | `cmake --install build` | 部署到 `install/` |
| WPF | `dotnet publish src/MaaWpfGui/MaaWpfGui.csproj -c Release -r win-x64 -o install-staging /p:DisableBeauty=True` | `global.json` 写 `10.0.100` + `rollForward:latestFeature`，本机 10.0.300 自动启用 |
| **WPF 后处理** | **& "$env:USERPROFILE\.nuget\packages\nulastudio.netbeauty\2.1.5\tools\win-x64\nbeauty2.exe" --usepatch "$PWD\install-staging/." "./externals"** | **⚠️ 不可漏！漏跑会导致 `MAA.exe` 启动闪退报 `Could not load file or assembly 'libloader'`。详见 §4.1.1** |
| 本地一键 | `tools/local-install-staging.bat` | cmake 装 C++ + dotnet publish WPF + nbeauty2 后处理；启动 `install-staging/MAA.exe` |

#### 4.1.1 NetBeauty2 后处理（**必读踩坑**）

**问题**：`dotnet publish /p:DisableBeauty=True` 跳过 MSBuild 的 `NetBeautyOnPublish` target。
后果：`MAA.runtimeconfig.json` **不会**写入 `STARTUP_HOOKS=libloader` + `NetBeautyLibsDir` 配置。
新构建的 `MAA.exe` 在 .NET 10 严格 startup hook 检查下，加载原生 `libloader.dll` 失败 → **闪退无错误**。
**报错**（命令行启动可见）：
```
Unhandled exception. System.ArgumentException: Startup hook assembly 'libloader' failed to load.
 ---> System.IO.FileNotFoundException: Could not load file or assembly 'libloader, ...'
```

**修复**：必须显式调用 `nbeauty2.exe` 对输出目录做后处理。

```powershell
# 必须步骤：手动运行 NetBeauty2 后处理
$nbeauty = "$env:USERPROFILE\.nuget\packages\nulastudio.netbeauty\2.1.5\tools\win-x64\nbeauty2.exe"
& $nbeauty --usepatch "$PWD\install-staging/." "./externals"

# 验证：runtimeconfig.json 应包含 STARTUP_HOOKS
Select-String -Path install-staging/MAA.runtimeconfig.json -Pattern "STARTUP_HOOKS"
```

**检测脚本**：`tools/post-merge-validate.ps1` 第 [7] 项已加入 NetBeauty 配置检查。

**参考**：`tools/local-install-staging.bat` 第 27-28 行是正确两步流程的范例。

### 4.2 子模块

| 子模块 | 上游 | 备注 |
|--------|------|------|
| `src/MaaUtils` | `MaaXYZ/MaaUtils` | Windows 主构建依赖（C++ 通用工具） |
| `3rdparty/EmulatorExtras` | `MaaXYZ/EmulatorExtras` | Windows 主构建依赖（模拟器附加能力） |
| `src/MaaMacGui` | `MaaAssistantArknights/MaaMacGui.git` | macOS GUI 独立仓库；Windows 主构建不依赖 |
| `src/maa-cli` | `MaaAssistantArknights/maa-cli.git` | CLI 工具独立仓库；Windows 主构建不依赖 |

- 上游 master-v2 的 `.gitmodules` 含 6 条（多 `test`、`src/MAAUnified`），fork 已于 `fix/audit-fixes` 删除后两条（无 gitlink、Windows 不依赖）
- 首次 clone：`git clone --recursive`
- 已 clone 补全：`git submodule update --init --recursive`（MaaMacGui / maa-cli 在 Windows 上未 checkout 不会阻断构建）

### 4.3 辅助脚本（`tools/`）

| 脚本 | 用途 |
|------|------|
| `local-install.bat` | 本地构建并部署到 `install/` |
| `local-install-staging.bat` | 本地构建并部署到 `install-staging/` |
| `add_maa_to_nahimic_whitelist.ps1` | MAA.exe 加入 Nahimic DLL 注入白名单 |
| `disable_nahimic.ps1` | 停用 NahimicService 开机自启 |
| `cmake_build_for_wpf.bat` | 仅触发 cmake 的 WPF 构建 |
| `maadeps-download.py` | 依赖库下载 |
| `gen-downstream-changes.py` | 从 `LOG.md` 生成 `docs/downstream-changes.md` 下游改动清单 |
| `ClangFormatter/` | clang-format 集成 |
| `OverseasClients/`、`Roguelike*/`、`SmokeTesting/`、`SyncTemplate/`、`TaskSorter/`、`MaaWpfGui.Benchmarks/` | 功能性辅助工具 |


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
| 提交前检查 | `pre-commit run --all-files`：clang-format（仅 C++）/ oxipng（PNG）/ prettier（yaml/json/docs）/ ruff-format（Python）/ markdownlint（docs）。`LOG.md` 为 UTF-16 LE 二进制，pre-commit 不校验，靠 commit message 与人工维护 |


## 6. 进行中分支速查

| 分支 | 角色 | 修复目标 |
|------|------|----------|
| （无 — v6.18 beta.1/v6.18.0 同步均随 merge `6d32144858`/`0a3884231d` 直接落地 staging，无独立分支） | — | — |


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

### 7.5 fix/account-official-recognize（2026-08-07 已随 master-v2 基线重建回退删除）

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

### 7.6 fix/account-switch-retry（2026-08-07 已随 master-v2 基线重建回退删除）

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

### 7.7 fix/account_rotation/6（2026-08-07 已随 master-v2 基线重建部分回退：C++/tasks.json 侧回退，WPF 侧 AccountCycleOrchestrator 保留）

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

### 7.8 fix/account-switch-template-missing（2026-08-07 已随 master-v2 基线重建回退删除）

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

### 7.9 feat/recruit-result-display（已回退）

| 项 | 内容 |
|----|------|
| 用途 | 自动加急公招后告诉用户实际招募到谁（fork 私有功能，详见 §6 原行） |
| 生命周期 | 2026-07-10 创建 → 2026-07-30 合入 `staging`（`9f0f76d6fa`，46 files +2628） → 2026-08-02 由 `fix/remove-recruit-result-display` 回退合并到 staging |
| 关键 commit | `3b1e13fcf9`（启动） → `d408fde841`（多通道识别） → `882d9b64bf`（经验+准确率+screenshot monitor） → `9d8a43e9d7`（round summary + AutoRecruitTask 接入） → `94d1b16579`（WPF 接收 + 5 语 18 key + 6★ Toast） → `579bec6879`（services + C1 脱敏 + .gitignore） → `f293a811f7`（B3 失败聚类） → `88b7a86028`（编译修复） |
| 子修复分支 | `fix/recruit-screenshot-monitor-channels`（`84b339a870`，channels mismatch 修复），随功能回退一并删除 |
| WPF UI 扩展 | `feat/recruit-history-tab`（`69c5e4f74c`，合并 `c143d8eef9`）— 工具箱「公招历史」Tab UI 依赖本功能 callback，随回退一并删除 |
| 回退 commit | `fix/remove-recruit-result-display`：删 33 文件 + 回退 14 集成点，47 files +1/-2904，详见 `LOG.md` 2026-08-02 |
| 保留约定 | AGENTS §2.4 staging 工作流（feat 拉取源 = staging + branch 手动晋升）由本 feat 引入，但已用于其他分支，**保留** |
| 保留功能 | ~~`fix/auto-recruit-expedite-original-level` 的 `m_original_min_level` 加急判定 + RecruitResult 回调 level 用原始星级~~ —— **已于 2026-08-07 随 C++ 全回归 master（`c951f239c1`）删除**，`3904577917` 恢复 expedite_min_level 时未带回 `m_original_min_level`。当前公招加急判定仅用新恢复的 `m_expedite_min_level`（WPF `expedite_min_level` 字段），无「3→4 升级后原始星级」概念（该上游加急路径在 8/6 已移除） |
| 作用域 | 仅本仓库 fork 私有代码 + 协议 callback 实现，不推 upstream |

### 7.10 fix/audit-fixes（2026-08-07 已合入 staging，2026-08-13 晋升 branch）

| 项 | 内容 |
|----|------|
| 用途 | 2026-08-07 全仓审计修复：minitouch 补图 + .gitmodules 孤儿条目清理 + 文档三连刷（AGENTS/CHANGELOG/downstream）+ stash×4 清理 |
| 生命周期 | 2026-08-07 创建（从 staging 拉出） → 2026-08-07 `--no-ff` 合入 `staging`（`cc0e82247d`） |
| 关键 commit | `bc3f4a2a2a`（minitouch 补图） → `7d16e1f05b`（.gitmodules + LOG 启动） → `077f94970a`（AGENTS.md） → `cda6587d85`（CHANGELOG） → `16a40b0442`（downstream 重生成） → `33d91e9da5`（LOG 实施完成） |
| 子修复分支 | 无（独立 fix） |
| 作用域 | 仅文档与资源文件；不涉及 C++ / C# 代码改动（8/7 expedite C++ 恢复由 `3904577917` 单独落地） |
| 详见 | `LOG.md` 2026-08-07（fix/audit-fixes 启动 / 实施完成 / 合入 staging） |

### 7.11 fix/recruit-expedite-slot-target（2026-08-08 已合入 staging，2026-08-13 晋升 branch）

| 项 | 内容 |
|----|------|
| 用途 | 公招加急点击按槽位限定 RecruitNow roi，修复多槽位同时进行时加急串位到左上槽位 |
| 根因 | `RecruitNow` 原 roi `[0,300,1280,420]` 覆盖全部 4 槽位，多槽位同时进行时 OCR 多命中「立即招」，ProcessTask 点击第一个命中框（页面最上槽位）；实测 4 槽 rect `[364,366]/[996,368]/[364,645]/[994,645]`，三星被加急、四星留 9h |
| 修复 | `tasks.json` 追加 `RecruitNow@Slot0..3` 四变体（roi 按 `slot_index_from_rect` 分界线 x=640 / y=450 划分象限，baseTask 继承）；`AutoRecruitTask::recruit_now(slot_index)` 按目标槽位选择任务 |
| 生命周期 | 2026-08-08 创建 → 2026-08-08 `--no-ff` 合入 `staging`（`d4a763e04d`） |
| 关键 commit | `1ea65d0aed` |
| 子修复分支 | 无 |
| 作用域 | 仅本仓库 fork 私有，不推 upstream |
| 详见 | `LOG.md` 2026-08-08（fix/recruit-expedite-slot-target 启动） |

### 7.12 fix/account-cycle-start-race（2026-08-08 提交落地到 staging，2026-08-13 晋升 branch）

| 项 | 内容 |
|----|------|
| 用途 | 修账号轮换推进时 `AsstStart()` 竞态误报「出现未知错误」+ 误停轮换 |
| 根因 | `AllTasksCompleted` 回调时 Core 工作线程仍处于 `wait_for(task_delay)` 睡眠窗口（约 500ms），`m_thread_idle=false`，`AsstStart()` 必返回 false |
| 修复 | `TaskQueueViewModel.cs` `AdvanceAccountCycle`：`startOk = taskRet && (AsstStart() || AsstRunning())` 兜底判定；失败分支 `AsstStop()` 清队列；`WORKFLOW.md` §6.5.1 登记 fork 私有标记 |
| 生命周期 | 2026-08-07 修复（LOG 记录 + install-staging 实测通过） → 2026-08-08 直接提交 `staging`（`70b3d63770`） |
| 关键 commit | `70b3d63770` |
| 子修复分支 | 无（工作区修复直接落地，未走 fix 分支） |
| 作用域 | 仅本仓库 fork 私有，不推 upstream |
| 详见 | `LOG.md` 2026-08-07 / 2026-08-08 |

### 7.13 fix/copilot-view-missing-icon-resource（2026-08-09 已合入 staging，2026-08-13 晋升 branch）

| 项 | 内容 |
|----|------|
| 用途 | 修 MAA 启动后切「自动战斗」Tab 闪退：`XamlParseException` 无法找到名为 `ClipboardLinkSet20Regular` 的资源 |
| 根因 | 上游 `e9d00b94af` (#14624) 引入 `<Geometry x:Key="ClipboardLinkSet20Regular">` + `CopilotView.xaml` 作业集按钮引用；上游 `be0d9f342d` (2026-07-26 "移除过期格式兼容按钮") 同时删除 Geometry 定义 + XAML 引用。fork `706f8babf4` merge v6.16.5 §6 手解时保留 fork 私有 `PasteClipboardCopilotSet` 按钮 + XAML `{StaticResource ClipboardLinkSet20Regular}` 引用，但 `Geometries.xaml` 资源定义未带过来 → XAML 孤立引用 → 启动闪退 |
| 修复 | `src/MaaWpfGui/Res/Styles/Basic/Geometries.xaml`：恢复 `ClipboardLinkSet20Regular` Geometry（path data 原样复制自 upstream `e9d00b94af`），插入位置对齐 upstream（`ClipboardLink20Regular` 与 `FolderOpen20Regular` 之间） |
| 生命周期 | 2026-08-09 创建（从 staging 拉出） → 2026-08-09 `--no-ff` 合入 `staging`（`399b22c617`） |
| 关键 commit | `265bd875a4` |
| 子修复分支 | 无（独立 fix） |
| 作用域 | 仅本仓库 fork 私有，不推 upstream（恢复 upstream `e9d00b94af` 引入、被 `be0d9f342d` 删除的资源定义，供 fork 保留的作业集按钮引用） |
| 详见 | `LOG.md` 2026-08-09 |

### 7.14 feat/account-scoped-recognition-data（2026-08-25 已合入 staging，2026-08-30 晋升 branch）

| 项 | 内容 |
|----|------|
| 用途 | 干员/仓库识别数据按账号分桶存储：`data\OperBoxData_<account>.json` / `DepotData_<account>.json`，各账号数据独立保留可查看；修复多账号轮换下 StageDrops 掉落增量以上一账号库存为基数的跨账号数据合并；切号即切桶（清脏数据 + 预载本账号桶）；旧全局单份文件一次性迁移（旧文件转 `.json.bak`） |
| 关键实现 | `ToolboxViewModel.cs` `#region AccountScopedRecognitionData`（桶路由/切换/迁移/下拉列表/掉落无基线守卫）；`TaskQueueViewModel.cs` `LinkStart` + `AdvanceAccountCycle` 两处 `SwitchDataAccount` 锚定；`ToolboxView.xaml` 两 Tab 账号查看下拉（运行中锁定）；五语 +2 key（`DataAccountLabel`/`DataAccountDefault`）；账号名清洗（非法字符→`_`、截断 48、空→`_default`），JSON 内嵌 `account` 原始名供显示 |
| 生命周期 | 2026-08-25 创建（从 staging 拉出） → 2026-08-25 `--no-ff` 合入 `staging`（`04217c1fcb`） |
| 关键 commit | `b5431a1a5f`（实施，10 files +355 -21） → `584fa5b3eb`（downstream 清单） |
| 子修复分支 | 无 |
| 验证 | dotnet build 0 错误 / 60 warning（基线一致）；install-staging 实机：启动冒烟通过、首启迁移实测成功（账号 `189****0830` → 桶 `*_189____0830.json` + `.bak`，account 字段嵌入）；完整轮换跑批待用户实测 |
| 部署备注 | `local-install-staging.bat` 全目标构建触发 §4.1 已知 VS 2026 SDK bug，本次按单目标 + 手工 publish/nbeauty/robocopy 绕行部署 |
| 作用域 | 仅本仓库 fork 私有，不推 upstream |
| 详见 | `LOG.md` 2026-08-25（启动 / 实施完成 / 合入 staging 三段） |

### 7.15 fix/reception-clue-restore（2026-08-28 已合入 staging，2026-08-30 晋升 branch）

| 项 | 内容 |
|----|------|
| 用途 | 修复「无法添加线索 / 不会自动填充线索」：官服会客室"快捷置入"分支 OCR 徽标数字（`InfrastClueQuickInsertConfirm`，roi `[1250,615,28,28]`）失败 / `available != vacancy_cnt` / `confirm_task` 缺失时无条件 `return true`，跳过下方 legacy 逐位放置循环 → 步骤报成功但一条线索都没放。对应上游 issue #16165（closed as not planned，至 v6.16.8 仍未修，dev-v2/master-v2 该文件均无差异） |
| 根因 | `proc_clue_vacancy()`（`InfrastReceptionTask.cpp:255-273` 原版）快捷置入路径控制流缺陷：4 类失败均 fallback 到 `return true` 而非 legacy 循环。fork 旧修复 `ad725916b4`（2026-07-27）已根治，但 `c951f239c1`（2026-08-07 master-v2 基线重建）整体回退 fork 私有 C++，带回了上游缺陷 |
| 关键实现 | `src/MaaCore/Task/Infrast/InfrastReceptionTask.cpp:255-340`：`vacancy_cnt==0` 显式提前 return；`confirm_task != nullptr` 单独判断；引入 `click_performed` 仅真实点击时 `return true`；OCR analyze 失败 / `chars_to_number` 解析失败 / `available != vacancy_cnt` / `confirm_task` 缺失各打 `Log.warn(..., "fallback")` 不 return，落入 legacy 循环；legacy 迭代顶部刷新 `image` 防陈旧截图；放置线索后 `Matcher(InfrastReceptionIcon)` 检测关闭右侧面板（与 `remove_clue` 同款模式） |
| 生命周期 | 2026-08-28 创建（从 staging 拉出） → 2026-08-28 `--no-ff` 合入 `staging`（`4590789380`） |
| 关键 commit | `1eea6807b8`（fix(reception-clue): 修复「无法添加线索」快捷置入 OCR 失败跳过 legacy 循环，2 files +38 -3） |
| 子修复分支 | 无（独立 fix） |
| **取舍说明** | 不照搬 fork 旧修复 5 处全文，仅恢复 1+3+4 三点。**不恢复 `remove_clue` suffix**（`{1..7}`→`{No1..No7}`）：当前 master `#16054`（`af783dd558`，2026-04-21）已引入 `ClueVacancy1..7.png` 彩色模板（饱和度 0.127 = 已放置线索），`remove_clue` 用 `{1..7}` 匹配彩色 = 正确；`ClueVacancyNo1..7.png` 灰（饱和度 0.015）= 空位模板。fork 旧 base 无 #16054 模板时方向相反，当前基线恢复会破坏 remove。**不恢复 `tasks.json UnlockClues.next` 去除 `InfrastBottomLeftTab`**：与本 bug 无关，用户确认保留上游兜底 |
| 模板语义附录 | `ClueVacancy*.png`（彩色 0.127）= 已放置线索；`ClueVacancyNo*.png`（灰 0.015）= 空位；`ClueVacancyPin.png`（饱和度 0）= 移除按钮。三者由饱和度区分：饱和度越高 = 已放置 |
| 文档附录 | `docs/downstream-changes.md:151`「保留 fork `vacancy_cnt==0` 早返回」描述已过时（实际未保留），本次恢复才补回；下下游清单下次刷新对齐 |
| 验证 | C++ 单目标 `cmake --build build --target MaaCore --config RelWithDebInfo` 0 错误 / 1 已知 LNK4075 warning（pre-existing，与本次无关）；`cmake --install build --prefix install-staging` 落地 MaaCore.dll 4243968 字节（2026-08-28 09:08:01）；install-staging/MAA.exe 冒烟 8s `AsstLoadResource ret: true` 无闪退；官服实机复现「快捷置入 + OCR 失败/数字不匹配」场景待用户实测 |
| 部署备注 | 仅 C++ 改动，WPF 未变，无 nbeauty2 重跑；先 `Stop-Process MAA.exe` 解 `Permission denied` 再 `cmake --install` |
| 作用域 | 仅本仓库 fork 私有，不推 upstream |
| 详见 | `LOG.md` 2026-08-28（fix/reception-clue-restore 启动 / 实施完成 / 合入 staging 三段） |

### 7.16 fix/account-rotation-supersede-switcher（2026-08-16 已合入 staging，2026-08-30 晋升 branch）

| 项 | 内容 |
|----|------|
| 用途 | 账号轮换彻底吸收账号切换的生态位：永久内联方案（删除原 AccountSwitch 单账号 section + 编辑模式 ComboBox + AddAccountAfter 按钮 + ShowEditSection/ShowAddMode/ShowDeleteMode 属性；保留 StartUpTask.AccountName/AccountSwitchEnabled 字段向后兼容旧 GUI 配置）；删除旧 4 个 localization key（5 语同步）；新增 5 个轮换相关 key（AccountCycleTip / AccountCycleAddNewAccount / AccountCycleRemoveTip / AccountCycleRemoveConfirm / AccountCycleRemoveMessage）；删除按钮加 MessageBox 二次确认（复用 `MessageBoxHelper.Show`，对齐 `TaskQueueViewModel.RemoveTask` 模式） |
| 关键实现 | `src/MaaWpfGui/Views/UserControl/TaskQueue/StartUpTaskUserControl.xaml:26-94`（删单账号 section + 编辑模式 ComboBox + 末尾 [+ 添加账号] 按钮）；`src/MaaWpfGui/ViewModels/UserControl/TaskQueue/StartUpSettingsUserControlModel.cs:53-58/107-110/200-210/294-318`（删 #region Account Switch (Single) 整段 + 4 个 Show* 属性 + `RemoveAccount` 加 MessageBox）；5 语 `Res/Localizations/*.xaml:694-700`（删 4 key + 加 5 key） |
| 生命周期 | 2026-08-16 创建（从 staging 拉出） → 2026-08-16 `--no-ff` 合入 `staging`（`9a46d7b4ce`） |
| 关键 commit | `fa99387755`（fix(cycle): supersede single-account switcher with cycle，9 文件 +100 -194） |
| 子修复分支 | 无 |
| **已知 TODO**（不在本 fix 范围） | ~~`CurrentAccountLabel` 在 `TaskQueueView.xaml:102` 引用但 5 语 xaml 全缺，Header 前缀空白。需 5 语 × 1 行 = 5 行修复，待后续 fix 单独处理~~ **已修 2026-09-16**（commit `ae24fd2851`，5 语各 +1 行插于 AccountCycle key 前） |
| 验证 | install-staging 部署（`MAA.exe` 2026/8/16 18:28，`MaaCore.dll` 4241920 B 与合并前一致因 C++ 无改动）；编译 0 错误 / 32 StyleCop warning（不阻断）；信任 staging 验证（无实测跑日常），因改动纯 UI 重构 + localization 字符串调整 |
| 部署备注 | `tools/local-install-staging.bat` 触发 §4.1 已知 VS 2026 SDK bug，本次绕行（`MSBuildSDKsPath=C:\Program Files\dotnet\sdk\10.0.300\Sdks` + 单 `cmake --build --target MaaCore`） |
| 作用域 | 仅本仓库 fork 私有，不推 upstream |
| 详见 | `LOG.md` 2026-08-16（fix/account-rotation-supersede-switcher 启动 / 实施完成 / 合入 staging 三段） |

### 7.17 fix/diagnostic-export-refactor（2026-08-16 已合入 staging，2026-08-30 晋升 branch）

| 项 | 内容 |
|----|------|
| 用途 | 「生成诊断报告」按钮 `GenerateSupportPayload()` 重构：170+ 行单一方法拆分为 7 个单一职责方法（TryPrepareExportContext / WriteDiagnosticJson / CopyAll / SplitIntoParts / CreateFullZip / Cleanup / ShowResult）；分卷策略从「区分 part01 含 gui.log/asst.log+根文件 vs part02+ 含 debug 子目录」改为「按 20MB 单卷上限统一大小分卷」；改 `async void` + `IsBusy` 绑定按钮 IsEnabled；删除自动 `OpenReportsFolder()` 调用（Growl 中改为纯文字提示路径）；`CopyDirectoryIfExists` 静默 catch 改为 `Log.Warning` + 累计失败文件列表；XAML 左右列重新布局（左列加说明 TextBlock + 超链接 + 分隔线 + 灰色 hint，右列加 TooltipBlock + BusyStatusText）；恢复 commit `faee6a9333` 漏删的 8 个 `Diagnostic*`/`GenerateDiagnosticReport*` localization key（5 语 xaml 同步）+ 新增 11 个 tooltip/busy key；`DiagnosticInfo.cs` 扩展 `Parts` 字段 + `PartInfo` record（FileName / UncompressedSizeBytes / FileCount），向后兼容 |
| 关键实现 | `src/MaaWpfGui/Models/DiagnosticInfo.cs:31-33/117-155`（新增 Parts + PartInfo record + 字段间空行 SA1516）；`src/MaaWpfGui/ViewModels/UserControl/Settings/IssueReportUserControlModel.cs:49/58-62/96-124/163-178/222-617`（MaxPartSizeBytes 常量 + Lazy<List<DateRangeOption>> + IsBusy/IsNotBusy/BusyStatusText 三属性 + ClearImageCache 加注释 + GenerateSupportPayload 入口改 async void + 7 个拆分方法 + ExportContext/CopyResult record + catch IOException/UnauthorizedAccessException 不静默 + ShowGrowl* + ReportBusyStatus + SafeDelete）；`src/MaaWpfGui/Views/UserControl/Settings/IssueReportUserControl.xaml:46-130`（左右列重新布局 + TooltipBlock + BusyStatusText TextBlock）；5 语 `Res/Localizations/*.xaml`（恢复 8 个 key + 新增 11 个 key） |
| 生命周期 | 2026-08-16 创建（从 staging 拉出） → 2026-08-16 `--no-ff` 合入 `staging`（`97f66604dc`） |
| 关键 commit | `e2f00a360a`（fix(diagnostic-export): 重构日志导出 - 拆分方法 + 异步执行 + 统一分卷 + UX 增强，9 文件 +580 -217） |
| 子修复分支 | 无（独立 fix） |
| **关键发现** | staging 当前 5 语 xaml 缺失 8 个 `Diagnostic*` / `GenerateDiagnosticReport*` key，原因为 commit `faee6a9333`「fix(localization): 重建 5 语 xaml」时以 branch 干净版为基底，仅 cherry-pick 了 166ad9b5ae 和 94d1b16579 两个 hunks，**遗漏**了 25e201b4ad 的 hunks。运行时表现：当前 staging 部署的 MAA.exe 在「设置 → 问题反馈」页，「生成诊断报告」按钮等控件显示成 key 名（DynamicResource 解析失败）。本 fix 同步追加 19 个 key 修复 |
| **已知 TODO**（不在本 fix 范围） | SA1402（File may only contain a single type）：`DiagnosticInfo.cs` 含 6 个 type，原文件已违反 SA1402（DateFilterInfo/AppInfo/SysInfo/GpuInfo）；CS8632 nullable 注解：`DiagnosticInfo.cs` 未启用 `#nullable enable`；`ClearImageCache` 的 MessageBox `yes=Cancel`/`no=Confirm` 反向语义：保留行为，仅加注释（按方案决策避免引入回归） |
| 验证 | install-staging 部署（`MAA.exe` 时间戳与合并前一致因仅 C# / XAML 改动）；`dotnet build` 0 错误 / 60 warning（8 新增 CS8632 ×4 nullable 注解 + SA1516 ×4 + SA1512 ×2，52 预存 warning pre-existing）；信任 staging 验证（修改纯 WPF C# + XAML + 5 语 localization + DiagnosticInfo.cs 数据模型），后续 staging → branch 晋升需实测：启动不闪退 + 设置页问题反馈 Tab 渲染正常 + 点击生成诊断报告异步执行 + 大报告按大小统一切分 + 5 语 xaml 文案正确 |
| 作用域 | 仅本仓库 fork 私有，不推 upstream |
| 详见 | `LOG.md` 2026-08-16（fix/diagnostic-export-refactor 启动 / 实施完成 / 合入 staging 三段） |

### 7.18 fix/depot-special-memorial-tab（2026-09-07 已合入 staging）

| 项 | 内容 |
|----|------|
| 用途 | 修复「仓库识别」自 2026-09-05 起持续 TaskChainError：游戏在仓库页新增「特别纪念」tab 致标签栏重排，`DepotMaterialTab` / `DepotAllTab` 老模板与老 ROI 全部失配 |
| 根因 | 游戏侧 2026-09-04 前后内容更新加 tab（非版本更新，用户无感知）；同一 09-03 18:19 构建二进制 09-03 18:42 成功（`DepotMaterialTab.png` 0.9927）、09-05 起全失败；NCC 实测失败帧老模板仅 0.4613 / 0.2504 且最高分位置仍在原 ROI 内 → 标签栏位置未变、像素内容已变 |
| 修复 | 摘取上游 dev-v2 `da5ccfe4ed`（fix: 增加特别纪念 tab 后仓库识别报错，2026-09-04 16:48 +0800，**不在 v6.17.0 release 内**）+ `30ff4de669`（Auto Templates Optimization PNG 终版）：`tasks.json` 3 处 ROI（DepotAllTab `[452,0,281,134]`→`[450,0,300,138]`、DepotMaterialTab / DepotMaterialTabClicked `[1026,0,254,138]`→`[945,0,300,138]`）+ `resource/template/Depot/` 3 张 PNG 重截（77x34 / 131x33 / 120x34），仅资源零代码 |
| 生命周期 | 2026-09-07 创建（从 staging 拉出） → 2026-09-07 `--no-ff` 合入 `staging`（`f580d7ed6d`） |
| 关键 commit | `bba10f5675` |
| 子修复分支 | 无（独立 fix） |
| 验证 | 离线 NCC：新 `DepotMaterialTab` 对 09-05 / 09-07 失败帧均 0.9117（≥0.9 阈值）；`DepotAllTab` 失败帧摄于初始未点击态测不出高分属正常（信任上游实测）；部署后待实机跑「仓库识别」 |
| 上游收敛 | 上游下个 release（v6.17.1+）携带同一改动，届时 WORKFLOW.md 同步自然收敛，无冲突风险 |
| 作用域 | 仅资源同步上游修复，不推 upstream（上游已有） |
| 详见 | `LOG.md` 2026-09-07（启动 / 实施完成 / 合入 staging 三段）；排查方法论沉淀于 §3.5 |

### 7.19 feat/upstream-v617-sync（2026-08-13 已合入 staging，2026-08-30 晋升 branch）

| 项 | 内容 |
|----|------|
| 用途 | 拉取上游 v6.16.5 → v6.17.0 共 4467 commit 三方合并（fork base `c8c8e75be5` 无父节点，通过 `git replace --graft` 接 `6147357bd0` v6.14.0 release 走 3-way 合并）；解决基线冲突 + 上游 dev-v2 → master-v2 全量切换 + fork 私有 C++ 整体回归 master |
| 关键阶段 | Phase A 移除 `feat/auto-recruit-3star-to-4star` + Phase B AGENTS/CHANGELOG/csproj/.gitignore 清理 + Phase D 上游 4467 commit 合入；2026-08-07 master-v2 对齐放弃 fork 私有 C++ + tasks.json account-switch 区块回归 master + 恢复 expedite_min_level C++ |
| 生命周期 | 2026-07-30 创建（从 staging 拉出）→ 2026-08-07 全量完成（8 commit + 1 PR） → 2026-08-13 `706f8babf4` merge 上游 v6.16.5 + 8/30 三个 fix |
| 关键 commit | `a92a8a9124`（merge: upstream v6.17.0 三方合并 — 同步基线）→ `706f8babf4`（v6.16.5 跟进） → `a0f530912c`（staging → branch 晋升） |
| 子修复分支 | 无（合并主线 + 8/30 三个独立 fix 走 §7.10/§7.11/§7.12/§7.13） |
| 作用域 | 仅本仓库 fork 同步操作，不推 upstream |
| 详见 | `LOG.md` 2026-07-30 ~ 2026-08-13 全部阶段；§2.4 staging 内容历史记录 |

### 7.20 feat/sync-isolation（2026-09-03 已合入 staging）

| 项 | 内容 |
|----|------|
| 用途 | 抽离 fork 私有逻辑到独立 partial class 文件 + 编写 `tools/inject-recruitnow-slot.py` 任务 JSON Slot 注入脚本；为 v6.18 同步准备最小冲突面 |
| 关键实现 | `refactor(sync-isolation): 抽离 fork 私有逻辑到独立 partial class/脚本`（`9d79bc0dc4`，27 文件 +1289 -779）：C# 5 个 partial（`TaskQueueViewModel.AccountCycle.cs` / `ToolboxViewModel.AccountScopedData.cs` / `CopilotViewModel.CopilotSet.cs` / `RecruitTask.Expedite.cs` / `RecruitSettingsUserControlModel.Expedite.cs` / `StartUpSettingsUserControlModel.AccountCycle.cs`）+ WPF 配置向后兼容 + Python `tools/inject-recruitnow-slot.py`（pre-merge --remove + post-merge --inject）+ `tools/auto-resolve-xaml-conflict.py` 5-lang xaml 冲突自动处理 |
| 生命周期 | 2026-08-31 创建（从 staging 拉出） → 2026-09-03 `--no-ff` 合入 `staging`（`0d55649f63`） |
| 关键 commit | `9d79bc0dc4` |
| 子修复分支 | 无 |
| 验证 | `tools/auto-resolve-xaml-conflict.py` 跑通；`tools/inject-recruitnow-slot.py` remove/inject 单元级联测试通过；本仓库 fork 私有 C# 类全部移至独立 partial，主类可与 upstream 三方合并零冲突 |
| 作用域 | 仅本仓库 fork 私有重构（partial class 拆解），不推 upstream |
| 详见 | `LOG.md` 2026-09-03（启动 / 实施完成 / 合入 staging 三段） |

### 7.21 feat/fork-externalization（2026-08-30 已合入 staging）

| 项 | 内容 |
|----|------|
| 用途 | 上游/下游一键脚本（`update-upstream` + `update-downstream`）+ 流程文档命令补全；合并上游时减少手动 git 操作（重置 master / fetch / branch -f） |
| 关键实现 | `tools: 上游/下游一键脚本 (update-upstream + update-downstream) + 流程文档命令补全`（`b8f975fbf8`）：3 个文件 +286（Python 脚本 + WORKFLOW.md 命令补全）；后随 `e079795071` 追加 AGENTS/LOG 文档同步 |
| 生命周期 | 2026-08-25 创建（从 staging 拉出） → 2026-08-25 `--no-ff` 合入 `staging`（`52caa00cd6` merge） |
| 关键 commit | `b8f975fbf8`（工具实现） + `e079795071`（AGENTS/LOG 同步，2026-09-16 cherry-pick 验证为空 commit） |
| 子修复分支 | 无 |
| 作用域 | 仅本仓库 fork 私有工具 + 文档，不推 upstream |
| 详见 | `LOG.md` 2026-08-25（启动 / 实施完成 / 合入 staging 三段） |

### 7.22 feat/upstream-v618-sync（beta.1 2026-09-16 合入 staging；v6.18.0 release 跟进 2026-09-25 合入 staging）

| 项 | 内容 |
|----|------|
| 用途 | 拉取上游 v6.17.0 → v6.18.0-beta.1 共 170 commit 三方合并（master-v2 head `ec14df252ae` Release v6.18.0-beta.1）；含 12 项新功能（RunningState → RunControlState + RunOwner 运行时重构 / SwitchTheme / MaterialSynthesis / Yituliu OpenAPI / 基建副手换班 / Roguelike 黑流树海 / GPU 驱动信息 / Achievement 等） |
| 关键策略 | 阶段 0 仓库卫生 + cherry-pick `e079795071`（验证为空，§7.21 早合）+ 删 feat/* 三个本地分支 + master reset upstream + 备份；阶段 4-6 三方合并 + 12 文件手解（实际冲突面远小于预估 30-50，fork partial class 隔离生效）+ fork marker 12 项验证；阶段 7 C# API 迁移（Idle→GetIdle / HandleStopping 无参化 / SetStopped() 签名对齐）+ C++ 编译环境重建（VS 2026 实例消失 → portable-msvc 14.51 到 C:\msvc-portable + Ninja）472/472 成功；阶段 8-10 部署 + 冒烟 + 文档 |
| 关键冲突区 | HIGH: TaskQueueViewModel.cs / ToolboxViewModel.cs / IssueReportUserControlModel.cs（保留 fork §7.17 分卷重写）/ 5-lang xaml / tasks.json 11 处（Swipe specialParams ×8 + CharsNameOcrReplace + StartButton1 + InfrastSmiley + BattleSelectFormation + BattleAvatarDialog + 仓库 tab）/ AsstProxy→OnAllTasksCompleted 链路（WORKFLOW §6.5.5 直调描述过时） |
| 生命周期 | 2026-09-16 创建（从 staging 拉出） → 2026-09-16 merge `6d32144858` 合入 staging → 2026-09-25 merge `0a3884231d`（v6.18.0 release，134 commit）+ `1b7b087aae`（编译修复三连）合入 staging |
| 关键 commit | `6d32144858`（merge: upstream/master-v2 v6.18.0-beta.1 三方合并 — 同步基线）+ API 迁移修复 commit；`0a3884231d`（merge: upstream/master-v2 v6.18.0 → staging）+ `1b7b087aae`（fix(post-merge) 编译修复三连） |
| 子修复分支 | 无 |
| 验证 | dotnet build 0 错误；MaaCore 472/472 编译成功；install-staging 部署完成（STARTUP_HOOKS=libloader ✓）；启动冒烟 AsstLoadResource ret: true 无闪退；fork marker 12 项全命中；MaaCore.dll MD5 与 build 产物一致；8 项实机冒烟待用户日常验证。v6.18.0 跟进（09-25）：dotnet build 0 错误；MaaCore 249/249（含上游新增 AutoRaisePotentialTaskPlugin 等）；两轮冒烟（22:34/22:40）`ret: true` + 0 Unhandled/Fatal；post-merge-validate 7/7；merge-base = `23feeddc53` = 上游 release commit |
| 部署备注 | C++ 工具链实际位于 `E:\Tools\msvc-portable\msvc\`（`C:\msvc-portable` 为 junction 指向同处；MSVC 14.51.36231 + SDK 28000，env 模板 `setup_x64.bat` 以 `%~dp0` 自定位，两路径均可）；CMake 用 Ninja + cl（preset `windows-publish-x64` 会报 "generator platform: x64 Does not match"，须 `cmake -S . -B build -G Ninja -DCMAKE_BUILD_TYPE=RelWithDebInfo -DCMAKE_{C,CXX}_COMPILER=cl`）；上游 release 窗口 bump 的子模块须在 MaaCore 打包前 `git submodule update`（否则 DLL 用旧 MaaUtils 编出） |
| 作用域 | 仅本仓库 fork 三方同步操作，不推 upstream |
| 详见 | `V618_SYNC_PLAN.md`（项目根，压缩执行计划）；`LOG.md` 2026-09-16 阶段 0-10 + 2026-09-25 阶段 11 全部记录 |


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


## 9. 上游开发约定（v6.18.0-beta.1 新增，merge from upstream master-v2）

> 本节为 v6.18 上游新增的「MAA 开发约定」全量同步（原文 = 上游 `23feeddc53:AGENTS.md`），与 fork §1-§8 工作流并存。  
> 上游 pre-commit / StyleCop / clang-format 强约束，本仓库 pre-commit run --all-files 仍按 §5 既有规则运行。

### MAA 开发约定

格式由 `.pre-commit-config.yaml` 钩子机器强制，各钩子有 files 范围；本地不处理好的话 GitHub CI 会定时代跑并生成额外的机器修复 commit，为避免此副作用，改完文件后须本地跑对应格式化（如 C++ 的 clang-formatter）。

### C++（MaaCore）

- 格式化用 `python tools/ClangFormatter/clang-formatter.py --input=<改动文件或其所在子目录>`（`--input` 收目录递归或单个文件），只传本次改动的范围，确需全仓库批量时才传 `src/MaaCore`（全量幂等但白扫全仓库）；版本需求：clang-format 23.1.1（对齐 `.pre-commit-config.yaml` 锁定版本）；5 位以上整数字面量加千分位撇（`65'535`，存量尚有漏改，勿照抄）。
- 日志用 `"Utils/Logger.hpp"`，日志走 `<<` 流式格式，正文一律英文。旧代码中存在使用函数式日志的情况，勿照抄。合理使用 `__FUNCTION__`、`LogTraceFunction` 标注当前函数，避免手动在日志中标记。避免匿名命名空间，会导致在日志中输出为 `<anonymous namespace>`，不利于定位。
- 字符串一律 UTF-8 `std::string`，中文裸写、禁 `u8""` 前缀；文件路径必须经 `asst::utils::path()` 或 `"xxx"_p` 字面量构造，仅 Win32 API 边界转 `to_osstring`。
- 错误处理返回 `bool` / `std::optional` / 空指针并记日志，常规任务与配置链路不用异常。
- 避免返回裸指针，如有需要，使用引用或智能指针；引用返回值须保证生命周期，禁止返回局部变量引用。
- Json 序列化用 `meojson`，`#include <meojson/json.hpp>`。简单结构可考虑使用 `MEO_JSONIZATION` 宏生成序列化函数，如有复杂需求则需手写 `to_json` / `check_json` / `from_json`。检查 Json 字段、类型、范围、枚举值等，应在 `check_json` 中完成，`from_json` 返回 false 则会直接抛出异常，目前无返回 false 的情况。meojson 支持枚举的忽略大小写反序列化，但是如有 `_` 分隔符，则需要手动编写 `json::_reflection::enum_name_storage` 进行映射。

### C# / WPF（MaaWpfGui）

- 新 `.cs` 文件必须带 `stylecop.json` 模板规定的版权头，`file=` 值须等于实际文件名。
- MVVM 框架是 Stylet：对话框 VM 继承 `Screen`，Root 用 `Conductor<Screen>.Collection.OneActive`，UserControl 子模型与列表项继承 `PropertyChangedBase`，配置树类继承 `NotifyPropertyChangedWithValue`；交互用 `Command="{s:Action 方法名}"` 直绑 VM 方法，不自建 ICommand。
- 派生属性标 `[PropertyDependsOn(nameof(X))]` 并在构造函数调 `PropertyDependsOnUtility.InitializePropertyDependencies(this)`。
- `MaaWpfGui.Configuration.` 命名空间内的类用纯 auto-property（Fody PropertyChanged 织入通知），命名空间外织入不生效，必须手动 `SetAndNotify`，否则 UI 静默不刷新。
- 配置持久化与迁移归口 `ConfigFactory`（全局 `Root.Gui.X`、当前档案 `CurrentConfig.X`，保存自动防抖）；GUI 设置值的读取一律走 `SettingsViewModel.XxxSettings.X`（设置 VM 属性与配置双向同步，static 属性类型名限定），直取 `CurrentConfig` 仅限 `TaskQueue` 任务对象（多任务间切换当前设置需直读配置，其余设置一律走设置 VM）；新任务类型须在 `BaseTask` 注册 `[JsonDerivedType(typeof(XxxTask), typeDiscriminator: nameof(XxxTask))]`。
- 服务在 Bootstrapper 用 StyletIoC 显式注册，跨对象取用走 `Instances.Xxx` 静态定位器，不做构造函数注入。
- 回 UI 线程用 Stylet 的 `Execute.OnUIThread`（同步）或 `Execute.OnUIThreadAsync`；纯后台链路 await 加 `ConfigureAwait(false)`，后续要碰 UI 的不加。
- 日志用 Serilog `Log.ForContext<T>()`，结构化占位、消息英文；UI 可见日志另走 `Instances.TaskQueueViewModel.AddLog`（Copilot 相关链路用 `Instances.CopilotViewModel.AddLog`）。如有需要，类内声明`private static readonly ILogger _logger = Log.ForContext<T>();`，避免反复 `ForContext`。
- View 与 VM 命名镜像由 Stylet 按约定解析，放错命名空间运行时找不到 View：`XxxView` 对应 `XxxViewModel`，`XxxUserControl` 对应 `XxxUserControlModel`（VM 类名以 Model 结尾）。
- TaskQueue 任务对象类的命名参考为 `XxxTask`，VM 为 `XxxTaskViewModel`，View 为 `XxxTaskView`，UserControl 为 `XxxTaskUserControl`；Settings 配置类的命名参考为 `XxxSettings`，配置 VM 为 `XxxSettingsViewModel`，配置 View 为 `XxxSettingsView`。

### XAML 与本地化

- 界面文本一律用 `controls:TextBlock` 而非原版 `TextBlock`；例外是文本样式需跟随宿主控件模板的场景（超链接、下拉选中项、按钮 Content、DataTemplate 内部等），这些用原版 `TextBlock`，不要套统一样式，保持原生表现。
- 本地化 key 与主题 Brush 一律 `{DynamicResource ...}`，StaticResource 在语言或主题热切换后不刷新；引用本仓库自定义样式等不随主题/语言热切换的资源时用 StaticResource。
- 提示文案 key 加 `Tip` 后缀（`ForceScheduledStart` 与 `ForceScheduledStartTip`），句内复用其他 key 写 `{key=Xxx}` 内联。
- `TooltipBlock` 长文案按句界显式换行，一句一行即可；无句界的长句不必强行拆，由 TooltipText 默认 MaxWidth 500 的软换行兜底。资源串内写 `&#10;` 或字面换行均可，且必须声明 `xml:space="preserve"`，否则换行符被 XAML 归一化；各语言在自己句界处换行，行数不要求一致。
- 固定选项的下拉框用 `LocalizedObservableList<T>`（`src/MaaWpfGui/Utilities/ValueType/LocalizedObservableList.cs`），构造给 (值, 本地化 key)，勿手动 GetString 固化 Display（热切换语言后该项停留旧语言）；ViewModel 订阅 `LocalizationHelper.LanguageChanged` 并在回调里 `RefreshLocalization()`；增删用自带 Add/Remove/Insert/Clear，勿直接操作 Items。
- 设置页新增区块复用 Expander + Border + UserControl 既有结构（样板见 `SettingsView.xaml`），否则破坏搜索过滤与折叠持久化。

### 资源与工程文件

- `resource/tasks/tasks.json` 字段须在 TaskData 白名单内，自定义字段须配 `xxx_Doc` 注释字段，校验只在 `ASST_DEBUG` 加载期（Release 静默）；`next` 列表特殊引用用 `#` 前缀（`#self` / `#next` / `#back`）。
- MaaCore 源文件由 CMake `file(GLOB_RECURSE)` 自动收集，新增 `.h/.cpp` 无需登记 CMakeLists。
- 改 `Res/Localizations/*.xaml` 中 stub 相关文案（如 ErrorCongratulations）须同步 `src/MaaAppHostStub/MaaAppHostStub.cpp`，CI 有硬校验。

### 文档维护

- 改任一语言后同步其余四语言（zh-cn/zh-tw/en-us/ja-jp/ko-kr）的对应段落（行号未必对应）：语义一致即可，某语言本就缺段落则保持现状但提示。
- 文档中的 UI 均指 WPF UI，修改时不必处理 Avalonia（MAAUnified）等社区实现的相关描述；明日方舟 PC 端不进文档。
- 新活动/模式尚无官方译名时以各语言 UI 显示为准（见 `Res/Localizations/*.xaml`），UI 也无译文则写中文并括号加描述，勿自行发明译名。
- docs 新增文档须建五语言同名文件，frontmatter 带 `icon` 与 `order`（侧边栏排序）；站内链接用相对路径，CI 有死链检查。

### 发布 / 打包

- Windows 完整包由 `.github/workflows/ci.yml`（正式）与 `.github/workflows/release-nightly-ota.yml`（Nightly）两条工作流各自独立打包，凡改打包产物或发布流程**必须同时同步两条**；`release-ota.yml` / `release-package-distribution.yml` 只基于已上传产物做差分分发，通常无需随动。
- 禁止裸 `dotnet build -o build\bin\Debug`（不带 -r）：该目录是自包含产物，framework-dependent 的 runtimeconfig 与残留自包含宿主混装会致启动报错；重建 GUI 用 `dotnet publish -r win-x64 --self-contained true` 或 `dotnet build -r win-x64`。`dotnet publish` 不重建 MaaCore.dll，需单独 CMake 编译后放置。
- CHANGELOG 由发版时统一整理，日常改动勿自行添加条目。

### 测试

- MaaCore 本地测试一律 Debug 构建：加载期检查整体在 `ASST_DEBUG` 内，Release 下零错误是假象。
- MSB3026/MSB3021 构建复制失败时，挡构建的是占用仓库构建版 `build\bin\Debug\MAA.exe` 的进程（与安装版无关），临时验证程序引用 MaaWpfGui 时同理。
- 文档站在主仓库 `docs/` 下直接跑，日常验证改完文件看 `pnpm dev` 热重载即可，无必要不跑 build；非交互 install 加 `CI=true`。

## 10. 通用踩坑（v6.18 沉淀）

本节沉淀不专属于上游同步的踩坑事项，跨分支 / 跨场景复用。  
上游同步专用 SOP 见 [`WORKFLOW.md`](./WORKFLOW.md)（§5 graft / §6.5.x 冲突地图 / §8.2.1 NetBeauty / §11.1 8 维度总检）。

### 10.1 commit message 反斜杠转义陷阱

**症状**：

```
fatal: /: '/' is outside repository at 'C:/Users/.../FORK_MAA'
```

**根因**：PowerShell 5.1 + bash 混用 shell 时，commit message 含 `"`（双引号转义）会被 git 解析为路径 `/`，触发"outside repository"错误。

**触发场景**：commit message 内嵌文件路径或 key 字符串时，含 `"` 转义双引号。

**修复**：用 PowerShell here-string（单引号 `@'...'@`）定义变量再 `git commit -m $var`：

```powershell
$msg = @'
fix(localization): 补 CurrentAccountLabel 5 语 key

- TaskQueueView.xaml:80 引用 CurrentAccountLabel
- 5 语 xaml 各 +1 行 <system:String x:Key=CurrentAccountLabel>...</system:String>
'@
git commit -m $msg
```

**首次踩坑记录**：commit `16b11a0e31` 第一次尝试失败（消息含 `"ToolboxTabRecruitHistory"`），改用 here-string 后通过。

### 10.2 dead key 清理 SOP（§7.9 类型遗留）

**症状**：`tools/post-merge-validate.ps1` 报错 `ko-kr.xaml missing key: XxxXxx`，但 4/5 语 xaml 仍残留该 key。

**根因模式**：被回退的 feat/*（如 §7.9 `feat/recruit-history-tab`，2026-08-02 `a13825c68f`）引入的 localization key，C# 代码零引用但 xaml 字符串保留。ko-kr 因后续 PR 重构被清掉，其他 4 语残留。

**清理步骤**：

| # | 操作 | 命令 |
|---|------|------|
| 1 | 全仓 grep 残留 | `git grep -n "XxxKey" -- src/MaaWpfGui tools docs`（应为 0 命中） |
| 2 | 4 语 xaml 同步删除 | 逐语 `<script>` 工具脚本（参考 `tools/_add_current_account_label.py` 模式）或 `edit` 工具 |
| 3 | 同步 stale 检查项 | `tools/post-merge-validate.ps1` 与 `tools/auto-resolve-xaml-conflict.py` 同步删除 key 检查项 |
| 4 | 验证 | 重跑 `post-merge-validate.ps1` 应 0 missing key 报错；`git grep` 应仍 0 命中 |
| 5 | 重新部署 | xaml 编译进程序集（BAML），必须 `dotnet publish + nbeauty2` 才生效 |

**首次踩坑实例**：`ToolboxTabRecruitHistory`（§7.9 已回退 `feat/recruit-history-tab`）—— 2026-09-16 commit `16b11a0e31` 全清（4 语 xaml 各 -1 行 + 2 脚本 stale 项）。

### 10.3 修后冒烟 SOP

**触发条件**：任何改动 WPF xaml / C# 代码 / C++ 代码 / 资源 tasks.json 后，验证构建+运行无回归。

**5 步模板**：

| # | 步骤 | 预期 |
|---|------|------|
| 1 | 停旧进程 | `Get-Process MAA -ErrorAction SilentlyContinue \| Stop-Process -Force -ErrorAction SilentlyContinue` |
| 2 | 重新部署（如改 WPF / C++） | 见 [`WORKFLOW.md`](./WORKFLOW.md) §8.2（dotnet publish + nbeauty2 --usepatch） |
| 3 | 启动 | `Start-Process "install-staging\MAA.exe" -WorkingDirectory "install-staging"` |
| 4 | 等待 | `Start-Sleep -Seconds 18`（足够 LoadResource + 网络请求 tasks.json） |
| 5 | 验证三连 | `AsstLoadResource ret: true`（debug/gui.log 末行）+ 0 errors（`grep -c "Unhandled\|Fatal"`）+ 进程存活（`Get-Process MAA`） |
| 6 | 收尾 | `Stop-Process -Name MAA -Force` |

**首次踩坑实例**：2026-09-16 19:28 / 19:29 / 22:24 三次冒烟均通过 `ret: true`、0 错误、进程存活。

### 10.4 MAA HttpClient 代理配置：gui.new.json vs gui.json 陷阱

**症状**：设置了本地代理（Clash / SS / 其他）让 MAA 外部 HTTP（api.maa.plus / Penguin / Yituliu / GitHub）走代理，重启 MAA 后仍走默认直连；`gui.log` 无 `Rebuild HttpClient with proxy` 行；外部请求要么 `TaskCanceledException: Timeout 15s` 要么 `SSL EOF`。

**根因模式**：MAA 的新配置实际读 `install-staging/config/gui.new.json`（System.Text.Json `Root` 对象，`ConfigFactory.cs:43`），**不是** `install-staging/config/gui.json`（legacy Newtonsoft 扁平 key-value）。两者并存但语义不同：

| 文件 | 格式 | 读取方 | 代理字段路径 |
|------|------|--------|------------|
| `config/gui.new.json` | 嵌套对象（new） | `ConfigFactory.Root`（System.Text.Json，`ConfigFactory.cs:43`） | `Update.Proxy` / `Update.ProxyType`（顶层 `Update` 子对象） |
| `config/gui.json` | 扁平 key-value（legacy） | `ConfigurationHelper`（Newtonsoft，`ConfigurationHelper.cs:317`） | `Global.VersionUpdate.Proxy` / `Global.VersionUpdate.ProxyType` |

迁移代码（`Helper/ConfigConverter.cs:81-83`）只对 legacy 配置触发，不会把 `Global.VersionUpdate.Proxy` 自动搬到 `Root.Update.Proxy`。new-format 配置彻底绕过 migration。

**正确做法**：

| # | 操作 | 命令 |
|---|------|------|
| 1 | 启动 MAA 后查代理是否挂载 | `Select-String -Path install-staging/debug/gui.log -Pattern 'Rebuild HttpClient with proxy'` |
| 2 | 若无日志，编辑正确文件 | `install-staging/config/gui.new.json`，找 `"Proxy": ""`，改为 `"Proxy": "127.0.0.1:7897"` |
| 3 | JSON 不破坏校验 | `[System.IO.File]::ReadAllText('install-staging/config/gui.new.json') \| ConvertFrom-Json` 应成功，`$result.Update.Proxy` 应为代理地址 |
| 4 | 重启 MAA | HttpService 是单例（`Bootstrapper.cs:902 InSingletonScope`），构造时一次挂代理；改配置必须重启 |
| 5 | 验证 | 重启后 gui.log 应有 `[INF][HttpService] Rebuild HttpClient with proxy Http://127.0.0.1:7897` |

**代理格式**：`Proxy` 不含 `://` 时自动加 `ProxyType + "://"` 前缀（`HttpService.cs:52`）。例：`"127.0.0.1:7897" + ProxyType="Http"` → `Http://127.0.0.1:7897`；SOCKS5 填 `127.0.0.1:7897` + `ProxyType=Socks5`。

**首次踩坑记录**：2026-09-19 — fork 用户报 MAA 跑日常 Penguin/Yituliu 报数失败（直连 SSL EOF）。在 `gui.json` 的 `Global` 加 `VersionUpdate.Proxy` 键失败，日志无 `Rebuild HttpClient with proxy`。根因即上表 new vs legacy 路径混淆。改 `gui.new.json` 的 `Update.Proxy` 字段后 1 分钟内日志出现代理行，api.maa.plus 返回 304。**Proxy / ProxyType 字段位置在 `gui.new.json` 顶层 `"Update": { ... }` 子对象里，不是顶层散落，也不是 `Global` 子对象里**。

### 10.5 MaaUtils 子模块 bump 与第三方 DLL 同步（2026-09-25 沉淀）

**症状**：合并上游后新构建的 MAA 跑「开始唤醒」即 `TaskChainError OutOfMemory`（`Unhandled out of memory in task thread`），GPU / CPU 模式均复现；机器内存充裕（RAM 5.4/15.8 GB 空、VRAM 1183/8188 MiB），显非真 OOM。

**根因**：第三方依赖 DLL 的真身在 **`src/MaaUtils` 子模块内**（`MaaUtils/MaaDeps/runtime/maa-x64-windows/`，vcpkg 安装树 `MaaUtils/MaaDeps/vcpkg/installed/maa-x64-windows/`）。子模块 bump（如 v6.18.0 同步 `0c2556cf` → `6e9ba33`）会**整套替换** onnxruntime / fastdeploy / opencv 的 ABI：新 MaaCore 链接新 import lib，而 `install-staging/` 若仍留旧 DLL（cmake --install 未生效或手动部署漏拷贝），运行时半匹配 → `OcrPack::check_and_load` 内 DBDetector/Recognizer 构造抛 `bad_alloc`。

**定位技巧**（LogTraceFunction RAII 特性）：

| # | 手段 | 判读 |
|---|------|------|
| 1 | 对比失败帧 `asst.log`：`check_and_load \| enter` 后无 `det 1 rec 1 ocr 1` | `LogTraceFunction` 是 RAII 析构打印，异常穿栈也打 leave；无 det/rec/ocr 行 = 异常抛在三个模型加载之内 |
| 2 | `Compare-Object` build/bin 与 install-staging 的 DLL `Name+Length` | 尺寸差 = 部署漏同步（本次 onnxruntime 80,622,080 vs 18,998,784） |
| 3 | 修复 | `Copy-Item build\bin\{onnxruntime_maa,fastdeploy_ppocr_maa,opencv_world4_maa,opencv_world4d_maa,DirectML}.dll install-staging\ -Force` 后重测 |

**防复发**：`tools/post-merge-validate.ps1` 第 [8] 项已加入 7 个 DLL 尺寸一致性检查（含 MaaCore.dll / MaaUtils.dll）。凡动过 `src/MaaUtils` 子模块指针的合并，部署后必须重跑该脚本。

**首次踩坑记录**：2026-09-25 v6.18.0 release 同步（阶段 12）——`0a3884231d` merge + 子模块 bump 后 22:49/22:59/23:01 三次 OOM；23:05 尺寸对比定位，23:09 同步 DLL 后 `det 1 rec 1 ocr 1`（3091ms）恢复，全任务链跑通。详见 `LOG.md` 2026-09-25 阶段 12。

---

**§10 与其它章节互引**：
- §10.1 通用 → 所有 commit 操作前
- §10.2 dead key → 任何 feat/* / fix/* 回退后
- §10.3 冒烟 → 所有修改后必跑
- §10.4 代理配置 → 任何「设置本地代理走 Clash」类需求
- §10.5 子模块 bump → 任何 `git submodule update` / MaaUtils 指针变更后的部署与验证
- 上游同步专项 → [`WORKFLOW.md`](./WORKFLOW.md)
