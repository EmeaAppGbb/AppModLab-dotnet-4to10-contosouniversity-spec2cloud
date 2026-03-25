# FRD: Instructor Management

**Feature ID**: F-003
**Status**: Draft
**Priority**: P0
**Last Updated**: 2026-03-25

## Description

Instructor Management provides full CRUD operations for instructor records along with two relationship management capabilities: one-to-one office location assignments and many-to-many course assignments. The index view features a distinctive master-detail-detail drill-down — selecting an instructor shows their courses, and selecting a course shows enrolled students with grades. The instructor create and edit forms include a checkbox matrix for assigning/unassigning courses. When an instructor who serves as a department administrator is deleted, the department's administrator reference is cleared. This is the most complex entity management feature in the application due to its multi-level data relationships and interactive UI.

## User Stories

### US-F003-001: View Instructor List with Drill-Down

**As a** University Administrator
**I want to** view all instructors and drill down to see their courses and enrolled students
**So that** I can understand instructor workload and course enrollment.

**Acceptance Criteria:**
- GIVEN instructors exist WHEN I navigate to the Instructors index THEN I see a table with Last Name, First Name, Hire Date, Office Location, assigned Courses, and action links
- GIVEN I click "Select" on an instructor WHEN the page refreshes THEN the selected instructor's row is highlighted and a second table shows their assigned courses
- GIVEN a course table is shown WHEN I click "Select" on a course THEN a third table shows students enrolled in that course with their names and grades
- GIVEN an instructor has no office assignment WHEN I view their row THEN the Office column is empty
- GIVEN an instructor has no course assignments WHEN I view their row THEN the Courses column is empty

### US-F003-002: Create an Instructor with Course Assignments

**As a** University Administrator
**I want to** create a new instructor and assign them to courses in the same operation
**So that** instructor records are complete upon creation.

**Acceptance Criteria:**
- GIVEN I am on the Create Instructor page WHEN I see the form THEN I see fields for Last Name, First Name, Hire Date, Office Location, and a checkbox matrix of all available courses
- GIVEN I fill in valid data and check some courses WHEN I submit the form THEN an instructor is created with the selected course assignments and office assignment
- GIVEN I do not check any courses WHEN I submit the form THEN the instructor is created without course assignments
- GIVEN an instructor is successfully created WHEN the operation completes THEN a CREATE notification is sent

### US-F003-003: Edit Instructor with Course Assignment Changes

**As a** University Administrator
**I want to** edit an instructor's information and modify their course assignments
**So that** instructor records and teaching assignments stay current.

**Acceptance Criteria:**
- GIVEN an instructor exists WHEN I navigate to their edit page THEN the form shows current values and the checkbox matrix reflects currently assigned courses (checked)
- GIVEN I check a new course WHEN I submit the form THEN a new CourseAssignment record is created
- GIVEN I uncheck a currently assigned course WHEN I submit the form THEN the CourseAssignment record is removed
- GIVEN I clear the office location field WHEN I submit the form THEN the OfficeAssignment is set to null
- GIVEN an instructor is successfully updated WHEN the operation completes THEN an UPDATE notification is sent

### US-F003-004: View Instructor Details

**As a** University Administrator
**I want to** view an instructor's basic details
**So that** I can review their information.

**Acceptance Criteria:**
- GIVEN an instructor exists WHEN I navigate to their details page THEN I see their Last Name, First Name, and Hire Date

### US-F003-005: Delete an Instructor

**As a** University Administrator
**I want to** delete an instructor record
**So that** departed instructors are removed from the system.

**Acceptance Criteria:**
- GIVEN an instructor exists WHEN I navigate to the delete page THEN I see their details as confirmation
- GIVEN the instructor is a department administrator WHEN I confirm deletion THEN the department's InstructorID is set to null before the instructor is deleted
- GIVEN I confirm deletion WHEN I click Delete THEN the instructor and their course/office assignments are removed
- GIVEN an instructor is successfully deleted WHEN the operation completes THEN a DELETE notification is sent

## Functional Requirements

### FR-F003-001: Instructor Index with Multi-Level Eager Loading

The system SHALL display all instructors with their office assignments and course assignments eagerly loaded. When an instructor is selected (via `instructorID` query parameter), their courses are displayed. When a course is selected (via `courseID` query parameter), enrolled students with grades are displayed.

- Input: Optional `instructorID` and `courseID` query parameters
- Processing: Query instructors with `Include(OfficeAssignment).Include(CourseAssignments).ThenInclude(Course)`. If instructor selected, query their courses. If course selected, query enrollments with students.
- Output: Up to three tables: instructors, courses (if selected), enrollments (if selected)
- Error handling: `.Single()` used for lookups — throws if not found

### FR-F003-002: Course Assignment Checkbox Matrix

The system SHALL display all available courses as a checkbox matrix (3 columns) on Create and Edit forms. Checked checkboxes represent assigned courses. The `PopulateAssignedCourseData()` helper builds `AssignedCourseData` view models comparing all courses against the instructor's current assignments.

- Input: All courses from database, instructor's current CourseAssignments
- Processing: Build list of `AssignedCourseData` (CourseID, Title, Assigned boolean)
- Output: Checkbox matrix in ViewBag.Courses
- Error handling: None — assumes courses table is populated

### FR-F003-003: Update Course Assignments

The system SHALL add and remove course assignments based on checkbox state changes. Uses `HashSet<int>` comparison between selected course IDs and current assignments. New selections create `CourseAssignment` records; unchecked items remove them.

- Input: Array of selected `courseID` values from form
- Processing: Compare HashSet of selected IDs vs current IDs. Add new, remove unchecked.
- Output: Updated CourseAssignment records
- Error handling: Null selectedCourses treated as empty array

### FR-F003-004: Office Assignment Management

The system SHALL create, update, or remove an instructor's office assignment based on the Location field. If the location is set, an OfficeAssignment is created/updated. If cleared (empty string), the OfficeAssignment is set to null.

- Input: `OfficeAssignment.Location` from form
- Processing: Check if empty → set to null; otherwise create/update
- Output: OfficeAssignment created, updated, or removed
- Error handling: None specific

### FR-F003-005: Department Cleanup on Delete

The system SHALL clear the `InstructorID` on any department where the deleted instructor is the administrator, preventing orphaned foreign key references.

- Input: Instructor ID being deleted
- Processing: Query departments where `InstructorID` matches, set to null, save
- Output: Department records updated
- Error handling: Database save within try-catch

## Non-Functional Requirements

### NFR-F003-001: Eager Loading Performance

The index view uses multi-level `Include()` and `ThenInclude()` to load instructors → office assignments → course assignments → courses. For large datasets, this could produce significant SQL joins.

### NFR-F003-002: CSRF Protection

All POST actions protected by `ValidateAntiForgeryToken`.

## Dependencies

| Dependency | Type | Direction | Description |
|------------|------|-----------|-------------|
| Course (F-002) | Feature | Upstream | Course assignment requires existing courses |
| Department (F-004) | Feature | Bidirectional | Instructor can be department admin; delete clears department FK |
| SchoolContext (EF Core) | Infrastructure | Upstream | Data access |
| NotificationService (F-005) | Feature | Downstream | CRUD notifications |
| BaseController | Shared infrastructure | Upstream | DbContext and NotificationService |

---

## Current Implementation (Brownfield Extension)

### Files Involved

| File Path | Role | Notes |
|-----------|------|-------|
| `Controllers/InstructorsController.cs` | Route handlers with complex course assignment logic | ~250 lines, most complex controller |
| `Models/Instructor.cs` | Entity model (inherits Person) | HireDate, nav to CourseAssignments, OfficeAssignment |
| `Models/CourseAssignment.cs` | Join entity | Composite key (CourseID, InstructorID) |
| `Models/OfficeAssignment.cs` | One-to-one entity | InstructorID as PK and FK |
| `Models/SchoolViewModels/InstructorIndexData.cs` | View model for Index | Holds Instructors, Courses, Enrollments collections |
| `Models/SchoolViewModels/AssignedCourseData.cs` | Checkbox view model | CourseID, Title, Assigned flag |
| `Views/Instructors/Index.cshtml` | Master-detail-detail view | Three-table drill-down with row highlighting |
| `Views/Instructors/Create.cshtml` | Create form with checkbox matrix | 3-column course assignment layout |
| `Views/Instructors/Edit.cshtml` | Edit form with checkbox matrix | Pre-populated from current assignments |
| `Views/Instructors/Details.cshtml` | Basic detail view | Shows name and hire date only |
| `Views/Instructors/Delete.cshtml` | Delete confirmation | Standard pattern |

### Architecture Pattern

MVC controller with direct DbContext access. Course assignment logic (HashSet comparison, add/remove) is inline in the controller. `PopulateAssignedCourseData()` helper method builds view model but is a private method on the controller, not a reusable service.

### Test Coverage

| Test Type | Files | Assertions | Coverage |
|-----------|-------|------------|----------|
| Unit | — | 0 | 0% |
| Integration | — | 0 | 0% |
| E2E | — | 0 | 0% |

**Untested paths**: Course assignment add/remove logic, concurrent edits to same instructor, department cleanup on delete, instructor with 0 vs many courses, null office location handling.

### Known Limitations

- `.Single()` assumptions in Index — will throw `InvalidOperationException` if instructor/course not found or if duplicates exist
- `TryUpdateModel()` in Edit — legacy pattern, bypasses normal model binding
- Details view is incomplete — does not show office location or course assignments (only shows name and hire date)
- N+1 query risk: index loads all instructors with all courses with all enrollments via eager loading
- No pagination on instructor list
- `UpdateInstructorCourses()` does not check for duplicate course assignments

### Integration Points

| External System | Protocol | Purpose | Config Location |
|----------------|----------|---------|-----------------|
| SQL Server LocalDB | TCP/SQL via EF Core 3.1 | Instructor, CourseAssignment, OfficeAssignment data | `Web.config` |
| MSMQ | System.Messaging | CRUD notifications | `Web.config` appSettings |
