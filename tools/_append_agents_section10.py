#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Append AGENTS.md §10 通用踩坑 + WORKFLOW.md §10-§11 cross-reference."""

AGENTS_PATH = "AGENTS.md"

section_10 = """

## 10. 通用踩坑（v6.18 沉淀）

本节沉淀不专属于上游同步的踩坑事项，跨分支 / 跨场景复用。  
上游同步专用 SOP 见 [`WORKFLOW.md`](./WORKFLOW.md)（§5 graft / §6.5.x 冲突地图 / §8.2.1 NetBeauty / §11.1 8 维度总检）。

### 10.1 commit message 反斜杠转义陷阱

**症状**：

```
fatal: /: '/' is outside repository at 'C:/Users/.../FORK_MAA'
```

**根因**：PowerShell 5.1 + bash 混用 shell 时，commit message 含 `\"`（双引号转义）会被 git 解析为路径 `/`，触发"outside repository"错误。

**触发场景**：commit message 内嵌文件路径或 key 字符串时，含 `\"` 转义双引号。

**修复**：用 PowerShell here-string（单引号 `@'...'@`）定义变量再 `git commit -m $var`：

```powershell
$msg = @'
fix(localization): 补 CurrentAccountLabel 5 语 key

- TaskQueueView.xaml:80 引用 CurrentAccountLabel
- 5 语 xaml 各 +1 行 <system:String x:Key=CurrentAccountLabel>...</system:String>
'@
git commit -m $msg
```

**首次踩坑记录**：commit `16b11a0e31` 第一次尝试失败（消息含 `\"ToolboxTabRecruitHistory\"`），改用 here-string 后通过。

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
| 3 | 启动 | `Start-Process "install-staging\\MAA.exe" -WorkingDirectory "install-staging"` |
| 4 | 等待 | `Start-Sleep -Seconds 18`（足够 LoadResource + 网络请求 tasks.json） |
| 5 | 验证三连 | `AsstLoadResource ret: true`（debug/gui.log 末行）+ 0 errors（`grep -c "Unhandled\\|Fatal"`）+ 进程存活（`Get-Process MAA`） |
| 6 | 收尾 | `Stop-Process -Name MAA -Force` |

**首次踩坑实例**：2026-09-16 19:28 / 19:29 / 22:24 三次冒烟均通过 `ret: true`、0 错误、进程存活。

---

**§10 与其它章节互引**：
- §10.1 通用 → 所有 commit 操作前
- §10.2 dead key → 任何 feat/* / fix/* 回退后
- §10.3 冒烟 → 所有修改后必跑
- 上游同步专项 → [`WORKFLOW.md`](./WORKFLOW.md)
"""

with open(AGENTS_PATH, "rb") as f:
    data = f.read()

# UTF-8 with BOM
if data.startswith(b"\xef\xbb\xbf"):
    body = data[3:]
else:
    body = data

new_bytes = section_10.encode("utf-8")

with open(AGENTS_PATH, "wb") as f:
    f.write(b"\xef\xbb\xbf" + body + new_bytes)

print(f"Appended {len(new_bytes)} bytes")