# FRD: Department Management

**Feature ID**: F-004
**Status**: Draft
**Priority**: P0
**Last Updated**: 2026-03-25

## Description

Department Management provides full CRUD operations for academic department records. Each department has a name, budget, start date, and an optional administrator (an instructor). The distinguishing feature of this module is its optimistic concurrency control implementation — the Department entity uses a SQL Server `RowVersion` timestamp column, and the Edit action catches `DbUpdateConcurrencyException` to present field-level conflict details to the user when concurrent edits conflict. This is the only entity in the application with concurrency handling, making it the most robust CRUD implementation from a data integrity perspective.

## User Stories

### US-F004-001: View Department List

**As a** University Administrator
**I want to** view all departments with their administrators
**So that** I can see the organizational structure.

**Acceptance Criteria:**
- GIVEN departments exist WHEN I navigate to the Departments index THEN I see a table with Name, Budget, Start Date, Administrator name, and action links
- GIVEN a department has an administrator assigned WHEN I view the list THEN the administrator's full name is displayed
- GIVEN a department has no administrator WHEN I view the list THEN the Administrator column is empty

### US-F004-002: Create a Department

**As a** University Administrator
**I want to** create a new department with budget and optional administrator
**So that** the university's organizational structure is expanded.

**Acceptance Criteria:**
- GIVEN I am on the Create Department page WHEN I see the form THEN I see fields for Name, Budget, Start Date, and an Administrator dropdown listing all instructors
- GIVEN I submit valid department data WHEN the form is posted THEN a new department is created
- GIVEN a department is successfully created WHEN the operation completes THEN a CREATE notification is sent

### US-F004-003: Edit a Department with Concurrency Handling

**As a** University Administrator
**I want to** edit department details with protection against concurrent modifications
**So that** changes are not silently overwritten by another user's edits.

**Acceptance Criteria:**
- GIVEN a department exists WHEN I navigate to the edit page THEN the form shows current values including a hidden RowVersion field
- GIVEN I submit valid changes WHEN no other user has modified the record THEN the department is updated successfully
- GIVEN I submit changes WHEN another user has modified the record since I loaded it THEN a concurrency conflict error is displayed
- GIVEN a concurrency conflict occurs WHEN the error is displayed THEN I see the current database values alongside my submitted values for each conflicting field
- GIVEN a concurrency conflict occurs WHEN I re-submit the form THEN my changes overwrite the database values (last-writer-wins with informed consent)
- GIVEN a department is successfully updated WHEN the operation completes THEN an UPDATE notification is sent

### US-F004-004: View Department Details

**As a** University Administrator
**I want to** view a department's full information
**So that** I can review department data.

**Acceptance Criteria:**
- GIVEN a department exists WHEN I navigate to its details page THEN I see Name, Budget, Start Date, and Administrator name

### US-F004-005: Delete a Department

**As a** University Administrator
**I want to** delete a department
**So that** discontinued departments are removed.

**Acceptance Criteria:**
- GIVEN a department exists WHEN I navigate to the delete page THEN I see department details as confirmation
- GIVEN I confirm deletion WHEN I click Delete THEN the department is permanently removed
- GIVEN a department is successfully deleted WHEN the operation completes THEN a DELETE notification is sent

## Functional Requirements

### FR-F004-001: Department List with Administrator

The system SHALL display all departments with their administrator's full name. Administrator is loaded via navigation property to Instructor entity.

- Input: None
- Processing: Query departments with Instructor include
- Output: Table with Name, Budget, Start Date, Administrator.FullName
- Error handling: Null administrator renders empty cell

### FR-F004-002: Create Department

The system SHALL create a department record with name, budget, start date, and optional administrator selection from a dropdown of all instructors.

- Input: `Name`, `Budget`, `StartDate`, `InstructorID` (nullable) via form POST
- Processing: Validate model, add to DbContext, `SaveChanges()`, send notification
- Output: Redirect to Index on success
- Error handling: ModelState validation errors redisplay form

### FR-F004-003: Optimistic Concurrency on Edit

The system SHALL detect concurrent modifications using the `RowVersion` column (SQL Server `timestamp` type). When `SaveChanges()` throws `DbUpdateConcurrencyException`, the system:
1. Reloads the current database values
2. Compares each field (Name, Budget, StartDate, InstructorID) between client-submitted and database-current values
3. Adds field-specific error messages for each conflict (e.g., "Current value: Engineering")
4. For InstructorID conflicts, resolves the instructor name for display
5. Sets the RowVersion to the database-current value so resubmission succeeds

- Input: Department data including hidden `RowVersion` field
- Processing: Fetch entity, apply updates, catch `DbUpdateConcurrencyException`, compare values
- Output: On conflict — redisplay form with field-level error messages. On success — redirect to Index
- Error handling: Concurrency exception caught and converted to user-friendly field messages

### FR-F004-004: Delete Department

The system SHALL permanently delete a department with two-step confirmation.

- Input: Department `id` (GET for confirmation, POST for execution)
- Processing: Find department, remove, `SaveChanges()`, send notification
- Output: Redirect to Index
- Error handling: Try-catch with error logging

## Non-Functional Requirements

### NFR-F004-001: Concurrency Safety

The `RowVersion` column provides optimistic concurrency control at the database level. SQL Server automatically updates the timestamp on every row modification, ensuring reliable conflict detection.

### NFR-F004-002: CSRF Protection

All POST actions protected by `ValidateAntiForgeryToken`.

## Dependencies

| Dependency | Type | Direction | Description |
|------------|------|-----------|-------------|
| Instructor (F-003) | Feature | Upstream | Administrator is an Instructor entity |
| Course (F-002) | Feature | Downstream | Courses belong to departments |
| SchoolContext (EF Core) | Infrastructure | Upstream | Data access |
| NotificationService (F-005) | Feature | Downstream | CRUD notifications |
| BaseController | Shared infrastructure | Upstream | DbContext and NotificationService |

---

## Current Implementation (Brownfield Extension)

### Files Involved

| File Path | Role | Notes |
|-----------|------|-------|
| `Controllers/DepartmentsController.cs` | Route handlers with concurrency logic | ~180 lines |
| `Models/Department.cs` | Entity model | Name, Budget, StartDate, InstructorID, RowVersion (byte[]) |
| `Views/Departments/Index.cshtml` | List view | Standard table with administrator name |
| `Views/Departments/Create.cshtml` | Create form | Instructor dropdown, currency formatting |
| `Views/Departments/Edit.cshtml` | Edit form | Hidden RowVersion field, concurrency error display |
| `Views/Departments/Details.cshtml` | Detail view | Shows all fields |
| `Views/Departments/Delete.cshtml` | Delete confirmation | Standard pattern |

### Architecture Pattern

MVC controller with direct DbContext access. Concurrency handling is inline in the Edit action method — not abstracted into a service or middleware. This is the only controller that handles `DbUpdateConcurrencyException`.

### Test Coverage

| Test Type | Files | Assertions | Coverage |
|-----------|-------|------------|----------|
| Unit | — | 0 | 0% |
| Integration | — | 0 | 0% |
| E2E | — | 0 | 0% |

**Untested paths**: Concurrency conflict detection and resolution, field-level comparison logic, RowVersion propagation, null administrator handling in conflict display, cascading behavior when department is deleted (orphaned courses).

### Known Limitations

- Concurrency handling only in Edit — not in Delete (concurrent delete scenario unhandled)
- Empty catch block in Edit (line 168) — silently ignores some exceptions
- `db.Instructors.Find(databaseValues.InstructorID)` could return null if instructor was deleted between conflict and display
- No validation for deleting a department that has associated courses (potential orphaned records)
- No cascade delete configuration visible — relies on EF Core defaults

### Integration Points

| External System | Protocol | Purpose | Config Location |
|----------------|----------|---------|-----------------|
| SQL Server LocalDB | TCP/SQL via EF Core 3.1 | Department data with RowVersion | `Web.config` |
| MSMQ | System.Messaging | CRUD notifications | `Web.config` appSettings |
