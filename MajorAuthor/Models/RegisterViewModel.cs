// Проект: MajorAuthor.Web
// Файл: Models/RegisterViewModel.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication; // Для AuthenticationScheme
using System.Linq; // Для метода ToList()

namespace MajorAuthor.Models
{
    /// <summary>
    /// ViewModel для страницы регистрации.
    /// </summary>
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Поле Email является обязательным.")]
        [EmailAddress(ErrorMessage = "Пожалуйста, введите корректный адрес электронной почты.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty; // Инициализируем, чтобы избежать null

        [Required(ErrorMessage = "Поле Пароль является обязательным.")]
        [StringLength(100, ErrorMessage = "{0} должен быть не менее {2} и не более {1} символов.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = string.Empty; // Инициализируем, чтобы избежать null

        [DataType(DataType.Password)]
        [Display(Name = "Подтверждение пароля")]
        [Compare("Password", ErrorMessage = "Пароль и пароль подтверждения не совпадают.")]
        public string ConfirmPassword { get; set; } = string.Empty; // Инициализируем, чтобы избежать null

        // Сделали обнуляемыми и инициализировали пустой коллекцией, чтобы избежать ошибок "required"
        public IList<AuthenticationScheme>? ExternalLogins { get; set; } = new List<AuthenticationScheme>();

        // Сделали обнуляемым
        public string? ReturnUrl { get; set; }
    }
}
