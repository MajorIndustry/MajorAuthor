namespace MajorAuthor.Models
{
    public class NotificationViewModel
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsRead { get; set; }
        public string Type { get; set; }
        public string TargetUrl { get; set; }
        public string FormattedDate => CreatedDate.ToString("dd.MM.yyyy HH:mm");
        public bool HasTargetUrl => !string.IsNullOrEmpty(TargetUrl);
    }
}
