// Проект: MajorAuthor.Data
// Файл: Services/IBookInvitationService.cs
using MajorAuthor.Data.Entities;
using System.Threading.Tasks;

namespace MajorAuthor.Services
{
    /// <summary>
    /// Интерфейс для службы управления приглашениями соавторов.
    /// </summary>
    public interface IBookInvitationService
    {
        Task<BookInvitation> GetInvitationByTokenAsync(string token);
        Task AddInvitationAsync(BookInvitation invitation);
        Task UpdateInvitationAsync(BookInvitation invitation);
        Task<List<BookInvitation>> GetPendingInvitationsByEmailAsync(string email);
        Task<BookInvitation> CreateInvitationAsync(int bookId, string email, string userId = null);
    }
}