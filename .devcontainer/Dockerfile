FROM mcr.microsoft.com/devcontainers/dotnet:1-9.0

# Remove stale Yarn APT repository (expired GPG key causes feature installs to fail)
RUN rm -f /etc/apt/sources.list.d/yarn.list 2>/dev/null || true

# Install additional tools
RUN apt-get update && export DEBIAN_FRONTEND=noninteractive \
    && apt-get -y install --no-install-recommends \
    curl \
    git \
    wget \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*
