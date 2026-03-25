# FRD: Notification System

**Feature ID**: F-005
**Status**: Draft
**Priority**: P2
**Last Updated**: 2026-03-25

## Description

The Notification System monitors CREATE, UPDATE, and DELETE operations across all major entities (Students, Courses, Instructors, Departments) and delivers real-time notifications to administrators. When a CRUD operation completes in any controller, a notification message is sent to a Microsoft Message Queuing (MSMQ) private queue. The frontend polls a JSON endpoint every 5 seconds, retrieves pending notifications, and displays them as color-coded toast messages in the top-right corner of the page (green for create, blue for update, orange for delete). The system includes a notification dashboard page explaining the feature and providing quick-test buttons. This is a cross-cutting feature that depends on all entity management features and consists of three layers: a backend service (MSMQ producer/consumer), an API endpoint (JSON polling), and a frontend UI (JavaScript toast display).

## User Stories

### US-F005-001: Receive Real-Time Notifications

**As a** University Administrator
**I want to** see notifications when entity data changes
**So that** I am aware of modifications happening in the system.

**Acceptance Criteria:**
- GIVEN I have the application open WHEN a student/course/instructor/department is created THEN a green notification appears in the top-right corner within 5 seconds
- GIVEN I have the application open WHEN an entity is updated THEN a blue notification appears
- GIVEN I have the application open WHEN an entity is deleted THEN an orange notification appears
- GIVEN a notification is displayed WHEN 60 seconds pass THEN the notification auto-dismisses
- GIVEN multiple notifications arrive WHEN more than 5 are pending THEN only the 5 most recent are displayed

### US-F005-002: View Notification Dashboard

**As a** University Administrator
**I want to** view a dashboard explaining the notification system
**So that** I understand what is being monitored and can test the system.

**Acceptance Criteria:**
- GIVEN I navigate to the Notifications page WHEN the dashboard loads THEN I see monitored operations (CREATE, UPDATE, DELETE) and monitored entities (Students, Courses, Instructors, Departments)
- GIVEN the dashboard is displayed WHEN I view the technology section THEN I see MSMQ queue information and configuration details
- GIVEN the dashboard is displayed WHEN I click a quick-test button THEN I am directed to the create page for the selected entity

### US-F005-003: Poll for Notifications

**As a** the frontend JavaScript client
**I want to** poll the notifications endpoint periodically
**So that** new notifications are fetched and displayed.

**Acceptance Criteria:**
- GIVEN the polling timer fires WHEN the client calls `/Notifications/GetNotifications` THEN a JSON response is returned with `success`, `count`, and `notifications` array
- GIVEN notifications are pending in the queue WHEN polled THEN up to 10 notifications are returned and removed from the queue
- GIVEN no notifications are pending WHEN polled THEN an empty array is returned with count 0

## Functional Requirements

### FR-F005-001: Send Notification to Queue

The system SHALL send a notification message to the MSMQ queue after each successful CRUD operation. The `BaseController.SendEntityNotification()` method creates a `Notification` object with entity type, entity ID, operation type, and a human-readable message, then calls `NotificationService.SendNotification()` which serializes to JSON and enqueues.

- Input: Entity type string, entity ID, operation enum (Create/Update/Delete), optional display name
- Processing: Create Notification object, serialize to JSON, send to MSMQ private queue
- Output: Message enqueued in MSMQ
- Error handling: Try-catch in BaseController — notification failures do not block CRUD operations; errors logged to Debug.WriteLine

### FR-F005-002: Receive Notifications from Queue

The system SHALL dequeue notifications via `NotificationService.ReceiveNotification()` with a 1-second timeout. Messages are deserialized from JSON back to `Notification` objects. The `NotificationsController.GetNotifications()` endpoint reads up to 10 messages per poll.

- Input: HTTP GET `/Notifications/GetNotifications`
- Processing: Loop up to 10 times calling `ReceiveNotification()`, collect results
- Output: JSON object `{ success: bool, count: int, notifications: [...] }`
- Error handling: MSMQ timeout returns null (no message), breaking the loop. Exceptions caught and return `{ success: false }`

### FR-F005-003: MSMQ Queue Management

The system SHALL auto-create the MSMQ private queue on first use if it doesn't exist. Queue path defaults to `.\Private$\ContosoUniversityNotifications` but is configurable via `Web.config` appSettings key `NotificationQueuePath`. Queue is created with transactional mode and "Everyone" FullControl permissions.

- Input: Queue path from config or default
- Processing: Check `MessageQueue.Exists()`, create if missing, set permissions
- Output: Initialized MessageQueue instance
- Error handling: Creation failure logged to Debug

### FR-F005-004: Mark Notification as Read (Stub)

The system SHALL provide a `MarkAsRead` endpoint that accepts a notification ID via POST. Currently returns a success JSON response but **does not persist the read status** — this is a stub implementation.

- Input: `id` parameter via POST
- Processing: None (stub)
- Output: `{ success: true }`
- Error handling: N/A (stub)

## Non-Functional Requirements

### NFR-F005-001: Non-Blocking Notifications

Notification send failures MUST NOT block or fail the primary CRUD operation. All notification sends are wrapped in try-catch.

### NFR-F005-002: Polling Interval

Frontend polls at 5-second intervals. Each poll fetches up to 10 messages. Max 5 notifications displayed simultaneously on the UI.

### NFR-F005-003: Auto-Dismiss

Notifications auto-dismiss after 60 seconds or can be manually closed.

## Dependencies

| Dependency | Type | Direction | Description |
|------------|------|-----------|-------------|
| Student Management (F-001) | Feature | Upstream | Triggers CREATE/UPDATE/DELETE notifications |
| Course Management (F-002) | Feature | Upstream | Triggers CREATE/UPDATE/DELETE notifications |
| Instructor Management (F-003) | Feature | Upstream | Triggers CREATE/UPDATE/DELETE notifications |
| Department Management (F-004) | Feature | Upstream | Triggers CREATE/UPDATE/DELETE notifications |
| MSMQ (System.Messaging) | External | Infrastructure | Message queue for notification delivery |
| BaseController | Shared infrastructure | Upstream | `SendEntityNotification()` method |

---

## Current Implementation (Brownfield Extension)

### Files Involved

| File Path | Role | Notes |
|-----------|------|-------|
| `Services/NotificationService.cs` | MSMQ producer/consumer | Queue init, send, receive, dispose |
| `Controllers/NotificationsController.cs` | JSON API + dashboard view | GetNotifications, MarkAsRead, Index |
| `Controllers/BaseController.cs` | Notification trigger point | SendEntityNotification() in every CRUD controller |
| `Models/Notification.cs` | Entity model | EntityType, EntityId, Operation, Message, CreatedAt, IsRead |
| `Views/Notifications/Index.cshtml` | Dashboard view | Feature explanation, quick-test buttons |
| `Scripts/notifications.js` | Frontend polling + toast UI | (Referenced in _Layout.cshtml, not in csproj compile items) |

### Architecture Pattern

Cross-cutting concern implemented via base class inheritance (not middleware or event system). Notification sending is tightly coupled to controller CRUD actions. MSMQ serves as an IPC mechanism between the send path and the polling endpoint. No event bus or publish-subscribe pattern.

### Test Coverage

| Test Type | Files | Assertions | Coverage |
|-----------|-------|------------|----------|
| Unit | — | 0 | 0% |
| Integration | — | 0 | 0% |
| E2E | — | 0 | 0% |

**Untested paths**: MSMQ queue creation, message serialization/deserialization, polling endpoint, concurrent read from queue, queue permission setup, timeout behavior.

### Known Limitations

- **MarkAsRead() is a stub** — returns success but doesn't persist read status
- **Dual storage gap**: `Notification` entity exists in DbContext but notifications are only written to MSMQ, not the database. No synchronization between the two stores.
- **"Everyone" FullControl** on MSMQ queue — security vulnerability in production
- **XmlMessageFormatter configured but JSON used** — semantic mismatch (works because body is string)
- **No message TTL** — stale notifications accumulate in queue if not polled
- **Messages consumed destructively** — once read from queue, they are gone. If frontend misses them, they're lost.
- **No reconnection logic** — if MSMQ service stops, notifications silently fail
- **Windows-only** — MSMQ and `System.Messaging` have no cross-platform equivalent
- **Silent failure** — all errors logged to `Debug.WriteLine()` only

### Integration Points

| External System | Protocol | Purpose | Config Location |
|----------------|----------|---------|-----------------|
| MSMQ | System.Messaging (IPC) | Notification message queue | `Web.config` appSettings `NotificationQueuePath` |
| SQL Server LocalDB | TCP/SQL via EF Core 3.1 | Notification entity (unused for write) | `Web.config` connectionStrings |
