using MajorAuthor.Data;
using MajorAuthor.Data.Entities;
using MajorAuthor.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MajorAuthor.Services
{
    public class ChapterService : IChapterService
    {
        private readonly MajorAuthorDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        private const int MAX_PAGE_LENGTH = 4000;

        public ChapterService(
            MajorAuthorDbContext context,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // --- Вспомогательные методы ---
        private static List<string> SplitTextByLength(string text, int maxLength)
        {
            var pages = new List<string>();
            if (string.IsNullOrEmpty(text))
            {
                pages.Add(string.Empty);
                return pages;
            }

            for (int i = 0; i < text.Length; i += maxLength)
            {
                int length = Math.Min(maxLength, text.Length - i);
                pages.Add(text.Substring(i, length));
            }

            return pages;
        }

        private string RemoveDangerousHtml(string html)
        {
            var dangerousPatterns = new[]
            {
                @"<script[^>]*>.*?</script>",
                @"<style[^>]*>.*?</style>",
                @"<iframe[^>]*>.*?</iframe>",
                @"<object[^>]*>.*?</object>",
                @"<embed[^>]*>.*?</embed>",
                @"<applet[^>]*>.*?</applet>",
                @"<form[^>]*>.*?</form>",
                @"on\w+=""[^""]*""",
                @"on\w+='[^']*'",
                @"javascript:"
            };

            foreach (var pattern in dangerousPatterns)
            {
                html = Regex.Replace(html, pattern, "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            }

            return html;
        }

        private async Task RenumberPagesInternalAsync(int chapterId)
        {
            var pages = await _context.Pages
                .Where(p => p.ChapterId == chapterId)
                .OrderBy(p => p.PageNumber)
                .ToListAsync();

            for (int i = 0; i < pages.Count; i++)
            {
                pages[i].PageNumber = i + 1;
            }

            await _context.SaveChangesAsync();
        }
        private async Task UpdateBookLastUpdateTime(int bookId)
        {
            var book = await _context.Books.FindAsync(bookId);
            if (book != null)
            {
                book.LastUpdateTime = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
        // --- Основные методы ---
        public async Task<bool> SaveChapterSnapshotAsync(ChapterEditorViewModel model, string userId)
        {
            try
            {
                if (model.ChapterId == null)
                {
                    Console.WriteLine("Ошибка сохранения главы: ChapterId не предоставлен.");
                    return false;
                }

                var chapter = await GetChapterByIdAsync(model.ChapterId.Value);
                if (chapter == null)
                {
                    return false;
                }

                if (!chapter.Book.BookAuthors.Any(ba => ba.Author.ApplicationUserId == userId))
                {
                    return false;
                }

                // 1. Обновляем заголовок главы
                chapter.Title = model.ChapterTitle ?? "Без названия";

                // ВАЖНОЕ ИСПРАВЛЕНИЕ: Обновляем IsPublic только если передано true
                // Не сбрасываем публикацию при обычном сохранении
                if (model.IsPublic)
                {
                    chapter.IsPublic = true;
                    chapter.PublicationDate = DateTime.UtcNow;
                }
                // Если передано false - оставляем текущее значение (не сбрасываем)

                // 2. Обработка текстового содержимого
                string cleanContent = model.Content ?? string.Empty;

                // Удаляем HTML теги и декодируем сущности
                cleanContent = Regex.Replace(cleanContent, @"<[^>]*>", string.Empty);
                cleanContent = System.Net.WebUtility.HtmlDecode(cleanContent);
                cleanContent = cleanContent.Trim();

                // 3. Разбиваем текст на страницы по 4000 символов
                var textPages = SplitTextByLength(cleanContent, MAX_PAGE_LENGTH);

                // 4. Получаем существующие страницы
                var existingPages = chapter.Pages.OrderBy(p => p.PageNumber).ToList();

                // 5. Разделяем текстовые страницы и страницы с изображениями
                var textPageEntities = existingPages.Where(p => string.IsNullOrEmpty(p.ImageUrl)).ToList();
                var imagePageEntities = existingPages.Where(p => !string.IsNullOrEmpty(p.ImageUrl)).ToList();

                // 6. Обновляем текстовые страницы
                for (int i = 0; i < textPages.Count; i++)
                {
                    if (i < textPageEntities.Count)
                    {
                        // Обновляем существующую текстовую страницу
                        textPageEntities[i].PageNumber = i + 1;
                        textPageEntities[i].TextContent = textPages[i];
                        textPageEntities[i].ImageUrl = null;
                    }
                    else
                    {
                        // Создаем новую текстовую страницу
                        chapter.Pages.Add(new Page
                        {
                            PageNumber = i + 1,
                            TextContent = textPages[i],
                            ImageUrl = null
                        });
                    }
                }

                // 7. Удаляем лишние текстовые страницы
                if (textPages.Count < textPageEntities.Count)
                {
                    var pagesToRemove = textPageEntities.Skip(textPages.Count).ToList();
                    foreach (var page in pagesToRemove)
                    {
                        chapter.Pages.Remove(page);
                        _context.Pages.Remove(page);
                    }
                }

                // 8. Обновляем номера страниц для изображений (они идут после текста)
                int startImagePageNumber = textPages.Count + 1;
                foreach (var imagePage in imagePageEntities.OrderBy(p => p.PageNumber))
                {
                    imagePage.PageNumber = startImagePageNumber++;
                }

                await UpdateChapterAsync(chapter);

                // Обновляем время последнего изменения книги
                await UpdateBookLastUpdateTime(chapter.BookId);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка сохранения главы в сервисе: {ex.Message}");
                return false;
            }
        }

        public async Task<Chapter> GetChapterByIdAsync(int chapterId)
        {
            return await _context.Chapters
                .Include(c => c.Book)
                .ThenInclude(b => b.BookAuthors)
                .ThenInclude(ba => ba.Author)
                .Include(c => c.Pages)
                .FirstOrDefaultAsync(c => c.Id == chapterId);
        }

        public async Task AddChapterAsync(Chapter chapter)
        {
            var maxOrder = await _context.Chapters
                .Where(c => c.BookId == chapter.BookId)
                .MaxAsync(c => (int?)c.Order) ?? 0;

            chapter.Order = maxOrder + 1;
            _context.Chapters.Add(chapter);
            await _context.SaveChangesAsync();

            await UpdateBookLastUpdateTime(chapter.BookId);
        }

        public async Task UpdateChapterAsync(Chapter chapter)
        {
            _context.Entry(chapter).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            await UpdateBookLastUpdateTime(chapter.BookId);
        }
        private async Task UpdateBookPublicationStatusAsync(int bookId)
        {
            var book = await _context.Books
                .Include(b => b.Chapters)
                .FirstOrDefaultAsync(b => b.Id == bookId);

            if (book != null)
            {
                bool hasPublicChapters = book.Chapters.Any(c => c.IsPublic);
                book.IsPublic = hasPublicChapters;
                await _context.SaveChangesAsync();
            }
        }
        public async Task UpdateChapterStatusAsync(int chapterId, bool isPublic)
        {
            var chapter = await _context.Chapters.FindAsync(chapterId);
            if (chapter != null)
            {
                chapter.IsPublic = isPublic;
                if (isPublic)
                {
                    chapter.PublicationDate = DateTime.UtcNow;
                }
                await _context.SaveChangesAsync();

                await UpdateBookPublicationStatusAsync(chapter.BookId);
            }
        }

        public async Task DeleteChapterAsync(int chapterId)
        {
            var chapter = await _context.Chapters
                .Include(c => c.Pages)
                .FirstOrDefaultAsync(c => c.Id == chapterId);

            if (chapter != null)
            {
                // Удаляем физические файлы изображений
                var imagePages = chapter.Pages.Where(p => !string.IsNullOrEmpty(p.ImageUrl)).ToList();
                foreach (var page in imagePages)
                {
                    await RemovePageImageAsync(page);
                }

                _context.Pages.RemoveRange(chapter.Pages);
                _context.Chapters.Remove(chapter);
                await _context.SaveChangesAsync();

                await RenumberChaptersAsync(chapter.BookId);
                await UpdateBookLastUpdateTime(chapter.BookId);
            }
        }

        private async Task RemovePageImageAsync(Page page)
        {
            if (!string.IsNullOrEmpty(page.ImageUrl))
            {
                string filePath = Path.Combine(_webHostEnvironment.WebRootPath,
                                             page.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
        }

        public async Task ReorderChaptersAsync(int bookId, List<int> chapterIdsInOrder)
        {
            var chapters = await _context.Chapters
                .Where(c => c.BookId == bookId)
                .ToListAsync();

            for (int i = 0; i < chapterIdsInOrder.Count; i++)
            {
                var chapter = chapters.FirstOrDefault(c => c.Id == chapterIdsInOrder[i]);
                if (chapter != null)
                {
                    chapter.Order = i + 1;
                }
            }

            await _context.SaveChangesAsync();
        }

        private async Task RenumberChaptersAsync(int bookId)
        {
            var chapters = await _context.Chapters
                .Where(c => c.BookId == bookId)
                .OrderBy(c => c.Order)
                .ToListAsync();

            for (int i = 0; i < chapters.Count; i++)
            {
                chapters[i].Order = i + 1;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<Chapter>> GetChaptersByBookIdAsync(int bookId)
        {
            return await _context.Chapters
                .Where(c => c.BookId == bookId)
                .Include(c => c.Pages)
                .OrderBy(c => c.Order)
                .ToListAsync();
        }

        // --- Методы для работы с изображениями ---
        public async Task<ImageUploadResult> UploadChapterImageAsync(
    IFormFile image,
    int chapterId,
    int bookId,
    string webRootPath)
        {
            if (image == null || image.Length == 0)
                throw new ArgumentException("Файл не выбран");

            // Проверяем существование главы
            var chapter = await GetChapterByIdAsync(chapterId);
            if (chapter == null || chapter.BookId != bookId)
                throw new ArgumentException("Глава не найдена или не принадлежит книге");

            // Определяем следующий номер страницы
            var maxPageNumber = await _context.Pages
                .Where(p => p.ChapterId == chapterId)
                .MaxAsync(p => (int?)p.PageNumber) ?? 0;

            int newPageNumber = maxPageNumber + 1;

            // Генерируем уникальное имя файла
            string uniqueFileName = $"{Guid.NewGuid()}.webp";
            string uploadFolder = Path.Combine(webRootPath, "images", "chapters", chapterId.ToString());

            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            string filePath = Path.Combine(uploadFolder, uniqueFileName);

            // Конвертируем и сохраняем изображение в формате WebP
            using (var image1 = await Image.LoadAsync(image.OpenReadStream()))
            {
                // Оптимизируем изображение (можно настроить качество)
                await image1.SaveAsWebpAsync(filePath, new WebpEncoder
                {
                    Quality = 80, // Качество от 1 до 100
                    Method = WebpEncodingMethod.BestQuality // Можно изменить на BestSpeed для быстрейшей обработки
                });
            }

            // Создаем новую страницу с изображением
            var page = new Page
            {
                ChapterId = chapterId,
                PageNumber = newPageNumber,
                ImageUrl = $"/images/chapters/{chapterId}/{uniqueFileName}",
                TextContent = ""
            };

            _context.Pages.Add(page);
            await _context.SaveChangesAsync();

            return new ImageUploadResult
            {
                Id = page.Id,
                Url = page.ImageUrl
            };
        }

        public async Task<bool> RemoveChapterImageAsync(int pageId, string webRootPath)
        {
            // Находим страницу по ID
            var page = await _context.Pages.FindAsync(pageId);
            if (page == null)
                return false;

            // Удаляем физический файл
            if (!string.IsNullOrEmpty(page.ImageUrl))
            {
                string filePath = Path.Combine(webRootPath, page.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            // Удаляем страницу из БД
            _context.Pages.Remove(page);
            await _context.SaveChangesAsync();

            // Перенумеровываем оставшиеся страницы
            await RenumberPagesInternalAsync(page.ChapterId);

            return true;
        }

        public async Task<List<Page>> GetChapterPagesAsync(int pageId)
        {
            return await _context.Pages
                .Where(p => p.Id == pageId)
                .ToListAsync();
        }
        // В ChapterService.cs добавьте:

        public async Task<List<Chapter>> GetPublishedChaptersByBookIdAsync(int bookId)
        {
            return await _context.Chapters
                .Where(c => c.BookId == bookId && c.IsPublic)
                .Include(c => c.Pages)
                .OrderBy(c => c.Order)
                .ToListAsync();
        }
        public async Task RenumberChapterPagesAsync(int chapterId)
        {
            await RenumberPagesInternalAsync(chapterId);
        }
        // Добавьте этот метод в класс ChapterService
        public async Task<string> GetChapterCombinedTextAsync(int chapterId)
        {
            try
            {
                // Получаем все текстовые страницы главы (без изображений)
                var textPages = await _context.Pages
                    .Where(p => p.ChapterId == chapterId && string.IsNullOrEmpty(p.ImageUrl))
                    .OrderBy(p => p.PageNumber)
                    .Select(p => p.TextContent ?? "")
                    .ToListAsync();

                // Объединяем все текстовые страницы
                return string.Join("", textPages);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении объединенного текста главы: {ex.Message}");
                return string.Empty;
            }
        }
        public async Task<ChapterPagesInfo> GetChapterPagesInfoAsync(int chapterId)
        {
            var pages = await GetAllPagesInOrderAsync(chapterId);

            // Собираем текстовые страницы
            var textContent = new StringBuilder();
            var imagePages = new List<ChapterImagePage>();

            foreach (var page in pages)
            {
                if (string.IsNullOrEmpty(page.ImageUrl))
                {
                    textContent.Append(page.TextContent ?? "");
                }
                else
                {
                    imagePages.Add(new ChapterImagePage
                    {
                        Id = page.Id,
                        PageNumber = page.PageNumber,
                        ImageUrl = page.ImageUrl
                    });
                }
            }

            return new ChapterPagesInfo
            {
                TextContent = textContent.ToString(),
                ImagePages = imagePages
            };
        }
        public async Task<List<Page>> GetAllPagesInOrderAsync(int chapterId)
        {
            return await _context.Pages
                .Where(p => p.ChapterId == chapterId)
                .OrderBy(p => p.PageNumber)
                .Select(p => new Page
                {
                    Id = p.Id,
                    ChapterId = p.ChapterId,
                    PageNumber = p.PageNumber,
                    TextContent = p.TextContent,
                    ImageUrl = p.ImageUrl
                })
                .ToListAsync();
        }


    }
}