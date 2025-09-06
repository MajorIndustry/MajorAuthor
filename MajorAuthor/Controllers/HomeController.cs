// Файл: Controllers/HomeController.cs
// Проект: MajorAuthor
using MajorAuthor.Models;
using MajorAuthor.Services; // Используем наш новый сервис
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MajorAuthor.Controllers
{
    public class HomeController : Controller
    {
        // Контроллер теперь зависит от абстракции (интерфейса), а не от конкретной реализации MajorAuthorDbContext
        private readonly IHomeService _homeService;

        /// <summary>
        /// Конструктор контроллера, использующий внедрение зависимостей для IHomeService.
        /// </summary>
        /// <param name="homeService">Экземпляр IHomeService.</param>
        public HomeController(IHomeService homeService)
        {
            _homeService = homeService;
        }

        /// <summary>
        /// Отображает главную страницу с различными секциями книг и авторов.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            // Получаем ID пользователя, если он авторизован
            var currentUserId = User.Identity.IsAuthenticated ? User.FindFirstValue(ClaimTypes.NameIdentifier) : null;

            // Вся сложная логика запросов вынесена в сервис
            var viewModel = await _homeService.GetHomeDataAsync(currentUserId);

            return View(viewModel);
        }

        /// <summary>
        /// Action для получения книг по выбранному жанру (используется AJAX).
        /// </summary>
        /// <param name="genreId">ID выбранного жанра.</param>
        [HttpGet]
        public async Task<IActionResult> GetBooksByGenre(int genreId)
        {
            var books = await _homeService.GetBooksByGenreAsync(genreId);
            return PartialView("_BookListPartial", books);
        }

        // Action для обработки ошибок
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
