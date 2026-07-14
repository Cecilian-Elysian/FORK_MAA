@echo off
setlocal

powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0release-zip.ps1" %*
if errorlevel 1 (
    echo.
    echo [release-zip] FAILED, errorlevel=%errorlevel%
    exit /b %errorlevel%
)

pause
exit /b 0
