# FRD: Enrollment Statistics

**Feature ID**: F-006
**Status**: Draft
**Priority**: P1
**Last Updated**: 2026-03-25

## Description

Enrollment Statistics provides a read-only "About" page that displays student body statistics — a table showing how many students enrolled on each enrollment date. The data is computed via a LINQ group-by aggregation query against the Student entity, grouping by `EnrollmentDate` and counting students per group. This is the only analytics/reporting feature in the application and provides a simple demographic overview of the student body.

## User Stories

### US-F006-001: View Enrollment Statistics

**As a** University Administrator
**I want to** see how many students enrolled on each date
**So that** I can understand enrollment trends over time.

**Acceptance Criteria:**
- GIVEN students exist in the database WHEN I navigate to the About page THEN I see a table with Enrollment Date and Student Count columns
- GIVEN students enrolled on the same date WHEN I view the table THEN they are aggregated into a single row with the total count
- GIVEN no students exist WHEN I view the About page THEN the table is empty

## Functional Requirements

### FR-F006-001: Enrollment Date Aggregation

The system SHALL group all students by their `EnrollmentDate` and count the number of students in each group. Results are returned as a list of `EnrollmentDateGroup` view models.

- Input: None (reads entire Student table)
- Processing: LINQ query: `group student by student.EnrollmentDate into dateGroup select new EnrollmentDateGroup { EnrollmentDate, StudentCount }`
- Output: List of `EnrollmentDateGroup` (EnrollmentDate, StudentCount) passed to view
- Error handling: None — empty result set renders empty table

## Non-Functional Requirements

### NFR-F006-001: Database-Level Aggregation

The group-by query is translated to SQL by EF Core, executing the aggregation at the database level rather than loading all student records into memory. `.ToList()` materializes only the aggregated results.

## Dependencies

| Dependency | Type | Direction | Description |
|------------|------|-----------|-------------|
| Student entity (F-001) | Data model | Upstream | Queries Student.EnrollmentDate |
| SchoolContext (EF Core) | Infrastructure | Upstream | Data access |
| BaseController | Shared infrastructure | Upstream | Provides DbContext |

---

## Current Implementation (Brownfield Extension)

### Files Involved

| File Path | Role | Notes |
|-----------|------|-------|
| `Controllers/HomeController.cs` | About action with LINQ query | ~5 lines of logic |
| `Models/SchoolViewModels/EnrollmentDateGroup.cs` | View model | EnrollmentDate (DateTime?), StudentCount (int) |
| `Views/Home/About.cshtml` | Statistics table view | Two-column table |

### Architecture Pattern

Inline LINQ query in controller action. No service layer, no caching. Query runs on every page load.

### Test Coverage

| Test Type | Files | Assertions | Coverage |
|-----------|-------|------------|----------|
| Unit | — | 0 | 0% |
| Integration | — | 0 | 0% |
| E2E | — | 0 | 0% |

**Untested paths**: Empty database, large dataset performance, null EnrollmentDate handling.

### Known Limitations

- No caching — aggregation query runs on every page load
- `EnrollmentDate` in view model is nullable (`DateTime?`) but Student model requires it — potential display issue
- No sorting on the statistics table
- No date range filtering — always shows all-time statistics
- Query could be expensive with very large student populations

### Integration Points

| External System | Protocol | Purpose | Config Location |
|----------------|----------|---------|-----------------|
| SQL Server LocalDB | TCP/SQL via EF Core 3.1 | Student enrollment data | `Web.config` |
