using MajorAuthor.Models;
using MajorAuthor.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MajorAuthor.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IUserProfileService _userProfileService;
        private readonly IAuthorService _authorService;

        public ProfileController(IUserProfileService userProfileService, IAuthorService authorService)
        {
            _userProfileService = userProfileService;
            _authorService = authorService;
        }

        public async Task<IActionResult> MyProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var viewModel = await _userProfileService.GetUserProfileViewModelAsync(userId, userId);
            if (viewModel == null)
            {
                return NotFound($"Не удалось загрузить данные профиля для ID '{userId}'.");
            }

            viewModel.IsOwnProfile = true;
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> UserProfile(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userId = await _userProfileService.GetUsserIdByAuthorIdAsync(id);

            if (string.IsNullOrEmpty(userId))
            {
                return NotFound($"Автор с ID '{id}' не найден.");
            }

            var viewModel = await _userProfileService.GetUserProfileViewModelAsync(userId, currentUserId);
            if (viewModel == null)
            {
                return NotFound($"Не удалось загрузить данные профиля для ID '{userId}'.");
            }

            viewModel.IsOwnProfile = currentUserId == userId;

            // Логирование для отладки
            Console.WriteLine($"Books: {viewModel.AuthoredBooks?.Count ?? 0}");
            Console.WriteLine($"Poems: {viewModel.AuthoredPoems?.Count ?? 0}");
            Console.WriteLine($"Blogs: {viewModel.AuthoredBlogs?.Count ?? 0}");

            return View("MyProfile", viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> ReaderProfile(string id)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(id))
            {
                return NotFound("ID пользователя не указан.");
            }

            var viewModel = await _userProfileService.GetUserProfileViewModelAsync(id, currentUserId);
            if (viewModel == null)
            {
                return NotFound($"Не удалось загрузить данные профиля для ID '{id}'.");
            }

            viewModel.IsOwnProfile = currentUserId == id;
            return View("MyProfile", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> FollowAuthor([FromBody] FollowRequest request)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId == null)
            {
                return Json(new { success = false, message = "Пользователь не авторизован" });
            }

            var result = await _authorService.FollowAuthorAsync(currentUserId, request.AuthorId);

            if (result.success)
            {
                try
                {
                    var followerCount = await _authorService.GetFollowerCountAsync(request.AuthorId);
                    var followerInfo = await _authorService.GetFollowerInfoAsync(currentUserId);
                    // Получаем обновленный список подписчиков
                    var followers = await _authorService.GetAuthorFollowersAsync(request.AuthorId);

                    return Json(new
                    {
                        success = true,
                        followerCount,
                        isFollowing = true,
                        follower = new
                        {
                            userId = followerInfo.userId,
                            userName = followerInfo.userName
                        },
                        followers = followers // Добавляем список подписчиков
                    });
                }
                catch (Exception ex)
                {
                    return Json(new
                    {
                        success = true,
                        followerCount = 0,
                        isFollowing = true,
                        follower = new
                        {
                            userId = currentUserId,
                            userName = User.Identity?.Name ?? "Пользователь"
                        },
                        followers = new List<object>()
                    });
                }
            }

            return Json(new
            {
                success = false,
                message = result.message ?? "Ошибка при подписке"
            });
        }

        [HttpPost]
        public async Task<IActionResult> UnfollowAuthor([FromBody] FollowRequest request)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId == null)
            {
                return Json(new { success = false, message = "Пользователь не авторизован" });
            }

            var result = await _authorService.UnfollowAuthorAsync(currentUserId, request.AuthorId);

            if (result.success)
            {
                try
                {
                    var followerCount = await _authorService.GetFollowerCountAsync(request.AuthorId);
                    var followerInfo = await _authorService.GetFollowerInfoAsync(currentUserId);
                    // Получаем обновленный список подписчиков
                    var followers = await _authorService.GetAuthorFollowersAsync(request.AuthorId);

                    return Json(new
                    {
                        success = true,
                        followerCount,
                        isFollowing = false,
                        follower = new
                        {
                            userId = followerInfo.userId,
                            userName = followerInfo.userName
                        },
                        followers = followers // Добавляем список подписчиков
                    });
                }
                catch (Exception)
                {
                    return Json(new
                    {
                        success = true,
                        followerCount = 0,
                        isFollowing = false,
                        followers = new List<object>()
                    });
                }
            }

            return Json(new
            {
                success = false,
                message = result.message ?? "Ошибка при отписке"
            });
        }

        public class FollowRequest
        {
            public int AuthorId { get; set; }
        }
        [HttpGet]
        public async Task<IActionResult> SearchOnProfile(string query, string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest("Не удалось определить пользователя для поиска.");
                }
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var viewModel = await _userProfileService.GetUserProfileViewModelAsync(userId, currentUserId);

            if (viewModel == null)
            {
                return PartialView("_ProfileContentPartial", new MyProfileViewModel());
            }

            // Устанавливаем флаг поиска
            viewModel.IsSearch = true;

            // Если запрос не пустой, фильтруем списки
            if (!string.IsNullOrWhiteSpace(query))
            {
                var lowerQuery = query.ToLower();

                // Фильтруем все списки, которые есть в ViewModel
                if (viewModel.AuthoredBooks != null)
                    viewModel.AuthoredBooks = viewModel.AuthoredBooks
                        .Where(b => b.Title.ToLower().Contains(lowerQuery))
                        .ToList();

                if (viewModel.AuthoredPoems != null)
                    viewModel.AuthoredPoems = viewModel.AuthoredPoems
                        .Where(p => p.Title.ToLower().Contains(lowerQuery))
                        .ToList();

                if (viewModel.AuthoredBlogs != null)
                    viewModel.AuthoredBlogs = viewModel.AuthoredBlogs
                        .Where(b => b.Title.ToLower().Contains(lowerQuery))
                        .ToList();

                if (viewModel.FollowersList != null)
                    viewModel.FollowersList = viewModel.FollowersList
                        .Where(f => f.UserName.ToLower().Contains(lowerQuery))
                        .ToList();

                if (viewModel.Subscriptions != null)
                    viewModel.Subscriptions = viewModel.Subscriptions
                        .Where(s => s.AuthorName.ToLower().Contains(lowerQuery))
                        .ToList();

                if (viewModel.LikedBooks != null)
                    viewModel.LikedBooks = viewModel.LikedBooks
                        .Where(b => b.Title.ToLower().Contains(lowerQuery))
                        .ToList();

                if (viewModel.LikedPoems != null)
                    viewModel.LikedPoems = viewModel.LikedPoems
                        .Where(p => p.Title.ToLower().Contains(lowerQuery))
                        .ToList();

                if (viewModel.LikedBlogs != null)
                    viewModel.LikedBlogs = viewModel.LikedBlogs
                        .Where(b => b.Title.ToLower().Contains(lowerQuery))
                        .ToList();

                if (viewModel.FavoriteBooks != null)
                    viewModel.FavoriteBooks = viewModel.FavoriteBooks
                        .Where(b => b.Title.ToLower().Contains(lowerQuery))
                        .ToList();
            }

            return PartialView("_ProfileContentPartial", viewModel);
        }

    }
}