// ������: MajorAuthor.Web
// ����: Controllers/HomeController.cs
using Microsoft.AspNetCore.Mvc;
using MajorAuthor.Web.Models; // ���������� ���� ViewModel
using MajorAuthor.Data; // ���������� ��� DbContext
using Microsoft.EntityFrameworkCore; // ��� ������� ���������� EF Core, ����� ��� Include, ToListAsync
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims; // ��������� ��� ClaimTypes
using System.Diagnostics;
using MajorAuthor.Models; // ��� Activity (��� ErrorViewModel)


namespace MajorAuthor.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly MajorAuthorDbContext _context;

        /// <summary>
        /// ����������� �����������, ������������ ��������� ������������ ��� DbContext.
        /// </summary>
        /// <param name="context">��������� MajorAuthorDbContext.</param>
        public HomeController(MajorAuthorDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// ���������� ������� �������� � ���������� �������� ���� � �������.
        /// ������ ����������� �� ���� ������.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var viewModel = new HomeViewModel
            {
                IsUserLoggedIn = User.Identity.IsAuthenticated // ���������, ����������� �� ������������
            };

            // --- ���������� ������ �� ���� ������ ---

            // ��������� ���� ��������� ������ ��� ����������
            viewModel.AvailableGenres = await _context.Genres
                .Select(g => new HomeViewModel.GenreDisplayModel { Id = g.Id, Name = g.Name })
                .OrderBy(g => g.Name)
                .ToListAsync();

            // ���������� ����� (��������, ���-10 �� ���������� ������ ��� ���������)
            viewModel.PopularBooks = await _context.Books
                .OrderByDescending(b => b.LikesCount + b.ReadsCount / 10.0) // ������ ���������������� ��������
                .Take(10)
                .Select(b => new HomeViewModel.BookDisplayModel
                {
                    Id = b.Id,
                    Title = b.Title,
                    // ��� ������: ���� ����� ����� ����� ��������� �������,
                    // ����� ���������� �� ����� ��� ������� �������/���������.
                    // ����� ��������������, ��� �� ������ ���������� PenName ��������� ������ ��� ������ �������.
                    // ��� ���������, ���� ����� PenName ������� ������, ���� �� ����.
                    AuthorName = b.BookAuthors.Select(ba => ba.Author.PenName).FirstOrDefault() ?? "����������",
                    CoverImageUrl = b.CoverImageUrl,
                    ReadsCount = b.ReadsCount,
                    LikesCount = b.LikesCount,
                    IsAdultContent = b.IsAdultContent
                })
                .ToListAsync();

            // ���������� ������ (��������, ���-10 �� ������ ���������� ��������� �� ����)
            viewModel.PopularAuthors = await _context.Authors
                .OrderByDescending(a => a.BookAuthors.Sum(ba => ba.Book.ReadsCount)) // ��������� ��������� ���� ���� ������
                .Take(10)
                .Select(a => new HomeViewModel.AuthorDisplayModel
                {
                    Id = a.Id,
                    Name = a.PenName,
                    PhotoUrl = a.PhotoUrl,
                    BooksCount = a.BookAuthors.Count(),
                    TotalReadsCount = a.BookAuthors.Sum(ba => ba.Book.ReadsCount)
                })
                .ToListAsync();

            // ������� ����������� ����� (�� ��������� 7 ����, ������������� �� LastUpdateTime)
            viewModel.RecentlyUpdatedBooks = await _context.Books
                .Where(b => b.LastUpdateTime >= DateTime.UtcNow.AddDays(-7))
                .OrderByDescending(b => b.LastUpdateTime)
                .Take(10)
                .Select(b => new HomeViewModel.BookDisplayModel
                {
                    Id = b.Id,
                    Title = b.Title,
                    AuthorName = b.BookAuthors.Select(ba => ba.Author.PenName).FirstOrDefault() ?? "����������",
                    CoverImageUrl = b.CoverImageUrl,
                    UpdateInfo = GetRelativeTime(b.LastUpdateTime) // ���������� ����������� �����
                })
                .ToListAsync();

            // ����� ���������� ����� (������������ �������, �� ��� ������� ����� ������/���������)
            viewModel.NewPopularBooks = await _context.Books
                .Where(b => b.PublicationDate >= DateTime.UtcNow.AddDays(-30)) // ������������ �� ��������� �����
                .OrderByDescending(b => b.LikesCount + b.ReadsCount / 5.0) // ������� �����������
                .Take(10)
                .Select(b => new HomeViewModel.BookDisplayModel
                {
                    Id = b.Id,
                    Title = b.Title,
                    AuthorName = b.BookAuthors.Select(ba => ba.Author.PenName).FirstOrDefault() ?? "����������",
                    CoverImageUrl = b.CoverImageUrl,
                    ReadsCount = b.ReadsCount,
                    LikesCount = b.LikesCount
                })
                .ToListAsync();

            // ����� ���������� ������ (������������������ �������, �� ��� ������� ����� ���������)
            viewModel.NewPopularAuthors = await _context.Authors
                .Where(a => a.AuthorProfileCreationDate >= DateTime.UtcNow.AddDays(-90)) // ������� ������ �� ��������� 3 ������
                .OrderByDescending(a => a.BookAuthors.Sum(ba => ba.Book.ReadsCount))
                .Take(10)
                .Select(a => new HomeViewModel.AuthorDisplayModel
                {
                    Id = a.Id,
                    Name = a.PenName,
                    PhotoUrl = a.PhotoUrl,
                    RegistrationInfo = GetRelativeTime(a.AuthorProfileCreationDate), // ���������� ����������� �����
                    TotalReadsCount = a.BookAuthors.Sum(ba => ba.Book.ReadsCount)
                })
                .ToListAsync();


            // ������������ ��� ������������������ �������������
            if (viewModel.IsUserLoggedIn)
            {
                // � �������� ����������: var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                // ��� ������������:
                var userId = 1; // �������� �� �������� UserId ��������������� ������������

                // ������: ������������ �� ������ ������� ������ � �����
                var userPreferredGenreIds = await _context.UserPreferredGenres
                    .Where(upg => upg.UserId == userId)
                    .Select(upg => upg.GenreId)
                    .ToListAsync();

                var userPreferredTagIds = await _context.UserPreferredTags
                    .Where(upt => upt.UserId == userId)
                    .Select(upt => upt.TagId)
                    .ToListAsync();

                viewModel.RecommendedBooks = await _context.Books
                    .Where(b => b.BookGenres.Any(bg => userPreferredGenreIds.Contains(bg.GenreId)) || // ����� �� ������� ������
                                b.BookTags.Any(bt => userPreferredTagIds.Contains(bt.TagId)))       // ����� �� ������� �����
                    .OrderByDescending(b => b.LikesCount + b.ReadsCount) // ������� ������� ��� ������������
                    .Take(10)
                    .Select(b => new HomeViewModel.BookDisplayModel
                    {
                        Id = b.Id,
                        Title = b.Title,
                        AuthorName = b.BookAuthors.Select(ba => ba.Author.PenName).FirstOrDefault() ?? "����������",
                        CoverImageUrl = b.CoverImageUrl,
                        // ������ ������� ������ ������� ������������:
                        RecommendationReason = (b.BookGenres.Any(bg => userPreferredGenreIds.Contains(bg.GenreId)) ? "������ ��� �� ������ ���� ����" : "") +
                                               (b.BookTags.Any(bt => userPreferredTagIds.Contains(bt.TagId)) ? " � ���� ���" : "")
                    })
                    .ToListAsync();
            }

            return View(viewModel);
        }

        /// <summary>
        /// Action ��� ��������� ���� �� ���������� ����� (������������ AJAX).
        /// </summary>
        /// <param name="genreId">ID ���������� �����.</param>
        [HttpGet]
        public async Task<IActionResult> GetBooksByGenre(int genreId)
        {
            var books = await _context.Books
                .Where(b => b.BookGenres.Any(bg => bg.GenreId == genreId))
                .OrderByDescending(b => b.PublicationDate)
                .Take(10)
                .Select(b => new HomeViewModel.BookDisplayModel
                {
                    Id = b.Id,
                    Title = b.Title,
                    AuthorName = b.BookAuthors.Select(ba => ba.Author.PenName).FirstOrDefault() ?? "����������",
                    CoverImageUrl = b.CoverImageUrl,
                    ReadsCount = b.ReadsCount,
                    LikesCount = b.LikesCount
                })
                .ToListAsync();

            // ���������� ��������� �������������, ������� ����� ��������� ������ ����
            return PartialView("_BookListPartial", books);
        }

        /// <summary>
        /// ��������������� ������� ��� ��������� �������������� ������� (��������, "2 ��� �����").
        /// ������� ����������� ��� �������������� �������������� EF Core.
        /// </summary>
        /// <param name="dateTime">���� � ����� ��� ��������������.</param>
        /// <returns>������, �������������� ������������� �����.</returns>
        private static string GetRelativeTime(DateTime dateTime)
        {
            var timeSpan = DateTime.UtcNow - dateTime;

            if (timeSpan <= TimeSpan.FromSeconds(60))
                return $"{timeSpan.Seconds} ������ �����";
            if (timeSpan <= TimeSpan.FromMinutes(60))
                return $"{timeSpan.Minutes} ����� �����";
            if (timeSpan <= TimeSpan.FromHours(24))
                return $"{timeSpan.Hours} ����� �����";
            if (timeSpan <= TimeSpan.FromDays(30))
                return $"{timeSpan.Days} ���� �����";
            if (timeSpan <= TimeSpan.FromDays(365))
                return $"{timeSpan.Days / 30} ������� �����";
            return $"{timeSpan.Days / 365} ��� �����";
        }

        // Action ��� ��������� ������
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
