using MajorAuthor.Data;
using MajorAuthor.Data.Entities;
using MajorAuthor.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MajorAuthor.Controllers
{
    [Authorize]
    public class BooksController : Controller
    {
        private readonly IBookService _bookService;
        private readonly MajorAuthorDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<BooksController> _logger;

        public BooksController(
            IBookService bookService,
            MajorAuthorDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<BooksController> logger)
        {
            _bookService = bookService ?? throw new ArgumentNullException(nameof(bookService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<IActionResult> Bookmarks()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Challenge();
                }

                // Получаем избранные книги с полной информацией
                var userFavorites = await _context.UserFavoriteBooks
                    .Where(ufb => ufb.ApplicationUserId == userId)
                    .Include(ufb => ufb.Book)
                        .ThenInclude(b => b.BookAuthors)
                            .ThenInclude(ba => ba.Author)
                    .Include(ufb => ufb.Book)
                        .ThenInclude(b => b.Chapters)
                    .Include(ufb => ufb.Book)
                        .ThenInclude(b => b.BookTags)
                            .ThenInclude(bt => bt.Tag)
                    .Include(ufb => ufb.Book)
                        .ThenInclude(b => b.BookGenres)
                            .ThenInclude(bg => bg.Genre)
                    .OrderByDescending(ufb => ufb.AddedDate)
                    .ToListAsync();

                return View(userFavorites);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при загрузке закладок пользователя");
                return View(new List<UserFavoriteBook>());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromFavorites([FromBody] RemoveFromFavoritesRequest request)
        {
            try
            {
                if (request == null || request.BookId <= 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Некорректный запрос. BookId обязателен."
                    });
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Пользователь не авторизован"
                    });
                }

                var result = await _bookService.RemoveFromFavoritesAsync(request.BookId, userId);

                if (result)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Книга удалена из закладок"
                    });
                }

                return Json(new
                {
                    success = false,
                    message = "Книга не найдена в закладках"
                });
            }
            catch (ArgumentException ex) when (ex.Message.Contains("не существует"))
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении книги из закладок");
                return Json(new
                {
                    success = false,
                    message = "Произошла ошибка при удалении из закладок"
                });
            }
        }

        // Вспомогательный класс для запроса
        public class RemoveFromFavoritesRequest
        {
            public int BookId { get; set; }
        }
    }
}