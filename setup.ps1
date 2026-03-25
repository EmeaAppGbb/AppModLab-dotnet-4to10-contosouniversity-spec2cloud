<#
.SYNOPSIS
    Local development environment setup for the Contoso University Modernization Workshop.

.DESCRIPTION
    This script validates and installs prerequisites needed to run the workshop
    on a local Windows machine. It checks for required tooling and provides
    guidance for anything that needs manual installation.

.NOTES
    Run this script in an elevated PowerShell terminal (Run as Administrator)
    if you need to enable Windows features like MSMQ.
#>

param(
    [switch]$SkipMSMQ,
    [switch]$CheckOnly
)

$ErrorActionPreference = "Continue"

function Write-Header {
    param([string]$Text)
    Write-Host ""
    Write-Host "============================================" -ForegroundColor Cyan
    Write-Host "  $Text" -ForegroundColor Cyan
    Write-Host "============================================" -ForegroundColor Cyan
}

function Write-Check {
    param([string]$Name, [bool]$Found, [string]$Detail = "")
    if ($Found) {
        Write-Host "  [OK] $Name" -ForegroundColor Green -NoNewline
        if ($Detail) { Write-Host " ($Detail)" -ForegroundColor Gray } else { Write-Host "" }
    }
    else {
        Write-Host "  [MISSING] $Name" -ForegroundColor Red -NoNewline
        if ($Detail) { Write-Host " - $Detail" -ForegroundColor Yellow } else { Write-Host "" }
    }
    return $Found
}

$allGood = $true

Write-Header "Contoso University Workshop - Environment Check"

# ---------- .NET Framework 4.8 ----------
Write-Host ""
Write-Host "--- .NET Framework ---" -ForegroundColor White
$netFxVersion = (Get-ItemProperty "HKLM:\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full" -ErrorAction SilentlyContinue).Release
$hasFx48 = $netFxVersion -ge 528040
$allGood = (Write-Check ".NET Framework 4.8+" $hasFx48 $(if ($hasFx48) { "Release $netFxVersion" } else { "Install from https://dotnet.microsoft.com/download/dotnet-framework/net48" })) -and $allGood

# ---------- .NET 9/10 SDK ----------
Write-Host ""
Write-Host "--- .NET Modern SDK ---" -ForegroundColor White
$dotnetSdks = @()
try { $dotnetSdks = & dotnet --list-sdks 2>$null } catch {}
$hasModernSdk = ($dotnetSdks | Where-Object { $_ -match "^(9|10)\." }).Count -gt 0
$sdkDetail = if ($hasModernSdk) { ($dotnetSdks | Where-Object { $_ -match "^(9|10)\." } | Select-Object -First 1) } else { "Install from https://dotnet.microsoft.com/download" }
$allGood = (Write-Check ".NET 9+ SDK" $hasModernSdk $sdkDetail) -and $allGood

# ---------- Visual Studio or VS Code ----------
Write-Host ""
Write-Host "--- IDE ---" -ForegroundColor White
$hasVSCode = $null -ne (Get-Command code -ErrorAction SilentlyContinue)
$hasVS = Test-Path "${env:ProgramFiles}\Microsoft Visual Studio\2022" -ErrorAction SilentlyContinue
$hasIDE = $hasVSCode -or $hasVS
$ideDetail = @()
if ($hasVSCode) { $ideDetail += "VS Code" }
if ($hasVS) { $ideDetail += "Visual Studio 2022" }
$allGood = (Write-Check "IDE (VS Code or Visual Studio)" $hasIDE $(if ($hasIDE) { $ideDetail -join ", " } else { "Install VS Code from https://code.visualstudio.com/" })) -and $allGood

# ---------- Git ----------
Write-Host ""
Write-Host "--- Git ---" -ForegroundColor White
$hasGit = $null -ne (Get-Command git -ErrorAction SilentlyContinue)
$gitVersion = if ($hasGit) { (& git --version 2>$null) } else { "" }
$allGood = (Write-Check "Git" $hasGit $(if ($hasGit) { $gitVersion } else { "Install from https://git-scm.com/" })) -and $allGood

# ---------- SQL Server LocalDB ----------
Write-Host ""
Write-Host "--- SQL Server LocalDB ---" -ForegroundColor White
$hasSqlLocalDb = $null -ne (Get-Command SqlLocalDB -ErrorAction SilentlyContinue)
$sqlDetail = ""
if ($hasSqlLocalDb) {
    $instances = & SqlLocalDB info 2>$null
    $sqlDetail = "Instances: $($instances -join ', ')"
}
else {
    $sqlDetail = "Install with Visual Studio or from https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb"
}
$allGood = (Write-Check "SQL Server LocalDB" $hasSqlLocalDb $sqlDetail) -and $allGood

# ---------- Node.js (for npx spec2cloud) ----------
Write-Host ""
Write-Host "--- Node.js ---" -ForegroundColor White
$hasNode = $null -ne (Get-Command node -ErrorAction SilentlyContinue)
$nodeVersion = if ($hasNode) { (& node --version 2>$null) } else { "" }
$allGood = (Write-Check "Node.js" $hasNode $(if ($hasNode) { $nodeVersion } else { "Install from https://nodejs.org/" })) -and $allGood

$hasNpx = $null -ne (Get-Command npx -ErrorAction SilentlyContinue)
$allGood = (Write-Check "npx (comes with Node.js)" $hasNpx) -and $allGood

# ---------- GitHub Copilot CLI ----------
Write-Host ""
Write-Host "--- GitHub Copilot ---" -ForegroundColor White
$hasCopilot = $null -ne (Get-Command copilot -ErrorAction SilentlyContinue)
$allGood = (Write-Check "GitHub Copilot CLI" $hasCopilot $(if (-not $hasCopilot) { "Install the GitHub Copilot extension in VS Code" })) -and $allGood

# ---------- MSMQ (optional) ----------
if (-not $SkipMSMQ) {
    Write-Host ""
    Write-Host "--- MSMQ (for legacy notification system) ---" -ForegroundColor White
    $msmqFeature = Get-WindowsOptionalFeature -Online -FeatureName MSMQ-Server -ErrorAction SilentlyContinue
    $hasMSMQ = $msmqFeature -and ($msmqFeature.State -eq "Enabled")
    if (-not $hasMSMQ -and -not $CheckOnly) {
        Write-Host "  [INFO] MSMQ is optional (needed only to run the legacy app as-is)" -ForegroundColor Yellow
        Write-Host "  [INFO] To enable: Enable-WindowsOptionalFeature -Online -FeatureName MSMQ-Server" -ForegroundColor Yellow
    }
    else {
        Write-Check "MSMQ (optional)" $hasMSMQ "Only needed for legacy notification feature"
    }
}

# ---------- Summary ----------
Write-Header "Summary"
if ($allGood) {
    Write-Host ""
    Write-Host "  All prerequisites are installed! You're ready for the workshop." -ForegroundColor Green
    Write-Host ""
}
else {
    Write-Host ""
    Write-Host "  Some prerequisites are missing. Please install them before the workshop." -ForegroundColor Yellow
    Write-Host "  Items marked [MISSING] above need to be addressed." -ForegroundColor Yellow
    Write-Host ""
}

if (-not $CheckOnly) {
    Write-Header "VS Code Extensions"
    if ($hasVSCode) {
        Write-Host "  Installing recommended VS Code extensions..." -ForegroundColor White
        $extensions = @(
            "ms-dotnettools.csharp",
            "ms-dotnettools.csdevkit",
            "ms-dotnettools.vscode-dotnet-runtime",
            "ms-azuretools.vscode-azureappservice",
            "ms-azuretools.vscode-azureresourcegroups",
            "github.copilot",
            "github.copilot-chat",
            "humao.rest-client",
            "eamodio.gitlens",
            "ms-mssql.mssql"
        )
        foreach ($ext in $extensions) {
            Write-Host "    Installing $ext..." -ForegroundColor Gray
            & code --install-extension $ext --force 2>$null | Out-Null
        }
        Write-Host "  Done!" -ForegroundColor Green
    }
    else {
        Write-Host "  Skipped (VS Code not found)." -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "Next steps:" -ForegroundColor White
Write-Host "  1. Open this repository in VS Code:  code ." -ForegroundColor Gray
Write-Host "  2. Read the DEVELOPER_GUIDE.md for setup details" -ForegroundColor Gray
Write-Host "  3. Follow the workshop steps in README.MD" -ForegroundColor Gray
Write-Host ""
