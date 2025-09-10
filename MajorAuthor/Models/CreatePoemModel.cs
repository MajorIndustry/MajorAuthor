using System.ComponentModel.DataAnnotations;

namespace MajorAuthor.Models
{
    public class CreatePoemModel
    {
        [Required(ErrorMessage = "Название обязательно для заполнения.")]
        [StringLength(500, ErrorMessage = "Название не может превышать 500 символов.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Текст стихотворения обязателен.")]
        public string Content { get; set; }
    }
}
