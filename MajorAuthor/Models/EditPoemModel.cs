// Файл: Models/EditPoemModel.cs
using System.ComponentModel.DataAnnotations;

namespace MajorAuthor.Models
{
    // Используем CreatePoemModel в качестве основы, чтобы не дублировать
    // атрибуты валидации, и добавляем обязательный Id.
    public class EditPoemModel : CreatePoemModel
    {
        [Required]
        public int Id { get; set; }
    }
}