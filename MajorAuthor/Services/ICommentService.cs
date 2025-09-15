using MajorAuthor.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

// Принцип разделения интерфейса (ISP).
// Этот интерфейс предназначен только для работы с комментариями,
// отделяя эту функциональность от других сервисов.
namespace MajorAuthor.Services
{
    public interface ICommentService
    {
        Task<List<CommentViewModel>> GetCommentsForWorkAsync(int workId, string workType);
        Task<CommentViewModel> AddCommentAsync(int workId, string userId, string workType, string commentText);
        Task<bool> DeleteCommentAsync(int commentId, string type, string userId);
    }
}
