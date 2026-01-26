// File: Services/SimilarBookCalculatorService.cs
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MajorAuthor.Data;
using MajorAuthor.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace MajorAuthor.Services
{
    public class SimilarBookCalculatorService : ISimilarBookCalculatorService, IHostedService, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SimilarBookCalculatorService> _logger;
        private Timer _timer;

        public SimilarBookCalculatorService(
            IServiceProvider serviceProvider,
            ILogger<SimilarBookCalculatorService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("SimilarBookCalculatorService запущен");

            // Запускаем таймер на ежедневный пересчет (в 2:00 ночи)
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromDays(1));

            // Также запускаем немедленно при старте
            _ = Task.Run(() => CalculateSimilaritiesForAllBooksAsync());

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("SimilarBookCalculatorService остановлен");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        private async void DoWork(object state)
        {
            try
            {
                _logger.LogInformation("Запуск ежедневного пересчета похожих книг");
                await CalculateSimilaritiesForAllBooksAsync();
                await RecalculateOutdatedSimilaritiesAsync(7);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при расчете похожих книг");
            }
        }

        public async Task CalculateSimilaritiesForAllBooksAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<MajorAuthorDbContext>();

            try
            {
                var bookIds = await context.Books
                    .Where(b => b.IsPublic)
                    .Select(b => b.Id)
                    .ToListAsync();

                _logger.LogInformation($"Начинаем расчет для {bookIds.Count} книг");

                foreach (var bookId in bookIds)
                {
                    await CalculateSimilaritiesForBookAsync(bookId);
                }

                _logger.LogInformation("Расчет похожих книг завершен");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при расчете похожих книг для всех книг");
            }
        }

        public async Task CalculateSimilaritiesForBookAsync(int bookId)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<MajorAuthorDbContext>();

            try
            {
                // Получаем текущую книгу
                var currentBook = await context.Books
                    .Include(b => b.BookGenres)
                        .ThenInclude(bg => bg.Genre)
                    .Include(b => b.BookTags)
                        .ThenInclude(bt => bt.Tag)
                    .Include(b => b.BookAuthors)
                        .ThenInclude(ba => ba.Author)
                    .FirstOrDefaultAsync(b => b.Id == bookId);

                if (currentBook == null)
                    return;

                // Получаем ID жанров и тегов текущей книги
                var currentGenreIds = currentBook.BookGenres.Select(bg => bg.GenreId).ToList();
                var currentTagIds = currentBook.BookTags.Select(bt => bt.TagId).ToList();
                var currentAuthorIds = currentBook.BookAuthors.Select(ba => ba.AuthorId).ToList();

                // Находим все книги для сравнения (исключая текущую и неопубликованные)
                var allBooks = await context.Books
                    .Where(b => b.Id != bookId && b.IsPublic)
                    .Include(b => b.BookGenres)
                        .ThenInclude(bg => bg.Genre)
                    .Include(b => b.BookTags)
                        .ThenInclude(bt => bt.Tag)
                    .Include(b => b.BookAuthors)
                        .ThenInclude(ba => ba.Author)
                    .ToListAsync();

                var similarities = allBooks
                    .Select(otherBook => new
                    {
                        Book = otherBook,
                        Score = CalculateSimilarityScore(
                            currentBook,
                            otherBook,
                            currentGenreIds,
                            currentTagIds,
                            currentAuthorIds)
                    })
                    .Where(x => x.Score > 0) // Только книги с положительным сходством
                    .OrderByDescending(x => x.Score)
                    .Take(10) // Сохраняем топ-10 для запаса
                    .ToList();

                // Удаляем старые записи о схожести
                var existingSimilarities = await context.SimilarBooks
                    .Where(sb => sb.BookId == bookId)
                    .ToListAsync();

                context.SimilarBooks.RemoveRange(existingSimilarities);

                // Добавляем новые записи (топ-5)
                foreach (var similarity in similarities.Take(5))
                {
                    var similarBook = new SimilarBook
                    {
                        BookId = bookId,
                        SimilarToBookId = similarity.Book.Id,
                        SimilarityScore = similarity.Score,
                        CalculationDate = DateTime.UtcNow,
                        CommonGenresCount = similarity.Book.BookGenres
                            .Count(bg => currentGenreIds.Contains(bg.GenreId)),
                        CommonTagsCount = similarity.Book.BookTags
                            .Count(bt => currentTagIds.Contains(bt.TagId)),
                        SameAuthor = similarity.Book.BookAuthors
                            .Any(ba => currentAuthorIds.Contains(ba.AuthorId))
                    };

                    context.SimilarBooks.Add(similarBook);
                }

                await context.SaveChangesAsync();

                _logger.LogInformation($"Рассчитаны похожие книги для книги ID {bookId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при расчете похожих книг для книги {bookId}");
            }
        }

        private double CalculateSimilarityScore(
            Book currentBook,
            Book otherBook,
            List<int> currentGenreIds,
            List<int> currentTagIds,
            List<int> currentAuthorIds)
        {
            double score = 0;

            // Жанры: 3 балла за каждый общий жанр
            var otherGenreIds = otherBook.BookGenres.Select(bg => bg.GenreId).ToList();
            var commonGenres = currentGenreIds.Intersect(otherGenreIds).Count();
            score += commonGenres * 3;

            // Теги: 2 балла за каждый общий тег
            var otherTagIds = otherBook.BookTags.Select(bt => bt.TagId).ToList();
            var commonTags = currentTagIds.Intersect(otherTagIds).Count();
            score += commonTags * 2;

            // Авторы: 5 баллов за общего автора
            var otherAuthorIds = otherBook.BookAuthors.Select(ba => ba.AuthorId).ToList();
            var commonAuthors = currentAuthorIds.Intersect(otherAuthorIds).Count();
            score += commonAuthors * 5;

            // Бонус за совпадение типа книги
            if (currentBook.TypeId == otherBook.TypeId)
                score += 2;

            // Бонус за нахождение в одном цикле
            if (currentBook.CycleId.HasValue && currentBook.CycleId == otherBook.CycleId)
                score += 4;

            // Штраф за разницу в статусе (например, завершенная vs пишется)
            if (currentBook.StatusId != otherBook.StatusId)
                score -= 1;

            // Нормализуем счёт до 0-1
            double maxPossibleScore = (currentGenreIds.Count * 3) +
                                     (currentTagIds.Count * 2) +
                                     (currentAuthorIds.Count * 5) +
                                     6; // бонусы

            return maxPossibleScore > 0 ? Math.Min(1.0, score / maxPossibleScore) : 0;
        }

        public async Task RecalculateOutdatedSimilaritiesAsync(int daysThreshold = 7)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<MajorAuthorDbContext>();

            try
            {
                var outdatedDate = DateTime.UtcNow.AddDays(-daysThreshold);

                var outdatedBooks = await context.SimilarBooks
                    .Where(sb => sb.CalculationDate < outdatedDate)
                    .Select(sb => sb.BookId)
                    .Distinct()
                    .ToListAsync();

                _logger.LogInformation($"Найдено {outdatedBooks.Count} книг с устаревшими данными о схожести");

                foreach (var bookId in outdatedBooks)
                {
                    await CalculateSimilaritiesForBookAsync(bookId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при пересчете устаревших данных о схожести");
            }
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}