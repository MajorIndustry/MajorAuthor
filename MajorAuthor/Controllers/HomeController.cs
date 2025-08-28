// Проект: MajorAuthor
// Файл: Controllers/HomeController.cs
using Microsoft.AspNetCore.Mvc;
using MajorAuthor.Models; // Используем нашу ViewModel
using MajorAuthor.Data; // Используем наш DbContext
using MajorAuthor.Data.Entities; // Добавлено для доступа к сущностям PromotionPlan, Promotion, Book, Author, Genre и т.д.
using Microsoft.EntityFrameworkCore; // Для методов расширения EF Core, таких как Include, ToListAsync
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims; // Добавлено для ClaimTypes
using System.Threading.Tasks;
using System.Diagnostics; // Добавлено для Activity (для ErrorViewModel)

namespace MajorAuthor.Controllers // Пространство имен для контроллеров
{
    public class HomeController : Controller
    {
        private readonly MajorAuthorDbContext _context;

        /// <summary>
        /// Конструктор контроллера, использующий внедрение зависимостей для DbContext.
        /// </summary>
        /// <param name="context">Экземпляр MajorAuthorDbContext.</param>
        public HomeController(MajorAuthorDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Отображает главную страницу с различными секциями книг и авторов.
        /// Данные извлекаются из базы данных.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var viewModel = new HomeViewModel
            {
                IsUserLoggedIn = User.Identity.IsAuthenticated // Проверяем, авторизован ли пользователь
            };

            // --- Извлечение данных из базы данных ---

            // Получение всех доступных жанров для фильтрации
            viewModel.AvailableGenres = await _context.Genres
                .Select(g => new HomeViewModel.GenreDisplayModel { Id = g.Id, Name = g.Name })
                .OrderBy(g => g.Name)
                .ToListAsync();

            // Популярные книги (например, топ-10 по количеству лайков и прочтений)
            viewModel.PopularBooks = await _context.Books
                .Include(b => b.BookAuthors) // Включаем BookAuthors для доступа к Author
                    .ThenInclude(ba => ba.Author) // Включаем Author
                .OrderByDescending(b => b.LikesCount + (double)b.ReadsCount / 10.0) // Пример комбинированного рейтинга
                .Take(10)
                .Select(b => new HomeViewModel.BookDisplayModel
                {
                    Id = b.Id,
                    Title = b.Title,
                    // Для автора: Если книга может иметь несколько авторов,
                    // можно объединить их имена или выбрать первого/основного.
                    // Используем Author.PenName (псевдоним)
                    AuthorName = b.BookAuthors.Select(ba => ba.Author.PenName).FirstOrDefault() ?? "Неизвестен",
                    CoverImageUrl = b.CoverImageUrl,
                    ReadsCount = b.ReadsCount,
                    LikesCount = b.LikesCount,
                    IsAdultContent = b.IsAdultContent
                })
                .ToListAsync();

            // Популярные авторы (например, топ-10 по общему количеству прочтений их книг)
            viewModel.PopularAuthors = await _context.Authors
                .Select(a => new HomeViewModel.AuthorDisplayModel
                {
                    Id = a.Id,
                    // Используем PenName для отображения имени автора
                    Name = a.PenName,
                    PhotoUrl = a.PhotoUrl,
                    BooksCount = a.BookAuthors.Count(),
                    TotalReadsCount = a.BookAuthors.Sum(ba => ba.Book.ReadsCount)
                })
                .OrderByDescending(a => a.TotalReadsCount) // Сортируем по TotalReadsCount, рассчитанному выше
                .Take(10)
                .ToListAsync();

            // Недавно обновленные книги (за последние 7 дней, упорядоченные по LastUpdateTime)
            viewModel.RecentlyUpdatedBooks = await _context.Books
                .Include(b => b.BookAuthors) // Включаем BookAuthors для доступа к Author
                    .ThenInclude(ba => ba.Author) // Включаем Author
                .Where(b => b.LastUpdateTime >= DateTime.UtcNow.AddDays(-7))
                .OrderByDescending(b => b.LastUpdateTime)
                .Take(10)
                .Select(b => new HomeViewModel.BookDisplayModel
                {
                    Id = b.Id,
                    Title = b.Title,
                    AuthorName = b.BookAuthors.Select(ba => ba.Author.PenName).FirstOrDefault() ?? "Неизвестен",
                    CoverImageUrl = b.CoverImageUrl,
                    UpdateInfo = GetRelativeTime(b.LastUpdateTime), // Используем статический метод
                    ReadsCount = b.ReadsCount, // Добавлено
                    LikesCount = b.LikesCount // Добавлено
                })
                .ToListAsync();

            // Новые популярные книги (опубликованы недавно, но уже набрали много лайков/прочтений)
            viewModel.NewPopularBooks = await _context.Books
                .Include(b => b.BookAuthors) // Включаем BookAuthors для доступа к Author
                    .ThenInclude(ba => ba.Author) // Включаем Author
                .Where(b => b.PublicationDate >= DateTime.UtcNow.AddDays(-30)) // Опубликованы за последний месяц
                .OrderByDescending(b => b.LikesCount + (double)b.ReadsCount / 5.0) // Высокое соотношение
                .Take(10)
                .Select(b => new HomeViewModel.BookDisplayModel
                {
                    Id = b.Id,
                    Title = b.Title,
                    AuthorName = b.BookAuthors.Select(ba => ba.Author.PenName).FirstOrDefault() ?? "Неизвестен",
                    CoverImageUrl = b.CoverImageUrl,
                    ReadsCount = b.ReadsCount,
                    LikesCount = b.LikesCount
                })
                .ToListAsync();

            // Новые популярные авторы (зарегистрировались недавно, но уже набрали много прочтений)
            viewModel.NewPopularAuthors = await _context.Authors
                .Where(a => a.AuthorProfileCreationDate >= DateTime.UtcNow.AddDays(-90)) // Профиль создан за последние 3 месяца
                .Select(a => new HomeViewModel.AuthorDisplayModel
                {
                    Id = a.Id,
                    Name = a.PenName, // Используем PenName
                    PhotoUrl = a.PhotoUrl,
                    RegistrationInfo = GetRelativeTime(a.AuthorProfileCreationDate), // Используем AuthorProfileCreationDate
                    TotalReadsCount = a.BookAuthors.Sum(ba => ba.Book.ReadsCount)
                })
                .OrderByDescending(a => a.TotalReadsCount)
                .Take(10)
                .ToListAsync();

            // Продвигающиеся книги (на основе активных продвижений)
            viewModel.PromotedBooks = await _context.Promotions
                .Include(p => p.Book)
                    .ThenInclude(b => b.BookAuthors)
                        .ThenInclude(ba => ba.Author)
                .Where(p => p.StartDate <= DateTime.UtcNow && p.EndDate >= DateTime.UtcNow)
                .Select(p => new HomeViewModel.BookDisplayModel
                {
                    Id = p.Book.Id,
                    Title = p.Book.Title,
                    AuthorName = p.Book.BookAuthors.Select(ba => ba.Author.PenName).FirstOrDefault() ?? "Неизвестен",
                    CoverImageUrl = p.Book.CoverImageUrl,
                    ReadsCount = p.Book.ReadsCount,
                    LikesCount = p.Book.LikesCount,
                    IsAdultContent = p.Book.IsAdultContent
                })
                .ToListAsync();


            // Рекомендации для зарегистрированных пользователей
            if (viewModel.IsUserLoggedIn)
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (currentUserId != null)
                {
                    // Получаем любимые жанры пользователя
                    var userPreferredGenreIds = await _context.UserPreferredGenres
                        .Where(upg => upg.ApplicationUserId == currentUserId)
                        .Select(upg => upg.GenreId)
                        .ToListAsync();

                    // Получаем любимые теги пользователя
                    var userPreferredTagIds = await _context.UserPreferredTags
                        .Where(upt => upt.ApplicationUserId == currentUserId)
                        .Select(upt => upt.TagId)
                        .ToListAsync();

                    viewModel.RecommendedBooks = await _context.Books
                        .Include(b => b.BookAuthors)
                            .ThenInclude(ba => ba.Author)
                        .Include(b => b.BookGenres) // Включаем для фильтрации по жанрам
                        .Include(b => b.BookTags)   // Включаем для фильтрации по тегам
                        .Where(b => b.BookGenres.Any(bg => userPreferredGenreIds.Contains(bg.GenreId)) || // Книги по любимым жанрам
                                     b.BookTags.Any(bt => userPreferredTagIds.Contains(bt.TagId)))         // Книги по любимым тегам
                        .OrderByDescending(b => b.LikesCount + (double)b.ReadsCount) // Простой рейтинг для рекомендаций
                        .Take(10)
                        .Select(b => new HomeViewModel.BookDisplayModel
                        {
                            Id = b.Id,
                            Title = b.Title,
                            AuthorName = b.BookAuthors.Select(ba => ba.Author.PenName).FirstOrDefault() ?? "Неизвестен",
                            CoverImageUrl = b.CoverImageUrl,
                            ReadsCount = b.ReadsCount,
                            LikesCount = b.LikesCount,
                            IsAdultContent = b.IsAdultContent,
                            // Пример сложной логики причины рекомендации:
                            RecommendationReason = (b.BookGenres.Any(bg => userPreferredGenreIds.Contains(bg.GenreId)) ? "Потому что вы любите этот жанр" : "") +
                                                   (b.BookTags.Any(bt => userPreferredTagIds.Contains(bt.TagId)) ? " и этот тег" : "")
                        })
                        .ToListAsync();
                }
            }

            return View(viewModel);
        }

        /// <summary>
        /// Action для получения книг по выбранному жанру (используется AJAX).
        /// </summary>
        /// <param name="genreId">ID выбранного жанра.</param>
        [HttpGet]
        public async Task<IActionResult> GetBooksByGenre(int genreId)
        {
            var books = await _context.Books
                .Include(b => b.BookAuthors)
                    .ThenInclude(ba => ba.Author)
                .Where(b => b.BookGenres.Any(bg => bg.GenreId == genreId))
                .OrderByDescending(b => b.PublicationDate) // Сортируем по дате публикации
                .Take(10)
                .Select(b => new HomeViewModel.BookDisplayModel
                {
                    Id = b.Id,
                    Title = b.Title,
                    AuthorName = b.BookAuthors.Select(ba => ba.Author.PenName).FirstOrDefault() ?? "Неизвестен",
                    CoverImageUrl = b.CoverImageUrl,
                    ReadsCount = b.ReadsCount,
                    LikesCount = b.LikesCount,
                    IsAdultContent = b.IsAdultContent // Добавлено
                })
                .ToListAsync();

            // Возвращаем частичное представление, которое будет рендерить список книг
            return PartialView("_BookListPartial", books);
        }

        /// <summary>
        /// Вспомогательная функция для получения относительного времени (например, "2 дня назад").
        /// Сделана статической для предотвращения предупреждений EF Core.
        /// </summary>
        /// <param name="dateTime">Дата и время для форматирования.</param>
        /// <returns>Строка, представляющая относительное время.</returns>
        private static string GetRelativeTime(DateTime dateTime)
        {
            var timeSpan = DateTime.UtcNow - dateTime;

            if (timeSpan <= TimeSpan.FromSeconds(60))
                return $"{timeSpan.Seconds} секунд назад";
            if (timeSpan <= TimeSpan.FromMinutes(60))
                return $"{timeSpan.Minutes} минут назад";
            if (timeSpan <= TimeSpan.FromHours(24))
                return $"{timeSpan.Hours} часов назад";
            if (timeSpan <= TimeSpan.FromDays(30))
                return $"{timeSpan.Days} дней назад";
            if (timeSpan <= TimeSpan.FromDays(365))
                return $"{timeSpan.Days / 30} месяцев назад";
            return $"{timeSpan.Days / 365} лет назад";
        }

        // Action для обработки ошибок
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            // Убедитесь, что ErrorViewModel находится в MajorAuthor.Models (или MajorAuthor.Web.Models, если у вас такой проект)
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
