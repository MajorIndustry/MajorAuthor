// Проект: MajorAuthor
// Файл: Models/ResendEmailConfirmationViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace MajorAuthor.Models
{
    /// <summary>
    /// ViewModel для формы повторной отправки подтверждения Email.
    /// </summary>
    public class ResendEmailConfirmationViewModel
    {
        [Required(ErrorMessage = "Поле Email является обязательным.")]
        [EmailAddress(ErrorMessage = "Пожалуйста, введите корректный адрес электронной почты.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;
    }
}
