# Security Assessment

## Summary

- **Assessment depth**: Level 2 (auto-escalated from Level 1 — 2 critical + 4 high findings)
- **Total findings**: 16
- **Critical**: 2 | **High**: 4 | **Medium**: 7 | **Low**: 3
- **OWASP categories affected**: A01 (Broken Access Control), A03 (Injection), A04 (Insecure Design), A05 (Security Misconfiguration), A07 (Authentication Failures)
- **Escalation triggered**: Yes — Level 1 → Level 2 (2 critical findings)
- **Dependency CVE scan**: Clean — `dotnet list package --vulnerable` reports no known vulnerabilities

## Findings

### Critical

| # | OWASP | Finding | Location | Remediation | Effort |
|---|-------|---------|----------|-------------|--------|
| S-01 | A07 | **Hardcoded DB credentials** — SQL Server SA password (`YourStr0ng!Pass`) hardcoded in docker-compose.yml, visible in version control | `docker-compose.yml:10,18` | Use `.env` file (gitignored) or Docker secrets for sensitive values; reference via `${DB_PASSWORD}` variable substitution | Hours |
| S-02 | A07 | **Hardcoded admin password** — Default admin user `admin@contoso.edu` created with hardcoded password `Admin123!` in source code | `Data/IdentitySeeder.cs:31` | Read admin password from configuration (`IConfiguration`); force password change on first login; or skip seeding admin in production | Hours |

### High

| # | OWASP | Finding | Location | Remediation | Effort |
|---|-------|---------|----------|-------------|--------|
| S-03 | A07 | **Weak password policy** — Minimum 6 characters, no special characters required. Easily brute-forced. | `Program.cs:27-28` | Set `RequiredLength=12`, `RequireNonAlphanumeric=true`, `RequireUppercase=true`, `RequiredUniqueChars=4` | Hours |
| S-04 | A03 | **Missing CSRF on MarkAsRead POST** — `NotificationsController.MarkAsRead()` is a state-changing POST endpoint without `[ValidateAntiForgeryToken]` | `Controllers/NotificationsController.cs` | Add `[ValidateAntiForgeryToken]` attribute; include token in AJAX request header | Hours |
| S-05 | A01 | **Anonymous access to notifications** — `[AllowAnonymous]` on `GetNotifications()` exposes all unread notification data (entity types, IDs, operations, usernames) to unauthenticated users | `Controllers/NotificationsController.cs` | Remove `[AllowAnonymous]`; require authentication. Move SignalR fallback to auth-only. | Hours |
| S-06 | A01 | **IDOR on grade management** — Any authenticated user can view and modify grades for ANY course via `/Courses/Grades/{id}` and `SaveGrades`. No check that the user is the course instructor or an admin. | `Controllers/CoursesController.cs:Grades,SaveGrades` | Add `[Authorize(Roles = "Faculty,Admin")]`; verify instructor owns the course via `CourseAssignment` lookup; return `Forbid()` if not authorized | Days |

### Medium

| # | OWASP | Finding | Location | Remediation | Effort |
|---|-------|---------|----------|-------------|--------|
| S-07 | A03 | **DOM-based XSS in notifications** — `innerHTML` used to render notification data (message, entityType, createdBy) from JSON. Malicious entity names could inject script. | `wwwroot/Scripts/notifications.js` | Replace `innerHTML` with `textContent` for text and `createElement` for structure | Hours |
| S-08 | A03 | **@Html.Raw() usage pattern** — 8 occurrences of `@Html.Raw(" \| ")` across 4 view files. Safe today (static string) but establishes dangerous pattern. | `Views/Students/Index.cshtml`, `Views/Courses/Index.cshtml`, `Views/Departments/Index.cshtml`, `Views/Instructors/Index.cshtml` | Replace with `<span> \| </span>` HTML elements | Hours |
| S-09 | A04 | **Exception details in user-facing errors** — `ex.Message` included in ModelState errors shown to users, leaking file paths, SQL errors, and internal details | `Controllers/CoursesController.cs:101,188,237` | Log full exception via `ILogger`; show generic "An error occurred" message to user | Hours |
| S-10 | A05 | **Missing Content-Security-Policy header** — CSP, Permissions-Policy, and X-Permitted-Cross-Domain-Policies headers not set. Only X-Content-Type-Options, X-Frame-Options, and Referrer-Policy are configured. | `Program.cs:55-61` | Add CSP (`default-src 'self'; script-src 'self' cdnjs.cloudflare.com`), Permissions-Policy (`camera=(), microphone=()`), X-Permitted-Cross-Domain-Policies (`none`) | Hours |
| S-11 | A01 | **Class-level AllowAnonymous on HomeController** — All actions (including `About` which queries student enrollment data) are publicly accessible. Future actions added to this controller would also bypass auth. | `Controllers/HomeController.cs:13` | Move `[AllowAnonymous]` to individual actions (Index, Contact); add `[Authorize]` on About | Hours |
| S-12 | A05 | **No explicit cookie security configuration** — ASP.NET Identity cookie not explicitly configured for HttpOnly, Secure, SameSite, or session timeout | `Program.cs` | Add `ConfigureApplicationCookie()` with `HttpOnly=true`, `SecurePolicy=Always`, `SameSite=Strict`, `ExpireTimeSpan=30min` | Hours |
| S-13 | A04 | **File upload validates extension only** — Teaching material uploads check file extension (`.jpg`, `.png`, etc.) but not MIME type or file content magic bytes. Attacker can rename malicious file to `.jpg`. | `Controllers/CoursesController.cs:63-71,140-148` | Add MIME type validation (`ContentType` check) and magic byte verification for image file signatures | Hours |

### Low

| # | OWASP | Finding | Location | Remediation | Effort |
|---|-------|---------|----------|-------------|--------|
| S-14 | A03 | **No AJAX anti-forgery token pattern** — JavaScript `fetch()` calls in notifications.js don't include anti-forgery tokens. If GET endpoints change to POST, CSRF protection would be missing. | `wwwroot/Scripts/notifications.js` | Establish pattern for including `X-CSRF-TOKEN` header in AJAX requests | Hours |
| S-15 | A03 | **No search input length validation** — Student search accepts unlimited length strings. EF Core parameterizes (safe from SQLi) but extremely long strings could cause performance degradation. | `Controllers/StudentsController.cs:37-40` | Add `searchString.Length > 100` truncation | Hours |
| S-16 | A05 | **No rate limiting** — No request rate limiting on any endpoint. Anonymous `GetNotifications` and login endpoints are vulnerable to brute-force and DoS. | `Program.cs`, all controllers | Add ASP.NET Core rate limiting middleware (`AddRateLimiter`) with fixed-window policy | Hours |

## OWASP Top 10 Coverage

| OWASP ID | Category | Findings | Status |
|----------|----------|----------|--------|
| A01 | Broken Access Control | S-05, S-06, S-11 | ⚠️ 3 findings (1 high, 2 medium) |
| A02 | Cryptographic Failures | — | ✅ No findings (ASP.NET Identity handles hashing) |
| A03 | Injection | S-04, S-07, S-08, S-14, S-15 | ⚠️ 5 findings (1 high, 2 medium, 2 low) |
| A04 | Insecure Design | S-09, S-13 | ⚠️ 2 findings (medium) |
| A05 | Security Misconfiguration | S-10, S-12, S-16 | ⚠️ 3 findings (2 medium, 1 low) |
| A06 | Vulnerable Components | — | ✅ Clean — no CVEs in dependencies |
| A07 | Auth Failures | S-01, S-02, S-03 | 🔴 3 findings (2 critical, 1 high) |
| A08 | Integrity Failures | — | ✅ No findings |
| A09 | Logging/Monitoring Failures | — | ✅ Structured logging implemented (mod-003) |
| A10 | SSRF | — | ✅ No findings (no outbound HTTP calls) |

## Remediation Roadmap

### Immediate (before any deployment)

1. **S-01**: Externalize docker-compose credentials to `.env` file (gitignored)
2. **S-02**: Move admin password to configuration; add `[ChangePasswordOnFirstLogin]` flow or use env var
3. **S-05**: Remove `[AllowAnonymous]` from `GetNotifications()` — require auth
4. **S-04**: Add `[ValidateAntiForgeryToken]` to `MarkAsRead()`

### High Priority (next sprint)

5. **S-06**: Add role-based auth + instructor ownership check on grade management
6. **S-03**: Strengthen password policy (12+ chars, special chars required)
7. **S-07**: Fix DOM XSS — replace `innerHTML` with `textContent`/`createElement`

### Medium Priority (hardening)

8. **S-10**: Add Content-Security-Policy and Permissions-Policy headers
9. **S-12**: Configure cookie security (HttpOnly, Secure, SameSite, timeout)
10. **S-13**: Add MIME type + magic byte validation for file uploads
11. **S-09**: Replace `ex.Message` in user-facing errors with generic messages
12. **S-11**: Move `[AllowAnonymous]` to per-action on HomeController
13. **S-08**: Replace `@Html.Raw(" | ")` with `<span> | </span>`

### Low Priority (defense-in-depth)

14. **S-16**: Add rate limiting middleware
15. **S-15**: Add search input length truncation
16. **S-14**: Establish AJAX anti-forgery token pattern

## Decision Points

| Decision | Recommendation | ADR Needed? |
|----------|---------------|:-----------:|
| Secrets management strategy | Use `.env` files for local dev; Azure Key Vault for production deployment | No — standard practice, not architectural |
| Grade authorization model | Instructor-course ownership check via `CourseAssignment` table + Admin override | No — straightforward RBAC extension |
| Rate limiting strategy | ASP.NET Core built-in `AddRateLimiter` with fixed-window policy (100 req/min) | No — framework built-in, not architectural decision |
