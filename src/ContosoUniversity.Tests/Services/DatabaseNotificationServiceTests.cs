using System.Threading.Tasks;
using ContosoUniversity.Data;
using ContosoUniversity.Models;
using ContosoUniversity.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ContosoUniversity.Tests.Services
{
    public class DatabaseNotificationServiceTests
    {
        private SchoolContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<SchoolContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
            return new SchoolContext(options);
        }

        [Fact]
        public async Task SendNotification_SavesNotificationToDatabase()
        {
            using var context = CreateContext();
            var service = new DatabaseNotificationService(context, null, NullLogger<DatabaseNotificationService>.Instance);

            await service.SendNotificationAsync("Student", "1", "Test Student", EntityOperation.CREATE, "testuser");

            var notification = await context.Notifications.FirstOrDefaultAsync();
            Assert.NotNull(notification);
            Assert.Equal("Student", notification.EntityType);
            Assert.Equal("CREATE", notification.Operation);
            Assert.Equal("testuser", notification.CreatedBy);
            Assert.False(notification.IsRead);
        }

        [Fact]
        public async Task MarkAsRead_SetsIsReadAndReadAt()
        {
            using var context = CreateContext();
            var service = new DatabaseNotificationService(context, null, NullLogger<DatabaseNotificationService>.Instance);

            await service.SendNotificationAsync("Student", "1", EntityOperation.CREATE);
            var notification = await context.Notifications.FirstAsync();

            await service.MarkAsReadAsync(notification.Id);

            var updated = await context.Notifications.FindAsync(notification.Id);
            Assert.NotNull(updated);
            Assert.True(updated.IsRead);
            Assert.NotNull(updated.ReadAt);
        }
    }
}
