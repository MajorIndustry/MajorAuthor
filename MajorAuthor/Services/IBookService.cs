using MajorAuthor.Data.Entities;
using MajorAuthor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajorAuthor.Services
{
    public interface IBookService : IWorkService<Book>
    {
        Task LinkBookToAuthorAsync(int id1, int id2);
        Task<bool> IsAuthorOfBookAsync(int bookId, int authorId);
        Task<bool> IsAuthorOfBookAsync(int bookId, string userId);
        Task<List<Author>> GetBookAuthorsAsync(int bookId);
        Task<bool> RemoveAuthorFromBookAsync(int bookId, int authorId);

        // Главы
       

        // Статистика и аналитика
        Task<BookStats> GetBookStatsAsync(int bookId);
        Task UpdateBookPublicationStatusAsync(int bookId);
        Task<int> GetTotalCharactersAsync(int bookId);
        Task<int> GetTotalPagesAsync(int bookId);

        // Теги
        Task<List<string>> GetBookTagsStringsAsync(int bookId);
        Task<List<Tag>> GetBookTagsAsync(int bookId);
        Task AddTagToBookAsync(int bookId, string tagName);
        Task RemoveTagFromBookAsync(int bookId, string tagName);
        Task<bool> TagExistsAsync(string tagName);

        // Жанры
        Task<List<Genre>> GetBookGenresAsync(int bookId);
        Task AddGenreToBookAsync(int bookId, int genreId);
        Task RemoveGenreFromBookAsync(int bookId, int genreId);

        // Поиск и фильтрация
        Task<List<Book>> SearchBooksAsync(string query, int? authorId = null);
        Task<List<Book>> GetBooksByGenreAsync(int genreId);
        Task<List<Book>> GetPopularBooksAsync(int count = 10);
        Task<List<Book>> GetRecentlyUpdatedBooksAsync(int count = 10);

        // Лайки и чтения
        Task<Book> GetByIdWithCommentsAndLikesAsync(int id);
        Task<bool> AddLikeAsync(int bookId, string userId);
        Task<bool> RemoveLikeAsync(int bookId, string userId);
        Task<bool> HasUserLikedAsync(int bookId, string userId);
        Task<int> GetLikesCountAsync(int bookId);
        Task AddReadingAsync(int bookId, string userId);
        Task<int> GetReadingsCountAsync(int bookId);

        // Избранное
        Task<bool> AddToFavoritesAsync(int bookId, string userId);
        Task<bool> RemoveFromFavoritesAsync(int bookId, string userId);
        Task<bool> IsInFavoritesAsync(int bookId, string userId);
        Task<List<Book>> GetUserFavoritesAsync(string userId);

        // Приглашения соавторов
        Task<List<BookInvitation>> GetBookInvitationsAsync(int bookId);
        Task<BookInvitation> GetInvitationByTokenAsync(string token);
        Task<BookInvitation> CreateInvitationAsync(int bookId, string email, string? invitedUserId = null);
        Task<bool> UpdateInvitationAsync(BookInvitation invitation);
        Task<bool> DeleteInvitationAsync(int invitationId);
        Task<bool> UpdateBookCoverAsync(int bookId, IFormFile coverImage, string webRootPath);
        Task<List<Genre>> GetAllGenresAsync();
        Task UpdateBookGenresAsync(int bookId, List<string> genreNames);
        Task UpdateBookPropertiesAsync(int bookId, BookDashboardViewModel model);
        Task<List<Tag>> GetAllTagsAsync();
        Task<List<BookType>> GetAllBookTypesAsync();
        Task<BookCycle> CreateCycleAsync(string name, int authorId);
        Task<List<BookCycle>> GetCyclesByAuthorIdAsync(int authorId);
        Task UpdateBookTagsAsync(int bookId, List<string> tagNames);
        Task<List<BookAuthorInfo>> GetBookCoAuthorsAsync(int bookId);
        Task<bool> RemoveBookCoverAsync(int bookId, string webRootPath);
        Task<List<YearlyStat>> GetYearlyStatsAsync(int bookId);
        Task<BookCycle> GetBookCycleAsync(int? cycleId);
        Task<List<Book>> GetCycleBooksAsync(int value, int id);
        Task<List<Book>> GetSimilarBooksAsync(int id, int v);
        Task<List<CommentViewModel>> GetBookCommentsAsync(int id);
        Task<CommentViewModel> AddBookCommentAsync(int bookId, string userId, string commentText);
        Task<bool> DeleteBookCommentAsync(int commentId, string userId);
        Task<Comment> GetCommentByIdAsync(int commentId);
        Task<Comment> AddReplyAsync(int parentCommentId, string userId, string replyText);
        Task<bool> UpdateCommentAsync(Comment comment);
        Task<bool> UpdateReplyAsync(int replyId, string userId, string newText);
        Task<bool> DeleteReplyAsync(int replyId, string userId);
        Task<Comment> GetReplyByIdAsync(int replyId);
        Task<BookExportModel> GetBookForExportAsync(int bookId, string webRootPath = null);
        Task<List<string>> GetUsersWhoFavoritedBookAsync(int bookId);
    }

    public class BookStats
    {
        public int TotalCharacters { get; set; }
        public int TotalPages { get; set; }
        public int PublishedChapters { get; set; }
        public int TotalChapters { get; set; }
        public int LikesCount { get; set; }
        public int ReadsCount { get; set; }
        public int CommentsCount { get; set; }
        public DateTime LastUpdate { get; set; }
    }
}
