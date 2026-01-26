// Services/NotificationService.cs
using MajorAuthor.Data;
using MajorAuthor.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MajorAuthor.Services
{
    public class NotificationService : INotificationService
    {
        private readonly MajorAuthorDbContext _context;

        public NotificationService(MajorAuthorDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId, int count = 5)
        {
            return await _context.Notifications
                .Where(n => n.ApplicationUserId == userId)
                .OrderByDescending(n => n.CreatedDate)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Notification>> GetPaginatedNotificationsAsync(string userId, int page = 1, int pageSize = 20)
        {
            return await _context.Notifications
                .Where(n => n.ApplicationUserId == userId)
                .OrderByDescending(n => n.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _context.Notifications
                .Where(n => n.ApplicationUserId == userId && !n.IsRead)
                .CountAsync();
        }

        public async Task MarkAsReadAsync(int notificationId, string userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.ApplicationUserId == userId);

            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkAllAsReadAsync(string userId)
        {
            var unreadNotifications = await _context.Notifications
                .Where(n => n.ApplicationUserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<Notification> CreateNotificationAsync(string userId, string message, string type, string targetUrl = null)
        {
            var notification = new Notification
            {
                ApplicationUserId = userId,
                Message = message,
                Type = type,
                TargetUrl = targetUrl,
                CreatedDate = DateTime.UtcNow,
                IsRead = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return notification;
        }
        // ... существующие методы ...

        // Добавьте этот метод
        public async Task DeleteNotificationsAsync(IEnumerable<int> notificationIds, string userId)
        {
            var notificationsToDelete = await _context.Notifications
                .Where(n => notificationIds.Contains(n.Id) && n.ApplicationUserId == userId)
                .ToListAsync();

            if (notificationsToDelete.Any())
            {
                _context.Notifications.RemoveRange(notificationsToDelete);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteNotificationAsync(int notificationId, string userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.ApplicationUserId == userId);

            if (notification != null)
            {
                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> GetTotalCountAsync(string userId)
        {
            return await _context.Notifications
                .Where(n => n.ApplicationUserId == userId)
                .CountAsync();
        }
    }
    // ...
}