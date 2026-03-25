#!/bin/bash
#
# Local development environment setup for the Contoso University Modernization Workshop.
# Run on macOS/Linux to validate prerequisites.
#

set -e

GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
GRAY='\033[0;37m'
NC='\033[0m'

ALL_GOOD=true

header() {
    echo ""
    echo -e "${CYAN}============================================${NC}"
    echo -e "${CYAN}  $1${NC}"
    echo -e "${CYAN}============================================${NC}"
}

check() {
    local name="$1"
    local found="$2"
    local detail="$3"

    if [ "$found" = "true" ]; then
        echo -e "  ${GREEN}[OK]${NC} ${name} ${GRAY}(${detail})${NC}"
    else
        echo -e "  ${RED}[MISSING]${NC} ${name} ${YELLOW}- ${detail}${NC}"
        ALL_GOOD=false
    fi
}

header "Contoso University Workshop - Environment Check"

# --- .NET SDK ---
echo ""
echo "--- .NET Modern SDK ---"
if command -v dotnet &> /dev/null; then
    SDK_VERSION=$(dotnet --version 2>/dev/null || echo "unknown")
    HAS_MODERN=$(dotnet --list-sdks 2>/dev/null | grep -E "^(9|10)\." | head -1)
    if [ -n "$HAS_MODERN" ]; then
        check ".NET 9+ SDK" "true" "$HAS_MODERN"
    else
        check ".NET 9+ SDK" "false" "Install from https://dotnet.microsoft.com/download"
    fi
else
    check ".NET SDK" "false" "Install from https://dotnet.microsoft.com/download"
fi

# --- VS Code ---
echo ""
echo "--- IDE ---"
if command -v code &> /dev/null; then
    check "VS Code" "true" "$(code --version 2>/dev/null | head -1)"
else
    check "VS Code" "false" "Install from https://code.visualstudio.com/"
fi

# --- Git ---
echo ""
echo "--- Git ---"
if command -v git &> /dev/null; then
    check "Git" "true" "$(git --version)"
else
    check "Git" "false" "Install from https://git-scm.com/"
fi

# --- Docker (for Dev Container) ---
echo ""
echo "--- Docker ---"
if command -v docker &> /dev/null; then
    check "Docker" "true" "$(docker --version)"
else
    check "Docker" "false" "Install Docker Desktop from https://www.docker.com/products/docker-desktop/"
fi

# --- Node.js ---
echo ""
echo "--- Node.js ---"
if command -v node &> /dev/null; then
    check "Node.js" "true" "$(node --version)"
else
    check "Node.js" "false" "Install from https://nodejs.org/"
fi

if command -v npx &> /dev/null; then
    check "npx" "true" "available"
else
    check "npx" "false" "Comes with Node.js"
fi

# --- Summary ---
header "Summary"
if [ "$ALL_GOOD" = true ]; then
    echo ""
    echo -e "  ${GREEN}All prerequisites are installed! You're ready for the workshop.${NC}"
    echo ""
else
    echo ""
    echo -e "  ${YELLOW}Some prerequisites are missing. Please install them before the workshop.${NC}"
    echo ""
fi

echo ""
echo "Recommended: Use GitHub Codespaces for the fastest setup (zero install)."
echo "  See DEVELOPER_GUIDE.md for all onboarding options."
echo ""
