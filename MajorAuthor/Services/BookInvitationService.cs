using MajorAuthor.Data;
using MajorAuthor.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MajorAuthor.Services
{
    public class BookInvitationService : IBookInvitationService
    {
        private readonly MajorAuthorDbContext _context;

        public BookInvitationService(MajorAuthorDbContext context)
        {
            _context = context;
        }

        public async Task<BookInvitation> GetInvitationByTokenAsync(string token)
        {
            return await _context.BookInvitations
                .Include(i => i.Book)
                .Include(i => i.InviteeUser)
                .FirstOrDefaultAsync(i => i.InvitationToken == token);
        }

        public async Task AddInvitationAsync(BookInvitation invitation)
        {
            _context.BookInvitations.Add(invitation);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateInvitationAsync(BookInvitation invitation)
        {
            _context.BookInvitations.Update(invitation);
            await _context.SaveChangesAsync();
        }

        public async Task<List<BookInvitation>> GetPendingInvitationsByEmailAsync(string email)
        {
            return await _context.BookInvitations
                .Where(i => i.InviteeEmail == email && !i.IsAccepted && !i.IsExpired)
                .Include(i => i.Book)
                .ToListAsync();
        }

        public async Task<BookInvitation> CreateInvitationAsync(int bookId, string email, string userId = null)
        {
            var token = Guid.NewGuid().ToString();

            var invitation = new BookInvitation
            {
                BookId = bookId,
                InviteeEmail = email,
                InviteeUserId = userId,
                InvitationToken = token,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                InvitationType = string.IsNullOrEmpty(userId) ? InvitationType.ReaderToAuthor : InvitationType.CoAuthor
            };

            await AddInvitationAsync(invitation);
            return invitation;
        }
    }
}