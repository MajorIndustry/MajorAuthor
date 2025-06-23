// Проект: MajorAuthor.Web
// Файл: Models/EmailSettings.cs
using System.ComponentModel.DataAnnotations;

namespace MajorAuthor.Models
{
    /// <summary>
    /// Класс для хранения настроек SMTP-сервера для отправки электронной почты.
    /// </summary>
    public class EmailSettings
    {
        [Required]
        public string SmtpHost { get; set; } = string.Empty; // Имя или IP-адрес SMTP-сервера

        public int SmtpPort { get; set; } // Порт SMTP-сервера (например, 587 для TLS/SSL)

        [Required]
        public string SmtpUsername { get; set; } = string.Empty; // Имя пользователя для аутентификации на SMTP-сервере

        [Required]
        public string SmtpPassword { get; set; } = string.Empty; // Пароль для аутентификации на SMTP-сервере

        public bool EnableSsl { get; set; } = true; // Использовать SSL/TLS

        [Required]
        public string SenderEmail { get; set; } = string.Empty; // Адрес отправителя (ваша почта, majorauthor01@gmail.com)

        public string SenderName { get; set; } = string.Empty; // Имя отправителя, отображаемое получателю (например, "MajorAuthor")
    }
}
