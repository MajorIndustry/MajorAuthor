// Проект: MajorAuthor.Data
// Файл: Entities/PoemComment.cs
// Обновлен для использования ApplicationUser.Id (string) в качестве внешнего ключа.
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MajorAuthor.Data.Entities
{
    /// <summary>
    /// Представляет комментарий к стиху.
    /// </summary>
    public class PoemComment
    {
        /// <summary>
        /// Уникальный идентификатор комментария.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Внешний ключ к стиху, к которому оставлен комментарий.
        /// </summary>
        public int PoemId { get; set; }

        /// <summary>
        /// Навигационное свойство к стиху.
        /// </summary>
        public Poem Poem { get; set; }

        /// <summary>
        /// Внешний ключ к пользователю (ApplicationUser.Id).
        /// </summary>
        [Required]
        public string ApplicationUserId { get; set; }

        /// <summary>
        /// Навигационное свойство к пользователю.
        /// </summary>
        public ApplicationUser ApplicationUser { get; set; }

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
        public PoemComment ParentComment { get; set; }

        /// <summary>
        /// Коллекция дочерних комментариев (ответов на этот комментарий).
        /// </summary>
        public ICollection<PoemComment> Replies { get; set; } = new List<PoemComment>();
    }
}
