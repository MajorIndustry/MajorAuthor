namespace MajorAuthor.Services
{
    public interface IRatingManagerService
    {
        Task UpdateBookRatingAsync(int bookId);
        Task UpdatePoemRatingAsync(int poemId);
        Task UpdateBlogRatingAsync(int blogId);
        Task UpdateAuthorRatingAsync(int authorId);
        Task RecalculateAllRatingsAsync();
    }
}
