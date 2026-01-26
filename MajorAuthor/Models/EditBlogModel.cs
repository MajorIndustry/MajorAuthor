using System.ComponentModel.DataAnnotations;

namespace MajorAuthor.Models
{
    public class EditBlogModel:CreateBlogModel
    {
        [Required]
        public int Id { get; set; } // Идентификатор блога, который мы редактируем
        // URL существующего изображения для отображения в форме
        public string? ExistingImageUrl { get; set; }

        // Флаг для удаления существующего изображения
        public bool RemoveExistingPhoto { get; set; }
    }
}
