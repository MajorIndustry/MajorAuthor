// Проект: MajorAuthor.Data
// Файл: Entities/Chapter.cs
// Обновлен для включения коллекции ChapterRead.
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MajorAuthor.Data.Entities
{
    /// <summary>
    /// Представляет главу книги.
    /// </summary>
    public class Chapter
    {
        /// <summary>
        /// Уникальный идентификатор главы.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Название главы.
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string Title { get; set; }

        /// <summary>
        /// Порядковый номер главы в книге.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Внешний ключ к книге.
        /// </summary>
        public int BookId { get; set; }

        /// <summary>
        /// Навигационное свойство к книге.
        /// </summary>
        public Book Book { get; set; }

        /// <summary>
        /// Коллекция страниц, входящих в эту главу.
        /// </summary>
        public ICollection<Page> Pages { get; set; } = new List<Page>();

        /// <summary>
        /// Коллекция записей о прочтении этой главы пользователями.
        /// </summary>
        public ICollection<ChapterRead> ChapterReads { get; set; } = new List<ChapterRead>();
    }
}
