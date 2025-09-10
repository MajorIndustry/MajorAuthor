using System.ComponentModel.DataAnnotations;

namespace MajorAuthor.Models
{
    public class CreateBlogModel
    {
        [Required(ErrorMessage = "Заголовок обязателен")]
        [StringLength(500, ErrorMessage = "Длина заголовка не может превышать 500 символов")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Содержимое блога обязательно")]
        public string Content { get; set; }
        // Новое свойство для загрузки файла
        public IFormFile Photo { get; set; }
    }
}
