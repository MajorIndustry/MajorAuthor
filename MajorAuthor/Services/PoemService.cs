using MajorAuthor.Data;
using MajorAuthor.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MajorAuthor.Services
{
    public class PoemService : IWorkService<Poem>
    {
        private readonly MajorAuthorDbContext _context;

        public PoemService(MajorAuthorDbContext context)
        {
            _context = context;
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
            _context.Poems.Add(poem);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Poem poem)
        {
            _context.Entry(poem).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Poem poem)
        {
            _context.Poems.Remove(poem);
            await _context.SaveChangesAsync();
        }
    }
}
