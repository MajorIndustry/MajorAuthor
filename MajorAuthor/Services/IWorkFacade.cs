using System.Threading.Tasks;
using MajorAuthor.Models;

// Принцип единственной ответственности (SRP).
// Этот интерфейс действует как фасад для контроллера,
// предоставляя единую точку входа для всех операций,
// связанных с произведениями (получение, лайки, комментарии).
// Контроллер теперь не знает о конкретных сервисах.
namespace MajorAuthor.Services
{
    public interface IWorkFacade
    {
        Task<(bool success, CommentViewModel updatedComment)> EditCommentAsync(int commentId, string type, string userId, string newCommentText);
        Task<WorkViewModel> GetWorkViewModelAsync(int id, string type);
        Task<(bool success, int newLikesCount, bool isLiked)> ToggleLikeAsync(int id, string type, string userId);
        Task<CommentViewModel> AddCommentAsync(int id, string type, string userId, string commentText);
        Task<bool> DeleteCommentAsync(int commentId, string type, string userId);
        Task<bool> IsUserAuthorAsync(int workId, string workType, string userId);
        Task<bool> HasUserLikedAsync(int workId, string workType, string userId);
        Task<bool> RegisterReadingAsync(int workId, string workType, string userId);
    }
}
