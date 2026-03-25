# FRD: Student Management

**Feature ID**: F-001
**Status**: Draft
**Priority**: P0
**Last Updated**: 2026-03-25

## Description

Student Management is the core record-keeping feature of Contoso University. It provides full CRUD (Create, Read, Update, Delete) operations for student records, along with search by name, multi-column sorting, and server-side pagination. Each student has a last name, first middle name, and enrollment date. The student detail view shows related course enrollments with grades, providing a complete academic snapshot. This feature serves as the primary data entry point for the university's student body and is the most feature-rich entity management area in the application, with search, sort, and pagination capabilities not found in other entity controllers.

## User Stories

### US-F001-001: View Student List

**As a** University Administrator
**I want to** view a paginated list of all students
**So that** I can browse the student body efficiently without loading all records at once.

**Acceptance Criteria:**
- GIVEN the student database has records WHEN I navigate to the Students index page THEN I see a table of students with Last Name, First Name, and Enrollment Date columns
- GIVEN more than 10 students exist WHEN I view the student list THEN only 10 students are shown per page with Previous/Next pagination controls
- GIVEN I am on the first page WHEN I view pagination controls THEN the Previous button is disabled
- GIVEN I am on the last page WHEN I view pagination controls THEN the Next button is disabled

### US-F001-002: Search Students by Name

**As a** University Administrator
**I want to** search for students by first or last name
**So that** I can quickly find specific student records.

**Acceptance Criteria:**
- GIVEN students exist in the database WHEN I enter a search term and submit THEN only students whose last name or first name contains the search term are displayed
- GIVEN a search is active WHEN I click "Back to Full List" THEN the filter is cleared and all students are shown
- GIVEN a search term is active WHEN I paginate THEN the search filter is preserved across pages

### US-F001-003: Sort Student List

**As a** University Administrator
**I want to** sort the student list by different columns
**So that** I can organize students by name or enrollment date.

**Acceptance Criteria:**
- GIVEN the student list is displayed WHEN I click the "Last Name" column header THEN students are sorted by last name ascending
- GIVEN students are sorted ascending by last name WHEN I click "Last Name" again THEN the sort order toggles to descending
- GIVEN the student list is displayed WHEN I click the "Enrollment Date" column header THEN students are sorted by enrollment date
- GIVEN a sort order is active WHEN I search THEN the sort order is preserved
- GIVEN a sort order is active WHEN I paginate THEN the sort order is preserved

### US-F001-004: Create a New Student

**As a** University Administrator
**I want to** create a new student record with name and enrollment date
**So that** new students are registered in the system.

**Acceptance Criteria:**
- GIVEN I am on the Create Student page WHEN I submit valid student data (last name, first name, enrollment date) THEN a new student record is created and I am redirected to the student list
- GIVEN I submit the form WHEN the enrollment date is before year 1753 or after year 9999 THEN a validation error is displayed
- GIVEN I submit the form WHEN required fields are missing THEN client-side validation errors are shown
- GIVEN a student is successfully created WHEN the operation completes THEN a CREATE notification is sent to the notification system

### US-F001-005: View Student Details

**As a** University Administrator
**I want to** view a student's full details including their course enrollments
**So that** I can see the student's complete academic record.

**Acceptance Criteria:**
- GIVEN a student exists WHEN I navigate to their details page THEN I see their last name, first name, and enrollment date
- GIVEN a student has enrollments WHEN I view their details THEN I see a table of enrolled courses with course title and grade
- GIVEN a student has no enrollments WHEN I view their details THEN I see "No enrollments found."

### US-F001-006: Edit a Student

**As a** University Administrator
**I want to** edit an existing student's information
**So that** I can correct or update student records.

**Acceptance Criteria:**
- GIVEN a student exists WHEN I navigate to their edit page THEN the form is pre-populated with current values
- GIVEN I change valid student data WHEN I submit the form THEN the student record is updated and I am redirected to the student list
- GIVEN I submit invalid data WHEN validation fails THEN the form is redisplayed with error messages
- GIVEN a student is successfully updated WHEN the operation completes THEN an UPDATE notification is sent

### US-F001-007: Delete a Student

**As a** University Administrator
**I want to** delete a student record
**So that** I can remove students who are no longer enrolled.

**Acceptance Criteria:**
- GIVEN a student exists WHEN I navigate to the delete page THEN I see the student's details as a confirmation
- GIVEN I confirm deletion WHEN I click Delete THEN the student record is permanently removed and I am redirected to the student list
- GIVEN a student is successfully deleted WHEN the operation completes THEN a DELETE notification is sent

## Functional Requirements

### FR-F001-001: Student List with Pagination

The system SHALL display students in a paginated table with 10 records per page. Pagination is performed server-side using `IQueryable.Skip()` and `Take()` to minimize data transfer. The page displays Last Name, First Name, Enrollment Date columns, and action links (Details, Edit, Delete) for each student.

- Input: Optional page number (default: 1)
- Processing: Query students, apply sort/search if active, paginate via `PaginatedList<Student>.Create()`
- Output: Paginated table with navigation controls
- Error handling: Invalid page numbers default to page 1

### FR-F001-002: Name Search

The system SHALL filter students by a search string matching against `LastName` or `FirstMidName` using case-insensitive `Contains()`. The search string is passed via query parameter and preserved across pagination and sorting via ViewBag.

- Input: `searchString` query parameter (optional)
- Processing: Apply `.Where()` filter on `LastName` or `FirstMidName` containing the search string
- Output: Filtered student list (still paginated and sortable)
- Error handling: Empty search string returns all students

### FR-F001-003: Multi-Column Sorting

The system SHALL support sorting by Last Name (ascending/descending), First Name (descending), and Enrollment Date (ascending/descending). Sort order defaults to ascending by Last Name. Column headers toggle between ascending and descending. Current sort order is preserved across search and pagination via ViewBag parameters.

- Input: `sortOrder` query parameter (values: `name_desc`, `Date`, `date_desc`, or empty for name ascending)
- Processing: Apply `OrderBy`/`OrderByDescending` on the corresponding property
- Output: Sorted student list
- Error handling: Unknown sort values default to Last Name ascending

### FR-F001-004: Create Student

The system SHALL create a new student record with validated data. Server-side validation checks that EnrollmentDate is not the default/minimum DateTime value and falls within the range 1753–9999 (SQL Server `datetime` constraints). Client-side validation via jQuery Validate. CSRF protection via `ValidateAntiForgeryToken`.

- Input: `LastName`, `FirstMidName`, `EnrollmentDate` via form POST
- Processing: Validate date range, add to DbContext, `SaveChanges()`, send notification
- Output: Redirect to Index on success; redisplay form with errors on failure
- Error handling: Try-catch with `Trace.TraceError()` logging; returns to Index with generic error state

### FR-F001-005: Student Details with Enrollments

The system SHALL display a student's full information along with their course enrollments. Enrollments are eagerly loaded via `Include(s => s.Enrollments).ThenInclude(e => e.Course)`.

- Input: Student `id` route parameter
- Processing: Query student by ID with enrollment includes
- Output: Student detail view with enrollment table (Course Title, Grade)
- Error handling: Returns `HttpNotFound()` if student ID does not exist

### FR-F001-006: Edit Student

The system SHALL update an existing student record. Uses `TryUpdateModel()` to bind form values to the tracked entity. Same date validation as Create.

- Input: Student `id` and updated fields via form POST
- Processing: Fetch student by ID, apply `TryUpdateModel()`, validate dates, `SaveChanges()`, send notification
- Output: Redirect to Index on success; redisplay form with errors on failure
- Error handling: Try-catch with `Trace.TraceError()` logging

### FR-F001-007: Delete Student

The system SHALL permanently delete a student record after confirmation. Uses a two-step process: GET displays confirmation page, POST performs deletion.

- Input: Student `id` via route parameter (GET for confirmation, POST for execution)
- Processing: Find student by ID, remove from DbContext, `SaveChanges()`, send notification
- Output: Redirect to Index on success
- Error handling: Try-catch with `Trace.TraceError()` logging; redirects to Index with error flag

## Non-Functional Requirements

### NFR-F001-001: Pagination Performance

Student list pagination executes at the database level via `IQueryable` — only the requested page of records is fetched. No in-memory pagination of full result sets.

### NFR-F001-002: CSRF Protection

All state-changing operations (Create POST, Edit POST, Delete POST) are protected by `ValidateAntiForgeryToken` attribute and corresponding form tokens.

### NFR-F001-003: Client-Side Validation

Create and Edit forms include jQuery Validation and jQuery Validation Unobtrusive scripts for immediate client-side feedback before server round-trip.

## Dependencies

| Dependency | Type | Direction | Description |
|------------|------|-----------|-------------|
| SchoolContext (EF Core) | Infrastructure | Upstream | Data access for Student, Enrollment entities |
| PaginatedList<T> | Shared utility | Upstream | Generic pagination helper used by Index action |
| NotificationService (MSMQ) | Feature (F-005) | Downstream | Sends CREATE/UPDATE/DELETE notifications |
| BaseController | Shared infrastructure | Upstream | Provides DbContext and NotificationService |
| Enrollment entity | Data model | Related | Student details view displays enrollment data |
| Course entity | Data model | Related | Enrollments reference courses for display |

---

## Current Implementation (Brownfield Extension)

### Files Involved

| File Path | Role | Notes |
|-----------|------|-------|
| `Controllers/StudentsController.cs` | Route handlers for all CRUD + search/sort/pagination | Inherits BaseController |
| `Models/Student.cs` | Entity model (inherits Person) | EnrollmentDate, navigation to Enrollments |
| `Models/Person.cs` | Base entity (TPH inheritance) | ID, LastName, FirstMidName, FullName computed |
| `Models/Enrollment.cs` | Related entity | Grade enum, FK to Student and Course |
| `Views/Students/Index.cshtml` | List view with search, sort, pagination | Uses PaginatedList<Student> model |
| `Views/Students/Create.cshtml` | Create form | jQuery validation, anti-forgery token |
| `Views/Students/Edit.cshtml` | Edit form | jQuery validation, anti-forgery token |
| `Views/Students/Details.cshtml` | Detail view with enrollments table | Conditional "No enrollments found" |
| `Views/Students/Delete.cshtml` | Delete confirmation page | Shows student info before delete |
| `PaginatedList.cs` | Generic pagination helper | Inherits List<T> (anti-pattern) |

### Architecture Pattern

ASP.NET MVC 5 controller-per-entity pattern. Controller directly accesses EF Core DbContext (no repository layer, no service layer). Business logic (date validation, search filtering, sort logic) lives in the controller. ViewBag used for passing sort/filter state to views.

### Test Coverage

| Test Type | Files | Assertions | Coverage |
|-----------|-------|------------|----------|
| Unit | — | 0 | 0% |
| Integration | — | 0 | 0% |
| E2E | — | 0 | 0% |

**Untested paths**: All paths — search edge cases, pagination boundaries, date validation ranges, enrollment display with null grades, concurrent edit scenarios.

### Known Limitations

- Date validation logic (1753–9999 range check) is duplicated between Create and Edit actions
- `TryUpdateModel()` in Edit is a legacy MVC4 pattern — bypasses normal model binding
- `.Single()` used for enrollment lookup without null-safety — could throw if data is inconsistent
- Sort parameter values are magic strings (`"name_desc"`, `"Date"`, `"date_desc"`) — no enum or constants
- ViewBag used for type-unsafe parameter passing (CurrentSort, CurrentFilter, searchString)
- Page size (10) is hardcoded in controller — not configurable
- No async operations — all database calls are synchronous

### Integration Points

| External System | Protocol | Purpose | Config Location |
|----------------|----------|---------|-----------------|
| SQL Server LocalDB | TCP/SQL via EF Core 3.1 | Student and Enrollment data store | `Web.config` connectionStrings |
| MSMQ | System.Messaging | Send CRUD notifications | `Web.config` appSettings (queue path) |
