using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ContosoUniversity.Data;
using ContosoUniversity.Hubs;
using ContosoUniversity.Models;
using Microsoft.AspNetCore.SignalR;

namespace ContosoUniversity.Services
{
    public class DatabaseNotificationService : INotificationService
    {
        private readonly SchoolContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<DatabaseNotificationService> _logger;

        public DatabaseNotificationService(SchoolContext context, IHubContext<NotificationHub> hubContext, ILogger<DatabaseNotificationService> logger)
        {
            _context = context;
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task SendNotificationAsync(string entityType, string entityId, EntityOperation operation, string userName = null)
        {
            await SendNotificationAsync(entityType, entityId, null, operation, userName);
        }

        public async Task SendNotificationAsync(string entityType, string entityId, string entityDisplayName, EntityOperation operation, string userName = null)
        {
            try
            {
                var notification = new Notification
                {
                    EntityType = entityType,
                    EntityId = entityId,
                    Operation = operation.ToString(),
                    Message = GenerateMessage(entityType, entityId, entityDisplayName, operation),
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userName ?? "System",
                    IsRead = false
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                if (_hubContext != null)
                {
                    await _hubContext.Clients.All.SendAsync("ReceiveNotification", notification);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Failed to send notification: {ex.Message}");
            }
        }

        public Notification ReceiveNotification()
        {
            return _context.Notifications
                .Where(n => !n.IsRead)
                .OrderBy(n => n.CreatedAt)
                .FirstOrDefault();
        }

        public async Task MarkAsReadAsync(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        private string GenerateMessage(string entityType, string entityId, string entityDisplayName, EntityOperation operation)
        {
            var displayText = !string.IsNullOrWhiteSpace(entityDisplayName)
                ? $"{entityType} '{entityDisplayName}'"
                : $"{entityType} (ID: {entityId})";

            return operation switch
            {
                EntityOperation.CREATE => $"New {displayText} has been created",
                EntityOperation.UPDATE => $"{displayText} has been updated",
                EntityOperation.DELETE => $"{displayText} has been deleted",
                _ => $"{displayText} operation: {operation}"
            };
        }
    }
}
