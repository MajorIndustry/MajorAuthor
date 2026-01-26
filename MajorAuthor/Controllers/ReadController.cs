using MajorAuthor.Data.Entities;
using MajorAuthor.Models;
using MajorAuthor.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MajorAuthor.Controllers
{
    public class ReadController : Controller
    {
        private readonly IWorkFacade _workFacade;
        private readonly IWorkService<Poem> _poemService;
        private readonly IWorkService<Blog> _blogService;
        private readonly IAuthorService _authorService;
        private readonly IBookService _bookService;
        private readonly IChapterService _chapterService;
        private readonly IPdfExportService _pdfExportService;
        private readonly IPdfCacheService _pdfCacheService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IReadingProgressService _readingProgressService;

        public ReadController(IWorkFacade workFacade, IWorkService<Poem> poemService,
            IWorkService<Blog> blogService, IAuthorService authorService, IBookService bookService, IChapterService chapterService, IPdfExportService pdfExportService, IPdfCacheService pdfCacheService, IWebHostEnvironment webHostEnvironment, IReadingProgressService readingProgressService)
        {
            _workFacade = workFacade;
            _poemService = poemService;
            _blogService = blogService;
            _authorService = authorService;
            _bookService = bookService;
            _chapterService = chapterService;
            _pdfExportService = pdfExportService;
            _pdfCacheService = pdfCacheService;
            _webHostEnvironment = webHostEnvironment;
            _readingProgressService = readingProgressService;
        }

        [HttpGet]
        public async Task<IActionResult> ReadBlog(int id)
        {
            var blog = await _blogService.GetByIdWithCommentsAndLikesAsync(id);
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!string.IsNullOrEmpty(currentUserId))
            {
                var author = await _authorService.GetAuthorByUserIdAsync(currentUserId);
                if (author != null)
                {
                    ViewBag.IsAuthor = blog.AuthorId == author.Id;
                    ViewBag.HasLiked = await _workFacade.HasUserLikedAsync(id, "Блог", currentUserId);

                    // Регистрируем прочтение только если пользователь не автор
                    if (!ViewBag.IsAuthor)
                    {
                        await _workFacade.RegisterReadingAsync(id, "Блог", currentUserId);
                        // Перезагружаем блог для получения актуального счетчика просмотров
                        blog = await _blogService.GetByIdWithCommentsAndLikesAsync(id);
                    }
                }
                else
                {
                    ViewBag.IsAuthor = false;
                    ViewBag.HasLiked = await _workFacade.HasUserLikedAsync(id, "Блог", currentUserId);

                    // Регистрируем прочтение для не-авторов
                    await _workFacade.RegisterReadingAsync(id, "Блог", currentUserId);
                    blog = await _blogService.GetByIdWithCommentsAndLikesAsync(id);
                }
            }
            else
            {
                ViewBag.IsAuthor = false;
                ViewBag.HasLiked = false;

                // Для неавторизованных пользователей не регистрируем прочтение
                // (или можно использовать cookies для отслеживания уникальных просмотров)
            }

            return View(blog);
        }

        [HttpGet]
        public async Task<IActionResult> ReadPoem(int id)
        {
            var poem = await _poemService.GetByIdWithCommentsAndLikesAsync(id);
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!string.IsNullOrEmpty(currentUserId))
            {
                var author = await _authorService.GetAuthorByUserIdAsync(currentUserId);
                if (author != null)
                {
                    ViewBag.IsAuthor = poem.AuthorId == author.Id;
                    ViewBag.HasLiked = await _workFacade.HasUserLikedAsync(id, "Стих", currentUserId);

                    // Регистрируем прочтение только если пользователь не автор
                    if (!ViewBag.IsAuthor)
                    {
                        await _workFacade.RegisterReadingAsync(id, "Стих", currentUserId);
                        // Перезагружаем стих для получения актуального счетчика просмотров
                        poem = await _poemService.GetByIdWithCommentsAndLikesAsync(id);
                    }
                }
                else
                {
                    ViewBag.IsAuthor = false;
                    ViewBag.HasLiked = await _workFacade.HasUserLikedAsync(id, "Стих", currentUserId);

                    // Регистрируем прочтение для не-авторов
                    await _workFacade.RegisterReadingAsync(id, "Стих", currentUserId);
                    poem = await _poemService.GetByIdWithCommentsAndLikesAsync(id);
                }
            }
            else
            {
                ViewBag.IsAuthor = false;
                ViewBag.HasLiked = false;

                // Для неавторизованных пользователей не регистрируем прочтение
            }

            return View(poem);
        }
        [HttpPut]
        public async Task<IActionResult> EditComment(int commentId, string type, string commentText)
        {
            if (string.IsNullOrEmpty(type) || string.IsNullOrEmpty(commentText)) return BadRequest();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var result = await _workFacade.EditCommentAsync(commentId, type, userId, commentText);

            if (result.success)
            {
                return Json(new { success = true, comment = result.updatedComment });
            }
            return BadRequest(new { message = "Ошибка при редактировании комментария." });
        }

        [HttpPost]
        public async Task<IActionResult> LikeWork(int id, string type)
        {
            if (string.IsNullOrEmpty(type)) return BadRequest();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            // Проверяем, является ли пользователь автором работы
            var isAuthor = await _workFacade.IsUserAuthorAsync(id, type, userId);
            if (isAuthor)
            {
                return Json(new { success = false, message = "Нельзя ставить лайк своей работе" });
            }

            var result = await _workFacade.ToggleLikeAsync(id, type, userId);

            return Json(new
            {
                success = result.success,
                likesCount = result.newLikesCount,
                isLiked = result.isLiked
            });
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(int id, string type, string commentText)
        {
            if (string.IsNullOrEmpty(type) || string.IsNullOrEmpty(commentText)) return BadRequest();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var comment = await _workFacade.AddCommentAsync(id, type, userId, commentText);

            if (comment == null)
            {
                return BadRequest();
            }

            return Json(new { success = true, comment = comment });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteComment(int commentId, string type)
        {
            if (string.IsNullOrEmpty(type)) return BadRequest();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var success = await _workFacade.DeleteCommentAsync(commentId, type, userId);

            if (success)
            {
                return Ok(new { message = $" успешно удален." });
            }
            return BadRequest(new { message = "Неизвестный тип произведения." });
        }
        // ReadController.cs - добавим метод ReadBook
        [HttpGet]
        [HttpGet]
        public async Task<IActionResult> ReadBook(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var book = await _bookService.GetByIdAsync(id);

                if (book == null || (!book.IsPublic && !await IsUserAuthorOrCoAuthor(book.Id, userId)))
                    return NotFound();

                // Проверяем, является ли пользователь автором
                bool isAuthor = false;
                if (!string.IsNullOrEmpty(userId))
                {
                    var author = await _authorService.GetAuthorByUserIdAsync(userId);
                    if (author != null)
                    {
                        isAuthor = await _bookService.IsAuthorOfBookAsync(id, author.Id);
                    }
                }

                // УБРАН ВЫЗОВ РЕГИСТРАЦИИ ПРОЧТЕНИЯ КНИГИ ПРИ ЗАХОДЕ НА СТРАНИЦУ
                // Теперь прочтение книги регистрируется только при переходе к главе

                // Получаем ID главы для продолжения чтения
                int? continueReadingChapterId = null;
                var publishedChapters = await _chapterService.GetPublishedChaptersByBookIdAsync(id);
                HashSet<int> readChapterIds = new HashSet<int>();
                bool hasReadingProgress = false;

                if (!string.IsNullOrEmpty(userId))
                {
                    continueReadingChapterId = await _readingProgressService.GetContinueReadingChapterIdAsync(id, userId);
                    readChapterIds = await _readingProgressService.GetReadChapterIdsForBookAsync(id, userId);
                    hasReadingProgress = readChapterIds.Any();
                }

                var model = new BookReadViewModel
                {
                    Book = book,
                    PublishedChapters = publishedChapters,
                    SimilarBooks = await _bookService.GetSimilarBooksAsync(id, 5),
                    Comments = await _bookService.GetBookCommentsAsync(id),
                    HasLiked = !string.IsNullOrEmpty(userId) && await _bookService.HasUserLikedAsync(id, userId),
                    IsInFavorites = !string.IsNullOrEmpty(userId) && await _bookService.IsInFavoritesAsync(id, userId),
                    IsAuthor = isAuthor,
                    Stats = await _bookService.GetBookStatsAsync(id),
                    ContinueReadingChapterId = continueReadingChapterId,
                    ReadChapterIds = readChapterIds,
                    HasReadingProgress = hasReadingProgress
                };

                // Остальной код остается без изменений...
                var authors = await _bookService.GetBookAuthorsAsync(id);
                model.Authors = authors;

                model.Genres = await _bookService.GetBookGenresAsync(id);
                model.Tags = await _bookService.GetBookTagsAsync(id);

                if (book.CycleId.HasValue)
                {
                    model.Cycle = await _bookService.GetBookCycleAsync(book.CycleId);
                    model.CycleBooks = await _bookService.GetCycleBooksAsync(book.CycleId.Value, id);
                }

                return View(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке книги: {ex.Message}");
                return StatusCode(500, "Ошибка при загрузке книги");
            }
        }

        private async Task<bool> IsUserAuthorOrCoAuthor(int bookId, string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return false;

            var author = await _authorService.GetAuthorByUserIdAsync(userId);
            if (author == null)
                return false;

            return await _bookService.IsAuthorOfBookAsync(bookId, author.Id);
        }

        // Методы для работы с лайками и комментариями книги
        [HttpPost]
        public async Task<IActionResult> LikeBook(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // Проверяем, является ли пользователь автором книги
            var author = await _authorService.GetAuthorByUserIdAsync(userId);
            if (author != null)
            {
                var isAuthor = await _bookService.IsAuthorOfBookAsync(id, author.Id);
                if (isAuthor)
                {
                    return Json(new { success = false, message = "Нельзя ставить лайк своей книге" });
                }
            }

            var hasLiked = await _bookService.HasUserLikedAsync(id, userId);

            if (hasLiked)
            {
                await _bookService.RemoveLikeAsync(id, userId);
            }
            else
            {
                await _bookService.AddLikeAsync(id, userId);
            }

            var newLikesCount = await _bookService.GetLikesCountAsync(id);
            var newHasLiked = !hasLiked;

            return Json(new
            {
                success = true,
                likesCount = newLikesCount,
                hasLiked = newHasLiked
            });
        }

        [HttpPost]
        public async Task<IActionResult> AddBookComment(int id, string commentText)
        {
            if (string.IsNullOrEmpty(commentText))
                return BadRequest(new { message = "Комментарий не может быть пустым" });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            var comment = await _bookService.AddBookCommentAsync(id, userId, commentText);

            if (comment == null)
                return BadRequest(new { message = "Ошибка при добавлении комментария" });

            return Json(new { success = true, comment = comment });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleFavorite(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var isInFavorites = await _bookService.IsInFavoritesAsync(id, userId);

            if (isInFavorites)
            {
                await _bookService.RemoveFromFavoritesAsync(id, userId);
            }
            else
            {
                await _bookService.AddToFavoritesAsync(id, userId);
            }

            return Json(new
            {
                success = true,
                isInFavorites = !isInFavorites
            });
        }
        [HttpPost]
        public async Task<IActionResult> DeleteBookComment(int commentId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var success = await _bookService.DeleteBookCommentAsync(commentId, userId);

            if (success)
            {
                return Ok(new { success = true });
            }
            return BadRequest(new { success = false, message = "Ошибка при удалении комментария." });
        }
        [HttpPost]
        public async Task<IActionResult> EditBookComment(int commentId, string commentText)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var comment = await _bookService.GetCommentByIdAsync(commentId);
            if (comment == null || comment.ApplicationUserId != userId)
                return BadRequest(new { success = false, message = "Комментарий не найден или у вас нет прав на редактирование" });

            comment.Content = commentText;

            // Вам нужно добавить метод UpdateCommentAsync в BookService
            var success = await _bookService.UpdateCommentAsync(comment);

            if (success)
            {
                return Json(new
                {
                    success = true,
                    comment = new
                    {
                        id = comment.Id,
                        content = comment.Content,
                        timestamp = comment.CreatedDate
                    }
                });
            }

            return BadRequest(new { success = false, message = "Ошибка при редактировании комментария" });
        }

        [HttpPost]
        public async Task<IActionResult> AddReplyToComment(int commentId, string replyText)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            // Вам нужно добавить метод AddReplyAsync в BookService
            var reply = await _bookService.AddReplyAsync(commentId, userId, replyText);

            if (reply != null)
            {
                return Json(new
                {
                    success = true,
                    reply = new
                    {
                        id = reply.Id,
                        content = reply.Content,
                        userId = reply.ApplicationUserId,
                        userName = reply.ApplicationUser?.UserName,
                        userAvatar = reply.ApplicationUser?.ProfilePictureUrl,
                        timestamp = reply.CreatedDate
                    }
                });
            }

            return BadRequest(new { success = false, message = "Ошибка при добавлении ответа" });
        }
        // В ReadController.cs добавьте методы
        [HttpPost]
        public async Task<IActionResult> EditReply(int replyId, string replyText)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var success = await _bookService.UpdateReplyAsync(replyId, userId, replyText);

            if (success)
            {
                var updatedReply = await _bookService.GetReplyByIdAsync(replyId);
                return Json(new
                {
                    success = true,
                    reply = new
                    {
                        id = updatedReply.Id,
                        content = updatedReply.Content,
                        timestamp = updatedReply.CreatedDate
                    }
                });
            }

            return BadRequest(new { success = false, message = "Ошибка при редактировании ответа" });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteReply(int replyId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var success = await _bookService.DeleteReplyAsync(replyId, userId);

            if (success)
            {
                return Ok(new { success = true });
            }

            return BadRequest(new { success = false, message = "Ошибка при удалении ответа" });
        }
        [HttpGet]
        public async Task<IActionResult> DownloadBook(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var book = await _bookService.GetByIdAsync(id);

                if (book == null || (!book.IsPublic && !await IsUserAuthorOrCoAuthor(book.Id, userId)))
                    return NotFound();

                if (!book.AllowDownload)
                    return Forbid("Скачивание этой книги запрещено автором");

                // Получаем физический путь для доступа к файлам изображений
                var webRootPath = _webHostEnvironment.WebRootPath;

                // Используем кэш с учетом путей к изображениям
                var pdfBytes = await _pdfCacheService.GetOrCreatePdfAsync(id, async () =>
                {
                    // Передаем webRootPath для получения полных путей к изображениям
                    var exportModel = await _bookService.GetBookForExportAsync(id, webRootPath);
                    if (exportModel == null)
                        throw new Exception("Книга не найдена");

                    return await _pdfExportService.GenerateBookPdfAsync(exportModel);
                });

                var fileName = $"{book.Title.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd}.pdf";
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при скачивании книги: {ex.Message}");
                return StatusCode(500, "Произошла ошибка при генерации файла");
            }
        }
        // В ReadController.cs добавьте этот метод

        [HttpGet]
        public async Task<IActionResult> ReadChapter(int bookId, int chapterId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Получаем книгу
                var book = await _bookService.GetByIdAsync(bookId);
                if (book == null || (!book.IsPublic && !await IsUserAuthorOrCoAuthor(book.Id, userId)))
                    return NotFound();

                // Получаем главу
                var chapter = await _chapterService.GetChapterByIdAsync(chapterId);
                if (chapter == null || chapter.BookId != bookId || (!chapter.IsPublic && !await IsUserAuthorOrCoAuthor(book.Id, userId)))
                    return NotFound();

                // Проверяем, является ли пользователь автором
                bool isAuthor = false;
                if (!string.IsNullOrEmpty(userId))
                {
                    var author = await _authorService.GetAuthorByUserIdAsync(userId);
                    if (author != null)
                    {
                        isAuthor = await _bookService.IsAuthorOfBookAsync(bookId, author.Id);
                    }
                }

                // Регистрируем прочтение главы (только для не-авторов и авторизованных пользователей)
                if (!isAuthor && !string.IsNullOrEmpty(userId))
                {
                    await _readingProgressService.RegisterChapterReadAsync(bookId, chapterId, userId);
                    // Регистрируем прочтение книги при чтении первой главы
                    var firstChapter = (await _chapterService.GetPublishedChaptersByBookIdAsync(bookId)).FirstOrDefault();
                    if (firstChapter != null && firstChapter.Id == chapterId)
                    {
                        await _bookService.AddReadingAsync(bookId, userId);
                    }
                }

                // Получаем информацию о страницах главы
                var chapterPages = await _chapterService.GetChapterPagesInfoAsync(chapterId);

                // Получаем все главы книги
                var allChapters = await _chapterService.GetChaptersByBookIdAsync(bookId);

                // Получаем ID всех прочитанных глав этой книги для текущего пользователя
                HashSet<int> readChapterIds = new HashSet<int>();
                if (!string.IsNullOrEmpty(userId))
                {
                    // Используем ReadingProgressService для получения прочитанных глав
                    readChapterIds = await _readingProgressService.GetReadChapterIdsForBookAsync(bookId, userId);
                }

                // Находим предыдущую и следующую главы
                var currentIndex = allChapters.FindIndex(c => c.Id == chapterId);
                var previousChapter = currentIndex > 0 ? allChapters[currentIndex - 1] : null;
                var nextChapter = currentIndex < allChapters.Count - 1 ? allChapters[currentIndex + 1] : null;

                // Проверяем, прочитана ли текущая глава
                var isChapterRead = readChapterIds.Contains(chapterId);

                // Получаем прогресс чтения книги
                var bookProgress = await _readingProgressService.GetBookReadProgressAsync(bookId, userId);

                // Проверяем лайки и избранное
                var hasLiked = !string.IsNullOrEmpty(userId) && await _bookService.HasUserLikedAsync(bookId, userId);
                var isInFavorites = !string.IsNullOrEmpty(userId) && await _bookService.IsInFavoritesAsync(bookId, userId);

                // Получаем настройки чтения из куков
                var readMode = Request.Cookies[$"readMode_{bookId}"] ?? "pages";
                var fontSize = int.TryParse(Request.Cookies[$"fontSize_{bookId}"], out var size) ? size : 16;

                var model = new ChapterReadViewModel
                {
                    Book = book,
                    CurrentChapter = chapter,
                    ChapterPages = chapterPages,
                    AllChapters = allChapters.Where(c => c.IsPublic).ToList(),
                    PreviousChapter = previousChapter?.IsPublic == true ? previousChapter : null,
                    NextChapter = nextChapter?.IsPublic == true ? nextChapter : null,
                    IsAuthor = isAuthor,
                    HasLiked = hasLiked,
                    IsInFavorites = isInFavorites,
                    IsChapterRead = isChapterRead,
                    BookProgress = bookProgress,
                    AllowDownload = book.AllowDownload,
                    EnableTTS = book.EnableTTS,
                    ReadMode = readMode,
                    FontSize = fontSize,
                    ReadChapterIds = readChapterIds // Передаем ID прочитанных глав
                };
                ViewBag.ReadingHeaderModel = model;
                return View(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке главы: {ex.Message}");
                return StatusCode(500, "Ошибка при загрузке главы");
            }
        }

        // Добавьте этот метод для переключения режима чтения
        // Добавьте этот класс модели в ReadController.cs или в отдельный файл
        // Добавьте этот класс в ReadController.cs или создайте отдельный файл модели
        public class UpdateReadingSettingsModel
        {
            public int BookId { get; set; }
            public string ReadMode { get; set; }
            public int FontSize { get; set; }
        }

        [HttpPost]
        public IActionResult UpdateReadingSettings([FromBody] UpdateReadingSettingsModel model)
        {
            if (model == null)
            {
                return Json(new { success = false, message = "Invalid request" });
            }

            // Убираем Secure=true для локальной разработки, можно добавить проверку на окружение
            bool isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

            Response.Cookies.Append($"readMode_{model.BookId}", model.ReadMode, new CookieOptions
            {
                Expires = DateTimeOffset.Now.AddDays(30),
                HttpOnly = true,
                Secure = !isDevelopment, // В разработке false, в продакшене true
                SameSite = SameSiteMode.Lax
            });

            Response.Cookies.Append($"fontSize_{model.BookId}", model.FontSize.ToString(), new CookieOptions
            {
                Expires = DateTimeOffset.Now.AddDays(30),
                HttpOnly = true,
                Secure = !isDevelopment,
                SameSite = SameSiteMode.Lax
            });

            return Json(new { success = true });
        }
        [HttpGet]
        public async Task<IActionResult> GetLastReadChapter(int bookId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Json(new { success = false, message = "Не авторизован" });

                var lastChapterId = await _readingProgressService.GetLastReadChapterIdAsync(bookId, userId);

                if (lastChapterId.HasValue)
                {
                    var chapter = await _chapterService.GetChapterByIdAsync(lastChapterId.Value);
                    if (chapter != null && chapter.IsPublic)
                    {
                        return Json(new
                        {
                            success = true,
                            chapterId = lastChapterId.Value,
                            chapterTitle = chapter.Title,
                            chapterOrder = chapter.Order
                        });
                    }
                }

                // Если последней главы нет, возвращаем первую
                var firstChapter = await _chapterService.GetPublishedChaptersByBookIdAsync(bookId);
                if (firstChapter.Any())
                {
                    return Json(new
                    {
                        success = true,
                        chapterId = firstChapter.First().Id,
                        chapterTitle = firstChapter.First().Title,
                        chapterOrder = firstChapter.First().Order
                    });
                }

                return Json(new { success = false, message = "Нет доступных глав" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении последней прочитанной главы: {ex.Message}");
                return Json(new { success = false, message = "Ошибка сервера" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> MarkChapterUnread([FromBody] MarkChapterUnreadModel model)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Json(new { success = false, message = "Не авторизован" });

                var success = await _readingProgressService.MarkChapterUnreadAsync(model.BookId, model.ChapterId, userId);

                if (success)
                {
                    return Json(new
                    {
                        success = true,
                        bookProgress = await _readingProgressService.GetBookReadProgressAsync(model.BookId, userId)
                    });
                }

                return Json(new { success = false, message = "Не удалось обновить статус главы" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отметке главы как непрочитанной: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveReadingProgress(int bookId, int chapterId, int pageNumber, int totalPages, string readMode = "pages")
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Json(new { success = false, message = "Не авторизован" });

                var success = await _readingProgressService.SaveReadingProgressAsync(
                    bookId, chapterId, pageNumber, totalPages, userId, readMode);

                if (success)
                {
                    return Json(new
                    {
                        success = true,
                        chapterProgress = pageNumber >= totalPages ? 100 : Math.Round((double)pageNumber / totalPages * 100, 1),
                        bookProgress = await _readingProgressService.GetBookReadProgressAsync(bookId, userId)
                    });
                }

                return Json(new { success = false, message = "Не удалось сохранить прогресс" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка сохранения прогресса: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        public class MarkChapterUnreadModel
        {
            public int BookId { get; set; }
            public int ChapterId { get; set; }
        }
    }
}