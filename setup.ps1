#Requires -Version 5.1
<#
.SYNOPSIS
    First-time setup for the WorxForge backend: restores, builds, configures secrets, and optionally migrates the database.
#>
[CmdletBinding()]
param(
    [switch]$NonInteractive  # skip all prompts; useful in CI
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Write-Step  { Write-Host "  --> $args" -ForegroundColor Cyan   }
function Write-Ok    { Write-Host "  [OK] $args" -ForegroundColor Green  }
function Write-Fail  { Write-Host "  [FAIL] $args" -ForegroundColor Red  }
function Write-Warn  { Write-Host "  [WARN] $args" -ForegroundColor Yellow }
function Write-Info  { Write-Host "  $args" -ForegroundColor Gray }

$SolutionFile       = Join-Path $PSScriptRoot 'src\WorxForge.slnx'
$ApiProject         = Join-Path $PSScriptRoot 'src\WorxForge.API'
$InfraProject       = Join-Path $PSScriptRoot 'src\WorxForge.Infrastructure'

Write-Host ''
Write-Host '======================================' -ForegroundColor DarkCyan
Write-Host '   WorxForge Backend - First-Time Setup' -ForegroundColor DarkCyan
Write-Host '======================================' -ForegroundColor DarkCyan
Write-Host ''

# -- 1. Verify .NET SDK --------------------------------------------------------
Write-Step 'Checking .NET SDK...'
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Fail '.NET SDK not found. Download from https://dotnet.microsoft.com/download'
    exit 1
}
$dotnetVersion = dotnet --version
Write-Ok ".NET SDK $dotnetVersion detected."

if (-not (Test-Path $SolutionFile)) {
    Write-Fail "Solution file not found: $SolutionFile"
    exit 1
}

# -- 2. Restore local tools ---------------------------------------------------
Write-Step 'Restoring local .NET tools...'
dotnet tool restore
if ($LASTEXITCODE -ne 0) { Write-Fail 'Local tool restore failed.'; exit 1 }
Write-Ok 'Local .NET tools restored.'

# -- 2b. Ensure dotnet-ef is available ----------------------------------------
Write-Step 'Checking dotnet-ef tool...'
$efAvailable = dotnet ef --version 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Warn 'dotnet-ef not found. Installing globally (version 10.0.10)...'
    dotnet tool install --global dotnet-ef --version 10.0.10
    if ($LASTEXITCODE -ne 0) { Write-Fail 'Failed to install dotnet-ef. Install it manually: dotnet tool install --global dotnet-ef --version 10.0.10'; exit 1 }
    Write-Ok 'dotnet-ef installed.'
} else {
    Write-Ok "dotnet-ef $efAvailable detected."
}

# -- 3. Restore NuGet packages -------------------------------------------------
Write-Step 'Restoring NuGet packages...'
dotnet restore $SolutionFile --force-evaluate
if ($LASTEXITCODE -ne 0) { Write-Fail 'NuGet restore failed.'; exit 1 }
Write-Ok 'NuGet restore succeeded.'

# -- 4. Build ------------------------------------------------------------------
Write-Step 'Building solution...'
dotnet build $SolutionFile --no-restore --configuration Debug
if ($LASTEXITCODE -ne 0) { Write-Fail 'Build failed.'; exit 1 }
Write-Ok 'Build succeeded.'

# -- 5. User Secrets -----------------------------------------------------------
Write-Host ''
Write-Step 'Configuring user secrets...'

# Init is idempotent - safe to run every time
dotnet user-secrets init --project $ApiProject 2>&1 | Out-Null

$secretsList = (dotnet user-secrets list --project $ApiProject 2>&1) -join "`n"

# required secrets: key, description, example value, whether it can be skipped
$secrets = @(
    [pscustomobject]@{
        Key      = 'ConnectionStrings:DefaultConnection'
        Desc     = 'SQL Server connection string'
        Example  = 'Server=localhost;Database=WorxForge;Trusted_Connection=True;TrustServerCertificate=True'
        Optional = $false
    },
    [pscustomobject]@{
        Key      = 'Cors:AllowedOrigins:0'
        Desc     = 'Frontend origin allowed by CORS (no trailing slash)'
        Example  = 'http://localhost:5173'
        Optional = $false
    },
    [pscustomobject]@{
        Key      = 'Authentication:Google:ClientId'
        Desc     = 'Google OAuth 2.0 Client ID (needed for real Google sign-in; debug-login works without it)'
        Example  = '123456789-abc.apps.googleusercontent.com'
        Optional = $true
    }
)

$anyMissing = $false

foreach ($s in $secrets) {
    if ($secretsList -match [regex]::Escape($s.Key)) {
        Write-Ok "$($s.Key) is set."
        continue
    }

    $anyMissing = $true

    if ($NonInteractive) {
        if ($s.Optional) {
            Write-Warn "$($s.Key) not set (optional - skip if not using Google sign-in)."
        } else {
            Write-Warn "$($s.Key) not set - required. Run: dotnet user-secrets set `"$($s.Key)`" `"<value>`" --project src/WorxForge.API"
        }
        continue
    }

    Write-Host ''
    Write-Host "  $($s.Key)" -ForegroundColor White
    Write-Info $s.Desc
    Write-Info "Example: $($s.Example)"
    if ($s.Optional) { Write-Info '(Press Enter to skip)' }

    $value = Read-Host '  Value'
    $value = $value.Trim()

    if ([string]::IsNullOrEmpty($value)) {
        if ($s.Optional) {
            Write-Warn "Skipped $($s.Key)."
        } else {
            Write-Warn "$($s.Key) is required but was not provided. Set it before running the API."
        }
        continue
    }

    dotnet user-secrets set "$($s.Key)" "$value" --project $ApiProject | Out-Null
    if ($LASTEXITCODE -eq 0) {
        Write-Ok "$($s.Key) saved."
    } else {
        Write-Warn "Failed to save $($s.Key) - set it manually."
    }
}

# -- 6. Optional: apply EF Core migrations -------------------------------------
Write-Host ''

$runMigrations = $false
if (-not $NonInteractive) {
    $answer = Read-Host '  Apply EF Core database migrations now? [Y/n]'
    $runMigrations = ($answer.Trim() -eq '' -or $answer.Trim() -imatch '^y')
}

if ($runMigrations) {
    Write-Step 'Applying EF Core migrations...'
    $efArgs = @(
        'database', 'update',
        '--project', $InfraProject,
        '--startup-project', $ApiProject,
        '--context', 'WorxForge.Infrastructure.Configuration.WorxForgeDbContext'
    )
    dotnet ef @efArgs
    if ($LASTEXITCODE -ne 0) {
        Write-Fail 'Migration failed. Check that your connection string is correct and SQL Server is reachable.'
        exit 1
    }
    Write-Ok 'Database migrations applied.'
} else {
    Write-Info 'Skipped migrations. Run them when your database is ready:'
    Write-Info '  dotnet ef database update --project src\WorxForge.Infrastructure --startup-project src\WorxForge.API --context WorxForge.Infrastructure.Configuration.WorxForgeDbContext'
}

# -- 7. Done -------------------------------------------------------------------
Write-Host ''
Write-Host '  Setup complete.' -ForegroundColor Green
Write-Host ''
Write-Host '  Next steps:' -ForegroundColor White
Write-Info '  1. cd src\WorxForge.API'
Write-Info '  2. dotnet run'
Write-Info '  3. Open https://localhost:7235/swagger  (or http://localhost:5222/swagger)'
Write-Info '  4. Use POST /api/authentication/debug-login to get a dev JWT token'
Write-Host ''

Write-Host ''
Write-Host '  Backend setup complete.' -ForegroundColor Green
Write-Host ''
