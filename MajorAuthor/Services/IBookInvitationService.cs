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
        Task AddInvitationAsync(BookInvitation invitation);
        Task<BookInvitation> GetInvitationByTokenAsync(string token);
        Task UpdateInvitationAsync(BookInvitation invitation);
    }
}