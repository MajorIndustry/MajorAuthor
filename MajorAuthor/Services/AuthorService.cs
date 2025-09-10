using MajorAuthor.Data;
using MajorAuthor.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace MajorAuthor.Services
{
    public class AuthorService : IAuthorService
    {
        private readonly MajorAuthorDbContext _context;

        public AuthorService(MajorAuthorDbContext context)
        {
            _context = context;
        }

        public async Task<Author> GetAuthorByUserIdAsync(string userId)
        {
            return await _context.Authors.FirstOrDefaultAsync(a => a.ApplicationUserId == userId);
        }

        // Обновлено: теперь принимает penName в качестве параметра
        public async Task RegisterAuthorAsync(string userId, string penName)
        {
            var existingAuthor = await GetAuthorByUserIdAsync(userId);
            if (existingAuthor == null)
            {
                var newAuthor = new Author
                {
                    ApplicationUserId = userId,
                    PenName = penName, // Используем значение, предоставленное пользователем
                    FullName = string.Empty,
                    AuthorProfileCreationDate = System.DateTime.UtcNow,
                    Description = string.Empty
                };
                _context.Authors.Add(newAuthor);
                await _context.SaveChangesAsync();
            }
        }
    }
}
