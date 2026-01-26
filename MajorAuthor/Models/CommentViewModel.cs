// CommentViewModel.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace MajorAuthor.Models
{
    public class CommentViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Поле 'Текст комментария' обязательно для заполнения.")]
        [StringLength(500, ErrorMessage = "Текст комментария не может быть длиннее 500 символов.")]
        public string Text { get; set; }

        public DateTime CreationDate { get; set; }

        public string UserName { get; set; }
        public string Content { get;  set; }
        public string UserId { get;  set; }
        public DateTime Timestamp { get;  set; }
        public string? UserAvatar { get; internal set; }
        public List<CommentViewModel> Replies { get; internal set; }
    }
}
