namespace MajorAuthor.Models
{
    public class ConversationPreviewModel
    {
        public string PartnerId { get; set; }
        public string PartnerName { get; set; }
        public string PartnerPhotoUrl { get; set; }
        public string LastMessage { get; set; }
        public System.DateTime LastMessageDate { get; set; }
        public bool IsLastMessageFromPartner { get; set; }
        public bool IsLastMessageUnread { get; set; }
    }
}
