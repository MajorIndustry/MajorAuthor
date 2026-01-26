using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MajorAuthor.Models
{
    public class BookDashboardViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Название обязательно")]
        [StringLength(150, ErrorMessage = "Название не должно превышать 150 символов")]
        [Display(Name = "Название")]
        public string Title { get; set; }

        [StringLength(2000, ErrorMessage = "Описание не должно превышать 2000 символов")]
        [Display(Name = "Описание")]
        public string Description { get; set; }

        [Display(Name = "URL обложки")]
        public string CoverImageUrl { get; set; }

        [Display(Name = "Обложка")]
        public IFormFile CoverImage { get; set; }

        [Display(Name = "Контент 18+")]
        public bool IsAdultContent { get; set; }

        public bool EnableTTS { get; set; }

        public bool AllowDownload { get; set; }

        [Display(Name = "Дата публикации")]
        public DateTime PublicationDate { get; set; }

        [Display(Name = "Последнее обновление")]
        public DateTime LastUpdate { get; set; }

        // Эти свойства можно изменять через форму
        [Display(Name = "Статус")]
        public string Status { get; set; }

        [Display(Name = "Тип произведения")]
        public string WorkType { get; set; }

        [Required(ErrorMessage = "Тип произведения обязателен")]
        [Display(Name = "Тип произведения")]
        public int TypeId { get; set; }

        [Display(Name = "Цикл")]
        public string Cycle { get; set; }

        [Display(Name = "Цикл")]
        public int? CycleId { get; set; }

        [Display(Name = "Жанры")]
        public string Genre { get; set; }

        // Статистика (только для чтения)
        public int TotalCharacters { get; set; }
        public int TotalPages { get; set; }
        public int CharactersUntilNextUpdate { get; set; }
        public int PagesUntilNextUpdate { get; set; }

        // Основной автор (текущий пользователь)
        [Display(Name = "Основной автор")]
        public string CurrentAuthorName { get; set; }
        public int CurrentAuthorId { get; set; }

        // Добавленные свойства для исправления ошибок
        public int AuthorId { get; set; }
        public string AuthorName { get; set; }
        public List<BookAuthorInfo> CoAuthors { get; set; } = new List<BookAuthorInfo>();

        // Все авторы книги
        public List<BookAuthorInfo> Authors { get; set; } = new List<BookAuthorInfo>();

        // Коллекции
        public List<ChapterViewModel> Chapters { get; set; } = new List<ChapterViewModel>();
        public List<string> Tags { get; set; } = new List<string>();

        // Списки для выбора
        public List<BookTypeInfo> AllTypes { get; set; } = new List<BookTypeInfo>();
        public List<BookCycleInfo> AllCycles { get; set; } = new List<BookCycleInfo>();
        public List<GenreInfo> AllGenres { get; set; } = new List<GenreInfo>();
        public List<BookStatusInfo> AllStatuses { get; set; } = new List<BookStatusInfo>();

        // Новые свойства
        public bool IsPublic { get; set; }
        public int LikesCount { get; set; }
        public int ViewsCount { get; set; }

        // Для работы с тегами как строкой в форме
        [Display(Name = "Теги")]
        public string TagsString { get; set; }

        // НОВОЕ: Статистика по годам
        public List<YearlyStat> YearlyStats { get; set; } = new List<YearlyStat>();

        // НОВОЕ: Флаг удаления обложки
        public bool RemoveCover { get; set; }
    }

    // НОВЫЙ КЛАСС: Статистика по годам
    public class YearlyStat
    {
        public int Year { get; set; }
        public int Views { get; set; }
        public int Likes { get; set; }
        public int Reads { get; set; }
    }

    // Остальные существующие классы остаются без изменений...
    public class BookTypeInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class BookCycleInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class BookStatusInfo
    {
        public string Value { get; set; }
        public string Name { get; set; }
    }

    public class GenreInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class ChapterViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int Order { get; set; }
        public bool IsPublic { get; set; }
        public int CharacterCount { get; set; }
        public DateTime PublicationDate { get; set; }
    }

    public class BookAuthorInfo
    {
        public int Id { get; set; }
        public string PenName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; } = "Автор";
        public bool IsCurrentUser { get; set; }
    }
}