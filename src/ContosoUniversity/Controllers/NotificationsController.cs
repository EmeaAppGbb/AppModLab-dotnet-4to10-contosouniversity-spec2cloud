using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using ContosoUniversity.Services;
using ContosoUniversity.Models;
using ContosoUniversity.Data;

namespace ContosoUniversity.Controllers
{
    public class NotificationsController : BaseController
    {
        public NotificationsController(SchoolContext context, INotificationService notificationService)
            : base(context, notificationService) { }

        [HttpGet]
        public JsonResult GetNotifications()
        {
            var notifications = new List<Notification>();

            try
            {
                // Read unread notifications from database
                notifications = db.Notifications
                    .Where(n => !n.IsRead)
                    .OrderByDescending(n => n.CreatedAt)
                    .Take(10)
                    .ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error retrieving notifications: {ex.Message}");
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
        public JsonResult MarkAsRead(int id)
        {
            try
            {
                notificationService.MarkAsRead(id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error marking notification as read: {ex.Message}");
                return Json(new { success = false, message = "Error updating notification" });
            }
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
