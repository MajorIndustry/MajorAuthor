// Проект: MajorAuthor.Data
// Файл: ApplicationUser.cs
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using MajorAuthor.Data.Entities; // Необходимо для Author, UserPreferredGenre, UserPreferredTag, UserFavoriteBook

namespace MajorAuthor.Data
{
    /// <summary>
    /// Расширенная сущность пользователя Identity, содержащая дополнительные свойства.
    /// Это ваша основная сущность пользователя.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        // Пример дополнительного свойства, если оно вам нужно
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

        // Если у пользователя есть аватар, можно добавить URL
        public string? ProfilePictureUrl { get; set; }

        // Новое: Навигационное свойство для связи один-к-одному с профилем автора
        public Author? AuthorProfile { get; set; }

        public ICollection<BookReading> Readings { get; set; } = new List<BookReading>();

        /// <summary>
        /// Коллекция лайков книг, поставленных этим пользователем.
        /// </summary>
        public ICollection<BookLike> BookLikes { get; set; } = new List<BookLike>();

        /// <summary>
        /// Коллекция лайков стихов, поставленных этим пользователем.
        /// </summary>
        public ICollection<PoemLike> PoemLikes { get; set; } = new List<PoemLike>();
        /// <summary>
        /// Коллекция лайков блогов, поставленных этим пользователем.
        /// </summary>
        public ICollection<BlogLike> BlogLikes { get; set; } = new List<BlogLike>();

        /// <summary>
        /// Коллекция комментариев к книгам, оставленных этим пользователем.
        /// </summary>
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();

        /// <summary>
        /// Коллекция комментариев к стихам, оставленных этим пользователем.
        /// </summary>
        public ICollection<PoemComment> PoemComments { get; set; } = new List<PoemComment>();
        /// <summary>
        /// Коллекция комментариев к блогам, оставленных этим пользователем.
        /// </summary>
        public ICollection<BlogComment> BlogComments { get; set; } = new List<BlogComment>();

        /// <summary>
        /// Жанры, которые пользователь предпочитает (для рекомендаций).
        /// </summary>
        public ICollection<UserPreferredGenre> PreferredGenres { get; set; } = new List<UserPreferredGenre>();

        /// <summary>
        /// Теги, которые пользователь предпочитает (для рекомендаций).
        /// </summary>
        public ICollection<UserPreferredTag> PreferredTags { get; set; } = new List<UserPreferredTag>();

        /// <summary>
        /// Коллекция избранных книг пользователя.
        /// </summary>
        public ICollection<UserFavoriteBook> FavoriteBooks { get; set; } = new List<UserFavoriteBook>();

        /// <summary>
        /// Коллекция сообщений, отправленных этим пользователем.
        /// </summary>
        public ICollection<Message> SentMessages { get; set; } = new List<Message>();

        /// <summary>
        /// Коллекция сообщений, полученных этим пользователем.
        /// </summary>
        public ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();

        /// <summary>
        /// Коллекция подписок пользователя на авторов.
        /// </summary>
        public ICollection<Follower> Following { get; set; } = new List<Follower>();

        /// <summary>
        /// Коллекция уведомлений для этого пользователя.
        /// </summary>
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();

        /// <summary>
        /// Коллекция прочитанных глав пользователем.
        /// </summary>
        public ICollection<ChapterRead> ChaptersRead { get; set; } = new List<ChapterRead>();
    }
}
