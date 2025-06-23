// Проект: MajorAuthor.Web
// Файл: Models/HomeViewModel.cs
using System.Collections.Generic;

namespace MajorAuthor.Web.Models
{
    /// <summary>
    /// ViewModel для главной страницы, содержащая данные для различных секций.
    /// </summary>
    public class HomeViewModel
    {
        public List<BookDisplayModel> PopularBooks { get; set; } = new List<BookDisplayModel>();
        public List<AuthorDisplayModel> PopularAuthors { get; set; } = new List<AuthorDisplayModel>();
        public List<BookDisplayModel> RecentlyUpdatedBooks { get; set; } = new List<BookDisplayModel>();
        public List<BookDisplayModel> NewPopularBooks { get; set; } = new List<BookDisplayModel>();
        public List<AuthorDisplayModel> NewPopularAuthors { get; set; } = new List<AuthorDisplayModel>();
        public List<BookDisplayModel> RecommendedBooks { get; set; } = new List<BookDisplayModel>(); // Для советов

        public bool IsUserLoggedIn { get; set; } // Флаг, указывающий, вошел ли пользователь

        public List<GenreDisplayModel> AvailableGenres { get; set; } = new List<GenreDisplayModel>(); // Добавлено: Список всех доступных жанров для выбора
        public int? SelectedGenreId { get; set; } // Добавлено: ID выбранного жанра (для фильтрации)
        public List<BookDisplayModel> BooksBySelectedGenre { get; set; } = new List<BookDisplayModel>(); // Добавлено: Книги по выбранному жанру

        /// <summary>
        /// Модель для отображения информации о книге на главной странице.
        /// </summary>
        public class BookDisplayModel
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string AuthorName { get; set; } // Для простоты, может быть списком имен для соавторства
            public string CoverImageUrl { get; set; }
            public int ReadsCount { get; set; }
            public int LikesCount { get; set; }
            public bool IsAdultContent { get; set; }
            public string UpdateInfo { get; set; } // Например, "Обновлено: вчера"
            public string RecommendationReason { get; set; } // Например, "Потому что вы читали ..."
        }

        /// <summary>
        /// Модель для отображения информации об авторе на главной странице.
        /// </summary>
        public class AuthorDisplayModel
        {
            public int Id { get; set; }
            public string Name { get; set; } // Псевдоним автора
            public string PhotoUrl { get; set; }
            public int BooksCount { get; set; }
            public int TotalReadsCount { get; set; } // Общее количество прочтений книг автора
            public string RegistrationInfo { get; set; } // Например, "Зарегистрирован: 15 дней назад"
        }

        /// <summary>
        /// Модель для отображения информации о жанре.
        /// </summary>
        public class GenreDisplayModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
