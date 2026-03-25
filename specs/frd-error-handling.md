# FRD: Error Handling

**Feature ID**: F-008
**Status**: Draft
**Priority**: P2
**Last Updated**: 2026-03-25

## Description

Error Handling provides global exception handling for the application via ASP.NET MVC's `HandleErrorAttribute` and a custom error view. When an unhandled exception occurs in any controller action, the error filter catches it and renders a generic error page. In debug mode, the error page displays exception details (message, controller name, action name). The application also provides an `Unauthorized` action that returns a simple message for access denied scenarios, though no authorization framework currently enforces access controls.

## User Stories

### US-F008-001: See Friendly Error Page

**As a** University Administrator
**I want to** see a user-friendly error page when something goes wrong
**So that** I know an error occurred without seeing raw stack traces.

**Acceptance Criteria:**
- GIVEN an unhandled exception occurs WHEN the error filter catches it THEN a generic error page is displayed with "An error occurred while processing your request."
- GIVEN debug mode is enabled WHEN an error occurs THEN the error page additionally shows the exception message, controller name, and action name
- GIVEN debug mode is disabled WHEN an error occurs THEN only the generic error message is shown

### US-F008-002: See Unauthorized Page

**As a** a user without proper access
**I want to** see an unauthorized message
**So that** I know I don't have permission.

**Acceptance Criteria:**
- GIVEN I access the Unauthorized endpoint WHEN the page loads THEN I see a message indicating I am not authorized

## Functional Requirements

### FR-F008-001: Global Exception Filter

The system SHALL register `HandleErrorAttribute` as a global filter, catching unhandled exceptions in all controller actions and rendering the `Error` view.

- Input: Any unhandled exception from a controller action
- Processing: MVC `HandleErrorAttribute` intercepts, creates `HandleErrorInfo` model
- Output: `Views/Shared/Error.cshtml` rendered with error details
- Error handling: This IS the error handling mechanism

### FR-F008-002: Error View with Debug Details

The system SHALL display exception details (Exception.Message, Controller, Action) only when `HttpContext.IsDebuggingEnabled` is true. In production, only a generic message is shown.

- Input: `HandleErrorInfo` model from exception filter
- Processing: Conditional rendering based on debug mode
- Output: HTML error page
- Error handling: N/A

### FR-F008-003: Unauthorized Endpoint

The system SHALL provide a `/Home/Unauthorized` endpoint returning a view with an unauthorized message. This endpoint exists as a placeholder for future authorization implementation.

- Input: HTTP GET `/Home/Unauthorized`
- Processing: Returns view
- Output: Unauthorized message page
- Error handling: N/A

## Non-Functional Requirements

### NFR-F008-001: Production Safety

Debug information (exception messages, controller/action names) MUST NOT be displayed in production. The conditional check on `HttpContext.IsDebuggingEnabled` controls this, but `Web.config` currently has `debug="true"` — a deployment risk.

## Dependencies

| Dependency | Type | Direction | Description |
|------------|------|-----------|-------------|
| ASP.NET MVC HandleErrorAttribute | Framework | Upstream | Built-in exception filter |
| All controllers | Cross-cutting | Upstream | Error handling applies to all actions |

---

## Current Implementation (Brownfield Extension)

### Files Involved

| File Path | Role | Notes |
|-----------|------|-------|
| `App_Start/FilterConfig.cs` | Registers global HandleErrorAttribute | 1 line of config |
| `Controllers/HomeController.cs` | Error and Unauthorized actions | Returns views |
| `Views/Shared/Error.cshtml` | Error display template | Conditional debug details |
| `Models/ErrorViewModel.cs` | Error view model | RequestId, ShowRequestId — not consistently used |

### Architecture Pattern

ASP.NET MVC global filter pipeline. `HandleErrorAttribute` is a framework-provided filter that catches exceptions and renders a view. No custom error handling middleware.

### Test Coverage

| Test Type | Files | Assertions | Coverage |
|-----------|-------|------------|----------|
| Unit | — | 0 | 0% |
| Integration | — | 0 | 0% |
| E2E | — | 0 | 0% |

**Untested paths**: Exception rendering in debug vs production mode, ErrorViewModel usage, specific exception types.

### Known Limitations

- `ErrorViewModel` exists but is not used by the Error action — `HomeController.Error()` returns `View()` without a model
- `Web.config` has `debug="true"` — exception details would leak in a deployed environment
- No logging of unhandled exceptions to persistent storage (only in-memory debug/trace)
- Unauthorized action has no actual authorization checks — it's a dead endpoint until auth is implemented
- No custom error pages for specific HTTP status codes (404, 403, 500)
- `customErrors` not configured in `Web.config` — default ASP.NET error pages shown for non-MVC errors

### Integration Points

None — pure framework infrastructure.
