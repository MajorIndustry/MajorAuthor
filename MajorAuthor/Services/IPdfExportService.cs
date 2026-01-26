// IPdfExportService.cs
using MajorAuthor.Models;
using NuGet.Packaging;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace MajorAuthor.Services
{
    public interface IPdfExportService
    {
        Task<byte[]> GenerateBookPdfAsync(BookExportModel book);
        Task<string> GenerateBookPdfFileAsync(BookExportModel book, string outputPath);
    }
}
