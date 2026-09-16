#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Append 2026-09-16 v6.18 同步阶段 1-10 完成日志到 LOG.md (UTF-16 LE)."""

LOG_PATH = 'LOG.md'

new_section = """
### feat/upstream-v618-sync 合并执行（阶段 1-10）

| 阶段 | 操作 | 说明 |
|------|------|------|
| 1 | WORKFLOW §5-§6 基线核对 | 12 项 fork marker grep 全命中；上游未删 RecruitNow@Slot 4 变体，阶段 2-3 pre-merge remove/inject 跳过 |
| 4 | `git merge --no-commit --no-ff upstream/master-v2` | 触发 12 文件冲突（预估 30-50，fork partial class 隔离生效，冲突面大幅缩小） |
| 5.A | AGENTS.md | 保留 fork §1-§8 全部内容 + 追加上游「开发约定」章节作为新 §9（C++/C#/XAML/资源/文档/发布/测试七大块） |
| 5.B | CHANGELOG.md | 保留 fork 头部章节 + 上游 v6.18 段并入 |
| 5.C | tools/local-install.bat | 保留 ours `10.0.100` + `rollForward:latestFeature` |
| 5.D | ko-kr.xaml | 接 upstream（fork §7.9 残留 Recruit* key 全仓无引用，安全丢弃） |
| 5.E | StartUpTaskUserControl.xaml | 保留 fork AccountCycle 区块 + 上游 Run.Idle TouchMode ComboBox |
| 5.F | CopilotView.xaml | 保留 fork ClipboardLinkSet20Regular + PasteClipboardCopilotSet + 上游 Run.Idle |
| 5.G | CopilotViewModel.cs | 注释差异，保留 theirs |
| 5.H | TaskQueueViewModel.cs | 3 处：coreTaskIds 收集 fork SetTaskIds 保留 + ClearCurrentCycleAccountName 保留 + lateStageOn 下沉 partial |
| 5.I | ToolboxViewModel.cs | 5 处：StartOperBox 混合（fork SwitchDataAccount 前置 + 上游 Yituliu 分流/TryGetTaskBlockReason/BeginRun）+ Peep GetIdle + _operBoxDataSource + StartOperBoxFromYituliuApiAsync 全量接上游 |
| 5.J | IssueReportUserControlModel.cs | 保留 fork §7.17 SplitIntoParts 重写（含 diagnostic.json Parts 回填），丢弃上游旧版尾部 try/catch 残留 |
| 5.K | UserDataUpdateSettingsUserControlModel.cs | 接上游 `using Serilog` |
| 5.L | tasks.json | 11 处冲突手解：SlowlySwipeToTheLeft/Right/Up/Down specialParams `[200,1,2,0]→[200,1,37,1]` + SwipeToThe 四方向 `[150,0,1,1]→[150]` + CharsNameOcrReplace（fork 零私有改动，取上游 `[^\n]*` 防跨行优化 + 埃癸斯）+ StartButton1（上游删 preDelay:500）+ InfrastSmiley 三合一（代码侧零 InfrastSmileyOn* 引用）+ BattleSelectFormation 宽 10→5 + BattleAvatarDialog roi `[70,30,120,30]→[70,30,150,500]` + 仓库 tab ROI 与 fork §7.18 摘取的上游 da5ccfe4ed 数值一致自然收敛 |
| 6 | merge commit `6d32144858` | 「merge: upstream/master-v2 v6.18.0-beta.1 三方合并 — 同步基线」，风格仿 `a92a8a9124` |
| 6.5 | fork marker 12 项 post-merge 验证 | 全命中：AdvanceAccountCycle=3 / start-race=1 / expedite_min_level C++=8 / RecruitNow@Slot=4 / CurrentCycleAccountName=3 / SwitchDataAccount=3 / ClipboardLinkSet20Regular=3 / PasteClipboardCopilotSet=7 文件 / AccountCycleOrchestrator 存在 / 5 partial 文件全在 / AccountCycleTip 5 语 / AsstProxy→OnAllTasksCompleted→AdvanceAccountCycle 链路完整（WORKFLOW §6.5.5 描述的 AsstProxy L1313 直调已过时，现走 OnAllTasksCompleted 回调） |
| 7 | C# API 迁移 | `_runningState.Idle` 属性引用 4 处（TaskQueueViewModel LinkStartWithTasks + FightSettingsUserControlModel×3）→ `GetIdle()` 对齐上游；`HandleStopping(bool runStopScript)` → `HandleStopping()`（上游 SetStopped 移除 runStopScript 参数，手动/自动以 GetStopping() 区分）；partial L257/285 `SetStopped(runStopScript: false)` → `SetStopped()`；LinkStartWithTasks 补回 merge 时丢失的 lateStageOn/currentPhase 定义行；3 处 `_runningState.SetIdle(true)` 轻量复位保留（无 Core 队列需清，SetStopped 反而过重） |
| 7 | dotnet build | 0 错误（CS1668 LIB 路径 + CS8632 nullable 均为 pre-existing） |
| 7 | C++ 编译环境重建 | VS 2026 实例消失（原 E 盘路径已删，vswhere/注册表无记录，全盘无 MSVC toolset 残留）；Windows SDK 10.0.28000 仍在 E:\\Windows Kits；GCC/MinGW 不可行（vcpkg maa-x64-windows = MSVC ABI，OpenCV/onnxruntime/PaddleOCR 无法跨 ABI 链接）；选定 portable-msvc.py（mmozeiko，微软官方 manifest 解析）下载 MSVC 14.51.36231 + SDK 28000 到 `C:\\msvc-portable\\msvc\\`（479MB 下载，纯文件解包零安装，与 MSYS2 GCC 并存零冲突）；`E:\\msvc-setup\\refresh-msvc-env.ps1` 改指 C 盘新路径并刷新用户级环境变量；pip install ninja 1.13.2；删除失效 build/ 后 `cmake -G Ninja -DCMAKE_BUILD_TYPE=RelWithDebInfo -DCMAKE_C/CXX_COMPILER=cl` configure 成功（OpenCV 4.11.0/onnxruntime/zlib/tiff 全部命中 vcpkg maadeps） |
| 7 | `cmake --build build --target MaaCore` | 472/472 成功 0 错误，`bin\\MaaCore.dll` 5,844,992 bytes（含 v6.18 BlackFlow/JieGarden 新 Vision 模块） |
| 7 | `cmake --build build` + MAA.Updater | 全部目标 + Updater.exe 成功 |
| 7 | `cmake --install build --prefix install-staging` | 首次 Permission denied（MAA.exe PID 20564 运行中占用）→ Stop-Process 后成功部署 MaaCore.dll + MAA.Updater.exe + 头文件 |
| 7 | dotnet publish + nbeauty2 | publish 成功；nbeauty2 --usepatch 静默成功；MAA.runtimeconfig.json 含 STARTUP_HOOKS=libloader ✓ |
| 7 | robocopy resource /MIR | exit 1（正常，有文件复制） |
| 8 | 启动冒烟 | MAA.exe 存活无闪退；`AsstLoadResource ret: true`；在线资源 cache 加载 NotModified 正常；关闭进程后复核 |
| 9 | 终验 | RecruitNow@Slot=4 / expedite_min_level=8 / AccountCycleTip 5 语=5 / install-staging MaaCore.dll 与 build 产物 MD5 一致（CCFCEFDE868C6C2385CD5CB565F413E9） |
| 10 | 文档收尾 | AGENTS §2.4/§7.22 状态更新 + CHANGELOG 完整段 + 本表格 + downstream 清单重生成 |

**遗留事项**：
- 实机 8 项冒烟（多账号切号/公招加急/仓库识别/基建换班/一图流 OpenAPI/任务时长上限/切号数据桶/诊断报告）待用户日常使用验证
- VSCode/CMake 用户注意：后续 C++ 编译需在设置 INCLUDE/LIB/PATH 后跑 cmake（新开 shell 用户级环境变量已生效，可直接 `cmake --build build`）
- `_resolve_swipe_conflict.py` / `_cmp_ocr.py` / `_chk_predelay.py` 三个一次性排查脚本留在 tools/（下划线前缀，勿引用）

**状态**：staging @ merge commit `6d32144858` + 修复 commit（见下），feat/upstream-v618-sync 使命完成，待删除本地分支（远端保留）。

"""

with open(LOG_PATH, 'rb') as f:
    data = f.read()

new_bytes = new_section.encode('utf-16-le')

if data.startswith(b'\xff\xfe'):
    body = data[2:]
else:
    body = data

with open(LOG_PATH, 'wb') as f:
    f.write(b'\xff\xfe' + body + new_bytes)

print(f"Appended {len(new_bytes)} bytes")
