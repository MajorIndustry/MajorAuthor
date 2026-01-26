using MajorAuthor.Data.Entities;
using MajorAuthor.Models;
using static MajorAuthor.Services.ChapterService;

namespace MajorAuthor.Services
{
    public interface IChapterService
    {
        Task<bool> SaveChapterSnapshotAsync(ChapterEditorViewModel model, string userId);
        Task<List<Chapter>> GetChaptersByBookIdAsync(int bookId);
        Task<Chapter> GetChapterByIdAsync(int chapterId);
        Task AddChapterAsync(Chapter chapter);
        Task UpdateChapterAsync(Chapter chapter);
        Task UpdateChapterStatusAsync(int chapterId, bool isPublic);
        Task DeleteChapterAsync(int chapterId);
        Task ReorderChaptersAsync(int bookId, List<int> chapterIdsInOrder);
        Task<ImageUploadResult> UploadChapterImageAsync(
    IFormFile image,
    int chapterId,
    int bookId,
    string webRootPath);

        Task<bool> RemoveChapterImageAsync(int pageId, string webRootPath);
        Task<List<Page>> GetChapterPagesAsync(int chapterId);
        Task RenumberChapterPagesAsync(int chapterId);
        Task<string> GetChapterCombinedTextAsync(int chapterId);
    Task<Models.ChapterPagesInfo> GetChapterPagesInfoAsync(int chapterId);
        Task<List<Chapter>> GetPublishedChaptersByBookIdAsync(int id);
    }
}