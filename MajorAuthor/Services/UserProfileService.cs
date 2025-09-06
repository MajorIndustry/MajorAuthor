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

        public UserProfileService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<MyProfileViewModel> GetUserProfileViewModelAsync(string userId)
        {
            // Используем одну сложную LINQ-запрос для извлечения всех необходимых данных за один раз
            var user = await _userManager.Users
                .AsNoTracking() // Отключаем отслеживание для ускорения запроса, так как мы только читаем
                .Include(u => u.AuthorProfile)
                    .ThenInclude(ap => ap.BookAuthors)
                        .ThenInclude(ba => ba.Book)
                .Include(u => u.AuthorProfile)
                    .ThenInclude(ap => ap.Blogs)
                .Include(u => u.AuthorProfile)
                    .ThenInclude(ap => ap.Followers)
                        .ThenInclude(f => f.FollowerApplicationUser)
                .Include(u => u.Readings)
                    .ThenInclude(br => br.Book)
                        .ThenInclude(b => b.BookAuthors)
                            .ThenInclude(ba => ba.Author)
                .Include(u => u.FavoriteBooks)
                    .ThenInclude(ufb => ufb.Book)
                        .ThenInclude(b => b.BookAuthors)
                            .ThenInclude(ba => ba.Author)
                .Include(u => u.BookLikes)
                    .ThenInclude(bl => bl.Book)
                        .ThenInclude(b => b.BookAuthors)
                            .ThenInclude(ba => ba.Author)
                .Include(u => u.BlogLikes)
                    .ThenInclude(bl => bl.Blog)
                        .ThenInclude(blog => blog.Author)
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
                PhotoUrl = string.IsNullOrEmpty(user.ProfilePictureUrl) ? "/images/default_avatar.png" : user.ProfilePictureUrl
            };

            if (viewModel.IsAuthor)
            {
                PopulateAuthorProfile(viewModel, user.AuthorProfile);
            }
            else
            {
                PopulateUserProfile(viewModel, user);
            }

            return viewModel;
        }

        private void PopulateAuthorProfile(MyProfileViewModel viewModel, Author authorProfile)
        {
            viewModel.DisplayName = string.IsNullOrEmpty(authorProfile.PenName) ? viewModel.UserName : authorProfile.PenName;
            viewModel.FullName = authorProfile.FullName;
            viewModel.PhotoUrl = string.IsNullOrEmpty(authorProfile.PhotoUrl) ? viewModel.PhotoUrl : authorProfile.PhotoUrl;

            // Написанные книги автора
            viewModel.AuthoredBooks = authorProfile.BookAuthors
                .Select(ba => new MyProfileViewModel.BookDisplayModel
                {
                    Id = ba.Book.Id,
                    Title = ba.Book.Title,
                    AuthorName = authorProfile.PenName,
                    CoverImageUrl = ba.Book.CoverImageUrl,
                    ReadsCount = ba.Book.ReadsCount,
                    LikesCount = ba.Book.LikesCount,
                    IsAdultContent = ba.Book.IsAdultContent
                })
                .ToList();

            // Написанные блоги автора
            viewModel.AuthoredBlogs = authorProfile.Blogs
                .Select(blog => new MyProfileViewModel.BlogDisplayModel
                {
                    Id = blog.Id,
                    Title = blog.Title,
                    AuthorName = authorProfile.PenName,
                    ImageUrl = blog.ImageUrl,
                    ContentSnippet = blog.Content.Length > 150 ? blog.Content.Substring(0, 150) + "..." : blog.Content,
                    ViewsCount = blog.ViewsCount,
                    LikesCount = blog.LikesCount,
                    CommentsCount = blog.CommentsCount,
                    PublicationDate = blog.PublicationDate
                })
                .OrderByDescending(b => b.PublicationDate)
                .ToList();

            // Количество подписчиков и список
            viewModel.FollowerCount = authorProfile.Followers?.Count ?? 0;
            viewModel.FollowersList = authorProfile.Followers
                .Select(f => new MyProfileViewModel.FollowerDisplayModel
                {
                    UserId = f.FollowerApplicationUserId,
                    UserName = f.FollowerApplicationUser.UserName
                })
                .ToList();
        }

        private void PopulateUserProfile(MyProfileViewModel viewModel, ApplicationUser user)
        {
            // Прочитанные книги
            viewModel.ReadBooks = user.Readings
                .Select(br => new MyProfileViewModel.BookDisplayModel
                {
                    Id = br.Book.Id,
                    Title = br.Book.Title,
                    AuthorName = br.Book.BookAuthors.Select(ba => ba.Author?.PenName).FirstOrDefault() ?? "Неизвестен",
                    CoverImageUrl = br.Book.CoverImageUrl,
                    ReadsCount = br.Book.ReadsCount,
                    LikesCount = br.Book.LikesCount,
                    IsAdultContent = br.Book.IsAdultContent
                })
                .ToList();

            // Книги в закладках/избранном
            viewModel.FavoriteBooks = user.FavoriteBooks
                .Select(ufb => new MyProfileViewModel.BookDisplayModel
                {
                    Id = ufb.Book.Id,
                    Title = ufb.Book.Title,
                    AuthorName = ufb.Book.BookAuthors.Select(ba => ba.Author?.PenName).FirstOrDefault() ?? "Неизвестен",
                    CoverImageUrl = ufb.Book.CoverImageUrl,
                    ReadsCount = ufb.Book.ReadsCount,
                    LikesCount = ufb.Book.LikesCount,
                    IsAdultContent = ufb.Book.IsAdultContent
                })
                .ToList();

            // Книги, которые понравились
            viewModel.LikedBooks = user.BookLikes
                .Select(bl => new MyProfileViewModel.BookDisplayModel
                {
                    Id = bl.Book.Id,
                    Title = bl.Book.Title,
                    AuthorName = bl.Book.BookAuthors.Select(ba => ba.Author?.PenName).FirstOrDefault() ?? "Неизвестен",
                    CoverImageUrl = bl.Book.CoverImageUrl,
                    ReadsCount = bl.Book.ReadsCount,
                    LikesCount = bl.Book.LikesCount,
                    IsAdultContent = bl.Book.IsAdultContent
                })
                .ToList();

            // Блоги, которые понравились
            viewModel.LikedBlogs = user.BlogLikes
                .Select(bl => new MyProfileViewModel.BlogDisplayModel
                {
                    Id = bl.Blog.Id,
                    Title = bl.Blog.Title,
                    AuthorName = bl.Blog.Author?.PenName,
                    ImageUrl = bl.Blog.ImageUrl,
                    ContentSnippet = bl.Blog.Content.Length > 150 ? bl.Blog.Content.Substring(0, 150) + "..." : bl.Blog.Content,
                    ViewsCount = bl.Blog.ViewsCount,
                    LikesCount = bl.Blog.LikesCount,
                    CommentsCount = bl.Blog.CommentsCount,
                    PublicationDate = bl.Blog.PublicationDate
                })
                .ToList();
        }
    }
}
