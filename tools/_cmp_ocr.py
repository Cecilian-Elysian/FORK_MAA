#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""对比 CharsNameOcrReplace ocrReplace: merge-base vs ours vs theirs。"""
import re
import subprocess
import sys

sys.stdout.reconfigure(encoding="utf-8")

base = subprocess.run(
    ["git", "show", "c5251e220b:resource/tasks/tasks.json"],
    capture_output=True, check=True,
).stdout.decode("utf-8")

PAIR_RE = re.compile(r'\[\s*"((?:[^"\\]|\\.)*)"\s*,\s*"((?:[^"\\]|\\.)*)"\s*\]')

m = re.search(r'"CharsNameOcrReplace": \{.*?"ocrReplace": \[(.*?)\n        \]', base, re.DOTALL)
pairs_base = PAIR_RE.findall(m.group(1))
print("merge-base pair count:", len(pairs_base))

cur = open("resource/tasks/tasks.json", encoding="utf-8").read()
lines = cur.split("\n")
ours_seg = "\n".join(lines[1234:1530])
theirs_seg = "\n".join(lines[1531:1606])
ours_pairs = PAIR_RE.findall(ours_seg)
theirs_pairs = PAIR_RE.findall(theirs_seg)
print("ours pair count:", len(ours_pairs))
print("theirs pair count:", len(theirs_pairs))

ours_set = set(ours_pairs)
theirs_set = set(theirs_pairs)
base_set = set(pairs_base)

print()
print("=== in ours but NOT in merge-base (fork-added?) ===")
for p in ours_pairs:
    if p not in base_set:
        print(p)
print()
print("=== in ours but NOT in theirs ===")
for p in ours_pairs:
    if p not in theirs_set:
        print(p)
print()
print("=== in theirs but NOT in ours (upstream new) ===")
for p in theirs_pairs:
    if p not in ours_set:
        print(p)
