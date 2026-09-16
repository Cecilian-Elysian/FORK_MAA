#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""检查 StartButton1 preDelay 500 的来源。"""
import re
import subprocess
import sys

sys.stdout.reconfigure(encoding="utf-8")

data = subprocess.run(
    ["git", "show", "c5251e220b:resource/tasks/tasks.json"],
    capture_output=True, check=True,
).stdout.decode("utf-8")

m = re.search(r'"StartButton1": \{(.*?)\n    \}', data, re.DOTALL)
print("preDelay in merge-base:", '"preDelay"' in m.group(1))

log = subprocess.run(
    ["git", "log", "--oneline", "--all", "-S", '"preDelay": 500', "--", "resource/tasks/tasks.json"],
    capture_output=True, check=True,
).stdout.decode("utf-8", errors="replace")
print("commits touching preDelay 500:")
print(log)
