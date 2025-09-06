// Файл: Services/IHomeService.cs
using MajorAuthor.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MajorAuthor.Services
{
    /// <summary>
    /// Интерфейс для сервиса, предоставляющего данные для главной страницы.
    /// Это помогает следовать принципам SOLID, отделяя бизнес-логику от контроллера.
    /// </summary>
    public interface IHomeService
    {
        /// <summary>
        /// Получает все данные для главной страницы, включая различные секции книг и авторов.
        /// </summary>
        /// <param name="userId">ID текущего пользователя для персонализированных рекомендаций.</param>
        /// <returns>Модель HomeViewModel, содержащая все необходимые данные.</returns>
        Task<HomeViewModel> GetHomeDataAsync(string userId);

        /// <summary>
        /// Получает список книг по ID жанра. Используется для AJAX-запросов.
        /// </summary>
        /// <param name="genreId">ID жанра.</param>
        /// <returns>Список моделей BookDisplayModel.</returns>
        Task<List<HomeViewModel.BookDisplayModel>> GetBooksByGenreAsync(int genreId);
    }
}