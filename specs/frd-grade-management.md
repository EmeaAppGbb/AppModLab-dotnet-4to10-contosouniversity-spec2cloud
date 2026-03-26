# FRD: Grade Management

**Feature ID**: F-009
**Status**: Draft
**Priority**: P1
**Last Updated**: 2026-03-26

## Description

Grade Management enables instructors (and administrators) to enter, edit, and view grades for student enrollments in courses they teach. Currently, the `Enrollment` entity has a nullable `Grade` field (A/B/C/D/F) that is populated only via seed data — no UI exists for grade entry or modification. This feature adds a dedicated grade management interface accessible from the instructor's course view, allowing per-student grade assignment with inline editing and batch save. It also adds a read-only grade view for students on their enrollment details.

## User Stories

### US-F009-001: View Enrollments with Grades for a Course

**As an** Instructor or Administrator
**I want to** see all students enrolled in a course I teach, along with their current grades
**So that** I can review the class roster and identify students who need grading.

**Acceptance Criteria:**
- GIVEN I am an authenticated user WHEN I navigate to a course's grade management page THEN I see a table of all enrolled students with columns: Student Name, Enrollment Date, Current Grade
- GIVEN students have grades assigned WHEN I view the roster THEN existing grades (A/B/C/D/F) are displayed
- GIVEN a student has no grade assigned WHEN I view the roster THEN "No grade" is displayed
- GIVEN the course has no enrollments WHEN I view the grade page THEN a message "No students enrolled in this course" is shown

### US-F009-002: Assign or Update a Grade

**As an** Instructor or Administrator
**I want to** assign or change a grade for a student enrollment
**So that** student academic records are up to date.

**Acceptance Criteria:**
- GIVEN I am on the grade management page WHEN I see a student row THEN there is a dropdown with options: (blank/No grade), A, B, C, D, F
- GIVEN I select a grade from the dropdown WHEN I click Save THEN the enrollment's Grade field is updated in the database
- GIVEN I change a grade from B to A WHEN I click Save THEN the grade is updated and a success message is shown
- GIVEN I clear a grade (set to blank) WHEN I click Save THEN the grade is set to null (No grade)
- GIVEN a grade is saved WHEN the operation completes THEN a notification is sent (type: "Enrollment", operation: UPDATE)
- GIVEN I save a grade WHEN the audit interceptor fires THEN ModifiedAt and ModifiedBy are updated on the Enrollment record

### US-F009-003: Batch Update Grades

**As an** Instructor
**I want to** update multiple student grades at once and save them all in one operation
**So that** I can efficiently grade an entire class.

**Acceptance Criteria:**
- GIVEN I am on the grade management page WHEN I change grades for multiple students THEN all dropdowns retain my selections before I save
- GIVEN I have changed multiple grades WHEN I click "Save All Grades" THEN all modified enrollments are updated in a single database transaction
- GIVEN the batch save succeeds WHEN I see the page reload THEN a success message shows how many grades were updated

### US-F009-004: Access Grade Management from Instructor View

**As an** Instructor or Administrator
**I want to** access grade management from the existing instructor course drill-down
**So that** I can navigate naturally from my course list to grade entry.

**Acceptance Criteria:**
- GIVEN I am on the Instructors Index page WHEN I select an instructor and see their courses THEN each course row has a "Manage Grades" link
- GIVEN I click "Manage Grades" for a course WHEN the page loads THEN I see the grade management page for that specific course

### US-F009-005: View My Grades (Student Detail Enhancement)

**As an** Administrator viewing a student's details
**I want to** see the student's grades in context with course information
**So that** I can review their academic standing.

**Acceptance Criteria:**
- GIVEN I am on the Student Details page WHEN I view enrollments THEN grades are displayed alongside course titles (this already exists — no change needed, documented for completeness)

## Functional Requirements

### FR-F009-001: Grade Management Page

The system SHALL provide a grade management page at `/Courses/Grades/{courseId}` that displays all enrollments for a given course with editable grade dropdowns.

- Input: `courseId` route parameter
- Processing: Query enrollments for the course with Student includes, ordered by student last name
- Output: Table with Student Name, Enrollment Date, Grade dropdown (blank/A/B/C/D/F), per row
- Error handling: Return NotFound if course doesn't exist; display "No students enrolled" for empty enrollments

### FR-F009-002: Single Grade Update

The system SHALL update a single enrollment's grade via POST to `/Courses/UpdateGrade`.

- Input: `enrollmentId` (int), `grade` (Grade? enum — nullable)
- Processing: Find enrollment by ID, update Grade field, SaveChangesAsync, send notification
- Output: Redirect back to grade management page with success message
- Error handling: Return NotFound if enrollment doesn't exist; try-catch with error message on save failure

### FR-F009-003: Batch Grade Update

The system SHALL update multiple enrollment grades in a single POST to `/Courses/SaveGrades/{courseId}`.

- Input: `courseId` (int), array of `{ EnrollmentID, Grade }` pairs from form
- Processing: For each pair, find enrollment and update grade. Single SaveChangesAsync call for all changes. Send one notification per updated enrollment.
- Output: Redirect to grade management page with "N grades updated" success message
- Error handling: Wrap in transaction; roll back all changes if any fails

### FR-F009-004: Grade Management Navigation Link

The system SHALL add a "Manage Grades" link to each course row in the Instructor Index course table, linking to `/Courses/Grades/{courseId}`.

- Input: Course ID from the existing course table
- Processing: None (link generation only)
- Output: Anchor element in the course table

## Non-Functional Requirements

### NFR-F009-001: Authorization

Grade management actions require authentication (enforced by global `[Authorize]` filter). No additional role restriction in this iteration — any authenticated user can manage grades. Role-based restriction (instructors can only grade their own courses) is deferred to a future increment.

### NFR-F009-002: Audit Trail

Grade changes are automatically tracked via the existing `AuditInterceptor` since `Enrollment` implements `IAuditable`. `ModifiedAt` and `ModifiedBy` are set on every grade update.

### NFR-F009-003: Notification Integration

Grade updates trigger notifications via the existing `INotificationService` → SignalR pipeline. Notification format: "Enrollment for [Student Name] in [Course Title] has been updated".

## Dependencies

| Dependency | Type | Direction | Description |
|------------|------|-----------|-------------|
| Enrollment entity (F-001) | Data model | Upstream | Grade field on Enrollment |
| Course entity (F-002) | Data model | Upstream | Course context for grade page |
| Student entity (F-001) | Data model | Upstream | Student names displayed in grade roster |
| Instructor Index view (F-003) | Feature | Upstream | Navigation link to grade management |
| NotificationService (F-005) | Feature | Downstream | Grade change notifications |
| AuditInterceptor (mod-014) | Infrastructure | Upstream | Automatic audit trail on Enrollment |
| Authentication (mod-006) | Infrastructure | Upstream | Requires authenticated user |

---

## Current Implementation

### What Exists

- `Enrollment` entity with nullable `Grade` enum (A/B/C/D/F) — fully modeled, in database
- Grade display in Student Details view (`Views/Students/Details.cshtml`)
- Grade display in Instructor Index drill-down (third table: "Students Enrolled in Selected Course" shows Name + Grade)
- `Enrollment` implements `IAuditable` — audit fields exist
- 11 seed enrollments with grades for testing

### What's Missing

- No controller actions for grade entry/editing
- No dedicated grade management view
- No "Manage Grades" link in instructor course table
- No batch grade update capability
- No grade change notifications

### Files to Create

| File | Purpose |
|------|---------|
| `Views/Courses/Grades.cshtml` | Grade management page with editable dropdowns |
| `Models/ViewModels/GradeManagementViewModel.cs` | Strongly-typed view model |

### Files to Modify

| File | Change |
|------|--------|
| `Controllers/CoursesController.cs` | Add `Grades(int id)` GET, `SaveGrades(int id)` POST actions |
| `Views/Instructors/Index.cshtml` | Add "Manage Grades" link to course table rows |
