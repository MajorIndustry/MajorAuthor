using MajorAuthor.Data;
using MajorAuthor.Data.Entities;
using MajorAuthor.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MajorAuthor.Services
{
    public class WorkFacade : IWorkFacade
    {
        private readonly MajorAuthorDbContext _context;
        private readonly IWorkService<Poem> _poemService;
        private readonly IWorkService<Blog> _blogService;
        private readonly IAuthorService _authorService;
        private readonly IBookService _bookService;
        private readonly IRatingManagerService _ratingManagerService;

        public WorkFacade(
            MajorAuthorDbContext context,
            IWorkService<Poem> poemService,
            IWorkService<Blog> blogService,
            IAuthorService authorService,
            IBookService bookService,
            IRatingManagerService ratingManagerService)
        {
            _context = context;
            _poemService = poemService;
            _blogService = blogService;
            _authorService = authorService;
            _bookService = bookService;
            _ratingManagerService = ratingManagerService;
        }

        public async Task<WorkViewModel> GetWorkViewModelAsync(int id, string type)
        {
            object work = null;
            switch (type.ToLower())
            {
                case "стих":
                    work = await _poemService.GetByIdWithCommentsAndLikesAsync(id);
                    break;
                case "блог":
                    work = await _blogService.GetByIdWithCommentsAndLikesAsync(id);
                    break;
                case "книга":
                    work = await _bookService.GetByIdWithCommentsAndLikesAsync(id);
                    break;
            }

            if (work == null) return null;

            // Увеличиваем счетчик просмотров
            if (work is Poem poem)
            {
                poem.ViewsCount++;
                await _poemService.UpdateAsync(poem);
            }
            else if (work is Blog blog)
            {
                blog.ViewsCount++;
                await _blogService.UpdateAsync(blog);
            }
            else if (work is Book book)
            {
                book.ReadsCount++;
                await _bookService.UpdateAsync(book);
            }

            var comments = await GetCommentsForWorkAsync(id, type);
            var likesCount = await GetLikesCountForWorkAsync(id, type);

            return new WorkViewModel
            {
                Work = work,
                Comments = comments,
                LikesCount = likesCount,
                Type = type
            };
        }

        public async Task<(bool success, int newLikesCount, bool isLiked)> ToggleLikeAsync(int id, string type, string userId)
        {
            // Проверяем, является ли пользователь автором работы
            var isAuthor = await IsUserAuthorAsync(id, type, userId);
            if (isAuthor)
            {
                return (false, 0, false);
            }

            object work = null;
            switch (type.ToLower())
            {
                case "стих":
                    work = await _poemService.GetByIdAsync(id);
                    break;
                case "блог":
                    work = await _blogService.GetByIdAsync(id);
                    break;
                case "книга":
                    work = await _bookService.GetByIdAsync(id);
                    break;
            }

            if (work == null) return (false, 0, false);

            var hasLiked = await HasUserLikedAsync(id, type, userId);

            if (hasLiked)
            {
                // Удаляем лайк
                await RemoveLikeAsync(id, userId, type);
            }
            else
            {
                // Добавляем лайк
                await AddLikeAsync(id, userId, type);
            }

            int finalLikesCount = await GetLikesCountForWorkAsync(id, type);

            return (true, finalLikesCount, !hasLiked);
        }

        public async Task<CommentViewModel> AddCommentAsync(int id, string type, string userId, string commentText)
        {
            try
            {
                switch (type.ToLower())
                {
                    case "стих":
                        return await AddPoemCommentAsync(id, userId, commentText);
                    case "блог":
                        return await AddBlogCommentAsync(id, userId, commentText);
                    case "книга":
                        return await AddBookCommentAsync(id, userId, commentText);
                    default:
                        return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при добавлении комментария: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> DeleteCommentAsync(int commentId, string type, string userId)
        {
            try
            {
                switch (type.ToLower())
                {
                    case "стих":
                        return await DeletePoemCommentAsync(commentId, userId);
                    case "блог":
                        return await DeleteBlogCommentAsync(commentId, userId);
                    case "книга":
                        return await DeleteBookCommentAsync(commentId, userId);
                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении комментария: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> IsUserAuthorAsync(int workId, string workType, string userId)
        {
            var author = await _authorService.GetAuthorByUserIdAsync(userId);
            if (author == null) return false;

            switch (workType.ToLower())
            {
                case "стих":
                    var poem = await _poemService.GetByIdAsync(workId);
                    return poem != null && poem.AuthorId == author.Id;
                case "блог":
                    var blog = await _blogService.GetByIdAsync(workId);
                    return blog != null && blog.AuthorId == author.Id;
                case "книга":
                    return await _bookService.IsAuthorOfBookAsync(workId, author.Id);
                default:
                    return false;
            }
        }

        public async Task<bool> HasUserLikedAsync(int workId, string workType, string userId)
        {
            switch (workType.ToLower())
            {
                case "стих":
                    return await _context.PoemLikes
                        .AnyAsync(pl => pl.PoemId == workId && pl.ApplicationUserId == userId);
                case "блог":
                    return await _context.BlogLikes
                        .AnyAsync(bl => bl.BlogId == workId && bl.ApplicationUserId == userId);
                case "книга":
                    return await _context.BookLikes
                        .AnyAsync(bl => bl.BookId == workId && bl.ApplicationUserId == userId);
                default:
                    return false;
            }
        }
        public async Task<bool> RegisterReadingAsync(int workId, string workType, string userId)
        {
            try
            {
                switch (workType.ToLower())
                {
                    case "стих":
                        return await RegisterPoemReadingAsync(workId, userId);
                    case "блог":
                        return await RegisterBlogReadingAsync(workId, userId);
                    case "книга":
                        return await RegisterBookReadingAsync(workId, userId);
                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при регистрации прочтения: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> RegisterPoemReadingAsync(int poemId, string userId)
        {
            // Проверяем, не читал ли пользователь уже этот стих
            var existingReading = await _context.PoemReadings
                .FirstOrDefaultAsync(pr => pr.PoemId == poemId && pr.ApplicationUserId == userId);

            if (existingReading != null)
            {
                // Пользователь уже читал этот стих
                return false;
            }

            // Создаем новую запись о прочтении
            var reading = new PoemReading
            {
                PoemId = poemId,
                ApplicationUserId = userId,
                ReadDate = DateTime.UtcNow
            };

            _context.PoemReadings.Add(reading);

            // Обновляем счетчик просмотров
            var poem = await _poemService.GetByIdAsync(poemId);
            if (poem != null)
            {
                poem.ViewsCount++;
                await _poemService.UpdateAsync(poem);
            }

            await _context.SaveChangesAsync(); // СОХРАНЕНИЕ ДО обновления рейтинга
            await _ratingManagerService.UpdatePoemRatingAsync(poemId);
            var authorId = await _context.Poems
                .Where(p => p.Id == poemId)
                .Select(p => p.AuthorId).FirstAsync();
            await _ratingManagerService.UpdateAuthorRatingAsync(authorId);
            return true;
        }

        private async Task<bool> RegisterBlogReadingAsync(int blogId, string userId)
        {
            // Проверяем, не читал ли пользователь уже этот блог
            var existingReading = await _context.BlogReadings
                .FirstOrDefaultAsync(br => br.BlogId == blogId && br.ApplicationUserId == userId);

            if (existingReading != null)
            {
                // Пользователь уже читал этот блог
                return false;
            }

            // Создаем новую запись о прочтении
            var reading = new BlogReading
            {
                BlogId = blogId,
                ApplicationUserId = userId,
                ReadDate = DateTime.UtcNow
            };

            _context.BlogReadings.Add(reading);

            // Обновляем счетчик просмотров
            var blog = await _blogService.GetByIdAsync(blogId);
            if (blog != null)
            {
                blog.ViewsCount++;
                await _blogService.UpdateAsync(blog);
            }

            await _context.SaveChangesAsync(); // СОХРАНЕНИЕ ДО обновления рейтинга
            await _ratingManagerService.UpdateBlogRatingAsync(blogId);
            var authorId = await _context.Blogs
                .Where(p => p.Id == blogId)
                .Select(p => p.AuthorId).FirstAsync();
            await _ratingManagerService.UpdateAuthorRatingAsync(authorId);
            return true;
        }

        private async Task<bool> RegisterBookReadingAsync(int bookId, string userId)
        {
            // Проверяем, не читал ли пользователь уже эту книгу
            var existingReading = await _context.BookReadings
                .FirstOrDefaultAsync(br => br.BookId == bookId && br.ApplicationUserId == userId);

            if (existingReading != null)
            {
                // Обновляем прогресс чтения, если нужно
                existingReading.ReadDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return false;
            }

            // Создаем новую запись о прочтении
            var reading = new BookReading
            {
                BookId = bookId,
                ApplicationUserId = userId,
                ReadDate = DateTime.UtcNow,
                ReadingProgress = 0 // Начальный прогресс
            };

            _context.BookReadings.Add(reading);

            // Обновляем счетчик прочтений
            var book = await _bookService.GetByIdAsync(bookId);
            if (book != null)
            {
                book.ReadsCount++;
                await _bookService.UpdateAsync(book);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(bool success, CommentViewModel updatedComment)> EditCommentAsync(int commentId, string type, string userId, string newCommentText)
        {
            try
            {
                switch (type.ToLower())
                {
                    case "стих":
                        return await EditPoemCommentAsync(commentId, userId, newCommentText);
                    case "блог":
                        return await EditBlogCommentAsync(commentId, userId, newCommentText);
                    case "книга":
                        return await EditBookCommentAsync(commentId, userId, newCommentText);
                    default:
                        return (false, null);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при редактировании комментария: {ex.Message}");
                return (false, null);
            }
        }

        private async Task<(bool success, CommentViewModel updatedComment)> EditPoemCommentAsync(int commentId, string userId, string newCommentText)
        {
            var comment = await _context.PoemComments
                .Include(pc => pc.ApplicationUser)
                .FirstOrDefaultAsync(pc => pc.Id == commentId && pc.ApplicationUserId == userId);

            if (comment == null) return (false, null);

            comment.Content = newCommentText;
            await _context.SaveChangesAsync();

            return (true, new CommentViewModel
            {
                Id = comment.Id,
                Content = comment.Content,
                UserId = comment.ApplicationUserId,
                UserName = comment.ApplicationUser?.UserName ?? "Пользователь",
                Timestamp = comment.CreatedDate
            });
        }

        private async Task<(bool success, CommentViewModel updatedComment)> EditBlogCommentAsync(int commentId, string userId, string newCommentText)
        {
            var comment = await _context.BlogComments
                .Include(bc => bc.ApplicationUser)
                .FirstOrDefaultAsync(bc => bc.Id == commentId && bc.ApplicationUserId == userId);

            if (comment == null) return (false, null);

            comment.Content = newCommentText;
            await _context.SaveChangesAsync();

            return (true, new CommentViewModel
            {
                Id = comment.Id,
                Content = comment.Content,
                UserId = comment.ApplicationUserId,
                UserName = comment.ApplicationUser?.UserName ?? "Пользователь",
                Timestamp = comment.CommentDate
            });
        }

        private async Task<(bool success, CommentViewModel updatedComment)> EditBookCommentAsync(int commentId, string userId, string newCommentText)
        {
            var comment = await _context.Comments
                .Include(c => c.ApplicationUser)
                .FirstOrDefaultAsync(c => c.Id == commentId && c.ApplicationUserId == userId);

            if (comment == null) return (false, null);

            comment.Content = newCommentText;
            await _context.SaveChangesAsync();

            return (true, new CommentViewModel
            {
                Id = comment.Id,
                Content = comment.Content,
                UserId = comment.ApplicationUserId,
                UserName = comment.ApplicationUser?.UserName ?? "Пользователь",
                Timestamp = comment.CreatedDate
            });
        }
        #region Private Methods

        private async Task<int> GetLikesCountForWorkAsync(int workId, string workType)
        {
            switch (workType.ToLower())
            {
                case "стих":
                    return await _context.PoemLikes.CountAsync(pl => pl.PoemId == workId);
                case "блог":
                    return await _context.BlogLikes.CountAsync(bl => bl.BlogId == workId);
                case "книга":
                    return await _context.BookLikes.CountAsync(bl => bl.BookId == workId);
                default:
                    return 0;
            }
        }

        private async Task<List<CommentViewModel>> GetCommentsForWorkAsync(int workId, string workType)
        {
            switch (workType.ToLower())
            {
                case "стих":
                    var poemComments = await _context.PoemComments
                        .Where(pc => pc.PoemId == workId)
                        .Include(pc => pc.ApplicationUser)
                        .OrderByDescending(pc => pc.CreatedDate)
                        .ToListAsync();

                    return poemComments.Select(pc => new CommentViewModel
                    {
                        Id = pc.Id,
                        Content = pc.Content,
                        UserId = pc.ApplicationUserId,
                        UserName = pc.ApplicationUser.UserName,
                        Timestamp = pc.CreatedDate
                    }).ToList();

                case "блог":
                    var blogComments = await _context.BlogComments
                        .Where(bc => bc.BlogId == workId)
                        .Include(bc => bc.ApplicationUser)
                        .OrderByDescending(bc => bc.CommentDate)
                        .ToListAsync();

                    return blogComments.Select(bc => new CommentViewModel
                    {
                        Id = bc.Id,
                        Content = bc.Content,
                        UserId = bc.ApplicationUserId,
                        UserName = bc.ApplicationUser.UserName,
                        Timestamp = bc.CommentDate
                    }).ToList();

                case "книга":
                    var Comments = await _context.Comments
                        .Where(bc => bc.BookId == workId)
                        .Include(bc => bc.ApplicationUser)
                        .OrderByDescending(bc => bc.CreatedDate)
                        .ToListAsync();

                    return Comments.Select(bc => new CommentViewModel
                    {
                        Id = bc.Id,
                        Content = bc.Content,
                        UserId = bc.ApplicationUserId,
                        UserName = bc.ApplicationUser.UserName,
                        Timestamp = bc.CreatedDate
                    }).ToList();

                default:
                    return new List<CommentViewModel>();
            }
        }

        private async Task AddLikeAsync(int workId, string userId, string workType)
        {
            switch (workType.ToLower())
            {
                case "стих":
                    var poemLike = new PoemLike
                    {
                        PoemId = workId,
                        ApplicationUserId = userId,
                        LikeDate = DateTime.UtcNow
                    };
                    _context.PoemLikes.Add(poemLike);
                    await _context.SaveChangesAsync(); // СОХРАНЕНИЕ ДО обновления рейтинга
                    await _ratingManagerService.UpdatePoemRatingAsync(workId);
                    var authorId = await _context.Poems
                        .Where(p => p.Id == workId)
                        .Select(p => p.AuthorId).FirstAsync();
                    await _ratingManagerService.UpdateAuthorRatingAsync(authorId);
                    break;

                case "блог":
                    var blogLike = new BlogLike
                    {
                        BlogId = workId,
                        ApplicationUserId = userId,
                        LikeDate = DateTime.UtcNow
                    };
                    _context.BlogLikes.Add(blogLike);
                    await _context.SaveChangesAsync(); // СОХРАНЕНИЕ ДО обновления рейтинга
                    await _ratingManagerService.UpdateBlogRatingAsync(workId);
                    authorId = await _context.Blogs
                        .Where(p => p.Id == workId)
                        .Select(p => p.AuthorId).FirstAsync();
                    await _ratingManagerService.UpdateAuthorRatingAsync(authorId);
                    break;

                case "книга":
                    var bookLike = new BookLike
                    {
                        BookId = workId,
                        ApplicationUserId = userId,
                        LikeDate = DateTime.UtcNow
                    };
                    _context.BookLikes.Add(bookLike);
                    await _context.SaveChangesAsync(); // ДЛЯ КНИГ ТОЖЕ НУЖНО
                    break;
            }
        }

        private async Task RemoveLikeAsync(int workId, string userId, string workType)
        {
            
            switch (workType.ToLower())
            {
                case "стих":
                    var poemLike = await _context.PoemLikes
                        .FirstOrDefaultAsync(pl => pl.PoemId == workId && pl.ApplicationUserId == userId);
                    if (poemLike != null)
                    {
                        _context.PoemLikes.Remove(poemLike);
                        await _context.SaveChangesAsync(); // СОХРАНЕНИЕ ДО обновления рейтинга
                        await _ratingManagerService.UpdatePoemRatingAsync(workId);
                        var authorId = await _context.Poems
                            .Where(p => p.Id == workId)
                            .Select(p => p.AuthorId).FirstAsync();
                        await _ratingManagerService.UpdateAuthorRatingAsync(authorId);
                    }
                    break;

                case "блог":
                    var blogLike = await _context.BlogLikes
                        .FirstOrDefaultAsync(bl => bl.BlogId == workId && bl.ApplicationUserId == userId);
                    if (blogLike != null)
                    {
                        _context.BlogLikes.Remove(blogLike);
                        await _context.SaveChangesAsync(); // СОХРАНЕНИЕ ДО обновления рейтинга
                        await _ratingManagerService.UpdateBlogRatingAsync(workId);
                        var authorId = await _context.Blogs
                            .Where(p => p.Id == workId)
                            .Select(p => p.AuthorId).FirstAsync();
                        await _ratingManagerService.UpdateAuthorRatingAsync(authorId);
                    }
                    break;

                case "книга":
                    var bookLike = await _context.BookLikes
                        .FirstOrDefaultAsync(bl => bl.BookId == workId && bl.ApplicationUserId == userId);
                    if (bookLike != null)
                    {
                        _context.BookLikes.Remove(bookLike);
                        await _context.SaveChangesAsync(); // ДЛЯ КНИГ ТОЖЕ НУЖНО
                    }
                    break;
            }
        }

        private async Task<CommentViewModel> AddPoemCommentAsync(int poemId, string userId, string commentText)
        {
            var comment = new PoemComment
            {
                PoemId = poemId,
                ApplicationUserId = userId,
                Content = commentText,
                CreatedDate = DateTime.UtcNow
            };

            _context.PoemComments.Add(comment);
            await _context.SaveChangesAsync();

            // Обновляем счетчик комментариев
            var poem = await _poemService.GetByIdAsync(poemId);
            if (poem != null)
            {
                poem.CommentsCount = await _context.PoemComments.CountAsync(pc => pc.PoemId == poemId);
                await _poemService.UpdateAsync(poem);
            }

            var user = await _context.Users.FindAsync(userId);
            return new CommentViewModel
            {
                Id = comment.Id,
                Content = comment.Content,
                UserId = userId,
                UserName = user?.UserName ?? "Пользователь",
                Timestamp = comment.CreatedDate
            };
        }

        private async Task<CommentViewModel> AddBlogCommentAsync(int blogId, string userId, string commentText)
        {
            var comment = new BlogComment
            {
                BlogId = blogId,
                ApplicationUserId = userId,
                Content = commentText,
                CommentDate = DateTime.UtcNow
            };

            _context.BlogComments.Add(comment);
            await _context.SaveChangesAsync();

            // Обновляем счетчик комментариев
            var blog = await _blogService.GetByIdAsync(blogId);
            if (blog != null)
            {
                blog.CommentsCount = await _context.BlogComments.CountAsync(bc => bc.BlogId == blogId);
                await _blogService.UpdateAsync(blog);
            }

            var user = await _context.Users.FindAsync(userId);
            return new CommentViewModel
            {
                Id = comment.Id,
                Content = comment.Content,
                UserId = userId,
                UserName = user?.UserName ?? "Пользователь",
                Timestamp = comment.CommentDate
            };
        }

        private async Task<CommentViewModel> AddBookCommentAsync(int bookId, string userId, string commentText)
        {
            var comment = new Comment
            {
                BookId = bookId,
                ApplicationUserId = userId,
                Content = commentText,
                CreatedDate = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            var user = await _context.Users.FindAsync(userId);
            return new CommentViewModel
            {
                Id = comment.Id,
                Content = comment.Content,
                UserId = userId,
                UserName = user?.UserName ?? "Пользователь",
                Timestamp = comment.CreatedDate
            };
        }

        private async Task<bool> DeletePoemCommentAsync(int commentId, string userId)
        {
            var comment = await _context.PoemComments
                .FirstOrDefaultAsync(pc => pc.Id == commentId && pc.ApplicationUserId == userId);

            if (comment == null) return false;

            var poemId = comment.PoemId;
            _context.PoemComments.Remove(comment);
            await _context.SaveChangesAsync();

            // Обновляем счетчик комментариев
            var poem = await _poemService.GetByIdAsync(poemId);
            if (poem != null)
            {
                poem.CommentsCount = await _context.PoemComments.CountAsync(pc => pc.PoemId == poemId);
                await _poemService.UpdateAsync(poem);
            }

            return true;
        }

        private async Task<bool> DeleteBlogCommentAsync(int commentId, string userId)
        {
            var comment = await _context.BlogComments
                .FirstOrDefaultAsync(bc => bc.Id == commentId && bc.ApplicationUserId == userId);

            if (comment == null) return false;

            var blogId = comment.BlogId;
            _context.BlogComments.Remove(comment);
            await _context.SaveChangesAsync();

            // Обновляем счетчик комментариев
            var blog = await _blogService.GetByIdAsync(blogId);
            if (blog != null)
            {
                blog.CommentsCount = await _context.BlogComments.CountAsync(bc => bc.BlogId == blogId);
                await _blogService.UpdateAsync(blog);
            }

            return true;
        }

        private async Task<bool> DeleteBookCommentAsync(int commentId, string userId)
        {
            var comment = await _context.Comments
                .FirstOrDefaultAsync(bc => bc.Id == commentId && bc.ApplicationUserId == userId);

            if (comment == null) return false;

            var bookId = comment.BookId;
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return true;
        }

        #endregion
    }
}