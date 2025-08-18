// Проект: MajorAuthor.Data
// Файл: Entities/Poem.cs
// Обновлен для включения поля Status.
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MajorAuthor.Data.Entities
{
    /// <summary>
    /// Представляет стих, написанный автором.
    /// </summary>
    public class Poem
    {
        /// <summary>
        /// Уникальный идентификатор стиха.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Название стиха.
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string Title { get; set; }

        /// <summary>
        /// Текст стиха.
        /// </summary>
        [Required]
        public string Content { get; set; }

        /// <summary>
        /// Внешний ключ к автору, который написал стих.
        /// </summary>
        public int AuthorId { get; set; }

        /// <summary>
        /// Навигационное свойство к автору стиха.
        /// </summary>
        public Author Author { get; set; }

        /// <summary>
        /// Дата публикации стиха.
        /// </summary>
        public DateTime PublicationDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Статус публикации стиха (например, "опубликовано", "черновик", "архив").
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "draft"; // Значение по умолчанию "черновик"

        /// <summary>
        /// Количество лайков стиха.
        /// </summary>
        public int LikesCount { get; set; } = 0;

        /// <summary>
        /// Количество комментариев к стиху.
        /// </summary>
        public int CommentsCount { get; set; } = 0;

        /// <summary>
        /// Коллекция лайков этого стиха.
        /// </summary>
        public ICollection<PoemLike> Likes { get; set; } = new List<PoemLike>();

        /// <summary>
        /// Коллекция комментариев к этому стиху.
        /// </summary>
        public ICollection<PoemComment> Comments { get; set; } = new List<PoemComment>();
    }
}
