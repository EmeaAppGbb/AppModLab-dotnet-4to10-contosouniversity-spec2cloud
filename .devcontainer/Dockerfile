FROM mcr.microsoft.com/devcontainers/dotnet:1-9.0

# Install additional tools
RUN apt-get update && export DEBIAN_FRONTEND=noninteractive \
    && apt-get -y install --no-install-recommends \
    curl \
    git \
    wget \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*
