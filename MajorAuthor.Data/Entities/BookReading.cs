// Проект: MajorAuthor.Data
// Файл: Entities/BookReading.cs
// Обновлен для использования ApplicationUser.Id (string) в качестве внешнего ключа.
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
        /// Внешний ключ к пользователю (ApplicationUser.Id).
        /// </summary>
        [Required]
        public string ApplicationUserId { get; set; }

        /// <summary>
        /// Навигационное свойство к пользователю.
        /// </summary>
        public ApplicationUser ApplicationUser { get; set; }

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
