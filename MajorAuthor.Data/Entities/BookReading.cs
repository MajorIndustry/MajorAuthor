// Проект: MajorAuthor.Data
// Файл: Entities/BookReading.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace MajorAuthor.Data.Entities
{
    /// <summary>
    /// Представляет запись о прочтении книги пользователем.
    /// </summary>
    public class BookReading
    {
        /// <summary>
        /// Уникальный идентификатор записи.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Внешний ключ к книге.
        /// </summary>
        public int BookId { get; set; }

        /// <summary>
        /// Навигационное свойство к книге.
        /// </summary>
        public Book Book { get; set; }

        /// <summary>
        /// Внешний ключ к пользователю.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Навигационное свойство к пользователю.
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// Дата и время начала прочтения.
        /// </summary>
        public DateTime ReadDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Процент прочтения книги (от 0 до 100).
        /// </summary>
        public int ReadingProgress { get; set; }
    }
}
