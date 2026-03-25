using ContosoUniversity.Models;
using System.Threading.Tasks;

namespace ContosoUniversity.Services
{
    public interface INotificationService
    {
        Task SendNotificationAsync(string entityType, string entityId, EntityOperation operation, string userName = null);
        Task SendNotificationAsync(string entityType, string entityId, string entityDisplayName, EntityOperation operation, string userName = null);
        Notification ReceiveNotification();
        Task MarkAsReadAsync(int notificationId);
    }
}
