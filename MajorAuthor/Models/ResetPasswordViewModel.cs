// Проект: MajorAuthor
// Файл: Models/ResetPasswordViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace MajorAuthor.Models
{
    /// <summary>
    /// ViewModel для страницы сброса пароля.
    /// </summary>
    public class ResetPasswordViewModel
    {
        [Required(ErrorMessage = "Поле Email является обязательным.")]
        [EmailAddress(ErrorMessage = "Пожалуйста, введите корректный адрес электронной почты.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Поле Пароль является обязательным.")]
        [StringLength(100, ErrorMessage = "{0} должен быть не менее {2} и не более {1} символов.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Подтверждение пароля")]
        [Compare("Password", ErrorMessage = "Пароль и пароль подтверждения не совпадают.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        // Код сброса пароля, полученный по Email
        [Required(ErrorMessage = "Поле Code является обязательным.")]
        public string Code { get; set; } = string.Empty;
    }
}
