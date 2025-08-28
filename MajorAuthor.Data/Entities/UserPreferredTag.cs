// Проект: MajorAuthor.Data
// Файл: Entities/UserPreferredTag.cs
// Обновлен для использования ApplicationUser.Id (string) в качестве внешнего ключа.
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MajorAuthor.Data.Entities
{
    /// <summary>
    /// Представляет предпочтительный тег пользователя.
    /// </summary>
    public class UserPreferredTag
    {
        /// <summary>
        /// Внешний ключ к пользователю (ApplicationUser.Id).
        /// </summary>
        [Key, Column(Order = 0)]
        public string ApplicationUserId { get; set; }

        /// <summary>
        /// Навигационное свойство к пользователю.
        /// </summary>
        public ApplicationUser ApplicationUser { get; set; }

        /// <summary>
        /// Внешний ключ к тегу.
        /// </summary>
        [Key, Column(Order = 1)]
        public int TagId { get; set; }

        /// <summary>
        /// Навигационное свойство к тегу.
        /// </summary>
        public Tag Tag { get; set; }

        /// <summary>
        /// Уровень предпочтения (например, 1-5 или количество взаимодействий с книгами с этим тегом).
        /// </summary>
        public int PreferenceLevel { get; set; }
    }
}
