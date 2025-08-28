// Проект: MajorAuthor.Data
// Файл: Entities/Notification.cs
// Обновлен для использования ApplicationUser.Id (string) в качестве внешнего ключа.
using System;
using System.ComponentModel.DataAnnotations;

namespace MajorAuthor.Data.Entities
{
    /// <summary>
    /// Представляет уведомление для пользователя.
    /// </summary>
    public class Notification
    {
        /// <summary>
        /// Уникальный идентификатор уведомления.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Внешний ключ к пользователю, получающему уведомление (ApplicationUser.Id).
        /// </summary>
        [Required]
        public string ApplicationUserId { get; set; }

        /// <summary>
        /// Навигационное свойство к пользователю.
        /// </summary>
        public ApplicationUser ApplicationUser { get; set; }

        /// <summary>
        /// Текст уведомления.
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string Message { get; set; }

        /// <summary>
        /// Дата и время создания уведомления.
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Флаг, указывающий, было ли уведомление прочитано.
        /// </summary>
        public bool IsRead { get; set; } = false;

        /// <summary>
        /// Тип уведомления (например, "новый_комментарий", "новый_лайк", "новое_сообщение").
        /// </summary>
        [MaxLength(100)]
        public string Type { get; set; }

        /// <summary>
        /// URL, на который можно перейти по уведомлению (опционально).
        /// </summary>
        [MaxLength(1000)]
        public string TargetUrl { get; set; }
    }
}
