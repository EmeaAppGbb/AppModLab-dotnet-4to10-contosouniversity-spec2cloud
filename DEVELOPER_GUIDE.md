# Developer Onboarding Guide

> **Get up and running with the Contoso University Modernization Workshop in minutes.**

This guide covers **4 options** to set up your development environment. Choose the one that best fits your situation.

---

## Quick Comparison

| Option | Setup Time | Requirements | OS | Best For |
|--------|-----------|--------------|-----|----------|
| **1. GitHub Codespaces** | ~2 min | Browser + GitHub account | Any | Fastest start, workshops, no local install |
| **2. VS Code Dev Container** | ~5 min | Docker Desktop + VS Code | Any | Offline, same container as Codespaces |
| **3. Local Windows Setup** | ~15 min | Windows + .NET SDKs | Windows | Full legacy + modern experience |
| **4. VS Code (manual)** | ~10 min | VS Code + .NET SDKs | Any | Lightweight, no Docker needed |

---

## Option 1: GitHub Codespaces (Recommended)

**Zero install. Works in your browser.**

GitHub Codespaces provides a complete cloud-hosted development environment pre-configured with all the tools you need.

### Steps

1. Navigate to the repository on GitHub
2. Click the green **Code** button
3. Select the **Codespaces** tab
4. Click **Create codespace on main**

![Create Codespace](https://docs.github.com/assets/cb-138303/mw-1440/images/help/codespaces/new-codespace-button.webp)

That's it! The environment will automatically:
- Install .NET 9 SDK (and .NET 6 for tooling)
- Install GitHub CLI and Azure CLI
- Configure VS Code with all recommended extensions
- Forward ports for the web app (5000/5001) and SQL Server (1433)
- Display a welcome message with next steps

### What's Included

| Tool | Version | Purpose |
|------|---------|---------|
| .NET SDK | 9.0 | Target framework for modernization |
| GitHub CLI | Latest | Repository operations |
| Azure CLI | Latest | Azure deployment |
| PowerShell | Latest | Cross-platform scripting |
| GitHub Copilot | Extension | AI-assisted modernization |
| C# Dev Kit | Extension | Rich C# editing |

### Important Note

> The original .NET Framework 4.8 application **cannot build** inside Codespaces (Linux-based container). This is expected! You will modernize it to .NET 9 during the workshop, at which point it will build and run inside the Codespace.

### Performance Tips

- Choose a **4-core** machine type (sufficient for this workshop)
- If working with large files, consider **8-core** for faster IntelliSense
- Codespaces auto-stop after 30 minutes of inactivity

---

## Option 2: VS Code Dev Container (Local Docker)

**Same environment as Codespaces, running locally.**

This uses the same `.devcontainer` configuration but runs on your local Docker installation.

### Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) installed and running
- [VS Code](https://code.visualstudio.com/) with the [Dev Containers extension](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers)

### Steps

1. Clone the repository:
   ```bash
   git clone https://github.com/EmeaAppGbb/AppModLab-dotnet-4to9-contosouniversity-spec2cloud.git
   cd AppModLab-dotnet-4to9-contosouniversity-spec2cloud
   ```

2. Open in VS Code:
   ```bash
   code .
   ```

3. When prompted **"Reopen in Container"**, click **Yes**
   - Or use the Command Palette: `Dev Containers: Reopen in Container`

4. Wait for the container to build (~3-5 minutes on first run)

### With SQL Server (Docker Compose)

For a full environment with a local SQL Server instance:

1. Open the Command Palette (`Ctrl+Shift+P`)
2. Select `Dev Containers: Reopen in Container`
3. If prompted, choose the **docker-compose** configuration

This starts:
- **App container** — .NET 9 SDK, all tools pre-installed
- **SQL Server 2022** — Linux edition on port 1433 (password: `Workshop@2024!`)

### Troubleshooting

| Issue | Solution |
|-------|----------|
| Docker not running | Start Docker Desktop before opening VS Code |
| Port conflict on 1433 | Stop any local SQL Server: `Stop-Service MSSQLSERVER` |
| Slow build | Ensure Docker has at least 4 GB RAM allocated |

---

## Option 3: Local Windows Setup

**Full experience with both legacy (.NET Fx 4.8) and modern (.NET 9) support.**

This is the only option where you can build and run the **original legacy application** before modernizing it.

### Prerequisites

| Requirement | Download |
|-------------|----------|
| Windows 10/11 | — |
| .NET Framework 4.8 SDK | [Download](https://dotnet.microsoft.com/download/dotnet-framework/net48) |
| .NET 9 (or 10) SDK | [Download](https://dotnet.microsoft.com/download) |
| Visual Studio 2022 **or** VS Code | [VS](https://visualstudio.microsoft.com/) / [VS Code](https://code.visualstudio.com/) |
| SQL Server LocalDB | Included with Visual Studio, or [standalone](https://learn.microsoft.com/sql/database-engine/configure-windows/sql-server-express-localdb) |
| Node.js (LTS) | [Download](https://nodejs.org/) |
| Git | [Download](https://git-scm.com/) |
| GitHub Copilot | [VS Code Extension](https://marketplace.visualstudio.com/items?itemName=GitHub.copilot) |

### Automated Setup

Run the setup script to check all prerequisites and install VS Code extensions:

```powershell
# Clone the repository
git clone https://github.com/EmeaAppGbb/AppModLab-dotnet-4to9-contosouniversity-spec2cloud.git
cd AppModLab-dotnet-4to9-contosouniversity-spec2cloud

# Run the setup check (check only, no changes)
.\setup.ps1 -CheckOnly

# Run full setup (installs VS Code extensions)
.\setup.ps1
```

**Script options:**

| Flag | Description |
|------|-------------|
| `-CheckOnly` | Only verify prerequisites, don't install anything |
| `-SkipMSMQ` | Skip MSMQ check (MSMQ only needed for legacy notification feature) |

### macOS/Linux Users

A bash setup script is also available:

```bash
chmod +x setup.sh
./setup.sh
```

> Note: On macOS/Linux you cannot build the original .NET Framework 4.8 app. Use this for prerequisite checking only, then follow the Codespaces or Dev Container path.

### Enable MSMQ (Optional — Windows Only)

The legacy app uses MSMQ for notifications. To run the original app fully:

```powershell
# Run as Administrator
Enable-WindowsOptionalFeature -Online -FeatureName MSMQ-Server
```

### Build and Run the Legacy App

Using Visual Studio:
1. Open `src/ContosoUniversity/ContosoUniversity.sln`
2. Restore NuGet packages (automatic on build)
3. Press `F5` to build and run
4. The app opens at `https://localhost:44300/` (IIS Express)

Using MSBuild:
```powershell
cd src/ContosoUniversity
nuget restore ContosoUniversity.sln
msbuild ContosoUniversity.sln /p:Configuration=Release
```

---

## Option 4: VS Code with Manual Setup

**Lightweight setup — just VS Code with recommended settings, no Docker required.**

### Steps

1. Install the prerequisites from Option 3's table (at minimum: .NET 9 SDK, VS Code, Git, Node.js)

2. Clone and open:
   ```bash
   git clone https://github.com/EmeaAppGbb/AppModLab-dotnet-4to9-contosouniversity-spec2cloud.git
   cd AppModLab-dotnet-4to9-contosouniversity-spec2cloud
   code .
   ```

3. **Install recommended extensions** — VS Code will prompt you automatically via the `.vscode/extensions.json` file. Click **Install All**.

4. The workspace is pre-configured with:
   - **Build/Run/Clean tasks** (`Ctrl+Shift+B` to build)
   - **Debug configuration** (F5 to launch after modernization)
   - **Editor settings** (format on save, bracket colorization, file exclusions)
   - **Solution file** auto-detected

### Pre-configured VS Code Features

| Feature | File | Description |
|---------|------|-------------|
| Recommended extensions | `.vscode/extensions.json` | Auto-prompts to install 11 essential extensions |
| Workspace settings | `.vscode/settings.json` | Format on save, hide bin/obj folders, solution path |
| Build tasks | `.vscode/tasks.json` | Build, run, restore, clean commands |
| Debug launch | `.vscode/launch.json` | F5 debug config (works after modernization to .NET 9) |

---

## After Setup — Start the Workshop

Regardless of which option you chose:

1. **Read the main [README.MD](README.MD)** for the full workshop walkthrough
2. **Initialize Spec2Cloud** with `npx spec2cloud init`
3. **Follow the 9 modernization steps** in the README

---

## Troubleshooting

### General

| Issue | Solution |
|-------|----------|
| `dotnet` not found | Restart your terminal after installing .NET SDK |
| Extensions not loading | Reload VS Code window (`Ctrl+Shift+P` → `Reload Window`) |
| Git clone fails | Check your GitHub access — you may need to configure SSH keys or a PAT |

### Codespaces

| Issue | Solution |
|-------|----------|
| Codespace won't start | Check [GitHub Status](https://www.githubstatus.com/) for outages |
| Extensions missing | Run `Dev Containers: Rebuild Container` from the Command Palette |
| Slow performance | Switch to a larger machine type (Settings → Change machine type) |

### Database

| Issue | Solution |
|-------|----------|
| LocalDB not found | Install SQL Server LocalDB or use the Docker Compose option |
| Connection refused | Ensure SQL Server service is running: `SqlLocalDB start MSSQLLocalDB` |
| Database doesn't exist | The app creates it automatically on first run (EF Core `EnsureCreated`) |

---

## Repository Structure (Developer View)

```
AppModLab-dotnet-4to9-contosouniversity-spec2cloud/
├── .devcontainer/               # Dev Container / Codespaces configuration
│   ├── devcontainer.json        #   Container definition & VS Code settings
│   ├── Dockerfile               #   Container image build instructions
│   ├── docker-compose.yml       #   Multi-container setup (app + SQL Server)
│   ├── post-create.sh           #   Runs once after container creation
│   └── post-start.sh            #   Runs on every container start
├── .vscode/                     # VS Code workspace configuration
│   ├── extensions.json          #   Recommended extensions
│   ├── launch.json              #   Debug configuration
│   ├── settings.json            #   Editor settings
│   └── tasks.json               #   Build/run tasks
├── img/                         # Workshop screenshots
├── src/
│   └── ContosoUniversity/       # The legacy .NET 4.8 application
│       ├── Controllers/         #   MVC Controllers
│       ├── Data/                #   EF Core DbContext & seed data
│       ├── Models/              #   Data models & view models
│       ├── Views/               #   Razor views
│       ├── Services/            #   Business logic (notifications)
│       ├── Web.config           #   Legacy configuration
│       └── packages.config      #   Legacy NuGet packages
├── setup.ps1                    # Windows setup script (PowerShell)
├── setup.sh                     # macOS/Linux setup script (Bash)
├── DEVELOPER_GUIDE.md           # This file
└── README.MD                    # Workshop walkthrough
```
