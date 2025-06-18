// Проект: MajorAuthor.Data
// Файл: Entities/Message.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace MajorAuthor.Data.Entities
{
    /// <summary>
    /// Представляет личное сообщение между пользователями.
    /// </summary>
    public class Message
    {
        /// <summary>
        /// Уникальный идентификатор сообщения.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор отправителя сообщения.
        /// </summary>
        public int SenderId { get; set; }

        /// <summary>
        /// Навигационное свойство к отправителю.
        /// </summary>
        public User Sender { get; set; }

        /// <summary>
        /// Идентификатор получателя сообщения.
        /// </summary>
        public int ReceiverId { get; set; }

        /// <summary>
        /// Навигационное свойство к получателю.
        /// </summary>
        public User Receiver { get; set; }

        /// <summary>
        /// Текст сообщения.
        /// </summary>
        [Required]
        [MaxLength(1000)] // Ограничение на длину сообщения
        public string Content { get; set; }

        /// <summary>
        /// Дата и время отправки сообщения.
        /// </summary>
        public DateTime SentDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Флаг, указывающий, было ли сообщение прочитано получателем.
        /// </summary>
        public bool IsRead { get; set; } = false;
    }
}
