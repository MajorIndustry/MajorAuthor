using MajorAuthor.Models;
using System.Threading.Tasks;

namespace MajorAuthor.Services
{
    /// <summary>
    /// Сервис для получения данных профиля пользователя.
    /// Следует принципу единственной ответственности (SRP).
    /// </summary>
    public interface IUserProfileService
    {
        /// <summary>
        /// Получает полные данные профиля пользователя, подготовленные для ViewModel.
        /// </summary>
        /// <param name="userId">Идентификатор пользователя.</param>
        /// <returns>Модель представления MyProfileViewModel, или null, если пользователь не найден.</returns>
        Task<MyProfileViewModel> GetUserProfileViewModelAsync(string userId);
    }
}
