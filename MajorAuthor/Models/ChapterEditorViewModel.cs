// Models/ChapterEditorViewModel.cs
using MajorAuthor.Data.Entities;

namespace MajorAuthor.Models
{
    public class ChapterEditorViewModel
    {
        public int BookId { get; set; }
        public int? ChapterId { get; set; }
        public string BookTitle { get; set; }
        public string ChapterTitle { get; set; }

        // Для упрощения редактирования считаем, что текст хранится 
        // в первой странице или склеивается.
        public string Content { get; set; }
        public List<ChapterImagePage> ImagePages { get; set; } = new List<ChapterImagePage>();
        public bool IsPublic { get; set; }
        public int Order { get; set; }

        // Список активных соавторов для отображения в UI
        public List<BookAuthorInfo> CoAuthors { get; set; } = new List<BookAuthorInfo>();

        public string CurrentUserId { get; set; }
        public string CurrentUserPenName { get; set; }
        public string UserColor { get; set; }
    }
    public class ChapterImagePage
    {
        public int Id { get; set; }
        public int PageNumber { get; set; }
        public string ImageUrl { get; set; } = "";
        public string FileName { get; set; } = "";
    }
}