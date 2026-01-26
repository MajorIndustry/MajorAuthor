namespace MajorAuthor.Models
{
    public class UpdateChapterOrderModel
    {
        public int ChapterId { get; set; }
        public string Direction { get; set; } // "up" or "down"
    }
}
