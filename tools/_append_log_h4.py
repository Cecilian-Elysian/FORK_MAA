#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Append H4 dead-key cleanup record to LOG.md (UTF-16 LE)."""

LOG_PATH = "LOG.md"

new_section = """
### chore(cleanup): §7.9 dead key ToolboxTabRecruitHistory 全清（脚本 + 4 语 xaml）

| # | 操作 | 说明 |
|---|------|------|
| 1 | 触发 | 最终代码审计 H4 minor：post-merge-validate.ps1 stale 检查项 ToolboxTabRecruitHistory 误报（4/5 语仍残留 dead key） |
| 2 | 根因 | §7.9 feat/recruit-history-tab (2026-08-02 a13825c68f) 已回退，C# 代码零引用，但 zh-cn/zh-tw/en-us/ja-jp 残留 dead string，仅 ko-kr 之前被清理过 |
| 3 | 清理 4 语 xaml | 各删 1 行 `<system:String x:Key="ToolboxTabRecruitHistory">...` dead key（zh-cn「公招历史」/zh-tw「公招歷史」/en-us「Recruit History」/ja-jp「公招履歴」），4 行 |
| 4 | 同步 2 脚本 | tools/post-merge-validate.ps1:37-38 移除 `# feat/recruit-history-tab` 注释块（含 stale 检查项）；tools/auto-resolve-xaml-conflict.py:47-48 同步移除 |
| 5 | 验证 | post-merge-validate.ps1 7/7 检查全 OK（[OK] 5-language xaml fork keys complete）；git grep ToolboxTabRecruitHistory 全仓 0 命中 |
| 6 | 重新部署 | install-staging dotnet publish + nbeauty2 --usepatch（22:24 时间戳）；MAA.exe + MAA.dll 同步更新 |
| 7 | 冒烟 | 启动 MAA.exe 18s，AsstLoadResource ret: true、0 错误、进程存活 |
| 8 | commit `16b11a0e31` | chore(cleanup): §7.9 dead key ToolboxTabRecruitHistory 全清（脚本 + 4 语 xaml），6 files +5/-9 |

**更新总检结论**：staging 状态 PASS——fork 10 项修复完整、上游 12 项新功能全落地、C# API 迁移 0 残留、产物一致性过、文档无漂移、备份与 graft 存活、H4 minor 已修。staging 就绪待用户实机验证后晋升 branch。

"""

with open(LOG_PATH, "rb") as f:
    data = f.read()

new_bytes = new_section.encode("utf-16-le")

if data.startswith(b"\xff\xfe"):
    body = data[2:]
else:
    body = data

with open(LOG_PATH, "wb") as f:
    f.write(b"\xff\xfe" + body + new_bytes)

print(f"Appended {len(new_bytes)} bytes")