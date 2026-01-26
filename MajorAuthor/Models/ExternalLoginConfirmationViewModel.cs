// Project: MajorAuthor.Web
// File: Models/AccountViewModels.cs (фрагмент)

using System.ComponentModel.DataAnnotations;

public class ExternalLoginConfirmationViewModel
{
    // 💡 Ключевое поле: Email, который вводит пользователь
    [Required(ErrorMessage = "Введите Email")]
    [EmailAddress(ErrorMessage = "Некорректный формат Email")]
    [Display(Name = "Email")]
    public string Email { get; set; }

    // 💡 Обязательное поле для перенаправления после успеха
    public string? ReturnUrl { get; set; }
    public string TempDataKey { get; set; } = string.Empty;
}