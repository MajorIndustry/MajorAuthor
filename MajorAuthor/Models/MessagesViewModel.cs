using MajorAuthor.Data.Entities;

namespace MajorAuthor.Models
{
    public class MessagesViewModel
    {
        public List<Message> RecentConversations { get; set; }
        public int UnreadCount { get; set; }
    }
}
