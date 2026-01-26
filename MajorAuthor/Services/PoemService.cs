// Services/PoemService.cs
using MajorAuthor.Data;
using MajorAuthor.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MajorAuthor.Services
{
    public class PoemService : IWorkService<Poem>
    {
        private readonly MajorAuthorDbContext _context;
        private readonly INotificationService _notificationService;

        public PoemService(MajorAuthorDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<Poem> GetByIdAsync(int id)
        {
            return await _context.Poems.FindAsync(id);
        }

        public async Task<List<Poem>> GetAllByAuthorIdAsync(int authorId)
        {
            return await _context.Poems
                .Where(p => p.AuthorId == authorId)
                .ToListAsync();
        }

        public async Task AddAsync(Poem poem)
        {
            poem.ContentHash = ComputeContentHash(poem.Content);

            _context.Poems.Add(poem);
            await _context.SaveChangesAsync();

            try
            {
                var author = await _context.Authors
                    .Select(a => new { a.Id, a.PenName })
                    .FirstOrDefaultAsync(a => a.Id == poem.AuthorId);

                if (author != null)
                {
                    var followerUserIds = await _context.Followers
                        .Where(f => f.AuthorId == poem.AuthorId)
                        .Select(f => f.FollowerApplicationUserId)
                        .ToListAsync();

                    string message = $"Автор {author.PenName} опубликовал новый стих: «{poem.Title}»";
                    string targetUrl = $"/Read/ReadPoem/{poem.Id}";

                    foreach (var userId in followerUserIds)
                    {
                        await _notificationService.CreateNotificationAsync(
                            userId,
                            message,
                            "Новый стих",
                            targetUrl
                        );
                    }
                }
            }
            catch
            {
                // Игнорируем ошибки рассылки
            }
        }

        public async Task UpdateAsync(Poem poem)
        {
            poem.ContentHash = ComputeContentHash(poem.Content);

            _context.Entry(poem).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Poem poem)
        {
            _context.Poems.Remove(poem);
            await _context.SaveChangesAsync();
        }

        public async Task<Poem> GetByIdWithCommentsAndLikesAsync(int id)
        {
            return await _context.Poems
                .Include(p => p.Comments)
                .ThenInclude(c => c.ApplicationUser)
                .Include(p => p.Likes)
                .SingleOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<Poem>> GetAllAsync()
        {
            return await _context.Poems.ToListAsync();
        }

        public async Task<List<Poem>> GetAsync(Expression<Func<Poem, bool>> predicate)
        {
            return await _context.Poems.Include(p => p.Author).Where(predicate).ToListAsync();
        }
        private string ComputeContentHash(string content)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(content);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToHexString(hash).ToLower();
        }
    }
}