# ADR-003: Replace MSMQ with Modern Messaging

## Status

Proposed

## Context

Contoso University uses **Microsoft Message Queuing (MSMQ)** via `System.Messaging` for a notification system. The `NotificationService` creates a private transactional queue, serializes `Notification` objects to JSON, and sends/receives messages. The `NotificationsController` polls the queue for pending notifications.

Problems identified in the modernization assessment (finding **A-3**):

- **MSMQ is Windows-only** — cannot run on Linux, cannot containerize
- **`System.Messaging` has no .NET Core equivalent** — blocks .NET 10 migration
- **Queue created with "Everyone" full-control permissions** — security risk
- **Dual storage**: notifications exist in both the database (`Notification` entity) and MSMQ, with no synchronization
- **Polling architecture**: client repeatedly calls `GetNotifications()` — inefficient
- **`MarkAsRead()` is a stub** — not implemented
- **No message TTL** — stale messages accumulate indefinitely

## Decision Drivers

- Must be cross-platform (.NET 10 on Linux containers)
- Notification feature is relatively simple (entity CRUD events → UI notification)
- No complex routing, pub/sub, or guaranteed delivery requirements observed
- Should support real-time push (replace polling)

## Considered Options

### Option A: Azure Service Bus

Fully managed cloud messaging service with queues, topics, and subscriptions.

**Pros:**
- Enterprise-grade reliability, dead-letter queues, message scheduling
- Native .NET SDK (`Azure.Messaging.ServiceBus`)
- Scales with Azure deployment

**Cons:**
- Requires Azure subscription and running cost
- Over-engineered for simple entity CRUD notifications
- Adds external dependency for local development

### Option B: RabbitMQ

Open-source message broker with AMQP protocol.

**Pros:**
- Feature-rich (routing, pub/sub, acknowledgments)
- Runs anywhere (Docker, on-prem, cloud)
- Large ecosystem and community

**Cons:**
- Requires separate infrastructure (RabbitMQ server)
- Operational burden (monitoring, clustering, upgrades)
- Over-engineered for this use case

### Option C: In-process Channel + SignalR (Recommended)

Use `System.Threading.Channels` for in-process message passing and ASP.NET Core SignalR for real-time push to connected clients. Notifications are persisted to the database (already modeled as `Notification` entity).

**Pros:**
- Zero external dependencies — all built into ASP.NET Core
- Real-time push replaces inefficient polling
- Database is the durable store (already exists as `Notification` table)
- Simple architecture matches simple requirements
- Works in local dev and production identically

**Cons:**
- In-process only — notifications lost if server restarts before processing (mitigated by database persistence)
- Not suitable for multi-instance scenarios without additional coordination (but app is single-instance)

### Option D: Remove notifications entirely

The notification feature appears incomplete (`MarkAsRead` is a stub). Could remove it.

**Pros:**
- Simplest option — reduces scope

**Cons:**
- Loses existing functionality
- Notifications add user value (audit trail of changes)

## Decision

**Option C: In-process Channel + SignalR** — The notification requirements are simple (entity CRUD events → UI display). The current dual-storage (MSMQ + database) is replaced by database-only persistence with SignalR for real-time delivery. This eliminates the Windows dependency, removes polling, and uses only built-in ASP.NET Core features.

If the application later needs multi-instance support or complex routing, upgrade to Azure Service Bus (Option A) at that point.

## Consequences

### Positive
- Eliminates MSMQ / `System.Messaging` dependency (unblocks .NET 10 migration)
- Real-time notifications via SignalR (better UX than polling)
- Single source of truth (database) instead of dual storage
- No external infrastructure needed

### Negative
- Requires implementing SignalR hub and client-side integration
- `MarkAsRead()` must be properly implemented (currently a stub)
- In-process channel means notifications are not durable in-flight (acceptable — database is the durable store)

### Migration Steps
1. Remove `NotificationService.cs` (MSMQ implementation)
2. Create `INotificationService` interface
3. Implement `DatabaseNotificationService` — writes to `Notification` table
4. Add SignalR hub for real-time push
5. Update `NotificationsController` to read from database instead of MSMQ
6. Implement `MarkAsRead()` against database
7. Add SignalR client in `_Layout.cshtml` or notification component

## References

- Assessment finding: A-3
- [ASP.NET Core SignalR](https://learn.microsoft.com/en-us/aspnet/core/signalr/introduction)
- [System.Threading.Channels](https://learn.microsoft.com/en-us/dotnet/core/extensions/channels)
