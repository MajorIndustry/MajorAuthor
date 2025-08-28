// Проект: MajorAuthor.Data
// Файл: Entities/PoemLike.cs
// Обновлен для использования ApplicationUser.Id (string) в качестве внешнего ключа.
using System;
using System.ComponentModel.DataAnnotations;

namespace MajorAuthor.Data.Entities
{
    /// <summary>
    /// Представляет лайк, поставленный пользователем к стиху.
    /// </summary>
    public class PoemLike
    {
        /// <summary>
        /// Уникальный идентификатор лайка.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Внешний ключ к стиху, который получил лайк.
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
        /// Дата и время, когда был поставлен лайк.
        /// </summary>
        public DateTime LikeDate { get; set; } = DateTime.UtcNow;
    }
}
