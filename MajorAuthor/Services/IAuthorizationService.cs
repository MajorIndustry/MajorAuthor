namespace MajorAuthor.Services
{
    public interface IAuthorizationService
    {
        Task<bool> IsAuthorAsync(string userId);
        Task<bool> IsUserAuthorizedAsync(string userId);
    }
}
