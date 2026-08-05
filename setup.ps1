#Requires -Version 5.1
<#
.SYNOPSIS
    Restores and builds the WorxForge backend solution.
#>
[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Write-Step  { Write-Host "  --> $args" -ForegroundColor Cyan   }
function Write-Ok    { Write-Host "  [OK] $args" -ForegroundColor Green  }
function Write-Fail  { Write-Host "  [FAIL] $args" -ForegroundColor Red  }

$SolutionFile  = Join-Path $PSScriptRoot 'src\WorxForge.slnx'
$ApiProject    = Join-Path $PSScriptRoot 'src\WorxForge.API'

Write-Host ''
Write-Host '==============================' -ForegroundColor DarkCyan
Write-Host '  WorxForge Backend Setup     ' -ForegroundColor DarkCyan
Write-Host '==============================' -ForegroundColor DarkCyan
Write-Host ''

# Verify .NET SDK
Write-Step 'Checking .NET SDK...'
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Fail '.NET SDK not found. Install from https://dotnet.microsoft.com/download'
    exit 1
}
$dotnetVersion = dotnet --version
Write-Ok ".NET SDK $dotnetVersion detected."

# Verify solution file exists
if (-not (Test-Path $SolutionFile)) {
    Write-Fail "Solution file not found: $SolutionFile"
    exit 1
}

# Restore local tools
Write-Step 'Restoring local .NET tools...'
dotnet tool restore
if ($LASTEXITCODE -ne 0) {
    Write-Fail 'Local tool restore failed.'
    exit 1
}
Write-Ok 'Local .NET tools restored.'

# Restore
Write-Step 'Restoring NuGet packages and evaluating package versions...'
dotnet restore $SolutionFile --force-evaluate
if ($LASTEXITCODE -ne 0) {
    Write-Fail 'NuGet restore failed.'
    exit 1
}
Write-Ok 'NuGet restore succeeded.'

# Build
Write-Step 'Building backend solution...'
dotnet build $SolutionFile --no-restore --configuration Debug
if ($LASTEXITCODE -ne 0) {
    Write-Fail 'Backend build failed.'
    exit 1
}
Write-Ok 'Backend build succeeded.'

# Check user-secrets
Write-Host ''
Write-Step 'Checking required user-secrets...'

$requiredSecrets = @(
    'JwtAccessToken:Issuer',
    'JwtAccessToken:Audience',
    'JwtAccessToken:SigningKey',
    'JwtAccessToken:ExpirationMinutes',
    'ConnectionStrings:Default',
    'Authentication:Google:ClientId'
)

$secretsJson = dotnet user-secrets list --project $ApiProject 2>$null
$missingSecrets = @()

foreach ($secret in $requiredSecrets) {
    if ($secretsJson -notmatch [regex]::Escape($secret)) {
        $missingSecrets += $secret
    }
}

if ($missingSecrets.Count -gt 0) {
    Write-Host ''
    Write-Host '  [WARN] The following user-secrets are not set:' -ForegroundColor Yellow
    foreach ($s in $missingSecrets) {
        Write-Host "         dotnet user-secrets set `"$s`" `"<value>`" --project src/WorxForge.API" -ForegroundColor Yellow
    }
    Write-Host ''
    Write-Host '  See docs/architecture/authentication.md for required values.' -ForegroundColor Yellow
} else {
    Write-Ok 'All required user-secrets are present.'
}

Write-Host ''
Write-Host '  Backend setup complete.' -ForegroundColor Green
Write-Host ''
