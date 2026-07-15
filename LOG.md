# 修改日志

日志规范：每次修改文件后，在此记录修改内容。

## 2026-07-15

### 分支工作文档约束调整

LOG.md / AGENTS.md 从 `.gitignore` 移除，改为所有分支跟踪；feat*.md / fix*.md 保留 gitignore，永不提交。

| # | 文件 | 操作 | 说明 |
|---|------|------|------|
| 1 | `.gitignore` | 删除 | 移除 `LOG.md`、`AGENTS.md` 忽略规则 |
| 2 | `LOG.md` | 新建 | 本节 |
