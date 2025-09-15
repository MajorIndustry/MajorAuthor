// WorkViewModel.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace MajorAuthor.Models
{
    // Используем класс, чтобы он мог содержать общие данные для всех типов произведений
    public class WorkViewModel
    {
        public object Work { get; internal set; }
        public List<CommentViewModel> Comments { get; internal set; }
        public int LikesCount { get; internal set; }
        public string Type { get; internal set; }
    }
}
