#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Add BackgroundImageCopyTip 5-lang keys (feat/background-embed), inserted after the BackgroundImageStretchModeUniformToFill line."""

TIPS = {
    "zh-cn": "通过 ｢选择｣ 按钮选用的外部图片将复制到 MAA 目录 ｢Res\\Backgrounds\\Custom｣ 中，之后移动 MAA 或原图片均不影响背景显示",
    "zh-tw": "透過 ｢選擇｣ 按鈕選用的外部圖片將複製到 MAA 目錄 ｢Res\\Backgrounds\\Custom｣ 中，之後移動 MAA 或原始圖片均不影響背景顯示",
    "en-us": "External images picked via the ｢Select｣ button are copied into ｢Res\\Backgrounds\\Custom｣ inside the MAA directory, so moving MAA or the source file later will not break the background",
    "ja-jp": "｢選択｣ ボタンで選んだ外部画像は MAA ディレクトリ内の ｢Res\\Backgrounds\\Custom｣ にコピーされます。後から MAA や元画像を移動しても背景には影響しません",
    "ko-kr": "｢선택｣ 버튼으로 고른 외부 이미지는 MAA 디렉터리의 ｢Res\\Backgrounds\\Custom｣ 에 복사되며, 이후 MAA나 원본 이미지를 이동해도 배경에는 영향을 주지 않습니다",
}

ANCHOR = 'x:Key="BackgroundImageStretchModeUniformToFill"'

for lang, text in TIPS.items():
    p = f"src/MaaWpfGui/Res/Localizations/{lang}.xaml"
    content = open(p, encoding="utf-8", newline="").read()
    assert "BackgroundImageCopyTip" not in content, f"{lang} already has key"
    idx = content.index(ANCHOR)
    line_end = content.index("\n", idx) + 1
    new_line = f'    <system:String x:Key="BackgroundImageCopyTip">{text}</system:String>\r\n'
    content = content[:line_end] + new_line + content[line_end:]
    open(p, "w", encoding="utf-8", newline="").write(content)
    print(f"{lang}: inserted BackgroundImageCopyTip after UniformToFill")
