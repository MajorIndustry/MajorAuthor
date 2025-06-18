// Проект: MajorAuthor.Data
// Файл: Entities/UserPreferredGenre.cs
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
        /// Внешний ключ к пользователю.
        /// </summary>
        [Key, Column(Order = 0)]
        public int UserId { get; set; }

        /// <summary>
        /// Навигационное свойство к пользователю.
        /// </summary>
        public User User { get; set; }

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
