// Models/CreateBookViewModel.cs
using MajorAuthor.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace MajorAuthor.Models
{
    public class CreateBookViewModel
    {
        [Required(ErrorMessage = "Название книги обязательно")]
        [Display(Name = "Название книги")]
        public string Title { get; set; }

        [Display(Name = "Описание")]
        public string Description { get; set; }

        [Display(Name = "Обложка книги")]
        public IFormFile? CoverImage { get; set; }

        [Display(Name = "Контент 18+")]
        public bool IsAdultContent { get; set; }

        [Display(Name = "Email соавтора")]
        [EmailAddress(ErrorMessage = "Введите корректный email адрес")]
        public string? CoAuthorEmail { get; set; }

        // Новые свойства для типа, жанров и тегов
        [Required(ErrorMessage = "Тип книги обязателен")]
        [Display(Name = "Тип книги")]
        public int TypeId { get; set; }

        [Display(Name = "Жанры")]
        public List<int> SelectedGenreIds { get; set; } = new List<int>();

        [Display(Name = "Теги")]
        public List<string> Tags { get; set; } = new List<string>();

        // Списки для заполнения выпадающих списков
        public List<BookType> BookTypes { get; set; } = new List<BookType>();
        public List<Genre> Genres { get; set; } = new List<Genre>();
    }
}