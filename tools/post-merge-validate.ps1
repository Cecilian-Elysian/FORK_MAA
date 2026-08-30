# post-merge-validate.ps1
# Purpose: After merging upstream/master-v2, validate that fork-specific features are intact.
# Usage: powershell -ExecutionPolicy Bypass -File tools/post-merge-validate.ps1
# Exit codes: 0 = all pass, 1 = failures
#
# Automated version of AGENTS.md section 6.5.8 verification checklist.
# Fork marker baseline check is in WORKFLOW.md section 2.5, run before/after merge.

$ErrorActionPreference = "Stop"
$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")

Write-Host "=== Fork Private Feature Validation ===" -ForegroundColor Cyan

$failed = $false
$warnings = $false

# ---------- 1. 5-language xaml fork key check ----------
Write-Host ""
Write-Host "[1] 5-language xaml fork key integrity..." -ForegroundColor Yellow

$requiredKeys = @(
    # feat/account_rotation: AccountCycle series
    "AccountCycle", "AccountCycleTip", "AccountCycleAddNewAccount",
    "AccountCycleRemoveTip", "AccountCycleRemoveConfirm", "AccountCycleRemoveMessage",
    # feat/defer-rogue: LateStage
    "LateStageRogueAndReclamation", "LateStageRogueAndReclamationTip",
    # feat/expedite-threshold: ExpediteMode
    "ExpediteMode", "ExpediteModeTip", "ExpediteModeDisabled",
    "ExpediteModeAll", "ExpediteMode4", "ExpediteMode5", "ExpediteMode6",
    # feat/diagnostic-export: GenerateDiagnosticReport
    "GenerateDiagnosticReport", "GenerateDiagnosticReportSelectLocation",
    "DiagnosticDateRange",
    # feat/account-scoped-recognition-data
    "DataAccountLabel", "DataAccountDefault",
    # feat/copilot-paste-clipboard
    "PasteClipboardCopilotSetTip",
    # feat/recruit-history-tab
    "ToolboxTabRecruitHistory"
)

$forbiddenKeys = @(
    # feat/auto-recruit-3star-to-4star removed (Phase A)
    "AutoUseExpedited", "AutoUseExpeditedTip",
    # feat/account_rotation supersede: AccountSwitch replaced by AccountCycle
    "AccountSwitch", "AccountSwitchManualRun", "AccountSwitchTip"
)

$xamlFiles = @("zh-cn.xaml", "zh-tw.xaml", "en-us.xaml", "ja-jp.xaml", "ko-kr.xaml")
$xamlDir = Join-Path $repoRoot "src\MaaWpfGui\Res\Localizations"

foreach ($file in $xamlFiles) {
    $path = Join-Path $xamlDir $file
    if (-not (Test-Path $path)) {
        Write-Host "  [FAIL] Missing file: $file" -ForegroundColor Red
        $failed = $true
        continue
    }
    $content = Get-Content $path -Raw

    foreach ($key in $requiredKeys) {
        if ($content -notmatch "x:Key=`"$key`"") {
            Write-Host "  [FAIL] $file missing key: $key" -ForegroundColor Red
            $failed = $true
        }
    }
    foreach ($key in $forbiddenKeys) {
        if ($content -match "x:Key=`"$key`"") {
            Write-Host "  [WARN] $file contains deleted key: $key" -ForegroundColor Yellow
            $warnings = $true
        }
    }
}

if (-not $failed) {
    Write-Host "  [OK] 5-language xaml fork keys complete" -ForegroundColor Green
}

# ---------- 2. C++ fork marker check ----------
Write-Host ""
Write-Host "[2] C++ fork markers (expedite_min_level)..." -ForegroundColor Yellow

$cppChecks = @(
    @{ Path = "src\MaaCore\Task\Miscellaneous\AutoRecruitTask.h"; Pattern = "m_expedite_min_level"; MinCount = 1; Note = "member variable" },
    @{ Path = "src\MaaCore\Task\Miscellaneous\AutoRecruitTask.h"; Pattern = "set_expedite_min_level"; MinCount = 1; Note = "setter declaration" },
    @{ Path = "src\MaaCore\Task\Miscellaneous\AutoRecruitTask.cpp"; Pattern = "set_expedite_min_level"; MinCount = 1; Note = "setter impl" },
    @{ Path = "src\MaaCore\Task\Miscellaneous\AutoRecruitTask.cpp"; Pattern = "m_last_confirmed_min_level"; MinCount = 2; Note = "level state" },
    @{ Path = "src\MaaCore\Task\Interface\RecruitTask.cpp"; Pattern = "expedite_min_level"; MinCount = 2; Note = "param pass-through" }
)

foreach ($check in $cppChecks) {
    $path = Join-Path $repoRoot $check.Path
    if (Test-Path $path) {
        $count = (Select-String -Path $path -Pattern $check.Pattern -SimpleMatch).Count
        if ($count -lt $check.MinCount) {
            Write-Host "  [FAIL] $($check.Path) [$($check.Note)] hits $count < $($check.MinCount)" -ForegroundColor Red
            $failed = $true
        } else {
            Write-Host "  [OK]   $($check.Path) [$($check.Note)] hits $count" -ForegroundColor Green
        }
    } else {
        Write-Host "  [WARN] Missing file: $($check.Path)" -ForegroundColor Yellow
        $warnings = $true
    }
}

# ---------- 3. tasks.json custom tasks ----------
Write-Host ""
Write-Host "[3] tasks.json RecruitNow@Slot custom tasks..." -ForegroundColor Yellow

$tasksPath = Join-Path $repoRoot "resource\tasks\tasks.json"
if (Test-Path $tasksPath) {
    $tasksContent = Get-Content $tasksPath -Raw
    for ($i = 0; $i -le 3; $i++) {
        $key = "RecruitNow@Slot$i"
        if ($tasksContent -notmatch "`"$key`"") {
            Write-Host "  [FAIL] Missing task: $key" -ForegroundColor Red
            $failed = $true
        }
    }
    if ($tasksContent -match "AccountManagerPageConfirm") {
        Write-Host "  [WARN] Contains deleted task AccountManagerPageConfirm" -ForegroundColor Yellow
        $warnings = $true
    }
    if (-not $failed) {
        Write-Host "  [OK]   RecruitNow@Slot0..3 complete + AccountManagerPageConfirm removed" -ForegroundColor Green
    }
} else {
    Write-Host "  [FAIL] tasks.json not found" -ForegroundColor Red
    $failed = $true
}

# ---------- 4. docs/protocol/integration.md field docs ----------
Write-Host ""
Write-Host "[4] integration.md expedite_min_level field docs..." -ForegroundColor Yellow

$docFiles = @("zh-cn", "zh-tw", "en-us", "ja-jp", "ko-kr")
foreach ($lang in $docFiles) {
    $path = Join-Path $repoRoot "docs\$lang\protocol\integration.md"
    if (Test-Path $path) {
        $content = Get-Content $path -Raw
        if ($content -notmatch "expedite_min_level") {
            Write-Host "  [FAIL] docs/$lang/protocol/integration.md missing expedite_min_level" -ForegroundColor Red
            $failed = $true
        }
    } else {
        Write-Host "  [WARN] Missing docs/$lang/protocol/integration.md" -ForegroundColor Yellow
        $warnings = $true
    }
}
if (-not $failed) {
    Write-Host "  [OK]   5-language integration.md field docs complete" -ForegroundColor Green
}

# ---------- 5. Roguelike onErrorNext hardening ----------
Write-Host ""
Write-Host "[5] Roguelike onErrorNext hardening..." -ForegroundColor Yellow

# Note: tasks.json literals contain double quotes, git grep fails in PS 5.1, fall back to content search
$rogueFiles = @(
    @{ Path = "resource\tasks\Roguelike\base.json"; Marker = "Roguelike@ExitThenAbandon"; Note = "base.json stage fallback" },
    @{ Path = "resource\tasks\Roguelike\JieGarden.json"; Marker = "JieGarden@Roguelike@ExitThenAbandon"; Note = "JieGarden.json stage fallback" }
)

foreach ($check in $rogueFiles) {
    $path = Join-Path $repoRoot $check.Path
    if (Test-Path $path) {
        $content = Get-Content $path -Raw
        if ($content -notmatch [regex]::Escape($check.Marker)) {
            Write-Host "  [FAIL] $($check.Path) missing $($check.Marker)" -ForegroundColor Red
            $failed = $true
        } else {
            Write-Host "  [OK]   $($check.Path) [$($check.Note)] hardening preserved" -ForegroundColor Green
        }
    }
}

# ---------- 6. partial class split verification ----------
Write-Host ""
Write-Host "[6] TaskQueueViewModel partial class split..." -ForegroundColor Yellow

$accountCycleFile = Join-Path $repoRoot "src\MaaWpfGui\ViewModels\UI\TaskQueueViewModel.AccountCycle.cs"
if (Test-Path $accountCycleFile) {
    $acContent = Get-Content $accountCycleFile -Raw
    if ($acContent -match "AdvanceAccountCycle\(\)") {
        Write-Host "  [OK]   AdvanceAccountCycle extracted to partial class" -ForegroundColor Green
    } else {
        Write-Host "  [FAIL] TaskQueueViewModel.AccountCycle.cs missing AdvanceAccountCycle" -ForegroundColor Red
        $failed = $true
    }
} else {
    Write-Host "  [WARN] partial class file not found (refactor not applied)" -ForegroundColor Yellow
    $warnings = $true
}

# ---------- 7. NetBeauty2 post-processing check ----------
# 2026-08-26 实测漏跑此步会导致 MAA.exe 启动闪退:
#   System.IO.FileNotFoundException: Could not load file or assembly 'libloader'
# 详见 AGENTS.md §4.1.1
Write-Host ""
Write-Host "[7] NetBeauty2 post-processing (install-staging MAA.runtimeconfig.json)..." -ForegroundColor Yellow

$runtimeConfigPath = Join-Path $repoRoot "install-staging\MAA.runtimeconfig.json"
$exePath = Join-Path $repoRoot "install-staging\MAA.exe"

if (-not (Test-Path $runtimeConfigPath)) {
    Write-Host "  [WARN] install-staging/MAA.runtimeconfig.json not found (build first)" -ForegroundColor Yellow
    $warnings = $true
} elseif (-not (Test-Path $exePath)) {
    Write-Host "  [WARN] install-staging/MAA.exe not found (build first)" -ForegroundColor Yellow
    $warnings = $true
} else {
    $rcContent = Get-Content $runtimeConfigPath -Raw
    if ($rcContent -match "STARTUP_HOOKS") {
        Write-Host "  [OK]   MAA.runtimeconfig.json contains STARTUP_HOOKS" -ForegroundColor Green
    } else {
        Write-Host "  [FAIL] MAA.runtimeconfig.json MISSING STARTUP_HOOKS!" -ForegroundColor Red
        Write-Host "         This means NetBeauty2 post-processing was NOT run." -ForegroundColor Red
        Write-Host "         MAA.exe will crash on startup with:" -ForegroundColor Red
        Write-Host "         System.IO.FileNotFoundException: Could not load file or assembly 'libloader'" -ForegroundColor Red
        Write-Host "         Fix: run nbeauty2.exe manually (see AGENTS.md section 4.1.1)" -ForegroundColor Red
        $nbeautyBin = "$env:USERPROFILE\.nuget\packages\nulastudio.netbeauty\2.1.5\tools\win-x64\nbeauty2.exe"
        if (Test-Path $nbeautyBin) {
            Write-Host "         Quick fix:" -ForegroundColor Yellow
            Write-Host "           & '$nbeautyBin' --usepatch '$PWD\install-staging/.' './externals'" -ForegroundColor Yellow
        }
        $failed = $true
    }

    if ($rcContent -match "NetBeautyLibsDir") {
        Write-Host "  [OK]   MAA.runtimeconfig.json contains NetBeautyLibsDir" -ForegroundColor Green
    } else {
        Write-Host "  [WARN] MAA.runtimeconfig.json missing NetBeautyLibsDir (externals/ DLLs may not load)" -ForegroundColor Yellow
        $warnings = $true
    }
}

# ---------- Summary ----------
Write-Host ""
Write-Host "=== Validation Complete ===" -ForegroundColor Cyan

if ($failed) {
    Write-Host "[FAIL] Failures found, please review!" -ForegroundColor Red
    exit 1
} elseif ($warnings) {
    Write-Host "[WARN] Warnings found, suggest review" -ForegroundColor Yellow
    exit 0
} else {
    Write-Host "[OK]   All fork private features intact" -ForegroundColor Green
    exit 0
}