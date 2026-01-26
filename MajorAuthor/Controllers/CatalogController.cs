using MajorAuthor.Models;
using MajorAuthor.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace MajorAuthor.Controllers
{
    public class CatalogController : Controller
    {
        private readonly ICatalogService _catalogService;

        public CatalogController(ICatalogService catalogService)
        {
            _catalogService = catalogService;
        }

        [HttpGet]
        public async Task<IActionResult> AllBlogs(string sortBy = "rating", string sortOrder = "desc", int page = 1, int pageSize = 12, string searchQuery = null)
        {
            var viewModel = await _catalogService.GetAllBlogsAsync(sortBy, sortOrder, page, pageSize, searchQuery);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_BlogListPartial", viewModel);
            }

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> AllPoems(string sortBy = "rating", string sortOrder = "desc", int page = 1, int pageSize = 12, string searchQuery = null)
        {
            var viewModel = await _catalogService.GetAllPoemsAsync(sortBy, sortOrder, page, pageSize, searchQuery);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_PoemListPartial", viewModel);
            }

            return View(viewModel);
        }

        // CatalogController.cs - добавим параметры для фильтрации
        [HttpGet]
        public async Task<IActionResult> AllBooks(string sortBy = "rating", string sortOrder = "desc", int page = 1, int pageSize = 12, string searchQuery = null, string genres = null, string tags = null)
        {
            var viewModel = await _catalogService.GetAllBooksAsync(sortBy, sortOrder, page, pageSize, searchQuery, genres, tags);

            // Получаем списки всех жанров и тегов для фильтров ИЗ БАЗЫ ДАННЫХ
            ViewBag.Genres = await _catalogService.GetAllGenresAsync();
            ViewBag.Tags = await _catalogService.GetAllTagsAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_BookListPartial", viewModel);
            }

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> SearchBooks(string query, string sortBy = "rating", string sortOrder = "desc", int page = 1, int pageSize = 12, string genres = null, string tags = null)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                var viewModel = await _catalogService.GetAllBooksAsync(sortBy, sortOrder, page, pageSize, null, genres, tags);
                return PartialView("_BookListPartial", viewModel);
            }

            var searchViewModel = await _catalogService.SearchBooksAsync(query, sortBy, sortOrder, page, pageSize, genres, tags);
            return PartialView("_BookListPartial", searchViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> SearchBlogs(string query, string sortBy = "rating", string sortOrder = "desc", int page = 1, int pageSize = 12)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                var viewModel = await _catalogService.GetAllBlogsAsync(sortBy, sortOrder, page, pageSize);
                return PartialView("_BlogListPartial", viewModel);
            }

            var searchViewModel = await _catalogService.SearchBlogsAsync(query, sortBy, sortOrder, page, pageSize);
            return PartialView("_BlogListPartial", searchViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> SearchPoems(string query, string sortBy = "rating", string sortOrder = "desc", int page = 1, int pageSize = 12)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                var viewModel = await _catalogService.GetAllPoemsAsync(sortBy, sortOrder, page, pageSize);
                return PartialView("_PoemListPartial", viewModel);
            }

            var searchViewModel = await _catalogService.SearchPoemsAsync(query, sortBy, sortOrder, page, pageSize);
            return PartialView("_PoemListPartial", searchViewModel);
        }
    }
}