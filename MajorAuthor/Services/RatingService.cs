// Services/RatingService.cs
using MajorAuthor.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MajorAuthor.Services
{
    public class RatingService : IRatingService
    {
        private readonly IDbContextFactory<MajorAuthorDbContext> _contextFactory;

        public RatingService(IDbContextFactory<MajorAuthorDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        /// <summary>
        /// Расчет общего рейтинга (лайки важнее просмотров)
        /// Формула: лайки * 2 + просмотры * 0.1
        /// </summary>
        public double CalculateGlobalRating(int likes, int views)
        {
            if (views == 0) return 0;

            // Базовый рейтинг на основе соотношения лайков к просмотрам
            double baseRating = ((double)likes / views) * 10;

            // Логарифмическая поправка за большое количество просмотров
            // log10(views + 1) дает поправку от 0 до ~6 (при миллионе просмотров)
            double viewBonus = Math.Log10(views + 1) * 0.5; // Коэффициент 0.5 для регулировки влияния

            // Итоговый рейтинг с ограничением от 0 до 10
            double finalRating = Math.Min(baseRating + viewBonus, 10);
            finalRating = Math.Max(finalRating, 0); // Не ниже 0

            return Math.Round(finalRating, 2); // Округляем до 2 знаков
        }

        /// <summary>
        /// Расчет локального рейтинга (соотношение лайков к просмотрам)
        /// Формула: (лайки / просмотры) * коэффициент
        /// </summary>
        public double CalculateLocalRating(int likes, int views, double coefficient = 10)
        {
            if (views == 0) return 0;
            return Math.Round(((double)likes / views) * coefficient, 2);
        }

        /// <summary>
        /// Получение рейтинга книги за период
        /// </summary>
        public async Task<double> GetBookRatingForPeriodAsync(int bookId, DateTime startDate, DateTime endDate)
        {
            using var context = _contextFactory.CreateDbContext();

            var likes = await context.BookLikes
                .Where(bl => bl.BookId == bookId && bl.LikeDate >= startDate && bl.LikeDate <= endDate)
                .CountAsync();

            var views = await context.BookReadings
                .Where(br => br.BookId == bookId && br.ReadDate >= startDate && br.ReadDate <= endDate)
                .CountAsync();

            return CalculateGlobalRating(likes, views);
        }


        /// <summary>
        /// Получение рейтинга стиха за период
        /// </summary>
        public async Task<double> GetPoemRatingForPeriodAsync(int poemId, DateTime startDate, DateTime endDate)
        {
            using var context = _contextFactory.CreateDbContext();

            var likes = await context.PoemLikes
                .Where(pl => pl.PoemId == poemId && pl.LikeDate >= startDate && pl.LikeDate <= endDate)
                .CountAsync();

            var views = await context.PoemReadings
                .Where(pr => pr.PoemId == poemId && pr.ReadDate >= startDate && pr.ReadDate <= endDate)
                .CountAsync();

            return CalculateGlobalRating(likes, views);
        }

        /// <summary>
        /// Получение рейтинга автора за период
        /// </summary>
        public async Task<double> GetAuthorRatingForPeriodAsync(int authorId, DateTime startDate, DateTime endDate)
        {
            using var context = _contextFactory.CreateDbContext();

            // Суммируем рейтинги всех книг автора за период
            var booksRating = await context.Books
                .Where(b => b.BookAuthors.Any(ba => ba.AuthorId == authorId))
                .SumAsync(b => GetBookRatingForPeriodAsync(b.Id, startDate, endDate).Result);

            // Суммируем рейтинги всех стихов автора за период
            var poemsRating = await context.Poems
                .Where(p => p.AuthorId == authorId)
                .SumAsync(p => GetPoemRatingForPeriodAsync(p.Id, startDate, endDate).Result);

            return booksRating + poemsRating;
        }

        /// <summary>
        /// Получение популярных книг за период
        /// </summary>
        public async Task<List<(int BookId, double Rating)>> GetPopularBooksForPeriodAsync(DateTime startDate, DateTime endDate, int count = 10)
        {
            using var context = _contextFactory.CreateDbContext();

            var books = await context.Books
                .Include(b => b.Likes)
                .Include(b => b.Readings)
                .Select(b => new
                {
                    b.Id,
                    PeriodLikes = b.Likes.Count(bl => bl.LikeDate >= startDate && bl.LikeDate <= endDate),
                    PeriodViews = b.Readings.Count(br => br.ReadDate >= startDate && br.ReadDate <= endDate)
                })
                .ToListAsync();

            return books
                .Select(b => (b.Id, CalculateGlobalRating(b.PeriodLikes, b.PeriodViews)))
                .OrderByDescending(x => x.Item2)
                .Take(count)
                .ToList();
        }

        /// <summary>
        /// Получение популярных стихов за период
        /// </summary>
        public async Task<List<(int PoemId, double Rating)>> GetPopularPoemsForPeriodAsync(DateTime startDate, DateTime endDate, int count = 10)
        {
            using var context = _contextFactory.CreateDbContext();

            var poems = await context.Poems
                .Include(p => p.Likes)
                .Include(p => p.Readings)
                .Select(p => new
                {
                    p.Id,
                    PeriodLikes = p.Likes.Count(pl => pl.LikeDate >= startDate && pl.LikeDate <= endDate),
                    PeriodViews = p.Readings.Count(pr => pr.ReadDate >= startDate && pr.ReadDate <= endDate)
                })
                .ToListAsync();

            return poems
                .Select(p => (p.Id, CalculateGlobalRating(p.PeriodLikes, p.PeriodViews)))
                .OrderByDescending(x => x.Item2)
                .Take(count)
                .ToList();
        }
    }
}