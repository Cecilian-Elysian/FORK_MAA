#!/usr/bin/env python3
"""
auto-resolve-xaml-conflict.py

合并 upstream/master-v2 后自动处理 5 语 xaml 冲突标记, 特别是
AccountSwitch → AccountCycle 替换区域.

使用方法:
    python tools/auto-resolve-xaml-conflict.py
    python tools/auto-resolve-xaml-conflict.py src/MaaWpfGui/Res/Localizations/zh-cn.xaml

对应 AGENTS.md §3.4 (本地化) 与 WORKFLOW.md §6 (合并手解) 的自动化补充.
"""

import re
import sys
from pathlib import Path

# Fork 已删除的 key (如果上游修改了这些 key 的值, 自动取 fork 侧 = 删除)
DELETED_KEYS = [
    # feat/account_rotation supersede: AccountSwitch 已被 AccountCycle 替代
    "AccountSwitch",
    "AccountSwitchManualRun",
    "AccountSwitchTip",
    # feat/auto-recruit-3star-to-4star 已删 (Phase A)
    "AutoUseExpedited",
    "AutoUseExpeditedTip",
]

# Fork 新增的 key (如果上游删除了这些 key, 自动取 fork 侧保留; 双向补充保留 fork key)
ADDED_KEYS = [
    # feat/account_rotation: AccountCycle 系列
    "AccountCycle", "AccountCycleTip", "AccountCycleAddNewAccount",
    "AccountCycleRemoveTip", "AccountCycleRemoveConfirm", "AccountCycleRemoveMessage",
    # feat/defer-rogue: LateStage
    "LateStageRogueAndReclamation", "LateStageRogueAndReclamationTip",
    # feat/expedite-threshold: ExpediteMode
    "ExpediteMode", "ExpediteModeTip", "ExpediteModeDisabled",
    "ExpediteModeAll", "ExpediteMode4", "ExpediteMode5", "ExpediteMode6",
    # feat/diagnostic-export
    "GenerateDiagnosticReport", "GenerateDiagnosticReportSelectLocation",
    "DiagnosticDateRange",
    # feat/account-scoped-recognition-data
    "DataAccountLabel", "DataAccountDefault",
    # feat/copilot-paste-clipboard
    "PasteClipboardCopilotSetTip",
    # feat/recruit-history-tab
    "ToolboxTabRecruitHistory",
]


def has_deleted_key(text: str) -> bool:
    """检查文本块中是否包含已删除 key 的 x:Key 定义"""
    for key in DELETED_KEYS:
        if f'x:Key="{key}"' in text:
            return True
    return False


def has_added_key(text: str) -> bool:
    """检查文本块中是否包含新增 key 的 x:Key 定义"""
    for key in ADDED_KEYS:
        if f'x:Key="{key}"' in text:
            return True
    return False


def resolve_conflict(file_path: Path) -> bool:
    """处理单个 xaml 文件的冲突标记

    Returns:
        True: 文件被修改 (changes made)
        False: 文件无冲突或未自动处理
    """
    if not file_path.exists():
        print(f"  [SKIP] file not found: {file_path}")
        return False

    try:
        content = file_path.read_text(encoding="utf-8")
    except UnicodeDecodeError:
        # LOG.md/部分老文件是 UTF-16 LE BOM, 但 xaml 应是 UTF-8
        print(f"  [WARN] encoding error (not UTF-8): {file_path}")
        return False

    if "<<<<<<<" not in content:
        print(f"  [OK]   no conflict: {file_path.name}")
        return False

    # 匹配标准 git 冲突块: <<<<<<< HEAD\n...ours\n=======\n...theirs\n>>>>>>> upstream/master-v2
    # 也兼容任意分支名: <<<<<<< ...\n...ours\n=======\n...theirs\n>>>>>>> ...
    conflict_pattern = re.compile(
        r"<<<<<<<[^\n]*\n(.*?)\n=======\n(.*?)\n>>>>>>>[^\n]*\n",
        re.DOTALL,
    )

    changes_made = False
    unresolved = []

    def replace_conflict(match: re.Match) -> str:
        nonlocal changes_made
        ours = match.group(1)
        theirs = match.group(2)

        # 规则 1: 冲突块涉及已删除 key -> 取 fork 侧 (ours)
        if has_deleted_key(theirs):
            changes_made = True
            return ours
        # 规则 2: 上游删了 fork 新增的 key -> 取 fork 侧保留
        if has_added_key(ours) and not has_added_key(theirs):
            changes_made = True
            return ours
        # 规则 3: 冲突块两边都有新增 key 内容 -> 双向保留 (ours + theirs)
        if has_added_key(ours) and has_added_key(theirs):
            changes_made = True
            return ours + "\n" + theirs
        # 默认保守策略: 取 fork 侧 (避免误丢 fork 功能)
        unresolved.append(match.group(0)[:80])
        return ours

    new_content = conflict_pattern.sub(replace_conflict, content)

    if changes_made:
        # 保留文件末尾换行符
        if not new_content.endswith("\n"):
            new_content += "\n"
        file_path.write_text(new_content, encoding="utf-8")
        print(f"  [DONE] processed: {file_path.name}")
        if unresolved:
            print(f"    [WARN] {len(unresolved)} conservative block(s) (took fork), suggest manual review")
            for u in unresolved[:3]:
                print(f"           - {u}")
        return True
    else:
        print(f"  [WARN] conflict but all conservative (took fork): {file_path.name}")
        # 仍然写回 (因为 replace_conflict 至少输出了 ours)
        if not new_content.endswith("\n"):
            new_content += "\n"
        file_path.write_text(new_content, encoding="utf-8")
        return True


def main() -> int:
    if len(sys.argv) >= 2:
        files = [Path(f) for f in sys.argv[1:]]
    else:
        # 默认处理所有 5 语 xaml
        repo_root = Path(__file__).resolve().parent.parent
        xaml_dir = repo_root / "src" / "MaaWpfGui" / "Res" / "Localizations"
        files = sorted(xaml_dir.glob("*.xaml"))

    if not files:
        print("No xaml files found")
        return 1

    print("=== xaml Conflict Auto-Resolution ===")
    any_resolved = False
    for f in files:
        if resolve_conflict(f):
            any_resolved = True

    if not any_resolved:
        print("\nNo conflicts to resolve")
        return 0
    print("\nDone. Suggest running tools/post-merge-validate.ps1 to verify fork features.")
    return 0


if __name__ == "__main__":
    sys.exit(main())