// Проект: MajorAuthor.Data
// Файл: Entities/Page.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MajorAuthor.Data.Entities
{
    /// <summary>
    /// Представляет страницу книги. Может содержать текст, изображение или оба.
    /// </summary>
    public class Page
    {
        /// <summary>
        /// Уникальный идентификатор страницы.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Порядковый номер страницы в главе.
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Текстовое содержимое страницы. Может быть null, если страница содержит только изображение.
        /// Ограничение в 4000 символов для соответствия стандартному размеру NVARCHAR(4000) в SQL Server
        /// и для обеспечения "одинаковой длины страниц" текста.
        /// </summary>
        [MaxLength(4000)]
        public string TextContent { get; set; }

        /// <summary>
        /// URL изображения на странице (например, для манги). Может быть null, если страница содержит только текст.
        /// </summary>
        [MaxLength(1000)]
        public string ImageUrl { get; set; }

        /// <summary>
        /// Внешний ключ к главе, к которой принадлежит страница.
        /// </summary>
        public int ChapterId { get; set; }

        /// <summary>
        /// Навигационное свойство к главе.
        /// </summary>
        public Chapter Chapter { get; set; }
    }
}
