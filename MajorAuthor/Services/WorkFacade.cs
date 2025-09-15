using MajorAuthor.Data.Entities;
using MajorAuthor.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

// Обновленная версия фасада, которая работает с вашими конкретными сущностями
// без необходимости в общем интерфейсе.
namespace MajorAuthor.Services
{
    public class WorkFacade : IWorkFacade
    {
        private readonly IWorkService<Poem> _poemService;
        private readonly IWorkService<Blog> _blogService;
        private readonly ILikeService _likeService;
        private readonly ICommentService _commentService;

        public WorkFacade(
            IWorkService<Poem> poemService,
            IWorkService<Blog> blogService,
            ILikeService likeService,
            ICommentService commentService)
        {
            _poemService = poemService;
            _blogService = blogService;
            _likeService = likeService;
            _commentService = commentService;
        }

        public async Task<WorkViewModel> GetWorkViewModelAsync(int id, string type)
        {
            object work = null;
            switch (type)
            {
                case "Стих":
                    work = await ((PoemService)_poemService).GetByIdWithCommentsAndLikesAsync(id);
                    break;
                case "Блог":
                    work = await ((BlogService)_blogService).GetByIdWithCommentsAndLikesAsync(id);
                    break;
            }

            if (work == null) return null;

            // Увеличиваем счетчик просмотров. Используем "is" для безопасного приведения типов.
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

            return new WorkViewModel
            {
                Work = work,
                Comments = await _commentService.GetCommentsForWorkAsync(id, type),
                LikesCount = await _likeService.GetLikesCountForWorkAsync(id, type),
                Type = type
            };
        }

        public async Task<(bool success, int newLikesCount)> ToggleLikeAsync(int id, string type, string userId)
        {
            object work = null;
            switch (type)
            {
                case "Стих":
                    work = await _poemService.GetByIdAsync(id);
                    break;
                case "Блог":
                    work = await _blogService.GetByIdAsync(id);
                    break;
            }

            if (work == null) return (false, 0);

            var hasLiked = await _likeService.HasUserLikedWorkAsync(id, userId, type);

            if (!hasLiked)
            {
                await _likeService.AddLikeAsync(id, userId, type);
                if (work is Poem poem)
                {
                    poem.LikesCount++;
                    await _poemService.UpdateAsync(poem);
                    return (true, poem.LikesCount);
                }
                else if (work is Blog blog)
                {
                    blog.LikesCount++;
                    await _blogService.UpdateAsync(blog);
                    return (true, blog.LikesCount);
                }
            }

            // Возвращаем текущее количество лайков, даже если пользователь уже ставил лайк
            if (work is Poem existingPoem) return (false, existingPoem.LikesCount);
            if (work is Blog existingBlog) return (false, existingBlog.LikesCount);

            return (false, 0);
        }

        public async Task<CommentViewModel> AddCommentAsync(int id, string type, string userId, string commentText)
        {
            var comment = await _commentService.AddCommentAsync(id, userId, type, commentText);

            if (comment != null)
            {
                object work = null;
                switch (type)
                {
                    case "Стих":
                        work = await _poemService.GetByIdAsync(id);
                        if (work is Poem poem)
                        {
                            poem.CommentsCount++;
                            await _poemService.UpdateAsync(poem);
                        }
                        break;
                    case "Блог":
                        work = await _blogService.GetByIdAsync(id);
                        if (work is Blog blog)
                        {
                            blog.CommentsCount++;
                            await _blogService.UpdateAsync(blog);
                        }
                        break;
                }
            }

            return comment;
        }

        public async Task<bool> DeleteCommentAsync(int commentId, string type, string userId)
        {
            return await _commentService.DeleteCommentAsync(commentId, type, userId);
        }
    }
}
