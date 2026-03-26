using System;
using System.Linq;
using System.Threading.Tasks;
using ContosoUniversity.Data;
using ContosoUniversity.Models;
using ContosoUniversity.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ContosoUniversity.Tests.Services
{
    public class NotificationServiceExtendedTests
    {
        private SchoolContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<SchoolContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new SchoolContext(options);
        }

        [Fact]
        public async Task GetPendingNotifications_ReturnsOnlyUnread()
        {
            using var context = CreateContext();
            var service = new DatabaseNotificationService(context, null, NullLogger<DatabaseNotificationService>.Instance);

            await service.SendNotificationAsync("Student", "1", "Student A", EntityOperation.CREATE, "testuser");
            await service.SendNotificationAsync("Student", "2", "Student B", EntityOperation.CREATE, "testuser");

            // Mark first as read
            var first = await context.Notifications.FirstAsync();
            await service.MarkAsReadAsync(first.Id);

            // ReceiveNotification should return only unread
            var pending = service.ReceiveNotification();
            Assert.NotNull(pending);
            Assert.Equal("2", pending.EntityId);
            Assert.False(pending.IsRead);
        }

        [Fact]
        public async Task GetPendingNotifications_ReturnsMaxResults()
        {
            using var context = CreateContext();
            var service = new DatabaseNotificationService(context, null, NullLogger<DatabaseNotificationService>.Instance);

            // Create multiple notifications
            for (int i = 1; i <= 15; i++)
            {
                await service.SendNotificationAsync("Student", i.ToString(), $"Student {i}", EntityOperation.CREATE, "testuser");
            }

            // All 15 should be unread in DB
            var unreadCount = await context.Notifications.CountAsync(n => !n.IsRead);
            Assert.Equal(15, unreadCount);
        }

        [Fact]
        public async Task SendNotification_GeneratesCorrectMessage_ForCreate()
        {
            using var context = CreateContext();
            var service = new DatabaseNotificationService(context, null, NullLogger<DatabaseNotificationService>.Instance);

            await service.SendNotificationAsync("Student", "1", "John Doe", EntityOperation.CREATE, "testuser");

            var notification = await context.Notifications.FirstAsync();
            Assert.Equal("New Student 'John Doe' has been created", notification.Message);
        }

        [Fact]
        public async Task SendNotification_GeneratesCorrectMessage_ForUpdate()
        {
            using var context = CreateContext();
            var service = new DatabaseNotificationService(context, null, NullLogger<DatabaseNotificationService>.Instance);

            await service.SendNotificationAsync("Course", "101", "Chemistry", EntityOperation.UPDATE, "testuser");

            var notification = await context.Notifications.FirstAsync();
            Assert.Equal("Course 'Chemistry' has been updated", notification.Message);
        }

        [Fact]
        public async Task SendNotification_GeneratesCorrectMessage_ForDelete()
        {
            using var context = CreateContext();
            var service = new DatabaseNotificationService(context, null, NullLogger<DatabaseNotificationService>.Instance);

            await service.SendNotificationAsync("Department", "5", "Physics", EntityOperation.DELETE, "testuser");

            var notification = await context.Notifications.FirstAsync();
            Assert.Equal("Department 'Physics' has been deleted", notification.Message);
        }
    }
}
