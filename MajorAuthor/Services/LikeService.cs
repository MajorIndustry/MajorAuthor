using MajorAuthor.Data;
using MajorAuthor.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;

// Принцип единственной ответственности (SRP).
// Этот сервис отвечает исключительно за логику, связанную с лайками.
namespace MajorAuthor.Services
{
    public class LikeService : ILikeService
    {
        private readonly MajorAuthorDbContext _context;

        public LikeService(MajorAuthorDbContext context)
        {
            _context = context;
        }

        public async Task<int> GetLikesCountForWorkAsync(int workId, string workType)
        {
            return workType switch
            {
                "Стих" => await _context.PoemLikes.CountAsync(l => l.PoemId == workId),
                "Блог" => await _context.BlogLikes.CountAsync(l => l.BlogId == workId),
                _ => 0,
            };
        }

        public async Task<bool> AddLikeAsync(int workId, string userId, string workType)
        {
            switch (workType)
            {
                case "Стих":
                    var existingPoemLike = await _context.PoemLikes.FirstOrDefaultAsync(l => l.PoemId == workId && l.ApplicationUserId == userId);
                    if (existingPoemLike == null)
                    {
                        await _context.PoemLikes.AddAsync(new PoemLike { PoemId = workId, ApplicationUserId = userId });
                        await _context.SaveChangesAsync();
                        return true;
                    }
                    return false;
                case "Блог":
                    var existingBlogLike = await _context.BlogLikes.FirstOrDefaultAsync(l => l.BlogId == workId && l.ApplicationUserId == userId);
                    if (existingBlogLike == null)
                    {
                        await _context.BlogLikes.AddAsync(new BlogLike { BlogId = workId, ApplicationUserId = userId });
                        await _context.SaveChangesAsync();
                        return true;
                    }
                    return false;
                default:
                    return false;
            }
        }

        public async Task<bool> HasUserLikedWorkAsync(int workId, string userId, string workType)
        {
            return workType switch
            {
                "Стих" => await _context.PoemLikes.AnyAsync(l => l.PoemId == workId && l.ApplicationUserId == userId),
                "Блог" => await _context.BlogLikes.AnyAsync(l => l.BlogId == workId && l.ApplicationUserId == userId),
                _ => false,
            };
        }
    }
}
