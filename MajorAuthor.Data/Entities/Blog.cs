// Проект: MajorAuthor.Data
// Файл: Entities/Blog.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MajorAuthor.Data.Entities
{
    /// <summary>
    /// Представляет отдельную запись в блоге.
    /// </summary>
    public class Blog
    {
        /// <summary>
        /// Уникальный идентификатор записи блога.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор автора, которому принадлежит запись блога.
        /// </summary>
        [Required]
        public int AuthorId { get; set; }

        /// <summary>
        /// Навигационное свойство к автору.
        /// </summary>
        public Author Author { get; set; }

        /// <summary>
        /// Заголовок записи блога.
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string Title { get; set; }

        /// <summary>
        /// Содержимое записи блога.
        /// </summary>
        [Required]
        public string Content { get; set; } // Для больших текстов без ограничения длины

        /// <summary>
        /// URL изображения, связанного с записью блога (опционально).
        /// </summary>
        [MaxLength(1000)]
        public string ImageUrl { get; set; }

        /// <summary>
        /// Дата и время публикации записи.
        /// </summary>
        public DateTime PublicationDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Количество просмотров записи.
        /// </summary>
        public int ViewsCount { get; set; } = 0;

        /// <summary>
        /// Количество лайков записи блога.
        /// </summary>
        public int LikesCount { get; set; } = 0;

        /// <summary>
        /// Количество комментариев к записи блога.
        /// </summary>
        public int CommentsCount { get; set; } = 0;

        /// <summary>
        /// Коллекция лайков этой записи блога.
        /// </summary>
        public ICollection<BlogLike> Likes { get; set; } = new List<BlogLike>();

        /// <summary>
        /// Коллекция комментариев к этой записи блога.
        /// </summary>
        public ICollection<BlogComment> Comments { get; set; } = new List<BlogComment>();
    }
}
