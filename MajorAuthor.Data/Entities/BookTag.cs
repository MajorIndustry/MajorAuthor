// Проект: MajorAuthor.Data
// Файл: Entities/BookTag.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MajorAuthor.Data.Entities
{
    /// <summary>
    /// Промежуточная сущность для связи многие-ко-многим между Book и Tag.
    /// </summary>
    public class BookTag
    {
        /// <summary>
        /// Внешний ключ к книге.
        /// </summary>
        [Key, Column(Order = 0)]
        public int BookId { get; set; }

        /// <summary>
        /// Навигационное свойство к книге.
        /// </summary>
        public Book Book { get; set; }

        /// <summary>
        /// Внешний ключ к тегу.
        /// </summary>
        [Key, Column(Order = 1)]
        public int TagId { get; set; }

        /// <summary>
        /// Навигационное свойство к тегу.
        /// </summary>
        public Tag Tag { get; set; }
    }
}
