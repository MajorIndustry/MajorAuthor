using MajorAuthor.Data;
using MajorAuthor.Models;
using MajorAuthor.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MajorAuthor.Helpers;

namespace MajorAuthor.Services
{
    /// <summary>
    /// Сервис, который инкапсулирует всю логику запросов к базе данных
    /// для главной страницы, используя фабрику DbContext для
    /// безопасного выполнения параллельных запросов.
    /// </summary>
    public class HomeServiceWithFactory : IHomeService
    {
        private readonly IDbContextFactory<MajorAuthorDbContext> _contextFactory;

        /// <summary>
        /// Конструктор, который внедряет фабрику для создания контекстов.
        /// </summary>
        /// <param name="contextFactory">Фабрика DbContext.</param>
        public HomeServiceWithFactory(IDbContextFactory<MajorAuthorDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        /// <summary>
        /// Получает все данные, необходимые для HomeViewModel,
        /// выполняя запросы параллельно.
        /// </summary>
        /// <param name="userId">ID пользователя для персонализации.</param>
        public async Task<HomeViewModel> GetHomeDataAsync(string userId)
        {
            var viewModel = new HomeViewModel
            {
                IsUserLoggedIn = !string.IsNullOrEmpty(userId)
            };

            // Создаем список задач для параллельного выполнения.
            var tasks = new List<Task>();

            // Каждая задача будет создавать свой собственный, изолированный экземпляр DbContext,
            // что позволяет избежать ошибки InvalidOperationException.
            // Используем Task.Run для выполнения запроса в отдельном потоке из пула.

            // Запрос 1: Доступные жанры
            var genresTask = Task.Run(async () =>
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.Genres
                    .Select(g => new HomeViewModel.GenreDisplayModel { Id = g.Id, Name = g.Name })
                    .OrderBy(g => g.Name)
                    .ToListAsync();
            });
            tasks.Add(genresTask);

            // Запрос 2: Популярные книги
            var popularBooksTask = Task.Run(async () =>
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.Books
                    .Include(b => b.BookAuthors)
                        .ThenInclude(ba => ba.Author)
                    .OrderByDescending(b => b.LikesCount + (double)b.ReadsCount / 10.0)
                    .Take(10)
                    .Select(b => new HomeViewModel.BookDisplayModel
                    {
                        Id = b.Id,
                        Title = b.Title,
                        AuthorName = b.BookAuthors.Select(ba => ba.Author.PenName).FirstOrDefault() ?? "Неизвестен",
                        CoverImageUrl = b.CoverImageUrl,
                        ReadsCount = b.ReadsCount,
                        LikesCount = b.LikesCount,
                        IsAdultContent = b.IsAdultContent
                    })
                    .ToListAsync();
            });
            tasks.Add(popularBooksTask);

            // Запрос 3: Популярные авторы
            var popularAuthorsTask = Task.Run(async () =>
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.Authors
                    .Select(a => new HomeViewModel.AuthorDisplayModel
                    {
                        Id = a.Id,
                        Name = a.PenName,
                        PhotoUrl = a.PhotoUrl,
                        BooksCount = a.BookAuthors.Count(),
                        TotalReadsCount = a.BookAuthors.Sum(ba => ba.Book.ReadsCount)
                    })
                    .OrderByDescending(a => a.TotalReadsCount)
                    .Take(10)
                    .ToListAsync();
            });
            tasks.Add(popularAuthorsTask);

            // Запрос 4: Недавно обновленные книги
            var recentlyUpdatedBooksTask = Task.Run(async () =>
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.Books
                    .Include(b => b.BookAuthors)
                        .ThenInclude(ba => ba.Author)
                    .Where(b => b.LastUpdateTime >= DateTime.UtcNow.AddDays(-7))
                    .OrderByDescending(b => b.LastUpdateTime)
                    .Take(10)
                    .Select(b => new HomeViewModel.BookDisplayModel
                    {
                        Id = b.Id,
                        Title = b.Title,
                        AuthorName = b.BookAuthors.Select(ba => ba.Author.PenName).FirstOrDefault() ?? "Неизвестен",
                        CoverImageUrl = b.CoverImageUrl,
                        UpdateInfo = TimeHelper.GetRelativeTime(b.LastUpdateTime),
                        ReadsCount = b.ReadsCount,
                        LikesCount = b.LikesCount
                    })
                    .ToListAsync();
            });
            tasks.Add(recentlyUpdatedBooksTask);

            // Запрос 5: Новые популярные книги
            var newPopularBooksTask = Task.Run(async () =>
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.Books
                    .Include(b => b.BookAuthors)
                        .ThenInclude(ba => ba.Author)
                    .Where(b => b.PublicationDate >= DateTime.UtcNow.AddDays(-30))
                    .OrderByDescending(b => b.LikesCount + (double)b.ReadsCount / 5.0)
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
            });
            tasks.Add(newPopularBooksTask);

            // Запрос 6: Новые популярные авторы
            var newPopularAuthorsTask = Task.Run(async () =>
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.Authors
                    .Where(a => a.AuthorProfileCreationDate >= DateTime.UtcNow.AddDays(-90))
                    .Select(a => new HomeViewModel.AuthorDisplayModel
                    {
                        Id = a.Id,
                        Name = a.PenName,
                        PhotoUrl = a.PhotoUrl,
                        RegistrationInfo = TimeHelper.GetRelativeTime(a.AuthorProfileCreationDate),
                        TotalReadsCount = a.BookAuthors.Sum(ba => ba.Book.ReadsCount)
                    })
                    .OrderByDescending(a => a.TotalReadsCount)
                    .Take(10)
                    .ToListAsync();
            });
            tasks.Add(newPopularAuthorsTask);

            // Запрос 7: Продвигающиеся книги
            var promotedBooksTask = Task.Run(async () =>
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.Promotions
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
            });
            tasks.Add(promotedBooksTask);

            // Если пользователь авторизован, добавляем запрос на рекомендации
            if (!string.IsNullOrEmpty(userId))
            {
                var recommendedBooksTask = Task.Run(async () =>
                {
                    using var context = _contextFactory.CreateDbContext();
                    return await GetRecommendedBooksAsync(userId);
                });
                tasks.Add(recommendedBooksTask);
            }

            // Ожидаем завершения всех задач
            await Task.WhenAll(tasks);

            // Заполняем ViewModel данными из завершенных задач.
            // Поскольку мы уверены, что все задачи завершились, обращение к .Result безопасно.
            viewModel.AvailableGenres = genresTask.Result;
            viewModel.PopularBooks = popularBooksTask.Result;
            viewModel.PopularAuthors = popularAuthorsTask.Result;
            viewModel.RecentlyUpdatedBooks = recentlyUpdatedBooksTask.Result;
            viewModel.NewPopularBooks = newPopularBooksTask.Result;
            viewModel.NewPopularAuthors = newPopularAuthorsTask.Result;
            viewModel.PromotedBooks = promotedBooksTask.Result;
            if (tasks.Count > 7) // Если был добавлен запрос на рекомендации
            {
                viewModel.RecommendedBooks = ((Task<List<HomeViewModel.BookDisplayModel>>)tasks.Last()).Result;
            }

            return viewModel;
        }

        /// <summary>
        /// Получает список книг по ID жанра.
        /// </summary>
        /// <param name="genreId">ID жанра.</param>
        public async Task<List<HomeViewModel.BookDisplayModel>> GetBooksByGenreAsync(int genreId)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Books
                .Include(b => b.BookAuthors)
                    .ThenInclude(ba => ba.Author)
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
                    LikesCount = b.LikesCount,
                    IsAdultContent = b.IsAdultContent
                })
                .ToListAsync();
        }

        /// <summary>
        /// Внутренний метод для получения рекомендаций.
        /// </summary>
        /// <param name="userId">ID пользователя.</param>
        private async Task<List<HomeViewModel.BookDisplayModel>> GetRecommendedBooksAsync(string userId)
        {
            // Здесь мы намеренно не создаем новый контекст, так как этот метод
            // вызывается из GetHomeDataAsync, где уже создан отдельный контекст.
            // Это демонстрирует, что фабрика позволяет создавать контексты в
            // любом месте, где это необходимо.
            using var context = _contextFactory.CreateDbContext();

            var userPreferredGenreIds = await context.UserPreferredGenres
                .Where(upg => upg.ApplicationUserId == userId)
                .Select(upg => upg.GenreId)
                .ToListAsync();

            var userPreferredTagIds = await context.UserPreferredTags
                .Where(upt => upt.ApplicationUserId == userId)
                .Select(upt => upt.TagId)
                .ToListAsync();

            return await context.Books
                .Include(b => b.BookAuthors)
                    .ThenInclude(ba => ba.Author)
                .Include(b => b.BookGenres)
                .Include(b => b.BookTags)
                .Where(b => b.BookGenres.Any(bg => userPreferredGenreIds.Contains(bg.GenreId)) ||
                            b.BookTags.Any(bt => userPreferredTagIds.Contains(bt.TagId)))
                .OrderByDescending(b => b.LikesCount + (double)b.ReadsCount)
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
                    RecommendationReason = (b.BookGenres.Any(bg => userPreferredGenreIds.Contains(bg.GenreId)) ? "Потому что вы любите этот жанр" : "") +
                                           (b.BookTags.Any(bt => userPreferredTagIds.Contains(bt.TagId)) ? " и этот тег" : "")
                })
                .ToListAsync();
        }
    }
}
