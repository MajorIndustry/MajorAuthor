using MajorAuthor.Data;
using MajorAuthor.Data.Entities;
using MajorAuthor.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace MajorAuthor.Services
{
    public class BookService : IBookService
    {
        private readonly MajorAuthorDbContext _context;
        private readonly IChapterService _chapterService;
        private readonly ISimilarBookCalculatorService _similarBookCalculator;
        private readonly ILogger<BookService> _logger;
        private readonly IRatingManagerService _ratingManagerService;

        public BookService(MajorAuthorDbContext context, ISimilarBookCalculatorService similarBookCalculator,
        ILogger<BookService> logger, IChapterService chapterService, IRatingManagerService ratingManagerService)
        {
            _context = context;
            _similarBookCalculator = similarBookCalculator;
            _logger = logger;
            _chapterService = chapterService;
            _ratingManagerService = ratingManagerService;
        }

        // Основные CRUD операции
        public async Task<Book> GetByIdAsync(int id)
        {
            return await _context.Books
                .Include(b => b.BookAuthors)
                    .ThenInclude(ba => ba.Author)
                .Include(b => b.Chapters)
                .Include(b => b.BookTags)
                    .ThenInclude(bt => bt.Tag)
                .Include(b => b.BookGenres)
                    .ThenInclude(bg => bg.Genre)
                .Include(b => b.Status)
                .Include(b => b.Cycle)
                .Include(b => b.Type)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<List<Book>> GetAllByAuthorIdAsync(int authorId)
        {
            return await _context.BookAuthors
                .Where(ba => ba.AuthorId == authorId)
                .Include(ba => ba.Book)
                    .ThenInclude(b => b.Chapters)
                .Include(ba => ba.Book)
                    .ThenInclude(b => b.BookAuthors)
                        .ThenInclude(ba => ba.Author)
                .Select(ba => ba.Book)
                .ToListAsync();
        }

        public async Task<List<Book>> GetAllAsync()
        {
            return await _context.Books
                .Include(b => b.BookAuthors)
                    .ThenInclude(ba => ba.Author)
                .Include(b => b.Chapters)
                .Where(b => b.IsPublic)
                .ToListAsync();
        }

        public async Task<List<Book>> GetAsync(Expression<Func<Book, bool>> predicate)
        {
            return await _context.Books
                .Where(predicate)
                .Include(b => b.BookAuthors)
                    .ThenInclude(ba => ba.Author)
                .Include(b => b.Chapters)
                .ToListAsync();
        }

        public async Task AddAsync(Book book)
        {
            book.IsPublic = false;
            book.PublicationDate = DateTime.UtcNow;
            book.LastUpdateTime = DateTime.UtcNow;

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            // Запускаем расчет похожих книг в фоне
            _ = Task.Run(async () =>
            {
                try
                {
                    await _similarBookCalculator.CalculateSimilaritiesForBookAsync(book.Id);

                    // Также пересчитываем для книг, которые могут быть похожи на новую
                    var similarToCurrent = await GetSimilarBooksAsync(book.Id, 20);
                    foreach (var similarBook in similarToCurrent)
                    {
                        await _similarBookCalculator.CalculateSimilaritiesForBookAsync(similarBook.Id);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Ошибка при расчете похожих книг для новой книги {book.Id}");
                }
            });
        }

        public async Task UpdateAsync(Book book)
        {
            book.LastUpdateTime = DateTime.UtcNow;
            _context.Entry(book).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            await UpdateBookPublicationStatusAsync(book.Id);
        }

        public async Task DeleteAsync(Book book)
        {
            // Удаляем связанные данные
            var chapters = await _context.Chapters.Where(c => c.BookId == book.Id).ToListAsync();
            var bookAuthors = await _context.BookAuthors.Where(ba => ba.BookId == book.Id).ToListAsync();
            var bookTags = await _context.BookTags.Where(bt => bt.BookId == book.Id).ToListAsync();
            var bookGenres = await _context.BookGenres.Where(bg => bg.BookId == book.Id).ToListAsync();
            var likes = await _context.BookLikes.Where(bl => bl.BookId == book.Id).ToListAsync();
            var readings = await _context.BookReadings.Where(br => br.BookId == book.Id).ToListAsync();
            var favorites = await _context.UserFavoriteBooks.Where(ufb => ufb.BookId == book.Id).ToListAsync();
            var invitations = await _context.BookInvitations.Where(bi => bi.BookId == book.Id).ToListAsync();

            _context.Chapters.RemoveRange(chapters);
            _context.BookAuthors.RemoveRange(bookAuthors);
            _context.BookTags.RemoveRange(bookTags);
            _context.BookGenres.RemoveRange(bookGenres);
            _context.BookLikes.RemoveRange(likes);
            _context.BookReadings.RemoveRange(readings);
            _context.UserFavoriteBooks.RemoveRange(favorites);
            _context.BookInvitations.RemoveRange(invitations);

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
        }

        // Авторы и связи
        public async Task LinkBookToAuthorAsync(int bookId, int authorId)
        {
            var existingLink = await _context.BookAuthors
                .FirstOrDefaultAsync(ba => ba.BookId == bookId && ba.AuthorId == authorId);

            if (existingLink == null)
            {
                var bookAuthor = new BookAuthor
                {
                    BookId = bookId,
                    AuthorId = authorId
                };
                _context.BookAuthors.Add(bookAuthor);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> IsAuthorOfBookAsync(int bookId, int authorId)
        {
            return await _context.BookAuthors
                .AnyAsync(ba => ba.BookId == bookId && ba.AuthorId == authorId);
        }
        public async Task<bool> IsAuthorOfBookAsync(int bookId, string userId)
        {
            return await _context.BookAuthors.Include(b=>b.Author)
                .AnyAsync(ba => ba.BookId == bookId && ba.Author.ApplicationUserId == userId);
        }

        public async Task<List<Author>> GetBookAuthorsAsync(int bookId)
        {
            return await _context.BookAuthors
                .Where(ba => ba.BookId == bookId)
                .Include(ba => ba.Author)
                .Select(ba => ba.Author)
                .ToListAsync();
        }

        // Главы
        

        

        // Статистика и аналитика
        public async Task<BookStats> GetBookStatsAsync(int bookId)
        {
            var book = await _context.Books
                .Include(b => b.Chapters)
                    .ThenInclude(c => c.Pages)
                .Include(b => b.Likes)
                .Include(b => b.Readings)
                .Include(b => b.Comments)
                .FirstOrDefaultAsync(b => b.Id == bookId);

            if (book == null)
                return new BookStats();

            var totalCharacters = book.Chapters
                .SelectMany(c => c.Pages)
                .Sum(p => p.TextContent?.Length ?? 0);

            return new BookStats
            {
                TotalCharacters = totalCharacters,
                TotalPages = (int)Math.Ceiling(totalCharacters / 40000.0), // ~40k chars per author sheet
                PublishedChapters = book.Chapters.Count(c => c.IsPublic),
                TotalChapters = book.Chapters.Count,
                LikesCount = book.Likes.Count,
                ReadsCount = book.Readings.Count,
                CommentsCount = book.Comments.Count,
                LastUpdate = book.LastUpdateTime
            };
        }

        public async Task UpdateBookPublicationStatusAsync(int bookId)
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

        public async Task<int> GetTotalCharactersAsync(int bookId)
        {
            var chapters = await _context.Chapters
                .Where(c => c.BookId == bookId)
                .Include(c => c.Pages)
                .ToListAsync();

            return chapters
                .SelectMany(c => c.Pages)
                .Sum(p => p.TextContent?.Length ?? 0);
        }

        public async Task<int> GetTotalPagesAsync(int bookId)
        {
            var totalCharacters = await GetTotalCharactersAsync(bookId);
            return (int)Math.Ceiling(totalCharacters / 40000.0);
        }

        // Теги
        public async Task<List<string>> GetBookTagsStringsAsync(int bookId)
        {
            return await _context.BookTags
                .Where(bt => bt.BookId == bookId)
                .Include(bt => bt.Tag)
                .Select(bt => bt.Tag.Name)
                .ToListAsync();
        }
        public async Task<List<Tag>> GetBookTagsAsync(int bookId)
        {
            return await _context.BookTags
                .Where(bt => bt.BookId == bookId)
                .Include(bt => bt.Tag)
                .Select(bt => bt.Tag)
                .ToListAsync();
        }

        public async Task AddTagToBookAsync(int bookId, string tagName)
        {
            // Находим или создаем тег
            var tag = await _context.Tags
                .FirstOrDefaultAsync(t => t.Name.ToLower() == tagName.ToLower());

            if (tag == null)
            {
                tag = new Tag { Name = tagName };
                _context.Tags.Add(tag);
                await _context.SaveChangesAsync();
            }

            // Проверяем, не добавлен ли уже тег к книге
            var existingBookTag = await _context.BookTags
                .FirstOrDefaultAsync(bt => bt.BookId == bookId && bt.TagId == tag.Id);

            if (existingBookTag == null)
            {
                var bookTag = new BookTag
                {
                    BookId = bookId,
                    TagId = tag.Id
                };
                _context.BookTags.Add(bookTag);
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveTagFromBookAsync(int bookId, string tagName)
        {
            var bookTag = await _context.BookTags
                .Include(bt => bt.Tag)
                .FirstOrDefaultAsync(bt => bt.BookId == bookId && bt.Tag.Name.ToLower() == tagName.ToLower());

            if (bookTag != null)
            {
                _context.BookTags.Remove(bookTag);
                await _context.SaveChangesAsync();

                // Удаляем тег, если он больше не используется
                await CleanupUnusedTagsAsync();
            }
        }

        public async Task<bool> TagExistsAsync(string tagName)
        {
            return await _context.Tags
                .AnyAsync(t => t.Name.ToLower() == tagName.ToLower());
        }

        // Жанры
        public async Task<List<Genre>> GetBookGenresAsync(int bookId)
        {
            return await _context.BookGenres
                .Where(bg => bg.BookId == bookId)
                .Include(bg => bg.Genre)
                .Select(bg => bg.Genre)
                .ToListAsync();
        }

        public async Task AddGenreToBookAsync(int bookId, int genreId)
        {
            var existingBookGenre = await _context.BookGenres
                .FirstOrDefaultAsync(bg => bg.BookId == bookId && bg.GenreId == genreId);

            if (existingBookGenre == null)
            {
                var bookGenre = new BookGenre
                {
                    BookId = bookId,
                    GenreId = genreId
                };
                _context.BookGenres.Add(bookGenre);
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveGenreFromBookAsync(int bookId, int genreId)
        {
            var bookGenre = await _context.BookGenres
                .FirstOrDefaultAsync(bg => bg.BookId == bookId && bg.GenreId == genreId);

            if (bookGenre != null)
            {
                _context.BookGenres.Remove(bookGenre);
                await _context.SaveChangesAsync();
            }
        }

        // Поиск и фильтрация
        public async Task<List<Book>> SearchBooksAsync(string query, int? authorId = null)
        {
            var booksQuery = _context.Books
                .Include(b => b.BookAuthors)
                    .ThenInclude(ba => ba.Author)
                .Include(b => b.Chapters)
                .Where(b => b.IsPublic);

            if (!string.IsNullOrEmpty(query))
            {
                booksQuery = booksQuery.Where(b =>
                    b.Title.Contains(query) ||
                    b.Description.Contains(query) ||
                    b.BookAuthors.Any(ba => ba.Author.PenName.Contains(query)));
            }

            if (authorId.HasValue)
            {
                booksQuery = booksQuery.Where(b => b.BookAuthors.Any(ba => ba.AuthorId == authorId.Value));
            }

            return await booksQuery.ToListAsync();
        }

        public async Task<List<Book>> GetBooksByGenreAsync(int genreId)
        {
            return await _context.BookGenres
                .Where(bg => bg.GenreId == genreId)
                .Include(bg => bg.Book)
                    .ThenInclude(b => b.BookAuthors)
                        .ThenInclude(ba => ba.Author)
                .Where(bg => bg.Book.IsPublic)
                .Select(bg => bg.Book)
                .ToListAsync();
        }

        public async Task<List<Book>> GetPopularBooksAsync(int count = 10)
        {
            return await _context.Books
                .Include(b => b.BookAuthors)
                    .ThenInclude(ba => ba.Author)
                .Where(b => b.IsPublic)
                .OrderByDescending(b => b.LikesCount)
                .ThenByDescending(b => b.ReadsCount)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<Book>> GetRecentlyUpdatedBooksAsync(int count = 10)
        {
            return await _context.Books
                .Include(b => b.BookAuthors)
                    .ThenInclude(ba => ba.Author)
                .Where(b => b.IsPublic)
                .OrderByDescending(b => b.LastUpdateTime)
                .Take(count)
                .ToListAsync();
        }

        // Лайки и чтения
        public async Task<Book> GetByIdWithCommentsAndLikesAsync(int id)
        {
            return await _context.Books
                .Include(b => b.Comments)
                .Include(b => b.Likes)
                .Include(b => b.BookAuthors)
                    .ThenInclude(ba => ba.Author)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<bool> AddLikeAsync(int bookId, string userId)
        {
            var existingLike = await _context.BookLikes
                .FirstOrDefaultAsync(bl => bl.BookId == bookId && bl.ApplicationUserId == userId);

            if (existingLike == null)
            {
                var like = new BookLike
                {
                    BookId = bookId,
                    ApplicationUserId = userId,
                    LikeDate = DateTime.UtcNow
                };
                _context.BookLikes.Add(like);

                // Обновляем счетчик лайков
                var book = await _context.Books.FindAsync(bookId);
                if (book != null)
                {
                    book.LikesCount++;
                    await _context.SaveChangesAsync(); // СОХРАНЕНИЕ ДО обновления рейтинга

                    // Теперь обновляем рейтинг
                    await _ratingManagerService.UpdateBookRatingAsync(bookId);

                    // Обновляем рейтинги всех авторов книги
                    var authorIds = await _context.BookAuthors
                        .Where(ba => ba.BookId == bookId)
                        .Select(ba => ba.AuthorId)
                        .ToListAsync();

                    foreach (var authorId in authorIds)
                    {
                        await _ratingManagerService.UpdateAuthorRatingAsync(authorId);
                    }
                    return true;
                }
            }

            return false;
        }

        public async Task<bool> RemoveLikeAsync(int bookId, string userId)
        {
            var like = await _context.BookLikes
                .FirstOrDefaultAsync(bl => bl.BookId == bookId && bl.ApplicationUserId == userId);

            if (like != null)
            {
                _context.BookLikes.Remove(like);

                // Обновляем счетчик лайков
                var book = await _context.Books.FindAsync(bookId);
                if (book != null && book.LikesCount > 0)
                {
                    book.LikesCount--;
                    await _context.SaveChangesAsync(); // СОХРАНЕНИЕ ДО обновления рейтинга

                    // Обновляем рейтинг книги
                    await _ratingManagerService.UpdateBookRatingAsync(bookId);

                    // Обновляем рейтинги всех авторов книги
                    var authorIds = await _context.BookAuthors
                        .Where(ba => ba.BookId == bookId)
                        .Select(ba => ba.AuthorId)
                        .ToListAsync();

                    foreach (var authorId in authorIds)
                    {
                        await _ratingManagerService.UpdateAuthorRatingAsync(authorId);
                    }
                    return true;
                }
            }

            return false;
        }


        public async Task<bool> HasUserLikedAsync(int bookId, string userId)
        {
            return await _context.BookLikes
                .AnyAsync(bl => bl.BookId == bookId && bl.ApplicationUserId == userId);
        }

        public async Task<int> GetLikesCountAsync(int bookId)
        {
            return await _context.BookLikes
                .CountAsync(bl => bl.BookId == bookId);
        }

        public async Task AddReadingAsync(int bookId, string userId)
        {
            var existingReading = await _context.BookReadings
                .FirstOrDefaultAsync(br => br.BookId == bookId && br.ApplicationUserId == userId);

            if (existingReading == null)
            {
                var reading = new BookReading
                {
                    BookId = bookId,
                    ApplicationUserId = userId,
                    ReadDate = DateTime.UtcNow
                };
                _context.BookReadings.Add(reading);

                // Обновляем счетчик прочтений
                var book = await _context.Books.FindAsync(bookId);
                if (book != null)
                {
                    book.ReadsCount++;
                    await _context.SaveChangesAsync(); // СОХРАНЕНИЕ ДО обновления рейтинга

                    await _ratingManagerService.UpdateBookRatingAsync(bookId);

                    // Обновляем рейтинги всех авторов книги
                    var authorIds = await _context.BookAuthors
                        .Where(ba => ba.BookId == bookId)
                        .Select(ba => ba.AuthorId)
                        .ToListAsync();

                    foreach (var authorId in authorIds)
                    {
                        await _ratingManagerService.UpdateAuthorRatingAsync(authorId);
                    }
                }
            }
        }

        public async Task<int> GetReadingsCountAsync(int bookId)
        {
            return await _context.BookReadings
                .CountAsync(br => br.BookId == bookId);
        }

        // Избранное
        public async Task<bool> AddToFavoritesAsync(int bookId, string userId)
        {
            try
            {
                // Проверяем существование связи без навигационных свойств
                var exists = await _context.UserFavoriteBooks
                    .AnyAsync(ufb => ufb.BookId == bookId && ufb.ApplicationUserId == userId);

                if (exists)
                    return false;

                // Создаем новую запись с явно указанными ключами
                var favorite = new UserFavoriteBook
                {
                    BookId = bookId,
                    ApplicationUserId = userId,
                    AddedDate = DateTime.UtcNow
                };

                // Добавляем в контекст
                _context.UserFavoriteBooks.Add(favorite);

                // Сохраняем изменения
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch (DbUpdateException ex)
            {
                // Логируем детали ошибки
                Console.WriteLine($"Ошибка при добавлении в закладки: {ex.InnerException?.Message ?? ex.Message}");

                // Проверяем, существует ли книга и пользователь
                var bookExists = await _context.Books.AnyAsync(b => b.Id == bookId);
                var userExists = await _context.Users.AnyAsync(u => u.Id == userId);

                if (!bookExists)
                    throw new ArgumentException($"Книга с ID {bookId} не существует");

                if (!userExists)
                    throw new ArgumentException($"Пользователь с ID {userId} не существует");

                throw;
            }
        }

        // В методе RemoveFromFavoritesAsync в BookService.cs
        public async Task<bool> RemoveFromFavoritesAsync(int bookId, string userId)
        {
            try
            {
                var favorite = await _context.UserFavoriteBooks
                    .FirstOrDefaultAsync(ufb => ufb.BookId == bookId && ufb.ApplicationUserId == userId);

                if (favorite != null)
                {
                    _context.UserFavoriteBooks.Remove(favorite);
                    var result = await _context.SaveChangesAsync();
                    return result > 0;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при удалении книги {bookId} из закладок пользователя {userId}");
                throw;
            }
        }
        public async Task<List<string>> GetUsersWhoFavoritedBookAsync(int bookId)
        {
            return await _context.UserFavoriteBooks
                .Where(ufb => ufb.BookId == bookId)
                .Select(ufb => ufb.ApplicationUserId)
                .Distinct()
                .ToListAsync();
        }
        public async Task<bool> IsInFavoritesAsync(int bookId, string userId)
        {
            return await _context.UserFavoriteBooks
                .AnyAsync(ufb => ufb.BookId == bookId && ufb.ApplicationUserId == userId);
        }

        public async Task<List<Book>> GetUserFavoritesAsync(string userId)
        {
            return await _context.UserFavoriteBooks
                .Where(ufb => ufb.ApplicationUserId == userId)
                .Include(ufb => ufb.Book)
                    .ThenInclude(b => b.BookAuthors)
                        .ThenInclude(ba => ba.Author)
                .Select(ufb => ufb.Book)
                .ToListAsync();
        }

        // Приглашения соавторов
        public async Task<List<BookInvitation>> GetBookInvitationsAsync(int bookId)
        {
            return await _context.BookInvitations
                .Where(bi => bi.BookId == bookId)
                .Include(bi => bi.Book)
                .ToListAsync();
        }

        public async Task<BookInvitation> GetInvitationByTokenAsync(string token)
        {
            return await _context.BookInvitations
                .Include(bi => bi.Book)
                .FirstOrDefaultAsync(bi => bi.InvitationToken == token);
        }

        public async Task<BookInvitation> CreateInvitationAsync(int bookId, string email, string? invitedUserId = null)
        {
            var invitation = new BookInvitation
            {
                BookId = bookId,
                InviteeEmail = email,
                InviteeUserId = invitedUserId,
                InvitationToken = Guid.NewGuid().ToString(),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsAccepted = false
            };

            _context.BookInvitations.Add(invitation);
            await _context.SaveChangesAsync();

            return invitation;
        }

        public async Task<bool> UpdateInvitationAsync(BookInvitation invitation)
        {
            _context.Entry(invitation).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteInvitationAsync(int invitationId)
        {
            var invitation = await _context.BookInvitations.FindAsync(invitationId);
            if (invitation != null)
            {
                _context.BookInvitations.Remove(invitation);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        // Вспомогательные методы

        private async Task CleanupUnusedTagsAsync()
        {
            var unusedTags = await _context.Tags
                .Where(t => !t.BookTags.Any())
                .ToListAsync();

            _context.Tags.RemoveRange(unusedTags);
            await _context.SaveChangesAsync();
        }

        // Новые методы для BookDashboard
        public async Task<bool> UpdateBookCoverAsync(int bookId, IFormFile coverImage, string webRootPath)
        {
            try
            {
                var book = await GetByIdAsync(bookId);
                if (book == null) return false;

                // Удаляем старую обложку если есть
                if (!string.IsNullOrEmpty(book.CoverImageUrl))
                {
                    string oldFilePath = Path.Combine(webRootPath, book.CoverImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                // Сохраняем новую обложку
                string uploadFolder = Path.Combine(webRootPath, "images", "books");
                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(coverImage.FileName);
                string filePath = Path.Combine(uploadFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await coverImage.CopyToAsync(fileStream);
                }

                book.CoverImageUrl = "/images/books/" + uniqueFileName;
                await UpdateAsync(book);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обновлении обложки: {ex.Message}");
                return false;
            }
        }

        public async Task<List<Genre>> GetAllGenresAsync()
        {
            return await _context.Genres
                .OrderBy(g => g.Name)
                .ToListAsync();
        }

        public async Task UpdateBookGenresAsync(int bookId, List<string> genreNames)
        {
            // Удаляем текущие жанры книги
            var currentBookGenres = await _context.BookGenres
                .Where(bg => bg.BookId == bookId)
                .ToListAsync();

            _context.BookGenres.RemoveRange(currentBookGenres);

            // Добавляем новые жанры
            foreach (var genreName in genreNames)
            {
                var genre = await _context.Genres
                    .FirstOrDefaultAsync(g => g.Name.ToLower() == genreName.Trim().ToLower());

                if (genre == null)
                {
                    // Создаем новый жанр, если не существует
                    genre = new Genre { Name = genreName.Trim() };
                    _context.Genres.Add(genre);
                    await _context.SaveChangesAsync();
                }

                var bookGenre = new BookGenre
                {
                    BookId = bookId,
                    GenreId = genre.Id
                };
                _context.BookGenres.Add(bookGenre);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<BookAuthorInfo>> GetBookCoAuthorsAsync(int bookId)
        {
            var authors = await _context.BookAuthors
                .Where(ba => ba.BookId == bookId)
                .Include(ba => ba.Author)
                    .ThenInclude(a => a.ApplicationUser)
                .Select(ba => new BookAuthorInfo
                {
                    Id = ba.Author.Id,
                    PenName = ba.Author.PenName,
                    Email = ba.Author.ApplicationUser.Email,
                    Role = ba.Role ?? "Автор"
                })
                .ToListAsync();

            return authors;
        }

        public async Task<List<BookType>> GetAllBookTypesAsync()
        {
            return await _context.BookTypes
                .OrderBy(bt => bt.Name)
                .ToListAsync();
        }

        public async Task<List<BookCycle>> GetCyclesByAuthorIdAsync(int authorId)
        {
            return await _context.BookCycles
                .Where(bc => bc.AuthorId == authorId)
                .OrderBy(bc => bc.Name)
                .ToListAsync();
        }

        public async Task<BookCycle> CreateCycleAsync(string name, int authorId)
        {
            var cycle = new BookCycle
            {
                Name = name,
                AuthorId = authorId,
                CreatedDate = DateTime.UtcNow
            };

            _context.BookCycles.Add(cycle);
            await _context.SaveChangesAsync();

            return cycle;
        }

        public async Task<List<Tag>> GetAllTagsAsync()
        {
            return await _context.Tags
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        public async Task UpdateBookTagsAsync(int bookId, List<string> tagNames)
        {
            // Удаляем текущие связи с тегами
            var currentBookTags = await _context.BookTags
                .Where(bt => bt.BookId == bookId)
                .ToListAsync();

            _context.BookTags.RemoveRange(currentBookTags);

            // Добавляем новые
            foreach (var tagName in tagNames)
            {
                var cleanName = tagName.Trim();
                if (string.IsNullOrEmpty(cleanName)) continue;

                // Добавляем #, если нет
                if (!cleanName.StartsWith("#")) cleanName = "#" + cleanName;

                // Ищем существующий тег или создаем новый
                var tag = await _context.Tags
                    .FirstOrDefaultAsync(t => t.Name.ToLower() == cleanName.ToLower());

                if (tag == null)
                {
                    tag = new Tag { Name = cleanName };
                    _context.Tags.Add(tag);
                    await _context.SaveChangesAsync();
                }

                _context.BookTags.Add(new BookTag
                {
                    BookId = bookId,
                    TagId = tag.Id
                });
            }

            await _context.SaveChangesAsync();
            await CleanupUnusedTagsAsync();
        }

        public async Task UpdateBookPropertiesAsync(int bookId, BookDashboardViewModel model)
        {
            var book = await GetByIdAsync(bookId);
            if (book == null) return;

            book.Title = model.Title;
            book.Description = model.Description;
            book.IsAdultContent = model.IsAdultContent;
            book.EnableTTS = model.EnableTTS;
            book.AllowDownload = model.AllowDownload;
            book.TypeId = model.TypeId;
            book.CycleId = model.CycleId;
            book.IsPublic = model.IsPublic;

            // Обновляем статус
            if (!string.IsNullOrEmpty(model.Status))
            {
                var status = await _context.BookStatuses
                    .FirstOrDefaultAsync(s => s.Name.ToLower() == model.Status.ToLower());
                if (status != null)
                {
                    book.StatusId = status.Id;
                }
            }

            book.LastUpdateTime = DateTime.UtcNow;
            await UpdateAsync(book);
        }
        public async Task<List<BookAuthorInfo>> GetBookAuthorsInfoAsync(int bookId, int currentAuthorId)
        {
            var bookAuthors = await _context.BookAuthors
                .Where(ba => ba.BookId == bookId)
                .Include(ba => ba.Author)
                    .ThenInclude(a => a.ApplicationUser)
                .Select(ba => new BookAuthorInfo
                {
                    Id = ba.Author.Id,
                    PenName = ba.Author.PenName,
                    Email = ba.Author.ApplicationUser.Email,
                    Role = ba.Role,
                    IsCurrentUser = ba.Author.Id == currentAuthorId
                })
                .ToListAsync();

            return bookAuthors;
        }

        public async Task<bool> AddAuthorToBookAsync(int bookId, string email, string role = "Соавтор")
        {
            try
            {
                // Ищем пользователя по email
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user == null) return false;

                // Проверяем, является ли пользователь автором
                var author = await _context.Authors.FirstOrDefaultAsync(a => a.ApplicationUserId == user.Id);
                if (author == null)
                {
                    // Создаем автора
                    author = new Author
                    {
                        ApplicationUserId = user.Id,
                        PenName = user.UserName,
                        AuthorProfileCreationDate = DateTime.UtcNow
                    };
                    _context.Authors.Add(author);
                    await _context.SaveChangesAsync();
                }

                // Проверяем, не добавлен ли уже автор к книге
                var existingBookAuthor = await _context.BookAuthors
                    .FirstOrDefaultAsync(ba => ba.BookId == bookId && ba.AuthorId == author.Id);

                if (existingBookAuthor == null)
                {
                    var bookAuthor = new BookAuthor
                    {
                        BookId = bookId,
                        AuthorId = author.Id,
                        Role = role
                    };
                    _context.BookAuthors.Add(bookAuthor);
                    await _context.SaveChangesAsync();
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при добавлении автора: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RemoveAuthorFromBookAsync(int bookId, int authorId)
        {
            try
            {
                var bookAuthor = await _context.BookAuthors
                    .FirstOrDefaultAsync(ba => ba.BookId == bookId && ba.AuthorId == authorId);

                if (bookAuthor != null)
                {
                    _context.BookAuthors.Remove(bookAuthor);
                    await _context.SaveChangesAsync();
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении автора: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateAuthorRoleAsync(int bookId, int authorId, string role)
        {
            try
            {
                var bookAuthor = await _context.BookAuthors
                    .FirstOrDefaultAsync(ba => ba.BookId == bookId && ba.AuthorId == authorId);

                if (bookAuthor != null)
                {
                    bookAuthor.Role = role;
                    await _context.SaveChangesAsync();
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обновлении роли автора: {ex.Message}");
                return false;
            }
        }
        public async Task<bool> RemoveBookCoverAsync(int bookId, string webRootPath)
        {
            try
            {
                var book = await GetByIdAsync(bookId);
                if (book == null) return false;

                if (!string.IsNullOrEmpty(book.CoverImageUrl))
                {
                    // Удаляем файл обложки
                    string filePath = Path.Combine(webRootPath, book.CoverImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }

                    // Обновляем запись в БД
                    book.CoverImageUrl = null;
                    await UpdateAsync(book);

                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении обложки: {ex.Message}");
                return false;
            }
        }

        public async Task<List<YearlyStat>> GetYearlyStatsAsync(int bookId)
        {
            var yearlyStats = new List<YearlyStat>();

            try
            {
                // Статистика просмотров по годам
                var yearlyViews = await _context.BookReadings
                    .Where(br => br.BookId == bookId)
                    .GroupBy(br => br.ReadDate.Year)
                    .Select(g => new { Year = g.Key, Count = g.Count() })
                    .ToListAsync();

                // Статистика лайков по годам
                var yearlyLikes = await _context.BookLikes
                    .Where(bl => bl.BookId == bookId)
                    .GroupBy(bl => bl.LikeDate.Year)
                    .Select(g => new { Year = g.Key, Count = g.Count() })
                    .ToListAsync();

                // Объединяем данные
                var allYears = yearlyViews.Select(v => v.Year)
                    .Union(yearlyLikes.Select(l => l.Year))
                    .Distinct()
                    .OrderByDescending(y => y);

                foreach (var year in allYears)
                {
                    var views = yearlyViews.FirstOrDefault(v => v.Year == year)?.Count ?? 0;
                    var likes = yearlyLikes.FirstOrDefault(l => l.Year == year)?.Count ?? 0;

                    yearlyStats.Add(new YearlyStat
                    {
                        Year = year,
                        Views = views,
                        Likes = likes,
                        Reads = views // Можно добавить отдельную логику для прочтений если нужно
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении статистики по годам: {ex.Message}");
            }

            return yearlyStats;
        }
        // В BookService.cs добавьте следующие методы:

        public async Task<List<Book>> GetSimilarBooksAsync(int bookId, int count = 5)
        {
            try
            {
                var calculatedBooks = await GetSimilarBooksFromCalculationAsync(bookId, count);
                if (calculatedBooks.Any())
                    return calculatedBooks;

                // Fallback на старый алгоритм если расчетных данных нет
                _logger.LogWarning($"Для книги {bookId} нет расчетных похожих книг, использую fallback алгоритм");

                var currentBook = await _context.Books
                    .Include(b => b.BookGenres)
                        .ThenInclude(bg => bg.Genre)
                    .Include(b => b.BookTags)
                        .ThenInclude(bt => bt.Tag)
                    .FirstOrDefaultAsync(b => b.Id == bookId);

                if (currentBook == null)
                    return new List<Book>();

                var currentGenreIds = currentBook.BookGenres.Select(bg => bg.GenreId).ToList();
                var currentTagIds = currentBook.BookTags.Select(bt => bt.TagId).ToList();

                var similarBooks = await _context.Books
                    .Where(b => b.Id != bookId && b.IsPublic)
                    .Include(b => b.BookGenres)
                        .ThenInclude(bg => bg.Genre)
                    .Include(b => b.BookTags)
                        .ThenInclude(bt => bt.Tag)
                    .Include(b => b.BookAuthors)
                        .ThenInclude(ba => ba.Author)
                    .ToListAsync();

                var scoredBooks = similarBooks.Select(b => new
                {
                    Book = b,
                    Score = CalculateFallbackSimilarityScore(b, currentGenreIds, currentTagIds)
                })
                .Where(x => x.Score > 0)
                .OrderByDescending(x => x.Score)
                .Take(count)
                .Select(x => x.Book)
                .ToList();

                return scoredBooks;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при поиске похожих книг для книги {bookId}");
                return new List<Book>();
            }
        }
        private int CalculateFallbackSimilarityScore(Book book, List<int> currentGenreIds, List<int> currentTagIds)
        {
            int score = 0;
            var bookGenreIds = book.BookGenres.Select(bg => bg.GenreId).ToList();
            score += bookGenreIds.Intersect(currentGenreIds).Count() * 2;

            var bookTagIds = book.BookTags.Select(bt => bt.TagId).ToList();
            score += bookTagIds.Intersect(currentTagIds).Count();

            return score;
        }
        public async Task<List<Book>> GetSimilarBooksFromCalculationAsync(int bookId, int count = 5)
        {
            try
            {
                return await _context.SimilarBooks
                    .Where(sb => sb.BookId == bookId && sb.SimilarityScore > 0.2)
                    .OrderByDescending(sb => sb.SimilarityScore)
                    .Take(count)
                    .Include(sb => sb.SimilarToBook)
                        .ThenInclude(b => b.BookAuthors)
                            .ThenInclude(ba => ba.Author)
                    .Include(sb => sb.SimilarToBook)
                        .ThenInclude(b => b.BookGenres)
                            .ThenInclude(bg => bg.Genre)
                    .Select(sb => sb.SimilarToBook)
                    .Where(b => b.IsPublic)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при получении рассчитанных похожих книг для {bookId}");
                // Возвращаем пустой список или используем старый метод как fallback
                return await GetSimilarBooksAsync(bookId, count);
            }
        }
        public async Task<Comment> GetCommentByIdAsync(int commentId)
        {
            return await _context.Comments
                .Include(c => c.ApplicationUser)
                .FirstOrDefaultAsync(c => c.Id == commentId);
        }

        public async Task<bool> UpdateCommentAsync(Comment comment)
        {
            try
            {
                _context.Entry(comment).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<Comment> AddReplyAsync(int parentCommentId, string userId, string replyText)
        {
            try
            {
                var parentComment = await _context.Comments.FindAsync(parentCommentId);
                if (parentComment == null) return null;

                var reply = new Comment
                {
                    ParentCommentId = parentCommentId,
                    BookId = parentComment.BookId,
                    ApplicationUserId = userId,
                    Content = replyText,
                    CreatedDate = DateTime.UtcNow
                };

                _context.Comments.Add(reply);
                await _context.SaveChangesAsync();

                return await _context.Comments
                    .Include(c => c.ApplicationUser)
                    .FirstOrDefaultAsync(c => c.Id == reply.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении ответа на комментарий");
                return null;
            }
        }
        public async Task<BookCycle> GetBookCycleAsync(int? cycleId)
        {
            if (!cycleId.HasValue)
                return null;

            return await _context.BookCycles
                .Include(c => c.Books)
                    .ThenInclude(b => b.BookAuthors)
                        .ThenInclude(ba => ba.Author)
                .Include(c => c.Author)
                .FirstOrDefaultAsync(c => c.Id == cycleId.Value);
        }

        public async Task<List<Book>> GetCycleBooksAsync(int cycleId, int excludeBookId)
        {
            return await _context.Books
                .Where(b => b.CycleId == cycleId && b.Id != excludeBookId && b.IsPublic)
                .Include(b => b.BookAuthors)
                    .ThenInclude(ba => ba.Author)
                .Include(b => b.BookGenres)
                    .ThenInclude(bg => bg.Genre)
                .OrderBy(b => b.PublicationDate)
                .ToListAsync();
        }

        // В BookService.cs - исправленный метод GetBookCommentsAsync
        public async Task<List<CommentViewModel>> GetBookCommentsAsync(int bookId)
        {
            return await _context.Comments
                .Where(c => c.BookId == bookId && c.ParentCommentId == null) // Только родительские комментарии
                .Include(c => c.ApplicationUser)
                .Include(c => c.Replies) // Включаем ответы
                    .ThenInclude(r => r.ApplicationUser) // И пользователей для ответов
                .OrderByDescending(c => c.CreatedDate)
                .Select(c => new CommentViewModel
                {
                    Id = c.Id,
                    Content = c.Content,
                    UserId = c.ApplicationUserId,
                    UserName = c.ApplicationUser.UserName,
                    UserAvatar = c.ApplicationUser.ProfilePictureUrl,
                    Timestamp = c.CreatedDate,
                    Replies = c.Replies.Select(r => new CommentViewModel
                    {
                        Id = r.Id,
                        Content = r.Content,
                        UserId = r.ApplicationUserId,
                        UserName = r.ApplicationUser.UserName,
                        UserAvatar = r.ApplicationUser.ProfilePictureUrl,
                        Timestamp = r.CreatedDate
                    }).OrderBy(r => r.Timestamp).ToList() // Сортируем ответы по времени
                })
                .ToListAsync();
        }
        // В BookService.cs добавьте эти методы
        public async Task<Comment> GetReplyByIdAsync(int replyId)
        {
            return await _context.Comments
                .Include(c => c.ApplicationUser)
                .FirstOrDefaultAsync(c => c.Id == replyId);
        }

        public async Task<bool> UpdateReplyAsync(int replyId, string userId, string newText)
        {
            try
            {
                var reply = await _context.Comments
                    .FirstOrDefaultAsync(c => c.Id == replyId && c.ApplicationUserId == userId);

                if (reply == null) return false;

                reply.Content = newText;
                _context.Entry(reply).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteReplyAsync(int replyId, string userId)
        {
            try
            {
                var reply = await _context.Comments
                    .FirstOrDefaultAsync(c => c.Id == replyId && c.ApplicationUserId == userId);

                if (reply == null) return false;

                _context.Comments.Remove(reply);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // В BookService.cs - метод AddBookCommentAsync
        public async Task<CommentViewModel> AddBookCommentAsync(int bookId, string userId, string commentText)
        {
            if (string.IsNullOrWhiteSpace(commentText))
                throw new ArgumentException("Комментарий не может быть пустым");

            if (commentText.Length > 500)
                throw new ArgumentException("Комментарий слишком длинный");

            // Проверяем существование книги
            var book = await _context.Books.FindAsync(bookId);
            if (book == null)
                throw new ArgumentException("Книга не найдена");

            // Проверяем существование пользователя
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new ArgumentException("Пользователь не найден");

            var comment = new Comment
            {
                BookId = bookId,
                ApplicationUserId = userId,
                Content = commentText.Trim(),
                CreatedDate = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            // Получаем созданный комментарий с пользователем
            var createdComment = await _context.Comments
                .Include(c => c.ApplicationUser)
                .FirstOrDefaultAsync(c => c.Id == comment.Id);

            if (createdComment == null)
                return null;

            return new CommentViewModel
            {
                Id = createdComment.Id,
                Content = createdComment.Content,
                UserId = createdComment.ApplicationUserId,
                UserName = createdComment.ApplicationUser.UserName ?? "Пользователь",
                UserAvatar = createdComment.ApplicationUser.ProfilePictureUrl,
                Timestamp = createdComment.CreatedDate
            };
        }

        public async Task<bool> DeleteBookCommentAsync(int commentId, string userId)
        {
            var comment = await _context.Comments
                .FirstOrDefaultAsync(c => c.Id == commentId && c.ApplicationUserId == userId);

            if (comment == null)
                return false;

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            return true;
        }
        // Добавьте в BookService.cs
        public async Task<BookExportModel> GetBookForExportAsync(int bookId, string webRootPath = null)
        {
            try
            {
                var book = await GetByIdAsync(bookId);
                if (book == null || !book.AllowDownload)
                    return null;

                var chapters = await _chapterService.GetPublishedChaptersByBookIdAsync(bookId);
                var authors = await GetBookAuthorsAsync(bookId);
                var genres = await GetBookGenresAsync(bookId);
                var tags = await GetBookTagsAsync(bookId);

                var exportModel = new BookExportModel
                {
                    Id = book.Id,
                    Title = book.Title,
                    Description = book.Description,
                    CoverImageUrl = !string.IsNullOrEmpty(book.CoverImageUrl) ?
                                  (webRootPath != null ? Path.Combine(webRootPath, book.CoverImageUrl.TrimStart('/')) : book.CoverImageUrl) :
                                  null,
                    PublicationDate = book.PublicationDate.ToString("dd.MM.yyyy"),
                    Authors = authors.Select(a => new AuthorExportModel
                    {
                        PenName = a.PenName,
                        Role = a.BookAuthors?.FirstOrDefault(ba => ba.AuthorId == a.Id && ba.BookId == bookId)?.Role ?? "Автор"
                    }).ToList(),
                    Genres = genres.Select(g => g.Name).ToList(),
                    Tags = tags.Select(t => t.Name).ToList(),
                    Chapters = new List<ChapterExportModel>()
                };

                foreach (var chapter in chapters.OrderBy(c => c.Order))
                {
                    // Получаем все страницы главы
                    var pages = await _context.Pages
                        .Where(p => p.ChapterId == chapter.Id)
                        .OrderBy(p => p.PageNumber)
                        .ToListAsync();

                    // Собираем текстовый контент из текстовых страниц
                    var textContent = pages
                        .Where(p => string.IsNullOrEmpty(p.ImageUrl))
                        .Select(p => p.TextContent ?? "")
                        .ToList();

                    var chapterExport = new ChapterExportModel
                    {
                        Order = chapter.Order,
                        Title = chapter.Title,
                        Content = string.Join("\n\n", textContent),
                        Pages = pages.Where(p => !string.IsNullOrEmpty(p.ImageUrl))
                            .Select(p => new ChapterPageExportModel
                            {
                                PageNumber = p.PageNumber,
                                TextContent = p.TextContent ?? "",
                                ImageUrl = webRootPath != null ?
                                           Path.Combine(webRootPath, p.ImageUrl.TrimStart('/')) :
                                           p.ImageUrl
                            })
                            .ToList()
                    };

                    exportModel.Chapters.Add(chapterExport);
                }

                return exportModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при подготовке книги {bookId} для экспорта");
                return null;
            }
        }
    }
}