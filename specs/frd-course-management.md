# FRD: Course Management

**Feature ID**: F-002
**Status**: Draft
**Priority**: P0
**Last Updated**: 2026-03-25

## Description

Course Management provides full CRUD operations for academic course records within Contoso University. Each course has a manually-assigned course ID, title, credit value, and department assignment. The feature includes a teaching material image upload capability — administrators can attach an image (JPG/PNG/GIF/BMP, max 5MB) to each course, which is stored on the server filesystem and displayed as thumbnails in the course list and full-size on the detail page. Old images are automatically cleaned up when replaced. Courses are linked to departments (required), and their data feeds into the instructor management and enrollment systems.

## User Stories

### US-F002-001: View Course List

**As a** University Administrator
**I want to** view a list of all courses with their department and teaching material thumbnail
**So that** I can browse the course catalog.

**Acceptance Criteria:**
- GIVEN courses exist WHEN I navigate to the Courses index THEN I see a table with Course ID, Title, Credits, Department Name, Teaching Material thumbnail, and action links
- GIVEN a course has a teaching material image WHEN I view the list THEN a 50×50px thumbnail is displayed
- GIVEN a course has no teaching material image WHEN I view the list THEN "No image" text is displayed

### US-F002-002: Create a Course with Image Upload

**As a** University Administrator
**I want to** create a new course with optional teaching material image
**So that** the course catalog is expanded.

**Acceptance Criteria:**
- GIVEN I am on the Create Course page WHEN I submit valid course data (ID, title, credits, department) THEN a new course record is created
- GIVEN I attach a valid image file (JPG/PNG/GIF/BMP, ≤5MB) WHEN I submit the form THEN the image is saved to `/Uploads/TeachingMaterials/` with a unique filename
- GIVEN I attach a file with an unsupported extension WHEN I submit the form THEN a validation error is displayed listing accepted formats
- GIVEN I attach a file larger than 5MB WHEN I submit the form THEN a validation error about file size is displayed
- GIVEN I do not attach any file WHEN I submit the form THEN the course is created without a teaching material image
- GIVEN a course is successfully created WHEN the operation completes THEN a CREATE notification is sent

### US-F002-003: View Course Details

**As a** University Administrator
**I want to** view a course's full details including its teaching material image
**So that** I can review course information.

**Acceptance Criteria:**
- GIVEN a course exists WHEN I navigate to its details page THEN I see Course ID, Title, Credits, Department Name
- GIVEN the course has a teaching material image WHEN I view details THEN the image is displayed at 300×300px maximum
- GIVEN the course has no image WHEN I view details THEN "No image uploaded" is displayed

### US-F002-004: Edit a Course with Image Replacement

**As a** University Administrator
**I want to** edit course details and optionally replace the teaching material image
**So that** course information stays current.

**Acceptance Criteria:**
- GIVEN a course exists WHEN I navigate to its edit page THEN the form is pre-populated with current values
- GIVEN I upload a new image WHEN I submit the form THEN the old image file is deleted from disk and the new image is saved
- GIVEN I do not upload a new image WHEN I submit the form THEN the existing image is preserved
- GIVEN a course is successfully updated WHEN the operation completes THEN an UPDATE notification is sent

### US-F002-005: Delete a Course

**As a** University Administrator
**I want to** delete a course record
**So that** discontinued courses are removed from the catalog.

**Acceptance Criteria:**
- GIVEN a course exists WHEN I navigate to the delete page THEN I see course details as confirmation
- GIVEN I confirm deletion WHEN I click Delete THEN the course is permanently removed
- GIVEN a course is successfully deleted WHEN the operation completes THEN a DELETE notification is sent

## Functional Requirements

### FR-F002-001: Course List with Department and Image

The system SHALL display all courses in a table showing CourseID, Title, Credits, Department.Name, and a teaching material image thumbnail (50×50px) or "No image" text. Department is eagerly loaded via `Include()`.

- Input: None (displays all courses)
- Processing: Query courses with department include
- Output: Table view with image thumbnails
- Error handling: Null department handled via navigation property

### FR-F002-002: Teaching Material Image Upload

The system SHALL accept image uploads on Create and Edit with the following validation:
- Allowed extensions: `.jpg`, `.jpeg`, `.png`, `.gif`, `.bmp` (case-insensitive)
- Maximum file size: 5,242,880 bytes (5MB)
- Storage path: `~/Uploads/TeachingMaterials/`
- Filename format: `course_{CourseID}_{GUID}{extension}`
- Directory auto-creation if not exists

- Input: `HttpPostedFileBase` from multipart form
- Processing: Validate extension and size, generate unique filename, save to disk, store relative path in `TeachingMaterialImagePath`
- Output: File saved to server filesystem; path stored in database
- Error handling: Validation errors added to ModelState; file I/O errors caught with error message

### FR-F002-003: Image Cleanup on Replacement

The system SHALL delete the previous teaching material image file from disk when a new image is uploaded during Edit. Uses `Server.MapPath()` to resolve the physical path and `System.IO.File.Delete()` to remove.

- Input: Existing `TeachingMaterialImagePath` from database
- Processing: Resolve physical path, delete old file, save new file
- Output: Old file removed, new file in place
- Error handling: Delete failures logged but do not block the update

### FR-F002-004: Create Course

The system SHALL create a new course record. CourseID is manually assigned (not auto-generated). Department is selected via dropdown populated from all departments.

- Input: `CourseID`, `Title`, `Credits`, `DepartmentID`, optional image file via form POST
- Processing: Validate model, process image upload if present, add to DbContext, `SaveChanges()`, send notification
- Output: Redirect to Index on success
- Error handling: ModelState errors redisplay form; try-catch for database errors

### FR-F002-005: Edit Course

The system SHALL update an existing course record including optional image replacement. Uses `TryUpdateModel()` for field binding.

- Input: Course `id`, updated fields and optional image file via form POST
- Processing: Fetch course, bind updates, process image upload if present (delete old, save new), `SaveChanges()`, send notification
- Output: Redirect to Index on success
- Error handling: Concurrency and validation errors handled

### FR-F002-006: Delete Course

The system SHALL permanently delete a course record with two-step confirmation.

- Input: Course `id` (GET for confirmation, POST for execution)
- Processing: Find course, remove from DbContext, `SaveChanges()`, send notification
- Output: Redirect to Index
- Error handling: Try-catch with error logging

## Non-Functional Requirements

### NFR-F002-001: File Size Limit

Teaching material uploads are limited to 5MB at the application level. IIS-level `maxAllowedContentLength` is set to 10MB in `Web.config`.

### NFR-F002-002: CSRF Protection

All POST actions protected by `ValidateAntiForgeryToken`.

### NFR-F002-003: File Type Whitelist

Only image extensions (jpg, jpeg, png, gif, bmp) are accepted. Validation occurs server-side via extension check.

## Dependencies

| Dependency | Type | Direction | Description |
|------------|------|-----------|-------------|
| Department (F-004) | Feature | Upstream | Courses require a department assignment |
| SchoolContext (EF Core) | Infrastructure | Upstream | Data access for Course entity |
| NotificationService (F-005) | Feature | Downstream | Sends CRUD notifications |
| BaseController | Shared infrastructure | Upstream | Provides DbContext and NotificationService |
| Server filesystem | Infrastructure | External | Stores uploaded teaching material images |
| Enrollment entity | Data model | Downstream | Enrollments reference courses |
| CourseAssignment entity | Data model | Downstream | Instructor assignments reference courses |

---

## Current Implementation (Brownfield Extension)

### Files Involved

| File Path | Role | Notes |
|-----------|------|-------|
| `Controllers/CoursesController.cs` | Route handlers for CRUD + image upload | ~200 lines, inherits BaseController |
| `Models/Course.cs` | Entity model | CourseID (non-generated), Title, Credits, DepartmentID, TeachingMaterialImagePath |
| `Views/Courses/Index.cshtml` | List view with thumbnails | Conditional image display |
| `Views/Courses/Create.cshtml` | Create form with file upload | Multipart form, image type validation hint |
| `Views/Courses/Edit.cshtml` | Edit form with file upload | File replacement logic |
| `Views/Courses/Details.cshtml` | Detail view with full-size image | 300×300px max display |
| `Views/Courses/Delete.cshtml` | Delete confirmation | Standard confirmation pattern |
| `Uploads/TeachingMaterials/` | File storage directory | Contains uploaded images |

### Architecture Pattern

MVC controller with direct DbContext access. File upload handling is inline in the controller (no dedicated file service). Image validation, storage, and cleanup are all controller responsibilities.

### Test Coverage

| Test Type | Files | Assertions | Coverage |
|-----------|-------|------------|----------|
| Unit | — | 0 | 0% |
| Integration | — | 0 | 0% |
| E2E | — | 0 | 0% |

**Untested paths**: Image upload validation, file size boundary (exactly 5MB), concurrent image replacement, orphaned files if course creation fails after image save, directory creation race conditions.

### Known Limitations

- `Server.MapPath()` used for path resolution — ASP.NET Framework-specific, no equivalent in .NET Core
- File I/O is synchronous (blocking thread during upload/save/delete)
- No virus/malware scanning on uploaded files
- Race condition: old image deletion happens after new image save — failure leaves orphaned files
- Directory existence check runs on every upload request instead of once at startup
- Tilde paths (`~/Uploads/...`) in database — framework-specific path resolution
- `Course.Find(id)` could return null, but subsequent code does not always null-check before using the result

### Integration Points

| External System | Protocol | Purpose | Config Location |
|----------------|----------|---------|-----------------|
| SQL Server LocalDB | TCP/SQL via EF Core 3.1 | Course data store | `Web.config` connectionStrings |
| Local filesystem | File I/O | Teaching material image storage | Hardcoded path `~/Uploads/TeachingMaterials/` |
| MSMQ | System.Messaging | Send CRUD notifications | `Web.config` appSettings |
