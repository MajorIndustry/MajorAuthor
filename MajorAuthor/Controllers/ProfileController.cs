using MajorAuthor.Models;
using MajorAuthor.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MajorAuthor.Controllers
{
    // Ограничиваем доступ только для авторизованных пользователей
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IUserProfileService _userProfileService;

        // Теперь контроллер зависит от интерфейса, что упрощает тестирование
        public ProfileController(IUserProfileService userProfileService)
        {
            _userProfileService = userProfileService;
        }

        /// <summary>
        /// Отображает страницу профиля текущего пользователя.
        /// </summary>
        public async Task<IActionResult> MyProfile()
        {
            // Контроллер отвечает только за получение ID и вызов сервиса
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                // Пользователь не найден или не авторизован
                return RedirectToAction("Login", "Account");
            }

            // Вся сложная логика инкапсулирована в сервисе
            var viewModel = await _userProfileService.GetUserProfileViewModelAsync(userId);

            if (viewModel == null)
            {
                return NotFound($"Не удалось загрузить данные профиля для ID '{userId}'.");
            }

            return View(viewModel);
        }
    }
}