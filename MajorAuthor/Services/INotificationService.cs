using MajorAuthor.Data.Entities;

namespace MajorAuthor.Services
{
    public interface INotificationService
    {
        Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId, int count = 5);
        Task<IEnumerable<Notification>> GetPaginatedNotificationsAsync(string userId, int page = 1, int pageSize = 20);
        Task<int> GetUnreadCountAsync(string userId);
        Task MarkAsReadAsync(int notificationId, string userId);
        Task MarkAllAsReadAsync(string userId);
        Task<Notification> CreateNotificationAsync(string userId, string message, string type, string targetUrl = null);
        Task DeleteNotificationsAsync(IEnumerable<int> notificationIds, string userId);
        Task<int> GetTotalCountAsync(string userId);
        Task DeleteNotificationAsync(int id, string? userId);
    }
}
