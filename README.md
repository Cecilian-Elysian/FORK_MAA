<!-- markdownlint-disable -->

<div align="center">

<img alt="LOGO" src="./docs/.vuepress/public/images/maa-logo_512x512.png" width="256" height="256" />

# MAA Assistant Arknights · Cecilian-Elysian Fork

<br>
<div>
    <img alt="C++" src="https://img.shields.io/badge/C++-20-%2300599C?logo=cplusplus">
    <img alt="platform" src="https://img.shields.io/badge/platform-Windows%20%7C%20Linux%20%7C%20macOS-blueviolet">
</div>
<div>
    <img alt="license" src="https://img.shields.io/github/license/Cecilian-Elysian/FORK_MAA">
    <img alt="upstream" src="https://img.shields.io/badge/upstream-MaaAssistantArknights%2FMaaAssistantArknights-181717?logo=github">
</div>
<div>
    <a href="https://github.com/Cecilian-Elysian/FORK_MAA/stargazers"><img src="https://img.shields.io/github/stars/Cecilian-Elysian/FORK_MAA?style=social"></a>
    <a href="https://github.com/Cecilian-Elysian/FORK_MAA/commits"><img src="https://img.shields.io/github/commit-activity/m/Cecilian-Elysian/FORK_MAA?color=%23ff69b4"></a>
</div>

[简体中文](https://docs.maa.plus/zh-cn/) | [繁體中文](https://docs.maa.plus/zh-tw/) | [English](https://docs.maa.plus/en-us/) | [日本語](https://docs.maa.plus/ja-jp/) | [한국어](https://docs.maa.plus/ko-kr/)

> **MAA = MAA Assistant Arknights**
>
> 本仓库是 [`MaaAssistantArknights/MaaAssistantArknights`](https://github.com/MaaAssistantArknights/MaaAssistantArknights) 的**个人增强型本地分叉**。
>
> Fork 关注多账号轮换、上游基线跟进、本地化构建与验证，不接受上游直接合并。Fork 私有功能在 `feat/<name>` / `fix/<name>` 分支逐项演进，由 `staging` → `branch` 流程整合，详见 [`AGENTS.md`](./AGENTS.md) 和 [`WORKFLOW.md`](./WORKFLOW.md)。

</div>

## 1. 与上游的关系

| 项 | 说明 |
|----|------|
| 上游 | [`MaaAssistantArknights/MaaAssistantArknights`](https://github.com/MaaAssistantArknights/MaaAssistantArknights) `master-v2`（稳定 release 分支） |
| 镜像 | 本地 `master` 始终指向 `upstream/master-v2`，通过 `tools/update-upstream.ps1` 同步 |
| 同步 | 每次上游发布版本（如 v6.17.0），先创建 `feat/upstream-<version>-sync` 解决基线冲突，再派生子分支落地 Fork 改造 |
| Fork 私有功能 | 账号轮换、账号数据分桶、公招加急门槛、会客室线索回退、诊断报告、`expedite_min_level` 协议扩展等，全部来自 `branch` 分支的本地改造 |

> 不要将 `upstream/master-v2` 直接合并到 `staging` 或 `branch`。Fork 流程细节请参考 [`WORKFLOW.md`](./WORKFLOW.md) §5（假历史关联）和 §6（合并手解）。

## 2. 安装与下载

请参考上游用户文档完成安装：

- [新手上路](https://docs.maa.plus/zh-cn/manual/newbie.html)
- [官网下载](https://maa.plus)
- [GitHub Releases](https://github.com/MaaAssistantArknights/MaaAssistantArknights/releases)

本 Fork 与上游安装方式完全一致；如果当前 `branch` 与上游版本差异较大，请优先使用上游预编译产物，再按本仓库 `WORKFLOW.md` 自建本地版本。

## 3. 本地构建

`AGENTS.md` §4 与 `WORKFLOW.md` §8 列出完整命令，本机环境如使用 Visual Studio 2022 BuildTools（避开 Visual Studio 18 2026 的 CMake 预设），可直接复用仓库自带的 `tools/local-install-staging.bat` 或手工拼接构建步骤。

最小化的本地构建示例（仅供参考）：

```powershell
# 1) 同步 master 到上游 master-v2（仅当需要把 master 拉新时）
powershell -ExecutionPolicy Bypass -File tools\update-upstream.ps1

# 2) 在临时目录配置 VS 2022 BuildTools 兼容的 CMake 构建
cmake -G "Visual Studio 17 2022" -A x64 -S . `
    -B "$env:TEMP\maa-build" `
    -DMAADEPS_TRIPLET=maa-x64-windows `
    -DBUILD_WPF_GUI=ON -DBUILD_DEBUG_DEMO=OFF -DBUILD_RESOURCE_UPDATER=ON `
    -DINSTALL_RESOURCE=OFF -DINSTALL_PYTHON=OFF

# 3) 构建 C++ / 更新器并安装到 install-staging/
cmake --build "$env:TEMP\maa-build" --target MaaCore MAA.Updater res_updater --config RelWithDebInfo
cmake --install "$env:TEMP\maa-build" --config RelWithDebInfo --prefix install-staging

# 4) 发布 WPF（使用仓库自带的 global.json 锁版本 10.0.100 + rollForward latestFeature）
dotnet restore src/MaaWpfGui/MaaWpfGui.csproj
dotnet publish src/MaaWpfGui/MaaWpfGui.csproj -c Release -r win-x64 -o install-staging /p:DisableBeauty=True

# 5) NetBeauty2 后处理（漏跑会导致 libloader 启动闪退，详见 AGENTS §4.1.1）
& "$env:USERPROFILE\.nuget\packages\nulastudio.netbeauty\2.1.5\tools\win-x86\nbeauty2.exe" --usepatch "$PWD\install-staging\." .\externals

# 6) 同步资源
robocopy .\resource .\install-staging\resource /MIR /MT:8
```

如果仅需增量构建，可以跳过 CMake configure 直接执行 `cmake --build`。

## 4. 项目结构

| 目录 | 内容 | 说明 |
|------|------|------|
| `src/MaaCore/` | C++20 核心 | 任务、识别、接口、回调 |
| `src/MaaWpfGui/` | C# / .NET 10 WPF | UI + VM + 五语本地化 + 配置 |
| `src/MaaUpdater/` | C++ / Win32 | 启动/更新协调器 |
| `src/MaaUtils/` | C++ 通用工具（[MaaXYZ/MaaUtils](https://github.com/MaaXYZ/MaaUtils)） | Windows 主构建依赖 |
| `src/maa-cli/` | CLI 子项目（上游） | 命令行调用入口 |
| `src/MaaMacGui/` | macOS GUI 子项目（上游） | macOS 端 UI |
| `3rdparty/EmulatorExtras/` | 模拟器附加能力 | LDPlayer / MuMu12 增强 |
| `resource/` | 任务流 JSON、模板图片、活动资源 | 同步对象随上游版本更新 |
| `docs/` | vuepress 文档源 | 跟随上游 docs.maa.plus 同步 |
| `tools/` | 维护与构建脚本 | 构建与验证说明见 `AGENTS.md` §4 |

Fork 私有扩展主要在：

- `src/MaaWpfGui/ViewModels/UI/TaskQueueViewModel.AccountCycle.cs`（账号轮换编排）
- `src/MaaWpfGui/ViewModels/Orchestration/AccountCycleOrchestrator.cs`（编排者）
- `src/MaaWpfGui/ViewModels/UI/ToolboxViewModel.cs` 的 `#region AccountScopedRecognitionData`（账号数据分桶）
- `src/MaaWpfGui/Models/DiagnosticInfo.cs`（诊断报告模型）
- `src/MaaCore/Task/Infrast/InfrastReceptionTask.cpp` 的 `proc_clue_vacancy`（会客室线索快捷置入回退）
- `src/MaaCore/Task/Miscellaneous/AutoRecruitTask.{h,cpp}` 与 `src/MaaCore/Task/Interface/RecruitTask.cpp`（`expedite_min_level` 阈值判定）

## 5. Fork 私有功能速览

### 5.1 多账号自动轮换（`feat/account_rotation` + 后续 fix）

- 全部账号跑完基础任务后再依次执行肉鸽与生息演算（`feat/defer-rogue`）
- 切号即切桶：干员/仓库识别数据按账号分桶存放（`feat/account-scoped-recognition-data`）
- 修复切号 UI 残留、任务面板不刷新与「未知错误」误报（`fix/account_rotation/6`、`fix/account-cycle-start-race`）
- 单账号 section 已被账号轮换吸收，UI 永久可见的删除按钮带二次确认（`fix/account-rotation-supersede-switcher`）

### 5.2 公招加急门槛与按槽位点击（`feat/expedite-threshold` + `fix/recruit-expedite-slot-target`）

- `expedite_min_level` Fork 协议字段：仅在确认最低招募星级 ≥ 阈值（4 / 5 / 6）时使用加急许可
- 多槽位同时进行时，按目标槽位点击避免「三星加急、四星留 9h」

### 5.3 诊断报告（`feat/diagnostic-export`）

- `IssueReportUserControlModel.cs` 中的 `GenerateSupportPayload()` 重构：单一职责拆分 + 异步执行 + 20 MB 单卷分卷
- `DiagnosticInfo.cs`：诊断报告模型，包含 `Parts` 字段与 `PartInfo` record（FileName / UncompressedSizeBytes / FileCount）

### 5.4 会客室线索修复（`fix/reception-clue-restore`）

- `proc_clue_vacancy` 在快捷置入失败时不再直接 `return true`，而是回退至 legacy 逐位放置流程
- 上游 issue #16165（`closed as not planned`）至 v6.16.8 仍未修；本修复保留为 Fork  侧修复

详细维护历史与重要 commit 列表见 [`AGENTS.md`](./AGENTS.md) §6/§7。

## 6. 分支模型与贡献

| 分支 | 角色 | 规则 |
|------|------|------|
| `master` | 上游镜像 | 仅 `git fetch upstream` + `git reset --hard upstream/master-v2`，不推送 |
| `feat/upstream-<v>-sync` | 上游同步基线 | 仅解决基线冲突，改造工作按领域拆子分支 |
| `branch` | 下游稳定基线 | 接受 `staging` 的晋升，构建产物写入 `install/` |
| `staging` | 待验证整合区 | 所有 `feat/*`、`fix/*` 合入此处，构建到 `install-staging/` |
| `feat/<name>` | 新功能 | 从 `staging` 拉出，合入 `staging` |
| `fix/<name>` / `fix/<name>/<n>` | 修复分支 | 必须从对应 `feat/<name>` 拉出（详见 `WORKFLOW.md`） |

> **不要把 `master` 直接合并到 `staging` 或 `branch`。**
> 所有变更通过 commit message + [`LOG.md`](./LOG.md) + [`AGENTS.md`](./AGENTS.md) §7 跟踪；本仓库不使用 `feat_<name>.md` / `fix_<name>[_<n>].md` 工作笔记。

贡献流程参考 [`WORKFLOW.md`](./WORKFLOW.md) §6 / §7 / §8。修改高敏感文件前请查阅 [`docs/downstream-changes.md`](./docs/downstream-changes.md)。

## 7. 文档与协议

| 文档 | 位置 |
|------|------|
| 任务流程协议 | [`docs/zh-cn/protocol/task-schema.md`](./docs/zh-cn/protocol/task-schema.md)（五语均在 `docs/{lang}/protocol/`） |
| 回调消息协议 | [`docs/zh-cn/protocol/callback-schema.md`](./docs/zh-cn/protocol/callback-schema.md) |
| 自动战斗协议 | [`docs/zh-cn/protocol/copilot-schema.md`](./docs/zh-cn/protocol/copilot-schema.md) |
| 集成协议 | [`docs/zh-cn/protocol/integration.md`](./docs/zh-cn/protocol/integration.md) |
| 五语入口 | [`docs/zh-cn/`](./docs/zh-cn/)、[`docs/zh-tw/`](./docs/zh-tw/)、[`docs/en-us/`](./docs/en-us/)、[`docs/ja-jp/`](./docs/ja-jp/)、[`docs/ko-kr/`](./docs/ko-kr/) |
| 上游站点 | [docs.maa.plus](https://docs.maa.plus/) |

Fork 协议字段 `expedite_min_level` 与上游 `expedite` / `expedite_times` 并存，五语 `integration.md` 同时呈现。

## 8. 测试与验证

每次上游同步后必跑：

```powershell
# 1) 资源与模板结构
py -m json.tool resource\tasks\tasks.json > $null
py -m json.tool resource\global\YoStarKR\resource\tasks\tasks.json > $null

# 2) Fork 标记与保护校验
powershell -ExecutionPolicy Bypass -File tools\post-merge-validate.ps1

# 3) 构建并冒烟
Get-Process MAA -ErrorAction SilentlyContinue | Stop-Process -Force
& .\install-staging\MAA.exe
```

冒烟通过项参考 `AGENTS.md` §2.4 与 `LOG.md` 历次晋升记录。

## 9. 致谢

- 上游项目：[MaaAssistantArknights/MaaAssistantArknights](https://github.com/MaaAssistantArknights/MaaAssistantArknights) 与其贡献者
- 框架：[MaaXYZ/MaaFramework](https://github.com/MaaXYZ/MaaFramework)
- 作业站：[prts.plus](https://prts.plus)（[frontend](https://github.com/ZOOT-Plus/zoot-plus-frontend) / [backend](https://github.com/ZOOT-Plus/ZootPlusBackend)）
- 训练数据：[明日方舟工具箱](https://arkntools.app/)、[企鹅物流](https://penguin-stats.cn/)、[一图流](https://ark.yituliu.cn/)
- Fork 私有逻辑实现与适配：见 [`AGENTS.md`](./AGENTS.md) §7 与 [`LOG.md`](./LOG.md)

## 10. 声明

- 本 Fork 继承上游 [GNU Affero General Public License v3.0 only](https://spdx.org/licenses/AGPL-3.0-only.html) 及附加 [用户协议](./terms-of-service.md)。
- 软件 logo 不受 AGPL 3.0 授权；[耗毛](https://weibo.com/u/3251357314)、vie 两位画师及软件全体开发者保留所有权利。未经许可，不得用于商业用途。
- 软件仅供学习交流使用。若您遇到商家使用本软件进行代练并收费，可能涉及设备与时间等费用，产生的问题及后果与本软件无关。
- DirectML 并非本项目的开源部分，也不受 AGPL 3.0 约束。安装包附带的 `DirectML.dll` 为方便用户附带，并非本项目代码；无需 GPU 加速可安全删除。
