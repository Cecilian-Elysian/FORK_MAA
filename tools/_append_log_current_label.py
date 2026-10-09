#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Append CurrentAccountLabel fix record to LOG.md (UTF-16 LE)."""

LOG_PATH = "LOG.md"

new_section = """
### fix(localization): CurrentAccountLabel 5 语补全（§7.16 遗留 TODO 修复）

| # | 操作 | 说明 |
|---|------|------|
| 1 | v6.18 合并后全量检查（2 explore agents + 人工复核） | 上游 12 项新功能全落地（RunControlState/RunOwner、SwitchTheme、MaterialSynthesis、Yituliu OpenAPI、InfrastAssistantChange、BlackFlow 66 源文件+99 模板+ONNX、GPU 驱动信息、Achievement、三方服务设置页、tasks.json 827 处、5 语新 key 抽查、双状态类架构）；fork 10 项修复全完好（轮换核心/公招加急 C++/RecruitNow@Slot/数据分桶/诊断报告/会客室线索零 diff/Copilot 作业集/轮换 UI/配置兼容/expedite 链路） |
| 2 | 遗留项处置决策 | MaterialSynthesis 入口注释=上游有意（三方一致）；YituliuId 空串=上游有意修正（fork 旧代码错赋 PenguinId）；CurrentAccountLabel 5 语缺失=§7.16 真遗留，用户确认修复 |
| 3 | 5 语 xaml 各 +1 行 | `CurrentAccountLabel` 插于各语 `AccountCycle` key 之前：zh-cn「当前账号：」/ zh-tw「目前帳號：」/ en-us「Current account: 」/ ja-jp「現在のアカウント：」/ ko-kr「현재 계정: 」 |
| 4 | commit `ae24fd2851` | fix(localization): 补 CurrentAccountLabel 5 语 key（§7.16 遗留 TODO），6 files +27 |
| 5 | install-staging 重新部署 | dotnet publish + nbeauty2 --usepatch；STARTUP_HOOKS=libloader ✓；启动冒烟 19:29 `AsstLoadResource ret: true`、0 errors、进程存活 |
| 6 | AGENTS §7.16 TODO 标注 | 已知 TODO 行加删除线 + **已修 2026-09-16**（commit `ae24fd2851`） |

**检查结论**：v6.18 更新完成——merge `6d32144858` + API 迁移 `72f52d9650` + 文档 `452a94f7fc` + localization 修复 `ae24fd2851`，staging 就绪待用户实机验证后晋升 branch。

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
