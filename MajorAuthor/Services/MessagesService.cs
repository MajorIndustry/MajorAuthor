using MajorAuthor.Data;
using MajorAuthor.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MajorAuthor.Services
{
    public class MessageService : IMessageService
    {
        private readonly MajorAuthorDbContext _context;
        private readonly IAuthorService _authorService;

        public MessageService(MajorAuthorDbContext context, IAuthorService authorService)
        {
            _context = context;
            _authorService = authorService;
        }

        public async Task<(bool success, string message, int messageId)> SendMessageAsync(string senderId, string receiverId, string content)
        {
            try
            {
                // Проверяем, могут ли пользователи обмениваться сообщениями
                var canSend = await CanSendMessageAsync(senderId, receiverId);
                if (!canSend)
                    return (false, "Только авторы могут отправлять сообщения другим авторам", 0);

                // Проверяем существование получателя
                var receiver = await _context.Users.FindAsync(receiverId);
                if (receiver == null)
                    return (false, "Получатель не найден", 0);

                // Проверяем длину сообщения
                if (string.IsNullOrWhiteSpace(content) || content.Length > 1000)
                    return (false, "Сообщение должно содержать от 1 до 1000 символов", 0);

                var newMessage = new Message
                {
                    SenderId = senderId,
                    ReceiverId = receiverId,
                    Content = content.Trim(),
                    SentDate = DateTime.UtcNow,
                    IsRead = false
                };

                _context.Messages.Add(newMessage);
                await _context.SaveChangesAsync();

                return (true, "Сообщение отправлено", newMessage.Id);
            }
            catch (DbUpdateException dbEx)
            {
                return (false, $"Ошибка базы данных: {dbEx.InnerException?.Message ?? dbEx.Message}", 0);
            }
            catch (Exception ex)
            {
                return (false, $"Ошибка при отправке сообщения: {ex.Message}", 0);
            }
        }

        public async Task<List<Message>> GetConversationAsync(string currentUserId, string otherUserId)
        {
            return await _context.Messages
                .Where(m => (m.SenderId == currentUserId && m.ReceiverId == otherUserId) ||
                           (m.SenderId == otherUserId && m.ReceiverId == currentUserId))
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .OrderBy(m => m.SentDate)
                .ToListAsync();
        }

        public async Task<List<Message>> GetNewMessagesAsync(string currentUserId, string otherUserId, DateTime lastMessageTime)
        {
            return await _context.Messages
                .Where(m => (m.SenderId == currentUserId && m.ReceiverId == otherUserId) ||
                           (m.SenderId == otherUserId && m.ReceiverId == currentUserId))
                .Where(m => m.SentDate > lastMessageTime)
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .OrderBy(m => m.SentDate)
                .ToListAsync();
        }

        public async Task<List<Message>> GetRecentConversationsAsync(string userId)
        {
            // Исправленный запрос - сначала получаем ID последних сообщений, затем полные данные
            var conversationPartners = await _context.Messages.Include(m=>m.Sender).ThenInclude(s=>s.AuthorProfile).Include(m => m.Receiver).ThenInclude(r => r.AuthorProfile)
                .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                .Select(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
                .Distinct()
                .ToListAsync();

            var recentMessages = new List<Message>();

            foreach (var partnerId in conversationPartners)
            {
                var lastMessage = await _context.Messages.Include(m => m.Sender).ThenInclude(s => s.AuthorProfile).Include(m => m.Receiver).ThenInclude(r => r.AuthorProfile)
                    .Where(m => (m.SenderId == userId && m.ReceiverId == partnerId) ||
                               (m.SenderId == partnerId && m.ReceiverId == userId))
                    .Include(m => m.Sender)
                    .Include(m => m.Receiver)
                    .OrderByDescending(m => m.SentDate)
                    .FirstOrDefaultAsync();

                if (lastMessage != null)
                {
                    recentMessages.Add(lastMessage);
                }
            }

            return recentMessages.OrderByDescending(m => m.SentDate).ToList();
        }

        public async Task<int> GetUnreadMessagesCountAsync(string userId)
        {
            try
            {
                var count = await _context.Messages
                    .CountAsync(m => m.ReceiverId == userId && !m.IsRead);

                Console.WriteLine($"GetUnreadMessagesCountAsync for {userId}: {count}");
                return count;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetUnreadMessagesCountAsync: {ex.Message}");
                return 0;
            }
        }

        public async Task<bool> MarkAsReadAsync(int messageId, string userId)
        {
            var message = await _context.Messages
                .FirstOrDefaultAsync(m => m.Id == messageId && m.ReceiverId == userId);

            if (message != null && !message.IsRead)
            {
                message.IsRead = true;
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<bool> MarkConversationAsReadAsync(string currentUserId, string otherUserId)
        {
            try
            {
                Console.WriteLine($"MarkConversationAsReadAsync: currentUserId={currentUserId}, otherUserId={otherUserId}");

                // Проверяем существование пользователей
                var currentUser = await _context.Users.FindAsync(currentUserId);
                var otherUser = await _context.Users.FindAsync(otherUserId);

                if (currentUser == null || otherUser == null)
                {
                    Console.WriteLine("User not found");
                    return false;
                }

                var unreadMessages = await _context.Messages
                    .Where(m => m.SenderId == otherUserId && m.ReceiverId == currentUserId && !m.IsRead)
                    .ToListAsync();

                Console.WriteLine($"Found {unreadMessages.Count} unread messages");

                if (unreadMessages.Any())
                {
                    foreach (var message in unreadMessages)
                    {
                        message.IsRead = true;
                        Console.WriteLine($"Marked message {message.Id} as read");
                    }

                    var saved = await _context.SaveChangesAsync();
                    Console.WriteLine($"Saved {saved} changes to database");
                    return saved > 0;
                }

                Console.WriteLine("No unread messages to mark");
                return true; // Возвращаем true даже если не было сообщений для пометки
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in MarkConversationAsReadAsync: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        public async Task<bool> CanSendMessageAsync(string senderId, string receiverId)
        {
            // Проверяем, что отправитель - автор
            var senderAuthor = await _authorService.GetAuthorByUserIdAsync(senderId);
            if (senderAuthor == null)
                return false;

            // Проверяем, что получатель - автор
            var receiverAuthor = await _authorService.GetAuthorByUserIdAsync(receiverId);
            if (receiverAuthor == null)
                return false;

            // Нельзя отправлять сообщения самому себе
            if (senderId == receiverId)
                return false;

            return true;
        }

        // Помести этот новый метод внутрь класса MessageService (файл MessagesService.cs)

        public async Task<List<Message>> GetOlderMessagesAsync(string currentUserId, string otherUserId, DateTime olderThan, int count)
        {
            try
            {
                return await _context.Messages
                    .Where(m => (m.SenderId == currentUserId && m.ReceiverId == otherUserId) ||
                               (m.SenderId == otherUserId && m.ReceiverId == currentUserId))
                    .Where(m => m.SentDate < olderThan) // Ключевое отличие: сообщения СТАРШЕ (раньше) чем olderThan
                    .Include(m => m.Sender)
                    .Include(m => m.Receiver)
                    .OrderByDescending(m => m.SentDate) // Берем последние N из тех, что старше
                    .Take(count)
                    .OrderBy(m => m.SentDate) // Сортируем в прямом порядке для отображения
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetOlderMessagesAsync: {ex.Message}");
                return new List<Message>();
            }
        }
    }
}