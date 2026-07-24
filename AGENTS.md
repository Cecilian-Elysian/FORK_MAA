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
| `branch` | **本地下游整合** | 所有 feat / fix 最终合并至此 |
| `feat/<name>` | 新功能 | 从 `branch` 拉出 |
| `fix/<name>` / `fix/<name>/<n>` | 修复分支 | **必须从对应 `feat/<name>` 拉出**（详见 `§3.3`） |

### 2.3 已完结 feat 处理

feat 合并到 `branch` 后：

| 操作 | 说明 |
|------|------|
| 本地 | `git branch -d feat/<name>`（已合入，安全删除） |
| 远端 | 保留不删（便于回溯 / cherry-pick / 行为对比） |
| 记录 | 写入本文件 `§7` 与 `LOG.md` |


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
| 1 | `git switch -c feat/<name> branch` | 新分支 |
| 2 | `LOG.md` 追加「`feat/<name>` 启动」表格 | 启动记录 |
| 3 | 实施期间 commit message 记录关键决策 | commit 历史 |
| 4 | 实施完成 `LOG.md` 追加「`feat/<name>` 实施完成」表格（文件路径 + 行号 + commit） | 实施记录 |
| 5 | 编译 / 部署验证 | `install/` |
| 6 | FF 合并到 `branch`（无分叉时）；分叉时 `--no-ff` | merge commit |
| 7 | `LOG.md` 记录合并事件，按 `§2.3` 处理 feat 分支 | 生命周期 |

### 3.3 fix 分支命名与合并目标

| 约束 | 说明 |
|------|------|
| 来源 | fix **必须从被修复的 `feat/<name>` 拉出**；或从 `branch` 拉出修复 `branch` 自身 |
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
| `release-zip.{bat,ps1}` | 一键打包 zip |
| `add_maa_to_nahimic_whitelist.ps1` | MAA.exe 加入 Nahimic DLL 注入白名单 |
| `disable_nahimic.ps1` | 停用 NahimicService 开机自启 |
| `cmake_build_for_wpf.bat` | 仅触发 cmake 的 WPF 构建 |
| `maadeps-download.py` | 依赖库下载 |
| `ClangFormatter/` | clang-format 集成 |
| `OverseasClients/`、`Roguelike*/`、`SmokeTesting/`、`SyncTemplate/`、`TaskSorter/` | 功能性辅助工具 |


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
|------|------|---------|
| `fix/expedite-threshold` | 修复 `feat/expedite-threshold` 重构时遗失的 `m_last_confirmed_min_level` 重置,导致下一槽位读到上一槽位陈旧星级而误加急;同步 `fix/account-official-recognize` (cherry-pick from `branch`);StartUp 双重缓冲清理;StartUp::run 恢复原序 + 切号链 OCR 兜底（修多账号鹰角弹窗场景 20s 等待） | `branch` 自身（修复 `7df4e94e3f` 重构遗漏） |


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