using System;
using System.Collections.Generic;

namespace MajorAuthor.Models
{
    public class MyProfileViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public bool IsAuthor { get; set; }
        public int? AuthorId { get; set; }
        public string DisplayName { get; set; }
        public string FullName { get; set; }
        public string PhotoUrl { get; set; }
        public bool IsOwnProfile { get; set; }
        public bool IsFollowing { get; set; }
        public bool IsSearch { get; set; } = false;

        // Статистика
        public int FollowerCount { get; set; }

        // Для авторов
        public List<BookDisplayModel> AuthoredBooks { get; set; }
        public List<PoemDisplayModel> AuthoredPoems { get; set; }
        public List<BlogDisplayModel> AuthoredBlogs { get; set; }
        public List<FollowerDisplayModel> FollowersList { get; set; }

        // Для читателей
        public List<BookDisplayModel> ReadBooks { get; set; }
        public List<BookDisplayModel> FavoriteBooks { get; set; }
        public List<BookDisplayModel> LikedBooks { get; set; }
        public List<PoemDisplayModel> LikedPoems { get; set; }
        public List<BlogDisplayModel> LikedBlogs { get; set; }

        // Подписки (общее для всех)
        public List<AuthorSubscriptionDisplayModel> Subscriptions { get; set; }

        public MyProfileViewModel()
        {
            // Инициализация всех списков
            AuthoredBooks = new List<BookDisplayModel>();
            AuthoredPoems = new List<PoemDisplayModel>();
            AuthoredBlogs = new List<BlogDisplayModel>();
            FollowersList = new List<FollowerDisplayModel>();
            ReadBooks = new List<BookDisplayModel>();
            FavoriteBooks = new List<BookDisplayModel>();
            LikedBooks = new List<BookDisplayModel>();
            LikedPoems = new List<PoemDisplayModel>();
            LikedBlogs = new List<BlogDisplayModel>();
            Subscriptions = new List<AuthorSubscriptionDisplayModel>();
        }
        // В файле MyProfileViewModel.cs
        public class BookDisplayModel
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string AuthorName { get; set; }
            public string CoverImageUrl { get; set; }
            public int ReadsCount { get; set; }
            public int LikesCount { get; set; }
            public bool IsAdultContent { get; set; }

            // Новые свойства для совместимости с _BookListPartial
            public double Rating { get; set; }
            public string Description { get; set; }
            public List<string> Authors { get; set; } = new List<string>();
            public List<string> Genres { get; set; } = new List<string>();
            public List<string> Tags { get; set; } = new List<string>();
            public DateTime PublicationDate { get; set; }
        }

        public class PoemDisplayModel
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string AuthorName { get; set; }
            public string ContentSnippet { get; set; }
            public int ViewsCount { get; set; }
            public int LikesCount { get; set; }
            public int CommentsCount { get; set; }
            public DateTime PublicationDate { get; set; }
        }

        public class BlogDisplayModel
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string AuthorName { get; set; }
            public string ImageUrl { get; set; }
            public string ContentSnippet { get; set; }
            public int ViewsCount { get; set; }
            public int LikesCount { get; set; }
            public int CommentsCount { get; set; }
            public DateTime PublicationDate { get; set; }
        }

        public class FollowerDisplayModel
        {
            public string UserId { get; set; }
            public string UserName { get; set; }
        }

        // Новый класс для отображения подписок
        public class AuthorSubscriptionDisplayModel
        {
            public int AuthorId { get; set; }
            public string AuthorName { get; set; }
            public string PhotoUrl { get; set; }
            public int FollowerCount { get; set; }
        }
    }
}