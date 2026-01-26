using System.Collections.Generic;

namespace MajorAuthor.Models
{
    public class BookExportModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<AuthorExportModel> Authors { get; set; } = new List<AuthorExportModel>();
        public List<ChapterExportModel> Chapters { get; set; } = new List<ChapterExportModel>();
        public string CoverImageUrl { get; set; }
        public string PublicationDate { get; set; }
        public List<string> Genres { get; set; } = new List<string>();
        public List<string> Tags { get; set; } = new List<string>();
    }

    public class AuthorExportModel
    {
        public string PenName { get; set; }
        public string Role { get; set; }
    }

    public class ChapterExportModel
    {
        public int Order { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public List<ChapterPageExportModel> Pages { get; set; } = new List<ChapterPageExportModel>();
    }

    public class ChapterPageExportModel
    {
        public int PageNumber { get; set; }
        public string TextContent { get; set; }
        public string ImageUrl { get; set; }
        public bool HasImage => !string.IsNullOrEmpty(ImageUrl);
    }
}