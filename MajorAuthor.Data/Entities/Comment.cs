// Проект: MajorAuthor.Data
// Файл: Entities/Comment.cs
using System;
using System.Collections.Generic; // Добавлено для ICollection
using System.ComponentModel.DataAnnotations;

namespace MajorAuthor.Data.Entities
{
    /// <summary>
    /// Представляет комментарий к книге.
    /// </summary>
    public class Comment
    {
        /// <summary>
        /// Уникальный идентификатор комментария.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор книги, к которой оставлен комментарий.
        /// </summary>
        public int BookId { get; set; }

        /// <summary>
        /// Навигационное свойство к книге.
        /// </summary>
        public Book Book { get; set; }

        /// <summary>
        /// Идентификатор пользователя, оставившего комментарий.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Навигационное свойство к пользователю.
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// Текст комментария.
        /// </summary>
        [Required]
        [MaxLength(500)] // Ограничение на длину комментария
        public string Content { get; set; }

        /// <summary>
        /// Дата и время создания комментария.
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Идентификатор родительского комментария, если это ответ (для древовидных комментариев).
        /// Может быть null, если это комментарий верхнего уровня.
        /// </summary>
        public int? ParentCommentId { get; set; }

        /// <summary>
        /// Навигационное свойство к родительскому комментарию.
        /// </summary>
        public Comment ParentComment { get; set; }

        /// <summary>
        /// Коллекция дочерних комментариев (ответов на этот комментарий).
        /// </summary>
        public ICollection<Comment> Replies { get; set; } = new List<Comment>();
    }
}
