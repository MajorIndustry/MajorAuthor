// Проект: MajorAuthor.Data
// Файл: MajorAuthorDbContext.cs
using MajorAuthor.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity; // Для IdentityUser, хотя теперь используем ApplicationUser

namespace MajorAuthor.Data
{
    /// <summary>
    /// Контекст базы данных для MajorAuthor, теперь наследуется от IdentityDbContext<ApplicationUser>.
    /// </summary>
    public class MajorAuthorDbContext : IdentityDbContext<ApplicationUser> // Изменено на ApplicationUser
    {
        public MajorAuthorDbContext(DbContextOptions<MajorAuthorDbContext> options) : base(options)
        {
        }

        // DbSets для ваших пользовательских сущностей
        public DbSet<Author> Authors { get; set; }
        public DbSet<Book> Books { get; set; }
        // Удален DbSet<User> Users - убедитесь, что MajorAuthor.Data.Entities.User.cs либо удален, либо переименован/перепрофилирован.

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
        public DbSet<Tag> Tags { get; set; } // Добавлено
        public DbSet<BookTag> BookTags { get; set; } // Добавлено
        public DbSet<UserPreferredTag> UserPreferredTags { get; set; } // Добавлено
        public DbSet<UserFavoriteBook> UserFavoriteBooks { get; set; } // Добавлено
        public DbSet<PromotionPlan> PromotionPlans { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        /// <summary>
        /// Метод для настройки модели базы данных.
        /// </summary>
        /// <param name="modelBuilder">Построитель модели.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Важно: Сначала вызовите базовую реализацию OnModelCreating из IdentityDbContext.
            // Это гарантирует, что сущности Identity (ApplicationUser, IdentityRole, IdentityUserLogin и т.д.)
            // будут корректно добавлены в модель.
            base.OnModelCreating(modelBuilder);

            // Настройка связи один-к-одному между ApplicationUser и Author
            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.AuthorProfile) // ApplicationUser имеет один AuthorProfile
                .WithOne(a => a.User) // Один Author связан с одним ApplicationUser
                .HasForeignKey<Author>(a => a.IdentityUserId) // Внешний ключ находится в сущности Author
                .IsRequired(false) // IdentityUserId может быть NULL (автор может быть не привязан к пользователю)
                .OnDelete(DeleteBehavior.SetNull); // При удалении ApplicationUser, IdentityUserId в Author становится NULL


            // Настройка составного первичного ключа для BookGenre
            modelBuilder.Entity<BookGenre>()
                .HasKey(bg => new { bg.BookId, bg.GenreId });

            modelBuilder.Entity<BookGenre>()
                .HasOne(bg => bg.Book)
                .WithMany(b => b.BookGenres)
                .HasForeignKey(bg => bg.BookId);

            modelBuilder.Entity<BookGenre>()
                .HasOne(bg => bg.Genre)
                .WithMany(g => g.BookGenres)
                .HasForeignKey(bg => bg.GenreId);

            // Настройка составного первичного ключа для UserPreferredGenre
            // UserPreferredGenre.UserId должен ссылаться на ApplicationUser.Id (string)
            modelBuilder.Entity<UserPreferredGenre>()
                .HasKey(upg => new { upg.UserId, upg.GenreId });

            // UserPreferredGenre.UserId теперь ссылается на Id из ApplicationUser
            modelBuilder.Entity<UserPreferredGenre>()
                .HasOne(upg => upg.User) // Связываем с ApplicationUser через навигационное свойство User
                .WithMany(u => u.PreferredGenres) // У ApplicationUser может быть много UserPreferredGenre
                .HasForeignKey(upg => upg.UserId)
                .IsRequired(); // UserId должен быть обязательным

            modelBuilder.Entity<UserPreferredGenre>()
                .HasOne(upg => upg.Genre)
                .WithMany()
                .HasForeignKey(upg => upg.GenreId);

            // Настройка составного первичного ключа для BookAuthor
            modelBuilder.Entity<BookAuthor>()
                .HasKey(ba => new { ba.BookId, ba.AuthorId });

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
                .OnDelete(DeleteBehavior.Cascade);

            // Настройка связи один-ко-многим между Chapter и Page
            modelBuilder.Entity<Page>()
                .HasOne(p => p.Chapter)
                .WithMany(c => c.Pages)
                .HasForeignKey(p => p.ChapterId)
                .OnDelete(DeleteBehavior.Cascade);

            // Настройки для сообщений
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender) // Связываем с ApplicationUser через навигационное свойство Sender
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Receiver) // Связываем с ApplicationUser через навигационное свойство Receiver
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
                .HasOne(c => c.User) // Связываем с ApplicationUser через навигационное свойство User
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(c => c.ParentCommentId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

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

            // НАСТРОЙКИ для UserPreferredTag (многие-ко-многим между ApplicationUser и Tag)
            modelBuilder.Entity<UserPreferredTag>()
                .HasKey(upt => new { upt.UserId, upt.TagId });

            modelBuilder.Entity<UserPreferredTag>()
                .HasOne(upt => upt.User) // Связываем с ApplicationUser через навигационное свойство User
                .WithMany(u => u.PreferredTags) // У ApplicationUser может быть много UserPreferredTag
                .HasForeignKey(upt => upt.UserId);

            modelBuilder.Entity<UserPreferredTag>()
                .HasOne(upt => upt.Tag)
                .WithMany(t => t.UserPreferredTags)
                .HasForeignKey(upt => upt.TagId);

            // НОВЫЕ НАСТРОЙКИ для UserFavoriteBook (многие-ко-многим между ApplicationUser и Book)
            modelBuilder.Entity<UserFavoriteBook>()
                .HasKey(ufb => new { ufb.UserId, ufb.BookId });

            modelBuilder.Entity<UserFavoriteBook>()
                .HasOne(ufb => ufb.User) // Связываем с ApplicationUser через навигационное свойство User
                .WithMany(u => u.FavoriteBooks) // У ApplicationUser может быть много UserFavoriteBook
                .HasForeignKey(ufb => ufb.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserFavoriteBook>()
                .HasOne(ufb => ufb.Book)
                .WithMany(b => b.UserFavorites) // У книги может быть много UserFavoriteBook
                .HasForeignKey(ufb => ufb.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            // НОВЫЕ НАСТРОЙКИ для продвижения
            modelBuilder.Entity<Promotion>()
                .HasOne(p => p.Book)
                .WithMany() // У книги может быть много продвижений (нет прямого навигационного свойства в Book)
                .HasForeignKey(p => p.BookId)
                .OnDelete(DeleteBehavior.Restrict); // Не удалять книгу при удалении продвижения

            modelBuilder.Entity<Promotion>()
                .HasOne(p => p.PromotionPlan)
                .WithMany(pp => pp.Promotions)
                .HasForeignKey(p => p.PromotionPlanId)
                .OnDelete(DeleteBehavior.Restrict); // Не удалять план продвижения при удалении продвижения
        }
    }
}
