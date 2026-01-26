// File: Services/ISimilarBookCalculatorService.cs
using System.Threading.Tasks;

namespace MajorAuthor.Services
{
    public interface ISimilarBookCalculatorService
    {
        Task CalculateSimilaritiesForAllBooksAsync();
        Task CalculateSimilaritiesForBookAsync(int bookId);
        Task RecalculateOutdatedSimilaritiesAsync(int daysThreshold = 7);
    }
}