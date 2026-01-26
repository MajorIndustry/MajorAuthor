// PdfExportService.cs
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using MajorAuthor.Models;

namespace MajorAuthor.Services
{
    public class PdfExportService : IPdfExportService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<PdfExportService> _logger;

        public PdfExportService(IWebHostEnvironment webHostEnvironment, ILogger<PdfExportService> logger)
        {
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<byte[]> GenerateBookPdfAsync(BookExportModel bookModel)
        {
            try
            {
                _logger.LogInformation($"Начинаю генерацию PDF для книги: {bookModel.Title}");

                var pdfBytes = Document.Create(container =>
                {
                    // Титульная страница с обложкой
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.DefaultTextStyle(x => x.FontSize(11));

                        page.Content().Column(column =>
                        {
                            // Обложка книги (если есть)
                            if (!string.IsNullOrEmpty(bookModel.CoverImageUrl) && File.Exists(bookModel.CoverImageUrl))
                            {
                                column.Item()
                                    .PaddingTop(2, Unit.Centimetre)
                                    .AlignCenter()
                                    .Image(File.ReadAllBytes(bookModel.CoverImageUrl), ImageScaling.FitWidth);
                            }

                            column.Item().PaddingTop(3, Unit.Centimetre)
                                .AlignCenter()
                                .Text(bookModel.Title)
                                .FontSize(24)
                                .Bold();

                            if (bookModel.Authors.Any())
                            {
                                column.Item().PaddingTop(1, Unit.Centimetre)
                                    .AlignCenter()
                                    .Text($"Авторы: {string.Join(", ", bookModel.Authors.Select(a => a.PenName))}")
                                    .FontSize(16);
                            }

                            if (!string.IsNullOrEmpty(bookModel.PublicationDate))
                            {
                                column.Item().PaddingTop(0.5f, Unit.Centimetre)
                                    .AlignCenter()
                                    .Text($"Дата публикации: {bookModel.PublicationDate}")
                                    .FontSize(12);
                            }
                        });

                        // Футер для титульной страницы
                        page.Footer().Column(footerColumn =>
                        {
                            footerColumn.Item().AlignCenter().Text(text =>
                            {
                                text.Span("Страница ");
                                text.CurrentPageNumber();
                                text.Span(" из ");
                                text.TotalPages();
                            });
                        });
                    });

                    // Страница с описанием (только если есть описание)
                    if (!string.IsNullOrEmpty(bookModel.Description))
                    {
                        container.Page(page =>
                        {
                            page.Size(PageSizes.A4);
                            page.Margin(2, Unit.Centimetre);
                            page.DefaultTextStyle(x => x.FontSize(11));

                            page.Content().Padding(20).Column(column =>
                            {
                                column.Item().Text("Описание").FontSize(16).Bold();
                                column.Item().PaddingTop(10).Text(bookModel.Description).FontSize(12).LineHeight(1.3f);

                                if (bookModel.Genres.Any() || bookModel.Tags.Any())
                                {
                                    column.Item().PaddingTop(20);

                                    if (bookModel.Genres.Any())
                                    {
                                        column.Item().Text($"Жанры: {string.Join(", ", bookModel.Genres)}").FontSize(11);
                                    }

                                    if (bookModel.Tags.Any())
                                    {
                                        column.Item().PaddingTop(5).Text($"Теги: {string.Join(", ", bookModel.Tags)}").FontSize(11);
                                    }
                                }
                            });

                            // Футер для страницы описания
                            page.Footer().Column(footerColumn =>
                            {
                                footerColumn.Item().AlignCenter().Text(text =>
                                {
                                    text.Span("Страница ");
                                    text.CurrentPageNumber();
                                    text.Span(" из ");
                                    text.TotalPages();
                                });
                            });
                        });
                    }

                    // Главы
                    foreach (var chapter in bookModel.Chapters.OrderBy(c => c.Order))
                    {
                        ProcessChapter(container, chapter);
                    }
                }).GeneratePdf();

                _logger.LogInformation($"PDF успешно сгенерирован для книги: {bookModel.Title}, размер: {pdfBytes.Length} байт");
                return pdfBytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при генерации PDF для книги: {bookModel.Title}");
                throw;
            }
        }

        private void ProcessChapter(IDocumentContainer container, ChapterExportModel chapter)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(11));

                // Заголовок главы
                page.Header().Column(headerColumn =>
                {
                    headerColumn.Item().Text($"Глава {chapter.Order}: {chapter.Title}").FontSize(14).Bold();
                });

                // Контент
                page.Content().Column(column =>
                {
                    // Текстовый контент главы
                    if (!string.IsNullOrEmpty(chapter.Content))
                    {
                        // Разделяем контент на абзацы
                        var paragraphs = chapter.Content.Split(new[] { "\n\n", "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                        foreach (var paragraph in paragraphs)
                        {
                            if (!string.IsNullOrWhiteSpace(paragraph))
                            {
                                column.Item().PaddingBottom(10).Text(paragraph.Trim()).FontSize(11).LineHeight(1.3f);
                            }
                        }
                    }

                    // Изображения из страниц
                    foreach (var pageItem in chapter.Pages.Where(p => !string.IsNullOrEmpty(p.ImageUrl)))
                    {
                        try
                        {
                            if (File.Exists(pageItem.ImageUrl))
                            {
                                var imageBytes = File.ReadAllBytes(pageItem.ImageUrl);

                                // Добавляем изображение
                                column.Item().PaddingVertical(10)
                                    .AlignCenter()
                                    .Image(imageBytes, ImageScaling.FitWidth);

                                // Подпись к изображению (если есть текст)
                                if (!string.IsNullOrEmpty(pageItem.TextContent))
                                {
                                    column.Item().PaddingTop(5).AlignCenter()
                                        .Text(pageItem.TextContent)
                                        .FontSize(9)
                                        .Italic();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Ошибка при добавлении изображения в PDF: {pageItem.ImageUrl}");
                            column.Item().PaddingVertical(10).Text($"[Изображение недоступно: {Path.GetFileName(pageItem.ImageUrl)}]")
                                .FontSize(9).Italic();
                        }
                    }
                });

                // Футер для страниц глав
                page.Footer().Column(footerColumn =>
                {
                    footerColumn.Item().AlignCenter().Text(text =>
                    {
                        text.Span("Страница ");
                        text.CurrentPageNumber();
                        text.Span(" из ");
                        text.TotalPages();
                    });
                });
            });
        }

        public async Task<string> GenerateBookPdfFileAsync(BookExportModel book, string outputPath)
        {
            var pdfBytes = await GenerateBookPdfAsync(book);
            await File.WriteAllBytesAsync(outputPath, pdfBytes);
            return outputPath;
        }
    }
}