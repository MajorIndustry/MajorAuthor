using System.ComponentModel.DataAnnotations;

namespace MajorAuthor.Models
{
    public class CreateBlogModel
    {
        [Required(ErrorMessage = "Название обязательно для заполнения.")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Длина названия должна быть от 3 до 200 символов.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Содержимое обязательно для заполнения.")]
        [StringLength(20000, MinimumLength = 10, ErrorMessage = "Длина содержимого должна быть от 10 до 20000 символов.")]
        public string Content { get; set; }

        public IFormFile? Photo { get; set; }
    }
}
