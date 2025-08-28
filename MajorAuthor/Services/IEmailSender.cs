// Проект: MajorAuthor.Web
// Файл: Services/IEmailSender.cs
using System.Threading.Tasks;

namespace MajorAuthor.Services
{
    /// <summary>
    /// Интерфейс для службы отправки электронной почты.
    /// </summary>
    public interface IEmailSender
    {
        /// <summary>
        /// Отправляет электронное письмо.
        /// </summary>
        /// <param name="email">Адрес получателя.</param>
        /// <param name="subject">Тема письма.</param>
        /// <param name="htmlMessage">Содержимое письма в формате HTML.</param>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        Task SendEmailAsync(string email, string subject, string htmlMessage);
    }
}