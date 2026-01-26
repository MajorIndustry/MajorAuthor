using MajorAuthor.Data.Entities;

namespace MajorAuthor.Models
{
    // В Models/ViewModels.cs добавьте эти модели

    public class ChapterReadViewModel
    {
        public Book Book { get; set; }
        public Chapter CurrentChapter { get; set; }
        public ChapterPagesInfo ChapterPages { get; set; }
        public List<Chapter> AllChapters { get; set; }
        public Chapter PreviousChapter { get; set; }
        public Chapter NextChapter { get; set; }
        public bool IsAuthor { get; set; }
        public bool HasLiked { get; set; }
        public bool IsInFavorites { get; set; }
        public bool IsChapterRead { get; set; }
        public bool AllowDownload { get; set; }
        public bool EnableTTS { get; set; }
        public string ReadMode { get; set; } = "pages"; // "pages" или "continuous"
        public int FontSize { get; set; } = 16;
        public double BookProgress { get; set; }
        public HashSet<int> ReadChapterIds { get; set; } = new HashSet<int>();
    }

    public class ChapterPagesInfo
    {
        public string TextContent { get; set; } = "";
        public List<ChapterImagePage> ImagePages { get; set; } = new List<ChapterImagePage>();
    }

    public class ChapterReadingRecord
    {
        public int BookId { get; set; }
        public int ChapterId { get; set; }
        public DateTime ReadDate { get; set; }
        public int LastPageNumber { get; set; }
        public decimal ReadPercentage { get; set; }
    }
}
