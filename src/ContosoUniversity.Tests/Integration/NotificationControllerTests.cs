using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using ContosoUniversity.Data;
using ContosoUniversity.Models;
using ContosoUniversity.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ContosoUniversity.Tests.Integration
{
    public class NotificationControllerTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly TestWebApplicationFactory _factory;

        public NotificationControllerTests(TestWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task NotificationIndex_ReturnsDashboard()
        {
            // US-F005-002: Dashboard page returns 200
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/Notifications");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetNotifications_ReturnsJsonWithSuccessAndCount()
        {
            // US-F005-003: GetNotifications returns JSON with success and count
            // Seed a notification directly in the DB, then query via API
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SchoolContext>();
            db.Notifications.Add(new Notification
            {
                EntityType = "Student",
                EntityId = "1",
                Operation = "CREATE",
                Message = "Test notification",
                CreatedAt = System.DateTime.UtcNow,
                CreatedBy = "testuser",
                IsRead = false
            });
            await db.SaveChangesAsync();

            var client = _factory.CreateClient();
            var response = await client.GetAsync("/Notifications/GetNotifications");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            Assert.True(doc.RootElement.GetProperty("success").GetBoolean());
            Assert.True(doc.RootElement.TryGetProperty("count", out _));
        }

        [Fact]
        public async Task GetNotifications_EmptyDb_ReturnsSuccessWithZeroCount()
        {
            // US-F005-003: GetNotifications with no unread notifications
            // Use a fresh factory to get a clean DB
            using var freshFactory = new TestWebApplicationFactory();
            var client = freshFactory.CreateSeededClient();

            // Mark all existing notifications as read to ensure clean state
            using var scope = freshFactory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SchoolContext>();
            var unread = await db.Notifications.Where(n => !n.IsRead).ToListAsync();
            foreach (var n in unread)
            {
                n.IsRead = true;
            }
            await db.SaveChangesAsync();

            var response = await client.GetAsync("/Notifications/GetNotifications");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            Assert.True(doc.RootElement.GetProperty("success").GetBoolean());
            Assert.Equal(0, doc.RootElement.GetProperty("count").GetInt32());
        }
    }
}
