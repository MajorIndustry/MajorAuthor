using System.ComponentModel.DataAnnotations;

namespace MajorAuthor.Models
{
    public class CreatePoemModel
    {
        [Required(ErrorMessage = "Название обязательно для заполнения.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Длина названия должна быть от 3 до 100 символов.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Текст стихотворения обязателен.")]
        [StringLength(10000, MinimumLength = 10, ErrorMessage = "Длина текста должна быть от 10 до 10000 символов.")]
        public string Content { get; set; }
    }
}
