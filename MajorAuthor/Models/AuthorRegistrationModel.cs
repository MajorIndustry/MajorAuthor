using System.ComponentModel.DataAnnotations;

namespace MajorAuthor.Models
{
    public class AuthorRegistrationModel
    {
        [Required(ErrorMessage = "Имя или псевдоним обязательны для заполнения.")]
        [StringLength(100, ErrorMessage = "Имя или псевдоним не может превышать 100 символов.")]
        public string PenName { get; set; }
    }
}
