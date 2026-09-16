#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Append 2026-09-16 v6.18 同步日志到 LOG.md (UTF-16 LE)."""

LOG_PATH = 'LOG.md'

new_section = """
## 2026-09-16

### feat/upstream-v618-sync 仓库卫生（阶段 0）

| # | 操作 | 说明 |
|---|------|------|
| 1 | `git branch backup/staging-pre-v618-sync-20260916 staging` | 合并前快照（staging 含 9/7 depot 修复 + sync-isolation + fork-externalization 共 229 私有 commit） |
| 2 | `git branch backup/master-pre-v618-sync-20260916 master` | master 旧 v6.17.0 镜像快照 |
| 3 | `git switch master && git fetch upstream && git reset --hard upstream/master-v2` | master 同步到 v6.18.0-beta.1 head `ec14df252ae` |
| 4 | `git branch -f new-branch master` | 临时对比分支从新 master 拉出（用完删） |
| 5 | `git switch staging` | 切回 staging |
| 6 | `git cherry-pick e079795071` (feat/fork-externalization 1 commit) | 失败：与 staging 现有 AGENTS.md §2.4 / §6 / §7.18 冲突。3 处冲突全部是 staging 已有更完整内容；`git checkout --ours` + `git add` + `cherry-pick --skip` 跳过，HEAD 保持 0eb6cba26e |
| 7 | `git branch -D feat/fork-externalization` | 已通过 cherry-pick 验证为空 commit，远端保留 |
| 8 | `git branch -d feat/sync-isolation` | staging 已含 `0d55649f63` merge，远端保留 |
| 9 | `git branch -d feat/upstream-v617-sync` | staging 已含 `a92a8a9124` merge，远端保留 |
| 10 | 编辑 AGENTS.md §2.4 末尾 | 追加 v6.18 晋升占位行（_pending_/_pending_/~170 commit/trust staging/§7.22 + §7.18 备注） |
| 11 | 编辑 AGENTS.md §6 | 「feat/upstream-v617-sync」行改为「feat/upstream-v618-sync」（角色：v6.17.0~v6.18.0-beta.1 三方合并） |
| 12 | 编辑 AGENTS.md §7 | 新增 §7.19（feat/upstream-v617-sync）/§7.20（feat/sync-isolation）/§7.21（feat/fork-externalization）/§7.22（feat/upstream-v618-sync 进行中）4 个分支生命周期 |
| 13 | 编辑 CHANGELOG.md 头部 | 在「v6.17.0」之前插入「v6.18.0-beta.1（进行中）」段占位，含 Fork 增量清单 + v6.18 上游新功能预告 |
| 14 | 新增 V618_SYNC_PLAN.md（项目根） | 压缩执行计划书：11 阶段 + 14 sub-phase + A.1-A.4 增量清单 + B.1-B.3 风险预防 + C.1-C.6 检查表 + D 阶段脚本 + E 时间预算 + F 关键决策 |
| 15 | `python tools/gen-downstream-changes.py` | 重生成 docs/downstream-changes.md（合并前基线） |

**阶段 0 commit**：见下个 commit `docs(cleanup): v6.18 同步前仓库卫生 + feat/* 本地删 + AGENTS/CHANGELOG/LOG 漂移修复`

**当前状态**：staging @ 0eb6cba26e（未动），master @ ec14df252ae（v6.18.0-beta.1 同步），3 个 feat/* 本地分支已删（远端保留），备份点建立，文档漂移全部修复，准备进入阶段 1。

"""

# Read UTF-16 LE BOM + content
with open(LOG_PATH, 'rb') as f:
    data = f.read()

# Encode new section as UTF-16 LE (no BOM since we append to existing file)
new_bytes = new_section.encode('utf-16-le')

# Sanity: ensure we don't double-write BOM
if data.startswith(b'\xff\xfe'):
    body = data[2:]
else:
    body = data

# Append
with open(LOG_PATH, 'wb') as f:
    f.write(b'\xff\xfe' + body + new_bytes)

print(f"Appended {len(new_bytes)} bytes")
print(f"LOG.md new size: {len(b'\\xff\\xfe' + body + new_bytes)} bytes")
