# ADR-002: Authentication and Authorization Strategy

## Status

Accepted

## Context

Contoso University currently has **no authentication or authorization**. The `AuthorizeAttribute` is commented out in `FilterConfig.cs`. All CRUD operations (create, edit, delete students, courses, instructors, departments) are publicly accessible to anyone who can reach the application.

The application uses IIS Express Windows Authentication in development (`IISExpressWindowsAuthentication=enabled` in `.csproj`), but this is not enforced at the application level.

This was identified as finding **S-1 (Critical)** in the modernization assessment.

## Decision Drivers

- Application manages educational records (PII: student names, enrollment data)
- Must support role-based access (admin vs. read-only users at minimum)
- Target deployment is Azure (per modernization plan)
- Should integrate with organizational identity if available

## Considered Options

### Option A: ASP.NET Core Identity

Built-in identity system with local user accounts, password hashing, role management, and two-factor authentication.

**Pros:**
- Zero external dependencies — ships with ASP.NET Core
- Full control over user data and authentication flow
- Supports local accounts, roles, claims, and external login providers
- Well-documented with extensive ecosystem

**Cons:**
- Requires managing user registration, password reset, account lockout
- Database tables needed for identity (can share the existing SQL Server)
- More operational burden (password policies, user management UI)

### Option B: Microsoft Entra ID (Azure AD)

Delegate authentication to Microsoft Entra ID. Users sign in with organizational accounts.

**Pros:**
- No local credential management — delegated to Microsoft
- Single sign-on (SSO) with other Microsoft 365 services
- Built-in MFA, conditional access, audit logging
- Natural fit for Azure-hosted applications

**Cons:**
- Requires Entra ID tenant (may not be available)
- More complex initial setup (app registration, redirect URIs)
- Offline/disconnected scenarios more difficult

### Option C: External Identity Provider (Auth0, Okta)

Use a third-party identity-as-a-service platform.

**Pros:**
- Feature-rich (social logins, MFA, anomaly detection)
- Reduces identity management burden

**Cons:**
- Additional cost (per-user pricing)
- External dependency and vendor lock-in
- Latency for authentication round-trips

## Decision

**Option A: ASP.NET Core Identity** — chosen for its zero external dependencies, self-contained architecture, and built-in role management. Identity tables are co-located in the existing SchoolContext database. Roles (Admin, Faculty, ReadOnly) are seeded on startup. This keeps the application fully self-contained without requiring an external identity provider, while still allowing future migration to Entra ID if organizational SSO is needed.

## Consequences

Regardless of the chosen option:
- All controllers must require authentication by default (`[Authorize]` globally)
- Role-based authorization needed: Admin (full CRUD), Faculty (view + limited edit), Student (view-only)
- Anti-forgery tokens must be enforced on all state-changing operations
- Sensitive operations (delete) should require elevated privileges

## References

- Assessment finding: S-1
- [ASP.NET Core Identity](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity)
- [Microsoft Entra ID with ASP.NET Core](https://learn.microsoft.com/en-us/entra/identity-platform/quickstart-web-app-aspnet-core-sign-in)
