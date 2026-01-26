using MajorAuthor.Data;
using MajorAuthor.Models;
using MajorAuthor.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MajorAuthor.Services
{
    public class CatalogService : ICatalogService
    {
        private readonly IDbContextFactory<MajorAuthorDbContext> _contextFactory;
        private readonly IRatingService _ratingService;

        public CatalogService(IDbContextFactory<MajorAuthorDbContext> contextFactory, IRatingService ratingService)
        {
            _contextFactory = contextFactory;
            _ratingService = ratingService;
        }

        // CatalogService.cs - обновим методы для поддержки фильтрации
        // CatalogService.cs - обновленный метод для получения всех авторов
        public async Task<CatalogViewModel> GetAllBooksAsync(string sortBy, string sortOrder, int page, int pageSize, string searchQuery = null, string genres = null, string tags = null)
        {
            using var context = _contextFactory.CreateDbContext();

            var booksQuery = context.Books
                .Include(b => b.BookAuthors)
                    .ThenInclude(ba => ba.Author)
                .Include(b => b.BookGenres)
                    .ThenInclude(bg => bg.Genre)
                .Include(b => b.BookTags)
                    .ThenInclude(bt => bt.Tag)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var lowerQuery = searchQuery.ToLower();
                booksQuery = booksQuery.Where(b =>
                    b.Title.ToLower().Contains(lowerQuery) ||
                    b.Description.ToLower().Contains(lowerQuery) ||
                    b.BookAuthors.Any(ba => ba.Author.PenName.ToLower().Contains(lowerQuery)) ||
                    b.BookGenres.Any(bg => bg.Genre.Name.ToLower().Contains(lowerQuery)) ||
                    b.BookTags.Any(bt => bt.Tag.Name.ToLower().Contains(lowerQuery))
                );
            }

            // Фильтрация по жанрам
            if (!string.IsNullOrWhiteSpace(genres))
            {
                var genreList = genres.Split(',').Select(g => g.Trim().ToLower()).ToList();
                booksQuery = booksQuery.Where(b =>
                    b.BookGenres.Any(bg => genreList.Contains(bg.Genre.Name.ToLower()))
                );
            }

            // Фильтрация по тегам
            if (!string.IsNullOrWhiteSpace(tags))
            {
                var tagList = tags.Split(',').Select(t => t.Trim().ToLower()).ToList();
                booksQuery = booksQuery.Where(b =>
                    b.BookTags.Any(bt => tagList.Contains(bt.Tag.Name.ToLower()))
                );
            }

            booksQuery = ApplySorting(booksQuery, sortBy, sortOrder);

            var totalCount = await booksQuery.CountAsync();

            var books = await booksQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewModel = new CatalogViewModel
            {
                Books = books.Select(b => new CatalogViewModel.BookDisplayModel
                {
                    Id = b.Id,
                    Title = b.Title,
                    AuthorName = b.BookAuthors.Select(ba => ba.Author.PenName).FirstOrDefault() ?? "Неизвестен",
                    Authors = b.BookAuthors.Select(ba => ba.Author.PenName).ToList(), // Получаем всех авторов
                    CoverImageUrl = b.CoverImageUrl,
                    ReadsCount = b.ReadsCount,
                    LikesCount = b.LikesCount,
                    IsAdultContent = b.IsAdultContent,
                    Rating = _ratingService.CalculateGlobalRating(b.LikesCount, b.ReadsCount),
                    PublicationDate = b.PublicationDate,
                    Description = b.Description,
                    Genres = b.BookGenres.Select(bg => bg.Genre.Name).ToList(),
                    Tags = b.BookTags.Select(bt => bt.Tag.Name).ToList()
                }).ToList(),
                SortBy = sortBy,
                SortOrder = sortOrder,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                SearchQuery = searchQuery,
                SelectedGenres = genres,
                SelectedTags = tags
            };

            viewModel.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return viewModel;
        }

        public async Task<CatalogViewModel> SearchBooksAsync(string query, string sortBy, string sortOrder, int page, int pageSize, string genres = null, string tags = null)
        {
            return await GetAllBooksAsync(sortBy, sortOrder, page, pageSize, query, genres, tags);
        }

        // Добавим методы для получения всех жанров и тегов
        public async Task<List<string>> GetAllGenresAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Genres
                .Where(g => g.BookGenres.Any())
                .Select(g => g.Name)
                .Distinct()
                .OrderBy(g => g)
                .ToListAsync();
        }

        public async Task<List<string>> GetAllTagsAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Tags
                .Where(t => t.BookTags.Any())
                .Select(t => t.Name)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();
        }

        public async Task<CatalogViewModel> GetAllPoemsAsync(string sortBy, string sortOrder, int page, int pageSize, string searchQuery = null)
        {
            using var context = _contextFactory.CreateDbContext();

            var poemsQuery = context.Poems
                .Include(p => p.Author)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var lowerQuery = searchQuery.ToLower();
                poemsQuery = poemsQuery.Where(p =>
                    p.Title.ToLower().Contains(lowerQuery) ||
                    p.Content.ToLower().Contains(lowerQuery) ||
                    p.Author.PenName.ToLower().Contains(lowerQuery)
                );
            }

            poemsQuery = ApplySorting(poemsQuery, sortBy, sortOrder);

            var totalCount = await poemsQuery.CountAsync();

            var poems = await poemsQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewModel = new CatalogViewModel
            {
                Poems = poems.Select(p => new CatalogViewModel.PoemDisplayModel
                {
                    Id = p.Id,
                    Title = p.Title,
                    AuthorName = p.Author.PenName,
                    ContentSnippet = p.Content.Length > 150 ? p.Content.Substring(0, 150) + "..." : p.Content,
                    ReadsCount = p.ViewsCount,
                    LikesCount = p.LikesCount,
                    CommentsCount = p.Comments.Count,
                    Rating = _ratingService.CalculateGlobalRating(p.LikesCount, p.ViewsCount),
                    PublicationDate = p.PublicationDate,
                }).ToList(),
                SortBy = sortBy,
                SortOrder = sortOrder,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                SearchQuery = searchQuery
            };

            viewModel.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return viewModel;
        }

        public async Task<CatalogViewModel> SearchPoemsAsync(string query, string sortBy, string sortOrder, int page, int pageSize)
        {
            return await GetAllPoemsAsync(sortBy, sortOrder, page, pageSize, query);
        }

        public async Task<CatalogViewModel> GetAllBlogsAsync(string sortBy, string sortOrder, int page, int pageSize, string searchQuery = null)
        {
            using var context = _contextFactory.CreateDbContext();

            var blogsQuery = context.Blogs
                .Include(b => b.Author)
                .Include(b => b.Comments)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var lowerQuery = searchQuery.ToLower();
                blogsQuery = blogsQuery.Where(b =>
                    b.Title.ToLower().Contains(lowerQuery) ||
                    b.Content.ToLower().Contains(lowerQuery) ||
                    b.Author.PenName.ToLower().Contains(lowerQuery)
                );
            }

            blogsQuery = ApplySorting(blogsQuery, sortBy, sortOrder);

            var totalCount = await blogsQuery.CountAsync();

            var blogs = await blogsQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewModel = new CatalogViewModel
            {
                Blogs = blogs.Select(b => new CatalogViewModel.BlogDisplayModel
                {
                    Id = b.Id,
                    Title = b.Title,
                    AuthorName = b.Author.PenName,
                    ContentSnippet = b.Content.Length > 200 ? b.Content.Substring(0, 200) + "..." : b.Content,
                    CommentsCount = b.Comments.Count,
                    ViewsCount = b.ViewsCount,
                    LikesCount = b.LikesCount,
                    Rating = _ratingService.CalculateGlobalRating(b.LikesCount, b.ViewsCount),
                    PublicationDate = b.PublicationDate,
                }).ToList(),
                SortBy = sortBy,
                SortOrder = sortOrder,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                SearchQuery = searchQuery
            };

            viewModel.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return viewModel;
        }

        public async Task<CatalogViewModel> SearchBlogsAsync(string query, string sortBy, string sortOrder, int page, int pageSize)
        {
            return await GetAllBlogsAsync(sortBy, sortOrder, page, pageSize, query);
        }

        // Остальные методы остаются без изменений...
        private IQueryable<Book> ApplySorting(IQueryable<Book> query, string sortBy, string sortOrder)
        {
            return (sortBy?.ToLower(), sortOrder?.ToLower()) switch
            {
                ("date", "asc") => query.OrderBy(b => b.PublicationDate),
                ("date", "desc") => query.OrderByDescending(b => b.PublicationDate),
                ("likes", "asc") => query.OrderBy(b => b.LikesCount),
                ("likes", "desc") => query.OrderByDescending(b => b.LikesCount),
                ("reads", "asc") => query.OrderBy(b => b.ReadsCount),
                ("reads", "desc") => query.OrderByDescending(b => b.ReadsCount),
                ("rating", "asc") => query.OrderBy(b => b.LikesCount * 2 + b.ReadsCount * 0.1),
                ("rating", "desc") => query.OrderByDescending(b => b.LikesCount * 2 + b.ReadsCount * 0.1),
                _ => query.OrderByDescending(b => b.LikesCount * 2 + b.ReadsCount * 0.1)
            };
        }

        private IQueryable<Poem> ApplySorting(IQueryable<Poem> query, string sortBy, string sortOrder)
        {
            return (sortBy?.ToLower(), sortOrder?.ToLower()) switch
            {
                ("date", "asc") => query.OrderBy(p => p.PublicationDate),
                ("date", "desc") => query.OrderByDescending(p => p.PublicationDate),
                ("likes", "asc") => query.OrderBy(p => p.LikesCount),
                ("likes", "desc") => query.OrderByDescending(p => p.LikesCount),
                ("reads", "asc") => query.OrderBy(p => p.ViewsCount),
                ("reads", "desc") => query.OrderByDescending(p => p.ViewsCount),
                ("rating", "asc") => query.OrderBy(p => p.LikesCount * 2 + p.ViewsCount * 0.1),
                ("rating", "desc") => query.OrderByDescending(p => p.LikesCount * 2 + p.ViewsCount * 0.1),
                _ => query.OrderByDescending(p => p.LikesCount * 2 + p.ViewsCount * 0.1)
            };
        }

        private IQueryable<Blog> ApplySorting(IQueryable<Blog> query, string sortBy, string sortOrder)
        {
            return (sortBy?.ToLower(), sortOrder?.ToLower()) switch
            {
                ("date", "asc") => query.OrderBy(b => b.PublicationDate),
                ("date", "desc") => query.OrderByDescending(b => b.PublicationDate),
                ("likes", "asc") => query.OrderBy(b => b.LikesCount),
                ("likes", "desc") => query.OrderByDescending(b => b.LikesCount),
                ("views", "asc") => query.OrderBy(b => b.ViewsCount),
                ("views", "desc") => query.OrderByDescending(b => b.ViewsCount),
                ("comments", "asc") => query.OrderBy(b => b.Comments.Count),
                ("comments", "desc") => query.OrderByDescending(b => b.Comments.Count),
                ("rating", "asc") => query.OrderBy(b => b.LikesCount * 2 + b.ViewsCount * 0.1),
                ("rating", "desc") => query.OrderByDescending(b => b.LikesCount * 2 + b.ViewsCount * 0.1),
                _ => query.OrderByDescending(b => b.LikesCount * 2 + b.ViewsCount * 0.1)
            };
        }
    }
}