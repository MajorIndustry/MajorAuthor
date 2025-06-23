// Проект: MajorAuthor.Data
// Файл: Entities/Author.cs
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System; // Для DateTime

namespace MajorAuthor.Data.Entities
{
    public class Author
    {
        public int Id { get; set; }

        // Новое: Свойство для псевдонима автора
        [StringLength(100, ErrorMessage = "Псевдоним не может превышать 100 символов.")]
        [Display(Name = "Псевдоним")]
        public string? PenName { get; set; } // Добавлено

        [StringLength(255, ErrorMessage = "Полное имя не может превышать 255 символов.")]
        [Display(Name = "Полное имя (ФИО)")]
        public string? FullName { get; set; } // Сделали nullable

        // Новое: Дата создания профиля автора (для "Новые популярные авторы")
        public DateTime AuthorProfileCreationDate { get; set; } = DateTime.UtcNow; // Добавлено

        // Новое: URL фотографии автора
        [MaxLength(500)]
        public string? PhotoUrl { get; set; } // Добавлено/Убедился в наличии

        // Внешний ключ для связи с ApplicationUser
        public string? IdentityUserId { get; set; }

        // Новое: Навигационное свойство для связи один-к-одному с ApplicationUser
        public ApplicationUser? User { get; set; }


        // Навигационное свойство для книг, которые написал автор (многие-ко-многим)
        public ICollection<BookAuthor> BookAuthors { get; set; } = new List<BookAuthor>();

        // Метод валидации на уровне сущности (можно использовать в сервисе/контроллере)
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(FullName) || !string.IsNullOrWhiteSpace(PenName);
        }

        // Вы можете добавить этот метод, чтобы получить отображаемое имя автора
        public string DisplayName =>
            !string.IsNullOrWhiteSpace(PenName) ? PenName :
            !string.IsNullOrWhiteSpace(FullName) ? FullName : "Неизвестный автор";
    }
}
