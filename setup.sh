#!/usr/bin/env bash
# First-time setup: restores, builds, configures secrets, and optionally migrates the database.
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SOLUTION_FILE="$SCRIPT_DIR/src/WorxForge.slnx"
API_PROJECT="$SCRIPT_DIR/src/WorxForge.API"
INFRA_PROJECT="$SCRIPT_DIR/src/WorxForge.Infrastructure"
NON_INTERACTIVE="${1:-}"

GREEN='\033[0;32m'; CYAN='\033[0;36m'; RED='\033[0;31m'; YELLOW='\033[0;33m'; GRAY='\033[0;37m'; RESET='\033[0m'

step() { echo -e "${CYAN}  --> $*${RESET}"; }
ok()   { echo -e "${GREEN}  [OK] $*${RESET}"; }
fail() { echo -e "${RED}  [FAIL] $*${RESET}"; }
warn() { echo -e "${YELLOW}  [WARN] $*${RESET}"; }
info() { echo -e "${GRAY}  $*${RESET}"; }

echo ""
echo -e "${CYAN}======================================${RESET}"
echo -e "${CYAN}   WorxForge Backend — First-Time Setup${RESET}"
echo -e "${CYAN}======================================${RESET}"
echo ""

# ── 1. Verify .NET SDK ────────────────────────────────────────────────────────
step "Checking .NET SDK..."
if ! command -v dotnet &>/dev/null; then
    fail ".NET SDK not found. Download from https://dotnet.microsoft.com/download"
    exit 1
fi
ok ".NET SDK $(dotnet --version) detected."

if [[ ! -f "$SOLUTION_FILE" ]]; then
    fail "Solution file not found: $SOLUTION_FILE"
    exit 1
fi

# ── 2. Restore local tools (includes dotnet-ef) ───────────────────────────────
step "Restoring local .NET tools..."
dotnet tool restore
ok "Local .NET tools restored."

# ── 3. Restore NuGet packages ─────────────────────────────────────────────────
step "Restoring NuGet packages..."
dotnet restore "$SOLUTION_FILE" --force-evaluate
ok "NuGet restore succeeded."

# ── 4. Build ──────────────────────────────────────────────────────────────────
step "Building solution..."
dotnet build "$SOLUTION_FILE" --no-restore --configuration Debug
ok "Build succeeded."

# ── 5. User Secrets ───────────────────────────────────────────────────────────
echo ""
step "Configuring user secrets..."

dotnet user-secrets init --project "$API_PROJECT" 2>/dev/null || true

SECRETS_LIST=$(dotnet user-secrets list --project "$API_PROJECT" 2>/dev/null || true)

declare -a SECRET_KEYS=( "ConnectionStrings:DefaultConnection" "Cors:AllowedOrigins:0" "Authentication:Google:ClientId" )
declare -a SECRET_DESCS=( "SQL Server connection string" "Frontend origin allowed by CORS (no trailing slash)" "Google OAuth 2.0 Client ID (needed for real Google sign-in; debug-login works without it)" )
declare -a SECRET_EXAMPLES=( "Server=localhost;Database=WorxForge;Trusted_Connection=True;TrustServerCertificate=True" "http://localhost:5173" "123456789-abc.apps.googleusercontent.com" )
declare -a SECRET_OPTIONAL=( "false" "false" "true" )

for i in "${!SECRET_KEYS[@]}"; do
    key="${SECRET_KEYS[$i]}"
    desc="${SECRET_DESCS[$i]}"
    example="${SECRET_EXAMPLES[$i]}"
    optional="${SECRET_OPTIONAL[$i]}"

    if grep -qF "$key" <<< "$SECRETS_LIST" 2>/dev/null; then
        ok "$key is set."
        continue
    fi

    if [[ "$NON_INTERACTIVE" == "--non-interactive" ]]; then
        if [[ "$optional" == "true" ]]; then
            warn "$key not set (optional)."
        else
            warn "$key not set — required."
        fi
        continue
    fi

    echo ""
    echo -e "  \033[1m$key\033[0m"
    info "$desc"
    info "Example: $example"
    [[ "$optional" == "true" ]] && info "(Press Enter to skip)"

    read -r -p "  Value: " value
    value="${value// /}"

    if [[ -z "$value" ]]; then
        if [[ "$optional" == "true" ]]; then
            warn "Skipped $key."
        else
            warn "$key is required but was not provided. Set it before running the API."
        fi
        continue
    fi

    dotnet user-secrets set "$key" "$value" --project "$API_PROJECT" > /dev/null
    ok "$key saved."
done

# ── 6. Optional: apply EF Core migrations ─────────────────────────────────────
echo ""
RUN_MIGRATIONS=false

if [[ "$NON_INTERACTIVE" != "--non-interactive" ]]; then
    read -r -p "  Apply EF Core database migrations now? [Y/n] " answer
    answer="${answer:-y}"
    [[ "$answer" =~ ^[Yy] ]] && RUN_MIGRATIONS=true
fi

if [[ "$RUN_MIGRATIONS" == "true" ]]; then
    step "Applying EF Core migrations..."
    dotnet ef database update \
        --project "$INFRA_PROJECT" \
        --startup-project "$API_PROJECT" \
        --context "WorxForge.Infrastructure.Configuration.WorxForgeDbContext"
    ok "Database migrations applied."
else
    info "Skipped migrations. Run them when your database is ready:"
    info "  dotnet ef database update --project src/WorxForge.Infrastructure --startup-project src/WorxForge.API --context WorxForge.Infrastructure.Configuration.WorxForgeDbContext"
fi

# ── 7. Done ───────────────────────────────────────────────────────────────────
echo ""
echo -e "${GREEN}  Setup complete.${RESET}"
echo ""
echo "  Next steps:"
info "  1. cd src/WorxForge.API"
info "  2. dotnet run"
info "  3. Open https://localhost:7235/swagger  (or http://localhost:5222/swagger)"
info "  4. Use POST /api/authentication/debug-login to get a dev JWT token"
echo ""
