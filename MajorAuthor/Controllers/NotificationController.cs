// Controllers/NotificationController.cs
using MajorAuthor.Models;
using MajorAuthor.Services;
using MajorAuthor.Services;
using MajorAuthor.ViewModels;
using MajorAuthor.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MajorAuthor.Web.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 20)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var notifications = await _notificationService.GetPaginatedNotificationsAsync(userId, page, pageSize);
            var unreadCount = await _notificationService.GetUnreadCountAsync(userId);
            var totalCount = await _notificationService.GetTotalCountAsync(userId);

            var viewModel = new NotificationListViewModel
            {
                Notifications = notifications.Select(n => new NotificationViewModel
                {
                    Id = n.Id,
                    Message = n.Message,
                    CreatedDate = n.CreatedDate,
                    IsRead = n.IsRead,
                    Type = n.Type,
                    TargetUrl = n.TargetUrl
                }).ToList(),
                CurrentPage = page,
                TotalPages = (totalCount + pageSize - 1) / pageSize,
                UnreadCount = unreadCount
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead([FromBody] MarkReadModel model)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            await _notificationService.MarkAsReadAsync(model.Id, userId);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            await _notificationService.MarkAllAsReadAsync(userId);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Delete([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any())
            {
                return BadRequest("Нет выбранных уведомлений");
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            await _notificationService.DeleteNotificationsAsync(ids, userId);
            return Ok(new { message = "Уведомления удалены" });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSingle([FromBody] DeleteSingleModel model)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            await _notificationService.DeleteNotificationAsync(model.Id, userId);
            return Ok(new { message = "Уведомление удалено" });
        }

        [HttpGet]
        public async Task<IActionResult> GetNotificationCount()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var count = await _notificationService.GetUnreadCountAsync(userId);
            return Json(new { unreadCount = count });
        }

        [HttpGet]
        public async Task<IActionResult> GetRecentNotifications()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var notifications = await _notificationService.GetUserNotificationsAsync(userId, 5);

            var result = notifications.Select(n => new NotificationViewModel
            {
                Id = n.Id,
                Message = n.Message,
                CreatedDate = n.CreatedDate,
                IsRead = n.IsRead,
                Type = n.Type,
                TargetUrl = n.TargetUrl
            });

            return Json(result);
        }
    }

    public class MarkReadModel
    {
        public int Id { get; set; }
    }

    public class DeleteSingleModel
    {
        public int Id { get; set; }
    }
}