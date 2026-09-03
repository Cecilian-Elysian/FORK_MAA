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

本倉庫是 [MaaAssistantArknights/MaaAssistantArknights](https://github.com/MaaAssistantArknights/MaaAssistantArknights) 的**個人增強型本地分叉**。
關注多帳號輪換、上游基線跟進、當地化构建與驗證，不接受上游直接合併；Fork 私有功能在 `feat/<name>` / `fix/<name>` 分支逐項演進，由 `staging` → `branch` 流程整合。

一款明日方舟遊戲小助手

基於圖像辨識技術，一鍵完成全部日常任務！

绝赞更新中 ✿✿ヽ(°▽°)ノ✿

:::

## 關於本 Fork

| 項 | 說明 |
|----|------|
| 上游 | [MaaAssistantArknights/MaaAssistantArknights](https://github.com/MaaAssistantArknights/MaaAssistantArknights) `master-v2`（穩定 release 分支） |
| 鏡像 | 本地 `master` 始終指向 `upstream/master-v2`，透過 `tools/update-upstream.ps1` 同步 |
| 同步 | 每次上游發布版本（如 v6.17.0），先建立 `feat/upstream-<version>-sync` 解決基線衝突，再派生子分支落地 Fork 改造 |
| Fork 私有功能 | 帳號輪換、帳號資料分桶、公招加急門檻、會客室線索回退、診斷報告、`expedite_min_level` 協定擴充等，全部來自 `branch` 分支的本地改造 |
| 文件 | 倉庫根 [README](../../README.md) + [AGENTS.md](../../AGENTS.md) + [WORKFLOW.md](../../WORKFLOW.md) + [LOG.md](../../LOG.md) |

> 不要將 `upstream/master-v2` 直接合併到 `staging` 或 `branch`。Fork 流程細節請參考 [WORKFLOW.md](../../WORKFLOW.md) §5（假歷史關聯）與 §6（合併手解）。

## 下載與安裝

請閱讀 [文檔](./manual/newbie.md) 後前往 [官網](https://maa.plus) 或 [Releases](https://github.com/MaaAssistantArknights/MaaAssistantArknights/releases) 下載，並參考 [新手上路](./manual/newbie.md) 進行安裝。

本 Fork 與上游安裝方式完全一致；若 `branch` 與上游版本差異較大，請優先使用上游預編譯產物，再按 [WORKFLOW.md](../../WORKFLOW.md) 自建本地版本。

## 亮點功能

- 刷理智，掉落辨識及上傳 [企鵝物流](https://penguin-stats.io/)，[一圖流](https://ark.yituliu.cn/)
- 智慧基建換班，自動計算幹員效率，支援跨設施組合；同時也支援 [自訂排班](./protocol/base-scheduling-schema.md)
- 自動公招，可選使用加急許可，一次全部刷完！公招數據自動上傳 [企鵝物流](https://penguin-stats.io/result/stage/recruit/recruit)，[一圖流](https://ark.yituliu.cn/survey/maarecruitdata)
- 支援手動辨識公招介面，方便對高星公招做出選擇 ~~（你的這個高資回費出的是推進之王呢還是推進之王呢）~~
- 支援辨識幹員列表，統計已有和未有幹員及潛能，並在公招辨識顯示
- 支援辨識養成材料，並匯出至 [企鵝物流刷圖規劃](https://penguin-stats.io/planner)、[明日方舟工具箱](https://arkntools.app/#/material)、[ARK-NIGHTS 幹員培養表](https://ark-nights.com/settings)
- 造訪好友、獲取信用及購物、領取日常獎勵等，一鍵全日常自動長草
- 肉鴿全自動刷源石錠和等級，自動燒水和凹直升，智慧辨識幹員及練度
- 選擇作業 JSON 檔案，自動抄作業， [影片展示](https://www.bilibili.com/video/BV1H841177Fk/)
- 支援 C, Python, Java, Rust, Golang, Java HTTP, Rust HTTP 等多種介面，方便整合呼叫，自訂你的 MAA！

### Fork 專屬增強

- **多帳號自動輪換**（`feat/account_rotation`）：日常任務按帳號序列自動執行；肉鴿與生息演算延後到最後（`feat/defer-rogue`）
- **幹員 / 倉庫辨識資料按帳號分桶**（`feat/account-scoped-recognition-data`）：切號即切桶，不會出現跨帳號庫存合併
- **公招加急門檻**（`feat/expedite-threshold`）：Fork 協定欄位 `expedite_min_level`，僅在確認最低招募星級 ≥ 閾值（4 / 5 / 6）時使用加急許可
- **公招加急按目標槽位點擊**（`fix/recruit-expedite-slot-target`）：多槽位同時進行時不會串位
- **「生成診斷報告」重構**（`fix/diagnostic-export-refactor`）：分卷匯出、系統資訊收集、非同步執行
- **會客室線索快捷置入失敗回退**（`fix/reception-clue-restore`）：修復上游 issue #16165
- **本地構建工具鏈增強**：`tools/local-install-staging.bat`、VS 2022 BuildTools 相容路徑、NetBeauty2 後處理

### Fork 私有功能的關鍵檔案清單

- `src/MaaWpfGui/ViewModels/UI/TaskQueueViewModel.AccountCycle.cs`：帳號輪換編排
- `src/MaaWpfGui/ViewModels/Orchestration/AccountCycleOrchestrator.cs`：編排者
- `src/MaaWpfGui/ViewModels/UI/ToolboxViewModel.cs` 的 `#region AccountScopedRecognitionData`：帳號資料分桶
- `src/MaaWpfGui/Models/DiagnosticInfo.cs`：診斷報告模型
- `src/MaaCore/Task/Infrast/InfrastReceptionTask.cpp` 的 `proc_clue_vacancy`：會客室線索快捷置入回退
- `src/MaaCore/Task/Miscellaneous/AutoRecruitTask.{h,cpp}` 與 `src/MaaCore/Task/Interface/RecruitTask.cpp`：`expedite_min_level` 閾值判定

話不多說，看圖！

<ImageGrid :imageList="[
  {
    light: 'images/zh-tw/readme/1-light.png',
    dark: 'images/zh-tw/readme/1-dark.png'
  },
  {
    light: 'images/zh-tw/readme/2-light.png',
    dark: 'images/zh-tw/readme/2-dark.png'
  },
  {
    light: 'images/zh-tw/readme/3-light.png',
    dark: 'images/zh-tw/readme/3-dark.png'
  },
  {
    light: 'images/zh-tw/readme/4-light.png',
    dark: 'images/zh-tw/readme/4-dark.png'
  }
]" />

## 使用說明

### 功能介紹

請參閱 [使用者手冊](./manual/)。

### 外服支援

目前國際服（美服）、日服、韓服、繁中服的絕大部分功能均已支援。  
但由於外服使用者較少及項目人手不足，很多功能並沒有進行全面的測試，所以請自行體驗。  
若您遇到了 Bug，或對某個功能有強烈需求，歡迎在 [Issues](https://github.com/MaaAssistantArknights/MaaAssistantArknights/issues) 和 [討論區](https://github.com/MaaAssistantArknights/MaaAssistantArknights/discussions) 催更；或加入我們一起建設 MAA！請參閱 [外服適配教學](#外服適配)。

### CLI 支援

MAA 支援命令列介面（CLI）操作，支援 Linux，macOS 和 Windows，可用於自動化指令碼或在無圖形介面的伺服器上使用。  
請參閱 [CLI 使用指南](./manual/cli/)。

## 加入我們

### 主要關聯專案

- 全新框架：[MaaFramework](https://github.com/MaaXYZ/MaaFramework)
- [作業站](https://prts.plus) 前端：[zoot-plus-frontend](https://github.com/ZOOT-Plus/zoot-plus-frontend)
- [作業站](https://prts.plus) 後端：[ZootPlusBackend](https://github.com/ZOOT-Plus/ZootPlusBackend)
- [官網](https://maa.plus)：[前端](https://github.com/MaaAssistantArknights/maa-website)
- 深度學習：[MaaAI](https://github.com/MaaAssistantArknights/MaaAI)
- 本 Fork 倉庫：[Cecilian-Elysian/FORK_MAA](https://github.com/Cecilian-Elysian/FORK_MAA)

### 多語言 (i18n)

MAA 以中文（簡體）為第一語言，翻譯詞條均以中文（簡體）為準。

### 參與開發

請參閱 [開發指南](./develop/development.md) 與倉庫根 [AGENTS.md](../../AGENTS.md) / [WORKFLOW.md](../../WORKFLOW.md)。本 Fork 流程重點：

- 修改前查閱 [`docs/downstream-changes.md`](../../docs/downstream-changes.md) 確認 Fork 改動清單
- 每個新功能用獨立 `feat/<name>` / `fix/<name>` 分支，避免跨域改動混在同一衝突集
- Fork 私有功能集中在 `src/MaaWpfGui/Models`、`TaskQueueViewModel.AccountCycle.cs`、`ToolboxViewModel.cs` 與 `src/MaaCore/Task/Miscellaneous/AutoRecruitTask.*` 等熱點檔案

### API

- [C 介面](../../include/AsstCaller.h)：[整合範例](../../src/Cpp/main.cpp)
- [Python 介面](../../src/Python/asst/asst.py)：[整合範例](../../src/Python/sample.py)
- [Golang 介面](../../src/Golang)：[整合範例](../../src/Golang/maa/maa.go)
- [Dart 介面](../../src/Dart)
- [Java 介面](../../src/Java/src/main/java/com/iguigui/maaj/easySample/MaaCore.java)：[整合範例](../../src/Java/src/main/java/com/iguigui/maaj/easySample/MaaJavaSample.java)
- [Java HTTP 介面](../../src/Java/Readme.md)
- [Rust 介面](../../src/Rust/src/maa_sys)：[HTTP 介面](../../src/Rust)
- [TypeScript 介面](https://github.com/MaaAssistantArknights/MaaX/tree/main/packages/main/coreLoader)
- [Woolang 介面](../../src/Woolang/maa.wo)：[整合範例](../../src/Woolang/demo.wo)
- [整合文件](./protocol/integration.md)
- [回呼訊息協定](./protocol/callback-schema.md)
- [任務流程協定](./protocol/task-schema.md)
- [自動抄作業協定](./protocol/copilot-schema.md)

### 外服適配

請參閱 [外服適配教學](./develop/overseas-client-adaptation.md)，對於官服已支援的功能，絕大部分的外服適配工作僅需要截圖 + 簡單的 JSON 修改即可。

### Issue bot

請參閱 [Issue Bot 使用方法](./develop/issue-bot-usage.md)。

## 致謝

### 開源庫

- 影像辨識庫：[opencv](https://github.com/opencv/opencv.git)
- ~~文字辨識庫：[chineseocr_lite](https://github.com/DayBreak-u/chineseocr_lite.git)~~
- 文字辨識庫：[PaddleOCR](https://github.com/PaddlePaddle/PaddleOCR)
- 深度學習佈署庫：[FastDeploy](https://github.com/PaddlePaddle/FastDeploy)
- 機器學習加速器：[onnxruntime](https://github.com/microsoft/onnxruntime)
- ~~關卡掉落辨識：[企鵝物流辨識](https://github.com/penguin-statistics/recognizer)~~
- 地圖格子辨識：[Arknights-Tile-Pos](https://github.com/yuanyan3060/Arknights-Tile-Pos)
- C++ JSON 庫：[meojson](https://github.com/MistEO/meojson.git)
- C++ 運算子解析器：[calculator](https://github.com/kimwalisch/calculator)
- ~~C++ base64 編解碼：[cpp-base64](https://github.com/ReneNyffenegger/cpp-base64)~~
- C++ 解壓縮庫：[zlib](https://github.com/madler/zlib)
- C++ Gzip 封裝：[gzip-hpp](https://github.com/mapbox/gzip-hpp)
- Android 觸控事件器：[Minitouch](https://github.com/DeviceFarmer/minitouch)
- Android 觸控事件器：[MaaTouch](https://github.com/MaaAssistantArknights/MaaTouch)
- WPF MVVM 框架：[Stylet](https://github.com/canton7/Stylet)
- WPF 控制項庫：[HandyControl](https://github.com/HandyOrg/HandyControl) -> [HandyControls](https://github.com/ghost1372/HandyControls)
- C# 記錄檔：[Serilog](https://github.com/serilog/serilog)
- C# JSON 庫：[Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json) & [System.Text.Json](https://github.com/dotnet/runtime)
- ~~下載器：[aria2](https://github.com/aria2/aria2)~~

### 資料來源

- ~~公開招募數據：[明日方舟工具箱](https://www.bigfun.cn/tools/aktools/hr)~~
- ~~幹員及基建數據：[PRTS Wiki](http://prts.wiki/)~~
- 關卡數據：[企鵝物流數據統計](https://penguin-stats.io/)
- 遊戲數據及資源：[明日方舟客戶端素材](https://github.com/yuanyan3060/ArknightsGameResource)
- 遊戲數據：[《明日方舟》Yostar 遊戲數據](https://github.com/ArknightsAssets/ArknightsGamedata)

### 貢獻 / 參與者

感謝所有參與到開發 / 測試中的朋友們，是大家的幫助讓 MAA 越來越好！ (\*´▽｀)ノノ

[![Contributors](https://contributors-img.web.app/image?repo=Cecilian-Elysian/FORK_MAA&max=105&columns=15)](https://github.com/Cecilian-Elysian/FORK_MAA/graphs/contributors)<br>
[上游貢獻者](https://github.com/MaaAssistantArknights/MaaAssistantArknights/graphs/contributors)

## 聲明

- 本軟體使用 [GNU Affero General Public License v3.0 only](https://spdx.org/licenses/AGPL-3.0-only.html) 開源，並附帶額外 [用戶協議](https://github.com/MaaAssistantArknights/MaaAssistantArknights/blob/dev-v2/terms-of-service.md)。
- 本軟體 logo 並非使用 AGPL 3.0 協議開源，[耗毛](https://weibo.com/u/3251357314)、vie 兩位畫師及軟體全體開發者保留所有權利。不得以 AGPL 3.0 協議已授權為由在未經授權的情況下使用本軟體 logo，不得在未經授權的情況下將本軟體 logo 用於任何商業用途。
- 本軟體開源、免費，僅供學習交流使用。若您遇到商家使用本軟體進行代練並收費，可能是設備與時間等費用，產生的問題及後果與本軟體無關。

### DirectML 支援說明

本軟體支援 GPU 加速功能，其在 Windows 平台上依賴於 Microsoft 提供的獨立元件 [DirectML](https://learn.microsoft.com/en-us/windows/ai/directml/)。  
DirectML 並非本專案的開源部分，也不受 AGPL 3.0 的約束。  
為方便使用者使用，我們隨安裝檔附帶了一個未經修改的 DirectML.dll 檔案。  
如果您無需 GPU 加速功能，可安全刪除該 DLL 檔案，軟體的核心功能仍可正常執行。

## 廣告

使用者交流 QQ 群：[MAA 使用 & 粥遊交流 QQ 群](https://api.maa.plus/MaaAssistantArknights/api/qqgroup/index.html)  
Discord 伺服器：[Discord 邀請連結](https://discord.gg/23DfZ9uA4V)  
使用者交流 TG 群：[Telegram 群](https://t.me/+Mgc2Zngr-hs3ZjU1)  
自動戰鬥 JSON 作業分享：[prts.plus](https://prts.plus)  
Bilibili 直播間：[MrEO 直播間](https://live.bilibili.com/2808861) 直播敲程式碼 & [MAA-Official 直播間](https://live.bilibili.com/27548877) 遊戲 / 雜談

技術群（舟無關、禁閒聊）：[內卷地獄！(QQ 群)](https://jq.qq.com/?_wv=1027&k=ypbzXcA2)  
開發者群：[QQ 群](https://jq.qq.com/?_wv=1027&k=JM9oCk3C)

如果覺得軟體對你有幫助，幫忙點個 Star 吧！~（網頁最上方右上角的小星星），這就是對我們最大的支持了！
