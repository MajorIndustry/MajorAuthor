// Services/RatingService.cs

namespace MajorAuthor.Services
{
    public interface IRatingService
    {
        double CalculateGlobalRating(int likes, int views);
        double CalculateLocalRating(int likes, int views, double coefficient = 100);
        Task<double> GetAuthorRatingForPeriodAsync(int authorId, DateTime startDate, DateTime endDate);
        Task<double> GetBookRatingForPeriodAsync(int bookId, DateTime startDate, DateTime endDate);
        Task<double> GetPoemRatingForPeriodAsync(int poemId, DateTime startDate, DateTime endDate);
        Task<List<(int BookId, double Rating)>> GetPopularBooksForPeriodAsync(DateTime startDate, DateTime endDate, int count = 10);
        Task<List<(int PoemId, double Rating)>> GetPopularPoemsForPeriodAsync(DateTime startDate, DateTime endDate, int count = 10);
    }
}