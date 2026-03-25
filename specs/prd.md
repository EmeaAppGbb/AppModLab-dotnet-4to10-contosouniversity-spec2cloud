# Product Requirements Document

## Product Vision

Contoso University is a web-based university administration system that enables staff to manage the core academic entities of a higher education institution: students, courses, instructors, and departments. The application provides full CRUD (Create, Read, Update, Delete) operations across all entity types, with supporting features including student enrollment tracking, instructor-to-course assignments, office assignments, teaching material image uploads, enrollment statistics, and a real-time notification system that alerts administrators to data changes. Built as a server-rendered MVC application backed by SQL Server, it serves as both a functional administration tool and a demonstration of Entity Framework data access patterns in an ASP.NET environment.

## User Personas

### University Administrator

- **Role**: Primary user responsible for managing all academic data — students, courses, instructors, departments, and enrollments.
- **Needs**: Full CRUD access to all entities, ability to search/sort/paginate student records, upload teaching material images for courses, assign instructors to courses and offices, appoint department administrators, view enrollment statistics, and receive notifications when data changes.
- **Goals**: Maintain accurate academic records, monitor data changes across the system, and ensure data integrity (e.g., concurrency conflict resolution for departments).
- **Source**: Inferred from codebase — all controllers expose full CRUD without access restrictions. NOTIFICATION_SYSTEM_README.md describes "admin-only" notification feature. TEACHING_MATERIAL_UPLOAD.md references "Admin" and "Teacher" roles for upload permissions, though no enforcement exists in code.

### Read-Only Viewer (Potential)

- **Role**: A user who can browse student, course, instructor, and department information but should not modify records.
- **Needs**: View-only access to entity lists and detail pages, enrollment statistics.
- **Goals**: Look up academic information without risk of accidental modification.
- **Source**: Inferred — `FilterConfig.cs` contains a commented-out global `[Authorize]` attribute with a note about "role-based authorization." TEACHING_MATERIAL_UPLOAD.md references distinct "Admin" and "Teacher" roles. No implementation exists, but the design intent suggests role differentiation was planned.

## Feature List

| ID | Feature | Description | Priority | Dependencies |
|----|---------|-------------|----------|--------------|
| F-001 | Student Management | Full CRUD operations for student records with search by first/last name, multi-column sorting (last name, first name, enrollment date with direction toggle), and pagination (10 per page). Student details view includes related course enrollments with grades. Date validation ensures enrollment date is within SQL Server datetime range (1753–9999). | P0 | — |
| F-002 | Course Management | Full CRUD operations for course records with department assignment via dropdown. Includes teaching material image upload (JPG/JPEG/PNG/GIF/BMP, max 5MB) with unique filename generation, server-side storage in `/Uploads/TeachingMaterials/`, thumbnail display (50×50px in list, 300×300px in detail), and automatic cleanup of old images on replacement. | P0 | F-004 |
| F-003 | Instructor Management | Full CRUD for instructor records with one-to-one office location assignment and many-to-many course assignment via checkbox matrix UI. Index view provides master-detail-detail drill-down: select instructor → see their courses → select course → see enrolled students. | P0 | F-002 |
| F-004 | Department Management | Full CRUD for department records with administrator (instructor) assignment, budget tracking, and start date. Implements optimistic concurrency control via SQL Server `RowVersion` timestamp — catches `DbUpdateConcurrencyException` and displays field-level conflict details to the user. | P0 | F-003 |
| F-005 | Notification System | Monitors CREATE/UPDATE/DELETE operations across Students, Courses, Instructors, and Departments. Sends notifications via MSMQ (Microsoft Message Queuing) to a private queue. Frontend polls every 5 seconds via AJAX, displays up to 5 color-coded notifications (green=create, blue=update, orange=delete) in the top-right corner with auto-dismiss after 60 seconds. Includes a notification dashboard page. | P2 | F-001, F-002, F-003, F-004 |
| F-006 | Enrollment Statistics | About page displays student body statistics — a table showing student counts grouped by enrollment date, computed via LINQ aggregation query. | P1 | F-001 |
| F-007 | Contact Information | Static page displaying Contoso University office address (Redmond, WA), phone number, and support/marketing email addresses. | P3 | — |
| F-008 | Error Handling | Global error handling via `HandleErrorAttribute`. Custom error view displays exception details in debug mode (message, controller, action). Unauthorized access page stub. | P2 | — |

## Non-Functional Requirements

### Performance

- **Pagination**: Student list paginated at 10 records per page using server-side `Skip()`/`Take()` on `IQueryable` (database-level pagination).
- **Request limits**: `maxRequestLength=10240` (10MB), `executionTimeout=3600` (1 hour) configured in `Web.config`.
- **File upload limit**: 5MB per teaching material image (application-level), 10MB total request (IIS-level via `maxAllowedContentLength`).
- **Notification polling**: 5-second interval, max 10 notifications fetched per poll, max 5 displayed simultaneously.
- **MSMQ receive timeout**: 1-second timeout on queue read to prevent blocking.
- **No caching**: No application-level caching configured. Every request creates a new `DbContext` and hits the database.
- **No async**: All database and file I/O operations are synchronous.

### Security

- **CSRF protection**: `ValidateAntiForgeryToken` attribute on all POST actions. Anti-forgery tokens rendered in all forms.
- **Authentication**: Windows Authentication configured in IIS Express settings but **not enforced** — global `[Authorize]` is commented out in `FilterConfig.cs`.
- **Authorization**: None implemented. All CRUD operations are publicly accessible.
- **Request validation**: Disabled in Views `Web.config` (`validateRequest="false"`) — XSS risk.
- **File upload validation**: Whitelist-based extension checking (jpg, jpeg, png, gif, bmp) and 5MB size limit.
- **SQL injection**: Mitigated by Entity Framework Core parameterized queries (implicit).
- **MSMQ permissions**: Queue created with "Everyone" FullControl — overly permissive.
- **Debug mode**: `<compilation debug="true">` enabled in `Web.config`.

### Reliability

- **Concurrency control**: Optimistic concurrency via `RowVersion` on `Department` entity with user-facing conflict resolution UI.
- **Error handling**: Try-catch blocks in controller actions with `Trace.TraceError()` logging. Global `HandleErrorAttribute` for unhandled exceptions.
- **Notification resilience**: Notification send failures are caught and silently logged — do not block primary CRUD operations.
- **No retry policies**: No retry logic for database operations or MSMQ communication.
- **No health checks**: No health check endpoints.
- **No circuit breakers**: No fault isolation between components.

### Scalability

- **Single-instance**: Application designed for single IIS instance. No horizontal scaling support.
- **MSMQ**: Windows-only, single-machine message queue. Not distributable.
- **File storage**: Local disk (`~/Uploads/TeachingMaterials/`). Not suitable for multi-instance deployment.
- **No containerization**: No Dockerfile or container orchestration. IIS-dependent.

### Observability

- **Logging**: `System.Diagnostics.Debug.WriteLine()` and `Trace.TraceError()` only. No structured logging framework (no Serilog, no NLog, no ILogger).
- **No metrics**: No application metrics collection.
- **No APM**: No Application Performance Monitoring integration.
- **No alerting**: No alerting rules or monitoring configuration.

## Out of Scope

The following capabilities are **not implemented** despite being common in university administration systems:

- **User authentication and authorization** — No login system, no role-based access control. Planned (comments reference roles) but not implemented.
- **Enrollment management** — Students cannot self-enroll in courses. Enrollments exist as seed data but no UI for creating/editing enrollments.
- **Grade management** — Grades exist on `Enrollment` records but there is no UI for instructors to enter or modify grades.
- **Student self-service** — No student-facing portal for viewing their own enrollments, grades, or schedule.
- **Reporting and analytics** — Beyond the simple enrollment date statistics, no reporting capabilities exist.
- **Audit trail** — `Notification` entity has `CreatedAt`/`CreatedBy` but no systematic audit trail across all entities.
- **Notification persistence** — `MarkAsRead()` is a stub. Notifications are consumed from MSMQ but not persisted to the database for history.
- **Import/export** — No bulk data import or export functionality.
- **Academic calendar** — No semester, term, or schedule management.
- **Multi-tenancy** — Single institution only.

## Appendix: Extraction Evidence

| PRD Section | Evidence Source |
|-------------|---------------|
| Product Vision | `src/ContosoUniversity/README.md` (application description), `Views/Home/Index.cshtml` (welcome text referencing "Entity Framework 6 demo with Windows Authentication") |
| University Administrator persona | `NOTIFICATION_SYSTEM_README.md` ("admin-only feature"), `TEACHING_MATERIAL_UPLOAD.md` ("Admin" role for upload), all controllers (full CRUD without restrictions) |
| Read-Only Viewer persona | `App_Start/FilterConfig.cs` (commented `AuthorizeAttribute`), `TEACHING_MATERIAL_UPLOAD.md` ("Teacher" role reference) |
| F-001 Student Management | `Controllers/StudentsController.cs`, `Views/Students/*.cshtml`, `Models/Student.cs`, `PaginatedList.cs` |
| F-002 Course Management | `Controllers/CoursesController.cs`, `Views/Courses/*.cshtml`, `Models/Course.cs`, `TEACHING_MATERIAL_UPLOAD.md` |
| F-003 Instructor Management | `Controllers/InstructorsController.cs`, `Views/Instructors/*.cshtml`, `Models/Instructor.cs`, `Models/CourseAssignment.cs`, `Models/OfficeAssignment.cs` |
| F-004 Department Management | `Controllers/DepartmentsController.cs`, `Views/Departments/*.cshtml`, `Models/Department.cs` (RowVersion property) |
| F-005 Notification System | `Controllers/NotificationsController.cs`, `Services/NotificationService.cs`, `Models/Notification.cs`, `NOTIFICATION_SYSTEM_README.md`, `Views/Notifications/Index.cshtml` |
| F-006 Enrollment Statistics | `Controllers/HomeController.cs` (About action), `Views/Home/About.cshtml`, `Models/SchoolViewModels/EnrollmentDateGroup.cs` |
| F-007 Contact Information | `Controllers/HomeController.cs` (Contact action), `Views/Home/Contact.cshtml` |
| F-008 Error Handling | `App_Start/FilterConfig.cs` (HandleErrorAttribute), `Views/Shared/Error.cshtml`, `Controllers/HomeController.cs` (Error/Unauthorized actions) |
| Performance NFRs | `Web.config` (maxRequestLength, executionTimeout), `Controllers/StudentsController.cs` (page size 10), `NOTIFICATION_SYSTEM_README.md` (polling interval, limits) |
| Security NFRs | `App_Start/FilterConfig.cs`, `Views/Web.config` (validateRequest), `Controllers/*.cs` (ValidateAntiForgeryToken), `Services/NotificationService.cs` (MSMQ permissions) |
| Reliability NFRs | `Controllers/DepartmentsController.cs` (DbUpdateConcurrencyException), `Controllers/BaseController.cs` (notification try-catch) |
| Out of Scope | Absence analysis across all source files — no enrollment CRUD UI, no grade entry UI, no auth middleware, no import/export routes |
