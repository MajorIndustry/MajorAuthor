// Проект: MajorAuthor.Data
// Файл: MajorAuthorDbContext.cs
// Обновлен для использования IdentityDbContext<ApplicationUser> и настройки всех новых связей.
using MajorAuthor.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore; // Для IdentityDbContext
using Microsoft.AspNetCore.Identity; // Для IdentityUser

namespace MajorAuthor.Data
{
    /// <summary>
    /// Контекст базы данных для MajorAuthor.
    /// Наследуется от IdentityDbContext для интеграции с ASP.NET Core Identity.
    /// </summary>
    public class MajorAuthorDbContext : IdentityDbContext<ApplicationUser> // Изменено на ApplicationUser
    {
        public MajorAuthorDbContext(DbContextOptions<MajorAuthorDbContext> options) : base(options)
        {
        }

        // DbSets для ваших сущностей
        public DbSet<Author> Authors { get; set; }
        public DbSet<Book> Books { get; set; }
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
        public DbSet<Follower> Followers { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Blog> Blogs { get; set; } // Теперь Blog представляет записи блога
        // public DbSet<BlogPost> BlogPosts { get; set; } // Удалено, так как BlogPost не существует
        public DbSet<Tag> Tags { get; set; }
        public DbSet<BookTag> BookTags { get; set; }
        public DbSet<UserPreferredTag> UserPreferredTags { get; set; }
        public DbSet<UserFavoriteBook> UserFavoriteBooks { get; set; }

        // DbSets для стихов, лайков на стихи, комментариев на стихи и прочитанных глав
        public DbSet<Poem> Poems { get; set; }
        public DbSet<PoemLike> PoemLikes { get; set; }
        public DbSet<PoemComment> PoemComments { get; set; }
        public DbSet<ChapterRead> ChapterReads { get; set; }

        // Новые DbSets для BlogComment, Promotion, PromotionPlan, BlogLike
        public DbSet<BlogComment> BlogComments { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<PromotionPlan> PromotionPlans { get; set; }
        public DbSet<BlogLike> BlogLikes { get; set; }
        public DbSet<BookInvitation> BookInvitations { get; set; }

        /// <summary>
        /// Метод для настройки модели базы данных.
        /// </summary>
        /// <param name="modelBuilder">Построитель модели.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Важно вызвать базовый метод для настройки Identity

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
            modelBuilder.Entity<UserPreferredGenre>()
                .HasKey(upg => new { upg.ApplicationUserId, upg.GenreId });

            modelBuilder.Entity<UserPreferredGenre>()
                .HasOne(upg => upg.ApplicationUser)
                .WithMany(u => u.PreferredGenres)
                .HasForeignKey(upg => upg.ApplicationUserId);

            modelBuilder.Entity<UserPreferredGenre>()
                .HasOne(upg => upg.Genre)
                .WithMany() // Предполагаем, что это корректная связь
                .HasForeignKey(upg => upg.GenreId);


            // Настройки для совместного написания и медиа

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
                .HasOne(m => m.Sender)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Receiver)
                .WithMany(u => u.ReceivedMessages)
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            // Настройки для комментариев к книгам
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Book)
                .WithMany()
                .HasForeignKey(c => c.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.ApplicationUser)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.ApplicationUserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(c => c.ParentCommentId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Настройки для сущности Follower (подписки)
            modelBuilder.Entity<Follower>()
                .HasKey(f => new { f.FollowerApplicationUserId, f.AuthorId });

            modelBuilder.Entity<Follower>()
                .HasOne(f => f.FollowerApplicationUser)
                .WithMany(u => u.Following)
                .HasForeignKey(f => f.FollowerApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Follower>()
                .HasOne(f => f.Author)
                .WithMany(a => a.Followers)
                .HasForeignKey(f => f.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Настройки для сущности Notification
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.ApplicationUser)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.ApplicationUserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Настройки для сущностей Blog
            modelBuilder.Entity<Blog>()
                .HasOne(b => b.Author)
                .WithMany(a => a.Blogs)
                .HasForeignKey(b => b.AuthorId)
                .OnDelete(DeleteBehavior.Cascade);

            // Настройка составного первичного ключа для BookTag
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

            // Настройка составного первичного ключа для UserPreferredTag
            modelBuilder.Entity<UserPreferredTag>()
                .HasKey(upt => new { upt.ApplicationUserId, upt.TagId });

            modelBuilder.Entity<UserPreferredTag>()
                .HasOne(upt => upt.ApplicationUser)
                .WithMany(u => u.PreferredTags)
                .HasForeignKey(upt => upt.ApplicationUserId);

            modelBuilder.Entity<UserPreferredTag>()
                .HasOne(upt => upt.Tag)
                .WithMany(t => t.UserPreferredTags)
                .HasForeignKey(upt => upt.TagId);

            // Настройка составного первичного ключа для UserFavoriteBook
            modelBuilder.Entity<UserFavoriteBook>()
                .HasKey(ufb => new { ufb.ApplicationUserId, ufb.BookId });

            modelBuilder.Entity<UserFavoriteBook>()
                .HasOne(ufb => ufb.ApplicationUser)
                .WithMany(u => u.FavoriteBooks)
                .HasForeignKey(ufb => ufb.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserFavoriteBook>()
                .HasOne(ufb => ufb.Book)
                .WithMany(b => b.UserFavorites)
                .HasForeignKey(ufb => ufb.BookId)
                .OnDelete(DeleteBehavior.Cascade);


            // Настройки для стихов
            modelBuilder.Entity<Poem>()
                .HasOne(p => p.Author)
                .WithMany(a => a.Poems)
                .HasForeignKey(p => p.AuthorId)
                .OnDelete(DeleteBehavior.Cascade);

            // Настройки для лайков стихов
            modelBuilder.Entity<PoemLike>()
                .HasOne(pl => pl.Poem)
                .WithMany(p => p.Likes)
                .HasForeignKey(pl => pl.PoemId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PoemLike>()
                .HasOne(pl => pl.ApplicationUser)
                .WithMany(u => u.PoemLikes)
                .HasForeignKey(pl => pl.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Настройки для комментариев стихов
            modelBuilder.Entity<PoemComment>()
                .HasOne(pc => pc.Poem)
                .WithMany(p => p.Comments)
                .HasForeignKey(pc => pc.PoemId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PoemComment>()
                .HasOne(pc => pc.ApplicationUser)
                .WithMany(u => u.PoemComments)
                .HasForeignKey(pc => pc.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PoemComment>()
                .HasOne(pc => pc.ParentComment)
                .WithMany(pc => pc.Replies)
                .HasForeignKey(pc => pc.ParentCommentId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Настройки для прочитанных глав
            modelBuilder.Entity<ChapterRead>()
                .HasKey(cr => new { cr.ApplicationUserId, cr.ChapterId });

            modelBuilder.Entity<ChapterRead>()
                .HasOne(cr => cr.ApplicationUser)
                .WithMany(au => au.ChaptersRead)
                .HasForeignKey(cr => cr.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ChapterRead>()
                .HasOne(cr => cr.Chapter)
                .WithMany(c => c.ChapterReads)
                .HasForeignKey(cr => cr.ChapterId)
                .OnDelete(DeleteBehavior.Cascade);

            // Настройки для BlogComment
            modelBuilder.Entity<BlogComment>()
                .HasOne(bc => bc.Blog) // Изменено с BlogPost на Blog
                .WithMany(b => b.Comments) // Изменено с bp.Comments на b.Comments
                .HasForeignKey(bc => bc.BlogId) // Изменено с BlogPostId на BlogId
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BlogComment>()
                .HasOne(bc => bc.ApplicationUser)
                .WithMany()
                .HasForeignKey(bc => bc.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BlogComment>()
                .HasOne(bc => bc.ParentComment)
                .WithMany(bc => bc.Replies)
                .HasForeignKey(bc => bc.ParentCommentId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Настройки для Promotion
            modelBuilder.Entity<Promotion>()
                .HasOne(p => p.Book)
                .WithMany(b => b.Promotions)
                .HasForeignKey(p => p.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Promotion>()
                .HasOne(p => p.PromotionPlan)
                .WithMany(pp => pp.Promotions)
                .HasForeignKey(p => p.PromotionPlanId)
                .OnDelete(DeleteBehavior.Restrict);

            // Настройки для BlogLike
            modelBuilder.Entity<BlogLike>()
                .HasOne(bl => bl.Blog) // Изменено с BlogPost на Blog
                .WithMany(b => b.Likes) // Изменено с bp.Likes на b.Likes
                .HasForeignKey(bl => bl.BlogId) // Изменено с BlogPostId на BlogId
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BlogLike>()
                .HasOne(bl => bl.ApplicationUser)
                .WithMany()
                .HasForeignKey(bl => bl.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Настройки для BookInvitation
            modelBuilder.Entity<BookInvitation>()
                .HasKey(bi => bi.Id);

            modelBuilder.Entity<BookInvitation>()
                .HasOne(bi => bi.Book)
                .WithMany(b => b.BookInvitations)
                .HasForeignKey(bi => bi.BookId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
