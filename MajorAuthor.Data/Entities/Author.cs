// Проект: MajorAuthor.Data
// Файл: Entities/Author.cs
// Обновлен для использования ApplicationUser.Id (string) в качестве внешнего ключа.
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MajorAuthor.Data.Entities
{
    /// <summary>
    /// Представляет профиль автора, связанный с пользователем ApplicationUser.
    /// Каждый автор обязательно является пользователем.
    /// </summary>
    public class Author
    {
        /// <summary>
        /// Уникальный идентификатор автора (первичный ключ сущности Author).
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Внешний ключ к ApplicationUser.Id (IdentityUser.Id).
        /// </summary>
        [Required]
        public string ApplicationUserId { get; set; }

        /// <summary>
        /// Навигационное свойство к связанному объекту ApplicationUser.
        /// </summary>
        public ApplicationUser ApplicationUser { get; set; }

        /// <summary>
        /// Псевдоним автора (может отличаться от имени пользователя).
        /// </summary>
        [Required]
        [MaxLength(255)]
        public string PenName { get; set; }

        /// <summary>
        /// ФИО автора
        /// </summary>
        [Required]
        public string FullName { get; set; }

        /// <summary>
        /// Дата создания авторского профиля.
        /// </summary>
        public DateTime AuthorProfileCreationDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Описание автора.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// URL фотографии автора (опционально).
        /// </summary>
        [MaxLength(500)]
        public string PhotoUrl { get; set; }

        /// <summary>
        /// Коллекция связей с книгами, в которых этот автор участвовал (для совместного написания).
        /// </summary>
        public ICollection<BookAuthor> BookAuthors { get; set; } = new List<BookAuthor>();

        /// <summary>
        /// Коллекция стихов, написанных этим автором.
        /// </summary>
        public ICollection<Poem> Poems { get; set; } = new List<Poem>();

        /// <summary>
        /// Коллекция пользователей, которые подписаны на этого автора.
        /// </summary>
        public ICollection<Follower> Followers { get; set; } = new List<Follower>();

        /// <summary>
        /// Навигационное свойство к блогу автора (если есть).
        /// </summary>
        public ICollection<Blog> Blogs { get; set; } = new List<Blog>();
    }
}
