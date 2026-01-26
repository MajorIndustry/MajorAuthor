using MajorAuthor.Data.Entities;
using MajorAuthor.Models.ViewModels;
using MajorAuthor.Services;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using MajorAuthor.Data;
using Microsoft.EntityFrameworkCore;
using MajorAuthor.Models;
using System.Text.Json; // Для ToListAsync и Include

namespace MajorAuthor.Controllers
{
    /// <summary>
    /// Контроллер для обработки поисковых запросов по книгам, стихам, блогам и авторам.
    /// </summary>
    public class SearchController : Controller
    {
        private readonly IBookService _bookService;
        private readonly IWorkService<Blog> _blogService;
        private readonly IAuthorService _authorService;
        private readonly MajorAuthorDbContext _context;
        private readonly IWorkService<Poem> _poemService;

        // Конструктор: Внедрение зависимостей
        public SearchController(IBookService bookService,
                                IWorkService<Blog> blogService,
                                IAuthorService authorService,
                                MajorAuthorDbContext context,
                                IWorkService<Poem> poemService)
        {
            _bookService = bookService;
            _blogService = blogService;
            _authorService = authorService;
            _context = context;
            _poemService = poemService;
        }

        /// <summary>
        /// Основное действие для отображения страницы поиска с поддержкой вкладок и сортировки.
        /// GET /Search?query={term}&tab={active_tab}&workSortBy={sort_type}&authorSortBy={author_sort_type}
        /// </summary>
        public async Task<IActionResult> Index(
            string query,
            string tab = "all",
            string workSortBy = "title", // 💡 Сортировка произведений
            string authorSortBy = "name") // 💡 Сортировка авторов
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return View(new SearchViewModel { SearchQuery = "", ActiveTab = "all", WorkSortBy = "title", AuthorSortBy = "name" });
            }

            var viewModel = new SearchViewModel
            {
                SearchQuery = query,
                ActiveTab = tab.ToLower(),
                WorkSortBy = workSortBy.ToLower(),
                AuthorSortBy = authorSortBy.ToLower()
            };

            var lowerQuery = query.ToLower();

            // --- 1. Поиск и маппинг Стихов ---
            var poems = await _context.Poems
                .Include(p => p.Author)
                .Where(p => p.Title.ToLower().Contains(lowerQuery))
                .ToListAsync();

            var poemWorkList = poems.Select(p => new SearchResultPoemViewModel
            {
                Id = p.Id,
                Title = p.Title,
                AuthorName = p.Author != null ? (p.Author.PenName ?? p.Author.ApplicationUser.FullName) : "Автор неизвестен",
                ContentSnippet = p.Content != null && p.Content.Length > 100 ? p.Content.Substring(0, 100) + "..." : p.Content ?? string.Empty,
                ViewsCount = p.ViewsCount,
                LikesCount = p.LikesCount
            }).ToList();
            viewModel.Poems.AddRange(poemWorkList);


            // --- 2. Поиск и маппинг Книг ---
            var books = await _bookService.GetAsync(b => b.Title.ToLower().Contains(lowerQuery));

            viewModel.Books.AddRange(books.Select(b => new SearchResultBookViewModel
            {
                Id = b.Id,
                Title = b.Title,
                CoverImageUrl = b.CoverImageUrl,
                IsAdultContent = b.IsAdultContent,
                AuthorName = "Неизвестный автор",
                ViewsCount = b.ReadsCount,
                LikesCount = b.LikesCount
            }));


            // --- 3. Поиск и маппинг Блогов ---
            var blogs = await _blogService.GetAsync(b => b.Title.ToLower().Contains(lowerQuery));

            viewModel.Blogs.AddRange(blogs.Select(b => new SearchResultBlogViewModel
            {
                Id = b.Id,
                Title = b.Title,
                AuthorName = "Неизвестный автор",
                ContentSnippet = b.Content.Length > 100 ? b.Content.Substring(0, 100) + "..." : b.Content,
                ViewsCount = b.ViewsCount,
                LikesCount = b.LikesCount
            }));


            // --- 4. Объединение в AllWorks (для вкладки "Все") ---
            // 💡 ВАЖНО: Маппинг всех произведений в унифицированный список
            viewModel.AllWorks.AddRange(poemWorkList.Select(p => new SearchResultWorkViewModel
            {
                Id = p.Id,
                Title = p.Title,
                AuthorName = p.AuthorName,
                ContentSnippet = p.ContentSnippet,
                ViewsCount = p.ViewsCount,
                LikesCount = p.LikesCount,
                Type = "Poem",
                TargetAction = "ReadPoem"
            }));

            viewModel.AllWorks.AddRange(viewModel.Books.Select(b => new SearchResultWorkViewModel
            {
                Id = b.Id,
                Title = b.Title,
                AuthorName = b.AuthorName,
                ContentSnippet = "Книга",
                ViewsCount = b.ViewsCount,
                LikesCount = b.LikesCount,
                Type = "Book",
                TargetAction = "ReadBook"
            }));

            viewModel.AllWorks.AddRange(viewModel.Blogs.Select(b => new SearchResultWorkViewModel
            {
                Id = b.Id,
                Title = b.Title,
                AuthorName = b.AuthorName,
                ContentSnippet = b.ContentSnippet,
                ViewsCount = b.ViewsCount,
                LikesCount = b.LikesCount,
                Type = "Blog",
                TargetAction = "ReadBlog"
            }));


            // --- 5. Поиск и маппинг Авторов (с учетом подписчиков) ---
            var authors = await _authorService.SearchAuthorsAsync(query);

            viewModel.Authors.AddRange(authors.Select(a => new SearchResultAuthorViewModel
            {
                Id = a.Id,
                Name = string.IsNullOrWhiteSpace(a.PenName) ? a.ApplicationUser.FullName : a.PenName,
                PhotoUrl = a.ApplicationUser.ProfilePictureUrl,
                PenName = a.PenName,
                FullName = a.ApplicationUser.FullName,
                SubscribersCount = a.Followers.Count() // 💡 Предполагаем, что это поле доступно
            }));


            // --- 6. Применение Сортировки к Произведениям ---
            var allWorksQuery = viewModel.AllWorks.AsQueryable();
            var poemQuery = viewModel.Poems.AsQueryable();
            var bookQuery = viewModel.Books.AsQueryable();
            var blogQuery = viewModel.Blogs.AsQueryable();

            // Сортировка AllWorks (для вкладки "Все")
            switch (viewModel.WorkSortBy)
            {
                case "views":
                    allWorksQuery = allWorksQuery.OrderByDescending(w => w.ViewsCount);
                    break;
                case "likes":
                    allWorksQuery = allWorksQuery.OrderByDescending(w => w.LikesCount);
                    break;
                case "title":
                default:
                    allWorksQuery = allWorksQuery.OrderBy(w => w.Title);
                    break;
            }
            viewModel.AllWorks = allWorksQuery.ToList();

            // Сортировка отдельных категорий (для отдельных вкладок)
            if (viewModel.ActiveTab != "all")
            {
                switch (viewModel.WorkSortBy)
                {
                    case "views":
                        viewModel.Poems = poemQuery.OrderByDescending(p => p.ViewsCount).ToList();
                        viewModel.Books = bookQuery.OrderByDescending(b => b.ViewsCount).ToList();
                        viewModel.Blogs = blogQuery.OrderByDescending(b => b.ViewsCount).ToList();
                        break;
                    case "likes":
                        viewModel.Poems = poemQuery.OrderByDescending(p => p.LikesCount).ToList();
                        viewModel.Books = bookQuery.OrderByDescending(b => b.LikesCount).ToList();
                        viewModel.Blogs = blogQuery.OrderByDescending(b => b.LikesCount).ToList();
                        break;
                    case "title": // 💡 Алфавитная
                    default:
                        viewModel.Poems = poemQuery.OrderBy(p => p.Title).ToList();
                        viewModel.Books = bookQuery.OrderBy(b => b.Title).ToList();
                        viewModel.Blogs = blogQuery.OrderBy(b => b.Title).ToList();
                        break;
                }
            }
            // Если вкладка "all", то отдельные списки сортируются по названию по умолчанию
            else
            {
                viewModel.Poems = poemQuery.OrderBy(p => p.Title).ToList();
                viewModel.Books = bookQuery.OrderBy(b => b.Title).ToList();
                viewModel.Blogs = blogQuery.OrderBy(b => b.Title).ToList();
            }


            // --- 7. Применение Сортировки к Авторам ---
            var authorQuery = viewModel.Authors.AsQueryable();

            switch (viewModel.AuthorSortBy)
            {
                case "subscribers":
                    authorQuery = authorQuery.OrderByDescending(a => a.SubscribersCount);
                    break;
                case "name": // 💡 Алфавитная сортировка
                default:
                    authorQuery = authorQuery.OrderBy(a => a.Name);
                    break;
            }
            viewModel.Authors = authorQuery.ToList();

            return View(viewModel);
        }
        [HttpGet]
        public async Task<IActionResult> Autocomplete(string query, int limit = 3)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            {
                return Json(new { works = new List<object>(), authors = new List<object>() });
            }

            var lowerQuery = query.ToLower();
            var results = new { works = new List<object>(), authors = new List<object>() };

            try
            {
                // --- Поиск произведений (сортировка по лайкам) ---
                var works = new List<object>();

                // Стихи
                var poems = await _context.Poems
                    .Include(p => p.Author)
                    .Where(p => p.Title.ToLower().Contains(lowerQuery) ||
                               (p.Content != null && p.Content.ToLower().Contains(lowerQuery)))
                    .OrderByDescending(p => p.LikesCount)
                    .Take(limit)
                    .Select(p => new
                    {
                        id = p.Id,
                        title = p.Title,
                        authorName = p.Author != null ? (p.Author.PenName ?? p.Author.ApplicationUser.FullName) : "Автор неизвестен",
                        type = "Poem",
                        targetAction = "ReadPoem",
                        viewsCount = p.ViewsCount,
                        likesCount = p.LikesCount
                    })
                    .ToListAsync();

                works.AddRange(poems);

                // Книги
                var books = await _bookService.GetAsync(b => b.Title.ToLower().Contains(lowerQuery));
                var topBooks = books
                    .OrderByDescending(b => b.LikesCount)
                    .Take(limit - works.Count)
                    .Select(b => new
                    {
                        id = b.Id,
                        title = b.Title,
                        authorName = "Неизвестный автор",
                        type = "Book",
                        targetAction = "ReadBook",
                        viewsCount = b.ReadsCount,
                        likesCount = b.LikesCount
                    })
                    .ToList();

                works.AddRange(topBooks);

                // Блоги
                var blogs = await _blogService.GetAsync(b => b.Title.ToLower().Contains(lowerQuery));
                var topBlogs = blogs
                    .OrderByDescending(b => b.LikesCount)
                    .Take(limit - works.Count)
                    .Select(b => new
                    {
                        id = b.Id,
                        title = b.Title,
                        authorName = "Неизвестный автор",
                        type = "Blog",
                        targetAction = "ReadBlog",
                        viewsCount = b.ViewsCount,
                        likesCount = b.LikesCount
                    })
                    .ToList();

                works.AddRange(topBlogs);

                // Берем топ-3 произведения
                var topWorks = works
                    .OrderByDescending(w => ((dynamic)w).likesCount)
                    .Take(limit)
                    .ToList();

                // --- Поиск авторов (если произведений меньше 3) ---
                var authors = new List<object>();
                if (topWorks.Count < limit)
                {
                    var authorsFromDb = await _authorService.SearchAuthorsAsync(query);
                    var topAuthors = authorsFromDb
                        .OrderByDescending(a => a.Followers.Count())
                        .Take(limit - topWorks.Count)
                        .Select(a => new
                        {
                            id = a.Id,
                            name = string.IsNullOrWhiteSpace(a.PenName) ? a.ApplicationUser.FullName : a.PenName,
                            photoUrl = a.ApplicationUser.ProfilePictureUrl,
                            subscribersCount = a.Followers.Count()
                        })
                        .ToList();

                    authors.AddRange(topAuthors);
                }

                return Json(new { works = topWorks, authors = authors });
            }
            catch (Exception ex)
            {
                // Логирование ошибки
                Console.WriteLine($"Error in Autocomplete: {ex.Message}");
                return Json(new { works = new List<object>(), authors = new List<object>() });
            }
        }
    }
}