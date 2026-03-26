# Increment Plan — Modernization

> Generated from `specs/assessment/modernization.md` (28 findings) and ADRs 001–003.
> Testability track: **not yet decided** — behavioral deltas include both Track A (Gherkin) and Track B (doc) placeholders.

## Dependency Graph

```
mod-001 ──┬── mod-002 (MSMQ → SignalR)
           ├── mod-003 (Structured logging)
           ├── mod-004 (Async/await) ──┬── mod-007 (Controller refactoring) ── mod-013 (View models)
           │                           ├── mod-008 (Query optimization)
           │                           └── mod-009 (Tests) ── mod-010 (CI/CD)
           ├── mod-005 (Security hardening)
           ├── mod-006 (Authentication) ★ blocked on ADR-002 decision
           ├── mod-011 (Containerization)
           ├── mod-012 (Dead code removal)
           └── mod-014 (Entity audit trail)
```

---

## mod-001: Migrate to .NET 10 + ASP.NET Core MVC + EF Core 9

- **Type:** modernization
- **Findings addressed:** A-1 (Critical), D-1 (Critical), P-1 (Critical), A-2 (Critical), D-2 (High), P-2 (High), CI-3 (Low), S-4 (Low)
- **ADR compliance:** ADR-001 (Full migration to .NET 10)
- **Scope:**
  - Convert old-style `.csproj` to SDK-style, retarget from `net48` to `net10.0`
  - Replace `Global.asax` + `Global.asax.cs` with `Program.cs` (minimal hosting)
  - Replace `Web.config` with `appsettings.json` + `appsettings.Development.json`
  - Replace `packages.config` with `<PackageReference>` in `.csproj`
  - Upgrade Entity Framework Core 3.1.32 → EF Core 9 (latest stable on .NET 10)
  - Adopt built-in DI container: register `SchoolContext` via `AddDbContext<SchoolContext>()` with scoped lifetime
  - Replace `BaseController` static `SchoolContextFactory.Create()` with constructor-injected `SchoolContext`
  - Replace `System.Web` dependencies: `Server.MapPath()` → `IWebHostEnvironment.WebRootPath`, `HttpPostedFileBase` → `IFormFile`, `BundleConfig` → `<link>`/`<script>` tags or LibMan
  - Replace `App_Start/RouteConfig.cs` with attribute routing + `MapControllerRoute()` in `Program.cs`
  - Replace `App_Start/FilterConfig.cs` with `builder.Services.AddControllersWithViews(options => ...)` filter registration
  - Stub MSMQ notification service: replace `System.Messaging`-based `NotificationService` with a simple `DatabaseNotificationService` that writes directly to the `Notification` table (no real-time push yet — that comes in mod-002)
  - Migrate Razor views: update `@using`, remove `@Scripts.Render`/`@Styles.Render`, replace with direct `<script>`/`<link>` tags
  - Remove all 40+ binding redirects (no longer needed)
  - Remove `packages/` directory (NuGet restore replaces it)
  - **Stays the same:** All entity models, all controller business logic, all Razor view markup, database schema, seed data, Bootstrap 5 + jQuery frontend
- **Acceptance Criteria:**
  - [ ] Application builds on .NET 10 with zero errors and zero warnings
  - [ ] `dotnet run` starts the application and serves the home page
  - [ ] Student CRUD operations work (create, read, edit, delete)
  - [ ] Course CRUD with image upload works
  - [ ] Instructor CRUD with course assignment checkboxes works
  - [ ] Department CRUD with concurrency handling works
  - [ ] About page displays enrollment statistics
  - [ ] Notifications are written to database on CRUD operations (no MSMQ)
  - [ ] Connection string read from `appsettings.json`
  - [ ] DI container resolves `SchoolContext` per-request (scoped lifetime)
  - [ ] No `System.Web`, `System.Messaging`, or `Global.asax` references remain
- **Test Strategy:**
  - Manual smoke test: all 6 CRUD controller flows (Students, Courses, Instructors, Departments, Notifications, Home)
  - Build verification: `dotnet build` succeeds with no warnings
  - Database verification: `EnsureCreated()` or migrations apply, seed data loads
  - DI verification: controllers receive injected `SchoolContext` (not static factory)
- **Behavioral Deltas:**
  - No user-facing behavioral changes expected — this is a runtime swap
  - Exception: Notification delivery becomes synchronous database write instead of MSMQ enqueue (temporary until mod-002 adds real-time push)
  - Regression: All existing page flows, forms, and data operations must work identically
- **Dependencies:** none (walking skeleton)
- **Rollback Plan:** Git revert to pre-migration commit. The .NET Framework 4.8 project is fully preserved in git history.
- **Risk:** **Medium** — Proven migration path for small apps. `System.Web` → ASP.NET Core API differences require careful mapping. MSMQ removal is the highest-risk sub-task (replaced with database stub). .NET Upgrade Assistant can automate ~60% of mechanical changes.

---

## mod-002: Replace MSMQ Notification Service with Database + SignalR

- **Type:** modernization
- **Findings addressed:** A-3 (High)
- **ADR compliance:** ADR-003 (In-process Channel + SignalR)
- **Scope:**
  - Create `INotificationService` interface with `SendNotification()`, `GetPendingNotifications()`, `MarkAsRead()` methods
  - Implement `DatabaseNotificationService` replacing the mod-001 stub: persist notifications to `Notification` table via EF Core, query unread notifications from database
  - Add ASP.NET Core SignalR hub (`NotificationHub`) that pushes new notifications to connected clients in real-time
  - Update `NotificationsController.GetNotifications()` to read from database instead of MSMQ
  - Implement `MarkAsRead()` properly — update `IsRead` flag and `ReadAt` timestamp in database
  - Replace frontend polling JavaScript (`notifications.js`) with SignalR client library (`@microsoft/signalr`)
  - Register `INotificationService` as scoped service in DI
  - Remove dead MSMQ configuration from `appsettings.json` (`NotificationQueuePath`)
  - **Stays the same:** Notification entity model, notification UI styling, color-coding (green/blue/orange), auto-dismiss behavior, notification dashboard page
- **Acceptance Criteria:**
  - [ ] Creating a student triggers a real-time notification in the UI (no 5-second polling delay)
  - [ ] Notifications persist in the database `Notification` table
  - [ ] `MarkAsRead()` updates `IsRead=true` and `ReadAt` timestamp
  - [ ] Notification dashboard page loads and displays system info (updated for SignalR)
  - [ ] No MSMQ references remain in the codebase
  - [ ] No `System.Messaging` NuGet package referenced
  - [ ] SignalR connection established on page load (visible in browser dev tools)
- **Test Strategy:**
  - Unit test: `DatabaseNotificationService.SendNotification()` writes to in-memory database
  - Unit test: `GetPendingNotifications()` returns unread notifications ordered by CreatedAt
  - Unit test: `MarkAsRead()` sets IsRead and ReadAt
  - Integration test: SignalR hub receives and broadcasts notification
  - Manual smoke test: open two browser tabs, create entity in one, see notification in both
- **Behavioral Deltas:**
  - Modified: Notification delivery changes from polling (5-second delay) to real-time push (sub-second)
  - New: `MarkAsRead()` now functional (was a stub)
  - New: Notifications persist in database (previously lost when consumed from MSMQ)
  - Regression: All CRUD operations still trigger notifications; UI appearance unchanged
- **Dependencies:** mod-001
- **Rollback Plan:** Revert to database-only stub from mod-001 (no real-time push, but notifications still persist).
- **Risk:** **Low** — SignalR is well-documented and built into ASP.NET Core. Notification requirements are simple (broadcast to all connected clients).

---

## mod-003: Implement Structured Logging

- **Type:** modernization
- **Findings addressed:** P-4 (High)
- **Scope:**
  - Add `Microsoft.Extensions.Logging` with `ILogger<T>` injection in all controllers and services
  - Replace all `Debug.WriteLine()` calls with `_logger.LogDebug()`
  - Replace all `Trace.TraceError()` calls with `_logger.LogError()`
  - Replace silent exception swallowing with `_logger.LogError(ex, "...")` including exception object
  - Configure logging providers in `Program.cs`: Console (development), structured JSON (production)
  - Add request logging middleware for HTTP request/response tracking
  - **Stays the same:** All business logic, all controller flow, all error handling behavior (errors are now logged, not swallowed)
- **Acceptance Criteria:**
  - [ ] Zero `Debug.WriteLine()` or `Trace.TraceError()` calls remain in codebase
  - [ ] All controllers use `ILogger<T>` via constructor injection
  - [ ] Exceptions logged with full stack trace (not silently discarded)
  - [ ] Console output shows structured log entries during `dotnet run`
  - [ ] Log levels configurable via `appsettings.json` `Logging` section
- **Test Strategy:**
  - Unit test: verify logger receives expected log calls (using `NullLogger<T>` or mock)
  - Code review: grep for any remaining `Debug.WriteLine` or `Trace.TraceError`
  - Manual: trigger an error (e.g., invalid model state) and verify log output
- **Behavioral Deltas:**
  - No user-facing behavioral changes — logging is infrastructure
  - Regression: All controller actions function identically
- **Dependencies:** mod-001 (needs DI for `ILogger<T>` injection)
- **Rollback Plan:** Revert logging changes; restore `Debug.WriteLine` calls from git history.
- **Risk:** **Low** — Mechanical replacement. `ILogger<T>` is built into ASP.NET Core.

---

## mod-004: Convert All I/O to Async/Await

- **Type:** modernization
- **Findings addressed:** A-4 (High)
- **Scope:**
  - Convert all controller actions to `async Task<IActionResult>`
  - Replace synchronous EF Core calls: `ToList()` → `ToListAsync()`, `SaveChanges()` → `SaveChangesAsync()`, `Find()` → `FindAsync()`, `Single()` → `SingleAsync()`, `FirstOrDefault()` → `FirstOrDefaultAsync()`
  - Convert file I/O in `CoursesController`: synchronous `SaveAs()` → `CopyToAsync()`, `File.Delete()` → `File.Delete()` (sync is acceptable for delete)
  - Update `PaginatedList.Create()` to `async Task<PaginatedList<T>> CreateAsync()`
  - **Stays the same:** All business logic, all controller flow, all view rendering
- **Acceptance Criteria:**
  - [ ] Zero synchronous EF Core calls remain (`ToList()`, `SaveChanges()`, `Find()`, `Single()`)
  - [ ] All controller actions return `async Task<IActionResult>`
  - [ ] `PaginatedList.CreateAsync()` is async
  - [ ] Application functions identically to synchronous version
  - [ ] No deadlocks or `Task.Result` / `.Wait()` anti-patterns
- **Test Strategy:**
  - Build verification: `dotnet build` with no async-related warnings
  - Manual smoke test: all CRUD flows still work
  - Code review: grep for remaining synchronous EF calls
- **Behavioral Deltas:**
  - No user-facing behavioral changes — async is a runtime optimization
  - Regression: All pages and operations function identically
- **Dependencies:** mod-001 (needs EF Core 9 for full async API)
- **Rollback Plan:** Revert async changes; restore synchronous calls from git history.
- **Risk:** **Low** — Mechanical conversion. EF Core 9 has complete async API coverage.

---

## mod-005: Security Hardening — XSS Protection and Environment Configuration

- **Type:** modernization
- **Findings addressed:** S-2 (High), S-3 (Medium)
- **Scope:**
  - ASP.NET Core Razor automatically HTML-encodes output (replaces `validateRequest="false"` concern) — verify all views use `@Model.Property` (encoded) not `@Html.Raw()` (unencoded)
  - Add Content Security Policy (CSP) headers via middleware
  - Configure `ASPNETCORE_ENVIRONMENT` for debug vs production behavior
  - Ensure detailed error pages only show in Development environment (`app.UseDeveloperExceptionPage()` guarded by `env.IsDevelopment()`)
  - Add HSTS and HTTPS redirection middleware
  - **Stays the same:** All business logic, all views, all forms
- **Acceptance Criteria:**
  - [ ] No `@Html.Raw()` calls exist in views (or they are justified and reviewed)
  - [ ] CSP header present in HTTP responses
  - [ ] `ASPNETCORE_ENVIRONMENT=Production` shows generic error page (no stack traces)
  - [ ] `ASPNETCORE_ENVIRONMENT=Development` shows developer exception page
  - [ ] HSTS header present in production responses
- **Test Strategy:**
  - Manual: inspect response headers for CSP and HSTS
  - Manual: trigger error in Production mode → verify generic error page
  - Code review: grep for `@Html.Raw` usage
- **Behavioral Deltas:**
  - New: CSP and HSTS headers added to all responses
  - Modified: Error page behavior changes based on environment variable (not `debug="true"` in config)
  - Regression: All pages render correctly with HTML encoding
- **Dependencies:** mod-001
- **Rollback Plan:** Remove CSP middleware; revert environment checks.
- **Risk:** **Low** — ASP.NET Core provides these capabilities out of the box.

---

## mod-006: Add Authentication and Authorization

- **Type:** modernization
- **Findings addressed:** S-1 (Critical)
- **ADR compliance:** ADR-002 (Deferred — requires user decision on auth provider)
- **Scope:**
  - Implement chosen auth provider (ASP.NET Core Identity OR Microsoft Entra ID per ADR-002 decision)
  - Add global `[Authorize]` filter — all controllers require authentication by default
  - Define roles: Admin (full CRUD), Faculty (view + limited edit), ReadOnly (view only)
  - Add role-based `[Authorize(Roles = "Admin")]` to delete actions
  - Add login/logout UI (nav bar integration)
  - Update `Notification.CreatedBy` to use actual authenticated user name instead of "System"
  - **Stays the same:** All CRUD logic, all views, all data models
- **Acceptance Criteria:**
  - [ ] Unauthenticated users are redirected to login page
  - [ ] Authenticated users can access Index/Details pages
  - [ ] Only Admin role users can Create/Edit/Delete entities
  - [ ] Login/Logout links visible in navigation bar
  - [ ] `Notification.CreatedBy` contains the authenticated user's name
  - [ ] Anti-forgery tokens enforced on all state-changing operations
- **Test Strategy:**
  - Unit test: anonymous request returns 401/redirect
  - Unit test: non-Admin user cannot access Delete endpoints
  - Integration test: full login → CRUD → logout flow
  - Manual smoke test: login with different roles, verify access
- **Behavioral Deltas:**
  - New: Login page required before accessing any page
  - New: Role-based access restrictions on Create/Edit/Delete
  - New: Login/Logout links in navigation
  - Modified: `Notification.CreatedBy` shows real username
  - Regression: After login, all CRUD operations work as before
- **Dependencies:** mod-001 (needs ASP.NET Core auth middleware)
- **Blocked on:** ADR-002 decision (user must choose auth provider)
- **Rollback Plan:** Remove auth middleware and `[Authorize]` attributes; all endpoints become public again.
- **Risk:** **Medium** — Auth integration touches every controller. Entra ID requires Azure app registration. Identity requires database schema additions.

---

## mod-007: Refactor Controller Patterns and Validation

- **Type:** modernization
- **Findings addressed:** P-5 (Medium), P-6 (Medium), P-7 (Medium), P-8 (Medium)
- **Scope:**
  - Replace `TryUpdateModel()` in `StudentsController.Edit()` and `InstructorsController.Edit()` with explicit view models and `[FromForm]` model binding
  - Create a custom `[ValidDateRange]` validation attribute for DateTime range checks (1753–9999), replacing duplicated inline validation in Create/Edit actions
  - Fix `DbInitializer.cs`: use `DateTime.ParseExact()` or `CultureInfo.InvariantCulture` for date parsing
  - Refactor `PaginatedList<T>` to use composition (contain a `List<T>`) instead of inheritance
  - **Stays the same:** All business logic outcomes, all view rendering, all data operations
- **Acceptance Criteria:**
  - [ ] Zero `TryUpdateModel()` calls remain
  - [ ] Date validation logic exists in ONE place (custom attribute)
  - [ ] `DbInitializer` uses culture-invariant date parsing
  - [ ] `PaginatedList<T>` does not inherit from `List<T>`
  - [ ] All existing CRUD operations function identically
- **Test Strategy:**
  - Unit test: custom `[ValidDateRange]` attribute validates correct/incorrect dates
  - Unit test: `PaginatedList<T>` properties (HasPreviousPage, HasNextPage, TotalPages) work correctly
  - Manual smoke test: create/edit student with boundary dates
- **Behavioral Deltas:**
  - No user-facing behavioral changes
  - Regression: Date validation, pagination, and edit flows work identically
- **Dependencies:** mod-001, mod-004 (async controllers should be done first)
- **Rollback Plan:** Revert to previous controller patterns from git history.
- **Risk:** **Low** — Refactoring with identical behavior. Small, contained changes.

---

## mod-008: Optimize Data Access Patterns

- **Type:** modernization
- **Findings addressed:** P-3 (High)
- **Scope:**
  - Replace `.Single()` calls with `.SingleOrDefaultAsync()` + null checks in all controllers
  - Refactor `DbInitializer`: replace individual `Add()` + `SaveChanges()` loops with `AddRange()` + single `SaveChangesAsync()`
  - Review `InstructorsController.Index()` eager loading: add pagination or explicit projections if dataset is large
  - Replace enrollment seeding N+1 pattern (`.Single()` per enrollment) with lookup dictionary
  - **Stays the same:** All query results, all data displayed, all seed data content
- **Acceptance Criteria:**
  - [ ] Zero `.Single()` calls without null-check fallback
  - [ ] `DbInitializer` uses `AddRange()` and single `SaveChangesAsync()`
  - [ ] No N+1 query patterns in seeding or controller queries
  - [ ] Instructor index loads efficiently (verify with EF Core logging)
- **Test Strategy:**
  - Unit test: controller returns NotFound when entity doesn't exist (not exception)
  - Integration test: seed data loads correctly with batched operations
  - Performance: enable EF Core query logging, verify query count for Instructor index
- **Behavioral Deltas:**
  - Modified: `.Single()` throwing `InvalidOperationException` on missing data → returning 404 Not Found page
  - Regression: All data is displayed correctly; seed data is identical
- **Dependencies:** mod-001, mod-004 (async queries)
- **Rollback Plan:** Revert data access changes from git history.
- **Risk:** **Low** — Query behavior changes are minimal. Null-check addition is strictly safer.

---

## mod-009: Add Unit and Integration Test Project

- **Type:** modernization
- **Findings addressed:** T-1 (Critical), T-2 (High), T-3 (Medium)
- **Scope:**
  - Add `ContosoUniversity.Tests` xUnit project to solution
  - Add test infrastructure: `WebApplicationFactory<T>` for integration tests, in-memory database provider for unit tests
  - Write unit tests for:
    - All model validation attributes (boundary values, null handling)
    - `PaginatedList<T>` behavior
    - Custom `[ValidDateRange]` attribute (from mod-007)
    - `DatabaseNotificationService` (from mod-002)
  - Write integration tests for:
    - Each controller's Index, Create, Edit, Delete actions via `WebApplicationFactory`
    - Database seeding via `DbInitializer`
    - SignalR notification delivery (from mod-002)
  - Target: >80% code coverage on controllers and services
  - **Stays the same:** All application code — tests are additive only
- **Acceptance Criteria:**
  - [ ] `dotnet test` runs and all tests pass
  - [ ] At least 1 unit test per model validation attribute
  - [ ] At least 1 integration test per controller (CRUD cycle)
  - [ ] Test project references main project correctly
  - [ ] In-memory database used for isolated test execution (no LocalDB dependency)
- **Test Strategy:** This IS the test strategy — the increment creates the test infrastructure.
- **Behavioral Deltas:**
  - No application behavioral changes — tests are additive
  - Regression: Application code is unmodified
- **Dependencies:** mod-001 (DI for testability), mod-003 (logging settled), mod-004 (async patterns settled)
- **Rollback Plan:** Remove test project from solution.
- **Risk:** **Low** — Additive only. Does not modify application code.

---

## mod-010: Add CI/CD Pipeline with GitHub Actions

- **Type:** modernization
- **Findings addressed:** CI-1 (High)
- **Scope:**
  - Create `.github/workflows/ci.yml` with: checkout → setup .NET 10 → restore → build → test → (optional) deploy
  - Trigger on push to `main` and pull requests
  - Fail pipeline if any test fails or build has warnings
  - Add build status badge to README
  - **Stays the same:** All application code, all test code
- **Acceptance Criteria:**
  - [ ] Push to `main` triggers CI pipeline
  - [ ] Pipeline builds, runs tests, and reports results
  - [ ] PR checks block merge on test failure
  - [ ] Build badge visible in repository README
- **Test Strategy:**
  - Trigger pipeline via push; verify green status
  - Introduce intentional test failure; verify red status
- **Behavioral Deltas:**
  - No application behavioral changes — CI is infrastructure
- **Dependencies:** mod-001 (.NET 10 project), mod-009 (tests to run)
- **Rollback Plan:** Delete workflow file.
- **Risk:** **Low** — GitHub Actions for .NET is well-documented.

---

## mod-011: Add Dockerfile and Container Support

- **Type:** modernization
- **Findings addressed:** CI-2 (Medium)
- **Scope:**
  - Create multi-stage `Dockerfile`: build stage (SDK image) → runtime stage (ASP.NET runtime image)
  - Create `docker-compose.yml` with app service + SQL Server container for local development
  - Configure app to read connection string from environment variable (container-friendly)
  - Add `.dockerignore` for build context optimization
  - **Stays the same:** All application code — Dockerfile is additive
- **Acceptance Criteria:**
  - [ ] `docker build` produces a runnable container image
  - [ ] `docker-compose up` starts app + SQL Server and serves the home page
  - [ ] Container image is <200MB (multi-stage build)
  - [ ] Application reads connection string from environment variable in container
- **Test Strategy:**
  - `docker build` succeeds
  - `docker-compose up` → navigate to localhost → verify home page
  - Run one CRUD operation in containerized app
- **Behavioral Deltas:**
  - No application behavioral changes — containerization is deployment infrastructure
- **Dependencies:** mod-001 (.NET 10 for cross-platform container support)
- **Rollback Plan:** Delete Dockerfile and docker-compose.yml.
- **Risk:** **Low** — .NET 10 has excellent container support with optimized base images.

---

## mod-012: Remove Dead Frontend Dependencies

- **Type:** modernization
- **Findings addressed:** D-4 (Medium), D-5 (Low), D-6 (Low)
- **Scope:**
  - Remove Modernizr 2.6.2 (script file + bundle reference)
  - Remove respond.js (script file + bundle reference)
  - Remove Antlr3.Runtime (eliminated by modern bundling)
  - Clean up bundling: replace `System.Web.Optimization` bundles (removed in mod-001) with direct `<script>`/`<link>` tags or LibMan for client-side dependencies
  - Verify Bootstrap 5 and jQuery load correctly without bundling middleware
  - **Stays the same:** All page layouts, all interactive functionality, Bootstrap styling, jQuery validation
- **Acceptance Criteria:**
  - [ ] No Modernizr references in views or scripts
  - [ ] No respond.js references
  - [ ] Bootstrap and jQuery load correctly on all pages
  - [ ] jQuery validation works on all forms
  - [ ] No 404 errors for script/style resources in browser console
- **Test Strategy:**
  - Manual: browse all pages, check browser console for errors
  - Manual: submit forms with validation errors → verify client-side validation works
- **Behavioral Deltas:**
  - Removed: Modernizr feature detection (no longer needed for modern browsers)
  - Removed: respond.js IE9 polyfill (IE is EOL)
  - Regression: All pages render identically; all forms validate correctly
- **Dependencies:** mod-001 (bundling approach changes in .NET Core)
- **Rollback Plan:** Re-add script files and references from git history.
- **Risk:** **Low** — Removing unused libraries. No functionality depends on Modernizr or respond.js.

---

## mod-013: Strongly Type All Views and View Models

- **Type:** modernization
- **Findings addressed:** P-9 (Low)
- **Scope:**
  - Create dedicated view models for list/filter views: `StudentListViewModel` (CurrentSort, CurrentFilter, SearchString, Students), `InstructorListViewModel`, etc.
  - Replace all `ViewBag` usage in controllers and views with strongly-typed view model properties
  - **Stays the same:** All data displayed, all filtering/sorting behavior, all view layouts
- **Acceptance Criteria:**
  - [ ] Zero `ViewBag` usage in controllers (grep verification)
  - [ ] All views have `@model` directive matching a concrete view model
  - [ ] Sort/filter/page parameters flow through view models, not dynamic properties
  - [ ] All existing list, search, sort, and pagination behavior works identically
- **Test Strategy:**
  - Build verification: compile-time type checking catches errors
  - Manual smoke test: student search + sort + pagination still works
  - Unit test: view model properties are correctly populated by controllers
- **Behavioral Deltas:**
  - No user-facing behavioral changes — view models are internal refactoring
  - Regression: All pages display identical data and behavior
- **Dependencies:** mod-007 (controller refactoring done first)
- **Rollback Plan:** Revert to ViewBag-based parameter passing from git history.
- **Risk:** **Low** — Mechanical refactoring with compile-time type safety.

---

## mod-014: Add Entity Audit Trail

- **Type:** modernization
- **Findings addressed:** A-6 (Low)
- **Scope:**
  - Create `IAuditable` interface with `CreatedAt`, `ModifiedAt`, `CreatedBy`, `ModifiedBy` properties
  - Implement interface on all entity models (Student, Instructor, Course, Department, Enrollment, CourseAssignment, OfficeAssignment)
  - Add EF Core `SaveChanges` interceptor that automatically populates audit fields on insert/update
  - Add database migration for new audit columns
  - Extend `RowVersion` (optimistic concurrency) from Department-only to all entities
  - **Stays the same:** All business logic, all views (audit fields not displayed in existing views)
- **Acceptance Criteria:**
  - [ ] All entities implement `IAuditable`
  - [ ] Creating a student sets `CreatedAt` and `CreatedBy` automatically
  - [ ] Editing a student sets `ModifiedAt` and `ModifiedBy` automatically
  - [ ] `RowVersion` present on all entities (not just Department)
  - [ ] Database migration applies cleanly to existing data
  - [ ] Existing data gets `CreatedAt = migration timestamp`, `CreatedBy = "migration"`
- **Test Strategy:**
  - Unit test: SaveChanges interceptor populates audit fields for new entities
  - Unit test: SaveChanges interceptor updates `ModifiedAt`/`ModifiedBy` on edits
  - Integration test: create entity → verify audit fields in database
  - Manual: edit entity → verify `ModifiedAt` updated
- **Behavioral Deltas:**
  - New: All entities track creation and modification metadata
  - New: Optimistic concurrency on all entities (not just Department)
  - Regression: All CRUD operations work identically; no visible UI changes
- **Dependencies:** mod-001 (needs EF Core 9 interceptors)
- **Rollback Plan:** Revert entity changes and migration; remove interceptor.
- **Risk:** **Low** — Additive schema change. Migration for existing data is straightforward (default values).

---

## Priority Order Summary

| Order | Increment | Severity Addressed | Risk | Key Dependency |
|:-----:|-----------|-------------------|------|----------------|
| 1 | **mod-001** | 4× Critical, 2× High, 2× Low | Medium | — (walking skeleton) |
| 2 | **mod-002** | 1× High | Low | mod-001 |
| 3 | **mod-003** | 1× High | Low | mod-001 |
| 4 | **mod-004** | 1× High | Low | mod-001 |
| 5 | **mod-005** | 1× High, 1× Medium | Low | mod-001 |
| 6 | **mod-006** | 1× Critical | Medium | mod-001 + ADR-002 ★ |
| 7 | **mod-007** | 4× Medium | Low | mod-001, mod-004 |
| 8 | **mod-008** | 1× High | Low | mod-001, mod-004 |
| 9 | **mod-009** | 1× Critical, 1× High, 1× Medium | Low | mod-001, mod-003, mod-004 |
| 10 | **mod-010** | 1× High | Low | mod-001, mod-009 |
| 11 | **mod-011** | 1× Medium | Low | mod-001 |
| 12 | **mod-012** | 1× Medium, 2× Low | Low | mod-001 |
| 13 | **mod-013** | 1× Low | Low | mod-007 |
| 14 | **mod-014** | 1× Low | Low | mod-001 |

## Finding Coverage Matrix

All 28 assessment findings are addressed:

| Finding | Severity | Increment |
|---------|----------|-----------|
| A-1 | Critical | mod-001 |
| D-1 | Critical | mod-001 |
| P-1 | Critical | mod-001 |
| A-2 | Critical | mod-001 |
| T-1 | Critical | mod-009 |
| S-1 | Critical | mod-006 |
| D-2 | High | mod-001 |
| P-2 | High | mod-001 |
| P-3 | High | mod-008 |
| P-4 | High | mod-003 |
| A-3 | High | mod-002 |
| A-4 | High | mod-004 |
| T-2 | High | mod-009 |
| CI-1 | High | mod-010 |
| S-2 | High | mod-005 |
| D-3 | Medium | — (jQuery kept; active, low risk) |
| D-4 | Medium | mod-012 |
| P-5 | Medium | mod-007 |
| P-6 | Medium | mod-007 |
| P-7 | Medium | mod-007 |
| P-8 | Medium | mod-007 |
| A-5 | Medium | — (Modular monolith acceptable at this scale per assessment) |
| T-3 | Medium | mod-009 |
| CI-2 | Medium | mod-011 |
| S-3 | Medium | mod-005 |
| D-5 | Low | mod-012 |
| D-6 | Low | mod-012 |
| CI-3 | Low | mod-001 |
| P-9 | Low | mod-013 |
| A-6 | Low | mod-014 |
| S-4 | Low | mod-001 |

---

# Increment Plan — Security Remediation

> Generated from `specs/assessment/security.md` (16 findings).
> Appended to existing modernization increment plan.
> Strict tier ordering: Tier 1 → Tier 2 → Tier 3 → Tier 4.

## Security Dependency Graph

```
Tier 1 (Immediate):
  sec-001 (Hardcoded secrets)          — no deps
  sec-002 (Anonymous notification access) — no deps

Tier 2 (High):
  sec-003 (IDOR grades)                — no deps
  sec-004 (Missing CSRF)               — no deps
  sec-005 (Weak password policy)       — no deps
  sec-006 (DOM XSS)                    — no deps

Tier 3 (Medium):
  sec-007 (Exception disclosure + Html.Raw + AllowAnonymous scope)  — no deps
  sec-008 (CSP headers + Cookie security)                           — no deps
  sec-009 (File upload content validation)                          — no deps

Tier 4 (Low):
  sec-010 (Rate limiting + Input length + AJAX anti-forgery)        — no deps
```

---

## Tier 1 — Critical (Immediate)

---

## sec-001: Remove Hardcoded Credentials from Source Code

- **Type:** security
- **Tier:** 1 (Critical)
- **Vulnerability:** Hardcoded SQL Server SA password and admin user password in version-controlled files (findings S-01, S-02)
- **OWASP:** A07:2021 — Identification and Authentication Failures
- **Scope:**
  - `docker-compose.yml` — Replace hardcoded `SA_PASSWORD` and connection string with `${DB_PASSWORD}` environment variable substitution
  - Create `.env.example` with placeholder values; create `.env` with actual values; add `.env` to `.gitignore`
  - `Data/IdentitySeeder.cs` — Read admin password from `IConfiguration["Identity:AdminPassword"]` instead of hardcoded string; skip seeding if config not present
  - `Program.cs` — Pass `IConfiguration` to `IdentitySeeder.SeedAsync()`
  - No other changes.
- **Acceptance Criteria:**
  - [ ] No passwords or secrets appear in any tracked source file
  - [ ] `docker-compose.yml` references `${DB_PASSWORD}` variable (not literal password)
  - [ ] `.env` file exists but is gitignored
  - [ ] `.env.example` committed with placeholder values
  - [ ] `IdentitySeeder` reads admin password from configuration
  - [ ] Application starts correctly when `.env` file provides required values
  - [ ] Application logs a warning (not crash) when admin password config is missing
- **Test Strategy:**
  - Verify: `git grep -i password -- ':!*.md' ':!*.example'` returns no results
  - Integration test: app starts with valid config
  - Integration test: IdentitySeeder skips gracefully when password config missing
  - Regression: all 65 existing tests pass
- **Behavioral Deltas:**
  - New: Application requires `Identity:AdminPassword` config or env var for admin seeding
  - Modified: docker-compose requires `.env` file to start
  - Regression: All CRUD operations, auth flows, notifications unchanged
- **Dependencies:** none
- **Rollback Plan:** Revert docker-compose.yml and IdentitySeeder.cs; restore hardcoded values
- **Risk:** Low — Configuration plumbing only. No business logic changes.

---

## sec-002: Remove Anonymous Access to Notifications Endpoint

- **Type:** security
- **Tier:** 1 (Critical — authentication bypass)
- **Vulnerability:** `[AllowAnonymous]` on `GetNotifications()` exposes all unread notification data to unauthenticated users (finding S-05)
- **OWASP:** A01:2021 — Broken Access Control
- **Scope:**
  - `Controllers/NotificationsController.cs` — Remove `[AllowAnonymous]` from `GetNotifications()`. The global `[Authorize]` filter will apply.
  - No other changes.
- **Acceptance Criteria:**
  - [ ] Unauthenticated GET to `/Notifications/GetNotifications` returns 302 redirect to login (not 200 with data)
  - [ ] Authenticated GET to `/Notifications/GetNotifications` returns 200 with JSON
  - [ ] SignalR notifications still work for authenticated users
- **Test Strategy:**
  - New test: anonymous GET to `/Notifications/GetNotifications` returns redirect
  - Existing test: authenticated GET returns JSON (verify still passes)
  - Regression: all 65 existing tests pass
- **Behavioral Deltas:**
  - Modified: `/Notifications/GetNotifications` now requires authentication
  - Regression: Authenticated notification flow unchanged
- **Dependencies:** none
- **Rollback Plan:** Re-add `[AllowAnonymous]` to `GetNotifications()`
- **Risk:** Low — Single attribute removal. SignalR push (primary path) unaffected.

---

## Tier 2 — High

---

## sec-003: Add Authorization Check on Grade Management (IDOR Fix)

- **Type:** security
- **Tier:** 2 (High)
- **Vulnerability:** Any authenticated user can view and modify grades for ANY course. No role check or instructor ownership verification. (finding S-06)
- **OWASP:** A01:2021 — Broken Access Control (IDOR)
- **Scope:**
  - `Controllers/CoursesController.cs` — Add `[Authorize(Roles = "Admin,Faculty")]` to `Grades()` and `SaveGrades()` actions. Inside both actions, verify the current user is either an Admin or the instructor assigned to the course via `CourseAssignment` table. Return `Forbid()` if not authorized.
  - No other changes.
- **Acceptance Criteria:**
  - [ ] Unauthenticated user is redirected to login on `/Courses/Grades/1050`
  - [ ] User with ReadOnly role receives 403 Forbidden on `/Courses/Grades/1050`
  - [ ] Admin user can access and modify grades for any course
  - [ ] Faculty user assigned to a course can manage grades for that course
  - [ ] Faculty user NOT assigned to a course receives 403 Forbidden
- **Test Strategy:**
  - New test: ReadOnly-role user GET `/Courses/Grades/{id}` returns 403
  - New test: Admin-role user GET `/Courses/Grades/{id}` returns 200
  - Existing grade management tests: verify still pass for authorized users
  - Regression: all 65 existing tests pass
- **Behavioral Deltas:**
  - New: Grade management requires Admin or Faculty role
  - New: Faculty users can only grade courses they are assigned to
  - Regression: Admin grade management workflow unchanged
- **Dependencies:** none
- **Rollback Plan:** Remove `[Authorize(Roles)]` and ownership check
- **Risk:** Medium — Requires querying CourseAssignment for ownership. Could break existing grade flow if user claims not properly configured.

---

## sec-004: Add CSRF Protection to MarkAsRead Endpoint

- **Type:** security
- **Tier:** 2 (High)
- **Vulnerability:** `MarkAsRead()` POST endpoint lacks `[ValidateAntiForgeryToken]` attribute (finding S-04)
- **OWASP:** A03:2021 — Injection (CSRF)
- **Scope:**
  - `Controllers/NotificationsController.cs` — Add `[ValidateAntiForgeryToken]` to `MarkAsRead()` action
  - `wwwroot/Scripts/notifications.js` — Include anti-forgery token in AJAX POST requests via `X-RequestVerificationToken` header
  - `Views/Shared/_Layout.cshtml` — Add hidden anti-forgery token field for JavaScript access
  - No other changes.
- **Acceptance Criteria:**
  - [ ] POST to `/Notifications/MarkAsRead` without anti-forgery token returns 400
  - [ ] POST with valid anti-forgery token succeeds (200)
  - [ ] JavaScript notifications client includes token in MarkAsRead requests
- **Test Strategy:**
  - New test: POST without anti-forgery token returns 400
  - Existing MarkAsRead integration: verify still works with token
  - Regression: all 65 existing tests pass
- **Behavioral Deltas:**
  - Modified: MarkAsRead requires anti-forgery token
  - Regression: Notification read status flow unchanged for UI users
- **Dependencies:** none
- **Rollback Plan:** Remove `[ValidateAntiForgeryToken]` from MarkAsRead
- **Risk:** Low — Standard ASP.NET Core anti-forgery pattern.

---

## sec-005: Strengthen Password Policy

- **Type:** security
- **Tier:** 2 (High)
- **Vulnerability:** Minimum 6-character password with no special characters required (finding S-03)
- **OWASP:** A07:2021 — Identification and Authentication Failures
- **Scope:**
  - `Program.cs` — Update Identity password options: `RequiredLength=12`, `RequireNonAlphanumeric=true`, `RequireUppercase=true`, `RequireLowercase=true`, `RequiredUniqueChars=4`
  - `Data/IdentitySeeder.cs` — Update seeded admin password to meet new policy
  - No other changes.
- **Acceptance Criteria:**
  - [ ] Registration with password shorter than 12 characters fails validation
  - [ ] Registration with password lacking special characters fails validation
  - [ ] Registration with a strong password (12+ chars, mixed case, special char) succeeds
  - [ ] Seeded admin user password meets the new policy
- **Test Strategy:**
  - Manual: attempt registration with weak password → verify rejection
  - Regression: all 65 existing tests pass (TestAuthHandler bypasses password)
- **Behavioral Deltas:**
  - Modified: Password requirements increased from 6 to 12 chars with complexity rules
  - Regression: Existing authenticated users unaffected (already logged in)
- **Dependencies:** none
- **Rollback Plan:** Revert password options in Program.cs
- **Risk:** Low — Only affects new registrations and password changes.

---

## sec-006: Fix DOM-Based XSS in Notifications JavaScript

- **Type:** security
- **Tier:** 2 (High)
- **Vulnerability:** `innerHTML` used to render notification data from JSON, enabling script injection via malicious entity names (finding S-07)
- **OWASP:** A03:2021 — Injection (XSS)
- **Scope:**
  - `wwwroot/Scripts/notifications.js` — Replace all `innerHTML` assignments with `textContent` for text content and `createElement`/`appendChild` for DOM structure.
  - No other changes.
- **Acceptance Criteria:**
  - [ ] Notification message containing `<script>alert(1)</script>` renders as escaped text, not executed script
  - [ ] Notification message containing `<img src=x onerror=...>` renders as text, not an image tag
  - [ ] Normal notification messages display correctly (no visual regression)
- **Test Strategy:**
  - Manual: create entity with HTML characters in name → verify notification renders safely
  - Visual regression: verify notification toast appearance unchanged for normal text
  - Regression: all 65 existing tests pass
- **Behavioral Deltas:**
  - Modified: Notification rendering uses safe DOM APIs
  - Regression: Notification appearance unchanged for normal text content
- **Dependencies:** none
- **Rollback Plan:** Revert notifications.js to innerHTML version
- **Risk:** Low — Isolated change to one JavaScript file. Output-only change.

---

## Tier 3 — Medium (Hardening)

---

## sec-007: Fix Information Disclosure and Code Patterns

- **Type:** security
- **Tier:** 3 (Medium)
- **Vulnerability:** Exception details in user-facing errors (S-09), @Html.Raw pattern (S-08), class-level AllowAnonymous scope (S-11)
- **OWASP:** A04 (Insecure Design), A03 (Injection pattern), A01 (Access Control)
- **Scope:**
  - `Controllers/CoursesController.cs` — Replace `ex.Message` in ModelState errors with generic "An error occurred" message; log full exception via `_logger.LogError()`
  - 4 view files (`Views/Students/Index.cshtml`, `Views/Courses/Index.cshtml`, `Views/Departments/Index.cshtml`, `Views/Instructors/Index.cshtml`) — Replace `@Html.Raw(" | ")` with `<span class="separator"> | </span>`
  - `Controllers/HomeController.cs` — Move `[AllowAnonymous]` from class to individual actions (Index, Contact, Error); add `[Authorize]` on About (requires auth to view student data)
  - No other changes.
- **Acceptance Criteria:**
  - [ ] File upload error shows generic message (not exception details)
  - [ ] Full exception logged to ILogger
  - [ ] No `@Html.Raw()` calls remain in any view file
  - [ ] `/Home/About` requires authentication
  - [ ] `/Home/Index` and `/Home/Contact` remain publicly accessible
- **Test Strategy:**
  - Code review: `grep -r "Html.Raw" Views/` returns zero results
  - Code review: `grep -r "ex.Message" Controllers/` in ModelState context returns zero
  - New test: anonymous GET `/Home/About` returns redirect
  - Regression: all 65 existing tests pass (update About test to use auth)
- **Behavioral Deltas:**
  - Modified: `/Home/About` now requires authentication
  - Modified: File upload error messages are generic
  - Regression: All other page behavior unchanged
- **Dependencies:** none
- **Rollback Plan:** Revert individual file changes from git
- **Risk:** Low — Three isolated changes with no shared state.

---

## sec-008: Add Security Headers and Cookie Configuration

- **Type:** security
- **Tier:** 3 (Medium)
- **Vulnerability:** Missing Content-Security-Policy, Permissions-Policy headers (S-10); no explicit cookie security settings (S-12)
- **OWASP:** A05:2021 — Security Misconfiguration
- **Scope:**
  - `Program.cs` — Add CSP header (`default-src 'self'; script-src 'self' cdnjs.cloudflare.com; style-src 'self' 'unsafe-inline'; img-src 'self' data:; connect-src 'self' ws: wss:`), Permissions-Policy (`camera=(), microphone=(), geolocation=()`), X-Permitted-Cross-Domain-Policies (`none`) to security headers middleware
  - `Program.cs` — Add `ConfigureApplicationCookie()` with `HttpOnly=true`, `SecurePolicy=Always`, `SameSite=Lax`, `ExpireTimeSpan=30min`, `SlidingExpiration=true`
  - No other changes.
- **Acceptance Criteria:**
  - [ ] HTTP response includes `Content-Security-Policy` header
  - [ ] HTTP response includes `Permissions-Policy` header
  - [ ] Auth cookie has HttpOnly, Secure, SameSite attributes
  - [ ] Session expires after 30 minutes of inactivity
- **Test Strategy:**
  - Integration test: verify response headers contain CSP and Permissions-Policy
  - Manual: inspect cookie attributes in browser dev tools
  - Regression: all 65 existing tests pass
- **Behavioral Deltas:**
  - New: CSP, Permissions-Policy, X-Permitted-Cross-Domain-Policies headers in all responses
  - Modified: Auth cookie attributes stricter (HttpOnly, Secure, SameSite)
  - Regression: All page functionality unchanged
- **Dependencies:** none
- **Rollback Plan:** Remove added headers and cookie config from Program.cs
- **Risk:** Low — CSP may block inline scripts if any exist. `style-src 'unsafe-inline'` accommodates Bootstrap inline styles. SignalR requires `connect-src 'self' ws: wss:`.

---

## sec-009: Validate File Upload Content (Magic Bytes)

- **Type:** security
- **Tier:** 3 (Medium)
- **Vulnerability:** File upload validates extension only, not MIME type or file content signature (finding S-13)
- **OWASP:** A04:2021 — Insecure Design
- **Scope:**
  - `Controllers/CoursesController.cs` — Add MIME type validation (`ContentType` in allowed list) and magic byte signature check for JPEG (FF D8 FF), PNG (89 50 4E 47), GIF (47 49 46 38), BMP (42 4D). Apply to both Create and Edit POST actions.
  - No other changes.
- **Acceptance Criteria:**
  - [ ] Valid JPEG file with `.jpg` extension → accepted
  - [ ] Renamed `.exe` file with `.jpg` extension → rejected ("Invalid file content")
  - [ ] Valid PNG file with `.png` extension → accepted
  - [ ] File with valid image content but wrong extension → rejected
- **Test Strategy:**
  - Unit test: valid image bytes pass signature check
  - Unit test: invalid bytes (e.g., MZ header for .exe) fail signature check
  - Regression: all 65 existing tests pass
- **Behavioral Deltas:**
  - Modified: File upload now validates MIME type and content signature
  - Regression: Valid image uploads work identically
- **Dependencies:** none
- **Rollback Plan:** Revert CoursesController file validation to extension-only
- **Risk:** Low — Additive validation. Only rejects files that shouldn't be accepted.

---

## Tier 4 — Low (Defense-in-Depth)

---

## sec-010: Add Rate Limiting, Input Validation, and AJAX Anti-Forgery

- **Type:** security
- **Tier:** 4 (Low)
- **Vulnerability:** No rate limiting (S-16), no search input length validation (S-15), no AJAX anti-forgery pattern (S-14)
- **OWASP:** A05 (Misconfiguration), A03 (Injection prevention)
- **Scope:**
  - `Program.cs` — Add `AddRateLimiter()` with fixed-window policy (100 requests/min per IP) and `app.UseRateLimiter()` in pipeline
  - `Controllers/StudentsController.cs` — Add `searchString` length truncation (max 100 chars)
  - `Views/Shared/_Layout.cshtml` — Add hidden anti-forgery token element for JavaScript access
  - `wwwroot/Scripts/notifications.js` — Read anti-forgery token from DOM and include in `X-RequestVerificationToken` header on any POST requests
  - No other changes.
- **Acceptance Criteria:**
  - [ ] More than 100 requests/min from same IP returns 429 Too Many Requests
  - [ ] Search string longer than 100 characters is truncated
  - [ ] AJAX POST requests include anti-forgery token header
- **Test Strategy:**
  - Manual: rapid-fire requests to verify rate limiter activates
  - Unit test: search truncation at 100 characters
  - Regression: all 65 existing tests pass
- **Behavioral Deltas:**
  - New: Rate limiting returns 429 on excessive requests
  - New: Search strings capped at 100 characters
  - Regression: Normal usage completely unaffected
- **Dependencies:** none
- **Rollback Plan:** Remove rate limiter registration; revert search truncation
- **Risk:** Low — All additive protections. No existing behavior changed for normal users.

---

## Priority Order Summary — Security Increments

| Order | Increment | Tier | Findings | Risk |
|:-----:|-----------|:----:|----------|:----:|
| 1 | **sec-001** | 1 | S-01, S-02 (hardcoded secrets) | Low |
| 2 | **sec-002** | 1 | S-05 (anonymous notification access) | Low |
| 3 | **sec-003** | 2 | S-06 (grade IDOR) | Medium |
| 4 | **sec-004** | 2 | S-04 (missing CSRF) | Low |
| 5 | **sec-005** | 2 | S-03 (weak password policy) | Low |
| 6 | **sec-006** | 2 | S-07 (DOM XSS) | Low |
| 7 | **sec-007** | 3 | S-08, S-09, S-11 (code hardening) | Low |
| 8 | **sec-008** | 3 | S-10, S-12 (config hardening) | Low |
| 9 | **sec-009** | 3 | S-13 (file content validation) | Low |
| 10 | **sec-010** | 4 | S-14, S-15, S-16 (defense-in-depth) | Low |

## Security Finding Coverage Matrix

| Finding | Severity | Tier | Increment |
|---------|----------|:----:|-----------|
| S-01 | Critical | 1 | sec-001 |
| S-02 | Critical | 1 | sec-001 |
| S-03 | High | 2 | sec-005 |
| S-04 | High | 2 | sec-004 |
| S-05 | High | 1 | sec-002 |
| S-06 | High | 2 | sec-003 |
| S-07 | Medium | 2 | sec-006 |
| S-08 | Medium | 3 | sec-007 |
| S-09 | Medium | 3 | sec-007 |
| S-10 | Medium | 3 | sec-008 |
| S-11 | Medium | 3 | sec-007 |
| S-12 | Medium | 3 | sec-008 |
| S-13 | Medium | 3 | sec-009 |
| S-14 | Low | 4 | sec-010 |
| S-15 | Low | 4 | sec-010 |
| S-16 | Low | 4 | sec-010 |
