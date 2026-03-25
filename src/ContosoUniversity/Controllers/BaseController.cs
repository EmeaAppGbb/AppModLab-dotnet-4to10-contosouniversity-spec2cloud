using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ContosoUniversity.Services;
using ContosoUniversity.Models;
using ContosoUniversity.Data;

namespace ContosoUniversity.Controllers
{
    public abstract class BaseController : Controller
    {
        protected readonly SchoolContext db;
        protected readonly INotificationService notificationService;
        protected readonly ILogger _baseLogger;

        public BaseController(SchoolContext context, INotificationService notificationService, ILogger logger)
        {
            db = context;
            this.notificationService = notificationService;
            _baseLogger = logger;
        }

        protected Task SendEntityNotificationAsync(string entityType, string entityId, EntityOperation operation)
        {
            return SendEntityNotificationAsync(entityType, entityId, null, operation);
        }

        protected async Task SendEntityNotificationAsync(string entityType, string entityId, string entityDisplayName, EntityOperation operation)
        {
            try
            {
                var userName = User?.Identity?.Name ?? "System";
                await notificationService.SendNotificationAsync(entityType, entityId, entityDisplayName, operation, userName);
            }
            catch (Exception ex)
            {
                _baseLogger.LogWarning($"Failed to send notification: {ex.Message}");
            }
        }
    }
}