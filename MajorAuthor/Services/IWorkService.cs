using MajorAuthor.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MajorAuthor.Services
{
    public interface IWorkService<T> where T : class
    {
        Task<T> GetByIdAsync(int id);
        Task<List<T>> GetAllByAuthorIdAsync(int authorId);
        Task AddAsync(T work);
        Task UpdateAsync(T work);
        Task DeleteAsync(T work);
    }
}
