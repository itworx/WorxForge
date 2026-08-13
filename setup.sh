#!/usr/bin/env bash
# First-time setup for the WorxForge monorepo (backend + frontend).
# By default both are set up. Use --backend-only or --frontend-only for just one side.
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

RUN_BACKEND=true
RUN_FRONTEND=true
NON_INTERACTIVE=false
SKIP_BUILD=false
START_DEV_SERVER=false

for arg in "$@"; do
  case "$arg" in
    --non-interactive)  NON_INTERACTIVE=true ;;
    --backend-only)     RUN_FRONTEND=false ;;
    --frontend-only)    RUN_BACKEND=false ;;
    --skip-build)       SKIP_BUILD=true ;;
    --start-dev-server) START_DEV_SERVER=true ;;
  esac
done

if [[ "$RUN_BACKEND" == "false" && "$RUN_FRONTEND" == "false" ]]; then
    echo "Use only one of --backend-only or --frontend-only (omit both to run everything)." >&2
    exit 1
fi

GREEN='\033[0;32m'; CYAN='\033[0;36m'; RED='\033[0;31m'; YELLOW='\033[0;33m'; GRAY='\033[0;37m'; WHITE='\033[1;37m'; RESET='\033[0m'

step() { echo -e "${CYAN}  --> $*${RESET}"; }
ok()   { echo -e "${GREEN}  [OK] $*${RESET}"; }
fail() { echo -e "${RED}  [FAIL] $*${RESET}"; }
warn() { echo -e "${YELLOW}  [WARN] $*${RESET}"; }
info() { echo -e "${GRAY}  $*${RESET}"; }
gray() { echo -e "${GRAY}    $*${RESET}"; }

echo ""
echo -e "${CYAN}==========================================${RESET}"
echo -e "${CYAN}   WorxForge Monorepo - First-Time Setup${RESET}"
echo -e "${CYAN}==========================================${RESET}"
echo ""

# =============================================================================
#  BACKEND
# =============================================================================
if [[ "$RUN_BACKEND" == "true" ]]; then
    SOLUTION_FILE="$SCRIPT_DIR/src/WorxForge.slnx"
    API_PROJECT="$SCRIPT_DIR/src/WorxForge.API"
    INFRA_PROJECT="$SCRIPT_DIR/src/WorxForge.Infrastructure"

    echo -e "${CYAN}------------------------------------------${RESET}"
    echo -e "${CYAN}  Backend setup${RESET}"
    echo -e "${CYAN}------------------------------------------${RESET}"
    echo ""

    # -- 1. Verify .NET SDK ----------------------------------------------------
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

    # -- 2. Restore local tools (includes dotnet-ef) ---------------------------
    step "Restoring local .NET tools..."
    dotnet tool restore
    ok "Local .NET tools restored."

    # -- 3. Restore NuGet packages ---------------------------------------------
    step "Restoring NuGet packages..."
    dotnet restore "$SOLUTION_FILE" --force-evaluate
    ok "NuGet restore succeeded."

    # -- 4. Build --------------------------------------------------------------
    step "Building solution..."
    dotnet build "$SOLUTION_FILE" --no-restore --configuration Debug
    ok "Build succeeded."

    # -- 5. User Secrets -------------------------------------------------------
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

        if [[ "$NON_INTERACTIVE" == "true" ]]; then
            if [[ "$optional" == "true" ]]; then
                warn "$key not set (optional)."
            else
                warn "$key not set - required."
            fi
            continue
        fi

        echo ""
        echo -e "  ${WHITE}$key${RESET}"
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

    # -- 6. Optional: apply EF Core migrations ---------------------------------
    echo ""
    RUN_MIGRATIONS=false

    if [[ "$NON_INTERACTIVE" != "true" ]]; then
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

    echo ""
    echo -e "${GREEN}  Backend setup complete.${RESET}"
    info "  Run the API:  cd src/WorxForge.API  ->  dotnet run"
    info "  Swagger:      https://localhost:7235/swagger  (or http://localhost:5222/swagger)"
    info "  Dev JWT:      POST /api/authentication/debug-login"
    echo ""
fi

# =============================================================================
#  FRONTEND
# =============================================================================
if [[ "$RUN_FRONTEND" == "true" ]]; then
    FRONTEND_DIR="$SCRIPT_DIR/worxforge-web"

    echo -e "${CYAN}------------------------------------------${RESET}"
    echo -e "${CYAN}  Frontend setup${RESET}"
    echo -e "${CYAN}------------------------------------------${RESET}"
    echo ""

    # -- Node.js ---------------------------------------------------------------
    step "Checking Node.js..."
    if ! command -v node &>/dev/null; then
        fail "Node.js not found. Install from https://nodejs.org"
        exit 1
    fi
    NODE_VERSION=$(node --version)
    ok "Node.js $NODE_VERSION detected."

    NVMRC="$SCRIPT_DIR/.nvmrc"
    if [[ -f "$NVMRC" ]]; then
        REQUIRED=$(cat "$NVMRC" | tr -d '[:space:]')
        if [[ -n "$REQUIRED" ]]; then
            CURRENT_MAJOR=$(echo "$NODE_VERSION" | sed 's/^v//' | cut -d. -f1)
            REQUIRED_MAJOR=$(echo "$REQUIRED"    | sed 's/^v//' | cut -d. -f1)
            if [[ "$CURRENT_MAJOR" != "$REQUIRED_MAJOR" ]]; then
                warn "Recommended Node.js is $REQUIRED (.nvmrc), current is $NODE_VERSION."
            else
                ok "Node.js major version matches .nvmrc ($REQUIRED)."
            fi
        fi
    fi

    # -- npm -------------------------------------------------------------------
    step "Checking npm..."
    if ! command -v npm &>/dev/null; then
        fail "npm not found. Reinstall Node.js from https://nodejs.org"
        exit 1
    fi
    NPM_VERSION=$(npm --version)
    ok "npm $NPM_VERSION detected."

    # -- Frontend directory ----------------------------------------------------
    if [[ ! -d "$FRONTEND_DIR" ]]; then
        fail "Frontend directory not found: $FRONTEND_DIR"
        exit 1
    fi
    if [[ ! -f "$FRONTEND_DIR/package.json" ]]; then
        fail "package.json not found in $FRONTEND_DIR"
        exit 1
    fi

    cd "$FRONTEND_DIR"

    # -- .env.local ------------------------------------------------------------
    ENV_EXAMPLE="$FRONTEND_DIR/.env.example"
    ENV_LOCAL="$FRONTEND_DIR/.env.local"

    if [[ -f "$ENV_LOCAL" ]]; then
        ok ".env.local already exists -- skipping env setup."
    elif [[ ! -f "$ENV_EXAMPLE" ]]; then
        warn ".env.example not found -- skipping env setup."
    elif [[ "$NON_INTERACTIVE" == "true" ]]; then
        step "Non-interactive mode: copying .env.example to .env.local..."
        cp "$ENV_EXAMPLE" "$ENV_LOCAL"
        ok "Created .env.local (fill in the required values before running npm run dev)."
    else
        step "Configuring environment variables..."
        echo ""
        gray "The following values will be written to .env.local."
        gray "Press Enter to accept the shown default, or type a new value."
        echo ""

        # Copy the template to start from
        cp "$ENV_EXAMPLE" "$ENV_LOCAL"

        # -- VITE_API_BASE_URL -------------------------------------------------
        echo -e "${WHITE}  VITE_API_BASE_URL${RESET}"
        gray "Backend API base URL used at runtime by the browser."
        gray "Leave empty (default) to use the Vite /api proxy in dev --"
        gray "the proxy target is set separately via VITE_API_PROXY_TARGET."
        gray "Example: https://localhost:7235  (or empty for local dev)"
        read -r -p "    Value [leave empty for proxy mode]: " API_BASE_URL

        # -- VITE_API_PROXY_TARGET ---------------------------------------------
        echo ""
        echo -e "${WHITE}  VITE_API_PROXY_TARGET${RESET}"
        gray "Vite dev proxy target -- the actual backend URL that /api"
        gray "requests are forwarded to when VITE_API_BASE_URL is empty."
        gray "Example: https://localhost:7235"
        read -r -p "    Value [https://localhost:7235]: " PROXY_TARGET
        PROXY_TARGET="${PROXY_TARGET:-https://localhost:7235}"

        # -- VITE_GOOGLE_CLIENT_ID ---------------------------------------------
        echo ""
        echo -e "${WHITE}  VITE_GOOGLE_CLIENT_ID${RESET}"
        gray "Google OAuth 2.0 Client ID from Google Cloud Console."
        gray "Required for the Google Sign-In button to render."
        gray "Example: 123456789-abc.apps.googleusercontent.com"
        read -r -p "    Value: " GOOGLE_CLIENT_ID

        # Write values into .env.local using sed in-place
        sed -i "s|^VITE_API_BASE_URL=.*|VITE_API_BASE_URL=${API_BASE_URL}|"         "$ENV_LOCAL"
        sed -i "s|^VITE_API_PROXY_TARGET=.*|VITE_API_PROXY_TARGET=${PROXY_TARGET}|" "$ENV_LOCAL"
        sed -i "s|^VITE_GOOGLE_CLIENT_ID=.*|VITE_GOOGLE_CLIENT_ID=${GOOGLE_CLIENT_ID}|" "$ENV_LOCAL"

        ok "Created .env.local."
        echo ""
    fi

    # -- Dependencies ----------------------------------------------------------
    step "Installing npm dependencies..."
    if [[ -f "$FRONTEND_DIR/package-lock.json" ]]; then
        npm ci
    else
        npm install
    fi
    ok "npm dependency install succeeded."

    # -- Build -----------------------------------------------------------------
    if [[ "$SKIP_BUILD" == "true" ]]; then
        ok "Skipping build verification by request."
    else
        step "Verifying frontend build..."
        npm run build
        ok "Frontend build succeeded."
    fi

    # -- Lint (interactive prompt) ---------------------------------------------
    if [[ "$NON_INTERACTIVE" != "true" ]]; then
        echo ""
        read -r -p "  Run linter now? [y/N]: " RUN_LINT
        if [[ "$RUN_LINT" =~ ^[Yy]$ ]]; then
            step "Running linter..."
            npm run lint
            ok "Lint succeeded."
        fi
    fi

    # -- Dev server ------------------------------------------------------------
    if [[ "$START_DEV_SERVER" == "true" ]]; then
        step "Starting Vite development server..."
        npm run dev
    fi

    echo ""
    echo -e "${GREEN}  Frontend setup complete.${RESET}"
    if [[ "$START_DEV_SERVER" != "true" ]]; then
        info "  Run the app:  cd worxforge-web  ->  npm run dev"
        info "  Then open:    http://localhost:5173"
    fi
    echo ""
fi

echo ""
echo -e "${GREEN}  WorxForge setup complete.${RESET}"
echo ""
