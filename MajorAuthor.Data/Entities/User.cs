// Проект: MajorAuthor.Data
// Файл: Entities/User.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MajorAuthor.Data.Entities
{
    /// <summary>
    /// Представляет пользователя платформы.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Уникальный идентификатор пользователя.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Имя пользователя (никнейм).
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Username { get; set; }

        /// <summary>
        /// Адрес электронной почты пользователя.
        /// </summary>
        [Required]
        [MaxLength(255)]
        public string Email { get; set; }

        /// <summary>
        /// Хэш пароля пользователя.
        /// </summary>
        [Required]
        public string PasswordHash { get; set; }

        /// <summary>
        /// Дата регистрации пользователя.
        /// </summary>
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Дата рождения пользователя (для возрастных ограничений). Может быть null, если не указана.
        /// </summary>
        public DateTime? DateOfBirth { get; set; }

        /// <summary>
        /// Навигационное свойство к авторскому профилю (если пользователь является автором).
        /// Может быть null, так как не каждый пользователь является автором.
        /// </summary>
        public Author AuthorProfile { get; set; }

        /// <summary>
        /// Коллекция записей о прочтениях книг этим пользователем.
        /// </summary>
        public ICollection<BookReading> Readings { get; set; } = new List<BookReading>();

        /// <summary>
        /// Коллекция лайков, поставленных этим пользователем.
        /// </summary>
        public ICollection<BookLike> Likes { get; set; } = new List<BookLike>();

        /// <summary>
        /// Жанры, которые пользователь предпочитает (для рекомендаций).
        /// </summary>
        public ICollection<UserPreferredGenre> PreferredGenres { get; set; } = new List<UserPreferredGenre>();

        /// <summary>
        /// Теги, которые пользователь предпочитает (для рекомендаций).
        /// </summary>
        public ICollection<UserPreferredTag> PreferredTags { get; set; } = new List<UserPreferredTag>();
    }
}
