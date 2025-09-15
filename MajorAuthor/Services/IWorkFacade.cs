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
        Task<WorkViewModel> GetWorkViewModelAsync(int id, string type);
        Task<(bool success, int newLikesCount)> ToggleLikeAsync(int id, string type, string userId);
        Task<CommentViewModel> AddCommentAsync(int id, string type, string userId, string commentText);
        Task<bool> DeleteCommentAsync(int commentId, string type, string userId);
    }
}
