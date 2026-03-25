# ADR-001: Migrate from .NET Framework 4.8 to .NET 10

## Status

Proposed

## Context

Contoso University runs on .NET Framework 4.8 with ASP.NET MVC 5.2.9 and EF Core 3.1.32. This configuration presents several critical problems identified in the modernization assessment:

- **EF Core 3.1 is EOL** (end-of-life December 2022) — no security patches available
- **Hybrid framework mismatch** — EF Core 3.1 on .NET Framework 4.8 is unsupported, requiring 40+ binding redirects and .NET Standard shims
- **.NET Framework 4.8 is in maintenance mode** — no new features, no performance improvements, Windows-only
- **No dependency injection** — `Global.asax` lifecycle and static factories prevent modern patterns
- **No async/await** — all I/O is synchronous, limiting scalability
- **No containerization** — IIS dependency prevents cloud-native deployment

The current LTS version of .NET is **.NET 10** (released November 2025, supported until November 2028).

## Decision Drivers

- Application is small (~30 source files, 6 controllers, 8 models)
- No existing tests to preserve
- No external consumers of the API (internal MVC app)
- Team wants cloud-native deployment capability

## Considered Options

### Option A: Full migration to .NET 10 (Recommended)

Migrate the entire application in one pass: `.csproj` → SDK-style, `Global.asax` → `Program.cs`, `Web.config` → `appsettings.json`, MVC 5 → ASP.NET Core MVC, EF Core 3.1 → EF Core 9.

**Pros:**
- Clean break — no hybrid state to maintain
- Application is small enough for single-pass migration
- Immediately unlocks all modern features (DI, async, middleware pipeline)
- .NET Upgrade Assistant can automate significant portions

**Cons:**
- All-or-nothing — application is unavailable until migration is complete
- Requires familiarity with ASP.NET Core differences (middleware, configuration, routing)

### Option B: Strangler fig (incremental migration)

Run .NET Framework and .NET 10 side-by-side using YARP or similar reverse proxy. Migrate one controller at a time.

**Pros:**
- Lower risk — old system remains available
- Can validate each controller independently

**Cons:**
- Significant infrastructure overhead for a small app
- Shared database requires compatible EF configurations
- Longer calendar time to complete

### Option C: Side-by-side rewrite

Create a new .NET 10 project and rewrite from scratch, using existing code as reference.

**Pros:**
- Cleanest architecture — no legacy constraints
- Can redesign data model, apply CQRS, etc.

**Cons:**
- Highest effort — rewriting working code
- Risk of introducing new bugs
- No code reuse

## Decision

**Option A: Full migration to .NET 10** — The application is small (~30 files), has no tests to preserve, no external API consumers, and the entire codebase can be migrated in a focused sprint. The .NET Upgrade Assistant provides automated assistance for the mechanical conversion steps.

## Consequences

### Positive
- Access to .NET 10 LTS with support until November 2028
- Built-in DI, async, middleware, structured logging
- EF Core 9 with modern query features, compiled queries, bulk operations
- Cross-platform deployment (Linux containers, Azure Container Apps)
- SDK-style project with simplified configuration
- Eliminates all 40+ binding redirects

### Negative
- Requires developer time (estimated: 1–2 weeks for core migration)
- Some API differences between MVC 5 and ASP.NET Core MVC (e.g., `TryUpdateModel` → model binding, `Server.MapPath` → `IWebHostEnvironment`)
- `System.Web` dependencies must be replaced (no equivalent in .NET Core)

### Risks
- MSMQ (`System.Messaging`) has no .NET Core equivalent — must be replaced (see ADR-003)
- Windows Authentication configuration differs in ASP.NET Core
- Bundle/minification approach changes (no `System.Web.Optimization`)

## References

- Assessment findings: A-1, D-1, D-2, A-2, A-4, CI-3
- [.NET Upgrade Assistant](https://learn.microsoft.com/en-us/dotnet/core/porting/upgrade-assistant-overview)
- [Migrate from ASP.NET MVC to ASP.NET Core MVC](https://learn.microsoft.com/en-us/aspnet/core/migration/mvc)
