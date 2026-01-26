namespace MajorAuthor.Services
{
    public interface IReadingProgressService
    {
        Task<bool> RegisterChapterReadAsync(int bookId, int chapterId, string userId);
        Task<bool> IsChapterReadAsync(int bookId, int chapterId, string userId);
        Task<double> GetChapterReadProgressAsync(int bookId, int chapterId, string userId);
        Task<double> GetBookReadProgressAsync(int bookId, string userId);
        Task<int> GetReadChaptersCountAsync(int bookId, string userId);
        Task<HashSet<int>> GetReadChapterIdsForBookAsync(int bookId, string userId);

        // НОВЫЕ методы для получения последней прочитанной главы
        Task<int?> GetLastReadChapterIdAsync(int bookId, string userId);

        // Метод для отметки главы как непрочитанной
        Task<bool> MarkChapterUnreadAsync(int bookId, int chapterId, string userId);

        // Метод для сохранения прогресса чтения (по страницам)
        Task<bool> SaveReadingProgressAsync(int bookId, int chapterId, int pageNumber, int totalPages, string userId, string readMode = "pages");

        // Метод для получения списка прочитанных глав с информацией
        Task<List<ChapterProgressInfo>> GetReadingProgressForBookAsync(int bookId, string userId);
        Task<int?> GetContinueReadingChapterIdAsync(int bookId, string userId);
    }
    public class ChapterProgressInfo
    {
        public int ChapterId { get; set; }
        public string ChapterTitle { get; set; }
        public int ChapterOrder { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime LastReadDate { get; set; }
        public double ProgressPercentage { get; set; }
    }
}
