// Проект: MajorAuthor.Data
// Файл: MajorAuthorDbContext.cs
using MajorAuthor.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Xml.Linq;

namespace MajorAuthor.Data
{
    /// <summary>
    /// Контекст базы данных для MajorAuthor.
    /// </summary>
    public class MajorAuthorDbContext : DbContext
    {
        public MajorAuthorDbContext(DbContextOptions<MajorAuthorDbContext> options) : base(options)
        {
        }

        // DbSets для ваших сущностей
        public DbSet<Author> Authors { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<BookReading> BookReadings { get; set; }
        public DbSet<BookLike> BookLikes { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<BookGenre> BookGenres { get; set; }
        public DbSet<UserPreferredGenre> UserPreferredGenres { get; set; }
        public DbSet<BookAuthor> BookAuthors { get; set; }
        public DbSet<Chapter> Chapters { get; set; }
        public DbSet<Page> Pages { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<BookTag> BookTags { get; set; }
        public DbSet<UserPreferredTag> UserPreferredTags { get; set; } // НОВЫЙ DbSet для предпочтительных тегов


        /// <summary>
        /// Метод для настройки модели базы данных.
        /// </summary>
        /// <param name="modelBuilder">Построитель модели.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Настройка составного первичного ключа для BookGenre
            modelBuilder.Entity<BookGenre>()
                .HasKey(bg => new { bg.BookId, bg.GenreId });

            // Настройка связи многие-ко-многим между Book и Genre
            modelBuilder.Entity<BookGenre>()
                .HasOne(bg => bg.Book)
                .WithMany(b => b.BookGenres)
                .HasForeignKey(bg => bg.BookId);

            modelBuilder.Entity<BookGenre>()
                .HasOne(bg => bg.Genre)
                .WithMany(g => g.BookGenres)
                .HasForeignKey(bg => bg.GenreId);

            // Настройка составного первичного ключа для UserPreferredGenre
            modelBuilder.Entity<UserPreferredGenre>()
                .HasKey(upg => new { upg.UserId, upg.GenreId });

            // Настройка связи многие-ко-многим между User и Genre (через UserPreferredGenre)
            modelBuilder.Entity<UserPreferredGenre>()
                .HasOne(upg => upg.User)
                .WithMany(u => u.PreferredGenres)
                .HasForeignKey(upg => upg.UserId);

            modelBuilder.Entity<UserPreferredGenre>()
                .HasOne(upg => upg.Genre)
                .WithMany() // Нет обратного навигационного свойства в Genre к UserPreferredGenre
                .HasForeignKey(upg => upg.GenreId);


            // Настройки для совместного написания и медиа

            // Настройка составного первичного ключа для BookAuthor
            modelBuilder.Entity<BookAuthor>()
                .HasKey(ba => new { ba.BookId, ba.AuthorId });

            // Настройка связи многие-ко-многим между Book и Author (через BookAuthor)
            modelBuilder.Entity<BookAuthor>()
                .HasOne(ba => ba.Book)
                .WithMany(b => b.BookAuthors)
                .HasForeignKey(ba => ba.BookId);

            modelBuilder.Entity<BookAuthor>()
                .HasOne(ba => ba.Author)
                .WithMany(a => a.BookAuthors)
                .HasForeignKey(ba => ba.AuthorId);

            // Настройка связи один-ко-многим между Book и Chapter
            modelBuilder.Entity<Chapter>()
                .HasOne(c => c.Book)
                .WithMany(b => b.Chapters)
                .HasForeignKey(c => c.BookId)
                .OnDelete(DeleteBehavior.Cascade); // При удалении книги удалять все главы

            // Настройка связи один-ко-многим между Chapter и Page
            modelBuilder.Entity<Page>()
                .HasOne(p => p.Chapter)
                .WithMany(c => c.Pages)
                .HasForeignKey(p => p.ChapterId)
                .OnDelete(DeleteBehavior.Cascade); // При удалении главы удалять все страницы

            // Настройки для сообщений
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany() // У пользователя может быть много отправленных сообщений (нет навигационного свойства в User для отправленных)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict); // Не удалять пользователя при удалении его сообщений

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Receiver)
                .WithMany() // У пользователя может быть много полученных сообщений (нет навигационного свойства в User для полученных)
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict); // Не удалять пользователя при удалении его сообщений

            // Настройки для комментариев
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Book)
                .WithMany() // У книги может быть много комментариев (нет навигационного свойства в Book)
                .HasForeignKey(c => c.BookId)
                .OnDelete(DeleteBehavior.Cascade); // При удалении книги удалять все комментарии к ней

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany() // У пользователя может быть много комментариев (нет навигационного свойства в User)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade); // При удалении пользователя удалять его комментарии

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(c => c.ParentCommentId)
                .IsRequired(false) // ParentCommentId может быть null
                .OnDelete(DeleteBehavior.Restrict); // Не удалять родительский комментарий при удалении дочерних

            // НАСТРОЙКИ для One-to-One связи между User и Author
            modelBuilder.Entity<User>()
                .HasOne(u => u.AuthorProfile)
                .WithOne(a => a.User)
                .HasForeignKey<Author>(a => a.Id);

            // НАСТРОЙКИ для BookTag (многие-ко-многим между Book и Tag)
            modelBuilder.Entity<BookTag>()
                .HasKey(bt => new { bt.BookId, bt.TagId });

            modelBuilder.Entity<BookTag>()
                .HasOne(bt => bt.Book)
                .WithMany(b => b.BookTags)
                .HasForeignKey(bt => bt.BookId);

            modelBuilder.Entity<BookTag>()
                .HasOne(bt => bt.Tag)
                .WithMany(t => t.BookTags)
                .HasForeignKey(bt => bt.TagId);

            // НОВЫЕ НАСТРОЙКИ для UserPreferredTag (многие-ко-многим между User и Tag)
            modelBuilder.Entity<UserPreferredTag>()
                .HasKey(upt => new { upt.UserId, upt.TagId });

            modelBuilder.Entity<UserPreferredTag>()
                .HasOne(upt => upt.User)
                .WithMany(u => u.PreferredTags)
                .HasForeignKey(upt => upt.UserId);

            modelBuilder.Entity<UserPreferredTag>()
                .HasOne(upt => upt.Tag)
                .WithMany(t => t.UserPreferredTags)
                .HasForeignKey(upt => upt.TagId);


            base.OnModelCreating(modelBuilder);
        }
    }
}
