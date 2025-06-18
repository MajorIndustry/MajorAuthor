// Проект: MajorAuthor.Data
// Файл: Entities/BookGenre.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MajorAuthor.Data.Entities
{
    /// <summary>
    /// Промежуточная сущность для связи многие-ко-многим между Book и Genre.
    /// </summary>
    public class BookGenre
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
        /// Внешний ключ к жанру.
        /// </summary>
        [Key, Column(Order = 1)]
        public int GenreId { get; set; }

        /// <summary>
        /// Навигационное свойство к жанру.
        /// </summary>
        public Genre Genre { get; set; }
    }
}
