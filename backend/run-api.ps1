<#
.SYNOPSIS
    Build and run the FarmMarketplace API for local development.

.PARAMETER NoBuild
    Skip the build step and launch the last compiled DLL directly.

.PARAMETER Port
    The HTTP port to listen on. Defaults to 5000.

.EXAMPLE
    # Build and run (standard use)
    .\run-api.ps1

.EXAMPLE
    # Skip rebuild, just run the existing binary
    .\run-api.ps1 -NoBuild
#>
param(
    [switch]$NoBuild,
    [int]$Port = 5000
)

$ErrorActionPreference = "Stop"

$projectDir  = "$PSScriptRoot\src\FarmMarketplace.Api"
$projectFile = "$projectDir\FarmMarketplace.Api.csproj"
$dll         = "$projectDir\bin\Debug\net9.0\FarmMarketplace.Api.dll"

# ── Build ────────────────────────────────────────────────────────────────────
if (-not $NoBuild) {
    Write-Host "`n[run-api] Building..." -ForegroundColor Cyan
    dotnet build $projectFile -c Debug --nologo
    if ($LASTEXITCODE -ne 0) {
        Write-Error "[run-api] Build failed. Aborting."
    }
    Write-Host "[run-api] Build succeeded.`n" -ForegroundColor Green
}

# ── Verify DLL exists ────────────────────────────────────────────────────────
if (-not (Test-Path $dll)) {
    Write-Error "[run-api] DLL not found at '$dll'. Run without -NoBuild first."
}

# ── Kill any lingering API process on the target port ────────────────────────
$listening = netstat -ano 2>$null |
    Select-String ":$Port\s" |
    ForEach-Object { ($_ -split '\s+')[-1] } |
    Select-Object -Unique

foreach ($pid in $listening) {
    if ($pid -match '^\d+$' -and $pid -ne 0) {
        try {
            Stop-Process -Id $pid -Force -ErrorAction SilentlyContinue
            Write-Host "[run-api] Stopped process $pid that was using port $Port." -ForegroundColor Yellow
        } catch { }
    }
}

# ── Launch ───────────────────────────────────────────────────────────────────
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ASPNETCORE_URLS        = "http://localhost:$Port"

Write-Host "[run-api] Starting API on http://localhost:$Port ..." -ForegroundColor Cyan
Write-Host "[run-api] Swagger UI: http://localhost:$Port/swagger`n" -ForegroundColor Cyan

Set-Location $projectDir
dotnet $dll
