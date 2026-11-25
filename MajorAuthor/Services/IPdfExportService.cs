// IPdfExportService.cs
using MajorAuthor.Models;
using NuGet.Packaging;

namespace MajorAuthor.Services
{
    public interface IPdfExportService
    {
        Task<byte[]> GenerateBookPdfAsync(BookExportModel book);
        Task<string> GenerateBookPdfFileAsync(BookExportModel book, string outputPath);
    }
}
