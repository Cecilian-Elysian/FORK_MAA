# AGENTS

本文件面向在本仓库协作的 AI 代理与人类贡献者，约定工作流与文档规范。

## feat / fix 工作笔记约定

仓库中存在 `feat_<name>.md` 与 `fix_<name>[_<n>].md` 两类**分支作用域**的本地工作笔记，
用于在对应分支开发期间记录变更、计划与决策。

### 命名约定

由于 Windows 文件系统不支持路径分隔符，分支路径中的 `/` 在文件名中写为 `_`：

| 分支                | 工作笔记文件                       |
| ------------------- | ---------------------------------- |
| `feat/<name>`       | `feat_<name>.md`                   |
| `fix/<name>`        | `fix_<name>.md`                    |
| `fix/<name>/<n>`    | `fix_<name>_<n>.md`                |
| `fix/<name>/修改次数` | `fix_<name>_修改次数.md`         |

### 各分支保留规则

每个分支应只保留与自身作用域相关的笔记文件：

| 分支类型                       | 应保留的 md                                | 不应保留                            |
| ------------------------------ | ------------------------------------------ | ----------------------------------- |
| `branch`（整合分支）           | 无                                         | 所有 `feat_*.md`、`fix_*.md`        |
| `feat/<name>`（功能分支）      | 仅 `feat_<name>.md`                        | 任何 `fix_*.md`                     |
| `fix/<name>`（派生自 `feat/<parent>`） | 上游 `feat_<parent>.md` + `fix_<name>.md` | 其它无关的 feat / fix 文件          |

示例：

- 在 `feat/account_rotation` 分支下，只应存在 `feat_account_rotation.md`
- 在 `fix/account_rotation/2` 分支下（其上游为 `feat/account_rotation`），
  应同时存在 `feat_account_rotation.md` 与 `fix_account_rotation_2.md`
- 在 `branch` 分支下不应存在任何 `feat_*.md` 或 `fix_*.md`

### 提交策略

这些文件是**本地工作笔记**，不应提交至仓库。`.gitignore` 已忽略 `feat*.md` 与
`fix*.md`，请勿主动 `git add -f` 强制添加。

切换分支时，若工作区出现不属于当前分支作用域的 `feat_*.md` / `fix_*.md` 文件，
应删除后再继续工作。