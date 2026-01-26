using MajorAuthor.Models;
using System.Threading.Tasks;

namespace MajorAuthor.Services
{
    public interface ICatalogService
    {
        Task<CatalogViewModel> GetAllBooksAsync(string sortBy, string sortOrder, int page, int pageSize, string searchQuery = null, string genres = null, string tags = null);
        Task<CatalogViewModel> SearchBooksAsync(string query, string sortBy, string sortOrder, int page, int pageSize, string genres = null, string tags = null);
        Task<CatalogViewModel> GetAllPoemsAsync(string sortBy, string sortOrder, int page, int pageSize, string searchQuery = null);
        Task<CatalogViewModel> SearchPoemsAsync(string query, string sortBy, string sortOrder, int page, int pageSize);
        Task<CatalogViewModel> GetAllBlogsAsync(string sortBy, string sortOrder, int page, int pageSize, string searchQuery = null);
        Task<CatalogViewModel> SearchBlogsAsync(string query, string sortBy, string sortOrder, int page, int pageSize);
        Task<List<string>> GetAllGenresAsync();
        Task<List<string>> GetAllTagsAsync();
    }
}