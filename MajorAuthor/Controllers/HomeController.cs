// Проект: MajorAuthor.Web
// Файл: Controllers/HomeController.cs
using Microsoft.AspNetCore.Mvc;
using MajorAuthor.Web.Models; // Используем нашу ViewModel
using MajorAuthor.Data; // Используем наш DbContext
using Microsoft.EntityFrameworkCore; // Для методов расширения EF Core, таких как Include, ToListAsync
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims; // Добавлено для ClaimTypes
using System.Diagnostics;
using MajorAuthor.Models; // Для Activity (для ErrorViewModel)


namespace MajorAuthor.Web.Controllers
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

            // Популярные книги (например, топ-10 по количеству лайков или прочтений)
            viewModel.PopularBooks = await _context.Books
                .OrderByDescending(b => b.LikesCount + b.ReadsCount / 10.0) // Пример комбинированного рейтинга
                .Take(10)
                .Select(b => new HomeViewModel.BookDisplayModel
                {
                    Id = b.Id,
                    Title = b.Title,
                    // Для автора: Если книга может иметь несколько авторов,
                    // можно объединить их имена или выбрать первого/основного.
                    // Здесь предполагается, что вы хотите отобразить PenName основного автора или просто первого.
                    // Для упрощения, пока берем PenName первого автора, если он есть.
                    AuthorName = b.BookAuthors.Select(ba => ba.Author.PenName).FirstOrDefault() ?? "Неизвестен",
                    CoverImageUrl = b.CoverImageUrl,
                    ReadsCount = b.ReadsCount,
                    LikesCount = b.LikesCount,
                    IsAdultContent = b.IsAdultContent
                })
                .ToListAsync();

            // Популярные авторы (например, топ-10 по общему количеству прочтений их книг)
            viewModel.PopularAuthors = await _context.Authors
                .OrderByDescending(a => a.BookAuthors.Sum(ba => ba.Book.ReadsCount)) // Суммируем прочтения всех книг автора
                .Take(10)
                .Select(a => new HomeViewModel.AuthorDisplayModel
                {
                    Id = a.Id,
                    Name = a.PenName,
                    PhotoUrl = a.PhotoUrl,
                    BooksCount = a.BookAuthors.Count(),
                    TotalReadsCount = a.BookAuthors.Sum(ba => ba.Book.ReadsCount)
                })
                .ToListAsync();

            // Недавно обновленные книги (за последние 7 дней, упорядоченные по LastUpdateTime)
            viewModel.RecentlyUpdatedBooks = await _context.Books
                .Where(b => b.LastUpdateTime >= DateTime.UtcNow.AddDays(-7))
                .OrderByDescending(b => b.LastUpdateTime)
                .Take(10)
                .Select(b => new HomeViewModel.BookDisplayModel
                {
                    Id = b.Id,
                    Title = b.Title,
                    AuthorName = b.BookAuthors.Select(ba => ba.Author.PenName).FirstOrDefault() ?? "Неизвестен",
                    CoverImageUrl = b.CoverImageUrl,
                    UpdateInfo = GetRelativeTime(b.LastUpdateTime) // Используем статический метод
                })
                .ToListAsync();

            // Новые популярные книги (опубликованы недавно, но уже набрали много лайков/прочтений)
            viewModel.NewPopularBooks = await _context.Books
                .Where(b => b.PublicationDate >= DateTime.UtcNow.AddDays(-30)) // Опубликованы за последний месяц
                .OrderByDescending(b => b.LikesCount + b.ReadsCount / 5.0) // Высокое соотношение
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
                .OrderByDescending(a => a.BookAuthors.Sum(ba => ba.Book.ReadsCount))
                .Take(10)
                .Select(a => new HomeViewModel.AuthorDisplayModel
                {
                    Id = a.Id,
                    Name = a.PenName,
                    PhotoUrl = a.PhotoUrl,
                    RegistrationInfo = GetRelativeTime(a.AuthorProfileCreationDate), // Используем статический метод
                    TotalReadsCount = a.BookAuthors.Sum(ba => ba.Book.ReadsCount)
                })
                .ToListAsync();


            // Рекомендации для зарегистрированных пользователей
            if (viewModel.IsUserLoggedIn)
            {
                // В реальном приложении: var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                // Для демонстрации:
                var userId = 1; // Замените на реальный UserId авторизованного пользователя

                // Пример: Рекомендации на основе любимых жанров и тегов
                var userPreferredGenreIds = await _context.UserPreferredGenres
                    .Where(upg => upg.UserId == userId)
                    .Select(upg => upg.GenreId)
                    .ToListAsync();

                var userPreferredTagIds = await _context.UserPreferredTags
                    .Where(upt => upt.UserId == userId)
                    .Select(upt => upt.TagId)
                    .ToListAsync();

                viewModel.RecommendedBooks = await _context.Books
                    .Where(b => b.BookGenres.Any(bg => userPreferredGenreIds.Contains(bg.GenreId)) || // Книги по любимым жанрам
                                b.BookTags.Any(bt => userPreferredTagIds.Contains(bt.TagId)))       // Книги по любимым тегам
                    .OrderByDescending(b => b.LikesCount + b.ReadsCount) // Простой рейтинг для рекомендаций
                    .Take(10)
                    .Select(b => new HomeViewModel.BookDisplayModel
                    {
                        Id = b.Id,
                        Title = b.Title,
                        AuthorName = b.BookAuthors.Select(ba => ba.Author.PenName).FirstOrDefault() ?? "Неизвестен",
                        CoverImageUrl = b.CoverImageUrl,
                        // Пример сложной логики причины рекомендации:
                        RecommendationReason = (b.BookGenres.Any(bg => userPreferredGenreIds.Contains(bg.GenreId)) ? "Потому что вы любите этот жанр" : "") +
                                               (b.BookTags.Any(bt => userPreferredTagIds.Contains(bt.TagId)) ? " и этот тег" : "")
                    })
                    .ToListAsync();
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
                .Where(b => b.BookGenres.Any(bg => bg.GenreId == genreId))
                .OrderByDescending(b => b.PublicationDate)
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
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
