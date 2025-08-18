// ������: MajorAuthor
// ����: Controllers/HomeController.cs
using Microsoft.AspNetCore.Mvc;
using MajorAuthor.Models; // ���������� ���� ViewModel
using MajorAuthor.Data; // ���������� ��� DbContext
using MajorAuthor.Data.Entities; // ��������� ��� ������� � ��������� PromotionPlan, Promotion, Book, Author, Genre � �.�.
using Microsoft.EntityFrameworkCore; // ��� ������� ���������� EF Core, ����� ��� Include, ToListAsync
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims; // ��������� ��� ClaimTypes
using System.Threading.Tasks;
using System.Diagnostics; // ��������� ��� Activity (��� ErrorViewModel)

namespace MajorAuthor.Controllers // ������������ ���� ��� ������������
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

            // ���������� ����� (��������, ���-10 �� ���������� ������ � ���������)
            viewModel.PopularBooks = await _context.Books
                .Include(b => b.BookAuthors) // �������� BookAuthors ��� ������� � Author
                    .ThenInclude(ba => ba.Author) // �������� Author
                .OrderByDescending(b => b.LikesCount + (double)b.ReadsCount / 10.0) // ������ ���������������� ��������
                .Take(10)
                .Select(b => new HomeViewModel.BookDisplayModel
                {
                    Id = b.Id,
                    Title = b.Title,
                    // ��� ������: ���� ����� ����� ����� ��������� �������,
                    // ����� ���������� �� ����� ��� ������� �������/���������.
                    // ���������� Author.PenName (���������)
                    AuthorName = b.BookAuthors.Select(ba => ba.Author.PenName).FirstOrDefault() ?? "����������",
                    CoverImageUrl = b.CoverImageUrl,
                    ReadsCount = b.ReadsCount,
                    LikesCount = b.LikesCount,
                    IsAdultContent = b.IsAdultContent
                })
                .ToListAsync();

            // ���������� ������ (��������, ���-10 �� ������ ���������� ��������� �� ����)
            viewModel.PopularAuthors = await _context.Authors
                .Select(a => new HomeViewModel.AuthorDisplayModel
                {
                    Id = a.Id,
                    // ���������� PenName ��� ����������� ����� ������
                    Name = a.PenName,
                    PhotoUrl = a.PhotoUrl,
                    BooksCount = a.BookAuthors.Count(),
                    TotalReadsCount = a.BookAuthors.Sum(ba => ba.Book.ReadsCount)
                })
                .OrderByDescending(a => a.TotalReadsCount) // ��������� �� TotalReadsCount, ������������� ����
                .Take(10)
                .ToListAsync();

            // ������� ����������� ����� (�� ��������� 7 ����, ������������� �� LastUpdateTime)
            viewModel.RecentlyUpdatedBooks = await _context.Books
                .Include(b => b.BookAuthors) // �������� BookAuthors ��� ������� � Author
                    .ThenInclude(ba => ba.Author) // �������� Author
                .Where(b => b.LastUpdateTime >= DateTime.UtcNow.AddDays(-7))
                .OrderByDescending(b => b.LastUpdateTime)
                .Take(10)
                .Select(b => new HomeViewModel.BookDisplayModel
                {
                    Id = b.Id,
                    Title = b.Title,
                    AuthorName = b.BookAuthors.Select(ba => ba.Author.PenName).FirstOrDefault() ?? "����������",
                    CoverImageUrl = b.CoverImageUrl,
                    UpdateInfo = GetRelativeTime(b.LastUpdateTime), // ���������� ����������� �����
                    ReadsCount = b.ReadsCount, // ���������
                    LikesCount = b.LikesCount // ���������
                })
                .ToListAsync();

            // ����� ���������� ����� (������������ �������, �� ��� ������� ����� ������/���������)
            viewModel.NewPopularBooks = await _context.Books
                .Include(b => b.BookAuthors) // �������� BookAuthors ��� ������� � Author
                    .ThenInclude(ba => ba.Author) // �������� Author
                .Where(b => b.PublicationDate >= DateTime.UtcNow.AddDays(-30)) // ������������ �� ��������� �����
                .OrderByDescending(b => b.LikesCount + (double)b.ReadsCount / 5.0) // ������� �����������
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
                .Select(a => new HomeViewModel.AuthorDisplayModel
                {
                    Id = a.Id,
                    Name = a.PenName, // ���������� PenName
                    PhotoUrl = a.PhotoUrl,
                    RegistrationInfo = GetRelativeTime(a.AuthorProfileCreationDate), // ���������� AuthorProfileCreationDate
                    TotalReadsCount = a.BookAuthors.Sum(ba => ba.Book.ReadsCount)
                })
                .OrderByDescending(a => a.TotalReadsCount)
                .Take(10)
                .ToListAsync();

            // �������������� ����� (�� ������ �������� �����������)
            viewModel.PromotedBooks = await _context.Promotions
                .Include(p => p.Book)
                    .ThenInclude(b => b.BookAuthors)
                        .ThenInclude(ba => ba.Author)
                .Where(p => p.StartDate <= DateTime.UtcNow && p.EndDate >= DateTime.UtcNow)
                .Select(p => new HomeViewModel.BookDisplayModel
                {
                    Id = p.Book.Id,
                    Title = p.Book.Title,
                    AuthorName = p.Book.BookAuthors.Select(ba => ba.Author.PenName).FirstOrDefault() ?? "����������",
                    CoverImageUrl = p.Book.CoverImageUrl,
                    ReadsCount = p.Book.ReadsCount,
                    LikesCount = p.Book.LikesCount,
                    IsAdultContent = p.Book.IsAdultContent
                })
                .ToListAsync();


            // ������������ ��� ������������������ �������������
            if (viewModel.IsUserLoggedIn)
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (currentUserId != null)
                {
                    // �������� ������� ����� ������������
                    var userPreferredGenreIds = await _context.UserPreferredGenres
                        .Where(upg => upg.ApplicationUserId == currentUserId)
                        .Select(upg => upg.GenreId)
                        .ToListAsync();

                    // �������� ������� ���� ������������
                    var userPreferredTagIds = await _context.UserPreferredTags
                        .Where(upt => upt.ApplicationUserId == currentUserId)
                        .Select(upt => upt.TagId)
                        .ToListAsync();

                    viewModel.RecommendedBooks = await _context.Books
                        .Include(b => b.BookAuthors)
                            .ThenInclude(ba => ba.Author)
                        .Include(b => b.BookGenres) // �������� ��� ���������� �� ������
                        .Include(b => b.BookTags)   // �������� ��� ���������� �� �����
                        .Where(b => b.BookGenres.Any(bg => userPreferredGenreIds.Contains(bg.GenreId)) || // ����� �� ������� ������
                                     b.BookTags.Any(bt => userPreferredTagIds.Contains(bt.TagId)))         // ����� �� ������� �����
                        .OrderByDescending(b => b.LikesCount + (double)b.ReadsCount) // ������� ������� ��� ������������
                        .Take(10)
                        .Select(b => new HomeViewModel.BookDisplayModel
                        {
                            Id = b.Id,
                            Title = b.Title,
                            AuthorName = b.BookAuthors.Select(ba => ba.Author.PenName).FirstOrDefault() ?? "����������",
                            CoverImageUrl = b.CoverImageUrl,
                            ReadsCount = b.ReadsCount,
                            LikesCount = b.LikesCount,
                            IsAdultContent = b.IsAdultContent,
                            // ������ ������� ������ ������� ������������:
                            RecommendationReason = (b.BookGenres.Any(bg => userPreferredGenreIds.Contains(bg.GenreId)) ? "������ ��� �� ������ ���� ����" : "") +
                                                   (b.BookTags.Any(bt => userPreferredTagIds.Contains(bt.TagId)) ? " � ���� ���" : "")
                        })
                        .ToListAsync();
                }
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
                .Include(b => b.BookAuthors)
                    .ThenInclude(ba => ba.Author)
                .Where(b => b.BookGenres.Any(bg => bg.GenreId == genreId))
                .OrderByDescending(b => b.PublicationDate) // ��������� �� ���� ����������
                .Take(10)
                .Select(b => new HomeViewModel.BookDisplayModel
                {
                    Id = b.Id,
                    Title = b.Title,
                    AuthorName = b.BookAuthors.Select(ba => ba.Author.PenName).FirstOrDefault() ?? "����������",
                    CoverImageUrl = b.CoverImageUrl,
                    ReadsCount = b.ReadsCount,
                    LikesCount = b.LikesCount,
                    IsAdultContent = b.IsAdultContent // ���������
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
            // ���������, ��� ErrorViewModel ��������� � MajorAuthor.Models (��� MajorAuthor.Web.Models, ���� � ��� ����� ������)
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
