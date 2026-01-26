using MajorAuthor.Data;
using MajorAuthor.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MajorAuthor.Services
{
    public class BlogService : IWorkService<Blog>
    {
        private readonly MajorAuthorDbContext _context;
        private readonly INotificationService _notificationService;

        public BlogService(MajorAuthorDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<Blog> GetByIdAsync(int id)
        {
            return await _context.Blogs.FindAsync(id);
        }

        public async Task<List<Blog>> GetAllByAuthorIdAsync(int authorId)
        {
            return await _context.Blogs
                .Where(b => b.AuthorId == authorId)
                .ToListAsync();
        }

        public async Task AddAsync(Blog blog)
        {
            _context.Blogs.Add(blog);
            await _context.SaveChangesAsync();
            try
            {
                // Получаем имя автора
                var author = await _context.Authors
                    .Select(a => new { a.Id, a.PenName })
                    .FirstOrDefaultAsync(a => a.Id == blog.AuthorId);

                if (author != null)
                {
                    // Получаем ID всех подписчиков этого автора
                    var followerUserIds = await _context.Followers
                        .Where(f => f.AuthorId == blog.AuthorId)
                        .Select(f => f.FollowerApplicationUserId)
                        .ToListAsync();

                    // Формируем сообщение и ссылку
                    // Примечание: Убедитесь, что маршрут /Blog/Details/{id} существует в вашем контроллере блогов
                    string message = $"Автор {author.PenName} опубликовал новый блог: «{blog.Title}»";
                    string targetUrl = $"/Read/ReadBlog/{blog.Id}";
                    // Отправляем уведомления
                    foreach (var userId in followerUserIds)
                    {
                        await _notificationService.CreateNotificationAsync(
                            userId,
                            message,
                            "Новый Блог", // Тип уведомления
                            targetUrl
                        );
                    }
                }
            }
            catch
            {
                // Логируем ошибку, но не прерываем работу, если уведомления не ушли.
                // Пользователь не должен получать ошибку 500, если просто сломалась рассылка.
            }
        }

        public async Task UpdateAsync(Blog blog)
        {
            _context.Entry(blog).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Blog blog)
        {
            _context.Blogs.Remove(blog);
            await _context.SaveChangesAsync();
        }

        public async Task<Blog> GetByIdWithCommentsAndLikesAsync(int id)
        {
            // Используем SingleOrDefaultAsync для получения одного элемента
            // и включаем (Include) связанные коллекции Comments и Likes
            // в одном запросе, чтобы избежать N+1 проблемы.
            var blog = await _context.Blogs
                .Include(p => p.Comments)
                .ThenInclude(c => c.ApplicationUser)
                .Include(p => p.Likes)
                .SingleOrDefaultAsync(p => p.Id == id);

            return blog;
        }

        public async Task<List<Blog>> GetAllAsync()
        {
            return await _context.Blogs.ToListAsync(); ;
        }

        public async Task<List<Blog>> GetAsync(Expression<Func<Blog, bool>> predicate)
        {
            return await _context.Blogs.Where(predicate).ToListAsync();
        }
    }
}
