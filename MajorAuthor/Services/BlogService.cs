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

        public BlogService(MajorAuthorDbContext context)
        {
            _context = context;
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
