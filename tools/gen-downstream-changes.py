#!/usr/bin/env python3
"""Generate a downstream-changes file list from LOG.md tables.

Parses 4-column tables in LOG.md (``| # | 文件/对象 | 操作 | 说明 |`` /
``| # | 文件 | 操作 | 说明 |``), extracts the second-column backtick-wrapped
path, strips trailing ``:line-number`` suffixes, filters out non-source paths
(build artifacts, branch names, the LOG/AGENTS files themselves), groups by
top-level directory, and writes a markdown document to
``docs/downstream-changes.md``.

Usage::

    py tools/gen-downstream-changes.py
    py tools/gen-downstream-changes.py --log LOG.md --out docs/downstream-changes.md

Re-run after every feat/fix merge to refresh the list.
"""

from __future__ import annotations

import argparse
import re
import sys
from collections import defaultdict
from dataclasses import dataclass, field
from pathlib import Path

# Match a 4-column markdown table row where column 2 is a single backtick path.
# Tolerates trailing spaces, multi-line-number suffixes (``a:1, 3, 5``), and
# shell brace expansion (``{zh-cn,en-us}.xaml``). Operation column is free
# text (``修改`` / ``cherry-pick from xxxx``) so it uses ``.+?``.
TABLE_ROW_RE = re.compile(
    r"^\|\s*(\d+)\s*\|\s*`([^`]+)`\s*\|\s*(.+?)\s*\|\s*(.*?)\s*\|\s*$"
)

# Top-level directory prefixes considered "source code" for this script.
SOURCE_PREFIXES: tuple[str, ...] = (
    "src/",
    "resource/",
    "tools/",
    "test/",
    "docs/",
    ".github/",
)

# Dotfiles / configs at the repo root worth tracking.
ROOT_DOTFILES: tuple[str, ...] = (
    ".pre-commit-config.yaml",
    ".gitignore",
    ".clang-format",
    ".editorconfig",
    "global.json",
)

# Root-level files we DO track but lack a leading ``src/`` etc. prefix.
ROOT_FILES: tuple[str, ...] = (
    "VERSION",
)

# Path substrings that indicate a non-source entry (build artifacts, docs
# metadata, etc.). Anything containing any of these is dropped.
EXCLUDE_SUBSTRINGS: tuple[str, ...] = (
    "install/",
    "install-staging/",
    "build/",
    "debug/",
    "config/",
    "cache/",
    "data/",
    "reports/",
)

# Files we never treat as "source" even though they live at the repo root.
ROOT_META_FILES: tuple[str, ...] = (
    "LOG.md",
    "AGENTS.md",
    "CHANGELOG.md",
    "package-definition.json",
)

# High-sensitivity threshold: files appearing in >= N entries are flagged.
HIGH_SENSITIVITY_THRESHOLD = 3


@dataclass
class FileEntry:
    """Aggregated change record for a single path."""

    path: str
    operations: list[str] = field(default_factory=list)
    descriptions: list[str] = field(default_factory=list)

    def add(self, operation: str, description: str) -> None:
        self.operations.append(operation)
        # Collapse multi-line descriptions to a single short line.
        self.descriptions.append(" ".join(description.split()))

    @property
    def change_count(self) -> int:
        return len(self.operations)

    @property
    def is_high_sensitivity(self) -> bool:
        return self.change_count >= HIGH_SENSITIVITY_THRESHOLD


def normalize_path(raw: str) -> str:
    """Strip line-number suffixes and split comma-separated entries.

    Handles three LOG.md idiosyncrasies:

    * single line-number suffix (``path:123``)
    * line-number range (``path:123-456``)
    * multiple locations in one entry (``path:1, 3-5, 7``)
    * shell brace expansion in the path itself (``{zh-cn,en-us}.xaml``)

    Comma splitting is brace-aware: a comma inside ``{...}`` does NOT split.
    """
    # Strip every ``:digits`` or ``:digits-digits`` suffix attached to a path
    # component. Do this BEFORE splitting on commas so a single entry like
    # ``foo.cpp:68-83`` collapses to ``foo.cpp`` while a multi-location
    # entry like ``foo.cpp:68-83, 173, 186`` splits into three.
    base = re.sub(r":\d+(?:-\d+)?", "", raw).strip()
    # Brace-aware comma split: keep ``{a,b,c}`` as a single token.
    parts: list[str] = []
    buf: list[str] = []
    depth = 0
    for ch in base:
        if ch == "{":
            depth += 1
            buf.append(ch)
        elif ch == "}":
            depth -= 1
            buf.append(ch)
        elif ch == "," and depth == 0:
            piece = "".join(buf).strip()
            if piece:
                parts.append(piece)
            buf = []
        else:
            buf.append(ch)
    tail = "".join(buf).strip()
    if tail:
        parts.append(tail)
    return parts


def expand_braces(path: str) -> list[str]:
    """Expand shell-style ``{a,b,c}`` brace lists within a single path.

    Only one brace group per path is supported (sufficient for LOG.md entries
    like ``Res/Localizations/{zh-cn,en-us,ja-jp,ko-kr,zh-tw}.xaml``). If the
    path contains no braces, returns ``[path]`` unchanged.
    """
    m = re.search(r"\{([^{}]+)\}", path)
    if not m:
        return [path]
    inner = m.group(1)
    options = [opt.strip() for opt in inner.split(",")]
    prefix = path[: m.start()]
    suffix = path[m.end() :]
    return [f"{prefix}{opt}{suffix}" for opt in options]


def is_source_path(path: str) -> bool:
    """Return True if ``path`` represents source code worth tracking."""
    if path in ROOT_DOTFILES or path in ROOT_FILES:
        return True
    if path in ROOT_META_FILES:
        return False
    if any(sub in path for sub in EXCLUDE_SUBSTRINGS):
        return False
    if any(path.startswith(prefix) for prefix in SOURCE_PREFIXES):
        return True
    # Plain branch / remote refs like ``fix/foo`` or ``Github/branch`` lack
    # a leading source-code prefix and no file extension; skip them.
    if "/" not in path and "\\" not in path:
        return False
    return False


def parse_log(log_path: Path) -> dict[str, FileEntry]:
    """Walk ``log_path`` and aggregate per-file change records."""
    entries: dict[str, FileEntry] = {}
    for line in _read_log_text(log_path).splitlines():
        m = TABLE_ROW_RE.match(line)
        if not m:
            continue

        raw_path = m.group(2).strip()
        operation = m.group(3).strip()
        description = m.group(4).strip()

        if not raw_path:
            continue

        # Normalize: strip line-number suffixes, split comma-separated
        # entries, expand shell brace lists. Each resulting piece becomes
        # its own entry so e.g. ``Res/Localizations/{zh-cn,en-us,...}.xaml``
        # yields five tracked files.
        for norm in normalize_path(raw_path):
            for expanded in expand_braces(norm):
                if not is_source_path(expanded):
                    continue
                if expanded not in entries:
                    entries[expanded] = FileEntry(path=expanded)
                entries[expanded].add(operation, description)

    return entries


def group_by_top_dir(entries: dict[str, FileEntry]) -> dict[str, list[FileEntry]]:
    """Group entries by their first path segment (``src/MaaCore/...`` -> ``src``).

    Root-level files (dotfiles, ``VERSION``, etc.) that lack a directory
    prefix are surfaced under a synthetic ``(root)`` bucket.
    """
    grouped: dict[str, list[FileEntry]] = defaultdict(list)
    for entry in entries.values():
        if "/" in entry.path:
            top = entry.path.split("/", 1)[0]
        else:
            top = "(root)"
        grouped[top].append(entry)
    for group in grouped.values():
        group.sort(key=lambda e: e.path)
    return dict(sorted(grouped.items()))


def render_markdown(entries: dict[str, FileEntry], total_table_rows: int) -> str:
    """Build the downstream-changes markdown document."""
    lines: list[str] = []
    lines.append("# 下游改动文件清单")
    lines.append("")
    lines.append(
        "本仓库相对上游 `MaaAssistantArknights/MaaAssistantArknights` "
        "的所有改动文件清单，由 `tools/gen-downstream-changes.py` 自动从 "
        "[`LOG.md`](../LOG.md) 的实施完成表格提取。"
    )
    lines.append("")
    lines.append("## 用法")
    lines.append("")
    lines.append(
        "在修改任何文件 **之前** 先查阅此清单："
    )
    lines.append("")
    lines.append(
        "- [OK] **不在清单中** = 上游原装代码，改动需谨慎（可能破坏上游兼容性）"
    )
    lines.append(
        "- [TGT] **在清单中** = 本仓库改动过，确认你的改动是否与已有下游逻辑冲突"
    )
    lines.append(
        "- [HOT] **高敏感**（被改 >= "
        f"{HIGH_SENSITIVITY_THRESHOLD} 次）= 多轮 feat/fix 反复动过，改动前优先读相关 LOG 段落"
    )
    lines.append("")
    lines.append("## 维护")
    lines.append("")
    lines.append(
        "每次 feat/fix 合并后，重跑脚本刷新："
    )
    lines.append("")
    lines.append("```")
    lines.append("py tools/gen-downstream-changes.py")
    lines.append("```")
    lines.append("")
    lines.append(
        f"共扫描 {total_table_rows} 个表格行，"
        f"聚合出 {len(entries)} 个唯一源文件路径。"
    )
    lines.append("")

    grouped = group_by_top_dir(entries)
    for top_dir, group in grouped.items():
        if top_dir == "(root)":
            heading = f"## 仓库根（{len(group)} 个文件）"
        else:
            heading = f"## `{top_dir}/`（{len(group)} 个文件）"
        lines.append(heading)
        lines.append("")
        for entry in group:
            marker = "[HOT]" if entry.is_high_sensitivity else "[TGT]"
            count = entry.change_count
            count_label = f"(x{count})" if count > 1 else ""
            lines.append(f"### {marker} `{entry.path}` {count_label}")
            lines.append("")
            lines.append("| 操作 | 说明 |")
            lines.append("|------|------|")
            for op, desc in zip(entry.operations, entry.descriptions):
                # Escape pipe chars inside descriptions.
                safe_desc = desc.replace("|", "\\|")
                lines.append(f"| {op} | {safe_desc} |")
            lines.append("")

    return "\n".join(lines) + "\n"


def _read_log_text(log_path: Path) -> str:
    """Read LOG.md tolerating UTF-16 LE BOM (project historical convention) and UTF-8 BOM."""
    raw = log_path.read_bytes()
    if raw.startswith(b"\xff\xfe"):
        return raw.decode("utf-16")
    if raw.startswith(b"\xef\xbb\xbf"):
        return raw.decode("utf-8-sig")
    return raw.decode("utf-8")


def count_table_rows(log_path: Path) -> int:
    """Count how many table rows matched (for the header summary)."""
    n = 0
    for line in _read_log_text(log_path).splitlines():
        if TABLE_ROW_RE.match(line):
            n += 1
    return n


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    parser.add_argument(
        "--log",
        type=Path,
        default=Path("LOG.md"),
        help="Path to LOG.md (default: LOG.md)",
    )
    parser.add_argument(
        "--out",
        type=Path,
        default=Path("docs/downstream-changes.md"),
        help="Output markdown path (default: docs/downstream-changes.md)",
    )
    parser.add_argument(
        "--dry-run",
        action="store_true",
        help="Print parsed summary to stdout without writing the output file.",
    )
    args = parser.parse_args(argv)

    if not args.log.exists():
        print(f"error: LOG.md not found at {args.log}", file=sys.stderr)
        return 1

    total_rows = count_table_rows(args.log)
    entries = parse_log(args.log)

    if args.dry_run:
        print(f"Scanned {total_rows} table rows; {len(entries)} unique source files.")
        for path, entry in sorted(entries.items()):
            marker = "[HOT]" if entry.is_high_sensitivity else "     "
            print(f"  {marker} {path}  (x{entry.change_count})")
        return 0

    args.out.parent.mkdir(parents=True, exist_ok=True)
    args.out.write_text(
        render_markdown(entries, total_rows),
        encoding="utf-8",
    )
    print(
        f"Wrote {args.out} ({len(entries)} files scanned from "
        f"{total_rows} LOG.md table rows)."
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())