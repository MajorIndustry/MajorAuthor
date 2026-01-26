// Проект: MajorAuthor.Data
// Файл: Entities/Poem.cs
// Обновлен для включения поля Status.
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MajorAuthor.Data.Entities
{
    /// <summary>
    /// Представляет стих, написанный автором.
    /// </summary>
    [Microsoft.EntityFrameworkCore.Index(nameof(ContentHash), IsUnique = true)]
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
        [Column(TypeName = "nvarchar(max)")]
        public string Content { get; set; }

        [Required]
        [MaxLength(64)]
        public string ContentHash { get; set; }
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
        public double Rating { get; set; } = 0;
        public double WeeklyRating { get; set; } = 0;
        public double MonthlyRating { get; set; } = 0;
        public double YearlyRating { get; set; } = 0;

        /// <summary>
        /// Коллекция лайков этого стиха.
        /// </summary>
        public ICollection<PoemLike> Likes { get; set; } = new List<PoemLike>();

        /// <summary>
        /// Коллекция комментариев к этому стиху.
        /// </summary>
        public ICollection<PoemComment> Comments { get; set; } = new List<PoemComment>();

        public ICollection<PoemReading> Readings { get; set; } = new List<PoemReading>();
        /// <summary>
        /// Количество просмотров записи.
        /// </summary>
        public int ViewsCount { get; set; } = 0;
    }
}
