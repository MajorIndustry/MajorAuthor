// BookReadViewModel.cs
using MajorAuthor.Data.Entities;
using MajorAuthor.Services;
using System.Collections.Generic;

namespace MajorAuthor.Models
{
    public class BookReadViewModel
    {
        public Book Book { get; set; }
        public List<Chapter> PublishedChapters { get; set; } = new List<Chapter>();
        public List<Book> SimilarBooks { get; set; } = new List<Book>();
        public BookCycle Cycle { get; set; }
        public List<Book> CycleBooks { get; set; } = new List<Book>();
        public List<CommentViewModel> Comments { get; set; } = new List<CommentViewModel>();
        public bool HasLiked { get; set; }
        public bool IsAuthor { get; set; }
        public bool IsInFavorites { get; set; }
        public string NewComment { get; set; }
        public List<Author> Authors { get; set; } = new List<Author>();
        public List<Genre> Genres { get; set; } = new List<Genre>();
        public List<Tag> Tags { get; set; } = new List<Tag>();
        public BookStats Stats { get; set; }
        public int? ContinueReadingChapterId { get; set; }
        public HashSet<int> ReadChapterIds { get; set; } = new HashSet<int>();
        public bool HasReadingProgress { get; set; }

    }
}