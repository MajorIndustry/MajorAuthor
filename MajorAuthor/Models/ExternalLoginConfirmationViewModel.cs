// Проект: MajorAuthor.Web
// Файл: Models/ExternalLoginConfirmationViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace MajorAuthor.Models
{
    /// <summary>
    /// ViewModel для страницы подтверждения внешнего логина.
    /// Используется, когда новый пользователь регистрируется через внешний провайдер.
    /// </summary>
    public class ExternalLoginConfirmationViewModel
    {
        [Required(ErrorMessage = "Поле Email является обязательным.")]
        [EmailAddress(ErrorMessage = "Пожалуйста, введите корректный адрес электронной почты.")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        // Здесь могут быть добавлены другие поля, если вы хотите собирать
        // дополнительную информацию от пользователя при первом входе через внешний провайдер.
    }
}
