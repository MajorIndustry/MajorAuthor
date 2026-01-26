// Services/HomeServiceWithFactory.cs
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
    public class HomeServiceWithFactory : IHomeService
    {
        private readonly IDbContextFactory<MajorAuthorDbContext> _contextFactory;
        private readonly IRatingService _ratingService;

        public HomeServiceWithFactory(IDbContextFactory<MajorAuthorDbContext> contextFactory, IRatingService ratingService)
        {
            _contextFactory = contextFactory;
            _ratingService = ratingService;
        }

        public async Task<HomeViewModel> GetHomeDataAsync(string userId)
        {
            var viewModel = new HomeViewModel
            {
                IsUserLoggedIn = !string.IsNullOrEmpty(userId)
            };

            var now = DateTime.UtcNow;
            var weekStart = GetStartOfWeek(now);
            var monthStart = new DateTime(now.Year, now.Month, 1);
            var yearStart = new DateTime(now.Year, 1, 1);

            var tasks = new List<Task>();

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

            // Запрос 2: Популярные книги (общий рейтинг) - ОГРАНИЧЕНО ДО 8
            var popularBooksTask = Task.Run(async () =>
            {
                using var context = _contextFactory.CreateDbContext();
                var books = await context.Books
                  .Include(b => b.BookAuthors)
                    .ThenInclude(ba => ba.Author)
                  .Include(b => b.BookTags)
                    .ThenInclude(t => t.Tag)
                  .Include(b => b.BookGenres)
                    .ThenInclude(g => g.Genre)
                  .Where(b => b.IsPublic)
                  .OrderByDescending(b => b.Rating)
                  .Take(8)
                  .ToListAsync();

                return books.Select(b => new HomeViewModel.BookDisplayModel
                {
                    Id = b.Id,
                    Title = b.Title,
                    AuthorName = b.BookAuthors.Select(ba => ba.Author.PenName).FirstOrDefault() ?? "Неизвестен",
                    CoverImageUrl = b.CoverImageUrl,
                    ReadsCount = b.ReadsCount,
                    LikesCount = b.LikesCount,
                    IsAdultContent = b.IsAdultContent,
                    Rating = b.Rating,
                    WeeklyRating = b.WeeklyRating,
                    MonthlyRating = b.MonthlyRating,
                    YearlyRating = b.YearlyRating,
                    IsPublic = b.IsPublic,
                    Genres = b.BookGenres.Select(bg => bg.Genre.Name).ToList(),
                    Tags = b.BookTags.Select(bt => bt.Tag.Name).ToList(),
                    Authors = b.BookAuthors.Select(ba => ba.Author.PenName).ToList(),
                    Description = b.Description,
                    PublicationDate = b.PublicationDate
                }).ToList();
            });
            tasks.Add(popularBooksTask);

            // Запрос 3: Популярные книги за текущую неделю - ОГРАНИЧЕНО ДО 8
            var weeklyBooksTask = Task.Run(async () =>
            {
                using var context = _contextFactory.CreateDbContext();
                var books = await context.Books
                  .Include(b => b.BookAuthors)
                    .ThenInclude(ba => ba.Author)
                  .Include(b => b.BookGenres)
                    .ThenInclude(g => g.Genre)
                  .Include(b => b.BookTags)
                    .ThenInclude(t => t.Tag)
                  .Where(b => b.IsPublic)
                  .OrderByDescending(b => b.WeeklyRating)
                  .ThenByDescending(b => b.Rating)
                  .Take(8)
                  .ToListAsync();

                return books.Select(b => new HomeViewModel.BookDisplayModel
                {
                    Id = b.Id,
                    Title = b.Title,
                    AuthorName = b.BookAuthors.Select(ba => ba.Author.PenName).FirstOrDefault() ?? "Неизвестен",
                    CoverImageUrl = b.CoverImageUrl,
                    ReadsCount = b.ReadsCount,
                    LikesCount = b.LikesCount,
                    IsAdultContent = b.IsAdultContent,
                    Rating = b.Rating,
                    WeeklyRating = b.WeeklyRating,
                    IsNew = b.PublicationDate >= weekStart,
                    IsPublic = b.IsPublic,
                    Genres = b.BookGenres.Select(bg => bg.Genre.Name).ToList(),
                    Tags = b.BookTags.Select(bt => bt.Tag.Name).ToList(),
                    Authors = b.BookAuthors.Select(ba => ba.Author.PenName).ToList(),
                    Description = b.Description,
                    PublicationDate = b.PublicationDate
                }).ToList();
            });
            tasks.Add(weeklyBooksTask);

            // Запрос 4: Популярные книги за текущий месяц - ОГРАНИЧЕНО ДО 8
            var monthlyBooksTask = Task.Run(async () =>
            {
                using var context = _contextFactory.CreateDbContext();
                var books = await context.Books
                  .Include(b => b.BookAuthors)
                    .ThenInclude(ba => ba.Author)
                  .Include(b => b.BookTags)
                    .ThenInclude(t => t.Tag)
                  .Include(b => b.BookGenres)
                    .ThenInclude(g => g.Genre)
                  .Where(b => b.IsPublic)
                  .OrderByDescending(b => b.MonthlyRating)
                  .ThenByDescending(b => b.Rating)
                  .Take(8)
                  .ToListAsync();

                return books.Select(b => new HomeViewModel.BookDisplayModel
                {
                    Id = b.Id,
                    Title = b.Title,
                    AuthorName = b.BookAuthors.Select(ba => ba.Author.PenName).FirstOrDefault() ?? "Неизвестен",
                    CoverImageUrl = b.CoverImageUrl,
                    ReadsCount = b.ReadsCount,
                    LikesCount = b.LikesCount,
                    IsAdultContent = b.IsAdultContent,
                    Rating = b.Rating,
                    MonthlyRating = b.MonthlyRating,
                    IsPublic = b.IsPublic,
                    Genres = b.BookGenres.Select(bg => bg.Genre.Name).ToList(),
                    Tags = b.BookTags.Select(bt => bt.Tag.Name).ToList(),
                    Authors = b.BookAuthors.Select(ba => ba.Author.PenName).ToList(),
                    Description = b.Description,
                    PublicationDate = b.PublicationDate
                }).ToList();
            });
            tasks.Add(monthlyBooksTask);

            // Запрос 5: Популярные книги за текущий год - ОГРАНИЧЕНО ДО 8
            var yearlyBooksTask = Task.Run(async () =>
            {
                using var context = _contextFactory.CreateDbContext();
                var books = await context.Books
                  .Include(b => b.BookAuthors)
                    .ThenInclude(ba => ba.Author)
                  .Include(b => b.BookTags)
                    .ThenInclude(t => t.Tag)
                  .Include(b => b.BookGenres)
                    .ThenInclude(g => g.Genre)
                  .Where(b => b.IsPublic)
                  .OrderByDescending(b => b.YearlyRating)
                  .ThenByDescending(b => b.Rating)
                  .Take(8)
                  .ToListAsync();

                return books.Select(b => new HomeViewModel.BookDisplayModel
                {
                    Id = b.Id,
                    Title = b.Title,
                    AuthorName = b.BookAuthors.Select(ba => ba.Author.PenName).FirstOrDefault() ?? "Неизвестен",
                    CoverImageUrl = b.CoverImageUrl,
                    ReadsCount = b.ReadsCount,
                    LikesCount = b.LikesCount,
                    IsAdultContent = b.IsAdultContent,
                    Rating = b.Rating,
                    YearlyRating = b.YearlyRating,
                    IsPublic = b.IsPublic,
                    Genres = b.BookGenres.Select(bg => bg.Genre.Name).ToList(),
                    Tags = b.BookTags.Select(bt => bt.Tag.Name).ToList(),
                    Authors = b.BookAuthors.Select(ba => ba.Author.PenName).ToList(),
                    Description = b.Description,
                    PublicationDate = b.PublicationDate
                }).ToList();
            });
            tasks.Add(yearlyBooksTask);

            // Запрос 6: Популярные авторы - ОГРАНИЧЕНО ДО 5
            var popularAuthorsTask = Task.Run(async () =>
            {
                using var context = _contextFactory.CreateDbContext();
                var authors = await context.Authors
                  .Include(a => a.BookAuthors)
                    .ThenInclude(ba => ba.Book)
                  .Include(a => a.Poems)
                  .Include(a => a.ApplicationUser)
                  .ToListAsync();

                var authorRatings = authors.Select(a => new
                {
                    Author = a,
                    TotalBookLikes = a.BookAuthors.Sum(ba => ba.Book.LikesCount),
                    TotalBookViews = a.BookAuthors.Sum(ba => ba.Book.ReadsCount),
                    TotalPoemLikes = a.Poems.Sum(p => p.LikesCount),
                    TotalPoemViews = a.Poems.Sum(p => p.ViewsCount)
                })
                .Select(x => new HomeViewModel.AuthorDisplayModel
                {
                    Id = x.Author.Id,
                    Name = x.Author.PenName,
                    PhotoUrl = x.Author.ApplicationUser.ProfilePictureUrl,
                    BooksCount = x.Author.BookAuthors.Count(),
                    TotalReadsCount = x.TotalBookViews + x.TotalPoemViews,
                    TotalLikesCount = x.TotalBookLikes + x.TotalPoemLikes,
                    Rating = _ratingService.CalculateGlobalRating(
                        x.TotalBookLikes + x.TotalPoemLikes,
                        x.TotalBookViews + x.TotalPoemViews
                    ),
                    IsNew = x.Author.AuthorProfileCreationDate >= monthStart
                })
                .OrderByDescending(a => a.Rating)
                .Take(5)
                .ToList();

                return authorRatings;
            });
            tasks.Add(popularAuthorsTask);

            // Запрос 7: Популярные стихи (общий рейтинг) - ОГРАНИЧЕНО ДО 5
            var popularPoemsTask = Task.Run(async () =>
            {
                using var context = _contextFactory.CreateDbContext();
                var poems = await context.Poems
                  .Include(p => p.Author)
                  .OrderByDescending(p => p.Rating)
                  .Take(5)
                  .ToListAsync();

                return poems.Select(p => new HomeViewModel.PoemDisplayModel
                {
                    Id = p.Id,
                    Title = p.Title,
                    AuthorName = p.Author.PenName,
                    ContentSnippet = p.Content.Length > 100 ? p.Content.Substring(0, 100) + "..." : p.Content,
                    ReadsCount = p.ViewsCount,
                    LikesCount = p.LikesCount,
                    CommentsCount = p.Comments.Count,
                    Rating = p.Rating,
                    WeeklyRating = p.WeeklyRating,
                    MonthlyRating = p.MonthlyRating,
                    YearlyRating = p.YearlyRating,
                    IsNew = p.PublicationDate >= weekStart
                }).ToList();
            });
            tasks.Add(popularPoemsTask);

            // Запрос 8: Популярные стихи за текущую неделю - ОГРАНИЧЕНО ДО 5
            var weeklyPoemsTask = Task.Run(async () =>
            {
                using var context = _contextFactory.CreateDbContext();
                var poems = await context.Poems
                  .Include(p => p.Author)
                  .OrderByDescending(p => p.WeeklyRating)
                  .ThenByDescending(p => p.Rating)
                  .Take(5)
                  .ToListAsync();

                return poems.Select(p => new HomeViewModel.PoemDisplayModel
                {
                    Id = p.Id,
                    Title = p.Title,
                    AuthorName = p.Author.PenName,
                    ContentSnippet = p.Content.Length > 100 ? p.Content.Substring(0, 100) + "..." : p.Content,
                    ReadsCount = p.ViewsCount,
                    LikesCount = p.LikesCount,
                    Rating = p.Rating,
                    WeeklyRating = p.WeeklyRating,
                    IsNew = p.PublicationDate >= weekStart
                }).ToList();
            });
            tasks.Add(weeklyPoemsTask);

            // Запрос 9: Популярные стихи за текущий месяц - ОГРАНИЧЕНО ДО 5
            var monthlyPoemsTask = Task.Run(async () =>
            {
                using var context = _contextFactory.CreateDbContext();
                var poems = await context.Poems
                  .Include(p => p.Author)
                  .OrderByDescending(p => p.MonthlyRating)
                  .ThenByDescending(p => p.Rating)
                  .Take(5)
                  .ToListAsync();

                return poems.Select(p => new HomeViewModel.PoemDisplayModel
                {
                    Id = p.Id,
                    Title = p.Title,
                    AuthorName = p.Author.PenName,
                    ContentSnippet = p.Content.Length > 100 ? p.Content.Substring(0, 100) + "..." : p.Content,
                    ReadsCount = p.ViewsCount,
                    LikesCount = p.LikesCount,
                    Rating = p.Rating,
                    MonthlyRating = p.MonthlyRating,
                    IsNew = p.PublicationDate >= monthStart
                }).ToList();
            });
            tasks.Add(monthlyPoemsTask);

            // Запрос 10: Новые стихи - ОГРАНИЧЕНО ДО 5
            var newPoemsTask = Task.Run(async () =>
            {
                using var context = _contextFactory.CreateDbContext();
                var poems = await context.Poems
                  .Include(p => p.Author)
                  .Where(p => p.PublicationDate >= DateTime.UtcNow.AddDays(-7))
                  .OrderByDescending(p => p.PublicationDate)
                  .Take(5)
                  .ToListAsync();

                return poems.Select(p => new HomeViewModel.PoemDisplayModel
                {
                    Id = p.Id,
                    Title = p.Title,
                    AuthorName = p.Author.PenName,
                    ContentSnippet = p.Content.Length > 100 ? p.Content.Substring(0, 100) + "..." : p.Content,
                    CreationInfo = TimeHelper.GetRelativeTime(p.PublicationDate),
                    ReadsCount = p.ViewsCount,
                    LikesCount = p.LikesCount,
                    Rating = p.Rating
                }).ToList();
            });
            tasks.Add(newPoemsTask);

            // Запрос 11: Популярные блоги - ОГРАНИЧЕНО ДО 5
            var popularBlogsTask = Task.Run(async () =>
            {
                using var context = _contextFactory.CreateDbContext();
                var blogs = await context.Blogs
                  .Include(b => b.Author)
                  .Include(b => b.Comments)
                  .OrderByDescending(b => b.Rating)
                  .Take(5)
                  .ToListAsync();

                return blogs.Select(b => new HomeViewModel.BlogDisplayModel
                {
                    Id = b.Id,
                    Title = b.Title,
                    AuthorName = b.Author.PenName,
                    ContentSnippet = b.Content.Length > 100 ? b.Content.Substring(0, 100) + "..." : b.Content,
                    LikesCount = b.LikesCount,
                    ViewsCount = b.ViewsCount,
                    CommentsCount = b.Comments.Count,
                    Rating = b.Rating,
                    WeeklyRating = b.WeeklyRating,
                    MonthlyRating = b.MonthlyRating
                }).ToList();
            });
            tasks.Add(popularBlogsTask);

            // Запрос 12: Недавно обновленные книги - ОГРАНИЧЕНО ДО 8
            var recentlyUpdatedBooksTask = Task.Run(async () =>
            {
                using var context = _contextFactory.CreateDbContext();
                var books = await context.Books
                  .Include(b => b.BookAuthors)
                    .ThenInclude(ba => ba.Author)
                  .Include(b => b.BookTags)
                    .ThenInclude(t => t.Tag)
                  .Include(b => b.BookGenres)
                    .ThenInclude(g => g.Genre)
                  .Where(b => b.LastUpdateTime >= DateTime.UtcNow.AddDays(-7) && b.IsPublic)
                  .OrderByDescending(b => b.LastUpdateTime)
                  .Take(8)
                  .ToListAsync();

                return books.Select(b => new HomeViewModel.BookDisplayModel
                {
                    Id = b.Id,
                    Title = b.Title,
                    AuthorName = b.BookAuthors.Select(ba => ba.Author.PenName).FirstOrDefault() ?? "Неизвестен",
                    CoverImageUrl = b.CoverImageUrl,
                    UpdateInfo = TimeHelper.GetRelativeTime(b.LastUpdateTime),
                    ReadsCount = b.ReadsCount,
                    LikesCount = b.LikesCount,
                    Rating = b.Rating,
                    IsPublic = b.IsPublic,
                    Genres = b.BookGenres.Select(bg => bg.Genre.Name).ToList(),
                    Tags = b.BookTags.Select(bt => bt.Tag.Name).ToList(),
                    Authors = b.BookAuthors.Select(ba => ba.Author.PenName).ToList(),
                    Description = b.Description,
                    PublicationDate = b.PublicationDate
                }).ToList();
            });
            tasks.Add(recentlyUpdatedBooksTask);

            // Запрос 13: Продвигаемые книги - ОГРАНИЧЕНО ДО 8
            var promotedBooksTask = Task.Run(async () =>
            {
                using var context = _contextFactory.CreateDbContext();
                var promotions = await context.Promotions
                  .Include(p => p.Book)
                    .ThenInclude(b => b.BookTags)
                    .ThenInclude(t => t.Tag)
                  .Include(p => p.Book)
                    .ThenInclude(b => b.BookAuthors)
                      .ThenInclude(ba => ba.Author)
                  .Include(p => p.Book)
                    .ThenInclude(b => b.BookGenres)
                    .ThenInclude(g => g.Genre)
                  .Where(p => p.StartDate <= DateTime.UtcNow && p.EndDate >= DateTime.UtcNow && p.Book.IsPublic)
                  .Take(8)
                  .ToListAsync();

                return promotions.Select(p => new HomeViewModel.BookDisplayModel
                {
                    Id = p.Book.Id,
                    Title = p.Book.Title,
                    AuthorName = p.Book.BookAuthors.Select(ba => ba.Author.PenName).FirstOrDefault() ?? "Неизвестен",
                    CoverImageUrl = p.Book.CoverImageUrl,
                    ReadsCount = p.Book.ReadsCount,
                    LikesCount = p.Book.LikesCount,
                    IsAdultContent = p.Book.IsAdultContent,
                    Rating = p.Book.Rating,
                    IsPublic = p.Book.IsPublic,
                    Genres = p.Book.BookGenres.Select(bg => bg.Genre.Name).ToList(),
                    Tags = p.Book.BookTags.Select(bt => bt.Tag.Name).ToList(),
                    Authors = p.Book.BookAuthors.Select(ba => ba.Author.PenName).ToList(),
                    Description = p.Book.Description,
                    PublicationDate = p.Book.PublicationDate
                }).ToList();
            });
            tasks.Add(promotedBooksTask);

            // Если пользователь авторизован, добавляем запрос на рекомендации - ОГРАНИЧЕНО ДО 8
            if (!string.IsNullOrEmpty(userId))
            {
                var recommendedBooksTask = Task.Run(async () =>
                {
                    return await GetRecommendedBooksAsync(userId);
                });
                tasks.Add(recommendedBooksTask);
            }

            // Ожидаем завершения всех задач
            await Task.WhenAll(tasks);

            // Заполняем ViewModel данными из завершенных задач
            viewModel.AvailableGenres = genresTask.Result;
            viewModel.PopularBooks = popularBooksTask.Result;
            viewModel.WeeklyPopularBooks = weeklyBooksTask.Result;
            viewModel.MonthlyPopularBooks = monthlyBooksTask.Result;
            viewModel.YearlyPopularBooks = yearlyBooksTask.Result;
            viewModel.PopularAuthors = popularAuthorsTask.Result;
            viewModel.PopularPoems = popularPoemsTask.Result;
            viewModel.WeeklyPopularPoems = weeklyPoemsTask.Result;
            viewModel.MonthlyPopularPoems = monthlyPoemsTask.Result;
            viewModel.NewPoems = newPoemsTask.Result;
            viewModel.PopularBlogs = popularBlogsTask.Result;
            viewModel.RecentlyUpdatedBooks = recentlyUpdatedBooksTask.Result;
            viewModel.PromotedBooks = promotedBooksTask.Result;

            if (!string.IsNullOrEmpty(userId))
            {
                viewModel.RecommendedBooks = ((Task<List<HomeViewModel.BookDisplayModel>>)tasks.Last()).Result;
            }

            return viewModel;
        }

        private DateTime GetStartOfWeek(DateTime dt)
        {
            int diff = (7 + (dt.DayOfWeek - DayOfWeek.Monday)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }

        public async Task<List<HomeViewModel.BookDisplayModel>> GetBooksByGenreAsync(int genreId)
        {
            using var context = _contextFactory.CreateDbContext();
            var books = await context.Books
              .Include(b => b.BookAuthors)
                .ThenInclude(ba => ba.Author)
              .Include(b => b.BookGenres)
                .ThenInclude(g => g.Genre)
              .Include(b => b.BookTags)
                .ThenInclude(t => t.Tag)
              .Where(b => b.BookGenres.Any(bg => bg.GenreId == genreId) && b.IsPublic)
              .OrderByDescending(b => b.Rating)
              .Take(8)
              .ToListAsync();

            return books.Select(b => new HomeViewModel.BookDisplayModel
            {
                Id = b.Id,
                Title = b.Title,
                AuthorName = b.BookAuthors.Select(ba => ba.Author.PenName).FirstOrDefault() ?? "Неизвестен",
                CoverImageUrl = b.CoverImageUrl,
                ReadsCount = b.ReadsCount,
                LikesCount = b.LikesCount,
                IsAdultContent = b.IsAdultContent,
                Rating = b.Rating,
                Description = b.Description,
                PublicationDate = b.PublicationDate,
                Genres = b.BookGenres.Select(bg => bg.Genre.Name).ToList(),
                Tags = b.BookTags.Select(bt => bt.Tag.Name).ToList(),
                Authors = b.BookAuthors.Select(ba => ba.Author.PenName).ToList()
            }).ToList();
        }

        private async Task<List<HomeViewModel.BookDisplayModel>> GetRecommendedBooksAsync(string userId)
        {
            using var context = _contextFactory.CreateDbContext();

            var userPreferredGenreIds = await context.UserPreferredGenres
              .Where(upg => upg.ApplicationUserId == userId)
              .Select(upg => upg.GenreId)
              .ToListAsync();

            var userPreferredTagIds = await context.UserPreferredTags
              .Where(upt => upt.ApplicationUserId == userId)
              .Select(upt => upt.TagId)
              .ToListAsync();

            var books = await context.Books
              .Include(b => b.BookAuthors)
                .ThenInclude(ba => ba.Author)
              .Include(b => b.BookGenres)
                .ThenInclude(g => g.Genre)
              .Include(b => b.BookTags)
                .ThenInclude(t => t.Tag)
              .Where(b => b.IsPublic && (b.BookGenres.Any(bg => userPreferredGenreIds.Contains(bg.GenreId)) ||
                    b.BookTags.Any(bt => userPreferredTagIds.Contains(bt.TagId))))
              .Take(8)
              .ToListAsync();

            return books.Select(b => new HomeViewModel.BookDisplayModel
            {
                Id = b.Id,
                Title = b.Title,
                AuthorName = b.BookAuthors.Select(ba => ba.Author.PenName).FirstOrDefault() ?? "Неизвестен",
                CoverImageUrl = b.CoverImageUrl,
                ReadsCount = b.ReadsCount,
                LikesCount = b.LikesCount,
                IsAdultContent = b.IsAdultContent,
                Rating = b.Rating,
                IsPublic = b.IsPublic,
                Description = b.Description,
                PublicationDate = b.PublicationDate,
                Genres = b.BookGenres.Select(bg => bg.Genre.Name).ToList(),
                Tags = b.BookTags.Select(bt => bt.Tag.Name).ToList(),
                Authors = b.BookAuthors.Select(ba => ba.Author.PenName).ToList()
            })
            .OrderByDescending(b => b.Rating)
            .Take(8)
            .ToList();
        }

        public async Task<IEnumerable<HomeViewModel.PoemDisplayModel>> GetPopularPoemsForPeriodAsync(string period)
        {
            using var context = _contextFactory.CreateDbContext();

            IQueryable<Poem> query = context.Poems.Include(p => p.Author);

            query = period.ToLower() switch
            {
                "month" => query.OrderByDescending(p => p.MonthlyRating),
                "year" => query.OrderByDescending(p => p.YearlyRating),
                _ => query.OrderByDescending(p => p.WeeklyRating), // "week" по умолчанию
            };

            var poems = await query.Take(5).ToListAsync();

            return poems.Select(p => new HomeViewModel.PoemDisplayModel
            {
                Id = p.Id,
                Title = p.Title,
                AuthorName = p.Author.PenName,
                ContentSnippet = p.Content.Length > 100 ? p.Content.Substring(0, 100) + "..." : p.Content,
                ReadsCount = p.ViewsCount,
                LikesCount = p.LikesCount,
                Rating = p.Rating,
                WeeklyRating = p.WeeklyRating,
                MonthlyRating = p.MonthlyRating,
                YearlyRating = p.YearlyRating
            });
        }
    }
}