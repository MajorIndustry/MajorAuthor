namespace MajorAuthor.Services
{
    public interface IPdfCacheService
    {
        Task<byte[]> GetOrCreatePdfAsync(int bookId, Func<Task<byte[]>> createPdf);
        void InvalidateCache(int bookId);
    }
}