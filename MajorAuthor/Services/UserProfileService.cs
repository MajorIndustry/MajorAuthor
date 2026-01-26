using MajorAuthor.Data;
using MajorAuthor.Data.Entities;
using MajorAuthor.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace MajorAuthor.Services
{
    public class UserProfileService : IUserProfileService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuthorService _authorService;
        private readonly MajorAuthorDbContext _context;
        private readonly IRatingService _ratingService;

        public UserProfileService(UserManager<ApplicationUser> userManager,
                                IAuthorService authorService,
                                MajorAuthorDbContext context,
                                IRatingService ratingService)
        {
            _userManager = userManager;
            _authorService = authorService;
            _context = context;
            _ratingService = ratingService;
        }

        public async Task<MyProfileViewModel> GetUserProfileViewModelAsync(string userId, string currentUserId = null)
        {
            var user = await _context.Users
                .AsNoTracking()
                .Include(u => u.AuthorProfile)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return null;
            }

            var viewModel = new MyProfileViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                IsAuthor = user.AuthorProfile != null,
                DisplayName = user.UserName,
                PhotoUrl = string.IsNullOrEmpty(user.ProfilePictureUrl) ? "/images/default_avatar.png" : user.ProfilePictureUrl,
                IsOwnProfile = currentUserId == userId,
                IsFollowing = false
            };

            if (viewModel.IsAuthor)
            {
                viewModel.AuthorId = user.AuthorProfile.Id;
                await PopulateAuthorProfile(viewModel, user.AuthorProfile.Id, currentUserId);
            }
            else
            {
                await PopulateUserProfile(viewModel, userId, currentUserId);
            }

            // Заполняем подписки для всех пользователей
            await PopulateSubscriptions(viewModel, userId);

            return viewModel;
        }

        // В файле UserProfileService.cs
        private async Task PopulateAuthorProfile(MyProfileViewModel viewModel, int authorId, string currentUserId)
        {
            var author = await _context.Authors
                .AsNoTracking()
                .Include(a => a.ApplicationUser)
                .FirstOrDefaultAsync(a => a.Id == authorId);

            if (author == null) return;

            viewModel.DisplayName = string.IsNullOrEmpty(author.PenName) ? viewModel.UserName : author.PenName;
            viewModel.FullName = author.ApplicationUser?.FullName ?? viewModel.UserName;
            viewModel.PhotoUrl = string.IsNullOrEmpty(author.ApplicationUser?.ProfilePictureUrl)
                ? viewModel.PhotoUrl
                : author.ApplicationUser.ProfilePictureUrl;

            // Книги автора с вычислением рейтинга через IRatingService
            var authoredBooks = await _context.BookAuthors
                .Where(ba => ba.AuthorId == authorId)
                .Include(ba => ba.Book)
                    .ThenInclude(b => b.BookGenres)
                        .ThenInclude(bg => bg.Genre)
                .Include(ba => ba.Book)
                    .ThenInclude(b => b.BookTags)
                        .ThenInclude(bt => bt.Tag)
                .Select(ba => new MyProfileViewModel.BookDisplayModel
                {
                    Id = ba.Book.Id,
                    Title = ba.Book.Title,
                    AuthorName = author.PenName ?? viewModel.UserName,
                    Authors = new List<string> { author.PenName ?? viewModel.UserName },
                    CoverImageUrl = ba.Book.CoverImageUrl,
                    ReadsCount = ba.Book.ReadsCount,
                    LikesCount = ba.Book.LikesCount,
                    IsAdultContent = ba.Book.IsAdultContent,
                    Rating = _ratingService.CalculateGlobalRating(ba.Book.LikesCount, ba.Book.ReadsCount), // Используем сервис рейтингов
                    Description = ba.Book.Description ?? string.Empty,
                    Genres = ba.Book.BookGenres.Select(bg => bg.Genre.Name).ToList(),
                    Tags = ba.Book.BookTags.Select(bt => bt.Tag.Name).ToList(),
                    PublicationDate = ba.Book.PublicationDate
                })
                .ToListAsync();

            viewModel.AuthoredBooks = authoredBooks;

            // Стихи автора
            var authoredPoems = await _context.Poems
                .Where(p => p.AuthorId == authorId)
                .Select(poem => new MyProfileViewModel.PoemDisplayModel
                {
                    Id = poem.Id,
                    Title = poem.Title,
                    AuthorName = author.PenName ?? viewModel.UserName,
                    ContentSnippet = poem.Content.Length > 200 ? poem.Content.Substring(0, 200) + "..." : poem.Content,
                    ViewsCount = poem.ViewsCount,
                    LikesCount = poem.LikesCount,
                    CommentsCount = poem.Comments.Count,
                    PublicationDate = poem.PublicationDate,
                })
                .OrderByDescending(p => p.PublicationDate)
                .ToListAsync();

            viewModel.AuthoredPoems = authoredPoems;

            // Блоги автора
            var authoredBlogs = await _context.Blogs
                .Where(b => b.AuthorId == authorId)
                .Select(blog => new MyProfileViewModel.BlogDisplayModel
                {
                    Id = blog.Id,
                    Title = blog.Title,
                    AuthorName = author.PenName ?? viewModel.UserName,
                    ImageUrl = blog.ImageUrl,
                    ContentSnippet = blog.Content.Length > 150 ?
                        System.Text.RegularExpressions.Regex.Replace(blog.Content.Substring(0, 150), "<.*?>", string.Empty) + "..."
                        : System.Text.RegularExpressions.Regex.Replace(blog.Content, "<.*?>", string.Empty),
                    ViewsCount = blog.ViewsCount,
                    LikesCount = blog.LikesCount,
                    CommentsCount = blog.Comments.Count,
                    PublicationDate = blog.PublicationDate
                })
                .OrderByDescending(b => b.PublicationDate)
                .ToListAsync();

            viewModel.AuthoredBlogs = authoredBlogs;

            // Подписчики автора
            var followersList = await _context.Followers
                .Where(f => f.AuthorId == authorId)
                .Include(f => f.FollowerApplicationUser)
                .Select(f => new MyProfileViewModel.FollowerDisplayModel
                {
                    UserId = f.FollowerApplicationUserId,
                    UserName = f.FollowerApplicationUser.UserName
                })
                .ToListAsync();

            viewModel.FollowersList = followersList;

            // Количество подписчиков
            viewModel.FollowerCount = await _context.Followers
                .Where(f => f.AuthorId == authorId)
                .CountAsync();

            // Для своего профиля автора заполняем понравившиеся и избранное
            if (viewModel.IsOwnProfile)
            {
                await PopulateLikedAndFavoriteContent(viewModel, author.ApplicationUserId);
            }

            // Проверка подписки текущего пользователя
            if (!string.IsNullOrEmpty(currentUserId) && currentUserId != viewModel.UserId)
            {
                viewModel.IsFollowing = await _authorService.IsFollowingAsync(currentUserId, authorId);
            }
        }

        private async Task PopulateUserProfile(MyProfileViewModel viewModel, string userId, string currentUserId)
        {
            // Прочитанные книги (только для собственного профиля)
            if (viewModel.IsOwnProfile)
            {
                viewModel.ReadBooks = await _context.BookReadings
                    .Where(br => br.ApplicationUserId == userId)
                    .Include(br => br.Book)
                        .ThenInclude(b => b.BookAuthors)
                            .ThenInclude(ba => ba.Author)
                    .Include(br => br.Book)
                        .ThenInclude(b => b.BookGenres)
                            .ThenInclude(bg => bg.Genre)
                    .Include(br => br.Book)
                        .ThenInclude(b => b.BookTags)
                            .ThenInclude(bt => bt.Tag)
                    .Select(br => new MyProfileViewModel.BookDisplayModel
                    {
                        Id = br.Book.Id,
                        Title = br.Book.Title,
                        AuthorName = br.Book.BookAuthors.Select(ba => ba.Author.PenName).FirstOrDefault() ?? "Неизвестен",
                        Authors = br.Book.BookAuthors.Select(ba => ba.Author.PenName).ToList(),
                        CoverImageUrl = br.Book.CoverImageUrl,
                        ReadsCount = br.Book.ReadsCount,
                        LikesCount = br.Book.LikesCount,
                        IsAdultContent = br.Book.IsAdultContent,
                        Rating = _ratingService.CalculateGlobalRating(br.Book.LikesCount, br.Book.ReadsCount),
                        Description = br.Book.Description ?? string.Empty,
                        Genres = br.Book.BookGenres.Select(bg => bg.Genre.Name).ToList(),
                        Tags = br.Book.BookTags.Select(bt => bt.Tag.Name).ToList(),
                        PublicationDate = br.Book.PublicationDate
                    })
                    .ToListAsync();

                // Книги в закладках/избранном (только для собственного профиля)
                viewModel.FavoriteBooks = await _context.UserFavoriteBooks
                    .Where(ufb => ufb.ApplicationUserId == userId)
                    .Include(ufb => ufb.Book)
                        .ThenInclude(b => b.BookAuthors)
                            .ThenInclude(ba => ba.Author)
                    .Include(ufb => ufb.Book)
                        .ThenInclude(b => b.BookGenres)
                            .ThenInclude(bg => bg.Genre)
                    .Include(ufb => ufb.Book)
                        .ThenInclude(b => b.BookTags)
                            .ThenInclude(bt => bt.Tag)
                    .Select(ufb => new MyProfileViewModel.BookDisplayModel
                    {
                        Id = ufb.Book.Id,
                        Title = ufb.Book.Title,
                        AuthorName = ufb.Book.BookAuthors.Select(ba => ba.Author.PenName).FirstOrDefault() ?? "Неизвестен",
                        Authors = ufb.Book.BookAuthors.Select(ba => ba.Author.PenName).ToList(),
                        CoverImageUrl = ufb.Book.CoverImageUrl,
                        ReadsCount = ufb.Book.ReadsCount,
                        LikesCount = ufb.Book.LikesCount,
                        IsAdultContent = ufb.Book.IsAdultContent,
                        Rating = _ratingService.CalculateGlobalRating(ufb.Book.LikesCount, ufb.Book.ReadsCount),
                        Description = ufb.Book.Description ?? string.Empty,
                        Genres = ufb.Book.BookGenres.Select(bg => bg.Genre.Name).ToList(),
                        Tags = ufb.Book.BookTags.Select(bt => bt.Tag.Name).ToList(),
                        PublicationDate = ufb.Book.PublicationDate
                    })
                    .ToListAsync();
            }

            // Книги, которые понравились (показываем всегда для своего профиля)
            viewModel.LikedBooks = await _context.BookLikes
                .Where(bl => bl.ApplicationUserId == userId)
                .Include(bl => bl.Book)
                    .ThenInclude(b => b.BookAuthors)
                        .ThenInclude(ba => ba.Author)
                .Include(bl => bl.Book)
                    .ThenInclude(b => b.BookGenres)
                        .ThenInclude(bg => bg.Genre)
                .Include(bl => bl.Book)
                    .ThenInclude(b => b.BookTags)
                        .ThenInclude(bt => bt.Tag)
                .Select(bl => new MyProfileViewModel.BookDisplayModel
                {
                    Id = bl.Book.Id,
                    Title = bl.Book.Title,
                    AuthorName = bl.Book.BookAuthors.Select(ba => ba.Author.PenName).FirstOrDefault() ?? "Неизвестен",
                    Authors = bl.Book.BookAuthors.Select(ba => ba.Author.PenName).ToList(),
                    CoverImageUrl = bl.Book.CoverImageUrl,
                    ReadsCount = bl.Book.ReadsCount,
                    LikesCount = bl.Book.LikesCount,
                    IsAdultContent = bl.Book.IsAdultContent,
                    Rating = _ratingService.CalculateGlobalRating(bl.Book.LikesCount, bl.Book.ReadsCount),
                    Description = bl.Book.Description ?? string.Empty,
                    Genres = bl.Book.BookGenres.Select(bg => bg.Genre.Name).ToList(),
                    Tags = bl.Book.BookTags.Select(bt => bt.Tag.Name).ToList(),
                    PublicationDate = bl.Book.PublicationDate
                })
                .ToListAsync();

            // Остальной код для стихов и блогов остается без изменений...
            // Стихи, которые понравились
            viewModel.LikedPoems = await _context.PoemLikes
                .Where(pl => pl.ApplicationUserId == userId)
                .Include(pl => pl.Poem)
                    .ThenInclude(p => p.Author)
                .Select(pl => new MyProfileViewModel.PoemDisplayModel
                {
                    Id = pl.Poem.Id,
                    Title = pl.Poem.Title,
                    AuthorName = pl.Poem.Author.PenName ?? "Неизвестен",
                    ContentSnippet = pl.Poem.Content.Length > 200 ? pl.Poem.Content.Substring(0, 200) + "..." : pl.Poem.Content,
                    ViewsCount = pl.Poem.ViewsCount,
                    LikesCount = pl.Poem.LikesCount,
                    CommentsCount = pl.Poem.Comments.Count,
                    PublicationDate = pl.Poem.PublicationDate
                })
                .ToListAsync();

            // Блоги, которые понравились
            viewModel.LikedBlogs = await _context.BlogLikes
                .Where(bl => bl.ApplicationUserId == userId)
                .Include(bl => bl.Blog)
                    .ThenInclude(b => b.Author)
                .Select(bl => new MyProfileViewModel.BlogDisplayModel
                {
                    Id = bl.Blog.Id,
                    Title = bl.Blog.Title,
                    AuthorName = bl.Blog.Author.PenName ?? "Неизвестен",
                    ImageUrl = bl.Blog.ImageUrl,
                    ContentSnippet = bl.Blog.Content.Length > 150 ? bl.Blog.Content.Substring(0, 150) + "..." : bl.Blog.Content,
                    ViewsCount = bl.Blog.ViewsCount,
                    LikesCount = bl.Blog.LikesCount,
                    CommentsCount = bl.Blog.Comments.Count,
                    PublicationDate = bl.Blog.PublicationDate
                })
                .ToListAsync();
        }

        private async Task PopulateLikedAndFavoriteContent(MyProfileViewModel viewModel, string userId)
        {
            // Заполняем понравившиеся книги для автора
            viewModel.LikedBooks = await _context.BookLikes
                .Where(bl => bl.ApplicationUserId == userId)
                .Include(bl => bl.Book)
                    .ThenInclude(b => b.BookAuthors)
                        .ThenInclude(ba => ba.Author)
                .Include(bl => bl.Book)
                    .ThenInclude(b => b.BookGenres)
                        .ThenInclude(bg => bg.Genre)
                .Include(bl => bl.Book)
                    .ThenInclude(b => b.BookTags)
                        .ThenInclude(bt => bt.Tag)
                .Select(bl => new MyProfileViewModel.BookDisplayModel
                {
                    Id = bl.Book.Id,
                    Title = bl.Book.Title,
                    AuthorName = bl.Book.BookAuthors.Select(ba => ba.Author.PenName).FirstOrDefault() ?? "Неизвестен",
                    Authors = bl.Book.BookAuthors.Select(ba => ba.Author.PenName).ToList(),
                    CoverImageUrl = bl.Book.CoverImageUrl,
                    ReadsCount = bl.Book.ReadsCount,
                    LikesCount = bl.Book.LikesCount,
                    IsAdultContent = bl.Book.IsAdultContent,
                    Rating = _ratingService.CalculateGlobalRating(bl.Book.LikesCount, bl.Book.ReadsCount),
                    Description = bl.Book.Description ?? string.Empty,
                    Genres = bl.Book.BookGenres.Select(bg => bg.Genre.Name).ToList(),
                    Tags = bl.Book.BookTags.Select(bt => bt.Tag.Name).ToList(),
                    PublicationDate = bl.Book.PublicationDate
                })
                .ToListAsync();

            // Заполняем понравившиеся стихи для автора
            viewModel.LikedPoems = await _context.PoemLikes
                .Where(pl => pl.ApplicationUserId == userId)
                .Include(pl => pl.Poem)
                    .ThenInclude(p => p.Author)
                .Select(pl => new MyProfileViewModel.PoemDisplayModel
                {
                    Id = pl.Poem.Id,
                    Title = pl.Poem.Title,
                    AuthorName = pl.Poem.Author.PenName ?? "Неизвестен",
                    ContentSnippet = pl.Poem.Content.Length > 200 ? pl.Poem.Content.Substring(0, 200) + "..." : pl.Poem.Content,
                    ViewsCount = pl.Poem.ViewsCount,
                    LikesCount = pl.Poem.LikesCount,
                    CommentsCount = pl.Poem.Comments.Count,
                    PublicationDate = pl.Poem.PublicationDate
                })
                .ToListAsync();

            // Заполняем понравившиеся блоги для автора
            viewModel.LikedBlogs = await _context.BlogLikes
                .Where(bl => bl.ApplicationUserId == userId)
                .Include(bl => bl.Blog)
                    .ThenInclude(b => b.Author)
                .Select(bl => new MyProfileViewModel.BlogDisplayModel
                {
                    Id = bl.Blog.Id,
                    Title = bl.Blog.Title,
                    AuthorName = bl.Blog.Author.PenName ?? "Неизвестен",
                    ImageUrl = bl.Blog.ImageUrl,
                    ContentSnippet = bl.Blog.Content.Length > 150 ? bl.Blog.Content.Substring(0, 150) + "..." : bl.Blog.Content,
                    ViewsCount = bl.Blog.ViewsCount,
                    LikesCount = bl.Blog.LikesCount,
                    CommentsCount = bl.Blog.Comments.Count,
                    PublicationDate = bl.Blog.PublicationDate
                })
                .ToListAsync();

            // Заполняем избранные книги для автора
            viewModel.FavoriteBooks = await _context.UserFavoriteBooks
                .Where(ufb => ufb.ApplicationUserId == userId)
                .Include(ufb => ufb.Book)
                    .ThenInclude(b => b.BookAuthors)
                        .ThenInclude(ba => ba.Author)
                .Include(ufb => ufb.Book)
                    .ThenInclude(b => b.BookGenres)
                        .ThenInclude(bg => bg.Genre)
                .Include(ufb => ufb.Book)
                    .ThenInclude(b => b.BookTags)
                        .ThenInclude(bt => bt.Tag)
                .Select(ufb => new MyProfileViewModel.BookDisplayModel
                {
                    Id = ufb.Book.Id,
                    Title = ufb.Book.Title,
                    AuthorName = ufb.Book.BookAuthors.Select(ba => ba.Author.PenName).FirstOrDefault() ?? "Неизвестен",
                    Authors = ufb.Book.BookAuthors.Select(ba => ba.Author.PenName).ToList(),
                    CoverImageUrl = ufb.Book.CoverImageUrl,
                    ReadsCount = ufb.Book.ReadsCount,
                    LikesCount = ufb.Book.LikesCount,
                    IsAdultContent = ufb.Book.IsAdultContent,
                    Rating = _ratingService.CalculateGlobalRating(ufb.Book.LikesCount, ufb.Book.ReadsCount),
                    Description = ufb.Book.Description ?? string.Empty,
                    Genres = ufb.Book.BookGenres.Select(bg => bg.Genre.Name).ToList(),
                    Tags = ufb.Book.BookTags.Select(bt => bt.Tag.Name).ToList(),
                    PublicationDate = ufb.Book.PublicationDate
                })
                .ToListAsync();
        }

        private async Task PopulateSubscriptions(MyProfileViewModel viewModel, string userId)
        {
            viewModel.Subscriptions = await _context.Followers
                .Where(f => f.FollowerApplicationUserId == userId)
                .Include(f => f.Author)
                    .ThenInclude(a => a.ApplicationUser)
                .Select(f => new MyProfileViewModel.AuthorSubscriptionDisplayModel
                {
                    AuthorId = f.Author.Id,
                    AuthorName = f.Author.PenName ?? f.Author.ApplicationUser.UserName,
                    PhotoUrl = f.Author.ApplicationUser.ProfilePictureUrl,
                    FollowerCount = _context.Followers.Count(f2 => f2.AuthorId == f.Author.Id)
                })
                .ToListAsync();
        }

        public async Task<string> GetUsserIdByAuthorIdAsync(int id)
        {
            var author = await _context.Authors
                .AsNoTracking()
                .Include(a => a.ApplicationUser)
                .FirstOrDefaultAsync(a => a.Id == id);

            return author?.ApplicationUserId;
        }

        public async Task<bool> IsCurrentUserAuthorAsync(string currentUserId)
        {
            if (string.IsNullOrEmpty(currentUserId))
                return false;

            var user = await _context.Users
                .Include(u => u.AuthorProfile)
                .FirstOrDefaultAsync(u => u.Id == currentUserId);

            return user?.AuthorProfile != null;
        }
    }
}