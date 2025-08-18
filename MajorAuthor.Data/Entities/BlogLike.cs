// Проект: MajorAuthor.Data
// Файл: Entities/BlogLike.cs
// Представляет лайк к записи в блоге.
using System;
using System.ComponentModel.DataAnnotations;

namespace MajorAuthor.Data.Entities
{
    /// <summary>
    /// Представляет лайк к записи в блоге.
    /// </summary>
    public class BlogLike
    {
        /// <summary>
        /// Уникальный идентификатор лайка.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Внешний ключ к записи в блоге, которую лайкнули.
        /// </summary>
        [Required]
        public int BlogId { get; set; } // Изменено с BlogPostId на BlogId

        /// <summary>
        /// Навигационное свойство к записи в блоге.
        /// </summary>
        public Blog Blog { get; set; } // Изменено с BlogPost на Blog

        /// <summary>
        /// Внешний ключ к пользователю, который поставил лайк.
        /// </summary>
        [Required]
        public string ApplicationUserId { get; set; }

        /// <summary>
        /// Навигационное свойство к пользователю.
        /// </summary>
        public ApplicationUser ApplicationUser { get; set; }

        /// <summary>
        /// Дата и время, когда был поставлен лайк.
        /// </summary>
        public DateTime LikeDate { get; set; } = DateTime.UtcNow;
    }
}
