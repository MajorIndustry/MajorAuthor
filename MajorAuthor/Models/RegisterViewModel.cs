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
        [Required(ErrorMessage = "Поле Логин является обязательным.")]
        [StringLength(100, ErrorMessage = "{0} должен быть не более {1} символов.")]
        [Display(Name = "Логин")]
        public string UserName { get; set; } = string.Empty;
        [Display(Name = "Имя")]
        public string? FirstName { get; set; }
        [Display(Name = "Фамилия")]
        public string? LastName { get; set; }
        [Required(ErrorMessage = "Поле Email является обязательным.")]
        [EmailAddress(ErrorMessage = "Пожалуйста, введите корректный адрес электронной почты.")]
        [StringLength(200, ErrorMessage = "{0} должен быть не более {1} символов.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty; // Инициализируем, чтобы избежать null

        [Required(ErrorMessage = "Поле Пароль является обязательным.")]
        [StringLength(100, ErrorMessage = "{0} должен быть не менее {2} и не более {1} символов.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        [RegularExpression("(?=.*[A-Z])(?=.*[a-z])(?=.*\\d)(?=.*[^A-Za-z0-9]).{6,100}", ErrorMessage = "Пароль должен содержать хотя бы 1 заглавную букву, 1 строчную букву, 1 цифру и 1 специальный символ.")]
        public string Password { get; set; } = string.Empty; // Инициализируем, чтобы избежать null

        [Required(ErrorMessage = "Поле Подтверждение пароля является обязательным.")]
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
