// Проект: MajorAuthor.Data
// Файл: DbInitializer.cs
using MajorAuthor.Data.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MajorAuthor.Data
{
    /// <summary>
    /// Класс для инициализации базы данных тестовыми данными (seeding).
    /// </summary>
    public static class DbInitializer
    {
        /// <summary>
        /// Инициализирует базу данных, добавляя базовые жанры, если их нет.
        /// </summary>
        /// <param name="context">Контекст базы данных.</param>
        public static async Task Initialize(MajorAuthorDbContext context)
        {
            // Проверяем, есть ли уже какие-либо жанры
            if (context.Genres.Any())
            {
                return; // База данных уже инициализирована
            }

            // Добавляем базовые жанры
            var genres = new Genre[]
            {
                new Genre { Name = "Фантастика" },
                new Genre { Name = "Фэнтези" },
                new Genre { Name = "Детектив" },
                new Genre { Name = "Приключения" },
                new Genre { Name = "Романтика" },
                new Genre { Name = "Триллер" },
                new Genre { Name = "Ужасы" },
                new Genre { Name = "Научная фантастика" },
                new Genre { Name = "Исторический роман" },
                new Genre { Name = "Биография" }
            };

            await context.Genres.AddRangeAsync(genres);
            await context.SaveChangesAsync();

            // Здесь также можно добавить инициализацию тестовых книг, авторов, пользователей и т.д.
            // Например:
            // var author1 = new Author { PenName = "Тестовый Автор 1", User = new User { Username = "testuser1", Email = "test1@example.com", PasswordHash = "hashedpassword" } };
            // await context.Authors.AddAsync(author1);
            // await context.SaveChangesAsync();
            //
            // var book1 = new Book { Title = "Тестовая Книга Фантастика", PublicationDate = DateTime.UtcNow, Description = "Описание тестовой фантастической книги.", LikesCount = 0, ReadsCount = 0, LastUpdateTime = DateTime.UtcNow };
            // await context.Books.AddAsync(book1);
            // await context.SaveChangesAsync();
            //
            // await context.BookGenres.AddAsync(new BookGenre { BookId = book1.Id, GenreId = genres.First(g => g.Name == "Фантастика").Id });
            // await context.BookAuthors.AddAsync(new BookAuthor { BookId = book1.Id, AuthorId = author1.Id });
            // await context.SaveChangesAsync();
        }
    }
}
