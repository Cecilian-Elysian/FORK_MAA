# 以管理员身份运行此脚本
# 禁用 Nahimic 服务，阻止 DLL 注入

$serviceName = "NahimicService"

Write-Host "正在停止 $serviceName ..." -ForegroundColor Cyan
Stop-Service -Name $serviceName -Force -ErrorAction SilentlyContinue
if ($?) {
    Write-Host "  ✓ 已停止" -ForegroundColor Green
} else {
    Write-Host "  ⚠ 停止失败（可能已停止）" -ForegroundColor Yellow
}

Write-Host "正在禁用 $serviceName 开机自启..." -ForegroundColor Cyan
Set-Service -Name $serviceName -StartupType Disabled -ErrorAction SilentlyContinue
if ($?) {
    Write-Host "  ✓ 已禁用" -ForegroundColor Green
} else {
    Write-Host "  ✗ 禁用失败，请手动执行：" -ForegroundColor Red
    Write-Host "    sc config $serviceName start= disabled" -ForegroundColor Gray
}

Write-Host ""
Write-Host "请重启电脑后验证 gui.log 中 BadModules 是否消失" -ForegroundColor Yellow
