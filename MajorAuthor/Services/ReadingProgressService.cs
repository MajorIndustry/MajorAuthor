using MajorAuthor.Data;
using MajorAuthor.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MajorAuthor.Services
{
    public class ReadingProgressService : IReadingProgressService
    {
        private readonly MajorAuthorDbContext _context;
        private readonly IBookService _bookService;
        private readonly IChapterService _chapterService;

        public ReadingProgressService(
            MajorAuthorDbContext context,
            IBookService bookService,
            IChapterService chapterService)
        {
            _context = context;
            _bookService = bookService;
            _chapterService = chapterService;
        }

        public async Task<bool> RegisterChapterReadAsync(int bookId, int chapterId, string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                    return false;

                // Проверяем, является ли пользователь автором
                var isAuthor = await _bookService.IsAuthorOfBookAsync(bookId, userId);
                if (isAuthor)
                    return false;

                // Получаем главу
                var chapter = await _chapterService.GetChapterByIdAsync(chapterId);
                if (chapter == null || chapter.BookId != bookId)
                    return false;

                // Проверяем, существует ли уже запись о прочтении
                var existingRead = await _context.ChapterReads
                    .FirstOrDefaultAsync(cr =>
                        cr.ApplicationUserId == userId &&
                        cr.ChapterId == chapterId);

                if (existingRead != null)
                {
                    // Обновляем существующую запись
                    existingRead.LastReadDate = DateTime.UtcNow;
                    existingRead.IsCompleted = true;
                }
                else
                {
                    // Создаем новую запись
                    var chapterRead = new ChapterRead
                    {
                        ApplicationUserId = userId,
                        ChapterId = chapterId,
                        FirstReadDate = DateTime.UtcNow,
                        LastReadDate = DateTime.UtcNow,
                        IsCompleted = true
                    };
                    _context.ChapterReads.Add(chapterRead);

                    // Проверяем, является ли это первой главой книги
                    await CheckAndRegisterBookReading(bookId, chapterId, userId);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при регистрации прочтения главы: {ex.Message}");
                return false;
            }
        }

        private async Task CheckAndRegisterBookReading(int bookId, int chapterId, string userId)
        {
            try
            {
                // Получаем все главы книги
                var chapters = await _chapterService.GetChaptersByBookIdAsync(bookId);
                var publishedChapters = chapters.Where(c => c.IsPublic).ToList();

                if (!publishedChapters.Any())
                    return;

                // Находим первую главу по порядку
                var firstChapter = publishedChapters.OrderBy(c => c.Order).FirstOrDefault();

                // Если это первая глава, регистрируем прочтение книги
                if (firstChapter != null && firstChapter.Id == chapterId)
                {
                    // Проверяем, существует ли уже запись о прочтении книги
                    var existingBookReading = await _context.BookReadings
                        .FirstOrDefaultAsync(br =>
                            br.BookId == bookId &&
                            br.ApplicationUserId == userId);

                    if (existingBookReading == null)
                    {
                        var bookReading = new BookReading
                        {
                            BookId = bookId,
                            ApplicationUserId = userId,
                            ReadDate = DateTime.UtcNow,
                            ReadingProgress = await CalculateInitialProgress(bookId, userId)
                        };
                        _context.BookReadings.Add(bookReading);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при регистрации прочтения книги: {ex.Message}");
            }
        }

        private async Task<int> CalculateInitialProgress(int bookId, string userId)
        {
            try
            {
                // Получаем все опубликованные главы
                var chapters = await _chapterService.GetChaptersByBookIdAsync(bookId);
                var publishedChapters = chapters.Where(c => c.IsPublic).ToList();

                if (!publishedChapters.Any())
                    return 0;

                // Считаем прочитанные главы
                var readChapters = await _context.ChapterReads
                    .Where(cr =>
                        cr.ApplicationUserId == userId &&
                        publishedChapters.Select(c => c.Id).Contains(cr.ChapterId))
                    .CountAsync();

                // Рассчитываем процент
                return (int)Math.Round((double)readChapters / publishedChapters.Count * 100);
            }
            catch
            {
                return 0;
            }
        }

        public async Task<bool> IsChapterReadAsync(int bookId, int chapterId, string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return false;

            return await _context.ChapterReads
                .AnyAsync(cr =>
                    cr.ApplicationUserId == userId &&
                    cr.ChapterId == chapterId);
        }

        public async Task<double> GetChapterReadProgressAsync(int bookId, int chapterId, string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return 0;

            // Для главы просто возвращаем 100%, если она прочитана
            var isRead = await IsChapterReadAsync(bookId, chapterId, userId);
            return isRead ? 100 : 0;
        }

        public async Task<double> GetBookReadProgressAsync(int bookId, string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return 0;

            try
            {
                // Получаем все опубликованные главы
                var chapters = await _chapterService.GetChaptersByBookIdAsync(bookId);
                var publishedChapters = chapters.Where(c => c.IsPublic).ToList();

                if (!publishedChapters.Any())
                    return 0;

                // Считаем прочитанные главы
                var readChapters = await _context.ChapterReads
                    .Where(cr =>
                        cr.ApplicationUserId == userId &&
                        publishedChapters.Select(c => c.Id).Contains(cr.ChapterId))
                    .CountAsync();

                // Рассчитываем процент
                return Math.Round((double)readChapters / publishedChapters.Count * 100, 1);
            }
            catch
            {
                return 0;
            }
        }

        public async Task<int> GetReadChaptersCountAsync(int bookId, string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return 0;

            try
            {
                // Получаем все опубликованные главы
                var chapters = await _chapterService.GetChaptersByBookIdAsync(bookId);
                var publishedChapters = chapters.Where(c => c.IsPublic).ToList();

                if (!publishedChapters.Any())
                    return 0;

                // Считаем прочитанные главы
                return await _context.ChapterReads
                    .Where(cr =>
                        cr.ApplicationUserId == userId &&
                        publishedChapters.Select(c => c.Id).Contains(cr.ChapterId))
                    .CountAsync();
            }
            catch
            {
                return 0;
            }
        }
        public async Task<HashSet<int>> GetReadChapterIdsForBookAsync(int bookId, string userId)
        {
            return await _context.ChapterReads
                .Where(cr => cr.ApplicationUserId == userId && cr.Chapter.BookId == bookId)
                .Select(cr => cr.ChapterId)
                .ToHashSetAsync();
        }
        public async Task<int?> GetLastReadChapterIdAsync(int bookId, string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                    return null;

                // Получаем последнюю прочитанную главу по дате последнего прочтения
                var lastReadChapter = await _context.ChapterReads
                    .Include(cr => cr.Chapter)
                    .Where(cr => cr.ApplicationUserId == userId &&
                                cr.Chapter.BookId == bookId &&
                                cr.Chapter.IsPublic)
                    .OrderByDescending(cr => cr.LastReadDate)
                    .Select(cr => (int?)cr.ChapterId)
                    .FirstOrDefaultAsync();

                return lastReadChapter;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении последней прочитанной главы: {ex.Message}");
                return null;
            }
        }
        public async Task<bool> MarkChapterUnreadAsync(int bookId, int chapterId, string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                    return false;

                // Находим запись о прочтении главы
                var chapterRead = await _context.ChapterReads
                    .FirstOrDefaultAsync(cr => cr.ApplicationUserId == userId &&
                                              cr.ChapterId == chapterId);

                if (chapterRead != null)
                {
                    _context.ChapterReads.Remove(chapterRead);
                    await _context.SaveChangesAsync();
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отметке главы как непрочитанной: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SaveReadingProgressAsync(int bookId, int chapterId, int pageNumber, int totalPages, string userId, string readMode = "pages")
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                    return false;

                // Получаем главу
                var chapter = await _chapterService.GetChapterByIdAsync(chapterId);
                if (chapter == null || chapter.BookId != bookId)
                    return false;

                // Определяем, завершена ли глава
                bool isCompleted = false;

                if (readMode == "pages")
                {
                    // В постраничном режиме - когда дошли до последней страницы
                    isCompleted = pageNumber >= totalPages;
                }
                else if (readMode == "continuous")
                {
                    // В режиме сплошного текста - считаем завершенной сразу
                    // (прогресс отслеживается по скроллу)
                    isCompleted = pageNumber > 0; // Любое взаимодействие считается прочтением
                }

                // Проверяем, существует ли уже запись о прочтении
                var existingRead = await _context.ChapterReads
                    .FirstOrDefaultAsync(cr => cr.ApplicationUserId == userId &&
                                              cr.ChapterId == chapterId);

                if (existingRead != null)
                {
                    // Обновляем существующую запись
                    existingRead.LastReadDate = DateTime.UtcNow;
                    existingRead.IsCompleted = isCompleted;
                }
                else
                {
                    // Создаем новую запись
                    var chapterRead = new ChapterRead
                    {
                        ApplicationUserId = userId,
                        ChapterId = chapterId,
                        FirstReadDate = DateTime.UtcNow,
                        LastReadDate = DateTime.UtcNow,
                        IsCompleted = isCompleted
                    };
                    _context.ChapterReads.Add(chapterRead);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при сохранении прогресса чтения: {ex.Message}");
                return false;
            }
        }

        public async Task<List<ChapterProgressInfo>> GetReadingProgressForBookAsync(int bookId, string userId)
        {
            var result = new List<ChapterProgressInfo>();

            try
            {
                if (string.IsNullOrEmpty(userId))
                    return result;

                // Получаем все опубликованные главы книги
                var chapters = await _chapterService.GetPublishedChaptersByBookIdAsync(bookId);
                if (!chapters.Any())
                    return result;

                // Получаем прочитанные главы
                var readChapters = await _context.ChapterReads
                    .Where(cr => cr.ApplicationUserId == userId &&
                                chapters.Select(c => c.Id).Contains(cr.ChapterId))
                    .ToDictionaryAsync(cr => cr.ChapterId, cr => cr);

                // Формируем информацию о прогрессе для каждой главы
                foreach (var chapter in chapters.OrderBy(c => c.Order))
                {
                    readChapters.TryGetValue(chapter.Id, out var chapterRead);

                    result.Add(new ChapterProgressInfo
                    {
                        ChapterId = chapter.Id,
                        ChapterTitle = chapter.Title,
                        ChapterOrder = chapter.Order,
                        IsCompleted = chapterRead?.IsCompleted ?? false,
                        LastReadDate = chapterRead?.LastReadDate ?? DateTime.MinValue,
                        ProgressPercentage = chapterRead?.IsCompleted == true ? 100 : 0
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении прогресса чтения: {ex.Message}");
                return result;
            }
        }

        // Дополнительный метод для проверки прочитанной главы без bookId (если нужен)
        public async Task<bool> IsChapterReadAsync(int chapterId, string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                    return false;

                return await _context.ChapterReads
                    .AnyAsync(cr => cr.ApplicationUserId == userId &&
                                   cr.ChapterId == chapterId &&
                                   cr.IsCompleted);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при проверке прочитанной главы: {ex.Message}");
                return false;
            }
        }
        public async Task<int?> GetContinueReadingChapterIdAsync(int bookId, string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                    return null;

                // Получаем все опубликованные главы книги
                var publishedChapters = await _context.Chapters
                    .Where(c => c.BookId == bookId && c.IsPublic)
                    .OrderBy(c => c.Order)
                    .ToListAsync();

                if (!publishedChapters.Any())
                    return null;

                // Получаем все записи о прочтении глав для этой книги
                var chapterReads = await _context.ChapterReads
                    .Where(cr => cr.ApplicationUserId == userId &&
                                cr.Chapter.BookId == bookId &&
                                cr.Chapter.IsPublic)
                    .Include(cr => cr.Chapter)
                    .ToListAsync();

                if (!chapterReads.Any())
                {
                    // Если нет записей о прочтении, начинаем с первой главы
                    return publishedChapters.First().Id;
                }

                // Проверяем, все ли главы прочитаны
                var readChapterIds = chapterReads.Where(cr => cr.IsCompleted).Select(cr => cr.ChapterId).ToHashSet();
                bool allChaptersRead = publishedChapters.All(c => readChapterIds.Contains(c.Id));

                if (allChaptersRead)
                {
                    // Если все главы прочитаны, предлагаем читать с последней
                    return publishedChapters.Last().Id;
                }

                // Находим последнюю прочитанную главу, которая не завершена
                var lastUncompletedChapter = chapterReads
                    .Where(cr => !cr.IsCompleted)
                    .OrderByDescending(cr => cr.LastReadDate)
                    .FirstOrDefault();

                if (lastUncompletedChapter != null)
                {
                    return lastUncompletedChapter.ChapterId;
                }

                // Если все начатые главы завершены, ищем последнюю прочитанную главу
                var lastCompletedChapter = chapterReads
                    .Where(cr => cr.IsCompleted)
                    .OrderByDescending(cr => cr.LastReadDate)
                    .FirstOrDefault();

                if (lastCompletedChapter != null)
                {
                    var currentChapterIndex = publishedChapters
                        .FindIndex(c => c.Id == lastCompletedChapter.ChapterId);

                    if (currentChapterIndex < publishedChapters.Count - 1)
                    {
                        // Есть следующая глава
                        return publishedChapters[currentChapterIndex + 1].Id;
                    }
                    else
                    {
                        // Это последняя глава, и она прочитана, но есть непрочитанные ранее главы
                        // Ищем первую непрочитанную главу
                        var firstUnreadChapter = publishedChapters
                            .FirstOrDefault(c => !readChapterIds.Contains(c.Id));

                        return firstUnreadChapter?.Id ?? publishedChapters.Last().Id;
                    }
                }

                // Если нет записей о прочтении, начинаем с первой главы
                return publishedChapters.First().Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при определении главы для продолжения чтения: {ex.Message}");
                return null;
            }
        }
    }
}