// Проект: MajorAuthor
// Файл: Models/MyProfileViewModel.cs
using System.Collections.Generic;
using System.Linq; // Для LINQ-операций

namespace MajorAuthor.Models
{
    /// <summary>
    /// ViewModel для отображения страницы профиля пользователя.
    /// </summary>
    public class MyProfileViewModel
    {
        public bool IsAuthor { get; set; } // Указывает, является ли текущий пользователь автором
        public string UserId { get; set; } // ID пользователя (string)
        public string UserName { get; set; } // Имя пользователя (логин из Identity)
        public string DisplayName { get; set; } // Отображаемое имя (ник или псевдоним)
        public string PhotoUrl { get; set; } // URL аватарки/фото профиля
        public string FullName { get; set; } // ФИО автора (если применимо)

        // Данные для обычных пользователей (не авторов)
        public List<BookDisplayModel> ReadBooks { get; set; } = new List<BookDisplayModel>(); // Прочитанные книги
        public List<BookDisplayModel> FavoriteBooks { get; set; } = new List<BookDisplayModel>(); // Книги в закладках/избранном
        public List<BookDisplayModel> LikedBooks { get; set; } = new List<BookDisplayModel>(); // Книги, которые понравились
        public List<BlogDisplayModel> LikedBlogs { get; set; } = new List<BlogDisplayModel>(); // Блоги, которые понравились

        // Данные для авторов
        public List<BookDisplayModel> AuthoredBooks { get; set; } = new List<BookDisplayModel>(); // Написанные книги
        public List<BlogDisplayModel> AuthoredBlogs { get; set; } = new List<BlogDisplayModel>(); // Написанные блоги
        public int FollowerCount { get; set; } // Количество подписчиков
        public List<FollowerDisplayModel> FollowersList { get; set; } = new List<FollowerDisplayModel>(); // Список подписчиков

        /// <summary>
        /// Модель для отображения информации о книге.
        /// </summary>
        public class BookDisplayModel
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string AuthorName { get; set; }
            public string CoverImageUrl { get; set; }
            public int ReadsCount { get; set; }
            public int LikesCount { get; set; }
            public bool IsAdultContent { get; set; }
        }

        /// <summary>
        /// Модель для отображения информации о блоге.
        /// </summary>
        public class BlogDisplayModel
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string AuthorName { get; set; } // Имя автора блога
            public string ImageUrl { get; set; }
            public string ContentSnippet { get; set; } // Краткий отрывок содержимого
            public int ViewsCount { get; set; }
            public int LikesCount { get; set; }
            public int CommentsCount { get; set; }
            public System.DateTime PublicationDate { get; set; } // Дата публикации
        }

        /// <summary>
        /// Модель для отображения информации о подписчике.
        /// </summary>
        public class FollowerDisplayModel
        {
            public string UserId { get; set; }
            public string UserName { get; set; } // Имя пользователя подписчика
        }
    }
}
