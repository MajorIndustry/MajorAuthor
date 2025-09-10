using MajorAuthor.Data;
using MajorAuthor.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
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
    }
}
