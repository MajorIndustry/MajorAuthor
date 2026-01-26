// Models/HomeViewModel.cs
using System;
using System.Collections.Generic;

namespace MajorAuthor.Models
{
    /// <summary>
    /// ViewModel для главной страницы, содержащая данные для всех разделов с рейтингами.
    /// </summary>
    public class HomeViewModel
    {
        public bool IsUserLoggedIn { get; set; }
        public List<GenreDisplayModel> AvailableGenres { get; set; }
        public int SelectedGenreId { get; set; }

        // Основные рейтинги (общий)
        public List<BookDisplayModel> PopularBooks { get; set; }
        public List<BookDisplayModel> RecentlyUpdatedBooks { get; set; }
        public List<BookDisplayModel> PromotedBooks { get; set; }
        public List<BookDisplayModel> RecommendedBooks { get; set; }
        public List<AuthorDisplayModel> PopularAuthors { get; set; }

        // Рейтинги по периодам
        public List<BookDisplayModel> WeeklyPopularBooks { get; set; }
        public List<BookDisplayModel> MonthlyPopularBooks { get; set; }
        public List<BookDisplayModel> YearlyPopularBooks { get; set; }

        // Стихи
        public List<PoemDisplayModel> PopularPoems { get; set; }
        public List<PoemDisplayModel> NewPoems { get; set; }
        public List<PoemDisplayModel> WeeklyPopularPoems { get; set; }
        public List<PoemDisplayModel> MonthlyPopularPoems { get; set; }
        public List<PoemDisplayModel> YearlyPopularPoems { get; set; }

        // Блоги
        public List<BlogDisplayModel> PopularBlogs { get; set; }

        // Новые авторы
        public List<AuthorDisplayModel> NewPopularAuthors { get; set; }

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
            public bool IsNew { get; set; }
            public bool IsPublic { get; set; }
            public string Description { get; set; }
            public DateTime PublicationDate { get; set; }

            // Lists for UI
            public List<string> Genres { get; set; } = new List<string>();
            public List<string> Tags { get; set; } = new List<string>();
            public List<string> Authors { get; set; } = new List<string>();

            // Рейтинги
            public double Rating { get; set; } // Общий рейтинг (глобальный)
            public double WeeklyRating { get; set; }
            public double MonthlyRating { get; set; }
            public double YearlyRating { get; set; }
        }

        public class AuthorDisplayModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string PhotoUrl { get; set; }
            public int BooksCount { get; set; }
            public int TotalReadsCount { get; set; }
            public int TotalLikesCount { get; set; }
            public string RegistrationInfo { get; set; }
            public bool IsNew { get; set; }

            // Рейтинги
            public double Rating { get; set; }
            public double WeeklyRating { get; set; }
            public double MonthlyRating { get; set; }
            public double YearlyRating { get; set; }
        }

        public class GenreDisplayModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

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
            public bool IsNew { get; set; }

            // Рейтинги
            public double Rating { get; set; } // Общий рейтинг (глобальный)
            public double WeeklyRating { get; set; }
            public double MonthlyRating { get; set; }
            public double YearlyRating { get; set; }
        }

        public class BlogDisplayModel
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string AuthorName { get; set; }
            public string ContentSnippet { get; set; }
            public int CommentsCount { get; set; }
            public int ViewsCount { get; set; }
            public int LikesCount { get; set; }
            public bool IsNew { get; set; }

            // Рейтинги
            public double Rating { get; set; }
            public double WeeklyRating { get; set; }
            public double MonthlyRating { get; set; }
        }
    }
}