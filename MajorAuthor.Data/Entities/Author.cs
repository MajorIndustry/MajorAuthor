// Проект: MajorAuthor.Data
// Файл: Entities/Author.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MajorAuthor.Data.Entities
{
    /// <summary>
    /// Представляет профиль автора, связанный с пользователем.
    /// Каждый автор обязательно является пользователем.
    /// </summary>
    public class Author
    {
        /// <summary>
        /// Уникальный идентификатор автора.
        /// Является одновременно первичным ключом для Author и внешним ключом к User.Id.
        /// </summary>
        [Key]
        [ForeignKey("User")] // Указывает, что это внешний ключ к сущности User
        public int Id { get; set; }

        /// <summary>
        /// Навигационное свойство к связанному объекту User.
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// Псевдоним автора (может отличаться от имени пользователя).
        /// </summary>
        [Required]
        [MaxLength(255)]
        public string PenName { get; set; }

        /// <summary>
        /// Дата создания авторского профиля. Отличается от даты регистрации User.
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
    }
}
