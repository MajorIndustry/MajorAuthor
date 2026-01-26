using MajorAuthor.Data;
using MajorAuthor.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;

namespace MajorAuthor.Services
{
    public class RatingManagerService : IRatingManagerService
    {
        private readonly IDbContextFactory<MajorAuthorDbContext> _contextFactory;
        private readonly IRatingService _ratingService;

        public RatingManagerService(
            IDbContextFactory<MajorAuthorDbContext> contextFactory,
            IRatingService ratingService)
        {
            _contextFactory = contextFactory;
            _ratingService = ratingService;
        }

        public async Task UpdateBookRatingAsync(int bookId)
        {
            using var context = _contextFactory.CreateDbContext();

            var book = await context.Books
                .Include(b => b.Likes)
                .Include(b => b.Readings)
                .FirstOrDefaultAsync(b => b.Id == bookId);

            if (book == null) return;
            book.LikesCount = book.Likes.Count;
            book.ReadsCount = book.Readings.Count;
            var now = DateTime.UtcNow;
            var weekStart = GetStartOfWeek(now);
            var monthStart = new DateTime(now.Year, now.Month, 1);
            var yearStart = new DateTime(now.Year, 1, 1);

            // Общий рейтинг
            book.Rating = _ratingService.CalculateGlobalRating(book.Likes.Count, book.Readings.Count);

            // Рейтинг за неделю
            var weeklyLikes = book.Likes.Count(bl => bl.LikeDate >= weekStart);
            var weeklyReads = book.Readings.Count(br => br.ReadDate >= weekStart);
            book.WeeklyRating = _ratingService.CalculateGlobalRating(weeklyLikes, weeklyReads);

            // Рейтинг за месяц
            var monthlyLikes = book.Likes.Count(bl => bl.LikeDate >= monthStart);
            var monthlyReads = book.Readings.Count(br => br.ReadDate >= monthStart);
            book.MonthlyRating = _ratingService.CalculateGlobalRating(monthlyLikes, monthlyReads);

            // Рейтинг за год
            var yearlyLikes = book.Likes.Count(bl => bl.LikeDate >= yearStart);
            var yearlyReads = book.Readings.Count(br => br.ReadDate >= yearStart);
            book.YearlyRating = _ratingService.CalculateGlobalRating(yearlyLikes, yearlyReads);
            context.Entry(book).State = EntityState.Modified;
            await context.SaveChangesAsync();
        }

        public async Task UpdatePoemRatingAsync(int poemId)
        {
            using var context = _contextFactory.CreateDbContext();

            var poem = await context.Poems
                .Include(p => p.Likes)
                .Include(p => p.Readings)
                .FirstOrDefaultAsync(p => p.Id == poemId);

            if (poem == null) return;
            poem.LikesCount = poem.Likes.Count; // Теперь в таблице Poems будет верное число
            poem.ViewsCount = poem.Readings.Count;
            var now = DateTime.UtcNow;
            var weekStart = GetStartOfWeek(now);
            var monthStart = new DateTime(now.Year, now.Month, 1);
            var yearStart = new DateTime(now.Year, 1, 1);

            // Общий рейтинг
            poem.Rating = _ratingService.CalculateGlobalRating(poem.Likes.Count, poem.Readings.Count);

            // Рейтинг за неделю
            var weeklyLikes = poem.Likes.Count(pl => pl.LikeDate >= weekStart);
            var weeklyReads = poem.Readings.Count(pr => pr.ReadDate >= weekStart);
            poem.WeeklyRating = _ratingService.CalculateGlobalRating(weeklyLikes, weeklyReads);

            // Рейтинг за месяц
            var monthlyLikes = poem.Likes.Count(pl => pl.LikeDate >= monthStart);
            var monthlyReads = poem.Readings.Count(pr => pr.ReadDate >= monthStart);
            poem.MonthlyRating = _ratingService.CalculateGlobalRating(monthlyLikes, monthlyReads);

            // Рейтинг за год
            var yearlyLikes = poem.Likes.Count(pl => pl.LikeDate >= yearStart);
            var yearlyReads = poem.Readings.Count(pr => pr.ReadDate >= yearStart);
            poem.YearlyRating = _ratingService.CalculateGlobalRating(yearlyLikes, yearlyReads);
            context.Entry(poem).State = EntityState.Modified;
            await context.SaveChangesAsync();
        }

        public async Task UpdateBlogRatingAsync(int blogId)
        {
            using var context = _contextFactory.CreateDbContext();

            var blog = await context.Blogs
                .Include(b => b.Likes)
                .Include(b => b.Readings)
                .FirstOrDefaultAsync(b => b.Id == blogId);

            if (blog == null) return;
            blog.LikesCount = blog.Likes.Count;
            blog.ViewsCount = blog.Readings.Count;
            var now = DateTime.UtcNow;
            var weekStart = GetStartOfWeek(now);
            var monthStart = new DateTime(now.Year, now.Month, 1);

            // Общий рейтинг
            blog.Rating = _ratingService.CalculateGlobalRating(blog.Likes.Count, blog.Readings.Count);

            // Рейтинг за неделю
            var weeklyLikes = blog.Likes.Count(bl => bl.LikeDate >= weekStart);
            var weeklyReads = blog.Readings.Count(br => br.ReadDate >= weekStart);
            blog.WeeklyRating = _ratingService.CalculateGlobalRating(weeklyLikes, weeklyReads);

            // Рейтинг за месяц
            var monthlyLikes = blog.Likes.Count(bl => bl.LikeDate >= monthStart);
            var monthlyReads = blog.Readings.Count(br => br.ReadDate >= monthStart);
            blog.MonthlyRating = _ratingService.CalculateGlobalRating(monthlyLikes, monthlyReads);
            context.Entry(blog).State = EntityState.Modified;
            await context.SaveChangesAsync();
        }

        public async Task UpdateAuthorRatingAsync(int authorId)
        {
            using var context = _contextFactory.CreateDbContext();

            var author = await context.Authors
                .Include(a => a.BookAuthors)
                    .ThenInclude(ba => ba.Book)
                .Include(a => a.Poems)
                .Include(a => a.Blogs)
                .FirstOrDefaultAsync(a => a.Id == authorId);

            if (author == null) return;

            var now = DateTime.UtcNow;
            var weekStart = GetStartOfWeek(now);
            var monthStart = new DateTime(now.Year, now.Month, 1);
            var yearStart = new DateTime(now.Year, 1, 1);

            // Общий рейтинг автора (сумма рейтингов всех работ)
            author.Rating = author.BookAuthors.Sum(ba => ba.Book?.Rating ?? 0)
                          + author.Poems.Sum(p => p.Rating)
                          + author.Blogs.Sum(b => b.Rating);

            // Рейтинг за неделю
            author.WeeklyRating = author.BookAuthors.Sum(ba => ba.Book?.WeeklyRating ?? 0)
                                + author.Poems.Sum(p => p.WeeklyRating)
                                + author.Blogs.Sum(b => b.WeeklyRating);

            // Рейтинг за месяц
            author.MonthlyRating = author.BookAuthors.Sum(ba => ba.Book?.MonthlyRating ?? 0)
                                 + author.Poems.Sum(p => p.MonthlyRating)
                                 + author.Blogs.Sum(b => b.MonthlyRating);

            // Рейтинг за год
            author.YearlyRating = author.BookAuthors.Sum(ba => ba.Book?.YearlyRating ?? 0)
                                + author.Poems.Sum(p => p.YearlyRating);

            await context.SaveChangesAsync();
        }

        public async Task RecalculateAllRatingsAsync()
        {
            using var context = _contextFactory.CreateDbContext();

            // Пересчитываем рейтинги всех книг
            var bookIds = await context.Books.Select(b => b.Id).ToListAsync();
            foreach (var bookId in bookIds)
            {
                await UpdateBookRatingAsync(bookId);
            }

            // Пересчитываем рейтинги всех стихов
            var poemIds = await context.Poems.Select(p => p.Id).ToListAsync();
            foreach (var poemId in poemIds)
            {
                await UpdatePoemRatingAsync(poemId);
            }

            // Пересчитываем рейтинги всех блогов
            var blogIds = await context.Blogs.Select(b => b.Id).ToListAsync();
            foreach (var blogId in blogIds)
            {
                await UpdateBlogRatingAsync(blogId);
            }

            // Пересчитываем рейтинги всех авторов
            var authorIds = await context.Authors.Select(a => a.Id).ToListAsync();
            foreach (var authorId in authorIds)
            {
                await UpdateAuthorRatingAsync(authorId);
            }
        }

        private DateTime GetStartOfWeek(DateTime dt)
        {
            int diff = (7 + (dt.DayOfWeek - DayOfWeek.Monday)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }
    }
}