using MajorAuthor.Data;
using MajorAuthor.Data.Entities;
using MajorAuthor.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;

// Принцип единственной ответственности (SRP).
// Этот сервис отвечает исключительно за логику, связанную с комментариями.
namespace MajorAuthor.Services
{
    public class CommentService : ICommentService
    {
        private readonly MajorAuthorDbContext _context;

        public CommentService(MajorAuthorDbContext context)
        {
            _context = context;
        }

        public async Task<List<CommentViewModel>> GetCommentsForWorkAsync(int workId, string workType)
        {
            var comments = new List<CommentViewModel>();
            switch (workType)
            {
                case "Стих":
                    comments = await _context.PoemComments
                        .Where(c => c.PoemId == workId)
                        .Select(c => new CommentViewModel
                        {
                            Id = c.Id,
                            Content = c.Content,
                            UserId = c.ApplicationUserId,
                            Timestamp = c.CreatedDate
                        })
                        .ToListAsync();
                    break;
                case "Блог":
                    comments = await _context.BlogComments
                        .Where(c => c.BlogId == workId)
                        .Select(c => new CommentViewModel
                        {
                            Id = c.Id,
                            Content = c.Content,
                            UserId = c.ApplicationUserId,
                            Timestamp = c.CommentDate
                        })
                        .ToListAsync();
                    break;
            }
            return comments;
        }

        public async Task<CommentViewModel> AddCommentAsync(int workId, string userId, string workType, string commentText)
        {
            switch (workType)
            {
                case "Стих":
                    var newPoemComment = new PoemComment { PoemId = workId, ApplicationUserId = userId, Content = commentText, CreatedDate = DateTime.UtcNow };
                    await _context.PoemComments.AddAsync(newPoemComment);
                    await _context.SaveChangesAsync();
                    return new CommentViewModel
                    {
                        Id = newPoemComment.Id,
                        Content = newPoemComment.Content,
                        UserId = newPoemComment.ApplicationUserId,
                        Timestamp = newPoemComment.CreatedDate
                    };
                case "Блог":
                    var newBlogComment = new BlogComment { BlogId = workId, ApplicationUserId = userId, Content = commentText, CommentDate = DateTime.UtcNow };
                    await _context.BlogComments.AddAsync(newBlogComment);
                    await _context.SaveChangesAsync();
                    return new CommentViewModel
                    {
                        Id = newBlogComment.Id,
                        Content = newBlogComment.Content,
                        UserId = newBlogComment.ApplicationUserId,
                        Timestamp = newBlogComment.CommentDate
                    };
            }
            return null;
        }

        public async Task<bool> DeleteCommentAsync(int commentId, string workType, string userId)
        {
            switch (workType)
            {
                case "Стих":
                    var poemComment = await _context.PoemComments.FindAsync(commentId);
                    if (poemComment != null && poemComment.ApplicationUserId == userId)
                    {
                        _context.PoemComments.Remove(poemComment);
                        await _context.SaveChangesAsync();
                        return true;
                    }
                    break;
                case "Блог":
                    var blogComment = await _context.BlogComments.FindAsync(commentId);
                    if (blogComment != null && blogComment.ApplicationUserId == userId)
                    {
                        _context.BlogComments.Remove(blogComment);
                        await _context.SaveChangesAsync();
                        return true;
                    }
                    break;
            }
            return false;
        }
    }
}
