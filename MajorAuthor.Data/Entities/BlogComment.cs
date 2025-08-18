// Проект: MajorAuthor.Data
// Файл: Entities/BlogComment.cs
// Представляет комментарий к записи в блоге.
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MajorAuthor.Data.Entities
{
    /// <summary>
    /// Представляет комментарий к записи в блоге.
    /// </summary>
    public class BlogComment
    {
        /// <summary>
        /// Уникальный идентификатор комментария к блогу.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Содержимое комментария.
        /// </summary>
        [Required]
        public string Content { get; set; }

        /// <summary>
        /// Дата и время создания комментария.
        /// </summary>
        public DateTime CommentDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Внешний ключ к записи в блоге, к которой относится комментарий.
        /// </summary>
        [Required]
        public int BlogId { get; set; } // Изменено с BlogPostId на BlogId

        /// <summary>
        /// Навигационное свойство к записи в блоге.
        /// </summary>
        public Blog Blog { get; set; } // Изменено с BlogPost на Blog

        /// <summary>
        /// Внешний ключ к пользователю, который оставил комментарий.
        /// </summary>
        [Required]
        public string ApplicationUserId { get; set; }

        /// <summary>
        /// Навигационное свойство к пользователю.
        /// </summary>
        public ApplicationUser ApplicationUser { get; set; }

        /// <summary>
        /// Внешний ключ к родительскому комментарию, если это ответ.
        /// </summary>
        public int? ParentCommentId { get; set; }

        /// <summary>
        /// Навигационное свойство к родительскому комментарию.
        /// </summary>
        public BlogComment ParentComment { get; set; }

        /// <summary>
        /// Коллекция ответов на этот комментарий.
        /// </summary>
        public ICollection<BlogComment> Replies { get; set; } = new List<BlogComment>();
    }
}
