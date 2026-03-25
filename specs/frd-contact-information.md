# FRD: Contact Information

**Feature ID**: F-007
**Status**: Draft
**Priority**: P3
**Last Updated**: 2026-03-25

## Description

Contact Information is a static page displaying the Contoso University office address, phone number, and email contacts. It has no dynamic data, no database interaction, and no form submissions. It serves as a simple informational page in the site navigation.

## User Stories

### US-F007-001: View Contact Information

**As a** University Administrator
**I want to** view the university's contact details
**So that** I know how to reach the institution.

**Acceptance Criteria:**
- GIVEN I navigate to the Contact page WHEN the page loads THEN I see the office address (One Microsoft Way, Redmond, WA 98052-6399), phone number (425.555.0100), and email addresses for Support and Marketing

## Functional Requirements

### FR-F007-001: Display Static Contact Details

The system SHALL display hardcoded contact information: physical address, phone number, and two email addresses (Support, Marketing). No database query required.

- Input: None
- Processing: None — static view content
- Output: Rendered HTML with contact details
- Error handling: N/A

## Non-Functional Requirements

None specific to this feature.

## Dependencies

| Dependency | Type | Direction | Description |
|------------|------|-----------|-------------|
| BaseController | Shared infrastructure | Upstream | Inherits controller base class |

---

## Current Implementation (Brownfield Extension)

### Files Involved

| File Path | Role | Notes |
|-----------|------|-------|
| `Controllers/HomeController.cs` | Contact action | Returns `View()` — no logic |
| `Views/Home/Contact.cshtml` | Static HTML | Address, phone, emails hardcoded in view |

### Architecture Pattern

Pure static content. Controller action exists only to satisfy MVC routing conventions.

### Test Coverage

| Test Type | Files | Assertions | Coverage |
|-----------|-------|------------|----------|
| Unit | — | 0 | 0% |
| Integration | — | 0 | 0% |
| E2E | — | 0 | 0% |

### Known Limitations

- Contact details hardcoded in the Razor view — not configurable
- Email addresses are placeholder values

### Integration Points

None.
