using MajorAuthor.Data.Entities;
using System.Threading.Tasks;

namespace MajorAuthor.Services
{
    public interface IAuthorService
    {
        Task<Author> GetAuthorByUserIdAsync(string userId);
        Task RegisterAuthorAsync(string userId, string penName);
    }
}
