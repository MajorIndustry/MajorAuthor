// Проект: MajorAuthor.Data
// Файл: Entities/Follower.cs
// Обновлен для использования ApplicationUser.Id (string) в качестве внешнего ключа.
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MajorAuthor.Data.Entities
{
    /// <summary>
    /// Представляет подписку пользователя на автора.
    /// </summary>
    public class Follower
    {
        /// <summary>
        /// Внешний ключ к пользователю, который подписался (ApplicationUser.Id).
        /// </summary>
        [Key, Column(Order = 0)]
        public string FollowerApplicationUserId { get; set; }

        /// <summary>
        /// Навигационное свойство к пользователю, который подписался.
        /// </summary>
        public ApplicationUser FollowerApplicationUser { get; set; }

        /// <summary>
        /// Внешний ключ к автору, на которого подписались (Author.Id).
        /// </summary>
        [Key, Column(Order = 1)]
        public int AuthorId { get; set; }

        /// <summary>
        /// Навигационное свойство к автору, на которого подписались.
        /// </summary>
        public Author Author { get; set; }

        /// <summary>
        /// Дата подписки.
        /// </summary>
        public DateTime FollowDate { get; set; } = DateTime.UtcNow;
    }
}
