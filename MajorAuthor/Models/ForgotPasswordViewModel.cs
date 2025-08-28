// Проект: MajorAuthor
// Файл: Models/ForgotPasswordViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace MajorAuthor.Models
{
    /// <summary>
    /// ViewModel для формы запроса сброса пароля.
    /// </summary>
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Поле Email является обязательным.")]
        [EmailAddress(ErrorMessage = "Пожалуйста, введите корректный адрес электронной почты.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;
    }
}
