// Проект: MajorAuthor
// Файл: Controllers/ProfileController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using MajorAuthor.Data;
using MajorAuthor.Data.Entities;
using MajorAuthor.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MajorAuthor.Controllers
{
    // Ограничиваем доступ только для авторизованных пользователей
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly MajorAuthorDbContext _context;

        public ProfileController(UserManager<ApplicationUser> userManager, MajorAuthorDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        /// <summary>
        /// Отображает страницу профиля текущего пользователя.
        /// </summary>
        public async Task<IActionResult> MyProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                // Пользователь не найден или не авторизован, хотя контроллер [Authorize]
                // Это должно быть перехвачено Middleware, но для безопасности можно добавить
                return RedirectToAction("Login", "Account"); // Перенаправить на страницу входа
            }

            var user = await _userManager.Users
                .Include(u => u.AuthorProfile) // Загружаем профиль автора, если есть
                    .ThenInclude(ap => ap.BookAuthors)
                        .ThenInclude(ba => ba.Book)
                .Include(u => u.AuthorProfile) // Повторно, чтобы получить blogs
                    .ThenInclude(ap => ap.Blogs)
                .Include(u => u.AuthorProfile) // Повторно, чтобы получить Followers
                    .ThenInclude(ap => ap.Followers)
                        .ThenInclude(f => f.FollowerApplicationUser) // Загружаем данные подписчиков
                .Include(u => u.Readings) // Прочитанные книги
                    .ThenInclude(br => br.Book)
                        .ThenInclude(b => b.BookAuthors)
                            .ThenInclude(ba => ba.Author)
                .Include(u => u.FavoriteBooks) // Книги в закладках
                    .ThenInclude(ufb => ufb.Book)
                        .ThenInclude(b => b.BookAuthors)
                            .ThenInclude(ba => ba.Author)
                .Include(u => u.BookLikes) // Понравившиеся книги
                    .ThenInclude(bl => bl.Book)
                        .ThenInclude(b => b.BookAuthors)
                            .ThenInclude(ba => ba.Author)
                .Include(u => u.BlogLikes) // Понравившиеся блоги
                    .ThenInclude(bl => bl.Blog)
                        .ThenInclude(blog => blog.Author) // Для получения имени автора блога
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound($"Не удалось загрузить пользователя с ID '{_userManager.GetUserId(User)}'.");
            }

            var viewModel = new MyProfileViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                IsAuthor = user.AuthorProfile != null,
                DisplayName = user.UserName, // По умолчанию, если не автор
                // Используем ProfilePictureUrl из ApplicationUser, если он есть, иначе дефолтный аватар
                PhotoUrl = string.IsNullOrEmpty(user.ProfilePictureUrl) ? "/images/default_avatar.png" : user.ProfilePictureUrl
            };

            // Если пользователь является автором
            if (viewModel.IsAuthor)
            {
                viewModel.DisplayName = string.IsNullOrEmpty(user.AuthorProfile.PenName) ? user.UserName : user.AuthorProfile.PenName;
                viewModel.FullName = user.AuthorProfile.FullName; // Предполагается, что в Author есть FullName
                // PhotoUrl автора переопределяет PhotoUrl пользователя, если у автора есть свой
                viewModel.PhotoUrl = string.IsNullOrEmpty(user.AuthorProfile.PhotoUrl) ? viewModel.PhotoUrl : user.AuthorProfile.PhotoUrl;
                // viewModel.Description = user.AuthorProfile.Description; // УДАЛЕНО: Свойство Description отсутствует в вашем классе Author

                // Написанные книги автора
                viewModel.AuthoredBooks = user.AuthorProfile.BookAuthors
                    .Select(ba => new MyProfileViewModel.BookDisplayModel
                    {
                        Id = ba.Book.Id,
                        Title = ba.Book.Title,
                        AuthorName = user.AuthorProfile.PenName, // Автор, т.к. это его профиль
                        CoverImageUrl = ba.Book.CoverImageUrl,
                        ReadsCount = ba.Book.ReadsCount,
                        LikesCount = ba.Book.LikesCount,
                        IsAdultContent = ba.Book.IsAdultContent
                    })
                    .ToList();

                // Написанные блоги автора
                viewModel.AuthoredBlogs = user.AuthorProfile.Blogs
                    .Select(blog => new MyProfileViewModel.BlogDisplayModel
                    {
                        Id = blog.Id,
                        Title = blog.Title,
                        AuthorName = user.AuthorProfile.PenName,
                        ImageUrl = blog.ImageUrl,
                        ContentSnippet = blog.Content.Length > 150 ? blog.Content.Substring(0, 150) + "..." : blog.Content,
                        ViewsCount = blog.ViewsCount,
                        LikesCount = blog.LikesCount,
                        CommentsCount = blog.CommentsCount,
                        PublicationDate = blog.PublicationDate
                    })
                    .OrderByDescending(b => b.PublicationDate)
                    .ToList();

                // Количество подписчиков
                viewModel.FollowerCount = user.AuthorProfile.Followers?.Count ?? 0;
                // Список подписчиков
                viewModel.FollowersList = user.AuthorProfile.Followers
                    .Select(f => new MyProfileViewModel.FollowerDisplayModel
                    {
                        UserId = f.FollowerApplicationUserId,
                        UserName = f.FollowerApplicationUser.UserName
                    })
                    .ToList();
            }
            else // Обычный пользователь
            {
                // Прочитанные книги
                viewModel.ReadBooks = user.Readings
                    .Select(br => new MyProfileViewModel.BookDisplayModel
                    {
                        Id = br.Book.Id,
                        Title = br.Book.Title,
                        AuthorName = br.Book.BookAuthors.Select(ba => ba.Author.PenName).FirstOrDefault() ?? "Неизвестен",
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
                        AuthorName = ufb.Book.BookAuthors.Select(ba => ba.Author.PenName).FirstOrDefault() ?? "Неизвестен",
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
                        Title = bl.Book.Title, // Исправлено: bl.Blog.Title вместо bl.Book.Title (ошибка копипасты)
                        AuthorName = bl.Book.BookAuthors.Select(ba => ba.Author.PenName).FirstOrDefault() ?? "Неизвестен",
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
                        AuthorName = bl.Blog.Author.PenName,
                        ImageUrl = bl.Blog.ImageUrl,
                        ContentSnippet = bl.Blog.Content.Length > 150 ? bl.Blog.Content.Substring(0, 150) + "..." : bl.Blog.Content,
                        ViewsCount = bl.Blog.ViewsCount,
                        LikesCount = bl.Blog.LikesCount,
                        CommentsCount = bl.Blog.CommentsCount,
                        PublicationDate = bl.Blog.PublicationDate
                    })
                    .ToList();
            }

            return View(viewModel);
        }
    }
}
