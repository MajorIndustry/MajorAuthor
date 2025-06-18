// Проект: MajorAuthor.Data
// Файл: Entities/BookLike.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace MajorAuthor.Data.Entities
{
    /// <summary>
    /// Представляет запись о лайке книги пользователем.
    /// </summary>
    public class BookLike
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
        /// Дата и время, когда был поставлен лайк.
        /// </summary>
        public DateTime LikeDate { get; set; } = DateTime.UtcNow;
    }
}
