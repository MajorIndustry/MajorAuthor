// ����: Controllers/HomeController.cs
// ������: MajorAuthor
using MajorAuthor.Models;
using MajorAuthor.Services; // ���������� ��� ����� ������
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MajorAuthor.Controllers
{
    public class HomeController : Controller
    {
        // ���������� ������ ������� �� ���������� (����������), � �� �� ���������� ���������� MajorAuthorDbContext
        private readonly IHomeService _homeService;

        /// <summary>
        /// ����������� �����������, ������������ ��������� ������������ ��� IHomeService.
        /// </summary>
        /// <param name="homeService">��������� IHomeService.</param>
        public HomeController(IHomeService homeService)
        {
            _homeService = homeService;
        }

        /// <summary>
        /// ���������� ������� �������� � ���������� �������� ���� � �������.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            // �������� ID ������������, ���� �� �����������
            var currentUserId = User.Identity.IsAuthenticated ? User.FindFirstValue(ClaimTypes.NameIdentifier) : null;

            // ��� ������� ������ �������� �������� � ������
            var viewModel = await _homeService.GetHomeDataAsync(currentUserId);

            return View(viewModel);
        }

        /// <summary>
        /// Action ��� ��������� ���� �� ���������� ����� (������������ AJAX).
        /// </summary>
        /// <param name="genreId">ID ���������� �����.</param>
        [HttpGet]
        public async Task<IActionResult> GetBooksByGenre(int genreId)
        {
            var books = await _homeService.GetBooksByGenreAsync(genreId);
            return PartialView("_BookListPartial", books);
        }

        // Action ��� ��������� ������
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
