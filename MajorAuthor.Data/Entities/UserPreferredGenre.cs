// Проект: MajorAuthor.Data
// Файл: Entities/UserPreferredGenre.cs
// Обновлен для использования ApplicationUser.Id (string) в качестве внешнего ключа.
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MajorAuthor.Data.Entities
{
    /// <summary>
    /// Представляет предпочтительный жанр пользователя.
    /// Можно использовать для хранения явных предпочтений или рекомендаций.
    /// </summary>
    public class UserPreferredGenre
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
        /// Внешний ключ к жанру.
        /// </summary>
        [Key, Column(Order = 1)]
        public int GenreId { get; set; }

        /// <summary>
        /// Навигационное свойство к жанру.
        /// </summary>
        public Genre Genre { get; set; }

        /// <summary>
        /// Уровень предпочтения (например, 1-5 или количество прочтений книг этого жанра).
        /// </summary>
        public int PreferenceLevel { get; set; }
    }
}
