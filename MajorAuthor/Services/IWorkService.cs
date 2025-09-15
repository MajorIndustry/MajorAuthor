using MajorAuthor.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

// Принцип разделения интерфейса (ISP) и Принцип единственной ответственности (SRP).
// Этот интерфейс содержит только базовые операции CRUD для любого типа произведения.
// Это позволяет разным типам произведений (книга, стих, блог) использовать только те методы,
// которые им действительно нужны.
namespace MajorAuthor.Services
{
    public interface IWorkService<T> where T : class
    {
        Task<T> GetByIdAsync(int id);
        Task<List<T>> GetAllAsync();
        Task<List<T>> GetAllByAuthorIdAsync(int authorId);
        Task<List<T>> GetAsync(Expression<Func<T, bool>> predicate);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task<T> GetByIdWithCommentsAndLikesAsync(int id);
    }
}
