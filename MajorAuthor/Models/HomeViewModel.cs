using MajorAuthor.Data.Entities;
using System.Collections.Generic;

namespace MajorAuthor.Models
{
    /// <summary>
    /// ViewModel для главной страницы, содержащая данные для всех разделов.
    /// </summary>
    public class HomeViewModel
    {
        public bool IsUserLoggedIn { get; set; }
        public List<GenreDisplayModel> AvailableGenres { get; set; }
        public int SelectedGenreId { get; set; }
        public List<BookDisplayModel> BooksBySelectedGenre { get; set; }
        public List<BookDisplayModel> PopularBooks { get; set; }
        public List<BookDisplayModel> RecentlyUpdatedBooks { get; set; }
        public List<BookDisplayModel> NewPopularBooks { get; set; }
        public List<BookDisplayModel> PromotedBooks { get; set; }
        public List<BookDisplayModel> RecommendedBooks { get; set; }
        public List<AuthorDisplayModel> PopularAuthors { get; set; }
        public List<AuthorDisplayModel> NewPopularAuthors { get; set; }

        // НОВЫЕ СВОЙСТВА ДЛЯ СТИХОВ И БЛОГОВ
        public List<PoemDisplayModel> PopularPoems { get; set; }
        public List<PoemDisplayModel> NewPoems { get; set; }
        public List<BlogDisplayModel> PopularBlogs { get; set; }

        public class BookDisplayModel
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string AuthorName { get; set; }
            public string CoverImageUrl { get; set; }
            public int ReadsCount { get; set; }
            public int LikesCount { get; set; }
            public bool IsAdultContent { get; set; }
            public string UpdateInfo { get; set; }
            public string RecommendationReason { get; set; }
        }

        public class AuthorDisplayModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string PhotoUrl { get; set; }
            public int BooksCount { get; set; }
            public int TotalReadsCount { get; set; }
            public string RegistrationInfo { get; set; }
        }

        public class GenreDisplayModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        // НОВАЯ МОДЕЛЬ ДЛЯ СТИХОВ
        public class PoemDisplayModel
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string AuthorName { get; set; }
            public string ContentSnippet { get; set; }
            public int ReadsCount { get; set; }
            public int LikesCount { get; set; }
            public int CommentsCount { get; set; }
            public string CreationInfo { get; set; }
        }

        // НОВАЯ МОДЕЛЬ ДЛЯ БЛОГОВ
        public class BlogDisplayModel
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string AuthorName { get; set; }
            public string ContentSnippet { get; set; }
            public int CommentsCount { get; set; }
            public int ViewsCount { get; set; }
            public int LikesCount { get; set; } 
        }
    }
}
