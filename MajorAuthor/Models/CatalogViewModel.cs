using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MajorAuthor.Models
{
    public class CatalogViewModel
    {
        public List<BookDisplayModel> Books { get; set; } = new List<BookDisplayModel>();
        public List<PoemDisplayModel> Poems { get; set; } = new List<PoemDisplayModel>();
        public List<BlogDisplayModel> Blogs { get; set; } = new List<BlogDisplayModel>();

        public string SortBy { get; set; } = "rating";
        public string SortOrder { get; set; } = "desc";
        public string SearchQuery { get; set; }
        public string SelectedGenres { get; set; }
        public string SelectedTags { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }

        public class BookDisplayModel
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string AuthorName { get; set; }
            public List<string> Authors { get; set; } = new List<string>();
            public string CoverImageUrl { get; set; }
            public int ReadsCount { get; set; }
            public int LikesCount { get; set; }
            public bool IsAdultContent { get; set; }
            public double Rating { get; set; }
            public DateTime PublicationDate { get; set; }
            public string Description { get; set; }
            public List<string> Genres { get; set; } = new List<string>();
            public List<string> Tags { get; set; } = new List<string>(); // Добавляем теги
        }

        public class PoemDisplayModel
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string AuthorName { get; set; }
            public string ContentSnippet { get; set; }
            public int ReadsCount { get; set; }
            public int LikesCount { get; set; }
            public int CommentsCount { get; set; }
            public double Rating { get; set; }
            public DateTime PublicationDate { get; set; }
        }

        public class BlogDisplayModel
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string AuthorName { get; set; }
            public string ContentSnippet { get; set; }
            public int CommentsCount { get; set; }
            public int ViewsCount { get; set; }
            public int LikesCount { get; set; }
            public double Rating { get; set; }
            public DateTime PublicationDate { get; set; }
        }
    }
}