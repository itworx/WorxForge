#Requires -Version 5.1
<#
.SYNOPSIS
    Bootstraps the WorxForge frontend project for local development.

.DESCRIPTION
    This script validates prerequisites, prepares environment files,
    installs dependencies, verifies the build, and can optionally run
    lint, tests, or start the dev server.
#>
[CmdletBinding()]
param(
    [switch]$SkipBuild,
    [switch]$RunLint,
    [switch]$RunTests,
    [switch]$StartDevServer
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Write-Step  { Write-Host "  --> $args" -ForegroundColor Cyan   }
function Write-Ok    { Write-Host "  [OK] $args" -ForegroundColor Green  }
function Write-Fail  { Write-Host "  [FAIL] $args" -ForegroundColor Red  }

$RepoRoot = $PSScriptRoot
$FrontendDir = Join-Path $PSScriptRoot 'worxforge-web'
$NvmrcPath = Join-Path $RepoRoot '.nvmrc'

Write-Host ''
Write-Host '==============================' -ForegroundColor DarkCyan
Write-Host '  WorxForge Frontend Setup    ' -ForegroundColor DarkCyan
Write-Host '==============================' -ForegroundColor DarkCyan
Write-Host ''

# Verify Node.js
Write-Step 'Checking Node.js...'
if (-not (Get-Command node -ErrorAction SilentlyContinue)) {
    Write-Fail 'Node.js not found. Install from https://nodejs.org'
    exit 1
}
$nodeVersion = node --version
Write-Ok "Node.js $nodeVersion detected."

# Validate recommended Node.js version from .nvmrc when available
if (Test-Path $NvmrcPath) {
    $requiredNodeVersion = (Get-Content $NvmrcPath -Raw).Trim()
    if ($requiredNodeVersion) {
        $currentMajor = ($nodeVersion -replace '^v', '').Split('.')[0]
        $requiredMajor = ($requiredNodeVersion -replace '^v', '').Split('.')[0]
        if ($currentMajor -ne $requiredMajor) {
            Write-Host "  [WARN] Recommended Node.js is $requiredNodeVersion (.nvmrc), current is $nodeVersion." -ForegroundColor Yellow
        }
        else {
            Write-Ok "Node.js major version matches .nvmrc ($requiredNodeVersion)."
        }
    }
}

# Verify npm
Write-Step 'Checking npm...'
if (-not (Get-Command npm -ErrorAction SilentlyContinue)) {
    Write-Fail 'npm not found. Reinstall Node.js from https://nodejs.org'
    exit 1
}
$npmVersion = npm --version
Write-Ok "npm $npmVersion detected."

# Verify frontend directory
if (-not (Test-Path $FrontendDir)) {
    Write-Fail "Frontend directory not found: $FrontendDir"
    exit 1
}

$packageJsonPath = Join-Path $FrontendDir 'package.json'
if (-not (Test-Path $packageJsonPath)) {
    Write-Fail "package.json not found: $packageJsonPath"
    exit 1
}

Push-Location $FrontendDir
try {
    # Ensure environment file exists
    $envExamplePath = Join-Path $FrontendDir '.env.example'
    $envPath = Join-Path $FrontendDir '.env'
    if (-not (Test-Path $envPath) -and (Test-Path $envExamplePath)) {
        Write-Step 'Creating .env from .env.example...'
        Copy-Item -Path $envExamplePath -Destination $envPath
        Write-Ok 'Created .env file.'
    }
    elseif (Test-Path $envPath) {
        Write-Ok '.env file already exists.'
    }

    # Install dependencies
    $lockFilePath = Join-Path $FrontendDir 'package-lock.json'
    if (Test-Path $lockFilePath) {
        Write-Step 'Installing npm dependencies from lock file...'
        npm ci
    }
    else {
        Write-Step 'Lock file missing, installing npm dependencies...'
        npm install
    }

    if ($LASTEXITCODE -ne 0) {
        Write-Fail 'npm dependency installation failed.'
        exit 1
    }
    Write-Ok 'npm dependency install succeeded.'

    # Verify build unless explicitly skipped
    if (-not $SkipBuild) {
        Write-Step 'Verifying frontend build...'
        npm run build
        if ($LASTEXITCODE -ne 0) {
            Write-Fail 'Frontend build failed.'
            exit 1
        }
        Write-Ok 'Frontend build succeeded.'
    }
    else {
        Write-Ok 'Skipping build verification by request.'
    }

    if ($RunLint) {
        Write-Step 'Running linter...'
        npm run lint
        if ($LASTEXITCODE -ne 0) {
            Write-Fail 'Lint failed.'
            exit 1
        }
        Write-Ok 'Lint succeeded.'
    }

    if ($RunTests) {
        Write-Step 'Running tests...'
        npm run test
        if ($LASTEXITCODE -ne 0) {
            Write-Fail 'Tests failed.'
            exit 1
        }
        Write-Ok 'Tests succeeded.'
    }

    if ($StartDevServer) {
        Write-Step 'Starting Vite development server...'
        npm run dev
    }
}
finally {
    Pop-Location
}

Write-Host ''
Write-Host '  Frontend setup complete.' -ForegroundColor Green
if (-not $StartDevServer) {
    Write-Host '  Next step: cd .\worxforge-web ; npm run dev' -ForegroundColor DarkGray
}
Write-Host ''
