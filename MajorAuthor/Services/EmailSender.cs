// Проект: MajorAuthor.Web
// Файл: Services/EmailSender.cs
using MajorAuthor.Models; // Добавлено для EmailSettings
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options; // Добавлено для IOptions
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace MajorAuthor.Services
{
    /// <summary>
    /// Реализация службы отправки электронной почты с использованием SMTP.
    /// </summary>
    public class EmailSender : IEmailSender
    {
        private readonly ILogger<EmailSender> _logger;
        private readonly EmailSettings _emailSettings; // Добавлено для хранения настроек

        public EmailSender(ILogger<EmailSender> logger, IOptions<EmailSettings> emailSettings)
        {
            _logger = logger;
            _emailSettings = emailSettings.Value; // Получаем настройки EmailSettings
        }

        /// <summary>
        /// Отправляет электронное письмо.
        /// </summary>
        /// <param name="email">Адрес получателя.</param>
        /// <param name="subject">Тема письма.</param>
        /// <param name="htmlMessage">Содержимое письма в формате HTML.</param>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Проверяем, настроены ли SMTP-параметры
            if (string.IsNullOrEmpty(_emailSettings.SmtpHost) || string.IsNullOrEmpty(_emailSettings.SmtpUsername) || string.IsNullOrEmpty(_emailSettings.SmtpPassword))
            {
                _logger.LogWarning("Настройки SMTP не полностью сконфигурированы. Отправка Email пропущена.");
                // Можно бросить исключение или просто вернуться, если EmailSender не настроен.
                return;
            }

            try
            {
                using (var client = new SmtpClient(_emailSettings.SmtpHost, _emailSettings.SmtpPort))
                {
                    client.EnableSsl = _emailSettings.EnableSsl;
                    client.UseDefaultCredentials = false; // Важно: используем явные учетные данные
                    client.Credentials = new NetworkCredential(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword);

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                        Subject = subject,
                        Body = htmlMessage,
                        IsBodyHtml = true,
                    };
                    mailMessage.To.Add(email);

                    await client.SendMailAsync(mailMessage);
                    _logger.LogInformation($"Email успешно отправлен на {email} от {_emailSettings.SenderEmail}.");
                }
            }
            catch (SmtpException smtpEx)
            {
                _logger.LogError(smtpEx, $"Ошибка SMTP при отправке Email на {email}: {smtpEx.Message}");
                if (smtpEx.InnerException != null)
                {
                    _logger.LogError(smtpEx.InnerException, $"Внутренняя ошибка SMTP: {smtpEx.InnerException.Message}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Неизвестная ошибка при отправке Email на {email}: {ex.Message}");
            }
        }
    }
}
