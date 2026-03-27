#!/bin/bash
set -e

echo "=========================================="
echo " Contoso University Workshop Setup"
echo "=========================================="

# Install SQL Server tools (sqlcmd)
echo "Installing SQL Server command-line tools..."
curl -sSL https://packages.microsoft.com/keys/microsoft.asc | sudo tee /etc/apt/trusted.gpg.d/microsoft.asc > /dev/null
sudo add-apt-repository "$(wget -qO- https://packages.microsoft.com/config/ubuntu/22.04/prod.list)" 2>/dev/null || true
sudo apt-get update -qq
sudo ACCEPT_EULA=Y apt-get install -y -qq mssql-tools18 unixodbc-dev 2>/dev/null || echo "Note: sqlcmd install may require manual setup"

# Add sqlcmd to PATH
echo 'export PATH="$PATH:/opt/mssql-tools18/bin"' >> ~/.bashrc

# Install GitHub Copilot CLI extension
echo "Installing GitHub Copilot CLI extension..."
gh extension install github/gh-copilot 2>/dev/null || echo "Note: gh-copilot extension may already be installed"

# Trust the dev certificate
dotnet dev-certs https --trust 2>/dev/null || true

echo ""
echo "=========================================="
echo " Setup Complete!"
echo "=========================================="
echo ""
echo "This is a .NET Framework 4.8 modernization workshop."
echo "The original app cannot build in this Linux container."
echo "You will modernize it to .NET 9 during the workshop."
echo ""
echo "Quick start:"
echo "  1. Explore the legacy code in src/ContosoUniversity/"
echo "  2. Follow the workshop guide in README.MD"
echo "  3. Use GitHub Copilot to assist with modernization"
echo ""
