// Проект: MajorAuthor.Data
// Файл: Services/BookInvitationService.cs
using MajorAuthor.Data;
using MajorAuthor.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace MajorAuthor.Services
{
    /// <summary>
    /// Реализация службы управления приглашениями соавторов.
    /// </summary>
    public class BookInvitationService : IBookInvitationService
    {
        private readonly MajorAuthorDbContext _context;

        public BookInvitationService(MajorAuthorDbContext context)
        {
            _context = context;
        }

        public async Task AddInvitationAsync(BookInvitation invitation)
        {
            _context.BookInvitations.Add(invitation);
            await _context.SaveChangesAsync();
        }

        public async Task<BookInvitation> GetInvitationByTokenAsync(string token)
        {
            return await _context.BookInvitations.FirstOrDefaultAsync(i => i.InvitationToken == token);
        }

        public async Task UpdateInvitationAsync(BookInvitation invitation)
        {
            _context.Entry(invitation).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
    }
}
