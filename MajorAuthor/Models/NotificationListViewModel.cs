// ViewModels/NotificationListViewModel.cs
using MajorAuthor.Models;
using System.Collections.Generic;

namespace MajorAuthor.ViewModels
{
    public class NotificationListViewModel
    {
        public List<NotificationViewModel> Notifications { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int UnreadCount { get; set; }
    }
}