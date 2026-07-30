#!/usr/bin/env python3
"""
oper_phash.json 构建脚本（占位）

从游戏内「招募完成展示页」立绘截图计算 pHash，生成
resource/recruit/oper_phash.json。

实际实现待定：需用 PyAutoGUI / ADB 截取每干员立绘，cv2 计算 64-bit pHash。
当前为占位脚本，由 feat/recruit-result-display Phase 3 引入。

usage:
    py tools/build_phash.py --input <screenshots_dir> --output resource/recruit/oper_phash.json
"""

import argparse
import json
import sys
from pathlib import Path


def main() -> int:
    parser = argparse.ArgumentParser(description="Build oper_phash.json from operator portrait screenshots")
    parser.add_argument("--input", required=True, help="screenshots dir, subdirs named by oper_id (e.g. char_140_angel/)")
    parser.add_argument("--output", default="resource/recruit/oper_phash.json")
    args = parser.parse_args()

    # TODO: 实现 cv2 pHash 计算
    placeholder = {
        "_comment": "由 tools/build_phash.py 生成。当前为占位，待 Phase 8 实装。",
        "_generator": "tools/build_phash.py",
        "_version": "1",
    }
    Path(args.output).write_text(json.dumps(placeholder, ensure_ascii=False, indent=4), encoding="utf-8")
    print(f"Wrote placeholder {args.output} (TODO: implement cv2 pHash)")
    return 0


if __name__ == "__main__":
    sys.exit(main())