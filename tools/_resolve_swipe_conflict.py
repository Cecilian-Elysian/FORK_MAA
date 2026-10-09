#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""批量处理 tasks.json 中 Swipe specialParams 冲突：ours = [200,1,2,0], theirs = [200,1,37,1]。"""
import re

with open('resource/tasks/tasks.json', 'r', encoding='utf-8') as f:
    content = f.read()

# Pattern matches ours block + theirs block for Swipe specialParams
# ours: 4 numbers separated across lines + "next" wrapping
# theirs: compact JSON with same structure
pattern_str = (
    r'<<<<<<< HEAD\n'
    r'( *)(\d+),\n *(\d+),\n *(\d+),\n *(\d+)\n *\],\n'
    r' *"next": \[\n *"#next"\n *\],\n'
    r'=======\n'
    r'( *)"specificRect": \[[^\]]+\],\n *'
    r'"rectMove": \[[^\]]+\],\n *'
    r'"specialParams": \[(\d+), (\d+), (\d+), (\d+)\],\n *'
    r'"next": \["#next"\],\n'
    r'>>>>>>> upstream/master-v2'
)
pattern = re.compile(pattern_str)

def replace_one(m):
    indent_a = m.group(1)
    new_p1 = m.group(7)
    new_p2 = m.group(8)
    new_p3 = m.group(9)
    new_p4 = m.group(10)
    return (f'{indent_a}{new_p1},\n{indent_a}{new_p2},\n'
            f'{indent_a}{new_p3},\n{indent_a}{new_p4}\n'
            f'{indent_a}],\n{indent_a}"next": [\n{indent_a}"#next"\n{indent_a}],')

new_content, count = pattern.subn(replace_one, content)
print(f'replaced {count} swipe specialParams conflicts')

with open('resource/tasks/tasks.json', 'w', encoding='utf-8') as f:
    f.write(new_content)