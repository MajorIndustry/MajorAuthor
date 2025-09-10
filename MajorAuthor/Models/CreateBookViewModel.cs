// Проект: MajorAuthor.Models
// Файл: CreateBookViewModel.cs
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace MajorAuthor.Models
{
    /// <summary>
    /// ViewModel для формы создания книги.
    /// </summary>
    public class CreateBookViewModel
    {
        [Required(ErrorMessage = "Название обязательно.")]
        [StringLength(500, ErrorMessage = "Длина названия не должна превышать 500 символов.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Описание обязательно.")]
        public string Description { get; set; }

        public IFormFile? CoverImage { get; set; }

        public bool IsAdultContent { get; set; }

        [EmailAddress(ErrorMessage = "Некорректный формат email.")]
        public string? CoAuthorEmail { get; set; }
    }
}
