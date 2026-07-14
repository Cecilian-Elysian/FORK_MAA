# 以管理员身份运行此脚本
# 将 MAA 添加到 Nahimic DLL 注入白名单

$whiteAppsPath = "C:\ProgramData\A-Volute\A-Volute.Nahimic\Modules\Scheduled\Configurator\WhiteApps.dat"

if (-not (Test-Path $whiteAppsPath)) {
    Write-Host "未找到 Nahimic WhiteApps.dat，请确认已安装 Nahimic" -ForegroundColor Red
    exit 1
}

# 检查是否已存在 MAA 条目
$content = Get-Content $whiteAppsPath
if ($content -match "MAA\.exe") {
    Write-Host "MAA 已在白名单中" -ForegroundColor Green
    exit 0
}

# 追加 MAA 条目
Add-Content -Path $whiteAppsPath -Value "MAA|MAA.exe|0|MAA"

Write-Host "已添加 MAA.exe 到 Nahimic 白名单" -ForegroundColor Green
Write-Host ""
Write-Host "请手动重启 MAA 以生效" -ForegroundColor Yellow
Write-Host "重启后可在 gui.log 中验证：不再出现 BadModules 警告" -ForegroundColor Yellow
