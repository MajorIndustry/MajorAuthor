using System;
using System.ComponentModel.DataAnnotations;

namespace MajorAuthor.Data.Entities
{
    /// <summary>
    /// Представляет запись о прочтении стиха пользователем.
    /// </summary>
    public class PoemReading
    {
        /// <summary>
        /// Уникальный идентификатор записи.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Внешний ключ к стиху.
        /// </summary>
        public int PoemId { get; set; }

        /// <summary>
        /// Навигационное свойство к стиху.
        /// </summary>
        public Poem Poem { get; set; }

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
        /// Дата и время прочтения.
        /// </summary>
        public DateTime ReadDate { get; set; } = DateTime.UtcNow;
    }
}