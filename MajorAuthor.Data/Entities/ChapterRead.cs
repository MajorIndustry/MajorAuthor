// Проект: MajorAuthor.Data
// Файл: Entities/ChapterRead.cs
// НОВАЯ сущность для отслеживания прочитанных глав пользователем.
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MajorAuthor.Data.Entities
{
    /// <summary>
    /// Представляет запись о прочтении главы пользователем.
    /// </summary>
    public class ChapterRead
    {
        /// <summary>
        /// Уникальный идентификатор записи о прочтении главы.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Внешний ключ к пользователю (ApplicationUser.Id), который прочитал главу.
        /// </summary>
        [Required]
        public string ApplicationUserId { get; set; }

        /// <summary>
        /// Навигационное свойство к пользователю.
        /// </summary>
        public ApplicationUser ApplicationUser { get; set; }

        /// <summary>
        /// Внешний ключ к прочитанной главе.
        /// </summary>
        public int ChapterId { get; set; }

        /// <summary>
        /// Навигационное свойство к главе.
        /// </summary>
        public Chapter Chapter { get; set; }

        /// <summary>
        /// Дата и время первого прочтения главы.
        /// </summary>
        public DateTime FirstReadDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Дата и время последнего прочтения/взаимодействия с главой.
        /// </summary>
        public DateTime LastReadDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Прогресс чтения главы (например, 0.0 до 1.0, или просто флаг прочитано/не прочитано).
        /// Для простоты можно использовать булево поле IsCompleted.
        /// </summary>
        public bool IsCompleted { get; set; } = false;
    }
}
