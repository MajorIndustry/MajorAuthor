// Проект: MajorAuthor.Data
// Файл: Entities/UserFavoriteBook.cs
// Обновлен для использования ApplicationUser.Id (string) в качестве внешнего ключа.
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MajorAuthor.Data.Entities
{
    /// <summary>
    /// Промежуточная сущность для связи многие-ко-многим между ApplicationUser и Book для избранных книг.
    /// </summary>
    public class UserFavoriteBook
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
        /// Внешний ключ к книге.
        /// </summary>
        [Key, Column(Order = 1)]
        public int BookId { get; set; }

        /// <summary>
        /// Навигационное свойство к книге.
        /// </summary>
        public Book Book { get; set; }

        /// <summary>
        /// Дата, когда книга была добавлена в избранное.
        /// </summary>
        public DateTime AddedDate { get; set; } = DateTime.UtcNow;
    }
}
