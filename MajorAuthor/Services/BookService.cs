using MajorAuthor.Data;
using MajorAuthor.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MajorAuthor.Services
{
    public class BookService : IBookService
    {
        private readonly MajorAuthorDbContext _context;

        public BookService(MajorAuthorDbContext context)
        {
            _context = context;
        }

        public async Task<Book> GetByIdAsync(int id)
        {
            return await _context.Books.FindAsync(id);
        }

        public async Task<List<Book>> GetAllByAuthorIdAsync(int authorId)
        {
            return await _context.BookAuthors
                .Where(ba => ba.AuthorId == authorId)
                .Select(ba => ba.Book)
                .ToListAsync();
        }

        public async Task AddAsync(Book book)
        {
            _context.Books.Add(book);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Book book)
        {
            _context.Entry(book).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Book book)
        {
            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Связывает книгу с автором, создавая запись в промежуточной таблице BookAuthor.
        /// </summary>
        /// <param name="bookId">Идентификатор книги.</param>
        /// <param name="authorId">Идентификатор автора.</param>
        public async Task LinkBookToAuthorAsync(int bookId, int authorId)
        {
            var bookAuthor = new BookAuthor
            {
                BookId = bookId,
                AuthorId = authorId
            };
            _context.BookAuthors.Add(bookAuthor);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsAuthorOfBookAsync(int bookId, int authorId)
        {
            return await _context.BookAuthors.AnyAsync(ba => ba.BookId == bookId && ba.AuthorId == authorId);
        }
    }
}
