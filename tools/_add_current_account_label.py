#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""补 CurrentAccountLabel 5 语 key（§7.16 遗留 TODO），插在 AccountCycle key 之前。"""

LABELS = {
    "zh-cn": "当前账号：",
    "zh-tw": "目前帳號：",
    "en-us": "Current account: ",
    "ja-jp": "現在のアカウント：",
    "ko-kr": "현재 계정: ",
}

for lang, text in LABELS.items():
    p = f"src/MaaWpfGui/Res/Localizations/{lang}.xaml"
    content = open(p, encoding="utf-8").read()
    assert "CurrentAccountLabel" not in content, f"{lang} already has key"
    anchor = content.index('<system:String x:Key="AccountCycle">')
    line_end = content.rfind("\n", 0, anchor)
    new_line = f'    <system:String x:Key="CurrentAccountLabel">{text}</system:String>'
    content = content[: line_end + 1] + new_line + "\n" + content[line_end + 1 :]
    open(p, "w", encoding="utf-8", newline="").write(content)
    print(f"{lang}: inserted before AccountCycle")
