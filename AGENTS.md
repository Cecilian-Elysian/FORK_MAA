# AGENTS.md — MAA (MaaAssistantArknights)

## 强制约束（修改前必读）

1. **勿修改 `src/MaaUtils/` 内部代码** — 子模块，修改需推送到上游 `MaaXYZ/MaaUtils`。
2. **勿修改 `src/MaaMacGui/`、`src/maa-cli/`、`src/MAAUnified/`** — 均为子模块。
3. **新文件注意平台过滤** — `src/MaaCore/CMakeLists.txt:4-8` 有 Android/非 Android 的源文件排除规则，确认不被同时过滤。
4. **资源目录结构固定** — `resource/` 下的 `config.json`、`tasks/`、`template/` 路径不可变。
5. **`install/` 目录** — 构建产物目录，已纳入 git 跟踪。
6. **所有坐标/ROI 基于 1280×720** — 任务 JSON 和识别配置以该分辨率为准。
7. **`install_V1/` 保留勿删** — 旧版构建产物存档。
8. **`3rdparty/` 只放头文件和子模块** — 不加入实现文件或二进制。
9. **`dotnet publish` 产物需清理后使用** — 发布后手动删除所有非 MAA 必需 DLL，否则会报 DLL 劫持警告。
10. **构建后清理 `install/` 空目录** — 每次构建后检查并移除所有空目录，防止残留文件干扰。

---

## 分支 & 文档工作流

### 分支命名
| 分支前缀 | 用途 | 说明 |
|----------|------|------|
| `feat/*` | 新功能开发 | 验证新功能是否可用，从 `branch` 创建，完成后 PR 合并回 `branch` |
| `fix/*/(N)` | Bug 修复 | 解决 bug 和小型误差，从**对应 `feat/*`** 创建（而非 `branch`），随 `feat/*` 一起 PR 合并回 `branch` |

### 修复分支必须挂在对应 feat 之下
- **强制约束**：任何 `fix/*` 分支都从其要修复的 `feat/*` 分支拉出，而不是从 `branch` 拉出
- **合并路径**：`fix/*` → `feat/*` → `branch`
- **原因**：feat 分支未合入 `branch` 前，从 `branch` 拉的 fix 会缺少 feat 提交，无法独立验证；挂在 feat 下可随 feat 一起做集成测试
- **例外**：要修复的是 `branch` 自身历史 bug（无对应 feat）时，才从 `branch` 拉 `fix/*`
- **跨多个 feat**：若 fix 同时修复多个相关 feat 的交互缺陷，需在分支名前缀显式标注参考 feat（如 `fix/defer-rogue+account_rotation/修改次数`），且合并目标选依赖链最下游的 feat（即被其他 feat 依赖的那个），PR 说明中列出涉及的所有 feat 分支

### 工作文档（不纳入 git）
- 每个 `feat/*` 或 `fix/*` 分支在项目根目录创建同名 `.md` 文件，例如 `feat_账号轮换.md`、`fix_闪退修复.md`
- 这些文件已通过 `.gitignore` 中的 `feat*.md` / `fix*.md` 忽略，不会提交到仓库
- 用途：记录需求问答过程、实现流程、关键决策、踩坑记录

### 分支完成后的经验沉淀
- 分支合并后，将整个开发过程中积累的经验、踩坑记录、优化思路整理写入对应的 `feat_*.md` / `fix_*.md` 文件
- 这些文件作为宝贵资源长期保留在本地，供后续查阅反思

### 分支工作流示意图
```
dev-v2（源代码主分支，只读）
  └─ branch（最终完成版，生产就绪）
       └─ feat/*（新功能开发，附带 feat_*.md）
            └─ fix/*/(N)（修复，从对应 feat/* 拉出，附带 fix_*.md）← 修复挂在 feat 下
```

---

## 修改日志（LOG.md）

- 项目根目录的 `LOG.md` 记录每次手工修改
- 每次修改文件后必须更新 `LOG.md`，格式见该文件头部的日志规范
- 严格记录每一步：修改了什么、为什么改、有什么影响，便于后续回溯、反思与优化

---

## 常用命令

| 操作 | 命令 |
|------|------|
| 配置（Windows x64） | `cmake -B build --preset "windows-x64"` |
| 构建 | `cmake --build build --config RelWithDebInfo --parallel` |
| 安装 | `cmake --install build --config RelWithDebInfo` |
| 下载 MaaDeps | `python tools/maadeps-download.py x64-windows` |
| 同步子模块 | `bash .github/scripts/sync-optional-submodules.sh --init --depth 1 src/MaaUtils 3rdparty/EmulatorExtras` |
| WPF 发布 | `dotnet publish src/MaaWpfGui/MaaWpfGui.csproj -c Release -p:Platform=x64 -o install` |
| 单元测试 | `cmake -S unit_test -B build/unit_test -DCMAKE_BUILD_TYPE=Release && cmake --build build/unit_test --target maa-algorithm-test && ctest --test-dir build/unit_test --output-on-failure -R "^algorithm::"` |

> **验证约束**：每次修改代码并构建成功后，必须通过 `install/` 目录启动程序验证效果。

---

## 架构要点

- **控制流**：用户代码 → AsstCaller API → Assistant（3 线程：调用/工作/消息）→ InterfaceTask → PackageTask → ProcessTask（图像匹配 → 动作）→ Controller
- **消息回调**：`AsstMsg` 枚举（0‑9999 全局 / 10000‑19999 任务链 / 20000‑29999 子任务 / 30000+ Web 请求），回调详情始终为 JSON
- **任务系统**：InterfaceTask（对外类型）→ PackageTask（组合）→ ProcessTask（tasks.json 驱动，支持 #self/#next/#back 流程控制）
- **控制器**：ADB / Minitouch / Maatouch / Win32 / PlayTools / MaaFwAdb / MaaFwAndroidNative

---

## 代码组织

```
src/
  MaaCore/        C++20 核心（Task/ Vision/ Controller/ Config/ Common/ Utils）
  MaaWpfGui/      C# WPF GUI（Stylet MVVM + HandyControls）
  Python/Go/Rust/Java/Dart/Woolang/  FFI 绑定
  MaaUpdater/     自动更新器
  MaaWineBridge/  Wine/Linux 兼容层
resource/
  tasks/tasks.json    任务流水线配置
  config.json         全局配置
  template/           模板图像
  roguelike/          肉鸽数据
  global/             各客户端资源覆盖
  onnx/ PaddleOCR/    OCR 模型
  Arknights-Tile-Pos/ 地图瓦片数据
unit_test/        Catch2 v3 单元测试
tools/            构建/工具脚本
include/          公开 C API 头文件
```

---

## 注意事项

- **WPF 构建**需要 .NET 10.0 SDK（`global.json` 固定 `10.0.203`）
- **ClientType 时机**：使用 `[PackageName]` 时，`InstanceOptionKey::ClientType` 必须在连接**之前**设置
- **ONNX 会话**是进程级单例，在 `AsstLoadResource` 前设置 CPU/GPU
- **C++ 风格**：C++20、WebKit 风格 clang-format、4 空格缩进、120 列、`asst` 命名空间、`#pragma once`
- **命名规范**：C ABI 用 `AsstXxx`，类 `PascalCase`，方法 `camelCase`，成员 `m_` 前缀
- 测试框架 **Catch2 v3.8.1**，CI 根据 `git diff` 运行选定测试套件
- 所有手动修改需记录在 `LOG.md`

---

## 技能（Skills）

| 技能 | 路径 |
|------|------|
| changelog | `.crush/skills/changelog/SKILL.md` |
| maa-cyber-fortune-master | `.crush/skills/maa-cyber-fortune-master/SKILL.md` |
| maa-issue-log-analysis | `.crush/skills/maa-issue-log-analysis/SKILL.md` |
| pallas-style-reply | `.crush/skills/pallas-style-reply/SKILL.md` |
| code-review-report | `.crush/skills/code-review-report/SKILL.md` |
