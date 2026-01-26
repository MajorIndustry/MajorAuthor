using MajorAuthor.Data;
using MajorAuthor.Data.Entities;
using MajorAuthor.Models;
using MajorAuthor.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MajorAuthor.Controllers
{
    public partial class MyWorksController : Controller
    {
        private readonly IBookService _bookService;
        private readonly IChapterService _chapterService;
        private readonly IWorkService<Poem> _poemService;
        private readonly IWorkService<Blog> _blogService;
        private readonly IAuthorService _authorService;
        private readonly IBookInvitationService _bookInvitationService;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly INotificationService _notificationService;
        private Author _currentAuthor;
        private readonly ILogger<MyWorksController> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public MyWorksController(
            IBookService bookService,
            IWorkService<Poem> poemService,
            IWorkService<Blog> blogService,
            IAuthorService authorService,
            IBookInvitationService bookInvitationService,
            IEmailSender emailSender,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment webHostEnvironment,
            INotificationService notificationService,
            IChapterService chapterService,
            ILogger<MyWorksController> logger,
            IServiceScopeFactory serviceScopeFactory)
        {
            _bookService = bookService;
            _poemService = poemService;
            _blogService = blogService;
            _authorService = authorService;
            _bookInvitationService = bookInvitationService;
            _emailSender = emailSender;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _notificationService = notificationService;
            _chapterService = chapterService;
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        private async Task<Author> GetCurrentAuthorAsync()
        {
            if (_currentAuthor == null)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                _currentAuthor = await _authorService.GetAuthorByUserIdAsync(userId);
            }
            return _currentAuthor;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var author = await _authorService.GetAuthorByUserIdAsync(userId);
            var viewModel = new MyWorksViewModel();

            if (author == null)
            {
                viewModel.IsUserAuthor = false;
                viewModel.HasWorks = false;
            }
            else
            {
                viewModel.IsUserAuthor = true;

                var books = await _bookService.GetAllByAuthorIdAsync(author.Id);
                var poems = await _poemService.GetAllByAuthorIdAsync(author.Id);
                var blogs = await _blogService.GetAllByAuthorIdAsync(author.Id);

                viewModel.Works = new List<MyWorksViewModel.WorkDisplayModel>();
                viewModel.Works.AddRange(books.Select(b => new MyWorksViewModel.WorkDisplayModel { Id = b.Id, Title = b.Title, Type = "Книга" }));
                viewModel.Works.AddRange(poems.Select(p => new MyWorksViewModel.WorkDisplayModel { Id = p.Id, Title = p.Title, Type = "Стих" }));
                viewModel.Works.AddRange(blogs.Select(b => new MyWorksViewModel.WorkDisplayModel { Id = b.Id, Title = b.Title, Type = "Блог" }));

                viewModel.HasWorks = viewModel.Works.Any();
            }

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> CheckPenNameAvailability(string penName)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _authorService.CheckPenNameAvailabilityAsync(penName, userId);

            return Json(new
            {
                available = result.available,
                message = result.message
            });
        }
        private async Task SendChapterPublishNotificationsAsync(int bookId, int chapterId)
        {
            // Создаем новую область видимости для фоновой задачи
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                try
                {
                    // ВСЕ сервисы должны быть получены из новой области
                    var chapterService = scope.ServiceProvider.GetRequiredService<IChapterService>();
                    var bookService = scope.ServiceProvider.GetRequiredService<IBookService>();
                    var authorService = scope.ServiceProvider.GetRequiredService<IAuthorService>();
                    var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                    // Получаем книгу и главу через сервисы из НОВОЙ области
                    var book = await bookService.GetByIdAsync(bookId);
                    var chapter = await chapterService.GetChapterByIdAsync(chapterId);

                    if (book == null || chapter == null || !book.IsPublic || !chapter.IsPublic)
                    {
                        Console.WriteLine($"Не отправляем уведомления: книга или глава не опубликованы. Book: {bookId}, Chapter: {chapterId}");
                        return;
                    }

                    // 1. Получаем пользователей, добавивших книгу в избранное
                    var favoriteUserIds = await bookService.GetUsersWhoFavoritedBookAsync(bookId);
                    Console.WriteLine($"Пользователи, добавившие в избранное: {favoriteUserIds.Count}");

                    // 2. Получаем авторов книги
                    var bookAuthors = await bookService.GetBookAuthorsAsync(bookId);
                    Console.WriteLine($"Авторы книги: {bookAuthors.Count}");

                    var allTargetUserIds = new List<string>();

                    // Добавляем пользователей из избранного
                    allTargetUserIds.AddRange(favoriteUserIds);

                    // 3. Для каждого автора получаем его подписчиков
                    foreach (var author in bookAuthors)
                    {
                        var followers = await authorService.GetAuthorFollowersAsync(author.Id);
                        if (followers != null && followers.Any())
                        {
                            var followerUserIds = followers.Select(f => f.UserId).ToList();
                            Console.WriteLine($"Подписчики автора {author.Id}: {followerUserIds.Count}");
                            allTargetUserIds.AddRange(followerUserIds);
                        }
                    }

                    // 4. Удаляем дубликаты
                    var distinctUserIds = allTargetUserIds.Distinct().ToList();

                    // 5. Исключаем авторов книги
                    var finalUserIds = new List<string>();

                    // Получаем всех авторов для пользователей
                    var authorTasks = distinctUserIds.Select(async userId =>
                        new { UserId = userId, Author = await authorService.GetAuthorByUserIdAsync(userId) });

                    var userAuthors = await Task.WhenAll(authorTasks);

                    foreach (var userAuthor in userAuthors)
                    {
                        // Если пользователь не является автором ИЛИ является автором, но не автором этой книги
                        if (userAuthor.Author == null || !bookAuthors.Any(a => a.Id == userAuthor.Author.Id))
                        {
                            finalUserIds.Add(userAuthor.UserId);
                        }
                    }

                    Console.WriteLine($"Всего пользователей для уведомлений: {finalUserIds.Count}");

                    // 6. Отправляем уведомления
                    var message = $"В книге \"{book.Title}\" опубликована новая глава: \"{chapter.Title}\"";
                    var link = $"/Read/ReadChapter?bookId={bookId}&chapterId={chapterId}";

                    foreach (var userId in finalUserIds)
                    {
                        await notificationService.CreateNotificationAsync(
                            userId,
                            message,
                            "chapter_published",
                            link);
                        Console.WriteLine($"Уведомление отправлено пользователю {userId}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при отправке уведомлений: {ex.Message}");
                }
            }
        }
        private async Task<bool> IsUserBookAuthor(string userId, List<Author> bookAuthors, IAuthorService authorService)
        {
            // Используем authorService из новой области
            var userAuthor = await authorService.GetAuthorByUserIdAsync(userId);
            if (userAuthor == null)
                return false;

            return bookAuthors.Any(a => a.Id == userAuthor.Id);
        }
        public IActionResult AddWork()
        {
            return View();
        }

        public IActionResult CreateBook()
        {
            var model = new CreateBookViewModel();

            // Загружаем типы книг и жанры для выпадающих списков
            model.BookTypes = _bookService.GetAllBookTypesAsync().Result;
            model.Genres = _bookService.GetAllGenresAsync().Result;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBook(CreateBookViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var author = await _authorService.GetAuthorByUserIdAsync(userId);
                var currentUser = await _userManager.GetUserAsync(User);

                if (author != null)
                {
                    string uniqueFileName = null;
                    if (model.CoverImage != null)
                    {
                        string uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "books");
                        if (!Directory.Exists(uploadFolder))
                        {
                            Directory.CreateDirectory(uploadFolder);
                        }
                        uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.CoverImage.FileName);
                        string filePath = Path.Combine(uploadFolder, uniqueFileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.CoverImage.CopyToAsync(fileStream);
                        }
                    }

                    var newBook = new Book
                    {
                        Title = model.Title,
                        Description = model.Description,
                        CoverImageUrl = uniqueFileName != null ? "/images/books/" + uniqueFileName : null,
                        IsAdultContent = model.IsAdultContent,
                        IsPublic = false,
                        PublicationDate = DateTime.UtcNow,
                        LastUpdateTime = DateTime.UtcNow,
                        TypeId = model.TypeId,
                        StatusId = 1 // По умолчанию "в процессе"
                    };

                    await _bookService.AddAsync(newBook);
                    await _bookService.LinkBookToAuthorAsync(newBook.Id, author.Id);

                    // Добавляем выбранные жанры
                    if (model.SelectedGenreIds != null && model.SelectedGenreIds.Any())
                    {
                        foreach (var genreId in model.SelectedGenreIds)
                        {
                            await _bookService.AddGenreToBookAsync(newBook.Id, genreId);
                        }
                    }

                    // Добавляем теги
                    if (model.Tags != null && model.Tags.Any())
                    {
                        foreach (var tag in model.Tags)
                        {
                            if (!string.IsNullOrWhiteSpace(tag))
                            {
                                await _bookService.AddTagToBookAsync(newBook.Id, tag);
                            }
                        }
                    }

                    bool invitationSent = false;
                    if (!string.IsNullOrEmpty(model.CoAuthorEmail))
                    {
                        // Ищем пользователя по email
                        var inviteeUser = await _userManager.FindByEmailAsync(model.CoAuthorEmail);

                        if (inviteeUser != null)
                        {
                            // Проверяем, является ли пользователь автором
                            var inviteeAuthor = await _authorService.GetAuthorByUserIdAsync(inviteeUser.Id);
                            if (inviteeAuthor == null)
                            {
                                // Если пользователь не автор, создаем авторский профиль
                                var registrationResult = await _authorService.RegisterAuthorAsync(inviteeUser.Id, inviteeUser.UserName);
                                if (!registrationResult.success)
                                {
                                    TempData["WarningMessage"] = "Книга создана, но не удалось создать авторский профиль для приглашенного пользователя.";
                                    return View("BookCreated", newBook);
                                }
                                // Получаем созданного автора
                                inviteeAuthor = await _authorService.GetAuthorByUserIdAsync(inviteeUser.Id);
                            }
                        }

                        // Создаем приглашение
                        var invitation = await _bookInvitationService.CreateInvitationAsync(
                            newBook.Id,
                            model.CoAuthorEmail,
                            inviteeUser?.Id);

                        // Генерируем ссылку для принятия приглашения
                        var invitationLink = Url.Action(
                            "AcceptInvitation",
                            "MyWorks",
                            new { token = invitation.InvitationToken },
                            HttpContext.Request.Scheme);

                        // Отправляем приглашение
                        invitationSent = await SendInvitationAsync(
                            newBook.Id,
                            model.Title,
                            author.PenName,
                            model.CoAuthorEmail,
                            invitationLink,
                            inviteeUser?.Id);

                        if (invitationSent)
                        {
                            TempData["SuccessMessage"] = "Книга создана и приглашение отправлено!";
                        }
                        else
                        {
                            TempData["WarningMessage"] = "Книга создана, но не удалось отправить приглашение. Попробуйте позже.";
                        }
                    }
                    else
                    {
                        TempData["SuccessMessage"] = "Книга успешно создана!";
                    }

                    return View("BookCreated", newBook);
                }
            }

            // Если есть ошибки, перезагружаем списки для выпадающих списков
            model.BookTypes = await _bookService.GetAllBookTypesAsync();
            model.Genres = await _bookService.GetAllGenresAsync();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> AcceptInvitation(string token)
        {
            var invitation = await _bookInvitationService.GetInvitationByTokenAsync(token);

            if (invitation == null || invitation.IsAccepted || invitation.IsExpired)
            {
                ViewBag.Message = "Ссылка-приглашение недействительна, уже использована или истек срок действия.";
                return View("InvitationResult");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Если пользователь не авторизован
            if (string.IsNullOrEmpty(userId))
            {
                // Сохраняем токен в TempData для использования после регистрации
                TempData["InvitationToken"] = token;
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("AcceptInvitation", "MyWorks", new { token }) });
            }

            var currentUser = await _userManager.GetUserAsync(User);

            // Проверяем, соответствует ли email приглашения email пользователя
            if (currentUser.Email != invitation.InviteeEmail)
            {
                ViewBag.Message = "Это приглашение предназначено для другого email адреса.";
                return View("InvitationResult");
            }

            // Проверяем, является ли пользователь автором
            var author = await _authorService.GetAuthorByUserIdAsync(userId);
            if (author == null)
            {
                // Создаем авторский профиль для читателя
                var registrationResult = await _authorService.RegisterAuthorAsync(userId, currentUser.UserName);
                if (!registrationResult.success)
                {
                    ViewBag.Message = $"Не удалось создать авторский профиль: {registrationResult.message}";
                    return View("InvitationResult");
                }

                // Получаем созданного автора
                author = await _authorService.GetAuthorByUserIdAsync(userId);

                // Отправляем уведомление о том, что пользователь стал автором
                var notificationMessage = "Поздравляем! Вы стали автором на платформе MajorAuthor.";
                var profileUrl = Url.Action("MyProfile", "Profile");
                await _notificationService.CreateNotificationAsync(
                    userId,
                    notificationMessage,
                    "became_author",
                    profileUrl);
            }

            // Добавляем пользователя как соавтора книги
            await _bookService.LinkBookToAuthorAsync(invitation.BookId, author.Id);

            // Отмечаем приглашение как принятое
            invitation.IsAccepted = true;
            invitation.InviteeUserId = userId;
            await _bookInvitationService.UpdateInvitationAsync(invitation);

            // Отправляем уведомление о принятии приглашения
            var acceptNotificationMessage = $"Вы стали соавтором книги «{invitation.Book.Title}»";
            var bookUrl = Url.Action("Read", "Books", new { id = invitation.BookId });
            await _notificationService.CreateNotificationAsync(
                userId,
                acceptNotificationMessage,
                "coauthor_accepted",
                bookUrl);

            ViewBag.Message = "Поздравляем! Вы были добавлены в качестве соавтора.";
            ViewBag.BookTitle = invitation.Book.Title;
            ViewBag.BookId = invitation.BookId;

            return View("InvitationResult");
        }

        private async Task<bool> SendInvitationAsync(int bookId, string bookTitle, string inviterName,
            string inviteeEmail, string invitationLink, string inviteeUserId = null)
        {
            try
            {
                var isExistingUser = !string.IsNullOrEmpty(inviteeUserId);

                var subject = $"Приглашение стать соавтором книги «{bookTitle}»";
                var message = $@"
                    <h3>Здравствуйте!</h3>
                    <p>Автор <strong>{inviterName}</strong> приглашает вас стать соавтором книги «<strong>{bookTitle}</strong>».</p>
                    
                    {(isExistingUser ?
                        "<p>Поскольку вы уже зарегистрированы на нашей платформе, вы можете сразу принять приглашение.</p>" :
                        "<p>Для принятия приглашения вам необходимо зарегистрироваться на нашей платформе.</p>")}
                    
                    <p>Чтобы принять приглашение, перейдите по ссылке: 
                    <a href='{invitationLink}' style='color: #D4AF37; font-weight: bold;'>{invitationLink}</a></p>
                    
                    <p>Ссылка действительна в течение 7 дней.</p>
                    <br/>
                    <p>С уважением,<br/>Команда MajorAuthor</p>";

                await _emailSender.SendEmailAsync(inviteeEmail, subject, message);

                // Отправляем уведомление на платформу, если пользователь зарегистрирован
                if (isExistingUser)
                {
                    var notificationMessage = $"Автор {inviterName} приглашает вас стать соавтором книги «{bookTitle}»";
                    await _notificationService.CreateNotificationAsync(
                        inviteeUserId,
                        notificationMessage,
                        "coauthor_invitation",
                        invitationLink);
                }

                return true;
            }
            catch (Exception ex)
            {
                // Логирование ошибки
                Console.WriteLine($"Ошибка при отправке приглашения: {ex.Message}");
                return false;
            }
        }

        [HttpGet]
        public async Task<IActionResult> BookDashboard(int id)
        {
            var author = await GetCurrentAuthorAsync();
            if (author == null)
            {
                return RedirectToAction("Index");
            }

            var book = await _bookService.GetByIdAsync(id);
            if (book == null)
            {
                return NotFound();
            }

            if (!await _bookService.IsAuthorOfBookAsync(id, author.Id))
            {
                return Forbid();
            }

            // Получаем все необходимые данные
            var stats = await _bookService.GetBookStatsAsync(id);
            var tags = await _bookService.GetBookTagsStringsAsync(id);
            var chapters = await _chapterService.GetChaptersByBookIdAsync(id);
            var coAuthors = await _bookService.GetBookCoAuthorsAsync(id);
            var allGenres = await _bookService.GetAllGenresAsync();
            var allTypes = await _bookService.GetAllBookTypesAsync();
            var allCycles = await _bookService.GetCyclesByAuthorIdAsync(author.Id);
            var allTags = await _bookService.GetAllTagsAsync();

            var bookGenres = await _bookService.GetBookGenresAsync(id);
            var genreNames = bookGenres.Select(g => g.Name).ToList();

            // Исключаем текущего автора из списка соавторов
            var otherCoAuthors = coAuthors.Where(ca => ca.Id != author.Id).ToList();

            // НОВОЕ: Получаем статистику по годам
            var yearlyStats = await _bookService.GetYearlyStatsAsync(id);

            var model = new BookDashboardViewModel
            {
                Id = book.Id,
                Title = book.Title,
                Description = book.Description,
                CoverImageUrl = book.CoverImageUrl,
                IsAdultContent = book.IsAdultContent,
                EnableTTS = book.EnableTTS,
                AllowDownload = book.AllowDownload,
                PublicationDate = book.PublicationDate,
                LastUpdate = book.LastUpdateTime,
                Status = book.Status?.Name ?? "в процессе",
                WorkType = book.Type?.Name,
                TypeId = book.TypeId,
                Cycle = book.Cycle?.Name,
                CycleId = book.CycleId,
                Genre = string.Join(", ", genreNames),
                TotalCharacters = stats.TotalCharacters,
                TotalPages = stats.TotalPages,
                CharactersUntilNextUpdate = Math.Max(0, 15000 - (stats.TotalCharacters % 15000)),
                PagesUntilNextUpdate = (int)Math.Ceiling(stats.TotalPages / 10.0),
                CurrentAuthorName = author.PenName,
                CurrentAuthorId = author.Id,
                AuthorId = author.Id,
                AuthorName = author.PenName,
                Chapters = chapters.Select(c => new ChapterViewModel
                {
                    Id = c.Id,
                    Title = c.Title,
                    Order = c.Order,
                    IsPublic = c.IsPublic,
                    CharacterCount = c.Pages.Sum(p => p.TextContent?.Length ?? 0),
                    PublicationDate = c.PublicationDate
                }).ToList(),
                Tags = tags,
                CoAuthors = otherCoAuthors,
                AllTypes = allTypes.Select(t => new BookTypeInfo { Id = t.Id, Name = t.Name }).ToList(),
                AllCycles = allCycles.Select(c => new BookCycleInfo { Id = c.Id, Name = c.Name }).ToList(),
                AllGenres = allGenres.Select(g => new GenreInfo { Id = g.Id, Name = g.Name }).ToList(),
                AllStatuses = new List<BookStatusInfo>
        {
            new BookStatusInfo { Value = "в процессе", Name = "В процессе" },
            new BookStatusInfo { Value = "завершена", Name = "Завершена" },
            new BookStatusInfo { Value = "заморожена", Name = "Заморожена" },
            new BookStatusInfo { Value = "заброшена", Name = "Заброшена" }
        },
                IsPublic = book.IsPublic,
                LikesCount = book.LikesCount,
                ViewsCount = book.ReadsCount,
                TagsString = string.Join(", ", tags),
                // НОВОЕ: Добавляем статистику по годам
                YearlyStats = yearlyStats
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookDashboard(BookDashboardViewModel model)
        {
            var author = await GetCurrentAuthorAsync();
            if (author == null) return RedirectToAction("Index");

            if (!await _bookService.IsAuthorOfBookAsync(model.Id, author.Id)) return Forbid();

            // ОЧИЩАЕМ ОШИБКИ ДЛЯ ПОЛЕЙ, КОТОРЫЕ НЕ ОБЯЗАТЕЛЬНЫ ПРИ РЕДАКТИРОВАНИИ
            ClearNonRequiredFieldErrors();
            var currentBook = await _bookService.GetByIdAsync(model.Id);
            var wasPublic = currentBook?.IsPublic ?? false;
            // Валидация
            if (string.IsNullOrWhiteSpace(model.Title))
            {
                ModelState.AddModelError("Title", "Название книги обязательно");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // НОВОЕ: Удаляем обложку если установлен флаг
                    if (model.RemoveCover)
                    {
                        await _bookService.RemoveBookCoverAsync(model.Id, _webHostEnvironment.WebRootPath);
                    }

                    // 1. Обновляем обложку (только если файл передан)
                    if (model.CoverImage != null && model.CoverImage.Length > 0)
                    {
                        await _bookService.UpdateBookCoverAsync(model.Id, model.CoverImage, _webHostEnvironment.WebRootPath);
                    }

                    // 2. Обновляем свойства (теперь чекбоксы приходят корректно из скрытых полей)
                    await _bookService.UpdateBookPropertiesAsync(model.Id, model);

                    // 3. Жанры
                    string genresRaw = model.Genre ?? Request.Form["Genre"].ToString();
                    if (!string.IsNullOrEmpty(genresRaw))
                    {
                        var genreNames = genresRaw.Split(',')
                            .Select(g => g.Trim())
                            .Where(g => !string.IsNullOrEmpty(g))
                            .ToList();
                        await _bookService.UpdateBookGenresAsync(model.Id, genreNames);
                    }
                    else
                    {
                        await _bookService.UpdateBookGenresAsync(model.Id, new List<string>());
                    }

                    // 4. Теги
                    string tagsRaw = model.TagsString ?? Request.Form["TagsString"].ToString();
                    List<string> tagList = new List<string>();
                    if (!string.IsNullOrEmpty(tagsRaw))
                    {
                        tagList = tagsRaw.Split(',')
                            .Select(t => t.Trim())
                            .Where(t => !string.IsNullOrEmpty(t))
                            .ToList();
                    }
                    await _bookService.UpdateBookTagsAsync(model.Id, tagList);
                    if (model.IsPublic && !wasPublic)
                    {
                        // Отправляем уведомления в фоновом режиме
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                await SendBookPublishNotificationsAsync(model.Id, author.Id);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Ошибка при отправке уведомлений: {ex.Message}");
                            }
                        });
                    }
                    TempData["SuccessMessage"] = "Изменения сохранены успешно!";
                    return RedirectToAction("BookDashboard", new { id = model.Id });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Ошибка при сохранении: {ex.Message}");
                }
            }

            // Если ошибка, перезагружаем данные
            await LoadBookDashboardData(model.Id, model, author);
            return View(model);
        }

        private void ClearNonRequiredFieldErrors()
        {
            var fieldsToClear = new[] {
                "Cycle", "AuthorName",
                "CoverImageUrl", "CoverImage", // <--- ДОБАВЛЕНО CoverImage
                "CurrentAuthorName", "CurrentAuthorId",
                "TotalCharacters", "TotalPages", "CharactersUntilNextUpdate",
                "PagesUntilNextUpdate", "LikesCount", "ViewsCount", "LastUpdate",
                "PublicationDate", "CoAuthors", "Chapters", "Tags", "AllTypes",
                "AllCycles", "AllGenres", "AllStatuses", "Authors", "WorkType"
            };

            foreach (var field in fieldsToClear)
            {
                if (ModelState.ContainsKey(field))
                {
                    ModelState[field].Errors.Clear();
                    ModelState[field].ValidationState = Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Valid;
                }
            }
        }


        private async Task LoadBookDashboardData(int bookId, BookDashboardViewModel model, Author author)
        {
            var book = await _bookService.GetByIdAsync(bookId);
            var stats = await _bookService.GetBookStatsAsync(bookId);
            var tags = await _bookService.GetBookTagsStringsAsync(bookId);
            var chapters = await _chapterService.GetChaptersByBookIdAsync(bookId);
            var coAuthors = await _bookService.GetBookCoAuthorsAsync(bookId);
            var allGenres = await _bookService.GetAllGenresAsync();
            var allTypes = await _bookService.GetAllBookTypesAsync();
            var allCycles = await _bookService.GetCyclesByAuthorIdAsync(author.Id);
            var bookGenres = await _bookService.GetBookGenresAsync(bookId);
            var genreNames = bookGenres.Select(g => g.Name).ToList();

            // Восстанавливаем обязательные данные
            model.CoverImageUrl = book.CoverImageUrl;
            model.TotalCharacters = stats.TotalCharacters;
            model.TotalPages = stats.TotalPages;
            model.Chapters = chapters.Select(c => new ChapterViewModel
            {
                Id = c.Id,
                Title = c.Title,
                Order = c.Order,
                IsPublic = c.IsPublic,
                CharacterCount = c.Pages.Sum(p => p.TextContent?.Length ?? 0),
                PublicationDate = c.PublicationDate
            }).ToList();
            model.Tags = tags;
            model.CoAuthors = coAuthors.Where(ca => ca.Id != author.Id).ToList();
            model.AllGenres = allGenres.Select(g => new GenreInfo { Id = g.Id, Name = g.Name }).ToList();
            model.AllTypes = allTypes.Select(t => new BookTypeInfo { Id = t.Id, Name = t.Name }).ToList();
            model.AllCycles = allCycles.Select(c => new BookCycleInfo { Id = c.Id, Name = c.Name }).ToList();
            model.LikesCount = book.LikesCount;
            model.ViewsCount = book.ReadsCount;
            model.LastUpdate = book.LastUpdateTime;
            model.AuthorName = author.PenName;
            model.AuthorId = author.Id;
            model.CurrentAuthorName = author.PenName;
            model.CurrentAuthorId = author.Id;
            model.Status = book.Status?.Name ?? "в процессе";
            model.WorkType = book.Type?.Name;
            model.Cycle = book.Cycle?.Name;
            model.TagsString = string.Join(", ", tags);
        }

        // API методы
        [HttpGet]
        public async Task<IActionResult> GetBookTypes()
        {
            var types = await _bookService.GetAllBookTypesAsync();
            return Json(types.Select(t => new { id = t.Id, name = t.Name }));
        }

        [HttpGet]
        public async Task<IActionResult> GetTags(string query = "")
        {
            var tags = await _bookService.GetAllTagsAsync();

            if (!string.IsNullOrEmpty(query))
            {
                tags = tags.Where(t => t.Name.ToLower().Contains(query.ToLower())).ToList();
            }

            return Json(tags.Select(t => new { id = t.Id, name = t.Name }));
        }

        [HttpGet]
        public async Task<IActionResult> GetGenres(string query = "")
        {
            var genres = await _bookService.GetAllGenresAsync();

            if (!string.IsNullOrEmpty(query))
            {
                genres = genres.Where(g => g.Name.ToLower().Contains(query.ToLower())).ToList();
            }

            return Json(genres.Select(g => new { id = g.Id, name = g.Name }));
        }

        [HttpPost]
        public async Task<IActionResult> CreateCycle([FromBody] CreateCycleModel model)
        {
            var author = await GetCurrentAuthorAsync();
            if (author == null)
            {
                return BadRequest(new { success = false, message = "Автор не найден" });
            }

            try
            {
                var cycle = await _bookService.CreateCycleAsync(model.Name, author.Id);
                return Ok(new { success = true, cycle = new { id = cycle.Id, name = cycle.Name } });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> TogglePublication([FromBody] TogglePublicationModel model)
        {
            var author = await GetCurrentAuthorAsync();
            if (!await _bookService.IsAuthorOfBookAsync(model.bookId, author.Id))
            {
                return Forbid();
            }

            var book = await _bookService.GetByIdAsync(model.bookId);
            if (book != null)
            {
                var wasPublic = book.IsPublic;
                book.IsPublic = model.isPublic;
                book.LastUpdateTime = DateTime.UtcNow;
                await _bookService.UpdateAsync(book);

                // Если книга только что опубликована
                if (model.isPublic && !wasPublic)
                {
                    // Отправляем уведомления в фоновом режиме
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await SendBookPublishNotificationsAsync(book.Id, author.Id);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Ошибка при отправке уведомлений: {ex.Message}");
                        }
                    });
                }

                return Ok(new { success = true, isPublic = book.IsPublic });
            }

            return BadRequest(new { success = false, message = "Книга не найдена" });
        }
        private async Task SendBookPublishNotificationsAsync(int bookId, int authorId)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                try
                {
                    // Все сервисы из новой области
                    var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                    var bookService = scope.ServiceProvider.GetRequiredService<IBookService>();
                    var authorService = scope.ServiceProvider.GetRequiredService<IAuthorService>();

                    var book = await bookService.GetByIdAsync(bookId);
                    if (book == null || !book.IsPublic) return;

                    // Получаем всех авторов книги
                    var bookAuthors = await bookService.GetBookAuthorsAsync(bookId);

                    // Получаем текущего автора (кто опубликовал)
                    var publishingAuthor = bookAuthors.FirstOrDefault(a => a.Id == authorId);
                    var authorName = publishingAuthor?.PenName ?? "Автор";

                    var allTargetUserIds = new List<string>();

                    // 1. Получаем подписчиков всех авторов
                    foreach (var bookAuthor in bookAuthors)
                    {
                        var followers = await authorService.GetAuthorFollowersAsync(bookAuthor.Id);
                        if (followers != null && followers.Any())
                        {
                            var followerUserIds = followers.Select(f => f.UserId).ToList();
                            allTargetUserIds.AddRange(followerUserIds);
                        }
                    }

                    // 2. Удаляем дубликаты
                    var distinctUserIds = allTargetUserIds.Distinct().ToList();

                    // 3. Исключаем авторов книги
                    var finalUserIds = new List<string>();

                    // Получаем всех авторов для пользователей
                    var authorTasks = distinctUserIds.Select(async userId =>
                        new { UserId = userId, Author = await authorService.GetAuthorByUserIdAsync(userId) });

                    var userAuthors = await Task.WhenAll(authorTasks);

                    foreach (var userAuthor in userAuthors)
                    {
                        // Если пользователь не является автором ИЛИ является автором, но не автором этой книги
                        if (userAuthor.Author == null || !bookAuthors.Any(a => a.Id == userAuthor.Author.Id))
                        {
                            finalUserIds.Add(userAuthor.UserId);
                        }
                    }

                    // 4. Отправляем уведомления
                    var message = $"Автор {authorName} опубликовал новую книгу: \"{book.Title}\"";
                    var link = $"/Read/ReadBook?id={book.Id}";

                    foreach (var userId in finalUserIds)
                    {
                        await notificationService.CreateNotificationAsync(
                            userId,
                            message,
                            "book_published",
                            link);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при отправке уведомлений о публикации книги: {ex.Message}");
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddTag(int bookId, string newTag)
        {
            var author = await GetCurrentAuthorAsync();
            if (!await _bookService.IsAuthorOfBookAsync(bookId, author.Id))
            {
                return Forbid();
            }

            if (!string.IsNullOrEmpty(newTag))
            {
                // Убеждаемся, что тег начинается с #
                if (!newTag.StartsWith("#"))
                {
                    newTag = "#" + newTag;
                }

                // Ограничиваем длину тега
                if (newTag.Length > 50)
                {
                    newTag = newTag.Substring(0, 50);
                }

                await _bookService.AddTagToBookAsync(bookId, newTag);
                TempData["SuccessMessage"] = "Тег успешно добавлен";
            }
            else
            {
                TempData["ErrorMessage"] = "Тег не может быть пустым";
            }

            return RedirectToAction("BookDashboard", new { id = bookId });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveTag(int bookId, string tagName)
        {
            var author = await GetCurrentAuthorAsync();
            if (!await _bookService.IsAuthorOfBookAsync(bookId, author.Id))
            {
                return Forbid();
            }

            await _bookService.RemoveTagFromBookAsync(bookId, tagName);
            TempData["SuccessMessage"] = "Тег успешно удален";

            return RedirectToAction("BookDashboard", new { id = bookId });
        }

        [HttpPost]
        public async Task<IActionResult> AddCoAuthor(int bookId, string newCoAuthorEmail)
        {
            var author = await GetCurrentAuthorAsync();
            if (!await _bookService.IsAuthorOfBookAsync(bookId, author.Id))
            {
                return Forbid();
            }

            if (!string.IsNullOrEmpty(newCoAuthorEmail))
            {
                // Ищем пользователя по email
                var coAuthorUser = await _userManager.FindByEmailAsync(newCoAuthorEmail);
                if (coAuthorUser != null)
                {
                    // Проверяем, является ли пользователь автором
                    var coAuthor = await _authorService.GetAuthorByUserIdAsync(coAuthorUser.Id);
                    if (coAuthor == null)
                    {
                        // Создаем авторский профиль
                        var registrationResult = await _authorService.RegisterAuthorAsync(coAuthorUser.Id, coAuthorUser.UserName);
                        if (registrationResult.success)
                        {
                            coAuthor = await _authorService.GetAuthorByUserIdAsync(coAuthorUser.Id);
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Не удалось создать авторский профиль для приглашенного пользователя";
                            return RedirectToAction("BookDashboard", new { id = bookId });
                        }
                    }

                    // Добавляем как соавтора
                    await _bookService.LinkBookToAuthorAsync(bookId, coAuthor.Id);
                    TempData["SuccessMessage"] = "Соавтор успешно добавлен";
                }
                else
                {
                    TempData["ErrorMessage"] = "Пользователь с таким email не найден";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Email не может быть пустым";
            }

            return RedirectToAction("BookDashboard", new { id = bookId });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveCoAuthor(int bookId, int coAuthorId)
        {
            var author = await GetCurrentAuthorAsync();
            if (!await _bookService.IsAuthorOfBookAsync(bookId, author.Id))
            {
                return Forbid();
            }

            await _bookService.RemoveAuthorFromBookAsync(bookId, coAuthorId);
            TempData["SuccessMessage"] = "Соавтор успешно удален";

            return RedirectToAction("BookDashboard", new { id = bookId });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateBookStatus(int bookId, string status)
        {
            var author = await GetCurrentAuthorAsync();
            if (!await _bookService.IsAuthorOfBookAsync(bookId, author.Id))
            {
                return Forbid();
            }

            var book = await _bookService.GetByIdAsync(bookId);
            if (book != null)
            {
                book.IsPublic = status == "завершено";
                await _bookService.UpdateAsync(book);
            }

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteBook(int bookId)
        {
            var author = await GetCurrentAuthorAsync();
            if (!await _bookService.IsAuthorOfBookAsync(bookId, author.Id))
            {
                return Forbid();
            }

            var book = await _bookService.GetByIdAsync(bookId);
            if (book != null)
            {
                await _bookService.DeleteAsync(book);
                TempData["SuccessMessage"] = "Книга успешно удалена";
            }

            return Ok();
        }

        public async Task<IActionResult> CreateBlog()
        {
            var author = await GetCurrentAuthorAsync();
            if (author == null)
            {
                return RedirectToAction("Index");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBlog(CreateBlogModel model)
        {
            if (ModelState.IsValid)
            {
                var author = await GetCurrentAuthorAsync();
                if (author != null)
                {
                    var newBlog = new Blog
                    {
                        Title = model.Title,
                        Content = model.Content,
                        AuthorId = author.Id,
                        PublicationDate = System.DateTime.UtcNow
                    };

                    if (model.Photo != null)
                    {
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.Photo.FileName);
                        string uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "blogs");

                        if (!Directory.Exists(uploadFolder))
                        {
                            Directory.CreateDirectory(uploadFolder);
                        }

                        string filePath = Path.Combine(uploadFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.Photo.CopyToAsync(fileStream);
                        }
                        newBlog.ImageUrl = "/images/blogs/" + uniqueFileName;
                    }

                    await _blogService.AddAsync(newBlog);
                    return RedirectToAction("Index");
                }
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> RegisterAuthor([FromBody] AuthorRegistrationModel model)
        {
            if (string.IsNullOrEmpty(model.PenName))
            {
                return Json(new { success = false, message = "Пожалуйста, введите псевдоним или имя." });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Ошибка: пользователь не аутентифицирован." });
            }

            await _authorService.RegisterAuthorAsync(userId, model.PenName);
            return Json(new { success = true, message = "Поздравляем, вы стали автором! Ждём от вас новых интересных произведений." });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteWork([FromQuery] int id, [FromQuery] string type)
        {
            var author = await GetCurrentAuthorAsync();
            if (author == null)
            {
                return Forbid();
            }

            // Используем оператор switch для выбора нужного сервиса
            switch (type)
            {
                case "Книга":
                    var book = await _bookService.GetByIdAsync(id);
                    if (book == null) return NotFound();
                    if (!(await _bookService.IsAuthorOfBookAsync(id, author.Id))) return Forbid();

                    // Удаление обложки книги
                    if (!string.IsNullOrEmpty(book.CoverImageUrl))
                    {
                        string filePath = Path.Combine(_webHostEnvironment.WebRootPath, book.CoverImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                    }

                    await _bookService.DeleteAsync(book);
                    return Ok(new { message = $"Книга \"{book.Title}\" успешно удалена." });

                case "Стих":
                    var poem = await _poemService.GetByIdAsync(id);
                    if (poem == null) return NotFound();
                    if (poem.AuthorId != author.Id) return Forbid();

                    await _poemService.DeleteAsync(poem);
                    return Ok(new { message = $"Стихотворение \"{poem.Title}\" успешно удалено." });

                case "Блог":
                    var blog = await _blogService.GetByIdAsync(id);
                    if (blog == null) return NotFound();
                    if (blog.AuthorId != author.Id) return Forbid();

                    // Удаление изображения блога
                    if (!string.IsNullOrEmpty(blog.ImageUrl))
                    {
                        string filePath = Path.Combine(_webHostEnvironment.WebRootPath, blog.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                    }

                    await _blogService.DeleteAsync(blog);
                    return Ok(new { message = $"Блог \"{blog.Title}\" успешно удален." });

                default:
                    return BadRequest(new { message = "Неизвестный тип произведения." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditPoem(int id)
        {
            var author = await GetCurrentAuthorAsync();
            if (author == null)
            {
                return RedirectToAction("Index");
            }

            var poem = await _poemService.GetByIdAsync(id);

            if (poem == null)
            {
                return NotFound();
            }

            // Проверка, что текущий пользователь является автором стиха
            if (poem.AuthorId != author.Id)
            {
                return Forbid();
            }

            // Создаем модель для передачи в представление
            var model = new EditPoemModel
            {
                Id = poem.Id,
                Title = poem.Title,
                Content = poem.Content
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPoem(EditPoemModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var author = await GetCurrentAuthorAsync();
            if (author == null)
            {
                return RedirectToAction("Index");
            }

            try
            {
                var existingPoem = await _poemService.GetByIdAsync(model.Id);

                if (existingPoem == null)
                {
                    return NotFound();
                }

                if (existingPoem.AuthorId != author.Id)
                {
                    return Forbid();
                }

                existingPoem.Title = model.Title;
                existingPoem.Content = model.Content;
                existingPoem.PublicationDate = DateTime.UtcNow;

                await _poemService.UpdateAsync(existingPoem);

                TempData["SuccessMessage"] = "Стих успешно обновлен!";
                return RedirectToAction("Index");
            }
            catch (DbUpdateException ex)
            {
                // Обрабатываем ошибку уникальности контента при редактировании
                if (ex.InnerException?.Message?.Contains("IX_Poems_Content") == true ||
            ex.InnerException?.Message?.Contains("UNIQUE") == true ||
            ex.InnerException?.Message?.Contains("duplicate") == true)
                {
                    ModelState.AddModelError("Content", "Стих с таким содержанием уже существует в системе.");
                }
                else
                {
                    ModelState.AddModelError("", "Произошла ошибка при обновлении стиха. Пожалуйста, попробуйте еще раз.");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Произошла непредвиденная ошибка: {ex.Message}");
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditBlog(int id)
        {
            var author = await GetCurrentAuthorAsync();
            if (author == null)
            {
                return RedirectToAction("Index");
            }

            // Получаем блог из базы данных
            var blog = await _blogService.GetByIdAsync(id);

            if (blog == null)
            {
                return NotFound();
            }

            // Проверка, что текущий пользователь является автором блога
            if (blog.AuthorId != author.Id)
            {
                return Forbid();
            }

            // Создаем модель для передачи в представление
            var model = new EditBlogModel
            {
                Id = blog.Id,
                Title = blog.Title,
                Content = blog.Content,
                ExistingImageUrl = blog.ImageUrl
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBlog(EditBlogModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var author = await GetCurrentAuthorAsync();
            if (author == null)
            {
                return RedirectToAction("Index");
            }

            var existingBlog = await _blogService.GetByIdAsync(model.Id);
            if (existingBlog == null)
            {
                return NotFound();
            }

            if (existingBlog.AuthorId != author.Id)
            {
                return Forbid();
            }

            // Обрабатываем удаление существующего изображения
            if (model.RemoveExistingPhoto)
            {
                if (!string.IsNullOrEmpty(existingBlog.ImageUrl))
                {
                    string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath,
                        existingBlog.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                    existingBlog.ImageUrl = null;
                }
            }

            // Обрабатываем загрузку нового изображения
            if (model.Photo != null)
            {
                string uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "blogs");
                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                // Удаляем старое изображение если загружаем новое
                if (!string.IsNullOrEmpty(existingBlog.ImageUrl))
                {
                    string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath,
                        existingBlog.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.Photo.FileName);
                string filePath = Path.Combine(uploadFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.Photo.CopyToAsync(fileStream);
                }
                existingBlog.ImageUrl = "/images/blogs/" + uniqueFileName;
            }

            // Обновляем остальные поля
            existingBlog.Title = model.Title;
            existingBlog.Content = model.Content;
            existingBlog.LastUpdateTime = DateTime.UtcNow;

            await _blogService.UpdateAsync(existingBlog);
            TempData["SuccessMessage"] = "Блог успешно обновлен!";
            return RedirectToAction("Index");
        }

        public class CreateCycleModel
        {
            public string Name { get; set; }
        }

        public class TogglePublicationModel
        {
            public int bookId { get; set; }
            public bool isPublic { get; set; }
        }
        // Добавьте в MyWorksController.cs
        [HttpGet]
        public async Task<IActionResult> EditChapterCollaborative(int bookId, int? chapterId)
        {
            var author = await GetCurrentAuthorAsync();
            if (author == null) return RedirectToAction("Index");

            if (!await _bookService.IsAuthorOfBookAsync(bookId, author.Id))
            {
                return Forbid();
            }

            var book = await _bookService.GetByIdAsync(bookId);
            if (book == null) return NotFound();

            var model = new ChapterEditorViewModel
            {
                BookId = bookId,
                BookTitle = book.Title,
                ChapterId = chapterId,
                CurrentUserId = author.ApplicationUserId,
                CurrentUserPenName = author.PenName,
                UserColor = String.Format("#{0:X6}", new Random().Next(0x1000000)),
                CoAuthors = await _bookService.GetBookCoAuthorsAsync(bookId)
            };

            if (chapterId.HasValue)
            {
                var chapter = await _chapterService.GetChapterByIdAsync(chapterId.Value);
                if (chapter != null)
                {
                    model.ChapterTitle = chapter.Title;
                    model.IsPublic = chapter.IsPublic;
                    model.Order = chapter.Order;

                    // Получаем объединенный текст и изображения
                    var pagesInfo = await _chapterService.GetChapterPagesInfoAsync(chapterId.Value);
                    model.Content = pagesInfo.TextContent;
                    model.ImagePages = pagesInfo.ImagePages;
                }
            }
            else
            {
                model.ChapterTitle = "Новая глава";
                model.Content = "";
                model.IsPublic = false;

                var newChapter = new Chapter
                {
                    BookId = bookId,
                    Title = "Новая глава",
                    PublicationDate = DateTime.UtcNow,
                    Pages = new List<Page> { new Page { PageNumber = 1, TextContent = "" } }
                };
                await _chapterService.AddChapterAsync(newChapter);
                model.ChapterId = newChapter.Id;
            }

            return View("EditChapterCollaborative", model);
        }

        // Метод сохранения снэпшота (строки) в БД
        // В MyWorksController.cs - улучшенный метод сохранения
        // В BookService.cs - улучшенный метод сохранения
        [HttpPost]
        public async Task<IActionResult> SaveCollaborativeSnapshot([FromBody] ChapterEditorViewModel model)
        {
            var author = await GetCurrentAuthorAsync();
            if (author == null) return Forbid();

            // Получаем старый статус главы для проверки
            var oldChapter = model.ChapterId.HasValue
                ? await _chapterService.GetChapterByIdAsync(model.ChapterId.Value)
                : null;
            var wasPublic = oldChapter?.IsPublic ?? false;

            // Передаем IsPublic = true только при публикации
            if (await _chapterService.SaveChapterSnapshotAsync(model, author.ApplicationUserId))
            {
                // Если глава только что опубликована (была непубличной, стала публичной)
                if (model.IsPublic && !wasPublic)
                {
                    // Отправляем уведомления в фоновом режиме
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await SendChapterPublishNotificationsAsync(model.BookId, model.ChapterId.Value);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Ошибка при отправке уведомлений: {ex.Message}");
                        }
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = model.IsPublic ? "Глава опубликована" : "Глава сохранена",
                    redirectUrl = Url.Action("BookDashboard", "MyWorks", new { id = model.BookId })
                });
            }
            else
            {
                return BadRequest(new { success = false, message = "Ошибка сохранения" });
            }
        }
        [HttpGet]
        public async Task<IActionResult> CreatePoem()
        {
            var author = await GetCurrentAuthorAsync();
            if (author == null)
            {
                return RedirectToAction("Index");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePoem(CreatePoemModel model)
        {
            if (ModelState.IsValid)
            {
                var author = await GetCurrentAuthorAsync();
                if (author != null)
                {
                    try
                    {
                        var newPoem = new Poem
                        {
                            Title = model.Title,
                            Content = model.Content,
                            AuthorId = author.Id,
                            PublicationDate = DateTime.UtcNow
                        };

                        await _poemService.AddAsync(newPoem);
                        TempData["SuccessMessage"] = "Стих успешно создан!";
                        return RedirectToAction("Index");
                    }
                    catch (DbUpdateException ex)
                    {
                        // Проверяем, что это ошибка уникальности контента
                        if (ex.InnerException?.Message?.Contains("IX_Poems_ContentHash_AuthorId") == true ||
                            ex.InnerException?.Message?.Contains("UNIQUE") == true ||
                            ex.InnerException?.Message?.Contains("duplicate") == true)
                        {
                            ModelState.AddModelError("Content", "Стих с таким содержанием уже существует. Пожалуйста, напишите другой текст.");
                        }
                        else
                        {
                            ModelState.AddModelError("", $"Произошла ошибка при создании стиха: {ex.InnerException?.Message ?? ex.Message}");
                        }
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", $"Произошла непредвиденная ошибка: {ex.Message}");
                    }
                }
            }

            // Если есть ошибки, показываем форму снова
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteChapter(int chapterId)
        {
            var author = await GetCurrentAuthorAsync();
            if (author == null)
            {
                return Forbid();
            }

            var chapter = await _chapterService.GetChapterByIdAsync(chapterId);
            if (chapter == null)
            {
                return NotFound();
            }

            // Проверяем, что текущий пользователь является автором книги
            if (!await _bookService.IsAuthorOfBookAsync(chapter.BookId, author.Id))
            {
                return Forbid();
            }

            await _chapterService.DeleteChapterAsync(chapterId);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateChapterOrder([FromBody] UpdateChapterOrderModel model)
        {
            var author = await GetCurrentAuthorAsync();
            if (author == null) return Forbid();

            var chapter = await _chapterService.GetChapterByIdAsync(model.ChapterId);
            if (chapter == null) return NotFound();

            if (!await _bookService.IsAuthorOfBookAsync(chapter.BookId, author.Id)) return Forbid();

            // Получаем все главы и преобразуем в List для манипуляций
            var chapters = await _chapterService.GetChaptersByBookIdAsync(chapter.BookId);
            var orderedChapters = chapters.OrderBy(c => c.Order).ToList();

            var currentChapter = orderedChapters.FirstOrDefault(c => c.Id == model.ChapterId);
            if (currentChapter == null) return NotFound();

            int currentIndex = orderedChapters.IndexOf(currentChapter);
            int newIndex = currentIndex;

            if (model.Direction == "up" && currentIndex > 0)
            {
                newIndex = currentIndex - 1;
            }
            else if (model.Direction == "down" && currentIndex < orderedChapters.Count - 1)
            {
                newIndex = currentIndex + 1;
            }
            else
            {
                return Ok(); // Двигать некуда
            }

            // ИСПОЛЬЗУЕМ ВСТРОЕННЫЕ МЕТОДЫ СПИСКА ДЛЯ ПРАВИЛЬНОГО ПЕРЕМЕЩЕНИЯ
            orderedChapters.RemoveAt(currentIndex);
            orderedChapters.Insert(newIndex, currentChapter);

            // Извлекаем ID в новом правильном порядке
            var chapterIdsInOrder = orderedChapters.Select(c => c.Id).ToList();

            await _chapterService.ReorderChaptersAsync(chapter.BookId, chapterIdsInOrder);

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateChapterStatus([FromBody] UpdateChapterStatusModel model)
        {
            try
            {
                var author = await GetCurrentAuthorAsync();
                if (author == null)
                {
                    return Json(new { success = false, error = "Автор не найден" });
                }

                var chapter = await _chapterService.GetChapterByIdAsync(model.ChapterId);
                if (chapter == null)
                {
                    return Json(new { success = false, error = "Глава не найдена" });
                }

                if (!await _bookService.IsAuthorOfBookAsync(chapter.BookId, author.Id))
                {
                    return Json(new { success = false, error = "Доступ запрещен" });
                }

                // Сохраняем старый статус
                var wasPublic = chapter.IsPublic;

                // Используем ChapterService для обновления статуса
                await _chapterService.UpdateChapterStatusAsync(model.ChapterId, model.IsPublic);

                // Если глава стала публичной (была непубличной или статус изменился на публичный)
                if (model.IsPublic && !wasPublic)
                {
                    // Отправляем уведомления в фоновом режиме
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await SendChapterPublishNotificationsAsync(chapter.BookId, model.ChapterId);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Ошибка при отправке уведомлений: {ex.Message}");
                        }
                    });
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }
        [HttpPost]
        public async Task<IActionResult> UploadChapterImage(IFormFile image, int chapterId, int bookId)
        {
            var author = await GetCurrentAuthorAsync();
            if (author == null) return Forbid();

            if (!await _bookService.IsAuthorOfBookAsync(bookId, author.Id))
                return Forbid();

            try
            {
                var result = await _chapterService.UploadChapterImageAsync(
                    image,
                    chapterId,
                    bookId,
                    _webHostEnvironment.WebRootPath);

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Ошибка загрузки: {ex.Message}" });
            }
        }
        [HttpDelete]
        public async Task<IActionResult> RemoveChapterImage(int pageId)
        {
            var author = await GetCurrentAuthorAsync();
            if (author == null) return Forbid();

            try
            {
                // Получаем страницу для проверки прав доступа
                var page = await _chapterService.GetChapterPagesAsync(pageId);
                if (page == null || !page.Any(p => p.Id == pageId))
                    return NotFound();

                var chapter = await _chapterService.GetChapterByIdAsync(page.First().ChapterId);

                // Проверяем права доступа
                if (!await _bookService.IsAuthorOfBookAsync(chapter.BookId, author.Id))
                    return Forbid();

                var success = await _chapterService.RemoveChapterImageAsync(pageId, _webHostEnvironment.WebRootPath);

                return success ? Ok() : NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Ошибка удаления: {ex.Message}" });
            }
        }
    }
}