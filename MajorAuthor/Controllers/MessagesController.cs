using MajorAuthor.Data.Entities;
using MajorAuthor.Models;
using MajorAuthor.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MajorAuthor.Controllers
{
    [Authorize]
    public class MessagesController : Controller
    {
        private readonly IMessageService _messageService;
        private readonly IAuthorService _authorService;

        public MessagesController(IMessageService messageService, IAuthorService authorService)
        {
            _messageService = messageService;
            _authorService = authorService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var conversations = await _messageService.GetRecentConversationsAsync(currentUserId);

            var viewModel = new MessagesViewModel
            {
                RecentConversations = conversations,
                UnreadCount = await _messageService.GetUnreadMessagesCountAsync(currentUserId)
            };

            return View(viewModel);
        }

        // В файле MessagesController.cs
        [HttpGet]
        public async Task<IActionResult> Conversation(string userId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // 1. Проверяем, может ли пользователь общаться с этим автором
            var canMessage = await _messageService.CanSendMessageAsync(currentUserId, userId);
            if (!canMessage)
            {
                TempData["Error"] = "Вы не можете начать диалог с этим пользователем.";
                return RedirectToAction(nameof(Index));
            }

            // 2. Получаем автора и его псевдоним
            var otherAuthor = await _authorService.GetAuthorByUserIdAsync(userId);
            if (otherAuthor == null)
            {
                TempData["Error"] = "Собеседник не найден.";
                return RedirectToAction(nameof(Index));
            }

            // 3. Используем псевдоним автора, если он есть, иначе используем имя пользователя
            var displayName = otherAuthor.PenName ?? otherAuthor.ApplicationUser.UserName;

            var viewModel = new ConversationViewModel
            {
                OtherUserId = userId,
                OtherUserName = displayName, // Теперь здесь будет псевдоним автора
                Messages = new List<Message>()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Валидация
            if (string.IsNullOrEmpty(request.ReceiverId) || string.IsNullOrEmpty(request.Content))
            {
                return Json(new { success = false, message = "Неверные данные сообщения" });
            }

            var result = await _messageService.SendMessageAsync(currentUserId, request.ReceiverId, request.Content);

            if (result.success)
            {
                return Json(new { success = true, message = result.message, messageId = result.messageId });
            }

            return Json(new { success = false, message = result.message });
        }

        [HttpGet]
        public async Task<IActionResult> GetNewMessages(string otherUserId, string lastMessageTime)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            DateTime lastTime;

            // ИСПРАВЛЕНИЕ: JavaScript передает миллисенды в виде строки.
            // Мы должны парсить 'long', а не 'DateTime'.
            if (long.TryParse(lastMessageTime, out long lastTimeMilliseconds))
            {
                // Конвертируем миллисекунды эпохи Unix в DateTime
                lastTime = DateTime.UnixEpoch.AddMilliseconds(lastTimeMilliseconds);
            }
            else
            {
                // Если по какой-то причине пришел мусор, 
                // устанавливаем текущее время, чтобы не загружать всю историю.
                lastTime = DateTime.UtcNow;
            }

            // Получаем новые сообщения, НАЧИНАЯ С lastTime
            // (Сервисный метод GetNewMessagesAsync уже написан правильно: m.SentDate > lastMessageTime)
            var newMessages = await _messageService.GetNewMessagesAsync(currentUserId, otherUserId, lastTime);

            var result = newMessages.Select(m => new
            {
                id = m.Id,
                content = m.Content,
                senderId = m.SenderId,
                sentDate = m.SentDate,
                isRead = m.IsRead
            }).ToList();

            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetRecentMessages(string otherUserId, int count = 3)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Получаем все сообщения из диалога
            var allMessages = await _messageService.GetConversationAsync(currentUserId, otherUserId);

            // Берем последние count сообщений
            var recentMessages = allMessages
                .OrderByDescending(m => m.SentDate)
                .Take(count)
                .OrderBy(m => m.SentDate)
                .Select(m => new
                {
                    id = m.Id,
                    content = m.Content,
                    senderId = m.SenderId,
                    sentDate = m.SentDate,
                    isRead = m.IsRead
                })
                .ToList();

            return Json(recentMessages);
        }

        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                Console.WriteLine($"GetUnreadCount for user: {currentUserId}");

                var count = await _messageService.GetUnreadMessagesCountAsync(currentUserId);

                Console.WriteLine($"Unread count: {count}");
                return Json(new { unreadCount = count });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting unread count: {ex.Message}");
                return Json(new { unreadCount = 0 });
            }
        }
        // Помести этот новый метод (экшен) внутрь класса MessagesController (файл MessagesController.cs)
        // Например, после метода GetRecentMessages

        [HttpGet]
        public async Task<IActionResult> GetOlderMessages(string otherUserId, long olderThan, int count = 50)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // JavaScript передает время в миллисекундах (из .getTime())
            // Преобразуем его в DateTime
            var olderThanTime = DateTime.UnixEpoch.AddMilliseconds(olderThan);

            var olderMessages = await _messageService.GetOlderMessagesAsync(currentUserId, otherUserId, olderThanTime, count);

            var result = olderMessages.Select(m => new
            {
                id = m.Id,
                content = m.Content,
                senderId = m.SenderId,
                sentDate = m.SentDate,
                isRead = m.IsRead
            }).ToList();

            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> MarkConversationAsRead([FromBody] MarkConversationReadRequest request)
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                Console.WriteLine($"Marking conversation as read: currentUserId={currentUserId}, otherUserId={request.OtherUserId}");

                var result = await _messageService.MarkConversationAsReadAsync(currentUserId, request.OtherUserId);

                if (result)
                {
                    Console.WriteLine("Successfully marked conversation as read");
                    return Ok(new { success = true });
                }
                else
                {
                    Console.WriteLine("No unread messages found to mark as read");
                    // Возвращаем success=true даже если не было непрочитанных сообщений
                    // Это нормальная ситуация, когда сообщения уже прочитаны
                    return Ok(new { success = true });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error marking conversation as read: {ex.Message}");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        public class MarkConversationReadRequest
        {
            public string OtherUserId { get; set; }
        }


        public class SendMessageRequest
        {
            public string ReceiverId { get; set; }
            public string Content { get; set; }
        }
    }
}