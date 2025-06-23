// Проект: MajorAuthor.Data
// Файл: ApplicationUser.cs
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using MajorAuthor.Data.Entities; // Необходимо для Author, UserPreferredGenre, UserPreferredTag, UserFavoriteBook

namespace MajorAuthor.Data
{
    /// <summary>
    /// Расширенная сущность пользователя Identity, содержащая дополнительные свойства.
    /// Это ваша основная сущность пользователя.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        // Пример дополнительного свойства, если оно вам нужно
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

        // Если у пользователя есть аватар, можно добавить URL
        public string? ProfilePictureUrl { get; set; }

        // Новое: Навигационное свойство для связи один-к-одному с профилем автора
        public Author? AuthorProfile { get; set; }

        // Новое: Навигационное свойство для избранных книг (связь многие-ко-многим через UserFavoriteBook)
        public ICollection<UserFavoriteBook> FavoriteBooks { get; set; } = new List<UserFavoriteBook>();

        // Новое: Навигационное свойство для предпочтительных жанров (связь многие-ко-многим через UserPreferredGenre)
        public ICollection<UserPreferredGenre> PreferredGenres { get; set; } = new List<UserPreferredGenre>();

        // Новое: Навигационное свойство для предпочтительных тегов (связь многие-ко-многим через UserPreferredTag)
        public ICollection<UserPreferredTag> PreferredTags { get; set; } = new List<UserPreferredTag>();
    }
}
