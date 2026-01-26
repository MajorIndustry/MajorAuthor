using System;
using System.ComponentModel.DataAnnotations;

namespace MajorAuthor.Data.Entities
{
    /// <summary>
    /// Представляет запись о прочтении блога пользователем.
    /// </summary>
    public class BlogReading
    {
        /// <summary>
        /// Уникальный идентификатор записи.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Внешний ключ к блогу.
        /// </summary>
        public int BlogId { get; set; }

        /// <summary>
        /// Навигационное свойство к блогу.
        /// </summary>
        public Blog Blog { get; set; }

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