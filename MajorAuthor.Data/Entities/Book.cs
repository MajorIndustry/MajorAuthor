// Проект: MajorAuthor.Data
// Файл: Entities/Book.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MajorAuthor.Data.Entities
{
    /// <summary>
    /// Представляет книгу.
    /// </summary>
    public class Book
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(500)]
        public string Title { get; set; }

        public string Description { get; set; }
        public DateTime PublicationDate { get; set; } = DateTime.UtcNow;

        [MaxLength(500)]
        public string? CoverImageUrl { get; set; }

        public int LikesCount { get; set; } = 0;
        public int ReadsCount { get; set; } = 0;
        public DateTime LastUpdateTime { get; set; } = DateTime.UtcNow;

        public bool IsAdultContent { get; set; } = false;
        public bool IsPublic { get; set; } = false;
        public bool EnableTTS { get; set; } = false;
        public bool AllowDownload { get; set; } = false;

        // Внешние ключи
        public int TypeId { get; set; }
        public BookType Type { get; set; }

        public int? CycleId { get; set; }
        public BookCycle? Cycle { get; set; }

        public int StatusId { get; set; } = 1;
        public BookStatus Status { get; set; }
        public double Rating { get; set; } = 0;
        public double WeeklyRating { get; set; } = 0;
        public double MonthlyRating { get; set; } = 0;
        public double YearlyRating { get; set; } = 0;

        // Навигационные свойства
        public ICollection<BookReading> Readings { get; set; } = new List<BookReading>();
        public ICollection<BookLike> Likes { get; set; } = new List<BookLike>();
        public ICollection<BookGenre> BookGenres { get; set; } = new List<BookGenre>();
        public ICollection<BookAuthor> BookAuthors { get; set; } = new List<BookAuthor>();
        public ICollection<Chapter> Chapters { get; set; } = new List<Chapter>();
        public ICollection<BookTag> BookTags { get; set; } = new List<BookTag>();
        public ICollection<UserFavoriteBook> UserFavorites { get; set; } = new List<UserFavoriteBook>();
        public ICollection<Promotion> Promotions { get; set; } = new List<Promotion>();
        public ICollection<BookInvitation> BookInvitations { get; set; } = new List<BookInvitation>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<SimilarBook> SimilarBooks { get; set; }
    }
}
