// Проект: MajorAuthor.Data
// Файл: Entities/Book.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MajorAuthor.Data.Entities
{
    /// <summary>
    /// Представляет книгу.
    /// </summary>
    public class Book
    {
        /// <summary>
        /// Уникальный идентификатор книги.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Название книги.
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string Title { get; set; }

        /// <summary>
        /// Описание или аннотация книги.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Дата публикации книги.
        /// </summary>
        public DateTime PublicationDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// URL обложки книги (опционально).
        /// </summary>
        [MaxLength(500)]
        public string CoverImageUrl { get; set; }

        /// <summary>
        /// Количество лайков книги.
        /// </summary>
        public int LikesCount { get; set; }

        /// <summary>
        /// Количество прочтений книги.
        /// </summary>
        public int ReadsCount { get; set; }

        /// <summary>
        /// Дата последнего обновления книги. Используется для "Недавно обновленных книг".
        /// </summary>
        public DateTime LastUpdateTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Указывает, содержит ли книга контент 18+.
        /// </summary>
        public bool IsAdultContent { get; set; } = false;

        /// <summary>
        /// Коллекция записей о прочтениях этой книги.
        /// </summary>
        public ICollection<BookReading> Readings { get; set; } = new List<BookReading>();

        /// <summary>
        /// Коллекция лайков этой книги.
        /// </summary>
        public ICollection<BookLike> Likes { get; set; } = new List<BookLike>();

        /// <summary>
        /// Коллекция связей с жанрами для этой книги (многие-ко-многим).
        /// </summary>
        public ICollection<BookGenre> BookGenres { get; set; } = new List<BookGenre>();

        /// <summary>
        /// Коллекция связей с авторами, участвующими в написании книги (для совместного написания).
        /// </summary>
        public ICollection<BookAuthor> BookAuthors { get; set; } = new List<BookAuthor>();

        /// <summary>
        /// Коллекция глав книги.
        /// </summary>
        public ICollection<Chapter> Chapters { get; set; } = new List<Chapter>();

        /// <summary>
        /// Коллекция связей с тегами для этой книги (многие-ко-многим).
        /// </summary>
        public ICollection<BookTag> BookTags { get; set; } = new List<BookTag>();

        /// <summary>
        /// Коллекция пользователей, добавивших эту книгу в избранное.
        /// </summary>
        public ICollection<UserFavoriteBook> UserFavorites { get; set; } = new List<UserFavoriteBook>();
        /// <summary>
        /// Коллекция продвежения книг.
        /// </summary>
        public ICollection<Promotion> Promotions { get; set; } = new List<Promotion>();
    }
}
