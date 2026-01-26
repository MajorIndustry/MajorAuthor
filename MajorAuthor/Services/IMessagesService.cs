// Проект: MajorAuthor.Services
// Файл: Interfaces/IMessageService.cs
using MajorAuthor.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MajorAuthor.Services
{
    public interface IMessageService
    {
        Task<(bool success, string message, int messageId)> SendMessageAsync(string senderId, string receiverId, string content);
        Task<List<Message>> GetConversationAsync(string currentUserId, string otherUserId);
        Task<List<Message>> GetNewMessagesAsync(string currentUserId, string otherUserId, DateTime lastMessageTime);
        Task<List<Message>> GetRecentConversationsAsync(string userId);
        Task<int> GetUnreadMessagesCountAsync(string userId);
        Task<bool> MarkAsReadAsync(int messageId, string userId);
        Task<bool> MarkConversationAsReadAsync(string currentUserId, string otherUserId);
        Task<bool> CanSendMessageAsync(string senderId, string receiverId);
        Task<List<Message>> GetOlderMessagesAsync(string currentUserId, string otherUserId, DateTime olderThan, int count);
    }
}