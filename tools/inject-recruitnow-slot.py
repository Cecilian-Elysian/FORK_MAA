#!/usr/bin/env python3
"""
inject-recruitnow-slot.py

上游同步后,把 fork 私有的 RecruitNow@Slot0..3 四个 task 注入到
resource/tasks/tasks.json, 消除该文件与上游的 fork 私有 diff.

使用方法:
    python tools/inject-recruitnow-slot.py

锚点策略:
    在 `"RecruitNow"` task 定义的 `"next": ["RecruitNowConfirm"]` 之后
    插入 4 个 @Slot0..3 派生 task. 锚点是 task 名 + 子键, 不是行号,
    因此上游修改 tasks.json 任何其他位置都不会影响注入.

契约:
    C++ `recruit_now()` 引用这 4 个 task 名 (`RecruitNow@Slot0..3`).
    任何下游改动需同步此脚本的 SLOT_TASKS 常量 + C++ 侧的 slot 名常量.

对应 AGENTS.md §3.2 (Fork 同步 SOP) 与 WORKFLOW.md §P0 抽离隔离.
"""
import json
import sys
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parent.parent
TARGET = REPO_ROOT / "resource" / "tasks" / "tasks.json"

# Fork 私有: 加急点击按槽位限定 ROI (fix/recruit-expedite-slot-target)
# roi 按 `slot_index_from_rect` 分界线 x=640 / y=450 划分象限
SLOT_TASKS = {
    "RecruitNow@Slot0": {
        "baseTask": "RecruitNow",
        "roi": [0, 300, 640, 150],
    },
    "RecruitNow@Slot1": {
        "baseTask": "RecruitNow",
        "roi": [640, 300, 640, 150],
    },
    "RecruitNow@Slot2": {
        "baseTask": "RecruitNow",
        "roi": [0, 450, 640, 300],
    },
    "RecruitNow@Slot3": {
        "baseTask": "RecruitNow",
        "roi": [640, 450, 640, 300],
    },
}

ANCHOR_TASK = "RecruitNow"
ANCHOR_NEXT_KEY = "next"

# 文本锚点: `RecruitNowConfirm` task 的首行 (紧随 `RecruitNow` 之后)
ANCHOR_TEXT = '    "RecruitNowConfirm": {'

# 待插入的原文块, 缩进与上游紧凑风格对齐 (4 空格 base / 8 空格字段)
SLOT_BLOCK_TEXT = "".join(
    '    "{}": {{\n'.format(name)
    + '        "baseTask": "RecruitNow",\n'
    + '        "roi": {}\n'.format(json.dumps(body["roi"]))
    + "    },\n"
    for name, body in SLOT_TASKS.items()
)


def main() -> int:
    if not TARGET.exists():
        print(f"[ERR] tasks.json not found: {TARGET}", file=sys.stderr)
        return 1

    with TARGET.open("r", encoding="utf-8") as f:
        data = json.load(f)

    if ANCHOR_TASK not in data:
        print(f"[ERR] anchor task '{ANCHOR_TASK}' not in tasks.json", file=sys.stderr)
        return 1

    existing_slots = [name for name in SLOT_TASKS if name in data]
    if existing_slots:
        for name in existing_slots:
            if data[name] != SLOT_TASKS[name]:
                print(
                    f"[WARN] {name} 存在但内容与脚本常量不同, 请手动核对:",
                    file=sys.stderr,
                )
                print(f"  现有: {json.dumps(data[name], ensure_ascii=False)}",
                      file=sys.stderr)
                print(f"  期望: {json.dumps(SLOT_TASKS[name], ensure_ascii=False)}",
                      file=sys.stderr)
        print(f"[OK] {len(existing_slots)}/{len(SLOT_TASKS)} slot tasks 已是最新, 无需注入")
        return 0

    anchor_next = data[ANCHOR_TASK].get(ANCHOR_NEXT_KEY)
    if anchor_next != ["RecruitNowConfirm"]:
        print(
            f"[WARN] {ANCHOR_TASK}.{ANCHOR_NEXT_KEY} = {anchor_next}, "
            f"与预期 ['RecruitNowConfirm'] 不一致, 跳过注入以免破坏结构",
            file=sys.stderr,
        )
        return 1

    for name, body in SLOT_TASKS.items():
        data[name] = body

    backup = TARGET.with_suffix(".json.bak")
    backup.write_text(TARGET.read_text(encoding="utf-8"), encoding="utf-8")

    # 文本级插入: 上游 tasks.json 用紧凑风格 (数组同行), 若整份json.dumps(indent=4)
    # 会把上游格式全量重排, 制造 ~7000 行格式债, 让后续每次同步都付冲突代价。
    # 故只在锚点 task 前插入原文块, 保持上游其余部分逐字节不变。
    text = TARGET.read_text(encoding="utf-8")
    if text.count(ANCHOR_TEXT) != 1:
        print(
            f"[ERR] 文本锚点出现 {text.count(ANCHOR_TEXT)} 次 (期望 1 次), 已回滚",
            file=sys.stderr,
        )
        backup.replace(TARGET)
        return 1

    text = text.replace(ANCHOR_TEXT, SLOT_BLOCK_TEXT + ANCHOR_TEXT)
    TARGET.write_text(text, encoding="utf-8", newline="")

    # 回读校验: JSON 可解析 + 4 个 slot task 内容与常量一致
    try:
        verify = json.loads(TARGET.read_text(encoding="utf-8"))
    except ValueError as exc:
        print(f"[ERR] 注入后 JSON 无法解析 ({exc}), 已回滚", file=sys.stderr)
        backup.replace(TARGET)
        return 1
    for name, body in SLOT_TASKS.items():
        if verify.get(name) != body:
            print(f"[ERR] 注入后 {name} 内容不符, 已回滚", file=sys.stderr)
            backup.replace(TARGET)
            return 1

    print(f"[OK] 注入 {len(SLOT_TASKS)} 个 slot tasks 到 {TARGET} (文本级, 保持上游紧凑格式)")
    print(f"[OK] 备份: {backup}")
    return 0


if __name__ == "__main__":
    sys.exit(main())