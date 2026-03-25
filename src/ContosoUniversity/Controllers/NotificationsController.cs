using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ContosoUniversity.Services;
using ContosoUniversity.Models;
using ContosoUniversity.Data;

namespace ContosoUniversity.Controllers
{
    public class NotificationsController : BaseController
    {
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(SchoolContext context, INotificationService notificationService, ILogger<NotificationsController> logger)
            : base(context, notificationService, logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public async Task<JsonResult> GetNotifications()
        {
            var notifications = new List<Notification>();

            try
            {
                // Read unread notifications from database
                notifications = await db.Notifications
                    .Where(n => !n.IsRead)
                    .OrderByDescending(n => n.CreatedAt)
                    .Take(10)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error retrieving notifications: {ex.Message}");
                return Json(new { success = false, message = "Error retrieving notifications" });
            }

            return Json(new
            {
                success = true,
                notifications = notifications,
                count = notifications.Count
            });
        }

        [HttpPost]
        public async Task<JsonResult> MarkAsRead(int id)
        {
            try
            {
                await notificationService.MarkAsReadAsync(id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error marking notification as read: {ex.Message}");
                return Json(new { success = false, message = "Error updating notification" });
            }
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
