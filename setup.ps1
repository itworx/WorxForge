#Requires -Version 5.1
<#
.SYNOPSIS
    First-time setup for the WorxForge monorepo (backend + frontend).

.DESCRIPTION
    Runs backend setup (restore, build, user secrets, optional EF migration) and
    frontend setup (env config, npm install, build, optional lint). By default both
    are run; use -BackendOnly or -FrontendOnly to run just one side.

.PARAMETER NonInteractive
    Skip all interactive prompts; intended for CI. Missing backend secrets are
    reported as warnings; the frontend .env.local is copied from .env.example as-is.

.PARAMETER BackendOnly
    Run only the backend setup.

.PARAMETER FrontendOnly
    Run only the frontend setup.

.PARAMETER SkipBuild
    Frontend only: skip the build verification step.

.PARAMETER StartDevServer
    Frontend only: start the Vite development server after setup completes.
#>
[CmdletBinding()]
param(
    [switch]$NonInteractive,
    [switch]$BackendOnly,
    [switch]$FrontendOnly,
    [switch]$SkipBuild,
    [switch]$StartDevServer
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Write-Step  { Write-Host "  --> $args" -ForegroundColor Cyan   }
function Write-Ok    { Write-Host "  [OK] $args" -ForegroundColor Green  }
function Write-Fail  { Write-Host "  [FAIL] $args" -ForegroundColor Red  }
function Write-Warn  { Write-Host "  [WARN] $args" -ForegroundColor Yellow }
function Write-Info  { Write-Host "  $args" -ForegroundColor Gray }

if ($BackendOnly -and $FrontendOnly) {
    Write-Fail 'Use only one of -BackendOnly or -FrontendOnly (omit both to run everything).'
    exit 1
}
$runBackend  = -not $FrontendOnly
$runFrontend = -not $BackendOnly

Write-Host ''
Write-Host '==========================================' -ForegroundColor DarkCyan
Write-Host '   WorxForge Monorepo - First-Time Setup' -ForegroundColor DarkCyan
Write-Host '==========================================' -ForegroundColor DarkCyan
Write-Host ''

# =============================================================================
#  BACKEND
# =============================================================================
if ($runBackend) {
    $SolutionFile = Join-Path $PSScriptRoot 'src\WorxForge.slnx'
    $ApiProject   = Join-Path $PSScriptRoot 'src\WorxForge.API'
    $InfraProject = Join-Path $PSScriptRoot 'src\WorxForge.Infrastructure'

    Write-Host '------------------------------------------' -ForegroundColor DarkCyan
    Write-Host '  Backend setup' -ForegroundColor DarkCyan
    Write-Host '------------------------------------------' -ForegroundColor DarkCyan
    Write-Host ''

    # -- 1. Verify .NET SDK ----------------------------------------------------
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

    # -- 2. Restore local tools ------------------------------------------------
    Write-Step 'Restoring local .NET tools...'
    dotnet tool restore
    if ($LASTEXITCODE -ne 0) { Write-Fail 'Local tool restore failed.'; exit 1 }
    Write-Ok 'Local .NET tools restored.'

    # -- 2b. Ensure dotnet-ef is available -------------------------------------
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

    # -- 3. Restore NuGet packages ---------------------------------------------
    Write-Step 'Restoring NuGet packages...'
    dotnet restore $SolutionFile --force-evaluate
    if ($LASTEXITCODE -ne 0) { Write-Fail 'NuGet restore failed.'; exit 1 }
    Write-Ok 'NuGet restore succeeded.'

    # -- 4. Build --------------------------------------------------------------
    Write-Step 'Building solution...'
    dotnet build $SolutionFile --no-restore --configuration Debug
    if ($LASTEXITCODE -ne 0) { Write-Fail 'Build failed.'; exit 1 }
    Write-Ok 'Build succeeded.'

    # -- 5. User Secrets -------------------------------------------------------
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

    foreach ($s in $secrets) {
        if ($secretsList -match [regex]::Escape($s.Key)) {
            Write-Ok "$($s.Key) is set."
            continue
        }

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

    # -- 6. Optional: apply EF Core migrations ---------------------------------
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

    Write-Host ''
    Write-Host '  Backend setup complete.' -ForegroundColor Green
    Write-Info '  Run the API:  cd src\WorxForge.API  ->  dotnet run'
    Write-Info '  Swagger:      https://localhost:7235/swagger  (or http://localhost:5222/swagger)'
    Write-Info '  Dev JWT:      POST /api/authentication/debug-login'
    Write-Host ''
}

# =============================================================================
#  FRONTEND
# =============================================================================
if ($runFrontend) {
    $FrontendDir = Join-Path $PSScriptRoot 'src\WorxForge.UI'
    $NvmrcPath   = Join-Path $PSScriptRoot '.nvmrc'

    Write-Host '------------------------------------------' -ForegroundColor DarkCyan
    Write-Host '  Frontend setup' -ForegroundColor DarkCyan
    Write-Host '------------------------------------------' -ForegroundColor DarkCyan
    Write-Host ''

    # -- Node.js ---------------------------------------------------------------
    Write-Step 'Checking Node.js...'
    if (-not (Get-Command node -ErrorAction SilentlyContinue)) {
        Write-Fail 'Node.js not found. Install from https://nodejs.org'
        exit 1
    }
    $nodeVersion = node --version
    Write-Ok "Node.js $nodeVersion detected."

    if (Test-Path $NvmrcPath) {
        $requiredNodeVersion = (Get-Content $NvmrcPath -Raw).Trim()
        if ($requiredNodeVersion) {
            $currentMajor  = ($nodeVersion        -replace '^v', '').Split('.')[0]
            $requiredMajor = ($requiredNodeVersion -replace '^v', '').Split('.')[0]
            if ($currentMajor -ne $requiredMajor) {
                Write-Warn "Recommended Node.js is $requiredNodeVersion (.nvmrc), current is $nodeVersion."
            } else {
                Write-Ok "Node.js major version matches .nvmrc ($requiredNodeVersion)."
            }
        }
    }

    # -- npm -------------------------------------------------------------------
    Write-Step 'Checking npm...'
    if (-not (Get-Command npm -ErrorAction SilentlyContinue)) {
        Write-Fail 'npm not found. Reinstall Node.js from https://nodejs.org'
        exit 1
    }
    $npmVersion = npm --version
    Write-Ok "npm $npmVersion detected."

    # -- Frontend directory ----------------------------------------------------
    if (-not (Test-Path $FrontendDir)) {
        Write-Fail "Frontend directory not found: $FrontendDir"
        exit 1
    }
    if (-not (Test-Path (Join-Path $FrontendDir 'package.json'))) {
        Write-Fail "package.json not found in $FrontendDir"
        exit 1
    }

    Push-Location $FrontendDir
    try {

        # -- .env.local --------------------------------------------------------
        $envExamplePath = Join-Path $FrontendDir '.env.example'
        $envLocalPath   = Join-Path $FrontendDir '.env.local'

        if (Test-Path $envLocalPath) {
            Write-Ok '.env.local already exists -- skipping env setup.'
        } elseif (-not (Test-Path $envExamplePath)) {
            Write-Warn '.env.example not found -- skipping env setup.'
        } elseif ($NonInteractive) {
            Write-Step 'Non-interactive mode: copying .env.example to .env.local...'
            Copy-Item -Path $envExamplePath -Destination $envLocalPath
            Write-Ok 'Created .env.local (fill in the required values before running npm run dev).'
        } else {
            Write-Step 'Configuring environment variables...'
            Write-Host ''
            Write-Host '  The following values will be written to .env.local.' -ForegroundColor DarkGray
            Write-Host '  Press Enter to accept the shown default, or type a new value.' -ForegroundColor DarkGray
            Write-Host ''

            $envContent = Get-Content $envExamplePath -Raw

            Write-Host '  VITE_API_BASE_URL' -ForegroundColor White
            Write-Host '    Backend API base URL used at runtime by the browser.' -ForegroundColor DarkGray
            Write-Host '    Leave empty (default) to use the Vite /api proxy in dev --' -ForegroundColor DarkGray
            Write-Host '    the proxy target is set separately via VITE_API_PROXY_TARGET.' -ForegroundColor DarkGray
            Write-Host '    Example: https://localhost:7235  (or empty for local dev)' -ForegroundColor DarkGray
            $apiBaseUrl = Read-Host '    Value [leave empty for proxy mode]'

            Write-Host ''
            Write-Host '  VITE_API_PROXY_TARGET' -ForegroundColor White
            Write-Host '    Vite dev proxy target -- the actual backend URL that /api' -ForegroundColor DarkGray
            Write-Host '    requests are forwarded to when VITE_API_BASE_URL is empty.' -ForegroundColor DarkGray
            Write-Host '    Example: https://localhost:7235' -ForegroundColor DarkGray
            $proxyTarget = Read-Host '    Value [https://localhost:7235]'
            if ([string]::IsNullOrWhiteSpace($proxyTarget)) { $proxyTarget = 'https://localhost:7235' }

            Write-Host ''
            Write-Host '  VITE_GOOGLE_CLIENT_ID' -ForegroundColor White
            Write-Host '    Google OAuth 2.0 Client ID from Google Cloud Console.' -ForegroundColor DarkGray
            Write-Host '    Required for the Google Sign-In button to render.' -ForegroundColor DarkGray
            Write-Host '    Example: 123456789-abc.apps.googleusercontent.com' -ForegroundColor DarkGray
            $googleClientId = Read-Host '    Value'

            $envContent = $envContent -replace '(?m)^(VITE_API_BASE_URL=).*$',     "VITE_API_BASE_URL=$apiBaseUrl"
            $envContent = $envContent -replace '(?m)^(VITE_API_PROXY_TARGET=).*$', "VITE_API_PROXY_TARGET=$proxyTarget"
            $envContent = $envContent -replace '(?m)^(VITE_GOOGLE_CLIENT_ID=).*$', "VITE_GOOGLE_CLIENT_ID=$googleClientId"

            [System.IO.File]::WriteAllText($envLocalPath, $envContent, [System.Text.Encoding]::UTF8)
            Write-Ok 'Created .env.local.'
            Write-Host ''
        }

        # -- Dependencies ------------------------------------------------------
        Write-Step 'Installing npm dependencies...'
        $lockFilePath = Join-Path $FrontendDir 'package-lock.json'
        if (Test-Path $lockFilePath) {
            npm ci
        } else {
            npm install
        }
        Write-Ok 'npm dependency install succeeded.'

        # -- Build -------------------------------------------------------------
        if (-not $SkipBuild) {
            Write-Step 'Verifying frontend build...'
            npm run build
            Write-Ok 'Frontend build succeeded.'
        } else {
            Write-Ok 'Skipping build verification by request.'
        }

        # -- Lint (interactive prompt) -----------------------------------------
        if (-not $NonInteractive) {
            Write-Host ''
            $runLint = Read-Host '  Run linter now? [y/N]'
            if ($runLint -match '^[Yy]') {
                Write-Step 'Running linter...'
                npm run lint
                Write-Ok 'Lint succeeded.'
            }
        }

        # -- Dev server --------------------------------------------------------
        if ($StartDevServer) {
            Write-Step 'Starting Vite development server...'
            npm run dev
        }

    } finally {
        Pop-Location
    }

    Write-Host ''
    Write-Host '  Frontend setup complete.' -ForegroundColor Green
    if (-not $StartDevServer) {
        Write-Info '  Run the app:  cd src\WorxForge.UI  ->  npm run dev'
        Write-Info '  Then open:    http://localhost:5173'
    }
    Write-Host ''
}

Write-Host ''
Write-Host '  WorxForge setup complete.' -ForegroundColor Green
Write-Host ''
