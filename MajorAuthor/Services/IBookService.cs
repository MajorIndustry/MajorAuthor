using MajorAuthor.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajorAuthor.Services
{
    public interface IBookService : IWorkService<Book>
    {
        Task LinkBookToAuthorAsync(int id1, int id2);
        Task<bool> IsAuthorOfBookAsync(int bookId, int authorId);
    }
}
