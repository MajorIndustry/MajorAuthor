// Проект: MajorAuthor.Data
// Файл: Entities/Genre.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MajorAuthor.Data.Entities
{
    /// <summary>
    /// Представляет жанр книги.
    /// </summary>
    public class Genre
    {
        /// <summary>
        /// Уникальный идентификатор жанра.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Название жанра (например, "Фантастика", "Детектив").
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// Коллекция связей с книгами (многие-ко-многим).
        /// </summary>
        public ICollection<BookGenre> BookGenres { get; set; } = new List<BookGenre>();
    }
}
