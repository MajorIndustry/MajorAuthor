// Проект: MajorAuthor.Data
// Файл: Entities/BookAuthor.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MajorAuthor.Data.Entities
{
    /// <summary>
    /// Промежуточная сущность для связи многие-ко-многим между Book и Author (для совместного написания).
    /// </summary>
    public class BookAuthor
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
        /// Внешний ключ к автору.
        /// </summary>
        [Key, Column(Order = 1)]
        public int AuthorId { get; set; }

        /// <summary>
        /// Навигационное свойство к автору.
        /// </summary>
        public Author Author { get; set; }

        /// <summary>
        /// Роль автора в этой книге (например, "Основной автор", "Соавтор", "Иллюстратор").
        /// </summary>
        [MaxLength(100)]
        public string Role { get; set; } = "Автор";
    }
}
