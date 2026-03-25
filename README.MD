```
 ██████╗ ██████╗ ███╗   ██╗████████╗ ██████╗ ███████╗ ██████╗
██╔════╝██╔═══██╗████╗  ██║╚══██╔══╝██╔═══██╗██╔════╝██╔═══██╗
██║     ██║   ██║██╔██╗ ██║   ██║   ██║   ██║███████╗██║   ██║
██║     ██║   ██║██║╚██╗██║   ██║   ██║   ██║╚════██║██║   ██║
╚██████╗╚██████╔╝██║ ╚████║   ██║   ╚██████╔╝███████║╚██████╔╝
 ╚═════╝ ╚═════╝ ╚═╝  ╚═══╝   ╚═╝    ╚═════╝ ╚══════╝ ╚═════╝
██╗   ██╗███╗   ██╗██╗██╗   ██╗███████╗██████╗ ███████╗██╗████████╗██╗   ██╗
██║   ██║████╗  ██║██║██║   ██║██╔════╝██╔══██╗██╔════╝██║╚══██╔══╝╚██╗ ██╔╝
██║   ██║██╔██╗ ██║██║██║   ██║█████╗  ██████╔╝███████╗██║   ██║    ╚████╔╝
██║   ██║██║╚██╗██║██║╚██╗ ██╔╝██╔══╝  ██╔══██╗╚════██║██║   ██║     ╚██╔╝
╚██████╔╝██║ ╚████║██║ ╚████╔╝ ███████╗██║  ██║███████║██║   ██║      ██║
 ╚═════╝ ╚═╝  ╚═══╝╚═╝  ╚═══╝  ╚══════╝╚═╝  ╚═╝╚══════╝╚═╝   ╚═╝      ╚═╝
███████╗██████╗ ███████╗ ██████╗██████╗  ██████╗██╗      ██████╗ ██╗   ██╗██████╗
██╔════╝██╔══██╗██╔════╝██╔════╝╚════██╗██╔════╝██║     ██╔═══██╗██║   ██║██╔══██╗
███████╗██████╔╝█████╗  ██║      █████╔╝██║     ██║     ██║   ██║██║   ██║██║  ██║
╚════██║██╔═══╝ ██╔══╝  ██║     ██╔═══╝ ██║     ██║     ██║   ██║██║   ██║██║  ██║
███████║██║     ███████╗╚██████╗███████╗╚██████╗███████╗╚██████╔╝╚██████╔╝██████╔╝
╚══════╝╚═╝     ╚══════╝ ╚═════╝╚══════╝ ╚═════╝╚══════╝ ╚═════╝  ╚═════╝ ╚═════╝
```

<div align="center">

### 🚀 **.NET 4.8 → .NET 10 Modernization with Spec2Cloud** 🚀

*Learn how to modernize legacy .NET applications using **Spec-Driven Development** and the **Spec2Cloud** framework, powered by **GitHub Copilot** 🤖*

---

[![.NET](https://img.shields.io/badge/.NET_4.8_→_.NET_10-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![GitHub Copilot](https://img.shields.io/badge/GitHub_Copilot-000000?style=for-the-badge&logo=github&logoColor=white)](https://github.com/features/copilot)
[![Azure](https://img.shields.io/badge/Azure_App_Service-0078D4?style=for-the-badge&logo=microsoftazure&logoColor=white)](https://azure.microsoft.com/)
[![Spec2Cloud](https://img.shields.io/badge/Spec2Cloud-FF6F00?style=for-the-badge&logo=rocket&logoColor=white)](https://github.com/EmeaAppGbb/spec2cloud)

</div>

---

## 📖 Table of Contents

- [🚀 Developer Onboarding](#-developer-onboarding)
- [🎯 About This Lab](#-about-this-lab)
- [💡 What is Spec2Cloud?](#-what-is-spec2cloud)
- [💡 What is Spec-Driven Development?](#-what-is-spec-driven-development)
- [🏫 The Contoso University App](#-the-contoso-university-app)
- [📂 Repository Structure](#-repository-structure)
- [✅ Prerequisites](#-prerequisites)
- [🛠️ Modernization Steps](#️-modernization-steps)
  - [Step 1 — Initialize Spec2Cloud](#-step-1--initialize-spec2cloud)
  - [Step 2 — Launch GitHub Copilot](#-step-2--launch-github-copilot)
  - [Step 3 — Run Modernization Assessment](#-step-3--run-modernization-assessment)
  - [Step 4 — Review the Assessment](#-step-4--review-the-assessment)
  - [Step 5 — Generate the Modernization Plan](#-step-5--generate-the-modernization-plan)
  - [Step 6 — Review & Approve the Plan](#-step-6--review--approve-the-plan)
  - [Step 7 — Execute the Modernization](#-step-7--execute-the-modernization)
  - [Step 8 — Handle Authentication Decisions](#-step-8--handle-authentication-decisions)
  - [Step 9 — Celebrate! 🎉](#-step-9--celebrate-)
- [🌿 Branches & Tags](#-branches--tags)
- [📚 Additional Resources](#-additional-resources)

---

## 🚀 Developer Onboarding

**4 ways to set up your environment** — pick the one that works for you:

| Option | Time | What You Need |
|--------|------|---------------|
| 🌐 [**GitHub Codespaces**](DEVELOPER_GUIDE.md#option-1-github-codespaces-recommended) | ~2 min | Just a browser |
| 🐳 [**VS Code Dev Container**](DEVELOPER_GUIDE.md#option-2-vs-code-dev-container-local-docker) | ~5 min | Docker + VS Code |
| 🖥️ [**Local Windows Setup**](DEVELOPER_GUIDE.md#option-3-local-windows-setup) | ~15 min | Windows + .NET SDKs |
| ⚡ [**VS Code Manual**](DEVELOPER_GUIDE.md#option-4-vs-code-with-manual-setup) | ~10 min | VS Code + .NET SDKs |

> 📖 **Full details:** [**DEVELOPER_GUIDE.md**](DEVELOPER_GUIDE.md)

---

## 🎯 About This Lab

This is a **hands-on lab repository** designed to teach you how to modernize a legacy **.NET Framework 4.8** application into a modern **.NET 10** application using:

| Concept | Description |
|---------|-------------|
| 📝 **Spec-Driven Development** | A methodology where specifications drive the entire modernization lifecycle |
| ☁️ **Spec2Cloud Framework** | An AI-powered framework that translates specs into cloud-ready code |
| 🤖 **GitHub Copilot** | Your AI pair-programmer that orchestrates the modernization process |
| ☁️ **Azure App Service** | The target cloud platform for the modernized application |

> 💡 **Who is this for?** Developers, architects, and anyone interested in learning a structured, AI-assisted approach to application modernization.

---

## 💡 What is Spec2Cloud?

> 📖 **Full documentation:** [Spec2Cloud on GitHub](https://github.com/EmeaAppGbb/spec2cloud) | [Brownfield Guide](https://github.com/EmeaAppGbb/spec2cloud/blob/vNext/docs/brownfield.md)

**Spec2Cloud** is a framework that bridges the gap between **application specifications** and **cloud-ready implementations**. It uses AI-powered agents and GitHub Copilot skills to:

1. 🔍 **Assess** — Analyze your legacy codebase and generate comprehensive assessment documents
2. 📋 **Plan** — Create a detailed, step-by-step modernization plan based on the assessment
3. ⚙️ **Execute** — Automatically implement the modernization changes following the plan
4. ✅ **Validate** — Ensure the modernized application meets the original specifications

```
┌──────────────┐     ┌──────────────┐     ┌──────────────┐     ┌──────────────┐
│  📝 ASSESS   │ ──▶ │  📋 PLAN     │ ──▶ │  ⚙️ EXECUTE  │ ──▶ │  ✅ VALIDATE │
│              │     │              │     │              │     │              │
│ Analyze code │     │ Generate     │     │ Implement    │     │ Test & verify│
│ Generate FRD │     │ modernization│     │ changes      │     │ specs met    │
│ Generate PRD │     │ roadmap      │     │ iteratively  │     │              │
└──────────────┘     └──────────────┘     └──────────────┘     └──────────────┘
```

---

## 💡 What is Spec-Driven Development?

**Spec-Driven Development (SDD)** is a methodology where **specifications are the single source of truth** throughout the software lifecycle. Unlike traditional approaches where specs become outdated quickly, SDD ensures:

- 📄 **Specs are living documents** — They evolve with the code and are always up to date
- 🔗 **Traceability** — Every code change maps back to a specification requirement
- 🤖 **AI-Assisted** — GitHub Copilot uses specs as context to generate accurate, aligned code
- 🔄 **Iterative** — Specs and code evolve together through continuous feedback loops

---

## 🏫 The Contoso University App

The **Contoso University** application is a classic sample ASP.NET MVC 5 web application used for educational purposes. It's the perfect candidate for modernization! 🎓

![Contoso University Legacy App](img/appscreenshot-legacy-001.png)

### 🔧 Current Tech Stack (Legacy)

| Component | Technology |
|-----------|-----------|
| 🖥️ Framework | ASP.NET MVC 5 (.NET Framework 4.8) |
| 🗄️ ORM | Entity Framework Core 3.1.32 |
| 🗃️ Database | SQL Server LocalDB |
| 🌐 Server | IIS Express |
| 🔐 Auth | Windows Authentication |

### ✨ Application Features

- 👩‍🎓 **Student Management** — CRUD operations with pagination and search
- 📚 **Course Management** — Manage courses and department assignments
- 👨‍🏫 **Instructor Management** — Handle instructor assignments and office locations
- 🏢 **Department Management** — Manage departments and their administrators
- 📊 **Enrollment Statistics** — View enrollment stats by date

### 📁 Project Structure

```
ContosoUniversity/
├── 📂 App_Start/        # 🔧 Application startup configuration
├── 📂 Controllers/      # 🎮 MVC Controllers
├── 📂 Data/             # 🗄️ Entity Framework context & initializer
├── 📂 Models/           # 📦 Data models and view models
├── 📂 Views/            # 👁️ Razor views
├── 📂 Content/          # 🎨 CSS and static content
├── 📂 Scripts/          # 📜 JavaScript files
├── 📂 Services/         # ⚙️ Business logic services
├── 📄 Global.asax       # 🌍 Application global events
├── 📄 Web.config        # ⚙️ Configuration file
└── 📄 packages.config   # 📦 NuGet packages
```

---

## 📂 Repository Structure

```
📦 AppModLab-dotnet-4to9-contosouniversity-spec2cloud
├── 📄 README.MD              # 👈 You are here!
├── � DEVELOPER_GUIDE.md     # 🚀 Developer onboarding (4 setup options)
├── 📄 setup.ps1              # 🖥️ Windows setup script
├── 📄 setup.sh               # 🐧 macOS/Linux setup script
├── 📂 .devcontainer/         # 🐳 Codespaces & Dev Container config
├── 📂 .vscode/               # ⚙️ VS Code workspace settings
├── �📂 img/                   # 🖼️ Screenshots and diagrams for this guide
└── 📂 src/
    └── 📂 ContosoUniversity/ # 🏫 The legacy application source code
```

### 🌿 Branches & Tags

| Branch | Description |
|--------|-------------|
| 🟢 `main` | The **starting point** — contains the legacy .NET 4.8 codebase |
| 🏁 `final-solution` | The **finished result** — contains the fully modernized .NET 10 application |

> 💡 **Pro Tip:** Each modernization step is **tagged** in the `final-solution` branch! You can compare your progress at any point, or jump ahead to study the approach and results without going through the entire process. 🔖

---

## ✅ Prerequisites

Before starting this lab, make sure you have the following installed:

| Requirement | Purpose |
|-------------|---------|
| 🖥️ **Visual Studio 2019+** or **VS Code** | IDE for development |
| 🟣 **.NET Framework 4.8 SDK** | To build the legacy app |
| 🟣 **.NET 10 SDK** | Target framework for modernization |
| 🗃️ **SQL Server LocalDB** | Database engine |
| 📦 **Node.js & npm** | Required to run `npx spec2cloud` |
| 🤖 **GitHub Copilot** | AI assistant (requires active subscription) |
| ☁️ **Azure Subscription** *(optional)* | For deploying to Azure App Service |

> 📖 **Quick Start:** See the [**Developer Onboarding Guide**](DEVELOPER_GUIDE.md) for 4 ways to set up your environment — including **GitHub Codespaces** (zero install, ~2 min) and **local Docker** options.

---

## 🛠️ Modernization Steps

Follow these steps to modernize the Contoso University application from .NET 4.8 to .NET 10 using Spec2Cloud! 🚀

---

### 📌 Step 1 — Initialize Spec2Cloud

Open a terminal in the repository root and run:

```bash
npx spec2cloud init
```

![Spec2Cloud Init](img/shell-install.png)

You will be prompted to select the setup scope. Choose **🏗️ Brownfield** since we are modernizing an existing application.

> 📖 **Need more details?** Check out the full [Spec2Cloud Brownfield Documentation](https://github.com/EmeaAppGbb/spec2cloud/blob/vNext/docs/brownfield.md) for an in-depth guide on brownfield modernization scenarios.

> ℹ️ **What happens here?** Spec2Cloud scaffolds the necessary configuration files and Copilot skills into your repository, preparing it for the spec-driven modernization workflow.

---

### 📌 Step 2 — Launch GitHub Copilot

Start GitHub Copilot in your terminal:

```bash
copilot
```

> 💡 **Optional:** Use the `--yolo` flag for a smoother lab experience — this reduces the number of confirmation prompts so you can focus on the process:
>
> ```bash
> copilot --yolo
> ```

---

### 📌 Step 3 — Run Modernization Assessment

Use the **Modernization-Assessment** Spec2Cloud skill in Copilot:

```
/modernization-assessment
```

![Modernization Assessment](img/copilot-step1-mod-assess.png)

Then, ask Copilot to **analyze the codebase** and generate the application architecture, Functional Requirements Documents (FRDs), and Product Requirements Documents (PRDs):

```
Analyse the codebase and generate specs
```

![Code Analysis](img/copilot-step1-analize.png)

🎁 **What you get:** A rich set of documentation to help you deeply understand the application and its modernization needs! Copilot can also generate diagrams and architecture documentation based on the code analysis.

![Analysis Results](img/copilot-step1-analize-results.png)

---

### 📌 Step 4 — Review the Assessment

📖 **Read the output assessment documents** carefully and understand the modernization approach proposed for this application.

Key things to look for:
- 🔍 Identified dependencies and breaking changes
- 📋 Proposed target framework and platform
- ⚠️ Risk areas and mitigation strategies
- 📊 Effort estimation

> 🔧 **Customization:** You can tell Copilot to adjust the assessment based on your specific intent and requirements!

For this lab, we will proceed with the proposed modernization to **.NET 10** and **Azure App Service** ☁️

---

### 📌 Step 5 — Generate the Modernization Plan

Ask Copilot to generate a detailed modernization plan using the **Modernization Planning** Spec2Cloud skill:

```
/modernization-planner
```

![Modernization Planning](img/copilot-step1-mod-assess_plan.png)

> 📋 **What you get:** A comprehensive, step-by-step implementation plan that breaks down the modernization into manageable, ordered tasks.

---

### 📌 Step 6 — Review & Approve the Plan

📖 **Read the modernization plan** thoroughly and understand each step that will be taken to modernize the application.

- ✏️ You can ask Copilot to **modify the plan** based on your needs
- ✅ You will need to **give your consent** before Copilot begins executing the plan
- 🔄 The plan is iterative — you maintain full control at every stage

---

### 📌 Step 7 — Execute the Modernization

Once approved, the modernization process begins! 🏗️

Copilot will **iterate through each step** of the modernization plan and execute the changes automatically. You can:

- 👀 **Monitor progress** in the terminal output
- 📄 **Review generated documentation** as changes are made
- 🛑 **Pause or intervene** at any point if needed

![Modernization In Progress](img/copilot-step2-modernize-continue.png)

---

### 📌 Step 8 — Handle Authentication Decisions

🔐 This application uses **Windows Authentication**, which requires special attention during modernization.

Copilot will present you with **multiple authentication modernization approaches**. You'll need to select the best path based on:

- 🏢 Your organizational requirements
- ☁️ Your cloud identity strategy
- 🔒 Security compliance needs

![Authentication Decision](img/copilot-step2-modernize-auth.png)

> 🛡️ **This is a key decision point!** Take your time to evaluate the options and choose the approach that best fits your scenario.

---

### 📌 Step 9 — Celebrate! 🎉


![Modernization Complete](img/copilot-step2-modernize-finished.png)

🥳 **Congratulations!** Once the modernization process is completed, you will have:

| Outcome | Details |
|---------|---------|
| ✅ **Modernized Application** | Running on .NET 10 with modern patterns |
| ☁️ **Cloud-Ready** | Configured for Azure App Service deployment |
| 📚 **Full Documentation** | Detailed record of every change and decision |
| 🔍 **Traceability** | Every change mapped back to original specifications |

```
  🎓 Before                          🎓 After
┌─────────────────────┐         ┌─────────────────────┐
│  .NET Framework 4.8 │         │      .NET 10        │
│  ASP.NET MVC 5      │  ════▶  │  ASP.NET Core MVC   │
│  EF Core 3.1        │         │  EF Core 10         │
│  IIS Express         │         │  Azure App Service  │
│  Windows Auth       │         │  Modern Auth        │
└─────────────────────┘         └─────────────────────┘
```

---

## 📚 Additional Resources

| Resource | Link |
|----------|------|
| 📘 Spec2Cloud Repository | [github.com/EmeaAppGbb/spec2cloud](https://github.com/EmeaAppGbb/spec2cloud) |
| 📗 Spec2Cloud Brownfield Guide | [Brownfield Modernization Docs](https://github.com/EmeaAppGbb/spec2cloud/blob/vNext/docs/brownfield.md) |
| 🤖 GitHub Copilot Docs | [docs.github.com/copilot](https://docs.github.com/copilot) |
| 🟣 .NET Upgrade Guide | [learn.microsoft.com](https://learn.microsoft.com/en-us/dotnet/core/porting/) |
| ☁️ Azure App Service Docs | [learn.microsoft.com](https://learn.microsoft.com/en-us/azure/app-service/) |
| 🏫 Contoso University Tutorial | [learn.microsoft.com](https://learn.microsoft.com/en-us/aspnet/mvc/overview/getting-started/getting-started-with-ef-using-mvc/) |

---

<div align="center">

**💜 Built with love using Spec2Cloud & GitHub Copilot 🤖**

*If you found this lab useful, give it a ⭐ and share it with your team!*

</div>


