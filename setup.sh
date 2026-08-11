#!/usr/bin/env bash
# Restores and builds the WorxForge backend solution.
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SOLUTION_FILE="$SCRIPT_DIR/src/WorxForge.slnx"

GREEN='\033[0;32m'
CYAN='\033[0;36m'
RED='\033[0;31m'
RESET='\033[0m'

step()  { echo -e "${CYAN}  --> $*${RESET}"; }
ok()    { echo -e "${GREEN}  [OK] $*${RESET}"; }
fail()  { echo -e "${RED}  [FAIL] $*${RESET}"; }

echo ""
echo -e "${CYAN}==============================${RESET}"
echo -e "${CYAN}  WorxForge Backend Setup     ${RESET}"
echo -e "${CYAN}==============================${RESET}"
echo ""

# Verify .NET SDK
step "Checking .NET SDK..."
if ! command -v dotnet &>/dev/null; then
    fail ".NET SDK not found. Install from https://dotnet.microsoft.com/download"
    exit 1
fi
DOTNET_VERSION=$(dotnet --version)
ok ".NET SDK $DOTNET_VERSION detected."

# Verify solution file
if [[ ! -f "$SOLUTION_FILE" ]]; then
    fail "Solution file not found: $SOLUTION_FILE"
    exit 1
fi

# Restore local tools
step "Restoring local .NET tools..."
dotnet tool restore
if [[ $? -ne 0 ]]; then
    fail "Local tool restore failed."
    exit 1
fi
ok "Local .NET tools restored."

# Restore
step "Restoring NuGet packages and evaluating package versions..."
dotnet restore "$SOLUTION_FILE"
ok "NuGet restore succeeded."

# Build
step "Building backend solution..."
dotnet build "$SOLUTION_FILE" --no-restore --configuration Debug
ok "Backend build succeeded."

# Check user-secrets
echo ""
step "Checking required user-secrets..."

API_PROJECT="$SCRIPT_DIR/src/WorxForge.API"
REQUIRED_SECRETS=(
    "JwtAccessToken:Issuer"
    "JwtAccessToken:Audience"
    "JwtAccessToken:SigningKey"
    "JwtAccessToken:ExpirationMinutes"
    "ConnectionStrings:Default"
    "Authentication:Google:ClientId"
    "Cors:AllowedOrigins:0"
)

SECRETS_LIST=$(dotnet user-secrets list --project "$API_PROJECT" 2>/dev/null || true)
MISSING_SECRETS=()

for secret in "${REQUIRED_SECRETS[@]}"; do
    if ! grep -qF "$secret" <<< "$SECRETS_LIST"; then
        MISSING_SECRETS+=("$secret")
    fi
done

if [[ ${#MISSING_SECRETS[@]} -gt 0 ]]; then
    echo ""
    echo -e "\033[0;33m  [WARN] The following user-secrets are not set:\033[0m"
    for s in "${MISSING_SECRETS[@]}"; do
        echo -e "\033[0;33m         dotnet user-secrets set \"$s\" \"<value>\" --project src/WorxForge.API\033[0m"
    done
    echo ""
    echo -e "\033[0;33m  See README.md \"Local development configuration\" for required values.\033[0m"
else
    ok "All required user-secrets are present."
fi

echo ""
echo -e "${GREEN}  Backend setup complete.${RESET}"
echo ""
