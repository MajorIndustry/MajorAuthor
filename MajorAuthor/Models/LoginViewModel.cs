// Проект: MajorAuthor.Web
// Файл: Models/LoginViewModel.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using System.Linq; // Для метода ToList()

namespace MajorAuthor.Models
{
    /// <summary>
    /// ViewModel для страницы входа (логина).
    /// </summary>
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Поле Email является обязательным.")]
        [EmailAddress(ErrorMessage = "Пожалуйста, введите корректный адрес электронной почты.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty; // Инициализируем, чтобы избежать null

        [Required(ErrorMessage = "Поле Пароль является обязательным.")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = string.Empty; // Инициализируем, чтобы избежать null

        [Display(Name = "Запомнить меня")]
        public bool RememberMe { get; set; }

        // Сделали обнуляемыми и инициализировали пустой коллекцией, чтобы избежать ошибок "required"
        public IList<AuthenticationScheme>? ExternalLogins { get; set; } = new List<AuthenticationScheme>();

        // Сделали обнуляемым
        public string? ReturnUrl { get; set; }
    }
}
