using MajorAuthor.Data.Entities;

namespace MajorAuthor.Models
{
    public class ConversationViewModel
    {
        public List<Message> Messages { get; set; }
        public string OtherUserId { get; set; }
        public string OtherUserName { get; set; }
        public string OtherUserPhotoUrl { get; set; }
        public string CurrentUserId { get; set; }
    }
}
