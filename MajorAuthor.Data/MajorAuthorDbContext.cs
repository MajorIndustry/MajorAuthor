// Проект: MajorAuthor.Data
// Файл: MajorAuthorDbContext.cs
using MajorAuthor.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;

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
        public DbSet<UserPreferredTag> UserPreferredTags { get; set; }
        public DbSet<UserFavoriteBook> UserFavoriteBooks { get; set; } // НОВЫЙ DbSet для избранных книг


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
                .HasForeignKey(ba => ba.AuthorId); // Автор.Id является внешним ключом к User.Id, который используется здесь


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
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Receiver)
                .WithMany()
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            // Настройки для комментариев
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Book)
                .WithMany()
                .HasForeignKey(c => c.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(c => c.ParentCommentId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

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

            // НАСТРОЙКИ для UserPreferredTag (многие-ко-многим между User и Tag)
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

            // НОВЫЕ НАСТРОЙКИ для UserFavoriteBook (многие-ко-многим между User и Book)
            modelBuilder.Entity<UserFavoriteBook>()
                .HasKey(ufb => new { ufb.UserId, ufb.BookId });

            modelBuilder.Entity<UserFavoriteBook>()
                .HasOne(ufb => ufb.User)
                .WithMany(u => u.FavoriteBooks)
                .HasForeignKey(ufb => ufb.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Если пользователь удаляется, удаляем его избранные книги

            modelBuilder.Entity<UserFavoriteBook>()
                .HasOne(ufb => ufb.Book)
                .WithMany(b => b.UserFavorites)
                .HasForeignKey(ufb => ufb.BookId)
                .OnDelete(DeleteBehavior.Cascade); // Если книга удаляется, удаляем ее из избранных у всех пользователей


            base.OnModelCreating(modelBuilder);
        }
    }
}
