// Проект: MajorAuthor.Data
// Файл: Entities/Tag.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MajorAuthor.Data.Entities
{
    /// <summary>
    /// Представляет тег, который может быть применен к книгам.
    /// </summary>
    public class Tag
    {
        /// <summary>
        /// Уникальный идентификатор тега.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Название тега (например, "магия", "драконы", "космос").
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// Коллекция связей с книгами, использующими этот тег.
        /// </summary>
        public ICollection<BookTag> BookTags { get; set; } = new List<BookTag>();

        /// <summary>
        /// Коллекция связей с пользователями, предпочитающими этот тег.
        /// </summary>
        public ICollection<UserPreferredTag> UserPreferredTags { get; set; } = new List<UserPreferredTag>();
    }
}
