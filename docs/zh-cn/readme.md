---
icon: ic:round-home
index: true
dir:
  order: 0
---

<!-- markdownlint-disable -->

::: center

<!-- markdownlint-restore -->

![MAA Logo =256x256](/images/maa-logo_512x512.png)

# MAA Assistant Arknights · Cecilian-Elysian Fork

![C++](https://img.shields.io/badge/C++-20-%2300599C?logo=cplusplus)  
![platform](https://img.shields.io/badge/platform-Windows%20%7C%20Linux%20%7C%20macOS-blueviolet)  
![license](https://img.shields.io/github/license/Cecilian-Elysian/FORK_MAA) ![upstream](https://img.shields.io/badge/upstream-MaaAssistantArknights%2FMaaAssistantArknights-181717?logo=github)
![stars](https://img.shields.io/github/stars/Cecilian-Elysian/FORK_MAA?style=social) ![commit](https://img.shields.io/github/commit-activity/m/Cecilian-Elysian/FORK_MAA?color=%23ff69b4)
<a href="https://github.com/Cecilian-Elysian/FORK_MAA/commits/feat/upstream-v617-sync"><img src="https://img.shields.io/badge/sync-v6.17.0-blue?logo=git" alt="sync v6.17.0"></a>

MAA 的意思是 MAA Assistant Arknights

本仓库是 [MaaAssistantArknights/MaaAssistantArknights](https://github.com/MaaAssistantArknights/MaaAssistantArknights) 的**个人增强型本地分叉**。
关注多账号轮换、上游基线跟进、本地化构建与验证，不接受上游直接合并；Fork 私有功能在 `feat/<name>` / `fix/<name>` 分支逐项演进，由 `staging` → `branch` 流程整合。

基于图像识别技术，一键完成全部日常任务！

绝赞更新中 ✿✿ヽ(°▽°)ノ✿

:::

## 关于本 Fork

| 项 | 说明 |
|----|------|
| 上游 | [MaaAssistantArknights/MaaAssistantArknights](https://github.com/MaaAssistantArknights/MaaAssistantArknights) `master-v2`（稳定 release 分支） |
| 镜像 | 本地 `master` 始终指向 `upstream/master-v2`，通过 `tools/update-upstream.ps1` 同步 |
| 同步 | 每次上游发布版本（如 v6.17.0），先创建 `feat/upstream-<version>-sync` 解决基线冲突，再派生子分支落地 Fork 改造 |
| Fork 私有功能 | 账号轮换、账号数据分桶、公招加急门槛、会客室线索回退、诊断报告、`expedite_min_level` 协议扩展等，全部来自 `branch` 分支的本地改造 |
| 文档 | 仓库根 [README](../../README.md) + [AGENTS.md](../../AGENTS.md) + [WORKFLOW.md](../../WORKFLOW.md) + [LOG.md](../../LOG.md) |

> 不要将 `upstream/master-v2` 直接合并到 `staging` 或 `branch`。Fork 流程细节请参考 [WORKFLOW.md](../../WORKFLOW.md) §5（假历史关联）和 §6（合并手解）。

## 下载与安装

请阅读 [文档](./manual/newbie.md) 后前往 [官网](https://maa.plus) 或 [Releases](https://github.com/MaaAssistantArknights/MaaAssistantArknights/releases) 下载，并参考 [新手上路](./manual/newbie.md) 进行安装。

本 Fork 与上游安装方式完全一致；如果 `branch` 与上游版本差异较大，请优先使用上游预编译产物，再按 [WORKFLOW.md](../../WORKFLOW.md) 自建本地版本。

## 亮点功能

- 理智作战，掉落识别及上传 [企鹅物流](https://penguin-stats.cn/)，[一图流](https://ark.yituliu.cn/)
- 智能基建换班，自动计算干员效率，支持跨设施组合；同时也支持 [自定义排班](./protocol/base-scheduling-schema.md)
- 自动公招，可选使用加急许可，一次全部刷完！公招数据自动上传 [企鹅物流](https://penguin-stats.cn/result/stage/recruit/recruit)，[一图流](https://ark.yituliu.cn/survey/maarecruitdata)
- 支持手动识别公招界面，方便对高星公招做出选择 ~~（你的这个高资回费出的是推王呢还是推王呢）~~
- 支持识别干员列表，统计已有和未有干员及潜能，并在公招识别显示
- 支持识别养成材料，并导出至 [企鹅物流刷图规划](https://penguin-stats.cn/planner)、[明日方舟工具箱](https://arkntools.app/#/material)、[ARK-NIGHTS 干员培养表](https://ark-nights.com/settings)
- 访问好友、收取信用及购物、领取日常奖励等，一键全日常自动长草
- 肉鸽全自动刷源石锭和等级，自动烧水和凹直升，智能识别干员及练度
- 选择作业 JSON 文件，自动抄作业， [视频演示](https://www.bilibili.com/video/BV1H841177Fk/)
- 支持 C, Python, Java, Rust, Golang, Java HTTP, Rust HTTP 等多种接口，方便集成调用，自定义你的 MAA！

### Fork 专属增强

- **多账号自动轮换**（`feat/account_rotation`）：日常任务按账号序列全自动执行；肉鸽与生息演算延后到最后（`feat/defer-rogue`）
- **干员 / 仓库识别数据按账号分桶**（`feat/account-scoped-recognition-data`）：切号即切桶，不会出现跨账号库存合并
- **公招加急门槛**（`feat/expedite-threshold`）：Fork 协议字段 `expedite_min_level`，仅在确认最低招募星级 ≥ 阈值（4 / 5 / 6）时使用加急许可
- **公招加急按目标槽位点击**（`fix/recruit-expedite-slot-target`）：多槽位同时进行时不会串位
- **「生成诊断报告」重构**（`fix/diagnostic-export-refactor`）：分卷导出、系统信息收集、异步执行
- **会客室线索快捷置入失败回退**（`fix/reception-clue-restore`）：修复上游 issue #16165
- **本地构建工具链增强**：`tools/local-install-staging.bat`、VS 2022 BuildTools 兼容路径、NetBeauty2 后处理

### Fork 私有功能的关键文件清单

- `src/MaaWpfGui/ViewModels/UI/TaskQueueViewModel.AccountCycle.cs`：账号轮换编排
- `src/MaaWpfGui/ViewModels/Orchestration/AccountCycleOrchestrator.cs`：编排者
- `src/MaaWpfGui/ViewModels/UI/ToolboxViewModel.cs` 的 `#region AccountScopedRecognitionData`：账号数据分桶
- `src/MaaWpfGui/Models/DiagnosticInfo.cs`：诊断报告模型
- `src/MaaCore/Task/Infrast/InfrastReceptionTask.cpp` 的 `proc_clue_vacancy`：会客室线索快捷置入回退
- `src/MaaCore/Task/Miscellaneous/AutoRecruitTask.{h,cpp}` 与 `src/MaaCore/Task/Interface/RecruitTask.cpp`：`expedite_min_level` 阈值判定

话不多说，看图！

<ImageGrid :imageList="[
  {
    light: 'images/zh-cn/readme/1-light.png',
    dark: 'images/zh-cn/readme/1-dark.png'
  },
  {
    light: 'images/zh-cn/readme/2-light.png',
    dark: 'images/zh-cn/readme/2-dark.png'
  },
  {
    light: 'images/zh-cn/readme/3-light.png',
    dark: 'images/zh-cn/readme/3-dark.png'
  },
  {
    light: 'images/zh-cn/readme/4-light.png',
    dark: 'images/zh-cn/readme/4-dark.png'
  }
]" />

## 使用说明

### 功能介绍

请参阅 [用户手册](./manual/)。

### 外服支持

目前国际服（美服）、日服、韩服、繁中服的绝大部分功能均已支持。但由于外服用户较少及项目人手不足，很多功能并没有进行全面的测试，所以请自行体验。  
若您遇到了 Bug，或对某个功能有强需求，欢迎在 [上游 Issues](https://github.com/MaaAssistantArknights/MaaAssistantArknights/issues) 和 [讨论区](https://github.com/MaaAssistantArknights/MaaAssistantArknights/discussions) 催更；或加入我们一起建设 MAA！请参阅 [外服适配教程](#外服适配)

### CLI 支持

MAA 支持命令行界面（CLI）操作，支持 Linux，macOS 和 Windows，可用于自动化脚本或在无图形界面的服务器上使用。请参阅 [CLI 使用指南](./manual/cli/)

## 加入我们

### 主要关联项目

- 全新框架：[MaaFramework](https://github.com/MaaXYZ/MaaFramework)
- [作业站](https://prts.plus) 前端：[zoot-plus-frontend](https://github.com/ZOOT-Plus/zoot-plus-frontend)
- [作业站](https://prts.plus) 后端：[ZootPlusBackend](https://github.com/ZOOT-Plus/ZootPlusBackend)
- [官网](https://maa.plus)：[前端](https://github.com/MaaAssistantArknights/maa-website)
- 深度学习：[MaaAI](https://github.com/MaaAssistantArknights/MaaAI)
- 本 Fork 仓库：[Cecilian-Elysian/FORK_MAA](https://github.com/Cecilian-Elysian/FORK_MAA)

### 多语言 (i18n)

MAA 以中文（简体）为第一语言，翻译词条均以中文（简体）为准。

### 参与开发

请参阅 [开发指南](./develop/development.md) 与仓库根 [AGENTS.md](../../AGENTS.md) / [WORKFLOW.md](../../WORKFLOW.md)。本 Fork 流程关键点：

- 修改前查阅 [`docs/downstream-changes.md`](../../docs/downstream-changes.md) 确认 Fork 改动清单
- 每个新功能用独立 `feat/<name>` / `fix/<name>` 分支，避免跨域改动混在同一冲突集
- Fork 私有功能集中放在 `src/MaaWpfGui/Models`、`TaskQueueViewModel.AccountCycle.cs`、`ToolboxViewModel.cs` 与 `src/MaaCore/Task/Miscellaneous/AutoRecruitTask.*` 等热点文件

### API

- [C 接口](../../include/AsstCaller.h)：[集成示例](../../src/Cpp/main.cpp)
- [Python 接口](../../src/Python/asst/asst.py)：[集成示例](../../src/Python/sample.py)
- [Golang 接口](../../src/Golang)：[集成示例](../../src/Golang/maa/maa.go)
- [Dart 接口](../../src/Dart)
- [Java 接口](../../src/Java/src/main/java/com/iguigui/maaj/easySample/MaaCore.java)：[集成示例](../../src/Java/src/main/java/com/iguigui/maaj/easySample/MaaJavaSample.java)
- [Java HTTP 接口](../../src/Java/Readme.md)
- [Rust 接口](../../src/Rust/src/maa_sys)：[HTTP 接口](../../src/Rust)
- [TypeScript 接口](https://github.com/MaaAssistantArknights/MaaX/tree/main/packages/main/coreLoader)
- [Woolang 接口](../../src/Woolang/maa.wo)：[集成示例](../../src/Woolang/demo.wo)
- [集成文档](./protocol/integration.md)
- [回调消息协议](./protocol/callback-schema.md)
- [任务流程协议](./protocol/task-schema.md)
- [自动战斗协议](./protocol/copilot-schema.md)

### 外服适配

请参阅 [外服适配教程](./develop/overseas-client-adaptation.md)，对于国服已支持的功能，绝大部分的外服适配工作仅需要截图 + 简单的 JSON 修改即可。

### Issue bot

请参阅 [Issue Bot 使用方法](./develop/issue-bot-usage.md)

## 致谢

### 开源库

- 图像识别库：[opencv](https://github.com/opencv/opencv.git)
- ~~文字识别库：[chineseocr_lite](https://github.com/DayBreak-u/chineseocr_lite.git)~~
- 文字识别库：[PaddleOCR](https://github.com/PaddlePaddle/PaddleOCR)
- 深度学习部署库：[FastDeploy](https://github.com/PaddlePaddle/FastDeploy)
- 机器学习加速器：[onnxruntime](https://github.com/microsoft/onnxruntime)
- ~~关卡掉落识别：[企鹅物流识别](https://github.com/penguin-statistics/recognizer)~~
- 地图格子识别：[Arknights-Tile-Pos](https://github.com/yuanyan3060/Arknights-Tile-Pos)
- C++ JSON 库：[meojson](https://github.com/MistEO/meojson.git)
- C++ 运算符解析器：[calculator](https://github.com/kimwalisch/calculator)
- ~~C++ base64 编解码：[cpp-base64](https://github.com/ReneNyffenegger/cpp-base64)~~
- C++ 解压压缩库：[zlib](https://github.com/madler/zlib)
- C++ Gzip 封装：[gzip-hpp](https://github.com/mapbox/gzip-hpp)
- 安卓触控事件器：[Minitouch](https://github.com/DeviceFarmer/minitouch)
- 安卓触控事件器：[MaaTouch](https://github.com/MaaAssistantArknights/MaaTouch)
- WPF MVVM 框架：[Stylet](https://github.com/canton7/Stylet)
- WPF 控件库：[HandyControl](https://github.com/HandyOrg/HandyControl) -> [HandyControls](https://github.com/ghost1372/HandyControls)
- C# 日志：[Serilog](https://github.com/serilog/serilog)
- C# JSON 库：[Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json) & [System.Text.Json](https://github.com/dotnet/runtime)
- ~~下载器：[aria2](https://github.com/aria2/aria2)~~

### 数据源

- ~~公开招募数据：[明日方舟工具箱](https://www.bigfun.cn/tools/aktools/hr)~~
- ~~干员及基建数据：[PRTS Wiki](http://prts.wiki/)~~
- 关卡数据：[企鹅物流数据统计](https://penguin-stats.cn/)
- 游戏数据及资源：[明日方舟客户端素材](https://github.com/yuanyan3060/ArknightsGameResource)
- 游戏数据：[《明日方舟》Yostar游戏数据](https://github.com/ArknightsAssets/ArknightsGamedata)

### 贡献/参与者

感谢所有参与到开发/测试中的朋友们，是大家的帮助让 MAA 越来越好！ (\*´▽｀)ノノ

[![Contributors](https://contributors-img.web.app/image?repo=Cecilian-Elysian/FORK_MAA&max=105&columns=15)](https://github.com/Cecilian-Elysian/FORK_MAA/graphs/contributors)<br>
[上游贡献者](https://github.com/MaaAssistantArknights/MaaAssistantArknights/graphs/contributors)

## 声明

- 本 Fork 继承 [GNU Affero General Public License v3.0 only](https://spdx.org/licenses/AGPL-3.0-only.html) 与上游 [用户协议](../../terms-of-service.md)。
- 本软件 logo 并非使用 AGPL 3.0 协议开源，[耗毛](https://weibo.com/u/3251357314)、vie 两位画师及软件全体开发者保留所有权利。不得以 AGPL 3.0 协议已授权为由在未经授权的情况下使用本软件 logo，不得在未经授权的情况下将本软件 logo 用于任何商业用途。
- 本软件开源、免费，仅供学习交流使用。若您遇到商家使用本软件进行代练并收费，可能是设备与时间等费用，产生的问题及后果与本软件无关。

### DirectML 支持说明

本软件支持 GPU 加速功能，其在 Windows 平台上依赖于 Microsoft 提供的独立组件 [DirectML](https://learn.microsoft.com/en-us/windows/ai/directml/)。DirectML 并非本项目的开源部分，也不受 AGPL 3.0 的约束。为方便用户，我们随安装包附带了一个未经修改的 DirectML.dll 文件。如果您无需 GPU 加速功能，可安全删除该 DLL 文件，软件的核心功能仍可正常运行。

## 广告

用户交流 QQ 群：[MAA 使用 & 粥游交流 QQ 群](https://api.maa.plus/MaaAssistantArknights/api/qqgroup/index.html)  
Discord 服务器: [邀请链接](https://discord.gg/23DfZ9uA4V)  
用户交流 TG 群：[Telegram 群](https://t.me/+Mgc2Zngr-hs3ZjU1)  
自动战斗 JSON 作业分享：[prts.plus](https://prts.plus)  
Bilibili 直播间：[MrEO 直播间](https://live.bilibili.com/2808861) 直播敲代码 & [MAA-Official 直播间](https://live.bilibili.com/27548877) 游戏/杂谈

技术群（舟无关、禁水）：[内卷地狱！(QQ 群)](https://jq.qq.com/?_wv=1027&k=ypbzXcA2)  
开发者群：[QQ 群](https://jq.qq.com/?_wv=1027&k=JM9oCk3C)

如果觉得软件对你有帮助，帮忙点个 Star 吧！~（网页最上方右上角的小星星），这就是对我们最大的支持了！
