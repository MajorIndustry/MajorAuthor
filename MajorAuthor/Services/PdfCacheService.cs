using Microsoft.Extensions.Caching.Memory;

namespace MajorAuthor.Services
{
    public class PdfCacheService : IPdfCacheService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<PdfCacheService> _logger;

        public PdfCacheService(IMemoryCache cache, ILogger<PdfCacheService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public async Task<byte[]> GetOrCreatePdfAsync(int bookId, Func<Task<byte[]>> createPdf)
        {
            var cacheKey = $"pdf_book_{bookId}";

            if (!_cache.TryGetValue(cacheKey, out byte[] pdfBytes))
            {
                _logger.LogInformation($"Генерация PDF для книги {bookId} (кэш пуст)");
                pdfBytes = await createPdf();

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromHours(1))
                    .SetAbsoluteExpiration(TimeSpan.FromDays(1));

                _cache.Set(cacheKey, pdfBytes, cacheOptions);
            }
            else
            {
                _logger.LogInformation($"Использование кэшированного PDF для книги {bookId}");
            }

            return pdfBytes;
        }

        public void InvalidateCache(int bookId)
        {
            var cacheKey = $"pdf_book_{bookId}";
            _cache.Remove(cacheKey);
            _logger.LogInformation($"Кэш PDF для книги {bookId} очищен");
        }
        public void InvalidateCacheByBook(int bookId)
        {
            InvalidateCache(bookId);
        }
    }
}
